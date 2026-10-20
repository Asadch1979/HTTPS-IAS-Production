-- LEGACY STANDALONE REFERENCE ONLY. This is not the production deployment target.
-- IAS calls PKG_RPT.R_IAS_PARA_POSITION. Do not run this file to replace or "fix"
-- that package. Verify its live ALL_ARGUMENTS signature and approved package body.
-- Preserved below solely for calculation/history comparison.
-- Oracle 18c. Compile V_EXEC_KPI_LIFECYCLE, then compatibility SRC_A/O/C.
-- Historical DATE-based KPI totals are estimates; pre-transfer drops and
-- audit-conclusion timestamps cannot be recovered from post-compliance alone.
-- Standalone procedure: leaves PKG_RPT.R_FAD_MONTHLY_REVIEW unchanged.
CREATE OR REPLACE PROCEDURE R_IAS_PARA_POSITION(
    P_NO IN NUMBER, ENT_ID IN NUMBER, R_ID IN NUMBER, R_T IN NUMBER,
    S_DATE IN DATE, E_DATE IN DATE, IO_CURSOR OUT SYS_REFCURSOR) AS
    V_FROM DATE;
    V_TO DATE;
BEGIN
    IF S_DATE IS NULL OR E_DATE IS NULL OR TRUNC(S_DATE) > TRUNC(E_DATE)
       OR R_T IS NULL OR R_T = 0 OR R_T < -1 THEN
        RAISE_APPLICATION_ERROR(-20005, 'Select an entity type and a valid reporting period.');
    END IF;
    IF P_NO IS NULL OR ENT_ID IS NULL OR ENT_ID <= 0 OR R_ID IS NULL OR R_ID <= 0 THEN
        RAISE_APPLICATION_ERROR(-20006, 'An authenticated user posting is required.');
    END IF;
    V_FROM := TRUNC(S_DATE);
    V_TO := TRUNC(E_DATE) + 1;

    OPEN IO_CURSOR FOR
    WITH scoped AS (
        SELECT k.*, p.com_stage,
               CASE WHEN R_ID IN (1, 3, 5) AND R_T = -1
                    THEN '-' ELSE ro.name END AS p_name,
               CASE WHEN R_ID IN (1, 3, 5) AND R_T = -1
                    THEN k.entity_type_id ELSE k.entity_id END AS child_code,
               CASE WHEN R_ID IN (1, 3, 5) AND R_T = -1
                    THEN et.entitytypedesc ELSE ae.name END AS c_name
          FROM v_exec_kpi_lifecycle k
          JOIN t_auditee_entities ae ON ae.entity_id = k.entity_id
          JOIN t_auditee_ent_types et ON et.autid = k.entity_type_id
          LEFT JOIN t_auditee_entities ro ON ro.entity_id = k.reporting_office_id
          LEFT JOIN ais_t_au_post_compliance p
            ON k.record_type = 'PARA' AND p.com_id = k.com_id
         WHERE (R_ID NOT IN (1, 3, 5) OR R_T <> -1 OR et.audit_type IN ('B', 'D'))
           -- Preserve monthly-review auditor/reviewer scopes. Use EXISTS so
           -- multiple mapping/reviewer rows cannot multiply observations/paras.
           AND (
               (R_ID IN (1, 3, 5) AND R_T = -1)
               OR (R_ID IN (6, 7) AND k.audited_by_id = ENT_ID)
               OR (R_ID IN (43, 44) AND k.entity_type_id = R_T AND EXISTS (
                   SELECT 1 FROM v_get_compliance_reviewer_approver c
                    WHERE c.com_key = ae.complice_by AND c.entity_id = ENT_ID))
               OR (R_ID NOT IN (6, 7, 43, 44)
                   AND (R_ID NOT IN (15, 16) OR k.audited_by_id = ENT_ID)
                   AND EXISTS (
                       SELECT 1 FROM t_auditee_entities_maping m
                        WHERE m.entity_id = k.entity_id
                          AND NVL(m.status, 'Y') = 'Y'
                          AND m.c_type_id = CASE WHEN R_T = -1 THEN k.entity_type_id ELSE R_T END))
           )
    ), flags AS (
        SELECT s.*,
               CASE WHEN record_type = 'PARA' AND para_added_on < V_FROM
                      AND (para_status = 8 OR (para_status = 9 AND para_settled_on >= V_FROM))
                    THEN 1 ELSE 0 END AS opening_flag,
               -- Match the live lifecycle cursor: qualifying net additions.
               CASE WHEN record_type = 'PARA'
                          AND para_added_on >= V_FROM AND para_added_on < V_TO
                          AND (para_status = 8 OR
                               (para_status = 9 AND
                                (raw_para_settled_on < V_FROM
                                 OR raw_para_settled_on >= V_TO)))
                    THEN 1 ELSE 0 END AS added_flag,
               CASE WHEN record_type = 'PARA' AND para_added_on < V_TO
                      AND para_settled_on >= V_FROM AND para_settled_on < V_TO
                    THEN 1 ELSE 0 END AS settled_flag,
               CASE WHEN record_type = 'PARA' AND para_added_on < V_TO
                      AND (para_status = 8 OR (para_status = 9 AND para_settled_on >= V_TO))
                    THEN 1 ELSE 0 END AS outstanding_flag
          FROM scoped s
    )
    SELECT p_name, child_code, c_name,
           SUM(opening_flag) AS opening_bal,
           SUM(CASE WHEN record_type = 'ISSUE' AND issue_eligible = 1
                          AND issue_identified_on >= V_FROM AND issue_identified_on < V_TO
                    THEN 1 ELSE 0 END) AS issue_identified,
           -- Modern ISSUE_RECTIFIED=1 counts in its PARA_ADDED_ON cohort.
           -- ISSUE_RECTIFIED_ON retains its compatibility alias but is not
           -- the physical observation settlement date in this reporting mode.
           SUM(CASE WHEN record_type = 'ISSUE' AND issue_eligible = 1
                          AND issue_rectified_on >= V_FROM AND issue_rectified_on < V_TO
                    THEN 1 ELSE 0 END) AS issue_rectified,
           SUM(added_flag) AS para_added,
           SUM(CASE WHEN com_stage IN (41, 45) THEN settled_flag ELSE 0 END) AS settled_com,
           SUM(CASE WHEN com_stage NOT IN (41, 45) OR com_stage IS NULL
                    THEN settled_flag ELSE 0 END) AS settled_aud,
           SUM(outstanding_flag) AS outstanding,
           SUM(CASE WHEN risk_id = 1 THEN outstanding_flag ELSE 0 END) AS r1,
           SUM(CASE WHEN risk_id = 2 THEN outstanding_flag ELSE 0 END) AS r2,
           SUM(CASE WHEN risk_id IN (0, 3) THEN outstanding_flag ELSE 0 END) AS r3
      FROM flags
     GROUP BY p_name, child_code, c_name
     ORDER BY p_name, child_code;
END R_IAS_PARA_POSITION;
/
