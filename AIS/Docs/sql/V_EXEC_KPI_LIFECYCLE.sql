-- Oracle 18c. Compile revised V_EXEC_KPI_SRC_A first (F8), then this view.
-- Legacy SRC_O/SRC_C are intentionally unchanged.
-- ISSUE uses the latest conclusion of the current cycle and its persisted flag.
-- PARA source recognition remains unchanged in V_EXEC_KPI_SRC_A.
-- Existing column names and order are preserved.
-- One ISSUE per modern T_AU_OBSERVATION.ID (with matching engagement).
-- One PARA per post-compliance COM_ID across the three recognized indicators.
-- For legacy O/C, no audit engagement dates or 13/14 milestones are assumed.
-- Other/NULL IND codes are not silently relabeled as O/C; verify separately.
CREATE OR REPLACE VIEW V_EXEC_KPI_LIFECYCLE AS
WITH report_events AS
(
    SELECT l.e_id AS eng_id,
           MAX(l.created_on) AS concluded_on
      FROM t_au_plan_eng_log l
     WHERE l.status_id = 14
       AND NOT EXISTS (SELECT 1 FROM t_au_plan_eng_log r
                        WHERE r.e_id = l.e_id AND r.status_id = 10 AND r.id > l.id)
     GROUP BY l.e_id
),
issues AS
(
    SELECT
           'ISSUE' AS record_type,
           o.id AS record_id,
           o.id AS observation_id,
           CAST(NULL AS NUMBER) AS com_id,
           o.engplanid AS eng_id,
           e.period_id AS period_id,
           e.audit_startdate AS audit_startdate,
           e.audit_enddate AS audit_enddate,
           CAST(NULL AS DATE) AS report_finalized_on,
           ev.concluded_on AS report_concluded_on,
           e.entity_id AS entity_id,
           ae.type_id AS entity_type_id,
           e.auditby_id AS audited_by_id,
           o.severity AS risk_id,
           o.status AS observation_status,
           CAST(NULL AS NUMBER) AS para_status,
           CAST(NULL AS NUMBER) AS com_cycle,
           o.entereddate AS issue_identified_on,
           CASE
             WHEN e.status = 14
              AND o.status IN (4, 9)
              AND NVL(o.issue_rectified, 0) = 1
              AND o.stelled_on IS NOT NULL
              AND ev.concluded_on IS NOT NULL
              AND o.stelled_on < ev.concluded_on
             THEN o.stelled_on
           END AS issue_rectified_on,
           o.final_para_added_on AS final_para_assigned_on,
           CAST(NULL AS DATE) AS raw_para_added_on,
           CAST(NULL AS DATE) AS raw_para_settled_on,
           CAST(NULL AS DATE) AS para_added_on,
           CAST(NULL AS DATE) AS para_settled_on,
           CASE
             WHEN e.status = 14 AND o.status IN (4, 8, 9, 23)
             THEN 1 ELSE 0
           END AS issue_eligible,
           0 AS excluded_pre_finalization,
           0 AS current_open,
           0 AS current_high_risk_open,
           0 AS current_over_one_year,
           0 AS current_over_three_cycles,
           'A' AS source_ind,
           o.id AS source_para_id,
           1 AS source_record_found,
           o.status AS source_status
      FROM t_au_observation o
      JOIN t_au_plan_eng e ON e.eng_id = o.engplanid
      LEFT JOIN t_auditee_entities ae ON ae.entity_id = e.entity_id
      LEFT JOIN report_events ev ON ev.eng_id = e.eng_id
),
kpi_rows AS
(
    SELECT * FROM issues
    UNION ALL SELECT * FROM v_exec_kpi_src_a
    UNION ALL SELECT * FROM v_exec_kpi_src_o
    UNION ALL SELECT * FROM v_exec_kpi_src_c
)
SELECT k.*,
       s.reporting_office_id,
       s.reporting_1,
       s.reporting_2,
       s.reporting_3,
       s.reporting_4,
       s.reporting_5,
       s.reporting_6
  FROM kpi_rows k
  LEFT JOIN v_exec_entity_scope s
    ON s.entity_id = k.entity_id;