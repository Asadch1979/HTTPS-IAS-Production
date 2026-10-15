-- Legacy CAD: IND='C', NEW_PARA_ID -> T_AU_OBSERVATION_OLD_CAD_PARAS.PARA_ID.
-- Legacy rows do not need a report-finalization date and are never excluded
-- by the modern pre-finalization rule.
CREATE OR REPLACE VIEW V_EXEC_KPI_SRC_C AS
SELECT
       'PARA' AS record_type,
       p.com_id AS record_id,
       CAST(NULL AS NUMBER) AS observation_id,
       p.com_id AS com_id,
       CAST(NULL AS NUMBER) AS eng_id,
       CAST(NULL AS NUMBER) AS period_id,
       CAST(NULL AS DATE) AS audit_startdate,
       CAST(NULL AS DATE) AS audit_enddate,
       CAST(NULL AS DATE) AS report_finalized_on,
       CAST(NULL AS DATE) AS report_concluded_on,
       p.entity_id AS entity_id,
       p.entity_type_id AS entity_type_id,
       p.audited_by AS audited_by_id,
       p.risk AS risk_id,
       CAST(NULL AS NUMBER) AS observation_status,
       p.para_status AS para_status,
       p.com_cycle AS com_cycle,
       CAST(NULL AS DATE) AS issue_identified_on,
       CAST(NULL AS DATE) AS issue_rectified_on,
       CAST(NULL AS DATE) AS final_para_assigned_on,
       p.para_added_on AS raw_para_added_on,
       p.setteled_on AS raw_para_settled_on,
       p.para_added_on AS para_added_on,
       CASE WHEN p.para_status = 9 THEN p.setteled_on END AS para_settled_on,
       0 AS issue_eligible,
       0 AS excluded_pre_finalization,
       CASE WHEN p.para_status = 8 THEN 1 ELSE 0 END AS current_open,
       CASE WHEN p.para_status = 8 AND p.risk = 1 THEN 1 ELSE 0 END AS current_high_risk_open,
       CASE WHEN p.para_status = 8 AND p.para_added_on IS NOT NULL
                      AND TRUNC(SYSDATE) - TRUNC(p.para_added_on) > 365
                 THEN 1 ELSE 0 END AS current_over_one_year,
       CASE WHEN p.para_status = 8 AND NVL(p.com_cycle, 0) > 3 THEN 1 ELSE 0 END AS current_over_three_cycles,
       'C' AS source_ind,
       p.new_para_id AS source_para_id,
       CASE WHEN f.para_id IS NOT NULL THEN 1 ELSE 0 END AS source_record_found,
       f.para_status AS source_status
  FROM ais_t_au_post_compliance p
  LEFT JOIN t_au_observation_old_cad_paras f
    ON f.para_id = p.new_para_id
 WHERE p.ind = 'C';