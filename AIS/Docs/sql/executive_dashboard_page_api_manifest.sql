-- IAS executive page -> exact GET API dependency mapping.
-- IAS DEVELOPMENT/STAGING FIRST. No automatic production execution.
-- 1000300 TOP, 1000301 GROUP, 1000302 CCO, 1000303 CIA Command,
-- 1000304 Audit Para Snap, 1000305 RLM (dedicated new page).
-- A shared URL is registered ONCE in T_AU_API_MASTER; this table
-- associates it with more than one permitted parent page.
-- This script DOES NOT activate API masters, grant any role, or alter
-- existing user/group assignments. Execute as F5 / SQL*Plus after DBA review.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON;

-- DDL: create the manifest table once, idempotently.
BEGIN
  EXECUTE IMMEDIATE
    'CREATE TABLE T_IAS_EXEC_PAGE_API ('||
    'PAGE_ID NUMBER NOT NULL, API_ID NUMBER NOT NULL, '||
    'CONSTRAINT PK_IAS_EXEC_PAGE_API PRIMARY KEY(PAGE_ID,API_ID), '||
    'CONSTRAINT CK_IAS_EXEC_PAGE_POS CHECK(PAGE_ID BETWEEN 1000300 AND 1000305), '||
    'CONSTRAINT CK_IAS_EXEC_API_POS CHECK(API_ID > 0))';
EXCEPTION
  WHEN OTHERS THEN
    IF SQLCODE <> -955 THEN RAISE; END IF;
END;
/

-- Fail closed when an existing object is not the expected table shape.
DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM user_tab_columns
   WHERE table_name='T_IAS_EXEC_PAGE_API'
     AND ((column_name='PAGE_ID' AND data_type='NUMBER' AND nullable='N')
       OR (column_name='API_ID' AND data_type='NUMBER' AND nullable='N'));
  IF v_count <> 2 THEN
    RAISE_APPLICATION_ERROR(-20851,'Executive page/API manifest schema mismatch.');
  END IF;
END;
/

-- New, independent RLM page. Never change the existing Organization
-- Structure page or revoke its separate grants.
DECLARE
  v_collision NUMBER;
  v_existing NUMBER;
  v_menu_id NUMBER;
BEGIN
  LOCK TABLE T_MENU_PAGES IN EXCLUSIVE MODE;
  SELECT COUNT(*) INTO v_collision FROM T_MENU_PAGES
   WHERE (ID=1000305 AND
          LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH)))<>'executiveoverview/rlm')
      OR (ID<>1000305 AND
          LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH)))='executiveoverview/rlm');
  IF v_collision<>0 THEN
    RAISE_APPLICATION_ERROR(-20852,'RLM ID 1000305 / path collision.');
  END IF;
  SELECT COUNT(*),MIN(MENU_ID) INTO v_existing,v_menu_id
    FROM T_MENU_PAGES WHERE ID=1000300;
  IF v_existing<>1 OR v_menu_id IS NULL THEN
    RAISE_APPLICATION_ERROR(-20853,'Existing executive TOP page required first.');
  END IF;
  SELECT COUNT(*) INTO v_existing FROM T_MENU_PAGES WHERE ID=1000305;
  IF v_existing=0 THEN
    INSERT INTO T_MENU_PAGES
      (ID,MENU_ID,PAGE_NAME,PAGE_PATH,PAGE_ORDER,STATUS,
       HIDE_MENU,PAGE_KEY,PAGE_URL)
    VALUES (1000305,v_menu_id,'Division / GM / Regional Audit Dashboard',
            'ExecutiveOverview/RLM',86,'A',0,
            'EXECUTIVE_RLM','/ExecutiveOverview/RLM');
  END IF;
END;
/

-- Explicit manifest. ALL endpoint rows MUST already exist in master.
-- Existing mappings for other components/roles are not modified.
DECLARE
  v_expected NUMBER;
  v_found NUMBER;
