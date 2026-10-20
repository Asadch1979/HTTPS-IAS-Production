-- Compatibility projection for existing Oracle clients of V_EXEC_KPI_SRC_A.
-- Modern and legacy KPI facts are now calculated in V_EXEC_KPI_LIFECYCLE
-- using AIS_T_AU_POST_COMPLIANCE alone; no second definition of counts.
-- Column names and order are unchanged. Compile lifecycle view FIRST.
-- Missing original-source metadata remains NULL rather than guessed.
CREATE OR REPLACE VIEW V_EXEC_KPI_SRC_A AS
SELECT
    record_type,
    record_id,
    observation_id,
    com_id,
    eng_id,
    period_id,
    audit_startdate,
    audit_enddate,
    report_finalized_on,
    report_concluded_on,
    entity_id,
    entity_type_id,
    audited_by_id,
    risk_id,
    observation_status,
    para_status,
    com_cycle,
    issue_identified_on,
    issue_rectified_on,
    final_para_assigned_on,
    raw_para_added_on,
    raw_para_settled_on,
    para_added_on,
    para_settled_on,
    issue_eligible,
    excluded_pre_finalization,
    current_open,
    current_high_risk_open,
    current_over_one_year,
    current_over_three_cycles,
    source_ind,
    source_para_id,
    source_record_found,
    source_status
  FROM V_EXEC_KPI_LIFECYCLE
 WHERE record_type = 'PARA'
   AND source_ind = 'A';
