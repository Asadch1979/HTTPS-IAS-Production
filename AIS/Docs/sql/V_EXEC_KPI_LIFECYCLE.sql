-- IAS executive lifecycle. AIS_T_AU_POST_COMPLIANCE is the ONLY KPI fact table.
-- Its modern IND='A' rows are copied from observations at audit conclusion.
-- One ISSUE per distinct modern NEW_PARA_ID; one PARA per COM_ID for A/O/C.
--
-- T_AU_PERIOD and V_EXEC_ENTITY_SCOPE are reporting dimensions ONLY.
-- Period mapping is by exact normalized AUDIT_PERIOD/DESCRIPTION label;
-- ambiguous or unmatched labels return NULL (never a guessed period).
-- The view cannot reconstruct original observation status, engagement events,
-- pre-transfer status-4/23 observations, or actual conclusion timestamps.
-- Historical open/closing results using PARA_ADDED_ON remain ESTIMATES.
-- Do not interpret NULL source/detail fields as zero or false.
CREATE OR REPLACE VIEW V_EXEC_KPI_LIFECYCLE AS
WITH period_lookup AS
(
    SELECT UPPER(TRIM(t.description)) AS audit_period_key,
           CASE WHEN COUNT(DISTINCT t.auditperiodid) = 1
                THEN MIN(t.auditperiodid) END AS period_id
      FROM t_au_period t
     WHERE t.description IS NOT NULL
     GROUP BY UPPER(TRIM(t.description))
),
post_rows AS
(
    SELECT p.*,
           ap.period_id AS resolved_period_id,
           ROW_NUMBER() OVER (
               PARTITION BY p.ind,
                            CASE WHEN p.ind = 'A' THEN p.new_para_id
                                 ELSE p.com_id END
               ORDER BY p.com_id
           ) AS issue_row_num
      FROM ais_t_au_post_compliance p
      LEFT JOIN period_lookup ap
        ON ap.audit_period_key = UPPER(TRIM(p.audit_period))
     WHERE p.ind IN ('A', 'O', 'C')
),
lifecycle AS
(
    SELECT
        k.record_type,
        CASE WHEN k.record_type = 'ISSUE' THEN p.new_para_id
             ELSE p.com_id END AS record_id,
        CASE WHEN p.ind = 'A' THEN p.new_para_id END AS observation_id,
        CASE WHEN k.record_type = 'PARA' THEN p.com_id END AS com_id,
        CAST(NULL AS NUMBER) AS eng_id,
        CASE WHEN p.ind = 'A' THEN p.resolved_period_id
             ELSE CAST(NULL AS NUMBER) END AS period_id,
        CAST(NULL AS DATE) AS audit_startdate,
        CAST(NULL AS DATE) AS audit_enddate,
        CAST(NULL AS DATE) AS report_finalized_on,
        CAST(NULL AS DATE) AS report_concluded_on,
        p.entity_id AS entity_id,
        p.entity_type_id AS entity_type_id,
        p.audited_by AS audited_by_id,
        p.risk AS risk_id,
        CAST(NULL AS NUMBER) AS observation_status,
        CASE WHEN k.record_type = 'PARA' THEN p.para_status END AS para_status,
        CASE WHEN k.record_type = 'PARA' THEN p.com_cycle END AS com_cycle,
        CASE WHEN k.record_type = 'ISSUE' THEN p.para_added_on END AS issue_identified_on,
        -- The Rectified KPI uses the original issue recognition cohort:
        -- ISSUE_RECTIFIED=1 and PARA_ADDED_ON inside the selected range.
        -- ISSUE_RECTIFIED_ON retains its public alias for old consumers,
        -- but is a KPI recognition date, NOT the physical settlement date.
        CASE WHEN k.record_type = 'ISSUE'
                   AND NVL(p.issue_rectified, 0) = 1
             THEN p.para_added_on END AS issue_rectified_on,
        CAST(NULL AS DATE) AS final_para_assigned_on,
        CASE WHEN k.record_type = 'PARA' THEN p.para_added_on END AS raw_para_added_on,
        CASE WHEN k.record_type = 'PARA' THEN p.setteled_on END AS raw_para_settled_on,
        CASE WHEN k.record_type = 'PARA'
                   AND (p.ind <> 'A' OR NVL(p.issue_rectified, 0) <> 1)
             THEN p.para_added_on END AS para_added_on,
        CASE WHEN k.record_type = 'PARA'
                   AND p.para_status = 9
                   AND (p.ind <> 'A' OR NVL(p.issue_rectified, 0) <> 1)
             THEN p.setteled_on END AS para_settled_on,
        CASE WHEN k.record_type = 'ISSUE' THEN 1 ELSE 0 END AS issue_eligible,
        CASE WHEN k.record_type = 'PARA' AND p.ind = 'A'
                   AND NVL(p.issue_rectified, 0) = 1
             THEN 1 ELSE 0 END AS excluded_pre_finalization,
        CASE WHEN k.record_type = 'PARA' AND p.para_status = 8
                   AND (p.ind <> 'A' OR NVL(p.issue_rectified, 0) <> 1)
             THEN 1 ELSE 0 END AS current_open,
        CASE WHEN k.record_type = 'PARA' AND p.para_status = 8
                   AND p.risk = 1
                   AND (p.ind <> 'A' OR NVL(p.issue_rectified, 0) <> 1)
             THEN 1 ELSE 0 END AS current_high_risk_open,
        CASE WHEN k.record_type = 'PARA' AND p.para_status = 8
                   AND p.para_added_on IS NOT NULL
                   AND TRUNC(SYSDATE) - TRUNC(p.para_added_on) > 365
                   AND (p.ind <> 'A' OR NVL(p.issue_rectified, 0) <> 1)
             THEN 1 ELSE 0 END AS current_over_one_year,
        CASE WHEN k.record_type = 'PARA' AND p.para_status = 8
                   AND NVL(p.com_cycle, 0) > 3
                   AND (p.ind <> 'A' OR NVL(p.issue_rectified, 0) <> 1)
             THEN 1 ELSE 0 END AS current_over_three_cycles,
        p.ind AS source_ind,
        CASE WHEN p.ind = 'O' THEN p.old_para_id
             ELSE p.new_para_id END AS source_para_id,
        CAST(NULL AS NUMBER) AS source_record_found,
        CAST(NULL AS NUMBER) AS source_status
      FROM post_rows p
      CROSS JOIN (
          SELECT 'PARA' AS record_type FROM dual
          UNION ALL SELECT 'ISSUE' FROM dual
      ) k
     WHERE k.record_type = 'PARA'
        OR (p.ind = 'A' AND p.new_para_id IS NOT NULL AND p.issue_row_num = 1)
)
SELECT k.*,
       s.reporting_office_id,
       s.reporting_1,
       s.reporting_2,
       s.reporting_3,
       s.reporting_4,
       s.reporting_5,
       s.reporting_6
  FROM lifecycle k
  LEFT JOIN v_exec_entity_scope s
    ON s.entity_id = k.entity_id;
