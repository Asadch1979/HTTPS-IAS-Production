CREATE OR REPLACE PACKAGE PKG_EXEC_REPORTS AUTHID DEFINER AS
  PROCEDURE P_ANNUAL_FIELD_PROGRESS(P_DASHBOARD_TYPE      IN VARCHAR2,
                                    P_SCOPE_ROOT_ID       IN NUMBER,
                                    P_START_DATE          IN DATE,
                                    P_END_DATE            IN DATE,
                                    P_ENTITY_TYPE_ID      IN NUMBER,
                                    P_AUDITED_BY_ID       IN NUMBER,
                                    P_REPORTING_OFFICE_ID IN NUMBER,
                                    P_ENTITY_ID           IN NUMBER,
                                    IO_CURSOR             OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL);
  PROCEDURE P_FIELD_OFFICE_POSITION(P_DASHBOARD_TYPE      IN VARCHAR2,
                                    P_SCOPE_ROOT_ID       IN NUMBER,
                                    P_START_DATE          IN DATE,
                                    P_END_DATE            IN DATE,
                                    P_ENTITY_TYPE_ID      IN NUMBER,
                                    P_AUDITED_BY_ID       IN NUMBER,
                                    P_REPORTING_OFFICE_ID IN NUMBER,
                                    P_ENTITY_ID           IN NUMBER,
                                    IO_CURSOR             OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL);
  PROCEDURE P_GM_REGION_POSITION(P_DASHBOARD_TYPE      IN VARCHAR2,
                                 P_SCOPE_ROOT_ID       IN NUMBER,
                                 P_START_DATE          IN DATE,
                                 P_END_DATE            IN DATE,
                                 P_ENTITY_TYPE_ID      IN NUMBER,
                                 P_AUDITED_BY_ID       IN NUMBER,
                                 P_REPORTING_OFFICE_ID IN NUMBER,
                                 P_ENTITY_ID           IN NUMBER,
                                 IO_CURSOR             OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL);
  PROCEDURE P_REGION_RANKINGS(P_DASHBOARD_TYPE      IN VARCHAR2,
                              P_SCOPE_ROOT_ID       IN NUMBER,
                              P_START_DATE          IN DATE,
                              P_END_DATE            IN DATE,
                              P_ENTITY_TYPE_ID      IN NUMBER,
                              P_AUDITED_BY_ID       IN NUMBER,
                              P_REPORTING_OFFICE_ID IN NUMBER,
                              P_ENTITY_ID           IN NUMBER,
                              IO_CURSOR             OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL);
  PROCEDURE P_MANAGEMENT_SETTLEMENT(P_DASHBOARD_TYPE      IN VARCHAR2,
                                    P_SCOPE_ROOT_ID       IN NUMBER,
                                    P_START_DATE          IN DATE,
                                    P_END_DATE            IN DATE,
                                    P_ENTITY_TYPE_ID      IN NUMBER,
                                    P_AUDITED_BY_ID       IN NUMBER,
                                    P_REPORTING_OFFICE_ID IN NUMBER,
                                    P_ENTITY_ID           IN NUMBER,
                                    IO_CURSOR             OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL);
END PKG_EXEC_REPORTS;
/

