-- Manual read-only checks after compiling the corrected PKG_CCO_REPORTING body.
-- Do not execute the CCO reporting procedures until security approval.
-- 1. Confirm actual column name.
SELECT owner,table_name,column_name
  FROM all_tab_columns
 WHERE owner='ZTBLAIS_PROD'
   AND table_name='T_AUDITEE_ENTITIES_MAPING'
   AND column_name IN ('AUDITEDBY','AUDIT_BY')
 ORDER BY column_name;

-- 2. Check package/object status after applying the F8 spec/body.
SELECT owner,object_name,object_type,status
  FROM all_objects
 WHERE owner='ZTBLAIS_PROD' AND object_name='PKG_CCO_REPORTING'
 ORDER BY object_type;

-- 3. Read the precise failing statement position if any error remains.
SELECT owner,name,type,line,position,text
  FROM all_errors
 WHERE owner='ZTBLAIS_PROD'
   AND name='PKG_CCO_REPORTING'
 ORDER BY sequence;

-- 4. Confirm the source objects used by the CTE exist and are valid.
SELECT owner,object_name,object_type,status
  FROM all_objects
 WHERE object_name IN ('V_EXEC_ENTITY_SCOPE','V_EXEC_REPORT_SOURCE',
      'V_EXEC_KPI_LIFECYCLE','V_CCO_AUTHORIZED_ENTITIES',
      'T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING')
   AND owner='ZTBLAIS_PROD'
 ORDER BY object_name,object_type;

-- IMPORTANT: a VALID package body does not imply the reporting endpoints are
-- approved or operational. REQUIRE_VERIFIED_CONTEXT deliberately raises -20803.
-- Role/session/context/token verification and actual database result reconciliation
-- must be approved separately. Do not remove the guard to make the query run.
