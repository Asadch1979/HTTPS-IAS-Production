-- IAS CIA issue totals / TOP rectified exclusion smoke checks.
-- Oracle SQL Developer F8: execute each statement/block separately.
-- Expected USER_ERRORS: zero rows.
SELECT name, type, line, position, text
FROM user_errors
WHERE name = 'PKG_EXEC_DASHBOARD'
ORDER BY type, sequence;

-- CIA no date range: eligible all-time issue counts in authorized scope.
DECLARE
  c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUES(
    P_DASHBOARD_TYPE => 'CIA',
    P_ENTITY_ID => NULL,
    P_START_DATE => NULL,
    P_END_DATE => NULL,
    IO_CURSOR => c
  );
  DBMS_SQL.RETURN_RESULT(c);
END;

-- CIA selected 2026 date range, exact same organizational scope.
DECLARE
  c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUES(
    P_DASHBOARD_TYPE => 'CIA',
    P_ENTITY_ID => NULL,
    P_START_DATE => DATE '2026-01-01',
    P_END_DATE => DATE '2026-10-15',
    IO_CURSOR => c
  );
  DBMS_SQL.RETURN_RESULT(c);
END;

-- TOP counts: Issue Rectified explicitly excludes issue status 23;
-- ISSUE_DROPPED is returned separately. Compare observed 2026 figures.
DECLARE
  c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE(
    P_DASHBOARD_TYPE => 'TOP',
    P_ENTITY_ID => NULL,
    P_START_DATE => DATE '2026-01-01',
    P_END_DATE => DATE '2026-10-15',
    IO_CURSOR => c
  );
  DBMS_SQL.RETURN_RESULT(c);
END;

-- Diagnose any prospective dropped issue events that are not counted.
SELECT
   COUNT(*) AS identified_all_time,
   SUM(CASE WHEN issue_rectified_on IS NOT NULL
                AND observation_status IN (4,9) THEN 1 ELSE 0 END)
     AS rectified_excluding_dropped,
   SUM(CASE WHEN issue_rectified_on IS NOT NULL
                AND observation_status = 23 THEN 1 ELSE 0 END)
     AS dropped_separately
FROM v_exec_kpi_lifecycle
WHERE record_type = 'ISSUE' AND issue_eligible = 1;
