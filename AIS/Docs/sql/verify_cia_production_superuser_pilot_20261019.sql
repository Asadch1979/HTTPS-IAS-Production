-- CIA production verification for PL/SQL Developer SQL Window F8.
-- READ ONLY. No DML, DDL, COMMIT or user/role permission changes.
-- IMPORTANT: Run EACH SELECT or DECLARE...END; block separately using F8.
-- Do NOT execute the whole file as one statement; do NOT append '/'.
-- In PL/SQL Developer enable DBMS Output pane for the two procedure blocks.
-- Audit period 5 is the user-supplied period example. Change literal 5
-- consistently below after confirming T_AU_PERIOD.AUDITPERIODID.
--
-- 1. Verify selected IAS audit period (select and press F8).
SELECT p.auditperiodid, p.description, p.start_date, p.end_date
  FROM t_au_period p
 WHERE p.auditperiodid = 5;

-- 2. Check the database objects and their compilation status (F8).
SELECT object_name, object_type, status
  FROM user_objects
 WHERE object_name IN
       ('PKG_EXEC_DASHBOARD', 'V_EXEC_KPI_LIFECYCLE',
        'V_IAS_CIA_ISSUE_CHECKLIST_ROOT', 'T_IAS_CIA_ISSUE_NATURE_MAP')
 ORDER BY object_name, object_type;

-- 3. Review CIA API registration and actual grants (F8).
-- Only active Super User role 1 grants should remain for this pilot.
SELECT a.api_id, a.action_name, a.is_active AS api_active,
       p.role_id, p.is_active AS grant_active
  FROM t_au_api_master a
  LEFT JOIN t_au_role_api_permission p ON p.api_id = a.api_id
 WHERE UPPER(a.controller_name) = 'EXECUTIVEDASHBOARD'
   AND UPPER(a.http_method) = 'GET'
   AND UPPER(a.action_name) IN
       ('GETCIACOMMANDKPIS', 'GETCIAISSUENATURE')
 ORDER BY a.action_name, p.role_id;

-- 4. Review verified checklist mappings (F8).
SELECT source_type, source_id, category_code, match_priority, is_active
  FROM t_ias_cia_issue_nature_map
 WHERE source_type = 'CHECKLIST_ROOT'
 ORDER BY source_id;

-- 5. CIA five KPIs. Select the entire DECLARE...END; block, press F8.
-- In PL/SQL Developer use DBMS Output pane (enabled) to view results.
-- The 11 FETCH values follow the EXACT order returned by
-- PKG_EXEC_DASHBOARD.P_GET_CIA_EXEC_KPIS.
-- Oracle NULL prints N/A; Oracle zero prints 0.
DECLARE
  v_rc SYS_REFCURSOR;
  v_planned NUMBER;
  v_audited NUMBER;
  v_coverage NUMBER;
  v_delayed NUMBER;
  v_fraud NUMBER;
  v_awaiting NUMBER;
  v_teams NUMBER;
  v_period NUMBER;
  v_from DATE;
  v_to DATE;
  v_asof DATE;
  FUNCTION fmt(p_value NUMBER) RETURN VARCHAR2 IS
  BEGIN
    RETURN CASE WHEN p_value IS NULL THEN 'N/A' ELSE TO_CHAR(p_value) END;
  END;
  FUNCTION fmt_date(p_value DATE) RETURN VARCHAR2 IS
  BEGIN
    RETURN CASE WHEN p_value IS NULL THEN 'N/A'
                ELSE TO_CHAR(p_value, 'YYYY-MM-DD') END;
  END;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_EXEC_KPIS(
      P_DASHBOARD_TYPE => 'CIA',
      P_ENTITY_ID => NULL,
      P_AUDIT_PERIOD_ID => 5,
      IO_CURSOR => v_rc);

  FETCH v_rc INTO v_planned, v_audited, v_coverage,
       v_delayed, v_fraud, v_awaiting, v_teams,
       v_period, v_from, v_to, v_asof;

  IF v_rc%NOTFOUND THEN
    CLOSE v_rc;
    RAISE_APPLICATION_ERROR(-20761,
      'P_GET_CIA_EXEC_KPIS did not return a result row.');
  END IF;
  CLOSE v_rc;

  DBMS_OUTPUT.PUT_LINE('--- CIA COMMAND CENTER: Actual Oracle values ---');
  DBMS_OUTPUT.PUT_LINE('Audit period ID: ' || fmt(v_period));
  DBMS_OUTPUT.PUT_LINE('Period from/to: ' || fmt_date(v_from) ||
                       ' / ' || fmt_date(v_to));
  DBMS_OUTPUT.PUT_LINE('Operational as of: ' || fmt_date(v_asof));
  DBMS_OUTPUT.PUT_LINE('Planned entities: ' || fmt(v_planned));
  DBMS_OUTPUT.PUT_LINE('Audited entities: ' || fmt(v_audited));
  DBMS_OUTPUT.PUT_LINE('Plan coverage (%): ' || fmt(v_coverage));
  DBMS_OUTPUT.PUT_LINE('Delayed audit reports: ' || fmt(v_delayed));
  DBMS_OUTPUT.PUT_LINE('A1 observations: ' || fmt(v_fraud));
  DBMS_OUTPUT.PUT_LINE('Awaiting auditee response: ' || fmt(v_awaiting));
  DBMS_OUTPUT.PUT_LINE('Teams conducting audit: ' || fmt(v_teams));