BEGIN
  LOCK TABLE T_IAS_EXEC_PAGE_API IN EXCLUSIVE MODE;
  SELECT COUNT(*),COUNT(m.api_id) INTO v_expected,v_found
  FROM
  (
    SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetKpiSummary' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetRiskSummary' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetAgingSummary' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetCycleSummary' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetSettlementTrend' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetKpiLifecycle' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetKpiSummary' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetRiskSummary' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetAgingSummary' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetCycleSummary' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetSettlementTrend' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetRoleUnitComparison' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetRoleFollowups' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveReports' controller_name,'GetReportE' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetKpiSummary' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetRiskSummary' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetAgingSummary' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetCycleSummary' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetSettlementTrend' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveReports' controller_name,'GetReportB' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveReports' controller_name,'GetReportC' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveReports' controller_name,'GetReportD' action_name FROM dual
    UNION ALL SELECT 1000303 page_id,'ExecutiveDashboard' controller_name,'GetCiaCommandKpis' action_name FROM dual
    UNION ALL SELECT 1000303 page_id,'ExecutiveDashboard' controller_name,'GetCiaIssueNature' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetKpis' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetDistribution' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportA' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportB' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportC' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportD' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportE' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetKpiSummary' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetRiskSummary' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetAgingSummary' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetCycleSummary' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetSettlementTrend' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetRoleEntityHeatmap' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetRoleUnitComparison' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetRoleFollowups' action_name FROM dual
  ) x
  LEFT JOIN T_AU_API_MASTER m
    ON UPPER(TRIM(m.CONTROLLER_NAME))=UPPER(x.controller_name)
   AND UPPER(TRIM(m.ACTION_NAME))=UPPER(x.action_name)
   AND UPPER(TRIM(m.HTTP_METHOD))='GET'
   AND LOWER(TRIM(BOTH '/' FROM TRIM(m.API_PATH)))=
       LOWER('api/'||x.controller_name||'/'||x.action_name);
  IF v_found<>v_expected THEN
    RAISE_APPLICATION_ERROR(-20854,
      'Missing executive GET API registrations. Register each API before seeding the manifest.');
  END IF;

  -- Refuse duplicate registrations, avoiding unstable joins on API paths.
  SELECT COUNT(*) INTO v_found FROM
  (
    SELECT LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH))) path,
           COUNT(*) total
      FROM T_AU_API_MASTER
     WHERE UPPER(TRIM(HTTP_METHOD))='GET'
       AND UPPER(CONTROLLER_NAME) IN
           ('EXECUTIVEDASHBOARD','AUDITPARASNAP','EXECUTIVEREPORTS')
     GROUP BY LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH)))
    HAVING COUNT(*) > 1
  );
  IF v_found>0 THEN
    RAISE_APPLICATION_ERROR(-20855,'Duplicate executive API paths.');
  END IF;

  MERGE INTO T_IAS_EXEC_PAGE_API t
  USING
  (
    SELECT x.page_id, m.api_id
    FROM
    (
    SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetKpiSummary' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetRiskSummary' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetAgingSummary' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetCycleSummary' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetSettlementTrend' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000300 page_id,'ExecutiveDashboard' controller_name,'GetKpiLifecycle' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetKpiSummary' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetRiskSummary' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetAgingSummary' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetCycleSummary' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetSettlementTrend' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetRoleUnitComparison' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveDashboard' controller_name,'GetRoleFollowups' action_name FROM dual
    UNION ALL SELECT 1000301 page_id,'ExecutiveReports' controller_name,'GetReportE' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetKpiSummary' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetRiskSummary' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetAgingSummary' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetCycleSummary' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetSettlementTrend' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveDashboard' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveReports' controller_name,'GetReportB' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveReports' controller_name,'GetReportC' action_name FROM dual
    UNION ALL SELECT 1000302 page_id,'ExecutiveReports' controller_name,'GetReportD' action_name FROM dual
    UNION ALL SELECT 1000303 page_id,'ExecutiveDashboard' controller_name,'GetCiaCommandKpis' action_name FROM dual
    UNION ALL SELECT 1000303 page_id,'ExecutiveDashboard' controller_name,'GetCiaIssueNature' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetKpis' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetDistribution' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportA' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportB' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportC' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportD' action_name FROM dual
    UNION ALL SELECT 1000304 page_id,'AuditParaSnap' controller_name,'GetReportE' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetKpiSummary' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetRiskSummary' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetAgingSummary' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetCycleSummary' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetSettlementTrend' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetCriticalItems' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetRoleEntityHeatmap' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetRoleUnitComparison' action_name FROM dual
    UNION ALL SELECT 1000305 page_id,'ExecutiveDashboard' controller_name,'GetRoleFollowups' action_name FROM dual
    ) x
    JOIN T_AU_API_MASTER m
      ON UPPER(TRIM(m.CONTROLLER_NAME))=UPPER(x.controller_name)
     AND UPPER(TRIM(m.ACTION_NAME))=UPPER(x.action_name)
     AND UPPER(TRIM(m.HTTP_METHOD))='GET'
     AND LOWER(TRIM(BOTH '/' FROM TRIM(m.API_PATH)))=
         LOWER('api/'||x.controller_name||'/'||x.action_name)
  ) v ON (t.PAGE_ID=v.PAGE_ID AND t.API_ID=v.API_ID)
  WHEN NOT MATCHED THEN INSERT (PAGE_ID,API_ID) VALUES (v.PAGE_ID,v.API_ID);
  DBMS_OUTPUT.PUT_LINE('Seeded verified executive page/API manifest.');
  COMMIT;
EXCEPTION WHEN OTHERS THEN
  ROLLBACK;
  RAISE;
END;
/

-- Read-only verification: ensure all manifest APIs exist and check activation.
SELECT d.PAGE_ID,m.CONTROLLER_NAME,m.ACTION_NAME,m.API_PATH,
       m.IS_ACTIVE AS MASTER_ACTIVE
FROM T_IAS_EXEC_PAGE_API d
JOIN T_AU_API_MASTER m ON m.API_ID=d.API_ID
ORDER BY d.PAGE_ID,m.CONTROLLER_NAME,m.ACTION_NAME;
