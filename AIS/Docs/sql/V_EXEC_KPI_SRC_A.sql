-- Modern audit observations: linked by IND='A', NEW_PARA_ID -> T_AU_OBSERVATION.ID.
-- Business rule: Status 14 (audit concluded) is the ONLY cutoff milestone.
-- Observation statuses 4, 9 and 23 with STELLED_ON < CONCLUDED_ON are
-- dispositions before conclusion. They must NOT contribute to the A-source
-- Para Added or Paras Settled KPIs (regardless of current PARA_STATUS).
-- Missing conclusion milestones / settlement timestamps do not authorize
-- an exclusion. All raw records remain visible for reconciliation.
-- Column names/order are unchanged to preserve V_EXEC_KPI_LIFECYCLE UNION ALL.
CREATE OR REPLACE VIEW V_EXEC_KPI_SRC_A AS
WITH report_events AS
(
    SELECT l.e_id AS eng_id,
           MIN(l.created_on) AS concluded_on
      FROM t_au_plan_eng_log l
     WHERE l.status_id = 14
     GROUP BY l.e_id
)
SELECT
       'PARA' AS record_type,
       p.com_id AS record_id,
       o.id AS observation_id,
       p.com_id AS com_id,
       e.eng_id AS eng_id,
       e.period_id AS period_id,
       e.audit_startdate AS audit_startdate,
       e.audit_enddate AS audit_enddate,
       CAST(NULL AS DATE) AS report_finalized_on,
       ev.concluded_on AS report_concluded_on,
       p.entity_id AS entity_id,
       p.entity_type_id AS entity_type_id,
       p.audited_by AS audited_by_id,
       p.risk AS risk_id,
       o.status AS observation_status,
       p.para_status AS para_status,
       p.com_cycle AS com_cycle,
       CAST(NULL AS DATE) AS issue_identified_on,
       CAST(NULL AS DATE) AS issue_rectified_on,
       o.final_para_added_on AS final_para_assigned_on,
       p.para_added_on AS raw_para_added_on,
       p.setteled_on AS raw_para_settled_on,
       CASE
         WHEN o.status IN (4, 9, 23)
          AND o.stelled_on IS NOT NULL
          AND ev.concluded_on IS NOT NULL
          AND o.stelled_on < ev.concluded_on
         THEN NULL
         ELSE p.para_added_on
       END AS para_added_on,
       CASE
         WHEN p.para_status <> 9 OR p.para_status IS NULL THEN NULL
         WHEN o.status IN (4, 9, 23)
          AND o.stelled_on IS NOT NULL
          AND ev.concluded_on IS NOT NULL
          AND o.stelled_on < ev.concluded_on
         THEN NULL
         ELSE p.setteled_on
       END AS para_settled_on,
       0 AS issue_eligible,
       CASE
         WHEN o.status IN (4, 9, 23)
          AND o.stelled_on IS NOT NULL
          AND ev.concluded_on IS NOT NULL
          AND o.stelled_on < ev.concluded_on
         THEN 1 ELSE 0
       END AS excluded_pre_finalization, -- name retained for backward compatibility; now means pre-conclusion
       CASE WHEN p.para_status = 8 THEN 1 ELSE 0 END AS current_open,
       CASE WHEN p.para_status = 8 AND p.risk = 1 THEN 1 ELSE 0 END AS current_high_risk_open,
       CASE WHEN p.para_status = 8 AND p.para_added_on IS NOT NULL
                      AND TRUNC(SYSDATE) - TRUNC(p.para_added_on) > 365
                 THEN 1 ELSE 0 END AS current_over_one_year,
       CASE WHEN p.para_status = 8 AND NVL(p.com_cycle, 0) > 3 THEN 1 ELSE 0 END AS current_over_three_cycles,
       'A' AS source_ind,
       p.new_para_id AS source_para_id,
       CASE WHEN o.id IS NOT NULL THEN 1 ELSE 0 END AS source_record_found,
       o.status AS source_status
  FROM ais_t_au_post_compliance p
  LEFT JOIN t_au_observation o
    ON o.id = p.new_para_id
  LEFT JOIN t_au_plan_eng e
    ON e.eng_id = o.engplanid
  LEFT JOIN report_events ev
    ON ev.eng_id = e.eng_id
 WHERE p.ind = 'A';