END;

-- 6. Nature of Issue: ten categories. Run this block alone with F8.
-- Check sum of issue counts + unmapped = total. This block prints
-- raw Oracle numbers; it does NOT modify category mappings.
DECLARE
  v_rc SYS_REFCURSOR;
  v_code VARCHAR2(100);
  v_name VARCHAR2(300);
  v_order NUMBER;
  v_count NUMBER;
  v_total NUMBER;
  v_unmapped NUMBER;
  v_status VARCHAR2(50);
  v_rows PLS_INTEGER := 0;
  v_sum NUMBER := 0;
  v_expected_total NUMBER;
  v_expected_unmapped NUMBER;
  FUNCTION fmt(p_value NUMBER) RETURN VARCHAR2 IS
  BEGIN
    RETURN CASE WHEN p_value IS NULL THEN 'N/A'
                ELSE TO_CHAR(p_value) END;
  END;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE(
      P_DASHBOARD_TYPE => 'CIA',
      P_ENTITY_ID => NULL,
      P_AUDIT_PERIOD_ID => 5,
      IO_CURSOR => v_rc);

  DBMS_OUTPUT.PUT_LINE('--- CIA NATURE OF ISSUE: Audit period 5 ---');
  LOOP
    FETCH v_rc INTO v_code, v_name, v_order, v_count,
                    v_total, v_unmapped, v_status;
    EXIT WHEN v_rc%NOTFOUND;

    v_rows := v_rows + 1;
    IF v_rows = 1 THEN
      v_expected_total := v_total;
      v_expected_unmapped := v_unmapped;
    ELSIF (v_total IS NULL OR v_unmapped IS NULL
           OR v_total <> v_expected_total
           OR v_unmapped <> v_expected_unmapped) THEN
      CLOSE v_rc;
      RAISE_APPLICATION_ERROR(-20762,
        'Nature of Issue cursor has inconsistent totals.');
    END IF;

    IF v_count IS NULL THEN
      CLOSE v_rc;
      RAISE_APPLICATION_ERROR(-20763,
        'Nature of Issue cursor has a NULL category count.');
    END IF;
    v_sum := v_sum + v_count;

    DBMS_OUTPUT.PUT_LINE(fmt(v_order) || '. ' || v_code ||
      ' | ' || v_name || ' | ' || fmt(v_count) ||
      ' | ' || v_status);
  END LOOP;
  CLOSE v_rc;

  DBMS_OUTPUT.PUT_LINE('Rows: ' || v_rows);
  DBMS_OUTPUT.PUT_LINE('Mapped issues: ' || fmt(v_sum));
  DBMS_OUTPUT.PUT_LINE('Unmapped issues: ' || fmt(v_expected_unmapped));
  DBMS_OUTPUT.PUT_LINE('Total issues: ' || fmt(v_expected_total));

  IF v_rows <> 10 THEN
    RAISE_APPLICATION_ERROR(-20764,
      'Expected exactly 10 CIA Nature of Issue categories.');
  ELSIF v_expected_total IS NULL OR v_expected_unmapped IS NULL
     OR v_sum + v_expected_unmapped <> v_expected_total THEN
    RAISE_APPLICATION_ERROR(-20765,
      'Nature of Issue category counts do not reconcile.');
  ELSE
    DBMS_OUTPUT.PUT_LINE('Reconciliation: PASS');
  END IF;
END;

-- END. All statements above are read-only.
