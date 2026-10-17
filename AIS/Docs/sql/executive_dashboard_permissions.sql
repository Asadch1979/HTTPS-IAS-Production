-- IAS Executive Dashboards / Command Center permission registration
-- SOURCE ONLY: DBA review and approval required before executing in production.
-- Super User is verified IAS role 1. Never guess other role IDs.
-- New dashboard views are perspective-specific tabs, not new bank-wide page grants.
-- Group/Division and Region datasets remain enforced by authenticated C# role/root checks.
-- Execute as IAS owner; inspect existing page IDs and registered group IDs first.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON;
DECLARE
  V_TOP_ROLE NUMBER := NULL;    -- optional verified additional TOP role
  V_GROUP_ROLE NUMBER := NULL;  -- optional verified additional GROUP role
  V_MENU_ID NUMBER;
  V_API_ID NUMBER;
  V_PAGE_ID NUMBER;
  V_FOUND NUMBER;
  V_COLLISIONS NUMBER;

  PROCEDURE GRANT_SUPERUSER_PAGE(P_PAGE_ID NUMBER) IS
  BEGIN
    -- T_MENU_PAGES_GROUPMAP.GROUP_ID=1 follows current IAS Super User mapping.
    -- Confirm role/group correspondence in the target IAS before execution.
    INSERT INTO T_MENU_PAGES_GROUPMAP (GROUPMAP_ID, GROUP_ID, PAGE_ID)
    SELECT (SELECT NVL(MAX(GROUPMAP_ID), 0) + 1 FROM T_MENU_PAGES_GROUPMAP),
           1, P_PAGE_ID
      FROM dual
     WHERE NOT EXISTS
           (SELECT 1 FROM T_MENU_PAGES_GROUPMAP
             WHERE GROUP_ID = 1 AND PAGE_ID = P_PAGE_ID);
  END;

  PROCEDURE GRANT_PAGE_GROUP(P_PAGE_ID NUMBER, P_GROUP_ID NUMBER) IS
  BEGIN
    IF P_GROUP_ID IS NOT NULL THEN
      INSERT INTO T_MENU_PAGES_GROUPMAP (GROUPMAP_ID, GROUP_ID, PAGE_ID)
      SELECT (SELECT NVL(MAX(GROUPMAP_ID),0)+1 FROM T_MENU_PAGES_GROUPMAP),
             P_GROUP_ID, P_PAGE_ID
        FROM dual WHERE NOT EXISTS
          (SELECT 1 FROM T_MENU_PAGES_GROUPMAP
            WHERE GROUP_ID = P_GROUP_ID AND PAGE_ID = P_PAGE_ID);
    END IF;
  END;

  PROCEDURE REGISTER_EXEC_PAGE(P_ID NUMBER, P_TYPE VARCHAR2, P_ROLE NUMBER) IS
  BEGIN
    SELECT COUNT(*) INTO V_COLLISIONS
      FROM T_MENU_PAGES
     WHERE (ID = P_ID AND NVL(PAGE_KEY,'#') <> 'EXECUTIVE_' || P_TYPE)
        OR (LOWER(TRIM(PAGE_PATH)) = LOWER('ExecutiveOverview/' || P_TYPE)
            AND ID <> P_ID);
    IF V_COLLISIONS > 0 THEN
      RAISE_APPLICATION_ERROR(-20020, 'Executive page ID/path collision: ' || P_TYPE);
    END IF;
    MERGE INTO T_MENU_PAGES p USING (SELECT P_ID id FROM dual) s
      ON (p.ID = s.id)
    WHEN MATCHED THEN UPDATE SET
      p.PAGE_NAME = CASE P_TYPE
                       WHEN 'CIA' THEN 'CIA Audit Command Center'
                       WHEN 'GROUP' THEN 'Group / Divisional Management Audit'
                       ELSE 'Executive Dashboard ' || P_TYPE END,
      p.PAGE_PATH = 'ExecutiveOverview/' || P_TYPE,
      p.PAGE_URL = '/ExecutiveOverview/' || P_TYPE,
      p.STATUS = 'A', p.HIDE_MENU = 0
    WHEN NOT MATCHED THEN INSERT
      (ID, MENU_ID, PAGE_NAME, PAGE_PATH, PAGE_ORDER,
       STATUS, HIDE_MENU, PAGE_KEY, PAGE_URL)
    VALUES
      (P_ID, V_MENU_ID,
       CASE P_TYPE WHEN 'CIA' THEN 'CIA Audit Command Center'
         WHEN 'GROUP' THEN 'Group / Divisional Management Audit'
         ELSE 'Executive Dashboard ' || P_TYPE END,
       'ExecutiveOverview/' || P_TYPE, 80 + P_ID - 1000300,
       'A', 0, 'EXECUTIVE_' || P_TYPE,
       '/ExecutiveOverview/' || P_TYPE);
    GRANT_SUPERUSER_PAGE(P_ID);
    GRANT_PAGE_GROUP(P_ID,P_ROLE);
  END;

  -- Preserve existing routes: RLM and Team Leader workbenches do NOT
  -- receive invented page IDs or unscoped impersonation privileges.
  PROCEDURE GRANT_EXISTING_SUPERUSER_PAGE(P_PATH VARCHAR2) IS
  BEGIN
    SELECT COUNT(*), MIN(ID) INTO V_FOUND, V_PAGE_ID
      FROM T_MENU_PAGES
     WHERE LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH))) =
           LOWER(TRIM(BOTH '/' FROM P_PATH));
    IF V_FOUND > 1 THEN
      RAISE_APPLICATION_ERROR(-20022,'Ambiguous existing IAS page: '||P_PATH);
    ELSIF V_FOUND = 1 THEN
      GRANT_SUPERUSER_PAGE(V_PAGE_ID);
      DBMS_OUTPUT.PUT_LINE('Super User linked to existing page: '||P_PATH);
    ELSE
      DBMS_OUTPUT.PUT_LINE('CHECK MANUALLY: existing page not registered: '||P_PATH);
    END IF;
  END;

  PROCEDURE GRANT_API_ROLE(P_ROLE NUMBER, P_API_ID NUMBER) IS
  BEGIN
    IF P_ROLE IS NULL THEN RETURN; END IF;
    MERGE INTO T_AU_ROLE_API_PERMISSION p
    USING (SELECT P_ROLE role_id, P_API_ID api_id FROM dual) s
      ON (p.ROLE_ID=s.role_id AND p.API_ID=s.api_id)
    WHEN MATCHED THEN UPDATE SET p.IS_ACTIVE='Y'
    WHEN NOT MATCHED THEN INSERT
      (ROLE_ID, API_ID, IS_ACTIVE, CREATED_BY)
    VALUES (s.role_id,s.api_id,'Y',NULL);
  END;

  PROCEDURE REGISTER_API(P_ACTION VARCHAR2, P_KIND VARCHAR2) IS
  BEGIN
    -- CIA command actions are CIA/Super User only; role reporting actions
    -- are Super User and manually approved IAS Group/RLM roles only.
    SELECT COUNT(*) INTO V_COLLISIONS FROM T_AU_API_MASTER
     WHERE LOWER(API_PATH)=LOWER('/api/ExecutiveDashboard/'||P_ACTION)
       AND UPPER(HTTP_METHOD)='GET';
    IF V_COLLISIONS>1 THEN
      RAISE_APPLICATION_ERROR(-20023,'Duplicate executive API path: '||P_ACTION);
    END IF;
    IF V_COLLISIONS=0 THEN
      INSERT INTO T_AU_API_MASTER
        (ACTION_NAME,PAGE_ID,CONTROLLER_NAME,API_PATH,HTTP_METHOD,IS_ACTIVE,CREATED_ON)
      VALUES (P_ACTION,1000300,'ExecutiveDashboard',
              '/api/ExecutiveDashboard/'||P_ACTION,'GET','Y',SYSDATE);
    END IF;
    SELECT API_ID INTO V_API_ID FROM T_AU_API_MASTER
     WHERE LOWER(API_PATH)=LOWER('/api/ExecutiveDashboard/'||P_ACTION)
       AND UPPER(HTTP_METHOD)='GET';

    GRANT_API_ROLE(1,V_API_ID);
    IF P_KIND='COMMON' THEN
      GRANT_API_ROLE(3,V_API_ID); GRANT_API_ROLE(38,V_API_ID);
      GRANT_API_ROLE(V_TOP_ROLE,V_API_ID);
      GRANT_API_ROLE(V_GROUP_ROLE,V_API_ID);
    ELSIF P_KIND='CIA' THEN
      GRANT_API_ROLE(3,V_API_ID);
    ELSIF P_KIND='MANAGEMENT' THEN
      GRANT_API_ROLE(V_GROUP_ROLE,V_API_ID);
    END IF;
    DBMS_OUTPUT.PUT_LINE('Registered GET API: '||P_ACTION||' ['||P_KIND||']');
  END;
