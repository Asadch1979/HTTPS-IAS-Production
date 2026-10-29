-- CIA Position as on: read-only verification for two different cutoffs.
-- Set actual IAS audit-period ID and dates inside that period.
-- This checks the recorded engagement completion evidence only; the full
-- dashboard also applies audited-by division / department filters and plan joins.
-- Do not assume that unchanged numbers prove the date was ignored: numbers
-- legitimately stay constant between recorded engagement status movements.

-- Example bind setup for PL/SQL Developer: supply values before executing.
-- :P_AUDIT_PERIOD_ID (NUMBER)
-- :P_ASOF_EARLIER   (DATE)
-- :P_ASOF_LATER     (DATE)

SELECT d.position_date,
       COUNT(DISTINCT e.eng_id) AS engagement_count,
       COUNT(DISTINCT CASE WHEN e.position_status IN (13,14)
                           THEN e.entity_id END) AS completed_entities,
       COUNT(DISTINCT CASE WHEN e.position_status BETWEEN 4 AND 11
                           THEN e.team_id END) AS teams_in_progress,
       COUNT(DISTINCT CASE WHEN e.position_status IS NULL
                           THEN e.eng_id END) AS engagements_without_history
  FROM
      (SELECT TRUNC(:P_ASOF_EARLIER) AS position_date FROM dual
       UNION ALL
       SELECT TRUNC(:P_ASOF_LATER) AS position_date FROM dual) d
  JOIN
      (SELECT e.eng_id, e.period_id, e.entity_id, e.team_id,
              e.status AS current_master_status
         FROM t_au_plan_eng e
        WHERE e.period_id = :P_AUDIT_PERIOD_ID) e
    ON 1=1
  LEFT JOIN LATERAL
      (SELECT CASE WHEN l.status_id BETWEEN 1 AND 14
                        AND UPPER(NVL(l.remarks,' ')) NOT LIKE '%REVERS%'
                   THEN l.status_id END AS position_status
         FROM t_au_plan_eng_log l
        WHERE l.e_id = e.eng_id
          AND NVL(l.last_updated_on,l.created_on) < d.position_date+1
        ORDER BY NVL(l.last_updated_on,l.created_on) DESC, l.id DESC
        FETCH FIRST 1 ROW ONLY) history
    ON 1=1
 GROUP BY d.position_date
 ORDER BY d.position_date;

-- After deploying PKG_EXEC_DASHBOARD in QA, compare P_GET_CIA_EXEC_KPIS
-- with the two dates, same audit-period ID, division and department.
-- Make sure the API echoes POSITION_AS_ON exactly and KPI values correspond
-- to the recorded events. Planned counts are the approved plan cohort and
-- can stay unchanged. Observation status measures still rely on current
-- master status, so they are not certified historical snapshots.
