-- IAS EXECUTIVE DASHBOARD PAGE -> API MANIFEST
-- DBA-reviewed deployment source. Oracle 18c+. Never run on app startup.
-- No ROLE_ID or T_AU_ROLE_API_PERMISSION changes: page assignment is the source of entitlement.
-- Run after existing API_MASTER and T_MENU_PAGES registrations; then compile PKG_LG, PKG_AD,
-- redeploy IAS and require users to log out/in to refresh cached API permissions.
-- Six verified pages supplied by management (menu 16) plus CURRENT RLM 1000305.
-- Existing table: (PAGE_ID, API_ID), with NO dependency IS_ACTIVE column.
-- Do not ALTER/DROP existing T_IAS_EXEC_PAGE_API to deploy this change.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON;

DECLARE
  v_exists NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_exists FROM USER_TABLES WHERE TABLE_NAME='T_IAS_EXEC_PAGE_API';
  IF v_exists=0 THEN
    EXECUTE IMMEDIATE 'CREATE TABLE T_IAS_EXEC_PAGE_API (
      PAGE_ID NUMBER NOT NULL,
      API_ID NUMBER NOT NULL,
      CONSTRAINT PK_IAS_EXEC_PAGE_API PRIMARY KEY (PAGE_ID,API_ID))';
    DBMS_OUTPUT.PUT_LINE('Created T_IAS_EXEC_PAGE_API (PAGE_ID, API_ID only).');
  END IF;
  SELECT COUNT(*) INTO v_exists FROM USER_TAB_COLUMNS
   WHERE TABLE_NAME='T_IAS_EXEC_PAGE_API'
     AND COLUMN_NAME IN ('PAGE_ID','API_ID')
     AND DATA_TYPE='NUMBER';
  IF v_exists<>2 THEN
    RAISE_APPLICATION_ERROR(-20910,
      'Expected numeric PAGE_ID and API_ID in existing T_IAS_EXEC_PAGE_API; inspect the table.');
  END IF;
  -- This IAS table has two-column dependency semantics. If a different
  -- installation has a status column, do not ignore explicit deny decisions.
  SELECT COUNT(*) INTO v_exists FROM USER_TAB_COLUMNS
   WHERE TABLE_NAME='T_IAS_EXEC_PAGE_API' AND COLUMN_NAME='IS_ACTIVE';
  IF v_exists<>0 THEN
    RAISE_APPLICATION_ERROR(-20915,
      'Status-enabled dependency schema requires separate reviewed migration.');
  END IF;
  -- Prevent duplicate (PAGE_ID,API_ID) dependencies even where old
  -- installations lack the expected composite unique constraint.
  SELECT COUNT(*) INTO v_exists FROM (
    SELECT PAGE_ID,API_ID FROM T_IAS_EXEC_PAGE_API
     GROUP BY PAGE_ID,API_ID HAVING COUNT(*)>1);
  IF v_exists>0 THEN
    RAISE_APPLICATION_ERROR(-20916,
      'Duplicate existing PAGE_ID/API_ID dependencies; inspect before deployment.');
  END IF;
END;
/

