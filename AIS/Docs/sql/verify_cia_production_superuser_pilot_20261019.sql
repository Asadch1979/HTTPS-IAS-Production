-- CIA production-first verification: READ ONLY (Oracle 18c / SQL Developer).
-- Super User pilot only. No DML, DDL, permission activation, or COMMIT.
-- Execute with F5 (Run Script); record grid output and elapsed time.
-- Audit period 5 is from the user-supplied IAS period-5 sample:
-- adjust :P_AUDIT_PERIOD_ID after inspecting T_AU_PERIOD.
SET SERVEROUTPUT ON;
SET TIMING ON;
VARIABLE P_AUDIT_PERIOD_ID NUMBER;
EXEC :P_AUDIT_PERIOD_ID := 5;

PROMPT === 1. Confirm audit period and package/view object validity ===
SELECT p.auditperiodid, p.description, p.start_date, p.end_date
  FROM t_au_period p
 WHERE p.auditperiodid = :P_AUDIT_PERIOD_ID;

SELECT object_name,object_type,status
  FROM user_objects
 WHERE object_name IN
  ('PKG_EXEC_DASHBOARD','V_EXEC_KPI_LIFECYCLE',
   'V_IAS_CIA_ISSUE_CHECKLIST_ROOT','T_IAS_CIA_ISSUE_NATURE_MAP')
 ORDER BY object_name,object_type;

PROMPT === 2. CIA GET API registration/active role grants: must be role 1 only ===
SELECT a.api_id,a.action_name,a.is_active api_active,
       p.role_id,p.is_active grant_active
  FROM t_au_api_master a
  LEFT JOIN t_au_role_api_permission p
    ON p.api_id=a.api_id
 WHERE UPPER(a.controller_name)='EXECUTIVEDASHBOARD'
   AND UPPER(a.http_method)='GET'
   AND UPPER(a.action_name) IN
       ('GETCIACOMMANDKPIS','GETCIAISSUENATURE')
 ORDER BY a.action_name,p.role_id;

PROMPT === 3. Approved checklist-root mappings (no guessed IDs) ===
SELECT source_type,source_id,category_code,match_priority,is_active
  FROM t_ias_cia_issue_nature_map
 WHERE source_type='CHECKLIST_ROOT'
 ORDER BY source_id;

PROMPT === 4. Full CIA KPI refcursor: review NULL vs 0, workflow rules ===
DECLARE
  rc SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_EXEC_KPIS(
    P_DASHBOARD_TYPE=>'CIA',
    P_ENTITY_ID=>NULL,
    P_AUDIT_PERIOD_ID=>:P_AUDIT_PERIOD_ID,
    IO_CURSOR=>rc);
  DBMS_SQL.RETURN_RESULT(rc);
END;
/

PROMPT === 5. Ten-row CIA issue classification: total=categories+unmapped ===
DECLARE
  rc SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE(
    P_DASHBOARD_TYPE=>'CIA',
    P_ENTITY_ID=>NULL,
    P_AUDIT_PERIOD_ID=>:P_AUDIT_PERIOD_ID,
    IO_CURSOR=>rc);
  DBMS_SQL.RETURN_RESULT(rc);
END;
/

PROMPT === No SQL in this file changes production data or IAS permissions ===
