-- Oracle 18c / IAS Executive Dashboard lifecycle KPI verification.
-- Execute each SELECT separately using F8. No updates or commits.
-- Deploy V_EXEC_KPI_SRC_A/O/C and V_EXEC_KPI_LIFECYCLE BEFORE the
-- revised PKG_EXEC_DASHBOARD.sql package specification and body.
--
-- 1) Compile check (should return no rows after package deployment).
SELECT name, type, line, position, text
FROM user_errors
WHERE name IN ('PKG_EXEC_DASHBOARD','V_EXEC_KPI_LIFECYCLE',
               'V_EXEC_KPI_SRC_A','V_EXEC_KPI_SRC_O','V_EXEC_KPI_SRC_C')
ORDER BY name, type, sequence;

-- 2) Presence of the new procedure (should return two package subprogram rows).
SELECT object_name, procedure_name, subprogram_id
FROM user_procedures
WHERE object_name = 'PKG_EXEC_DASHBOARD'
  AND procedure_name = 'P_GET_KPI_LIFECYCLE';

-- 3) Cohort reconciliation: period 1 January to 15 October 2026 (inclusive),
-- for concluded engagements; expected from prior production check:
-- 14681 identified = 6789 pre-conclusion dispositions + 7892 final-track.
WITH concluded AS
(
    SELECT e_id AS eng_id, MIN(created_on) AS concluded_on
    FROM t_au_plan_eng_log
    WHERE status_id = 14
    GROUP BY e_id
)
SELECT
  COUNT(*) AS identified_cohort,
  SUM(CASE WHEN o.status IN (4,9,23) AND o.stelled_on < r.concluded_on
           THEN 1 ELSE 0 END) AS disposition_before_conclusion,
  SUM(CASE WHEN o.status = 8 OR
                (o.status = 9 AND o.stelled_on >= r.concluded_on)
           THEN 1 ELSE 0 END) AS final_track,
  SUM(CASE WHEN o.status = 23 AND o.stelled_on < r.concluded_on
           THEN 1 ELSE 0 END) AS dropped_subset
FROM t_au_observation o
JOIN t_au_plan_eng e ON e.eng_id = o.engplanid
JOIN concluded r ON r.eng_id = e.eng_id
WHERE e.status = 14
  AND o.entereddate >= DATE '2026-01-01'
  AND o.entereddate < DATE '2026-10-16';

-- 4) Date-based activity (different event clocks; NO identity is assumed).
-- Must agree with the new procedure output for the same filters.
SELECT
  SUM(CASE WHEN record_type = 'ISSUE'
             AND issue_eligible = 1
             AND issue_identified_on >= DATE '2026-01-01'
             AND issue_identified_on < DATE '2026-10-16'
           THEN 1 ELSE 0 END) AS issue_identified,
  SUM(CASE WHEN record_type = 'ISSUE'
             AND issue_rectified_on >= DATE '2026-01-01'
             AND issue_rectified_on < DATE '2026-10-16'
           THEN 1 ELSE 0 END) AS issue_rectified,
  SUM(CASE WHEN record_type = 'PARA'
             AND para_added_on >= DATE '2026-01-01'
             AND para_added_on < DATE '2026-10-16'
           THEN 1 ELSE 0 END) AS para_added,
  SUM(CASE WHEN record_type = 'PARA'
             AND para_settled_on >= DATE '2026-01-01'
             AND para_settled_on < DATE '2026-10-16'
           THEN 1 ELSE 0 END) AS paras_settled
FROM v_exec_kpi_lifecycle;

-- 5) Check how many status changes were actually logged per source.
-- A historical opening or closing count MUST NOT be inferred from
-- AIS_T_AU_POST_COMPLIANCE_HISTORY.COM_STATUS alone.
SELECT
  p.ind,
  COUNT(*) AS total_paras,
  SUM(CASE WHEN p.para_status = 8 THEN 1 ELSE 0 END) AS currently_open,
  SUM(CASE WHEN p.para_status = 9 THEN 1 ELSE 0 END) AS currently_settled,
  SUM(CASE WHEN p.para_status NOT IN (8,9) OR p.para_status IS NULL
           THEN 1 ELSE 0 END) AS other_current_status
FROM ais_t_au_post_compliance p
WHERE p.ind IN ('A','O','C')
GROUP BY p.ind
ORDER BY p.ind;

-- 6) Modern source status change evidence: frequency of authorizations,
-- including reopen (old=9,new=8) and re-settlement (old=8,new=9).
SELECT
  old_status,
  new_status,
  COUNT(*) AS authorized_changes,
  MIN(authorized_on) AS earliest_authorized,
  MAX(authorized_on) AS latest_authorized
FROM t_au_new_paras_status_change_log
WHERE authorized_on IS NOT NULL
GROUP BY old_status, new_status
ORDER BY old_status, new_status;

-- 7) Modern master para status-change evidence (covers all three IND types).
SELECT
  old_status,
  new_status,
  COUNT(*) AS authorized_changes
FROM ais_t_para_status_change_log
WHERE action_status = 'AUTHORIZED'
GROUP BY old_status, new_status
ORDER BY old_status, new_status;

-- Package cursor smoke test: run this standalone PL/SQL block using F8.
-- It does not write data. DBMS_SQL.RETURN_RESULT requires client support.
DECLARE
  c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE(
    P_DASHBOARD_TYPE => 'CIA',
    P_ENTITY_ID => NULL,
    P_START_DATE => DATE '2026-01-01',
    P_END_DATE => DATE '2026-10-15',
    P_ENTITY_TYPE_ID => NULL,
    P_AUDITED_BY_ID => NULL,
    P_REPORTING_OFFICE_ID => NULL,
    IO_CURSOR => c
  );
  DBMS_SQL.RETURN_RESULT(c);
END;
