-- IAS executive dashboard page/API registration (separate parent Razor views).
-- PR #321 SQL lifecycle/view fixes and PR #322 parent routes are already on main.
--
-- SOURCE-ONLY, MANUAL DBA REVIEW. Do not run automatically on application startup.
-- Existing page STATUS/HIDE_MENU and API/role IS_ACTIVE values are preserved.
-- New API records and new API-role grants default INACTIVE (N).
-- No production permission changes are authorized by this source commit.
--
-- Independent parent routes:
--   TOP 1000300, GROUP 1000301, CCO 1000302, CIA 1000303,
--   AuditParaSnap 1000304.
-- RLM has its own dedicated 1000305 /ExecutiveOverview/RLM page.
-- Organization Structure retains its independent legacy page permission.
-- Install executive_dashboard_page_api_manifest.sql before updating PKG_AD
-- to allow front-end page assignments to grant shared GET APIs.
-- Common executive GET API paths must be registered ONCE; PAGE_ID=1000300
-- is their legacy registration anchor, NOT permission to access TOP. MVC
-- checks authenticated perspective/page and the independent API role grant.
--
-- Verified fixed IAS role/group IDs: 1 Super User, 3 CIA, 38 CCO.
-- Configure optional roles only after checking real IAS roles/group IDs and
-- ExecutiveDashboard:RoleMappings / RoleDashboards:ManagementRlmRoleIds.
-- Do NOT guess Group, Top, GM, Regional or Division role IDs.
--
-- Execute as a whole SQL Developer F5/SQL*Plus script AFTER review.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON;
DECLARE
  V_TOP_ROLE NUMBER := NULL;                -- verified TOP role/group, optional
  V_GROUP_ROLE NUMBER := NULL;              -- verified GROUP role/group, optional
  V_RLM_REGIONAL_ROLES SYS.ODCINUMBERLIST := SYS.ODCINUMBERLIST();
  V_RLM_MANAGEMENT_ROLES SYS.ODCINUMBERLIST := SYS.ODCINUMBERLIST();
  V_NEW_API_ACTIVE CHAR(1) := 'N';
  V_NEW_GRANT_ACTIVE CHAR(1) := 'N';
  V_MENU_ID NUMBER;
  V_FOUND NUMBER;
  V_CONFLICTS NUMBER;
  V_API_ID NUMBER;
  V_OLD_PAGE NUMBER;

  PROCEDURE CHECK_OPTIONAL_ROLE(P_ROLE NUMBER, P_LABEL VARCHAR2) IS
  BEGIN
    IF P_ROLE IS NOT NULL AND
       (P_ROLE <= 0 OR P_ROLE <> TRUNC(P_ROLE) OR P_ROLE IN (1,3,38)) THEN
      RAISE_APPLICATION_ERROR(-20081,'Invalid unverified IAS role for '||P_LABEL);
    END IF;
  END;

  PROCEDURE CHECK_OPTIONAL_LIST(
      P_ROLES SYS.ODCINUMBERLIST, P_LABEL VARCHAR2) IS
  BEGIN
    FOR i IN 1..P_ROLES.COUNT LOOP
      IF P_ROLES(i) IS NULL THEN
        RAISE_APPLICATION_ERROR(-20081,'Null role in '||P_LABEL);
      END IF;
      CHECK_OPTIONAL_ROLE(P_ROLES(i),P_LABEL);
    END LOOP;
  END;

  PROCEDURE GRANT_PAGE(P_PAGE_ID NUMBER, P_GROUP_ID NUMBER) IS
  BEGIN
    IF P_GROUP_ID IS NULL THEN RETURN; END IF;
    INSERT INTO T_MENU_PAGES_GROUPMAP (GROUPMAP_ID,GROUP_ID,PAGE_ID)
    SELECT (SELECT NVL(MAX(GROUPMAP_ID),0)+1 FROM T_MENU_PAGES_GROUPMAP),
           P_GROUP_ID,P_PAGE_ID
      FROM dual
     WHERE NOT EXISTS
       (SELECT 1 FROM T_MENU_PAGES_GROUPMAP
         WHERE GROUP_ID=P_GROUP_ID AND PAGE_ID=P_PAGE_ID);
  END;

  PROCEDURE REGISTER_PAGE(
      P_PAGE_ID NUMBER,P_SUFFIX VARCHAR2,P_NAME VARCHAR2,
      P_ROLE_ID NUMBER DEFAULT NULL) IS
    V_PATH VARCHAR2(150) := 'ExecutiveOverview/'||P_SUFFIX;
    V_KEY VARCHAR2(150) := 'EXECUTIVE_'||P_SUFFIX;
  BEGIN
    SELECT COUNT(*) INTO V_CONFLICTS
      FROM T_MENU_PAGES
     WHERE (ID=P_PAGE_ID AND
             (NVL(PAGE_KEY,'#')<>V_KEY
              OR NVL(LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH))),'#')
                 <>LOWER(V_PATH)))
        OR (ID<>P_PAGE_ID AND
            (LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH)))=LOWER(V_PATH)
             OR PAGE_KEY=V_KEY));
    IF V_CONFLICTS>0 THEN
      RAISE_APPLICATION_ERROR(-20082,'Executive page key/path/ID collision: '||V_PATH);
    END IF;

    SELECT COUNT(*) INTO V_FOUND FROM T_MENU_PAGES WHERE ID=P_PAGE_ID;
    IF V_FOUND=0 THEN
      INSERT INTO T_MENU_PAGES
        (ID,MENU_ID,PAGE_NAME,PAGE_PATH,PAGE_ORDER,
         STATUS,HIDE_MENU,PAGE_KEY,PAGE_URL)
      VALUES
        (P_PAGE_ID,V_MENU_ID,P_NAME,V_PATH,80+P_PAGE_ID-1000300,
         'A',0,V_KEY,'/'||V_PATH);
    END IF;
    -- Existing status/visibility/name/URL are not rewritten by re-registration.
    GRANT_PAGE(P_PAGE_ID,1);
    GRANT_PAGE(P_PAGE_ID,P_ROLE_ID);
    DBMS_OUTPUT.PUT_LINE('Validated page: '||V_PATH||' ['||P_PAGE_ID||']');
  END;

  PROCEDURE GRANT_API(P_ROLE NUMBER,P_API NUMBER) IS
  BEGIN
    IF P_ROLE IS NULL THEN RETURN; END IF;
    MERGE INTO T_AU_ROLE_API_PERMISSION t
    USING (SELECT P_ROLE role_id,P_API api_id FROM dual) s
       ON (t.ROLE_ID=s.role_id AND t.API_ID=s.api_id)
    -- CRITICAL: do not change existing IS_ACTIVE decisions on rerun.
    WHEN NOT MATCHED THEN INSERT
       (ROLE_ID,API_ID,IS_ACTIVE,CREATED_BY)
    VALUES (s.role_id,s.api_id,V_NEW_GRANT_ACTIVE,NULL);
  END;

  PROCEDURE REGISTER_GET(
      P_CONTROLLER VARCHAR2,P_ACTION VARCHAR2,
      P_PAGE_ID NUMBER,P_CLASS VARCHAR2) IS
    V_PATH VARCHAR2(300) :=
      '/api/'||P_CONTROLLER||'/'||P_ACTION;
  BEGIN
    SELECT COUNT(*),MIN(API_ID) INTO V_FOUND,V_API_ID
      FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH))) =
           LOWER(TRIM(BOTH '/' FROM V_PATH))
       AND UPPER(TRIM(HTTP_METHOD))='GET';
    IF V_FOUND>1 THEN
      RAISE_APPLICATION_ERROR(-20083,'Duplicate GET API registrations: '||V_PATH);
    ELSIF V_FOUND=0 THEN
      INSERT INTO T_AU_API_MASTER
        (ACTION_NAME,PAGE_ID,CONTROLLER_NAME,API_PATH,
         HTTP_METHOD,IS_ACTIVE,CREATED_ON)
      VALUES
        (P_ACTION,P_PAGE_ID,P_CONTROLLER,V_PATH,
         'GET',V_NEW_API_ACTIVE,SYSDATE);
      SELECT API_ID INTO V_API_ID FROM T_AU_API_MASTER
       WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH))) =
             LOWER(TRIM(BOTH '/' FROM V_PATH))
         AND UPPER(TRIM(HTTP_METHOD))='GET';
    ELSE
      SELECT PAGE_ID INTO V_OLD_PAGE
        FROM T_AU_API_MASTER WHERE API_ID=V_API_ID;
      SELECT COUNT(*) INTO V_CONFLICTS
        FROM T_AU_API_MASTER
       WHERE API_ID=V_API_ID
         AND (UPPER(NVL(CONTROLLER_NAME,'#'))<>UPPER(P_CONTROLLER)
              OR UPPER(NVL(ACTION_NAME,'#'))<>UPPER(P_ACTION)
              OR (NVL(PAGE_ID,-1)<>P_PAGE_ID
                  AND NOT(P_CLASS='CIA'
                          AND PAGE_ID=1000300 AND P_PAGE_ID=1000303)));
      IF V_CONFLICTS>0 THEN
        RAISE_APPLICATION_ERROR(-20084,'Existing API metadata conflict: '||V_PATH);
      END IF;
      -- Correct only the legacy TOP-page mapping of CIA command actions.
      IF P_CLASS='CIA' AND V_OLD_PAGE=1000300 THEN
        UPDATE T_AU_API_MASTER SET PAGE_ID=1000303
         WHERE API_ID=V_API_ID;
      END IF;
    END IF;

    IF P_CLASS='CIA' OR P_CLASS='SNAP' THEN
      -- These expose authorized bank-wide CIA data. Fail closed if any
      -- other IAS role currently has an ACTIVE grant; no silent removal.
      SELECT COUNT(*) INTO V_CONFLICTS
        FROM T_AU_ROLE_API_PERMISSION
       WHERE API_ID=V_API_ID AND ROLE_ID NOT IN (1,3)
         AND NVL(IS_ACTIVE,'N')='Y';
      IF V_CONFLICTS>0 THEN
        RAISE_APPLICATION_ERROR(-20085,
          'Unexpected active bank-wide CIA/Snap API role: '||V_PATH);
      END IF;
    END IF;

    GRANT_API(1,V_API_ID);
    IF P_CLASS='COMMON' THEN
      GRANT_API(3,V_API_ID);
      GRANT_API(38,V_API_ID);
      GRANT_API(V_TOP_ROLE,V_API_ID);
      GRANT_API(V_GROUP_ROLE,V_API_ID);
      FOR i IN 1..V_RLM_REGIONAL_ROLES.COUNT LOOP
        GRANT_API(V_RLM_REGIONAL_ROLES(i),V_API_ID);
      END LOOP;
      FOR i IN 1..V_RLM_MANAGEMENT_ROLES.COUNT LOOP
        GRANT_API(V_RLM_MANAGEMENT_ROLES(i),V_API_ID);
      END LOOP;
    ELSIF P_CLASS='TOP' THEN
      GRANT_API(V_TOP_ROLE,V_API_ID);
    ELSIF P_CLASS='CIA' OR P_CLASS='SNAP' THEN
      GRANT_API(3,V_API_ID);
    ELSIF P_CLASS='REGIONAL' THEN
      FOR i IN 1..V_RLM_REGIONAL_ROLES.COUNT LOOP
        GRANT_API(V_RLM_REGIONAL_ROLES(i),V_API_ID);
      END LOOP;
    ELSIF P_CLASS='MANAGEMENT' THEN
      GRANT_API(V_GROUP_ROLE,V_API_ID);
      FOR i IN 1..V_RLM_MANAGEMENT_ROLES.COUNT LOOP
        GRANT_API(V_RLM_MANAGEMENT_ROLES(i),V_API_ID);
      END LOOP;
    ELSE
      RAISE_APPLICATION_ERROR(-20086,'Unknown API class: '||P_CLASS);
    END IF;
    DBMS_OUTPUT.PUT_LINE('Validated GET '||V_PATH||' / class '||P_CLASS);
  END;
