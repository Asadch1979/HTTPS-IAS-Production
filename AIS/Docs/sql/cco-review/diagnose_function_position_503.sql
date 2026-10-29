-- IAS / CCO Function Wise Position - READ-ONLY 503 DIAGNOSTICS
-- Execute in PL/SQL Developer SQL Window under the actual IAS application
-- database account (or separately with the DBA for inaccessible metadata).
-- No DML, DDL, impersonation, elevated role or manual scope-token minting.
--
-- The GetFunctionPosition HTTP path calls PKG_CCO_SCOPE.P_BEGIN_SCOPE,
-- PKG_CCO_SCOPE.P_ASSERT_SCOPE, PKG_CCO_FUNCTION_POSITION.P_GET_POSITION,
-- and PKG_CCO_SCOPE.P_END_SCOPE. PKG_CCO_REPORTING is NOT on this path.

-- 1. Prove the runtime's actual database identity.
SELECT USER AS connected_schema,
       SYS_CONTEXT('USERENV','CURRENT_SCHEMA') AS current_schema
  FROM dual;

-- 2. Which prerequisite objects are visible, and are their specs/bodies valid?
SELECT owner, object_name, object_type, status
  FROM all_objects
 WHERE object_name IN
  ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
   'V_CCO_AUTHORIZED_ENTITIES','V_IAS_ENTITY_REPORTING_IDS_GROUP_REPORTS',
   'V_EXEC_ENTITY_SCOPE','V_EXEC_REPORT_SOURCE')
 ORDER BY object_name,owner,object_type;

-- 3. Exact compilation errors (including indirectly invalid dependencies).
SELECT owner,name,type,line,position,text
  FROM all_errors
 WHERE name IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION')
 ORDER BY owner,name,type,sequence;

-- 4. Confirm procedure signatures as compiled in the reachable schema.
SELECT owner,package_name,object_name,argument_name,position,in_out,data_type
  FROM all_arguments
 WHERE package_name IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION')
   AND object_name IN ('P_BEGIN_SCOPE','P_ASSERT_SCOPE','P_END_SCOPE','P_GET_POSITION')
 ORDER BY owner,package_name,object_name,position;

-- 5. Check the runtime account's direct grants or owned-object access.
-- Do not add PUBLIC grants or use a broad privileged application login.
SELECT owner,table_name,grantee,privilege
  FROM all_tab_privs
 WHERE table_name IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
                      'V_CCO_AUTHORIZED_ENTITIES',
                      'V_IAS_ENTITY_REPORTING_IDS_GROUP_REPORTS',
                      'V_EXEC_REPORT_SOURCE','V_EXEC_ENTITY_SCOPE')
 ORDER BY owner,table_name,grantee,privilege;

-- 6. Check synonyms and which owner the application actually resolves.
SELECT owner,synonym_name,table_owner,table_name
  FROM all_synonyms
 WHERE synonym_name IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
                        'V_CCO_AUTHORIZED_ENTITIES')
   AND owner IN (USER,'PUBLIC')
 ORDER BY synonym_name,owner;

-- 7. The scope view should be inaccessible or empty outside a verified,
-- active IAS scope. Run this only if SELECT is already authorized; do not
-- attempt to recreate or bypass the trusted scope to make it return records.
-- SELECT COUNT(*) AS unscoped_rows FROM V_CCO_AUTHORIZED_ENTITIES;

-- 8. DBA-only verification, using existing controlled privileges:
-- A. IAS_CCO_CTX must reference the trusted package under a protected OWNER.
-- B. Scope transaction marker T_CCO_SCOPE_INIT_GTT, entity GTT and context
--    must match the corrected PKG_CCO_SCOPE body, with ON COMMIT DELETE ROWS.
-- C. The app login must have no direct GTT DML, CREATE/ALTER CONTEXT, or
--    rights to replace the protected scope package.
-- D. PKG_CCO_SCOPE.P_REQUIRE_ACTIVE must independently verify IAS session,
--    selected user context, role, page, group and posting. Never disable it.
-- E. Review exact GET /api/CcoCompliance/GetFunctionPosition grant using
--    the IAS permission subsystem; no wildcard, role or Super User exemption.
--
-- AFTER deploying the diagnostic logging fix, locate the matching application
-- log for "CCO Function Wise Position database failure at {Stage}; Oracle number
-- {OracleNumber}; correlation {Correlation}". The numerical error together
-- with Stage pinpoints the failing object or validation gate.
--
-- Examples:
-- ORA-06550 / PLS-00201 -> unresolved/missing package or EXECUTE privilege.
-- ORA-04063 / ORA-06508 -> invalid dependent package/body.
-- ORA-00942 -> missing object/view or inaccessible grant.
-- ORA-20810/20811 -> trusted context/session validation failed; investigate,
--                     do NOT relax the check.
-- ORA-20812/20813 -> scope token / transaction marker failed; verify same
--                     connection and uninterrupted transaction.
-- ORA-20803 -> reporting package rejected duplicates or unverified source.
--
-- No results from this diagnostic document authorize production activation.
