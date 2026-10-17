-- IAS role dashboard Oracle read-only preflight / verification (F8 compatible).
-- This script NEVER deploys or changes data. Execute one statement at a time.
-- PKG_EXEC_DASHBOARD.sql contains the package and its existing supporting views:
-- DBA must evaluate view replacement impact or compile spec/body separately.
-- Team Lead package depends on Working Papers V2 objects in target schema.

-- 1. Required objects in current Oracle schema.
SELECT object_name, object_type, status
  FROM user_objects
 WHERE object_name IN
  ('PKG_EXEC_DASHBOARD','PKG_TEAM_LEAD_DASHBOARD','V_EXEC_PARA_BASE',
   'V_EXEC_ENTITY_SCOPE','T_AU_AUDIT_TEAM_TASKLIST','T_AU_TEAM_MEMBERS',
   'T_AU_PLAN_ENG','T_AU_OBSERVATION','T_WP_HEADER','T_WP_ITEM')
 ORDER BY object_name,object_type;

-- 2. Verify source columns rather than assuming development schema == production.
SELECT table_name, column_name, data_type
  FROM user_tab_columns
 WHERE (table_name='T_WP_HEADER' AND column_name IN
   ('WP_ID','ENG_ID','ENTITY_ID','PAPER_TYPE','VERSION_NO','STATUS',
    'REVIEWER_PPNO','PREPARER_PPNO','DUE_DATE','REVIEWED_ON','APPROVED_ON'))
    OR (table_name='T_WP_ITEM' AND column_name IN ('WP_ID','RESULT'))
    OR (table_name='T_AU_AUDIT_TEAM_TASKLIST' AND column_name IN
      ('ID','TEAM_ID','ENG_PLAN_ID','TEAMMEMBER_PPNO','STATUS_ID','ISACTIVE'))
    OR (table_name='T_AU_TEAM_MEMBERS' AND column_name IN
      ('T_ID','MEMBER_PPNO','ISTEAMLEAD'))
    OR (table_name='T_AU_PLAN_ENG' AND column_name IN
      ('ENG_ID','PLAN_ID','PERIOD_ID','ENTITY_ID','STATUS',
       'AUDIT_STARTDATE','AUDIT_ENDDATE','OPERATION_STARTDATE','OPERATION_ENDDATE'))
 ORDER BY table_name,column_name;

-- 3. New procedure signatures should appear after DBA compiles scripts.
SELECT object_name,procedure_name
  FROM user_procedures
 WHERE object_name IN ('PKG_EXEC_DASHBOARD','PKG_TEAM_LEAD_DASHBOARD')
   AND procedure_name IN
    ('P_GET_ROLE_ENTITY_HEATMAP','P_GET_ROLE_UNIT_COMPARISON',
     'P_GET_ROLE_FOLLOWUPS','P_GET_SUMMARY','P_GET_ENGAGEMENTS',
     'P_GET_WORKPAPERS','P_GET_OBSERVATIONS')
 ORDER BY object_name,procedure_name;

-- 4. Package compilation errors: MUST be empty before backend integration.
SELECT name,type,line,position,text
  FROM user_errors
 WHERE name IN ('PKG_EXEC_DASHBOARD','PKG_TEAM_LEAD_DASHBOARD')
 ORDER BY name,type,sequence;

-- 5. Check actual IAS assignment eligibility (supply a permitted, known PPNO
-- to your own query; package API must never take this value from browser).
-- SELECT COUNT(DISTINCT e.eng_id)
--   FROM T_AU_AUDIT_TEAM_TASKLIST t
--   JOIN T_AU_TEAM_MEMBERS m
--     ON m.t_id=t.team_id AND m.member_ppno=t.teammember_ppno
--   JOIN T_AU_PLAN_ENG e ON e.eng_id=t.eng_plan_id
--  WHERE t.teammember_ppno=:SESSION_PPNO
--    AND m.isteamlead='Y' AND t.isactive='Y'
--    AND e.status BETWEEN 4 AND 12;

-- 6. Example isolated read-only cursor calls, manually insert verified values:
-- VAR RC REFCURSOR;
-- EXEC PKG_EXEC_DASHBOARD.P_GET_ROLE_ENTITY_HEATMAP('RLM', :SESSION_SCOPE_ROOT, NULL, NULL, 20, :RC);
-- PRINT RC;
-- EXEC PKG_EXEC_DASHBOARD.P_GET_ROLE_UNIT_COMPARISON('GROUP', :SESSION_GROUP_ROOT, NULL, :RC);
-- PRINT RC;
-- EXEC PKG_EXEC_DASHBOARD.P_GET_ROLE_FOLLOWUPS('RLM', :SESSION_SCOPE_ROOT, NULL, 20, :RC);
-- PRINT RC;
-- EXEC PKG_TEAM_LEAD_DASHBOARD.P_GET_SUMMARY(:SESSION_PPNO, :RC);
-- PRINT RC;
-- EXEC PKG_TEAM_LEAD_DASHBOARD.P_GET_ENGAGEMENTS(:SESSION_PPNO, :RC);
-- PRINT RC;
-- EXEC PKG_TEAM_LEAD_DASHBOARD.P_GET_WORKPAPERS(:SESSION_PPNO, NULL, :RC);
-- PRINT RC;
-- EXEC PKG_TEAM_LEAD_DASHBOARD.P_GET_OBSERVATIONS(:SESSION_PPNO, NULL, 20, :RC);
-- PRINT RC;

-- 7. Compare aggregate results with V_EXEC_PARA_BASE for same session root;
-- 8. Confirm unauthorized client root, PPNO and engagement IDs are blocked
-- by API layer; database cursors are not substitutes for session validation.
-- 9. Before production, confirm Team Leader status=3 review state,
--    verified Division/GM/Region role-to-root mapping and working-paper
--    installation; exercise existing CIA/TOP/CCO procedures unchanged.