BEGIN
  -- Keep all idempotent MAX+1 allocations serialized in this transaction.
  LOCK TABLE T_MENU_PAGES IN EXCLUSIVE MODE;
  LOCK TABLE T_MENU_PAGES_GROUPMAP IN EXCLUSIVE MODE;
  LOCK TABLE T_AU_API_MASTER IN EXCLUSIVE MODE;
  LOCK TABLE T_AU_ROLE_API_PERMISSION IN EXCLUSIVE MODE;

  IF V_NEW_API_ACTIVE <> 'N' OR V_NEW_GRANT_ACTIVE <> 'N' THEN
    RAISE_APPLICATION_ERROR(-20087,
      'Permission registration must leave newly created API access inactive.');
  END IF;

  CHECK_OPTIONAL_ROLE(V_TOP_ROLE,'TOP');
  CHECK_OPTIONAL_ROLE(V_GROUP_ROLE,'GROUP');
  CHECK_OPTIONAL_LIST(V_RLM_REGIONAL_ROLES,'RLM regional');
  CHECK_OPTIONAL_LIST(V_RLM_MANAGEMENT_ROLES,'RLM management');
  IF V_TOP_ROLE IS NOT NULL AND V_TOP_ROLE=V_GROUP_ROLE THEN
    RAISE_APPLICATION_ERROR(-20081,'TOP and GROUP cannot share optional role.');
  END IF;

  SELECT COUNT(DISTINCT MENU_ID),MIN(MENU_ID)
    INTO V_FOUND,V_MENU_ID
    FROM T_MENU_PAGES
   WHERE LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH))) =
         'dashboard/entity_wise_obs';
  IF V_FOUND<>1 OR V_MENU_ID IS NULL THEN
    RAISE_APPLICATION_ERROR(-20088,
      'Existing organization dashboard menu is missing or ambiguous.');
  END IF;

  -- Each parent has a distinct page ID and authorization path.
  REGISTER_PAGE(1000300,'TOP','Top Management Dashboard',V_TOP_ROLE);
  REGISTER_PAGE(1000301,'GROUP','ZTBL | Management Audit Dashboard',V_GROUP_ROLE);
  REGISTER_PAGE(1000302,'CCO','Chief Compliance Officer Dashboard',38);
  REGISTER_PAGE(1000303,'CIA','CIA Audit Command Center',3);
  REGISTER_PAGE(1000304,'AuditParaSnap','ZTBL | Audit Para Snap',3);
  REGISTER_PAGE(1000305,'RLM','Division / GM / Regional Audit Dashboard');

  -- Role-eligible RLM accounts receive the new RLM page, not the
  -- independent Organization Structure page.
  GRANT_PAGE(1000305,1);
  FOR i IN 1..V_RLM_REGIONAL_ROLES.COUNT LOOP
    GRANT_PAGE(1000305,V_RLM_REGIONAL_ROLES(i));
  END LOOP;
  FOR i IN 1..V_RLM_MANAGEMENT_ROLES.COUNT LOOP
    GRANT_PAGE(1000305,V_RLM_MANAGEMENT_ROLES(i));
  END LOOP;

  -- Common read-only data endpoints (one URL, multiple verified scopes).
  -- PAGE_ID=1000300 is a shared API registration anchor, not an
  -- authorization substitute. T_IAS_EXEC_PAGE_API maps its actual
  -- dependent executive pages; all MVC session/role/scope checks remain.
  REGISTER_GET('ExecutiveDashboard','GetKpiSummary',1000300,'COMMON');
  REGISTER_GET('ExecutiveDashboard','GetRiskSummary',1000300,'COMMON');
  REGISTER_GET('ExecutiveDashboard','GetAgingSummary',1000300,'COMMON');
  REGISTER_GET('ExecutiveDashboard','GetCycleSummary',1000300,'COMMON');
  REGISTER_GET('ExecutiveDashboard','GetSettlementTrend',1000300,'COMMON');
  REGISTER_GET('ExecutiveDashboard','GetCriticalItems',1000300,'COMMON');
  REGISTER_GET('ExecutiveDashboard','GetKpiLifecycle',1000300,'TOP');

  REGISTER_GET('ExecutiveDashboard','GetCiaCommandKpis',1000303,'CIA');
  REGISTER_GET('ExecutiveDashboard','GetCiaIssueNature',1000303,'CIA');
  REGISTER_GET('ExecutiveDashboard','GetRoleEntityHeatmap',1000300,'REGIONAL');
  REGISTER_GET('ExecutiveDashboard','GetRoleUnitComparison',1000300,'MANAGEMENT');
  REGISTER_GET('ExecutiveDashboard','GetRoleFollowups',1000300,'MANAGEMENT');

  -- Audit Para Snap has its own page and controller, eight GET endpoints.
  -- New records stay INACTIVE; existing activation/role decisions are kept.
  REGISTER_GET('AuditParaSnap','GetKpis',1000304,'SNAP');
  REGISTER_GET('AuditParaSnap','GetDistribution',1000304,'SNAP');
  REGISTER_GET('AuditParaSnap','GetCriticalItems',1000304,'SNAP');
  REGISTER_GET('AuditParaSnap','GetReportA',1000304,'SNAP');
  REGISTER_GET('AuditParaSnap','GetReportB',1000304,'SNAP');
  REGISTER_GET('AuditParaSnap','GetReportC',1000304,'SNAP');
  REGISTER_GET('AuditParaSnap','GetReportD',1000304,'SNAP');
  REGISTER_GET('AuditParaSnap','GetReportE',1000304,'SNAP');

  -- The ExecutiveReports controller and its report A-E API grants are
  -- managed by executive_reports_permissions.sql. Do not touch them here.
  -- /ExecutiveOverview/Index redirects after authenticated role resolution.
  COMMIT;
  DBMS_OUTPUT.PUT_LINE('Executive registration committed (no activations changed).');
EXCEPTION WHEN OTHERS THEN
  ROLLBACK;
  RAISE;
END;
/
