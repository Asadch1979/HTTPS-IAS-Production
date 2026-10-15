-- IAS lifecycle snapshot regression / Oracle 18c.
-- Execute each statement separately with F8, without SQL*Plus slash.
-- The source views must be compiled, then PKG_EXEC_DASHBOARD PACKAGE BODY.
-- This script does not change any data.

-- 1. Compilation errors: expect no rows.
SELECT NAME, TYPE, LINE, POSITION, TEXT
FROM USER_ERRORS
WHERE NAME = 'PKG_EXEC_DASHBOARD'
ORDER BY TYPE, SEQUENCE;

-- 2. Snapshot estimates by source for the observed report dates.
-- These represent the source A/O/C rows only and use date-based estimates.
WITH dates AS (
    SELECT DATE '2024-01-01' AS period_start,
           DATE '2026-10-15' AS end_exclusive
    FROM dual
)
SELECT k.source_ind,
       COUNT(*) AS all_para_rows,
       SUM(CASE WHEN k.para_added_on < d.period_start
                 AND (k.source_ind <> 'A' OR k.report_concluded_on IS NULL OR k.report_concluded_on < d.period_start)
                 AND (k.para_status = 8
                      OR (k.para_status = 9 AND k.para_settled_on >= d.period_start))
                THEN 1 ELSE 0 END) AS opening_estimate,
       SUM(CASE WHEN k.para_added_on < d.end_exclusive
                 AND (k.source_ind <> 'A' OR k.report_concluded_on IS NULL OR k.report_concluded_on < d.end_exclusive)
                 AND (k.para_status = 8
                      OR (k.para_status = 9 AND k.para_settled_on >= d.end_exclusive))
                THEN 1 ELSE 0 END) AS closing_estimate,
       SUM(CASE WHEN k.para_added_on < d.end_exclusive
                 AND (k.source_ind <> 'A' OR k.report_concluded_on IS NULL OR k.report_concluded_on < d.end_exclusive)
                 AND (k.para_status = 8
                      OR (k.para_status = 9 AND k.para_settled_on >= d.end_exclusive))
                 AND k.risk_id = 1
                THEN 1 ELSE 0 END) AS high_risk_estimate,
       SUM(CASE WHEN k.para_added_on < d.end_exclusive
                 AND (k.source_ind <> 'A' OR k.report_concluded_on IS NULL OR k.report_concluded_on < d.end_exclusive)
                 AND (k.para_status = 8
                      OR (k.para_status = 9 AND k.para_settled_on >= d.end_exclusive))
                 AND TRUNC(d.end_exclusive-1)-TRUNC(k.para_added_on)>365
                THEN 1 ELSE 0 END) AS over_one_year_estimate,
       SUM(CASE WHEN k.para_added_on < d.end_exclusive
                 AND (k.source_ind <> 'A' OR k.report_concluded_on IS NULL OR k.report_concluded_on < d.end_exclusive)
                 AND (k.para_status = 8
                      OR (k.para_status = 9 AND k.para_settled_on >= d.end_exclusive))
                 AND NVL(k.com_cycle, 0)>3
                THEN 1 ELSE 0 END) AS over_three_cycles_estimate
FROM V_EXEC_KPI_LIFECYCLE k
CROSS JOIN dates d
WHERE k.record_type = 'PARA'
GROUP BY k.source_ind
ORDER BY k.source_ind;

-- 3. Check how many authorized reopenings and resettlements are recorded.
-- These events are the reason historical master-state estimates are NOT certified.
SELECT
    CASE WHEN old_status = 9 AND new_status = 8 THEN 'REOPEN'
         WHEN old_status = 8 AND new_status = 9 THEN 'SETTLE'
         ELSE 'OTHER' END AS event_type,
    COUNT(*) AS authorized_events
FROM AIS_T_PARA_STATUS_CHANGE_LOG
WHERE action_status = 'AUTHORIZED'
GROUP BY CASE WHEN old_status = 9 AND new_status = 8 THEN 'REOPEN'
              WHEN old_status = 8 AND new_status = 9 THEN 'SETTLE'
              ELSE 'OTHER' END
ORDER BY event_type;

-- 4. Modern legacy log may also contain reopen events.
SELECT old_status, new_status, COUNT(*) AS approved
FROM T_AU_NEW_PARAS_STATUS_CHANGE_LOG
WHERE authorized_on IS NOT NULL
GROUP BY old_status, new_status
ORDER BY old_status, new_status;

-- 5. New cursor test: select TOP and the same organizational filters as UI.
-- Expected after revised package deployment:
-- HISTORICAL_OPENING_VERIFIED = E and CLOSING_ASOF_VERIFIED = E
-- All five previously blank numeric columns populated (including zero).
DECLARE
    c SYS_REFCURSOR;
BEGIN
    PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE(
        P_DASHBOARD_TYPE => 'TOP',
        P_ENTITY_ID => NULL,
        P_START_DATE => DATE '2024-01-01',
        P_END_DATE => DATE '2026-10-14',
        P_ENTITY_TYPE_ID => NULL,
        P_AUDITED_BY_ID => NULL,
        P_REPORTING_OFFICE_ID => NULL,
        IO_CURSOR => c
    );
    DBMS_SQL.RETURN_RESULT(c);
END;

-- 6. Today's End Date should instead return verified current-state Y
-- for closing; the opening period-start balance remains estimated E.
DECLARE
    c SYS_REFCURSOR;
BEGIN
    PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE(
        P_DASHBOARD_TYPE => 'TOP',
        P_ENTITY_ID => NULL,
        P_START_DATE => DATE '2024-01-01',
        P_END_DATE => TRUNC(SYSDATE),
        P_ENTITY_TYPE_ID => NULL,
        P_AUDITED_BY_ID => NULL,
        P_REPORTING_OFFICE_ID => NULL,
        IO_CURSOR => c
    );
    DBMS_SQL.RETURN_RESULT(c);
END;