DECLARE
  v_count NUMBER;
  v_api_id NUMBER;
  v_active VARCHAR2(1);
  v_rows NUMBER:=0;
  TYPE expected_map IS TABLE OF PLS_INTEGER INDEX BY VARCHAR2(100);
  approved expected_map;
  PROCEDURE verify_page(p_id NUMBER,p_path VARCHAR2,p_menu NUMBER DEFAULT 16) IS
  BEGIN
    SELECT COUNT(*) INTO v_count FROM T_MENU_PAGES
     WHERE ID=p_id
       AND LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH)))=LOWER(p_path)
       AND STATUS='A'
       AND (p_menu IS NULL OR MENU_ID=p_menu);
    IF v_count<>1 THEN RAISE_APPLICATION_ERROR(-20911,
       'Missing/inactive/wrong-menu page: '||p_id||' '||p_path); END IF;
  END;
  PROCEDURE link_get(p_id NUMBER,p_controller VARCHAR2,p_action VARCHAR2) IS
    v_path VARCHAR2(300):='api/'||p_controller||'/'||p_action;
  BEGIN
    SELECT COUNT(*), MIN(API_ID), MAX(IS_ACTIVE)
      INTO v_count,v_api_id,v_active FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH)))=LOWER(v_path)
       AND UPPER(TRIM(HTTP_METHOD))='GET';
    IF v_count<>1 OR NVL(v_active,'N')<>'Y' THEN
      RAISE_APPLICATION_ERROR(-20912,'One ACTIVE registered GET API required: '||v_path);
    END IF;
    MERGE INTO T_IAS_EXEC_PAGE_API d
    USING (SELECT p_id PAGE_ID,v_api_id API_ID FROM dual) v
       ON (d.PAGE_ID=v.PAGE_ID AND d.API_ID=v.API_ID)
    -- The existing mapping has no IS_ACTIVE column. Membership is approval;
    -- master API IS_ACTIVE and page STATUS remain mandatory.
    WHEN NOT MATCHED THEN INSERT (PAGE_ID,API_ID)
       VALUES(v.PAGE_ID,v.API_ID);
    approved(TO_CHAR(p_id)||':'||TO_CHAR(v_api_id)):=1;
    v_rows:=v_rows+1;
  END;
  PROCEDURE shared(p_id NUMBER) IS
  BEGIN
    link_get(p_id,'ExecutiveDashboard','GetKpiSummary');
    link_get(p_id,'ExecutiveDashboard','GetRiskSummary');
    link_get(p_id,'ExecutiveDashboard','GetAgingSummary');
    link_get(p_id,'ExecutiveDashboard','GetCycleSummary');
    link_get(p_id,'ExecutiveDashboard','GetSettlementTrend');
    link_get(p_id,'ExecutiveDashboard','GetCriticalItems');
  END;
  PROCEDURE reports(p_id NUMBER,p_ids VARCHAR2) IS
  BEGIN
    IF INSTR(p_ids,'A')>0 THEN link_get(p_id,'ExecutiveReports','GetReportA'); END IF;
    IF INSTR(p_ids,'B')>0 THEN link_get(p_id,'ExecutiveReports','GetReportB'); END IF;
    IF INSTR(p_ids,'C')>0 THEN link_get(p_id,'ExecutiveReports','GetReportC'); END IF;
    IF INSTR(p_ids,'D')>0 THEN link_get(p_id,'ExecutiveReports','GetReportD'); END IF;
    IF INSTR(p_ids,'E')>0 THEN link_get(p_id,'ExecutiveReports','GetReportE'); END IF;
  END;
