-- CIA Audit Command Center: read-only Oracle 18c verification.
-- Run each statement or anonymous block separately using SQL Developer F8.
-- Deploy T_IAS_CIA_ISSUE_NATURE_MAP.sql BEFORE updated PKG_EXEC_DASHBOARD.
-- No DML, commits, or migration of existing data in this file.

-- 1. Confirm package, body, mapping table and dependencies are VALID.
SELECT object_name, object_type, status
  FROM user_objects
 WHERE object_name IN
       ('PKG_EXEC_DASHBOARD', 'T_IAS_CIA_ISSUE_NATURE_MAP',
        'V_EXEC_KPI_LIFECYCLE', 'V_EXEC_ENTITY_SCOPE')
 ORDER BY object_name, object_type;

-- 2. Must return no rows.
SELECT type, line, position, text
  FROM user_errors
 WHERE name = 'PKG_EXEC_DASHBOARD'
 ORDER BY type, sequence;

-- 3. Published CIA procedure arguments; inspect signatures.
SELECT object_name, argument_name, position, data_type, in_out
  FROM user_arguments
 WHERE package_name = 'PKG_EXEC_DASHBOARD'
   AND object_name IN
       ('P_GET_CIA_EXEC_KPIS', 'P_GET_CIA_ISSUE_NATURE')
 ORDER BY object_name, sequence;

-- 4. Find a valid audit period ID for tests.
SELECT p.auditperiodid, p.description, p.start_date, p.end_date,
       p.status_id
  FROM t_au_period p
 ORDER BY p.start_date DESC;

-- 5. Verify the A1 annexure ID assumption; check CODE and HEADING.
SELECT id, code, heading
  FROM t_audit_checklist_annexure
 WHERE id = 1 OR UPPER(REPLACE(TRIM(code), '-', '')) = 'A1'
 ORDER BY id;

-- 6. Confirm actual status-2 population by audit period.
SELECT e.period_id AS audit_period_id,
       COUNT(DISTINCT o.id) AS awaiting_auditee_response
  FROM t_au_observation o
  JOIN t_au_plan_eng e ON e.eng_id = o.engplanid
 WHERE o.status = 2
 GROUP BY e.period_id
 ORDER BY e.period_id DESC;

-- 7. Review source mappings before publishing chart.
SELECT source_type, source_id, category_code,
       match_priority, is_active
  FROM t_ias_cia_issue_nature_map
 ORDER BY match_priority, source_type, source_id;

-- 8. Expected: application error -20041. Execute as one PL/SQL block.
DECLARE
  v_cursor SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_EXEC_KPIS(
    P_DASHBOARD_TYPE => 'TOP',
    P_ENTITY_ID => NULL,
    P_AUDIT_PERIOD_ID => 1,
    IO_CURSOR => v_cursor);
  RAISE_APPLICATION_ERROR(-20991,
    'FAIL: non-CIA call should have been rejected.');
EXCEPTION
  WHEN OTHERS THEN
    IF SQLCODE = -20041 THEN
      DBMS_OUTPUT.PUT_LINE('PASS: non-CIA perspective rejected.');
    ELSE
      RAISE;
    END IF;
END;

-- 9. Run with actual :P_AUDIT_PERIOD_ID from query 4.
-- CIA backend must independently verify authenticated role/scope.
DECLARE
  v_cursor SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_EXEC_KPIS(
    P_DASHBOARD_TYPE => 'CIA',
    P_ENTITY_ID => NULL,
    P_AUDIT_PERIOD_ID => :P_AUDIT_PERIOD_ID,
    IO_CURSOR => v_cursor);
  DBMS_SQL.RETURN_RESULT(v_cursor);
END;

-- 10. Run with the SAME :P_AUDIT_PERIOD_ID.
-- Verify 10 rows, and mapped + unmapped = TOTAL_ISSUES.
DECLARE
  v_cursor SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE(
    P_DASHBOARD_TYPE => 'CIA',
    P_ENTITY_ID => NULL,
    P_AUDIT_PERIOD_ID => :P_AUDIT_PERIOD_ID,
    IO_CURSOR => v_cursor);
  DBMS_SQL.RETURN_RESULT(v_cursor);
END;

-- 11. Check source category masters BEFORE adding verified ID mappings.
SELECT id, sub_v_name
  FROM t_control_violation_sub
 ORDER BY id;

-- 12. Validate scope: rerun 9 and 10 with the same allowed
-- P_ENTITY_ID / P_ENTITY_TYPE_ID / P_AUDITED_BY_ID /
-- P_REPORTING_OFFICE_ID inputs as the existing CIA dashboard.
-- Narrowed totals must not exceed authorized bank-wide totals.
-- Reconcile issue counts with P_GET_CIA_ISSUES on the same dates,
-- and independently check the 15-day delay and active-stage rules.