BEGIN
  -- NULL optional roles are intentional: Super User provisioning MUST NOT
  -- silently fail or invent organizational role IDs.
  IF V_TOP_ROLE IS NOT NULL AND (V_TOP_ROLE <= 0 OR V_TOP_ROLE IN (1,3,38)) THEN
    RAISE_APPLICATION_ERROR(-20021,'Invalid additional TOP role.');
  END IF;
  IF V_GROUP_ROLE IS NOT NULL AND (V_GROUP_ROLE <= 0 OR V_GROUP_ROLE IN (1,3,38)
       OR V_GROUP_ROLE=V_TOP_ROLE) THEN
    RAISE_APPLICATION_ERROR(-20021,'Invalid additional GROUP role.');
  END IF;

  SELECT MIN(MENU_ID) INTO V_MENU_ID FROM T_MENU_PAGES
   WHERE LOWER(TRIM(BOTH '/' FROM PAGE_PATH))='dashboard/entity_wise_obs';
  IF V_MENU_ID IS NULL THEN
    RAISE_APPLICATION_ERROR(-20024,'Organization dashboard menu must exist before registration.');
  END IF;

  REGISTER_EXEC_PAGE(1000300,'TOP',V_TOP_ROLE);
  REGISTER_EXEC_PAGE(1000301,'GROUP',V_GROUP_ROLE);
  REGISTER_EXEC_PAGE(1000302,'CCO',38);
  REGISTER_EXEC_PAGE(1000303,'CIA',3);

  -- RLM uses the existing organization-dashboard page alias in PageIdResolver.
  -- Team Leader workbench requires live assigned-engagement scope even as Super User.
  GRANT_EXISTING_SUPERUSER_PAGE('Dashboard/entity_wise_obs');
  GRANT_EXISTING_SUPERUSER_PAGE('FieldAudit/AR_Dashboard');
  GRANT_EXISTING_SUPERUSER_PAGE('ManagementAudit/MA_Dashboard');

  REGISTER_API('GetKpiSummary','COMMON');
  REGISTER_API('GetRiskSummary','COMMON');
  REGISTER_API('GetAgingSummary','COMMON');
  REGISTER_API('GetCycleSummary','COMMON');
  REGISTER_API('GetSettlementTrend','COMMON');
  REGISTER_API('GetCriticalItems','COMMON');
  REGISTER_API('GetKpiLifecycle','COMMON');
  REGISTER_API('GetCiaCommandKpis','CIA');
  REGISTER_API('GetCiaIssueNature','CIA');
  REGISTER_API('GetRoleEntityHeatmap','REGIONAL');
  REGISTER_API('GetRoleUnitComparison','MANAGEMENT');
  REGISTER_API('GetRoleFollowups','MANAGEMENT');

  -- Report A-E endpoints are governed by executive_reports_permissions.sql.
  -- Do not override its inactive-by-default rollout or grant broad RLM access.
  COMMIT;
EXCEPTION WHEN OTHERS THEN
  ROLLBACK;
  RAISE;
END;
/