BEGIN
  -- Named pages are an exact contract: never guess/reassign PAGE_ID values.
  verify_page(1000401,'HeadAuditDepartment/AuditTeamsMonitoring');
  verify_page(1000304,'ExecutiveOverview/AuditParaSnap');
  verify_page(1000303,'ExecutiveOverview/CIA');
  verify_page(1000302,'ExecutiveOverview/CCO');
  verify_page(1000301,'ExecutiveOverview/GROUP');
  verify_page(1000300,'ExecutiveOverview/TOP');
  -- RLM currently has a distinct authenticated page (1000305). It is not
  -- present in the six-row list: preserve it, do NOT alias GROUP (1000301).
  verify_page(1000305,'ExecutiveOverview/RLM',NULL);

  LOCK TABLE T_IAS_EXEC_PAGE_API IN EXCLUSIVE MODE;

  -- TOP: lifecycle + scoped views, no unauthorized report APIs.
  shared(1000300);
  link_get(1000300,'ExecutiveDashboard','GetKpiLifecycle');
  -- GROUP: Management Audit, Group charts, followups, CCM Report E only.
  shared(1000301);
  link_get(1000301,'ExecutiveDashboard','GetRoleUnitComparison');
  link_get(1000301,'ExecutiveDashboard','GetRoleFollowups');
  reports(1000301,'E');
  -- CCO: scoped compliance charts, Audit Para's, function, report B/C/D.
  shared(1000302);
  link_get(1000302,'CcoCompliance','GetFunctionPosition');
  link_get(1000302,'CcoCompliance','GetOrgFilters');
  link_get(1000302,'CcoCompliance','GetOrgPosition');
  link_get(1000302,'CcoCompliance','GetOrgSettlement');
  link_get(1000302,'CcoCompliance','GetOrgExceptions');
  reports(1000302,'BCD');
  -- CIA Command Center + authorized CIA audit-para reader/reports.
  shared(1000303);
  link_get(1000303,'ExecutiveDashboard','GetCiaCommandKpis');
  link_get(1000303,'ExecutiveDashboard','GetCiaIssueNature');
  reports(1000303,'ABCDE');
  -- Audit Para Snap is independent of TOP/CIA in page permissions.
  link_get(1000304,'AuditParaSnap','GetKpis');
  link_get(1000304,'AuditParaSnap','GetDistribution');
  link_get(1000304,'AuditParaSnap','GetCriticalItems');
  link_get(1000304,'AuditParaSnap','GetPerformance');
  link_get(1000304,'AuditParaSnap','GetParaText');
  FOR x IN (SELECT 'GetReportA' action_name FROM dual UNION ALL
            SELECT 'GetReportB' FROM dual UNION ALL
            SELECT 'GetReportC' FROM dual UNION ALL
            SELECT 'GetReportD' FROM dual UNION ALL
            SELECT 'GetReportE' FROM dual) LOOP
    link_get(1000304,'AuditParaSnap',x.action_name);
  END LOOP;
  -- Head of Audit monitoring GET endpoints: no executive page inheritance.
  link_get(1000401,'HeadAuditDepartment','Engagements');
  link_get(1000401,'HeadAuditDepartment','Work');
  link_get(1000401,'HeadAuditDepartment','Evidence');
  link_get(1000401,'HeadAuditDepartment','Report');
  link_get(1000401,'HeadAuditDepartment','ResponseEvidence');
  -- RLM: existing page 1000305, including GM/Region Pulse role APIs.
  shared(1000305);
  link_get(1000305,'ExecutiveDashboard','GetRoleEntityHeatmap');
  link_get(1000305,'ExecutiveDashboard','GetRoleFollowups');

  -- Any existing dependency outside this exact reviewed list would
  -- bypass the approval boundary. Reject it; do not silently delete it.
  FOR unexpected IN (
    SELECT PAGE_ID,API_ID FROM T_IAS_EXEC_PAGE_API
     WHERE PAGE_ID IN (1000300,1000301,1000302,1000303,1000304,1000305,1000401)
  ) LOOP
    IF NOT approved.EXISTS(TO_CHAR(unexpected.PAGE_ID)||':'||TO_CHAR(unexpected.API_ID)) THEN
      RAISE_APPLICATION_ERROR(-20914,
        'Unexpected page/API dependency; review and reconcile: '||
        unexpected.PAGE_ID||'/'||unexpected.API_ID);
    END IF;
  END LOOP;
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('PAGE->API dependencies verified: '||v_rows);
  DBMS_OUTPUT.PUT_LINE('No page assignments, API_MASTER status, role grants or Oracle package data changed.');
EXCEPTION WHEN OTHERS THEN
  ROLLBACK;
  RAISE;
END;
/
-- Independent read-only reconciliation; run before enabling traffic:
SELECT d.PAGE_ID,p.PAGE_PATH,m.HTTP_METHOD,m.API_PATH,m.IS_ACTIVE AS API_ACTIVE
  FROM T_IAS_EXEC_PAGE_API d
  JOIN T_MENU_PAGES p ON p.ID=d.PAGE_ID
  JOIN T_AU_API_MASTER m ON m.API_ID=d.API_ID
 WHERE d.PAGE_ID IN (1000300,1000301,1000302,1000303,1000304,1000305,1000401)
 ORDER BY d.PAGE_ID,m.API_PATH;
