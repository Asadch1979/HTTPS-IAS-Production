-- Run in Oracle SQL Developer F5 against UAT only.
SET SERVEROUTPUT ON;
SELECT object_name, object_type, status FROM USER_OBJECTS
 WHERE object_name IN ('PKG_HD','PKG_PG','T_AU_PLAN_DISABLED',
                       'SEQ_AU_PLAN_DISABLED_ID','V_IAS_ENTITY_REPORTING_NAMES')
 ORDER BY object_name,object_type;
SELECT name,type,line,position,text FROM USER_ERRORS
 WHERE name IN ('PKG_HD','PKG_PG') ORDER BY name,sequence;
SELECT constraint_name,constraint_type,status FROM USER_CONSTRAINTS
 WHERE table_name='T_AU_PLAN_DISABLED' ORDER BY constraint_name;
SELECT column_name,data_type,data_length,data_precision,data_scale
 FROM USER_TAB_COLUMNS WHERE table_name IN ('T_AU_PLAN','T_AU_PLAN_DISABLED')
 ORDER BY table_name,column_id;
SELECT page_name,page_path,id,status FROM T_MENU_PAGES
 WHERE LOWER(TRIM(BOTH '/' FROM PAGE_PATH))
      = 'planning/audit_plan_deletion_authorization';
SELECT P.PAGE_ID,P.GROUP_ID FROM T_MENU_PAGES_GROUPMAP P
 WHERE P.PAGE_ID IN (SELECT ID FROM T_MENU_PAGES
     WHERE LOWER(TRIM(BOTH '/' FROM PAGE_PATH))
          = 'planning/audit_plan_deletion_authorization');
-- Do not run unguarded DELETE or UPDATE in a verification script.