-- Oracle 18c image-aligned Executive Reports A-E package body.
-- Report A: year-by-year progress (reference images 01 and 06).
-- Report B: GM Field Offices / Other Field Offices (image 02).
-- Report C: GM -> Region (image 03).
-- Report D: Top/Bottom 5 regions by settlement percentage (image 04).
-- Report E: audited Group / Division from V_IAS_ENTITY_REPORTING_IDS_Group_reports (image 05).
--
-- Core movement counts use EFFECTIVE_ADDED_ON for modern source A:
-- conclusion date (status 14), when later than copied PARA_ADDED_ON.
-- All balance history remains ESTIMATED owing to reopens/transfers.
-- Family codes B/D and settlement stage 41/45 require live confirmation.
-- Returns the existing ExecutiveReportRow cursor contract unchanged.
CREATE OR REPLACE PACKAGE BODY PKG_EXEC_REPORTS AS
  PROCEDURE VALIDATE_REQUEST(
      P_REPORT_ID IN VARCHAR2,
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_SCOPE_ROOT_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER,
      P_AUDITED_BY_ID IN NUMBER,
      P_REPORTING_OFFICE_ID IN NUMBER,
      P_ENTITY_ID IN NUMBER) IS
    n NUMBER;
  BEGIN
    IF P_REPORT_ID IS NULL OR P_REPORT_ID NOT IN ('A','B','C','D','E')
       OR P_DASHBOARD_TYPE IS NULL OR
          P_DASHBOARD_TYPE NOT IN ('TOP','GROUP','CCO','CIA')
       OR P_START_DATE IS NULL OR P_END_DATE IS NULL
       OR P_START_DATE <> TRUNC(P_START_DATE)
       OR P_END_DATE <> TRUNC(P_END_DATE)
       OR P_START_DATE > P_END_DATE
       OR P_END_DATE >= DATE '9999-12-31'
    THEN
      RAISE_APPLICATION_ERROR(-20801, 'Invalid report or date range');
    END IF;
    IF P_DASHBOARD_TYPE='GROUP' AND
       (P_START_DATE <> TRUNC(CAST(SYSTIMESTAMP AT TIME ZONE 'Asia/Karachi' AS DATE),'YYYY') OR
        P_END_DATE <> TRUNC(CAST(SYSTIMESTAMP AT TIME ZONE 'Asia/Karachi' AS DATE))-1) THEN
      RAISE_APPLICATION_ERROR(-20801,'Management reporting requires current-year start through yesterday.');
    END IF;
    IF (P_ENTITY_TYPE_ID IS NOT NULL AND P_AUDITED_BY_ID IS NOT NULL)
       OR (P_DASHBOARD_TYPE = 'GROUP' AND P_SCOPE_ROOT_ID IS NULL)
       OR (P_DASHBOARD_TYPE <> 'GROUP' AND P_SCOPE_ROOT_ID IS NOT NULL)
    THEN
      RAISE_APPLICATION_ERROR(-20802, 'Invalid reporting scope');
    END IF;
    FOR x IN (
      SELECT P_SCOPE_ROOT_ID id FROM dual
      UNION ALL SELECT P_ENTITY_TYPE_ID FROM dual
      UNION ALL SELECT P_AUDITED_BY_ID FROM dual
      UNION ALL SELECT P_REPORTING_OFFICE_ID FROM dual
      UNION ALL SELECT P_ENTITY_ID FROM dual
    ) LOOP
      IF x.id IS NOT NULL AND (x.id <= 0 OR x.id <> TRUNC(x.id)) THEN
        RAISE_APPLICATION_ERROR(-20801, 'Filter ID must be a positive integer');
      END IF;
    END LOOP;
    IF P_DASHBOARD_TYPE = 'GROUP' THEN
      SELECT COUNT(*) INTO n FROM v_exec_entity_scope s
      WHERE s.entity_id = P_SCOPE_ROOT_ID
        AND s.business_group_id = P_SCOPE_ROOT_ID;
      IF n <> 1 THEN
        RAISE_APPLICATION_ERROR(-20802, 'Invalid GROUP scope');
      END IF;
    END IF;
    FOR x IN (
      SELECT P_REPORTING_OFFICE_ID id FROM dual
      UNION ALL SELECT P_ENTITY_ID FROM dual
    ) LOOP
      IF x.id IS NOT NULL THEN
        SELECT COUNT(*) INTO n FROM v_exec_entity_scope s
        WHERE s.entity_id = x.id
          AND (P_SCOPE_ROOT_ID IS NULL
               OR s.entity_id = P_SCOPE_ROOT_ID
               OR P_SCOPE_ROOT_ID IN
                  (s.reporting_1,s.reporting_2,s.reporting_3,
                   s.reporting_4,s.reporting_5,s.reporting_6));
        IF n <> 1 THEN
          RAISE_APPLICATION_ERROR(-20802, 'Invalid filtered entity scope');
        END IF;
      END IF;
    END LOOP;
  END VALIDATE_REQUEST;

  PROCEDURE OPEN_REPORT(
      P_REPORT_ID IN VARCHAR2,
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_SCOPE_ROOT_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER,
      P_AUDITED_BY_ID IN NUMBER,
      P_REPORTING_OFFICE_ID IN NUMBER,
      P_ENTITY_ID IN NUMBER,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2,
      P_SNAP_AUDITED_BY_ID IN NUMBER) IS
  BEGIN
    VALIDATE_REQUEST(P_REPORT_ID, P_DASHBOARD_TYPE, P_SCOPE_ROOT_ID,
      P_START_DATE, P_END_DATE, P_ENTITY_TYPE_ID,
      CASE WHEN P_SNAP_DIVISION_CODE IS NULL THEN P_AUDITED_BY_ID END,
      P_REPORTING_OFFICE_ID, P_ENTITY_ID);
    IF P_SNAP_DIVISION_CODE IS NOT NULL THEN
      PKG_AUDIT_SNAP_SCOPE.REQUIRE_SELECTION(P_SNAP_DIVISION_CODE,P_AUDITED_BY_ID);
    END IF;
    IF P_SNAP_AUDITED_BY_ID IS NOT NULL AND (P_SNAP_DIVISION_CODE IS NULL OR P_SNAP_AUDITED_BY_ID <= 0 OR P_SNAP_AUDITED_BY_ID <> TRUNC(P_SNAP_AUDITED_BY_ID)) THEN
      RAISE_APPLICATION_ERROR(-20801,'Invalid Snap audited-by selection.');
    END IF;
    IF P_REPORT_ID = 'A' THEN
      OPEN IO_CURSOR FOR
      WITH years AS (
        SELECT ADD_MONTHS(TRUNC(P_START_DATE, 'YYYY'), 12 * (LEVEL - 1))
                 AS year_start
        FROM dual
        CONNECT BY LEVEL <= EXTRACT(YEAR FROM P_END_DATE) -
                            EXTRACT(YEAR FROM P_START_DATE) + 1
      ),
      periods AS (
        SELECT GREATEST(year_start, P_START_DATE) AS period_start,
               LEAST(ADD_MONTHS(year_start, 12) - 1, P_END_DATE) AS period_end
        FROM years
      ),
      seed AS (
    SELECT p.period_start, p.period_end,
           s.*,
           CASE WHEN (s.effective_added_on < p.period_start AND (s.close_on IS NULL OR s.close_on >= p.period_start)) THEN 1 ELSE 0 END AS opening_balance,
           CASE WHEN (s.effective_added_on >= p.period_start AND s.effective_added_on < p.period_end + 1) THEN 1 ELSE 0 END AS paras_added,
           CASE WHEN (s.close_on >= p.period_start AND s.close_on < p.period_end + 1 AND s.effective_added_on < p.period_end + 1) THEN 1 ELSE 0 END AS settled_paras,
           CASE WHEN (s.effective_added_on < p.period_end + 1 AND (s.close_on IS NULL OR s.close_on >= p.period_end + 1)) THEN 1 ELSE 0 END AS outstanding_paras,
           CASE WHEN (s.effective_added_on < p.period_end + 1 AND (s.close_on IS NULL OR s.close_on >= p.period_end + 1)) AND s.risk = 1 THEN 1 ELSE 0 END AS high_risk,
           CASE WHEN (s.effective_added_on < p.period_end + 1 AND (s.close_on IS NULL OR s.close_on >= p.period_end + 1)) AND s.risk = 2 THEN 1 ELSE 0 END AS medium_risk,
           CASE WHEN (s.effective_added_on < p.period_end + 1 AND (s.close_on IS NULL OR s.close_on >= p.period_end + 1)) AND s.risk = 3 THEN 1 ELSE 0 END AS low_risk,
           CASE WHEN (s.effective_added_on < p.period_end + 1 AND (s.close_on IS NULL OR s.close_on >= p.period_end + 1)) AND (s.risk IS NULL OR s.risk NOT IN (1, 2, 3)) THEN 1 ELSE 0 END AS unrated_risk,
           CASE WHEN (s.close_on >= p.period_start AND s.close_on < p.period_end + 1 AND s.effective_added_on < p.period_end + 1) AND TRUNC(s.history_close_on) = TRUNC(s.close_on) AND s.history_close_stage IN (41, 45) THEN 1 ELSE 0 END AS settled_com,
           CASE WHEN (s.close_on >= p.period_start AND s.close_on < p.period_end + 1 AND s.effective_added_on < p.period_end + 1) AND TRUNC(s.history_close_on) = TRUNC(s.close_on) AND s.history_close_stage IS NOT NULL AND s.history_close_stage NOT IN (41, 45) THEN 1 ELSE 0 END AS settled_aud
        FROM periods p
        JOIN v_exec_report_source s
          ON s.effective_added_on < p.period_end + 1
        WHERE 1 = 1
       AND (P_ENTITY_TYPE_ID IS NULL OR s.entity_type_id = P_ENTITY_TYPE_ID)
       AND (P_AUDITED_BY_ID IS NULL OR s.audited_by = P_AUDITED_BY_ID)
       AND (P_SNAP_AUDITED_BY_ID IS NULL OR s.audited_by = P_SNAP_AUDITED_BY_ID)
       AND (P_SNAP_DIVISION_CODE IS NULL OR EXISTS (
                SELECT 1 FROM (
                  SELECT 112243 division_id, 'FAD' division_code FROM dual
                  UNION ALL SELECT 113254, 'MAD' FROM dual
                ) d WHERE (P_SNAP_DIVISION_CODE = '1' OR d.division_code = P_SNAP_DIVISION_CODE)
                  AND (s.audited_by = d.division_id OR EXISTS (
                    SELECT 1 FROM V_IAS_ENTITY_REPORTING_NAMES h
                    WHERE h.entity_id = s.audited_by AND d.division_id IN
                      (h.reporting_1,h.reporting_2,h.reporting_3,h.reporting_4,h.reporting_5,h.reporting_6)))))
       AND (P_REPORTING_OFFICE_ID IS NULL
            OR s.entity_id = P_REPORTING_OFFICE_ID
            OR P_REPORTING_OFFICE_ID IN
               (s.reporting_1, s.reporting_2, s.reporting_3,
                s.reporting_4, s.reporting_5, s.reporting_6))
       AND (P_SCOPE_ROOT_ID IS NULL OR s.entity_id = P_SCOPE_ROOT_ID
            OR P_SCOPE_ROOT_ID IN
               (s.reporting_1, s.reporting_2, s.reporting_3,
                s.reporting_4, s.reporting_5, s.reporting_6))
       AND (P_ENTITY_ID IS NULL OR s.entity_id = P_ENTITY_ID
            OR P_ENTITY_ID IN
               (s.reporting_1, s.reporting_2, s.reporting_3,
                s.reporting_4, s.reporting_5, s.reporting_6))
       AND ((P_REPORT_ID = 'E' AND s.audit_type = 'D')
            OR (P_REPORT_ID <> 'E' AND s.audit_type IN ('B', 'D')))
      ),
  facts AS (
    SELECT f.*,
           (f.settled_paras - f.settled_com - f.settled_aud)
             AS settled_unknown,
           (f.outstanding_paras - f.opening_balance -
            f.paras_added + f.settled_paras)
             AS reconciliation_difference
    FROM seed f
  )
      SELECT 'ANNUAL' AS row_kind,
             CAST(NULL AS NUMBER) AS group_id,
             CAST(NULL AS VARCHAR2(500)) AS group_name,
             EXTRACT(YEAR FROM period_start) AS item_id,
             TO_CHAR(period_start, 'YYYY') AS item_code,
             TO_CHAR(period_start, 'YYYY') AS item_name,
             period_start,
             period_end,
             NVL(SUM(opening_balance), 0) AS opening_balance,
           NVL(SUM(paras_added), 0) AS paras_added,
           NVL(SUM(opening_balance + paras_added), 0) AS total_paras,
           NVL(SUM(settled_paras), 0) AS settled_paras,
           NVL(SUM(outstanding_paras), 0) AS outstanding_paras,
           ROUND(100 * SUM(settled_paras) / NULLIF(SUM(opening_balance + paras_added), 0), 2) AS settlement_percentage,
           NVL(SUM(low_risk), 0) AS low_risk,
           NVL(SUM(medium_risk), 0) AS medium_risk,
           NVL(SUM(high_risk), 0) AS high_risk,
           NVL(SUM(unrated_risk), 0) AS unrated_risk,
           NVL(SUM(settled_aud), 0) AS settled_audit,
           NVL(SUM(settled_com), 0) AS settled_compliance,
           NVL(SUM(settled_unknown), 0) AS settled_unknown,
           0 AS reopening_events,
           0 AS resettlement_events,
           0 AS initially_closed_additions,
           NVL(SUM(reconciliation_difference), 0) AS reconciliation_difference,
           NVL(SUM(auditor_assignment_mismatch), 0) AS auditor_assignment_mismatch,
             CAST(NULL AS VARCHAR2(30)) AS ranking_group,
             CAST(NULL AS NUMBER) AS rank_position
      FROM facts
      GROUP BY period_start, period_end
      ORDER BY period_start;
    ELSE
      OPEN IO_CURSOR FOR
      WITH seed AS (
    SELECT P_START_DATE AS period_start, P_END_DATE AS period_end,
           s.*,
           CASE WHEN (s.effective_added_on < P_START_DATE AND (s.close_on IS NULL OR s.close_on >= P_START_DATE)) THEN 1 ELSE 0 END AS opening_balance,
           CASE WHEN (s.effective_added_on >= P_START_DATE AND s.effective_added_on < P_END_DATE + 1) THEN 1 ELSE 0 END AS paras_added,
           CASE WHEN (s.close_on >= P_START_DATE AND s.close_on < P_END_DATE + 1 AND s.effective_added_on < P_END_DATE + 1) THEN 1 ELSE 0 END AS settled_paras,
           CASE WHEN (s.effective_added_on < P_END_DATE + 1 AND (s.close_on IS NULL OR s.close_on >= P_END_DATE + 1)) THEN 1 ELSE 0 END AS outstanding_paras,
           CASE WHEN (s.effective_added_on < P_END_DATE + 1 AND (s.close_on IS NULL OR s.close_on >= P_END_DATE + 1)) AND s.risk = 1 THEN 1 ELSE 0 END AS high_risk,
           CASE WHEN (s.effective_added_on < P_END_DATE + 1 AND (s.close_on IS NULL OR s.close_on >= P_END_DATE + 1)) AND s.risk = 2 THEN 1 ELSE 0 END AS medium_risk,
           CASE WHEN (s.effective_added_on < P_END_DATE + 1 AND (s.close_on IS NULL OR s.close_on >= P_END_DATE + 1)) AND s.risk = 3 THEN 1 ELSE 0 END AS low_risk,
           CASE WHEN (s.effective_added_on < P_END_DATE + 1 AND (s.close_on IS NULL OR s.close_on >= P_END_DATE + 1)) AND (s.risk IS NULL OR s.risk NOT IN (1, 2, 3)) THEN 1 ELSE 0 END AS unrated_risk,
           CASE WHEN (s.close_on >= P_START_DATE AND s.close_on < P_END_DATE + 1 AND s.effective_added_on < P_END_DATE + 1) AND TRUNC(s.history_close_on) = TRUNC(s.close_on) AND s.history_close_stage IN (41, 45) THEN 1 ELSE 0 END AS settled_com,
           CASE WHEN (s.close_on >= P_START_DATE AND s.close_on < P_END_DATE + 1 AND s.effective_added_on < P_END_DATE + 1) AND TRUNC(s.history_close_on) = TRUNC(s.close_on) AND s.history_close_stage IS NOT NULL AND s.history_close_stage NOT IN (41, 45) THEN 1 ELSE 0 END AS settled_aud
        FROM v_exec_report_source s
        WHERE s.effective_added_on < P_END_DATE + 1
       AND (P_ENTITY_TYPE_ID IS NULL OR s.entity_type_id = P_ENTITY_TYPE_ID)
       AND (P_AUDITED_BY_ID IS NULL OR s.audited_by = P_AUDITED_BY_ID)
       AND (P_SNAP_AUDITED_BY_ID IS NULL OR s.audited_by = P_SNAP_AUDITED_BY_ID)
       AND (P_SNAP_DIVISION_CODE IS NULL OR EXISTS (
                SELECT 1 FROM (
                  SELECT 112243 division_id, 'FAD' division_code FROM dual
                  UNION ALL SELECT 113254, 'MAD' FROM dual
                ) d WHERE (P_SNAP_DIVISION_CODE = '1' OR d.division_code = P_SNAP_DIVISION_CODE)
                  AND (s.audited_by = d.division_id OR EXISTS (
                    SELECT 1 FROM V_IAS_ENTITY_REPORTING_NAMES h
                    WHERE h.entity_id = s.audited_by AND d.division_id IN
                      (h.reporting_1,h.reporting_2,h.reporting_3,h.reporting_4,h.reporting_5,h.reporting_6)))))
       AND (P_REPORTING_OFFICE_ID IS NULL
            OR s.entity_id = P_REPORTING_OFFICE_ID
            OR P_REPORTING_OFFICE_ID IN
               (s.reporting_1, s.reporting_2, s.reporting_3,
                s.reporting_4, s.reporting_5, s.reporting_6))
       AND (P_SCOPE_ROOT_ID IS NULL OR s.entity_id = P_SCOPE_ROOT_ID
            OR P_SCOPE_ROOT_ID IN
               (s.reporting_1, s.reporting_2, s.reporting_3,
                s.reporting_4, s.reporting_5, s.reporting_6))
       AND (P_ENTITY_ID IS NULL OR s.entity_id = P_ENTITY_ID
            OR P_ENTITY_ID IN
               (s.reporting_1, s.reporting_2, s.reporting_3,
                s.reporting_4, s.reporting_5, s.reporting_6))
       AND ((P_REPORT_ID = 'E' AND s.audit_type = 'D')
            OR (P_REPORT_ID <> 'E' AND s.audit_type IN ('B', 'D')))
      ),
  facts AS (
    SELECT f.*,
           (f.settled_paras - f.settled_com - f.settled_aud)
             AS settled_unknown,
           (f.outstanding_paras - f.opening_balance -
            f.paras_added + f.settled_paras)
             AS reconciliation_difference
    FROM seed f
  ),
  -- Report C's GM/Region dimensions must be derived from the auditee's
  -- approved GM mapping. Never join the raw mapping to para rows: multiple
  -- mapping records would multiply balances and settlements.
  report_c_direct_gm AS (
    SELECT m.entity_id,
           CASE WHEN COUNT(DISTINCT m.gm_office) = 1
                THEN MAX(CASE WHEN gm.type_id = 21 THEN m.gm_office END)
           END AS mapped_gm_id,
           CASE WHEN COUNT(DISTINCT m.audit_by) = 1
                THEN MAX(m.audit_by) END AS mapped_audit_by,
           CASE WHEN COUNT(DISTINCT m.audit_by) > 1
                THEN 1 ELSE 0 END AS ambiguous_audit_by
      FROM t_auditee_entities_maping m
      LEFT JOIN t_auditee_entities gm
        ON gm.entity_id = m.gm_office
     WHERE NVL(m.status, 'Y') = 'Y'
     GROUP BY m.entity_id
  ),
  report_c_auditor_gm AS (
    -- HO units (audit family B, not Branch/Region/GM/CAU): only
    -- infer a GM Office by matching AUDIT_BY when it is UNIQUE.
    -- Ambiguous auditors remain Unassigned GM for correction.
    SELECT m.audit_by,
           CASE WHEN COUNT(DISTINCT m.entity_id) = 1
                THEN MAX(m.entity_id) END AS unique_gm_id
      FROM t_auditee_entities_maping m
      JOIN t_auditee_entities gm
        ON gm.entity_id = m.entity_id AND gm.type_id = 21
     WHERE NVL(m.status, 'Y') = 'Y'
       AND m.audit_by IS NOT NULL
     GROUP BY m.audit_by
  ),
  report_c_gm_auditor AS (
    SELECT m.entity_id,
           CASE WHEN COUNT(DISTINCT m.audit_by) = 1
                THEN MAX(m.audit_by) END AS audited_by
      FROM t_auditee_entities_maping m
      JOIN t_auditee_entities gm
        ON gm.entity_id = m.entity_id AND gm.type_id = 21
     WHERE NVL(m.status, 'Y') = 'Y'
     GROUP BY m.entity_id
  ),
  report_c_offices AS (
    SELECT f.*,
           CASE
             WHEN f.entity_type_id = 21 THEN f.entity_id
             WHEN f.audit_type = 'B'
                  AND f.entity_type_id NOT IN (6, 21, 22, 28)
               THEN CASE WHEN NVL(md.ambiguous_audit_by, 0) = 0
                         THEN COALESCE(
                           CASE WHEN gm_audited.audited_by =
                                     COALESCE(md.mapped_audit_by, f.audited_by)
                                THEN md.mapped_gm_id END,
                           ag.unique_gm_id)
                    END
             ELSE COALESCE(md.mapped_gm_id, f.gm_office_id)
           END AS report_c_gm_id,
           CASE
             WHEN f.entity_type_id IN (21, 22, 28)
               THEN f.entity_id
             WHEN f.audit_type = 'B'
                  AND f.entity_type_id NOT IN (6, 21, 22, 28)
               THEN f.entity_id
             WHEN f.entity_type_id = 6
               THEN COALESCE(f.region_id, parent_region.entity_id)
             ELSE f.region_id
           END AS report_c_item_id
      FROM facts f
      LEFT JOIN report_c_direct_gm md
        ON md.entity_id = f.entity_id AND P_REPORT_ID = 'C'
      LEFT JOIN report_c_auditor_gm ag
        ON ag.audit_by = COALESCE(md.mapped_audit_by, f.audited_by)
       AND P_REPORT_ID = 'C'
      LEFT JOIN report_c_gm_auditor gm_audited
        ON gm_audited.entity_id = md.mapped_gm_id
       AND P_REPORT_ID = 'C'
      LEFT JOIN t_auditee_entities parent_region
        ON parent_region.entity_id = f.reporting_1
       AND parent_region.type_id = 22 AND P_REPORT_ID = 'C'
  ),
      -- Report E maps the audited entity's unique division to the existing
      -- Group/Division view. Reduce mapping BEFORE joining individual paras.
      -- The view itself does not apply STATUS, so require an active matching row.
      group_report_candidates AS (
        SELECT DISTINCT r.entity_id division_id, r.parent_id group_id,
               r.p_name group_name, r.c_name division_name
          FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r
         WHERE EXISTS (
               SELECT 1 FROM t_auditee_entities_maping m
                WHERE m.entity_id = r.entity_id AND m.parent_id = r.parent_id
                  AND m.c_type_id = 3
                  AND m.p_name = r.p_name AND m.c_name = r.c_name
                  AND NVL(m.status,'Y') = 'Y')
      ),
      group_report_map AS (
        SELECT division_id, COUNT(*) mapping_paths,
               MAX(group_id) group_id, MAX(group_name) group_name,
               MAX(division_name) division_name
          FROM group_report_candidates
         GROUP BY division_id
      ),
      dimensions AS (
        SELECT f.*,
               CASE P_REPORT_ID
                 WHEN 'B' THEN CASE WHEN f.audit_type = 'B' THEN 1 ELSE 2 END
                 WHEN 'C' THEN f.report_c_gm_id
                 WHEN 'D' THEN CAST(NULL AS NUMBER)
                 WHEN 'E' THEN CASE WHEN gr.mapping_paths = 1
                              AND (f.business_group_id IS NULL OR f.business_group_id = gr.group_id)
                              THEN gr.group_id END
               END AS group_id,
               CASE P_REPORT_ID
                 WHEN 'B' THEN CASE WHEN f.audit_type = 'B'
                                    THEN 'GM Field Offices'
                                    ELSE 'Other Field Offices' END
                 WHEN 'C' THEN NVL(report_c_gm.name, 'Unassigned GM')
                 WHEN 'D' THEN CAST(NULL AS VARCHAR2(500))
                 WHEN 'E' THEN CASE WHEN gr.mapping_paths = 1
                              AND (f.business_group_id IS NULL OR f.business_group_id = gr.group_id)
                              THEN gr.group_name ELSE 'Unmapped / conflicting Group' END
               END AS report_group_name,
               CASE P_REPORT_ID
                 WHEN 'B' THEN f.audited_by
                 WHEN 'C' THEN f.report_c_item_id
                 WHEN 'D' THEN f.region_id
                 WHEN 'E' THEN CASE WHEN gr.mapping_paths = 1
                              AND (f.business_group_id IS NULL OR f.business_group_id = gr.group_id)
                              THEN gr.division_id END
               END AS item_id,
               CASE P_REPORT_ID
                 WHEN 'C' THEN TO_CHAR(report_c_item.code)
                 WHEN 'D' THEN TO_CHAR(reg.code)
                 ELSE CAST(NULL AS VARCHAR2(80))
               END AS item_code,
               CASE P_REPORT_ID
                 WHEN 'B' THEN NVL(f.audited_by_name, 'Unassigned Audit Office')
                 WHEN 'C' THEN NVL(report_c_item.name, 'Unassigned Region / H.O. Unit')
                 WHEN 'D' THEN NVL(reg.name, 'Unassigned Region')
                 WHEN 'E' THEN CASE WHEN gr.mapping_paths = 1
                              AND (f.business_group_id IS NULL OR f.business_group_id = gr.group_id)
                              THEN gr.division_name ELSE 'Unmapped / conflicting Division' END
               END AS item_name
        FROM report_c_offices f
        LEFT JOIN group_report_map gr
          ON P_REPORT_ID = 'E'
         AND gr.division_id = COALESCE(f.division_id,
                                 CASE WHEN f.entity_type_id = 3 THEN f.entity_id END)
        LEFT JOIN t_auditee_entities reg ON reg.entity_id = f.region_id
        LEFT JOIN t_auditee_entities report_c_gm
          ON report_c_gm.entity_id = f.report_c_gm_id
         AND report_c_gm.type_id = 21 AND P_REPORT_ID = 'C'
        LEFT JOIN t_auditee_entities report_c_item
          ON report_c_item.entity_id = f.report_c_item_id
         AND P_REPORT_ID = 'C'
      ),
      aggregates AS (
        SELECT CASE
                 WHEN GROUPING(group_id) = 1 THEN 'TOTAL'
                 WHEN GROUPING(item_id) = 1 THEN 'SUBTOTAL'
                 ELSE 'DETAIL'
               END AS row_kind,
               group_id, report_group_name AS group_name, item_id, item_code, item_name,
               P_START_DATE AS period_start,
               P_END_DATE AS period_end,
               NVL(SUM(opening_balance), 0) AS opening_balance,
           NVL(SUM(paras_added), 0) AS paras_added,
           NVL(SUM(opening_balance + paras_added), 0) AS total_paras,
           NVL(SUM(settled_paras), 0) AS settled_paras,
           NVL(SUM(outstanding_paras), 0) AS outstanding_paras,
           ROUND(100 * SUM(settled_paras) / NULLIF(SUM(opening_balance + paras_added), 0), 2) AS settlement_percentage,
           NVL(SUM(low_risk), 0) AS low_risk,
           NVL(SUM(medium_risk), 0) AS medium_risk,
           NVL(SUM(high_risk), 0) AS high_risk,
           NVL(SUM(unrated_risk), 0) AS unrated_risk,
           NVL(SUM(settled_aud), 0) AS settled_audit,
           NVL(SUM(settled_com), 0) AS settled_compliance,
           NVL(SUM(settled_unknown), 0) AS settled_unknown,
           0 AS reopening_events,
           0 AS resettlement_events,
           0 AS initially_closed_additions,
           NVL(SUM(reconciliation_difference), 0) AS reconciliation_difference,
           NVL(SUM(auditor_assignment_mismatch), 0) AS auditor_assignment_mismatch
        FROM dimensions
        GROUP BY GROUPING SETS (
          (group_id, report_group_name, item_id, item_code, item_name),
          (group_id, report_group_name),
          ()
        )
      ),
      ranked AS (
        SELECT a.*,
               ROW_NUMBER() OVER (
                 ORDER BY a.settlement_percentage DESC NULLS LAST,
                          a.total_paras DESC, a.item_id) AS top_rank,
               ROW_NUMBER() OVER (
                 ORDER BY a.settlement_percentage ASC NULLS LAST,
                          a.total_paras DESC, a.item_id) AS bottom_rank
        FROM aggregates a
        WHERE a.row_kind = 'DETAIL'
          AND a.item_id IS NOT NULL
          AND a.total_paras > 0
      ),
      final_rows AS (
        SELECT a.*,
               CAST(NULL AS VARCHAR2(30)) AS ranking_group,
               CAST(NULL AS NUMBER) AS rank_position
        FROM aggregates a
        WHERE P_REPORT_ID <> 'D'
          AND (P_REPORT_ID <> 'E' OR a.row_kind <> 'SUBTOTAL')
          -- Report E: exclude only rows with no opening stock, movement,
          -- or closing stock. Preserve legitimate individual zero metrics.
          AND (P_REPORT_ID <> 'E'
               OR a.opening_balance <> 0
               OR a.paras_added <> 0
               OR a.total_paras <> 0
               OR a.settled_paras <> 0
               OR a.outstanding_paras <> 0)
        UNION ALL
        SELECT row_kind, group_id, group_name, item_id, item_code, item_name,
           period_start, period_end, opening_balance, paras_added,
           total_paras, settled_paras, outstanding_paras, settlement_percentage,
           low_risk, medium_risk, high_risk, unrated_risk,
           settled_audit, settled_compliance, settled_unknown,
           reopening_events, resettlement_events, initially_closed_additions,
           reconciliation_difference, auditor_assignment_mismatch,
               'TOP' AS ranking_group,
               top_rank AS rank_position
        FROM ranked
        WHERE P_REPORT_ID = 'D' AND top_rank <= 5
        UNION ALL
        SELECT row_kind, group_id, group_name, item_id, item_code, item_name,
           period_start, period_end, opening_balance, paras_added,
           total_paras, settled_paras, outstanding_paras, settlement_percentage,
           low_risk, medium_risk, high_risk, unrated_risk,
           settled_audit, settled_compliance, settled_unknown,
           reopening_events, resettlement_events, initially_closed_additions,
           reconciliation_difference, auditor_assignment_mismatch,
               'BOTTOM' AS ranking_group,
               bottom_rank AS rank_position
        FROM ranked
        WHERE P_REPORT_ID = 'D' AND bottom_rank <= 5
        UNION ALL
        SELECT row_kind, group_id, group_name, item_id, item_code, item_name,
           period_start, period_end, opening_balance, paras_added,
           total_paras, settled_paras, outstanding_paras, settlement_percentage,
           low_risk, medium_risk, high_risk, unrated_risk,
           settled_audit, settled_compliance, settled_unknown,
           reopening_events, resettlement_events, initially_closed_additions,
           reconciliation_difference, auditor_assignment_mismatch,
               'UNRANKED' AS ranking_group,
               CAST(NULL AS NUMBER) AS rank_position
        FROM aggregates
        WHERE P_REPORT_ID = 'D'
          AND row_kind = 'DETAIL'
          AND (item_id IS NULL OR total_paras = 0)
      )
      SELECT * FROM final_rows
      ORDER BY ranking_group, rank_position, group_name, group_id,
               CASE row_kind WHEN 'DETAIL' THEN 0
                             WHEN 'SUBTOTAL' THEN 1 ELSE 2 END,
               item_name, item_id;
    END IF;
  END OPEN_REPORT;

  PROCEDURE P_ANNUAL_FIELD_PROGRESS(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_SCOPE_ROOT_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER,
      P_AUDITED_BY_ID IN NUMBER,
      P_REPORTING_OFFICE_ID IN NUMBER,
      P_ENTITY_ID IN NUMBER,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL) IS
  BEGIN
    OPEN_REPORT('A', P_DASHBOARD_TYPE, P_SCOPE_ROOT_ID,
                P_START_DATE, P_END_DATE, P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID, P_REPORTING_OFFICE_ID,
                P_ENTITY_ID, IO_CURSOR, P_SNAP_DIVISION_CODE, P_SNAP_AUDITED_BY_ID);
  END P_ANNUAL_FIELD_PROGRESS;

  PROCEDURE P_FIELD_OFFICE_POSITION(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_SCOPE_ROOT_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER,
      P_AUDITED_BY_ID IN NUMBER,
      P_REPORTING_OFFICE_ID IN NUMBER,
      P_ENTITY_ID IN NUMBER,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL) IS
  BEGIN
    OPEN_REPORT('B', P_DASHBOARD_TYPE, P_SCOPE_ROOT_ID,
                P_START_DATE, P_END_DATE, P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID, P_REPORTING_OFFICE_ID,
                P_ENTITY_ID, IO_CURSOR, P_SNAP_DIVISION_CODE, P_SNAP_AUDITED_BY_ID);
  END P_FIELD_OFFICE_POSITION;

  PROCEDURE P_GM_REGION_POSITION(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_SCOPE_ROOT_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER,
      P_AUDITED_BY_ID IN NUMBER,
      P_REPORTING_OFFICE_ID IN NUMBER,
      P_ENTITY_ID IN NUMBER,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL) IS
  BEGIN
    OPEN_REPORT('C', P_DASHBOARD_TYPE, P_SCOPE_ROOT_ID,
                P_START_DATE, P_END_DATE, P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID, P_REPORTING_OFFICE_ID,
                P_ENTITY_ID, IO_CURSOR, P_SNAP_DIVISION_CODE, P_SNAP_AUDITED_BY_ID);
  END P_GM_REGION_POSITION;

  PROCEDURE P_REGION_RANKINGS(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_SCOPE_ROOT_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER,
      P_AUDITED_BY_ID IN NUMBER,
      P_REPORTING_OFFICE_ID IN NUMBER,
      P_ENTITY_ID IN NUMBER,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL) IS
  BEGIN
    OPEN_REPORT('D', P_DASHBOARD_TYPE, P_SCOPE_ROOT_ID,
                P_START_DATE, P_END_DATE, P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID, P_REPORTING_OFFICE_ID,
                P_ENTITY_ID, IO_CURSOR, P_SNAP_DIVISION_CODE, P_SNAP_AUDITED_BY_ID);
  END P_REGION_RANKINGS;

  PROCEDURE P_MANAGEMENT_SETTLEMENT(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_SCOPE_ROOT_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER,
      P_AUDITED_BY_ID IN NUMBER,
      P_REPORTING_OFFICE_ID IN NUMBER,
      P_ENTITY_ID IN NUMBER,
      IO_CURSOR OUT SYS_REFCURSOR,
      P_SNAP_DIVISION_CODE IN VARCHAR2 DEFAULT NULL,
      P_SNAP_AUDITED_BY_ID IN NUMBER DEFAULT NULL) IS
  BEGIN
    OPEN_REPORT('E', P_DASHBOARD_TYPE, P_SCOPE_ROOT_ID,
                P_START_DATE, P_END_DATE, P_ENTITY_TYPE_ID,
                P_AUDITED_BY_ID, P_REPORTING_OFFICE_ID,
                P_ENTITY_ID, IO_CURSOR, P_SNAP_DIVISION_CODE, P_SNAP_AUDITED_BY_ID);
  END P_MANAGEMENT_SETTLEMENT;
END PKG_EXEC_REPORTS;
/
