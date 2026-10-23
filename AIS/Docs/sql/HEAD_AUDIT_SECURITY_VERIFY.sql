-- READ ONLY: run only in the explicitly approved non-production IAS schema.
-- These queries do not compile objects or modify security assignments.
SELECT SYS_CONTEXT('USERENV','DB_NAME') database_name,
       SYS_CONTEXT('USERENV','CURRENT_SCHEMA') schema_name FROM dual;
SELECT table_name,column_name,data_type,nullable FROM user_tab_columns
 WHERE table_name IN ('T_AU_PLAN_ENG_LOG','T_USER_SESSION','T_USER_CONTEXT_ASSIGNMENT')
 ORDER BY table_name,column_id;
SELECT name,type,line,position,text FROM user_errors
 WHERE name IN ('PKG_HEAD_AUDIT_MONITOR','V_HEAD_AUDIT_OFFICE_SCOPE','V_HEAD_AUDIT_ENG_SCOPE')
 ORDER BY name,type,sequence;
SELECT object_name,object_type,status FROM user_objects
 WHERE object_name IN ('PKG_HEAD_AUDIT_MONITOR','V_HEAD_AUDIT_OFFICE_SCOPE','V_HEAD_AUDIT_ENG_SCOPE');
-- With DBA-provided test binds, run P_VALIDATE_SCOPE using an authorized current
-- identity/session/group. Repeat using pre-provisioned revoked-session and
-- unauthorized-assignment identities. Expect -20154 and -20151 respectively.
-- Do not put real session identifiers or user data into committed test logs.
-- BEGIN PKG_HEAD_AUDIT_MONITOR.P_VALIDATE_SCOPE(:user_id,:role_id,:entity_id,
--   :context_id,:group_id,:session_id); END;
-- Test out-of-scope engagement via P_GET_WORK (-20152), and today/out-of-period
-- cutoff through P_GET_ENGAGEMENTS (-20153). Inspect HTTP mappings in staging.
