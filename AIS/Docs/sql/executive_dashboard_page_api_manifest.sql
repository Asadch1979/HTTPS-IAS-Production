-- IAS COMPLETE EXECUTIVE/HEAD-AUDIT GET API REGISTRATION + PAGE DEPENDENCY MANIFEST
-- DBA-reviewed deployment source. Oracle 18c+. Never run on app startup.
-- IMPORTANT: Run this single file using SQL Developer F5.
-- Approved missing API_MASTER GET routes are INSERTED; approved inactive GET routes
-- are ACTIVATED. Duplicates are reconciled only for exact approved route/method
-- with page links confined to the reviewed PAGE_ID mapping.
-- No ROLE_ID or T_AU_ROLE_API_PERMISSION changes: page assignment is the source of entitlement.
-- Run after existing API_MASTER and T_MENU_PAGES registrations; then compile PKG_LG, PKG_AD,
-- redeploy IAS and require users to log out/in to refresh cached API permissions.
-- Six verified pages supplied by management (menu 16) plus CURRENT RLM 1000305.
-- Existing table: (PAGE_ID, API_ID), with NO dependency IS_ACTIVE column.
-- Do not ALTER/DROP existing T_IAS_EXEC_PAGE_API to deploy this change.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

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
  TYPE t_route_record IS RECORD (
    controller_name VARCHAR2(100),
    action_name     VARCHAR2(100),
    anchor_page_id  NUMBER
  );
  TYPE t_routes IS TABLE OF t_route_record INDEX BY VARCHAR2(300);
  TYPE t_allowed IS TABLE OF PLS_INTEGER INDEX BY VARCHAR2(400);
  TYPE t_ids IS TABLE OF NUMBER INDEX BY VARCHAR2(300);

  routes           t_routes;
  approved         t_allowed;
  canonical_ids    t_ids;
  route_key        VARCHAR2(300);
  mapping_key      VARCHAR2(400);
  route_record     t_route_record;
  api_id           NUMBER;
  found_count      NUMBER;
  collision_count  NUMBER;
  mappings_added   NUMBER := 0;
  new_apis         NUMBER := 0;
  activated_apis   NUMBER := 0;
  retired_apis     NUMBER := 0;
  moved_links      NUMBER := 0;
  retired_path     VARCHAR2(300);
  key_sep          PLS_INTEGER;
  selected_page    NUMBER;
  selected_route   VARCHAR2(300);

  PROCEDURE verify_page(p_id NUMBER,p_path VARCHAR2,p_menu NUMBER DEFAULT 16) IS
  BEGIN
    SELECT COUNT(*) INTO found_count
      FROM T_MENU_PAGES
     WHERE ID=p_id
       AND LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH)))=LOWER(p_path)
       AND STATUS='A'
       AND (p_menu IS NULL OR MENU_ID=p_menu);
    IF found_count<>1 THEN
      RAISE_APPLICATION_ERROR(-20911,
        'Missing/inactive/wrong-menu page: '||p_id||' / '||p_path);
    END IF;
  END;

  -- Collect the exact allowlist first, then perform registration. No role grants.
  PROCEDURE link_get(p_id NUMBER,p_controller VARCHAR2,p_action VARCHAR2) IS
    l_key VARCHAR2(300):=LOWER('api/'||p_controller||'/'||p_action);
    l_route t_route_record;
  BEGIN
    IF NOT routes.EXISTS(l_key) THEN
      l_route.controller_name:=p_controller;
      l_route.action_name:=p_action;
      -- API_MASTER.PAGE_ID is only a legacy anchor. Effective authorizations
      -- are the approved per-page dependencies below.
      l_route.anchor_page_id:=p_id;
      routes(l_key):=l_route;
    END IF;
    approved(TO_CHAR(p_id)||'|'||l_key):=1;
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

  -- Fail fast if another permission-registration session is running.
  LOCK TABLE T_AU_API_MASTER IN EXCLUSIVE MODE NOWAIT;
  LOCK TABLE T_IAS_EXEC_PAGE_API IN EXCLUSIVE MODE NOWAIT;

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


  IF routes.COUNT<>37 OR approved.COUNT<>66 THEN
    RAISE_APPLICATION_ERROR(-20918,
      'Unexpected route manifest size (routes='||routes.COUNT||
      ', page links='||approved.COUNT||'); deployment stopped.');
  END IF;

  -- Refuse unknown pre-existing links for these seven pages. A route may
  -- legitimately have multiple API_IDs, but every linked page must be approved.
  FOR linked IN (
    SELECT d.PAGE_ID, d.API_ID, m.API_PATH, m.HTTP_METHOD
      FROM T_IAS_EXEC_PAGE_API d
      LEFT JOIN T_AU_API_MASTER m ON m.API_ID=d.API_ID
     WHERE d.PAGE_ID IN
       (1000300,1000301,1000302,1000303,1000304,1000305,1000401)
  ) LOOP
    IF linked.API_PATH IS NULL OR
       NVL(UPPER(TRIM(linked.HTTP_METHOD)),'#')<>'GET' OR
       NOT approved.EXISTS(TO_CHAR(linked.PAGE_ID)||'|'||
         LOWER(TRIM(BOTH '/' FROM TRIM(linked.API_PATH)))) THEN
      RAISE_APPLICATION_ERROR(-20914,
        'Unapproved existing page/API dependency. PAGE_ID='||
        linked.PAGE_ID||', API_ID='||linked.API_ID||
        ', path='||NVL(linked.API_PATH,'<missing>'));
    END IF;
  END LOOP;

  -- Process each approved GET path exactly once. New APIs are inserted;
  -- existing inactive APIs are activated, as expressly requested.
  route_key:=routes.FIRST;
  WHILE route_key IS NOT NULL LOOP
    route_record:=routes(route_key);

    SELECT COUNT(*) INTO found_count
      FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH)))=route_key
       AND UPPER(TRIM(HTTP_METHOD))='GET';

    IF found_count=0 THEN
      INSERT INTO T_AU_API_MASTER
        (ACTION_NAME,PAGE_ID,CONTROLLER_NAME,API_PATH,
         HTTP_METHOD,IS_ACTIVE,CREATED_ON)
      VALUES
        (route_record.action_name,route_record.anchor_page_id,
         route_record.controller_name,
         '/api/'||route_record.controller_name||'/'||route_record.action_name,
         'GET','Y',SYSDATE)
      RETURNING API_ID INTO api_id;
      new_apis:=new_apis+1;
      DBMS_OUTPUT.PUT_LINE('INSERT  '||route_key||'  API_ID='||api_id);
    ELSE
      -- Prefer an already ACTIVE record; otherwise retain the oldest API_ID.
      -- This preserves 2428 over the historical 2487 GetReportE duplicate.
      SELECT API_ID INTO api_id FROM (
        SELECT API_ID
          FROM T_AU_API_MASTER
         WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH)))=route_key
           AND UPPER(TRIM(HTTP_METHOD))='GET'
         ORDER BY CASE WHEN IS_ACTIVE='Y' THEN 0 ELSE 1 END, API_ID
      ) WHERE ROWNUM=1;

      UPDATE T_AU_API_MASTER
         SET IS_ACTIVE='Y', UPDATED_ON=SYSDATE
       WHERE API_ID=api_id AND NVL(IS_ACTIVE,'N')<>'Y';
      IF SQL%ROWCOUNT>0 THEN
        activated_apis:=activated_apis+1;
        DBMS_OUTPUT.PUT_LINE('ACTIVATE '||route_key||'  API_ID='||api_id);
      END IF;
    END IF;

    canonical_ids(route_key):=api_id;

    -- Reconcile duplicates WITHOUT deleting API_MASTER or role history.
    -- Any approved PAGE_ID dependency on a duplicate is migrated to the
    -- canonical API_ID first; other page assignments were rejected above.
    FOR duplicate_api IN (
      SELECT API_ID FROM T_AU_API_MASTER
       WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH)))=route_key
         AND UPPER(TRIM(HTTP_METHOD))='GET'
         AND API_ID<>api_id
       ORDER BY API_ID
    ) LOOP
      FOR old_link IN (
        SELECT PAGE_ID FROM T_IAS_EXEC_PAGE_API
         WHERE API_ID=duplicate_api.API_ID
      ) LOOP
        IF NOT approved.EXISTS(TO_CHAR(old_link.PAGE_ID)||'|'||route_key) THEN
          RAISE_APPLICATION_ERROR(-20917,
            'Duplicate API_ID linked outside approved pages: '||
            duplicate_api.API_ID||' / '||old_link.PAGE_ID);
        END IF;

        MERGE INTO T_IAS_EXEC_PAGE_API target
        USING (SELECT old_link.PAGE_ID PAGE_ID,api_id API_ID FROM dual) src
           ON (target.PAGE_ID=src.PAGE_ID AND target.API_ID=src.API_ID)
        WHEN NOT MATCHED THEN INSERT (PAGE_ID,API_ID)
          VALUES(src.PAGE_ID,src.API_ID);

        DELETE FROM T_IAS_EXEC_PAGE_API
         WHERE PAGE_ID=old_link.PAGE_ID AND API_ID=duplicate_api.API_ID;
        moved_links:=moved_links+1;
      END LOOP;

      retired_path:='/api/__retired_exec_duplicate_'||duplicate_api.API_ID;
      SELECT COUNT(*) INTO collision_count FROM T_AU_API_MASTER
       WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH)))=
             LOWER(TRIM(BOTH '/' FROM retired_path))
         AND API_ID<>duplicate_api.API_ID;
      IF collision_count<>0 THEN
        RAISE_APPLICATION_ERROR(-20919,
          'Archive API path collision for API_ID '||duplicate_api.API_ID);
      END IF;

      UPDATE T_AU_API_MASTER
         SET API_PATH=retired_path,
             IS_ACTIVE='N',
             UPDATED_ON=SYSDATE
       WHERE API_ID=duplicate_api.API_ID;
      retired_apis:=retired_apis+1;
      DBMS_OUTPUT.PUT_LINE('RETIRE '||route_key||
        '  duplicate='||duplicate_api.API_ID||
        '  retained='||api_id);
    END LOOP;

    SELECT COUNT(*) INTO found_count FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH)))=route_key
       AND UPPER(TRIM(HTTP_METHOD))='GET'
       AND IS_ACTIVE='Y';
    IF found_count<>1 THEN
      RAISE_APPLICATION_ERROR(-20912,
        'Exact route is not uniquely active after registration: '||route_key);
    END IF;

    route_key:=routes.NEXT(route_key);
  END LOOP;

  -- Link exactly the approved page-to-API pairs (not role-to-API grants).
  mapping_key:=approved.FIRST;
  WHILE mapping_key IS NOT NULL LOOP
    key_sep:=INSTR(mapping_key,'|');
    selected_page:=TO_NUMBER(SUBSTR(mapping_key,1,key_sep-1));
    selected_route:=SUBSTR(mapping_key,key_sep+1);

    MERGE INTO T_IAS_EXEC_PAGE_API target
    USING (SELECT selected_page PAGE_ID,
                  canonical_ids(selected_route) API_ID FROM dual) src
       ON (target.PAGE_ID=src.PAGE_ID AND target.API_ID=src.API_ID)
    WHEN NOT MATCHED THEN INSERT (PAGE_ID,API_ID)
      VALUES(src.PAGE_ID,src.API_ID);

    IF SQL%ROWCOUNT>0 THEN
      mappings_added:=mappings_added+1;
    END IF;
    mapping_key:=approved.NEXT(mapping_key);
  END LOOP;

  -- Fail closed if any unexpected extra link remains for a dashboard page.
  FOR linked IN (
    SELECT d.PAGE_ID,d.API_ID,m.API_PATH,m.HTTP_METHOD
      FROM T_IAS_EXEC_PAGE_API d
      JOIN T_AU_API_MASTER m ON m.API_ID=d.API_ID
     WHERE d.PAGE_ID IN
       (1000300,1000301,1000302,1000303,1000304,1000305,1000401)
  ) LOOP
    selected_route:=LOWER(TRIM(BOTH '/' FROM TRIM(linked.API_PATH)));
    IF NVL(UPPER(TRIM(linked.HTTP_METHOD)),'#')<>'GET' OR
       NOT approved.EXISTS(TO_CHAR(linked.PAGE_ID)||'|'||selected_route) OR
       canonical_ids(selected_route)<>linked.API_ID THEN
      RAISE_APPLICATION_ERROR(-20914,
        'Unexpected final page/API link: '||
        linked.PAGE_ID||' / '||linked.API_ID);
    END IF;
  END LOOP;

  COMMIT;
  DBMS_OUTPUT.PUT_LINE('SUCCESS: all '||routes.COUNT||
    ' approved GET routes and '||approved.COUNT||' page dependencies verified.');
  DBMS_OUTPUT.PUT_LINE('New APIs: '||new_apis||
    ' | Activated: '||activated_apis||
    ' | Retired duplicates: '||retired_apis||
    ' | Migrated duplicate page links: '||moved_links||
    ' | New page links: '||mappings_added);
  DBMS_OUTPUT.PUT_LINE(
    'No T_AU_ROLE_API_PERMISSION rows, page assignments or audit packages modified.');
EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK;
    DBMS_OUTPUT.PUT_LINE('ROLLBACK: '||SQLERRM);
    RAISE;
END;
/

-- Read-only result for post-run reconciliation.
SELECT d.PAGE_ID,p.PAGE_PATH,m.API_ID,m.HTTP_METHOD,m.API_PATH,
       m.IS_ACTIVE AS API_ACTIVE
  FROM T_IAS_EXEC_PAGE_API d
  JOIN T_MENU_PAGES p ON p.ID=d.PAGE_ID
  JOIN T_AU_API_MASTER m ON m.API_ID=d.API_ID
 WHERE d.PAGE_ID IN
       (1000300,1000301,1000302,1000303,1000304,1000305,1000401)
 ORDER BY d.PAGE_ID,m.API_PATH;
