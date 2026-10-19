-- IAS Executive Dashboards: SUPER USER ONLY production evaluation.
-- Review and run manually as the IAS owning schema. No startup execution.
--
-- IMPORTANT: Restricts ONLY the five dedicated executive pages and
-- /api/ExecutiveDashboard/* plus /api/AuditParaSnap/* routes listed below.
-- No other IAS menus/pages, organization grants, or API endpoints are touched.
-- The RLM route ALIASES existing /Dashboard/entity_wise_obs:
-- DO NOT REMOVE non-Super-User grants on that shared organization page.
-- To hide /ExecutiveOverview/RLM entirely from ordinary users, an additional
-- authenticated role guard is required in ExecutiveOverviewController.
--
-- Verified IAS Super User ID/GROUP_ID = 1. No other executive role is granted.
-- Existing API IS_ACTIVE is preserved (Y or N); new API rows default N.
-- Existing role-1 API grants become Y; all other roles' grants become N.
-- All five executive parent page grants to other groups are REMOVED.
-- Existing T_MENU_PAGES STATUS/HIDE_MENU and other metadata are unchanged.
-- Report A-E /api/ExecutiveReports/* are handled only in
-- executive_reports_permissions.sql.
--
-- Back up these IAS permission tables and inspect the preview SELECTs in
-- verify_executive_superuser_permissions.sql before running this script.
-- Run with SQL Developer F5 (script mode) or SQL*Plus, NOT F8 single-statement.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON;
DECLARE
  C_SUPER_USER CONSTANT NUMBER := 1;
  C_NEW_API_ACTIVE CONSTANT CHAR(1) := 'N';
  V_MENU_ID NUMBER;
  V_FOUND NUMBER;
  V_CONFLICTS NUMBER;
  V_API_ID NUMBER;
  V_OLD_PAGE NUMBER;
  V_NEW_PAGES NUMBER := 0;
  V_REVOKED_PAGE_GRANTS NUMBER := 0;
  V_REVOKED_API_GRANTS NUMBER := 0;
  V_REGISTERED_API_COUNT NUMBER := 0;

  PROCEDURE REGISTER_PAGE(P_ID NUMBER, P_SUFFIX VARCHAR2, P_NAME VARCHAR2) IS
    V_PATH VARCHAR2(150) := 'ExecutiveOverview/' || P_SUFFIX;
    V_KEY VARCHAR2(150) := 'EXECUTIVE_' || P_SUFFIX;
  BEGIN
    SELECT COUNT(*) INTO V_CONFLICTS
      FROM T_MENU_PAGES
     WHERE (ID = P_ID AND
           (NVL(UPPER(TRIM(PAGE_KEY)), '#') <> UPPER(V_KEY)
            OR NVL(LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH))), '#')
                 <> LOWER(V_PATH)))
        OR (ID <> P_ID AND
           (LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH))) = LOWER(V_PATH)
            OR UPPER(TRIM(PAGE_KEY)) = UPPER(V_KEY)));

    IF V_CONFLICTS > 0 THEN
      RAISE_APPLICATION_ERROR(-20741, 'Executive page ID/path/key conflict: ' || V_PATH);
    END IF;

    SELECT COUNT(*) INTO V_FOUND FROM T_MENU_PAGES WHERE ID = P_ID;
    IF V_FOUND = 0 THEN
      INSERT INTO T_MENU_PAGES
        (ID, MENU_ID, PAGE_NAME, PAGE_PATH, PAGE_ORDER,
         STATUS, HIDE_MENU, PAGE_KEY, PAGE_URL)
      VALUES
        (P_ID, V_MENU_ID, P_NAME, V_PATH, 80 + P_ID - 1000300,
         'A', 0, V_KEY, '/' || V_PATH);
      V_NEW_PAGES := V_NEW_PAGES + 1;
    END IF;
    -- Never change existing page visibility, name, path or status.
    DBMS_OUTPUT.PUT_LINE('Verified executive page ' || P_ID || ': ' || V_PATH);
  END REGISTER_PAGE;

  PROCEDURE REGISTER_GET(
    P_CONTROLLER VARCHAR2, P_ACTION VARCHAR2,
    P_PAGE_ID NUMBER, P_ALLOW_LEGACY_CIA NUMBER DEFAULT 0) IS
    V_PATH VARCHAR2(250) := '/api/' || P_CONTROLLER || '/' || P_ACTION;
  BEGIN
    SELECT COUNT(*), MIN(API_ID) INTO V_FOUND, V_API_ID
      FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH))) =
           LOWER(TRIM(BOTH '/' FROM V_PATH))
       AND UPPER(TRIM(HTTP_METHOD)) = 'GET';
    IF V_FOUND > 1 THEN
      RAISE_APPLICATION_ERROR(-20742, 'Duplicate GET API path: ' || V_PATH);
    ELSIF V_FOUND = 0 THEN
      INSERT INTO T_AU_API_MASTER
        (ACTION_NAME, PAGE_ID, CONTROLLER_NAME, API_PATH,
         HTTP_METHOD, IS_ACTIVE, CREATED_ON)
      VALUES
        (P_ACTION, P_PAGE_ID, P_CONTROLLER, V_PATH,
         'GET', C_NEW_API_ACTIVE, SYSDATE);
      SELECT API_ID INTO V_API_ID
        FROM T_AU_API_MASTER
       WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH))) =
             LOWER(TRIM(BOTH '/' FROM V_PATH))
         AND UPPER(TRIM(HTTP_METHOD)) = 'GET';
    ELSE
      SELECT PAGE_ID INTO V_OLD_PAGE FROM T_AU_API_MASTER WHERE API_ID = V_API_ID;
      SELECT COUNT(*) INTO V_CONFLICTS
        FROM T_AU_API_MASTER
       WHERE API_ID = V_API_ID
         AND (UPPER(NVL(TRIM(CONTROLLER_NAME), '#')) <> UPPER(P_CONTROLLER)
              OR UPPER(NVL(TRIM(ACTION_NAME), '#')) <> UPPER(P_ACTION)
              OR (NVL(PAGE_ID, -1) <> P_PAGE_ID
                  AND NOT (P_ALLOW_LEGACY_CIA = 1
                           AND PAGE_ID = 1000300
                           AND P_PAGE_ID = 1000303)));
      IF V_CONFLICTS > 0 THEN
        RAISE_APPLICATION_ERROR(-20743, 'Existing API metadata conflict: ' || V_PATH);
      END IF;
      IF P_ALLOW_LEGACY_CIA = 1 AND V_OLD_PAGE = 1000300 THEN
        UPDATE T_AU_API_MASTER SET PAGE_ID = 1000303 WHERE API_ID = V_API_ID;
      END IF;
    END IF;

    -- Deliberately affects only the 20 explicit ExecutiveDashboard and Snap GETs.
    UPDATE T_AU_ROLE_API_PERMISSION
       SET IS_ACTIVE = 'N'
     WHERE API_ID = V_API_ID
       AND (ROLE_ID IS NULL OR ROLE_ID <> C_SUPER_USER)
       AND NVL(IS_ACTIVE, 'N') <> 'N';
    V_REVOKED_API_GRANTS := V_REVOKED_API_GRANTS + SQL%ROWCOUNT;

    -- Explicitly grant this API to Super User, even if another role previously
    -- received it. The independent master IS_ACTIVE still gates access.
    MERGE INTO T_AU_ROLE_API_PERMISSION p
    USING (SELECT C_SUPER_USER ROLE_ID, V_API_ID API_ID FROM dual) s
       ON (p.ROLE_ID = s.ROLE_ID AND p.API_ID = s.API_ID)
    WHEN MATCHED THEN UPDATE SET p.IS_ACTIVE = 'Y'
    WHEN NOT MATCHED THEN INSERT
      (ROLE_ID, API_ID, IS_ACTIVE, CREATED_BY)
      VALUES (s.ROLE_ID, s.API_ID, 'Y', NULL);

    V_REGISTERED_API_COUNT := V_REGISTERED_API_COUNT + 1;
    DBMS_OUTPUT.PUT_LINE('Super User only GET: ' || V_PATH);
  END REGISTER_GET;
BEGIN
  -- Serialize page/API ID allocation and privilege changes.
  LOCK TABLE T_MENU_PAGES IN EXCLUSIVE MODE;
  LOCK TABLE T_MENU_PAGES_GROUPMAP IN EXCLUSIVE MODE;
  LOCK TABLE T_AU_API_MASTER IN EXCLUSIVE MODE;
  LOCK TABLE T_AU_ROLE_API_PERMISSION IN EXCLUSIVE MODE;

  -- Existing organization menu is the anchor; it is not permission-modified.
  SELECT COUNT(*), MIN(MENU_ID) INTO V_FOUND, V_MENU_ID
    FROM T_MENU_PAGES
   WHERE LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH))) =
         'dashboard/entity_wise_obs';
  IF V_FOUND <> 1 OR V_MENU_ID IS NULL THEN
    RAISE_APPLICATION_ERROR(-20744,
      'One existing Dashboard/entity_wise_obs menu must be verified first.');
  END IF;

  REGISTER_PAGE(1000300, 'TOP', 'Top Management Dashboard');
  REGISTER_PAGE(1000301, 'GROUP', 'ZTBL | Management Audit Dashboard');
  REGISTER_PAGE(1000302, 'CCO', 'Chief Compliance Officer Dashboard');
  REGISTER_PAGE(1000303, 'CIA', 'CIA Audit Command Center');
  REGISTER_PAGE(1000304, 'AuditParaSnap', 'ZTBL | Audit Para Snap');

  -- Restrict ONLY these five new parent pages, not shared RLM/org pages.
  DELETE FROM T_MENU_PAGES_GROUPMAP
   WHERE PAGE_ID IN (1000300,1000301,1000302,1000303,1000304)
     AND (GROUP_ID IS NULL OR GROUP_ID <> C_SUPER_USER);
  V_REVOKED_PAGE_GRANTS := SQL%ROWCOUNT;

  FOR p IN (SELECT 1000300 PAGE_ID FROM dual
            UNION ALL SELECT 1000301 FROM dual
            UNION ALL SELECT 1000302 FROM dual
            UNION ALL SELECT 1000303 FROM dual
            UNION ALL SELECT 1000304 FROM dual) LOOP
    INSERT INTO T_MENU_PAGES_GROUPMAP (GROUPMAP_ID, GROUP_ID, PAGE_ID)
    SELECT (SELECT NVL(MAX(GROUPMAP_ID),0)+1 FROM T_MENU_PAGES_GROUPMAP),
           C_SUPER_USER, p.PAGE_ID
      FROM dual
     WHERE NOT EXISTS
           (SELECT 1 FROM T_MENU_PAGES_GROUPMAP
             WHERE PAGE_ID = p.PAGE_ID AND GROUP_ID = C_SUPER_USER);
  END LOOP;

  -- Shared GET URL metadata remains anchored to TOP page 1000300.
  -- MVC checks independent authenticated role/perspective/page/scope.
  REGISTER_GET('ExecutiveDashboard', 'GetKpiSummary', 1000300);
  REGISTER_GET('ExecutiveDashboard', 'GetRiskSummary', 1000300);
  REGISTER_GET('ExecutiveDashboard', 'GetAgingSummary', 1000300);
  REGISTER_GET('ExecutiveDashboard', 'GetCycleSummary', 1000300);
  REGISTER_GET('ExecutiveDashboard', 'GetSettlementTrend', 1000300);
  REGISTER_GET('ExecutiveDashboard', 'GetCriticalItems', 1000300);
  REGISTER_GET('ExecutiveDashboard', 'GetKpiLifecycle', 1000300);

  REGISTER_GET('ExecutiveDashboard', 'GetCiaCommandKpis', 1000303, 1);
  REGISTER_GET('ExecutiveDashboard', 'GetCiaIssueNature', 1000303, 1);

  REGISTER_GET('ExecutiveDashboard', 'GetRoleEntityHeatmap', 1000300);
  REGISTER_GET('ExecutiveDashboard', 'GetRoleUnitComparison', 1000300);
  REGISTER_GET('ExecutiveDashboard', 'GetRoleFollowups', 1000300);

  -- Separate Audit Para Snap controller uses page ID 1000304.
  REGISTER_GET('AuditParaSnap', 'GetKpis', 1000304);
  REGISTER_GET('AuditParaSnap', 'GetDistribution', 1000304);
  REGISTER_GET('AuditParaSnap', 'GetCriticalItems', 1000304);
  REGISTER_GET('AuditParaSnap', 'GetReportA', 1000304);
  REGISTER_GET('AuditParaSnap', 'GetReportB', 1000304);
  REGISTER_GET('AuditParaSnap', 'GetReportC', 1000304);
  REGISTER_GET('AuditParaSnap', 'GetReportD', 1000304);
  REGISTER_GET('AuditParaSnap', 'GetReportE', 1000304);

  IF V_REGISTERED_API_COUNT <> 20 THEN
    RAISE_APPLICATION_ERROR(-20745, 'Expected exactly 20 executive/Snap GET APIs.');
  END IF;

  DBMS_OUTPUT.PUT_LINE('New pages: ' || V_NEW_PAGES ||
    ', removed non-Super-User page grants: ' || V_REVOKED_PAGE_GRANTS ||
    ', disabled non-Super-User API grants: ' || V_REVOKED_API_GRANTS ||
    ', checked API actions: ' || V_REGISTERED_API_COUNT);
  COMMIT;
EXCEPTION WHEN OTHERS THEN
  ROLLBACK;
  RAISE;
END;
/
