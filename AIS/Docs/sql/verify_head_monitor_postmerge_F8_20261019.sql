-- PR #329: Head Audit Teams Monitoring deployment QA.
-- PL/SQL Developer F8: select/run ONE SELECT at a time. READ ONLY.
-- Source QA is not a substitute for compiling Oracle objects.

-- Package, view compilation:
SELECT object_name,object_type,status
  FROM user_objects
 WHERE object_name IN ('V_HEAD_AUDIT_ROLE_DEPT',
                       'V_HEAD_AUDIT_ENG_SCOPE','PKG_HEAD_AUDIT_MONITOR')
 ORDER BY object_name,object_type;

SELECT name,type,line,position,text
  FROM user_errors
 WHERE name IN ('V_HEAD_AUDIT_ROLE_DEPT','V_HEAD_AUDIT_ENG_SCOPE',
                'PKG_HEAD_AUDIT_MONITOR')
 ORDER BY name,sequence;

-- Confirm role-to-audit-department authorization, not inferred from descriptions:
SELECT role_id,dept_id,deptname
  FROM v_head_audit_role_dept
 ORDER BY role_id;

-- Confirm exact engagement statuses that the current dashboard calls ongoing.
-- In particular, check stage 13 (Finalized) before counting it as conducting audit.
SELECT e.status, s.status AS status_name, COUNT(*) AS engagements
  FROM t_au_plan_eng e
  LEFT JOIN t_au_plan_eng_status s ON s.id=e.status
 WHERE e.status BETWEEN 4 AND 13
 GROUP BY e.status,s.status
 ORDER BY e.status;

-- Check working-paper / downloaded evidence data storage types;
-- C# currently expects BLOB for legacy and base64 text for report/response.
SELECT table_name,column_name,data_type,data_length
  FROM user_tab_columns
 WHERE (table_name='T_AU_OBSERVATION_EVIDENCES' AND column_name='FILE_DATA')
    OR (table_name='T_AU_OBSERVATIONS_AUDITEE_EVIDENCES' AND column_name='FILE_DATA')
    OR (table_name='T_AUDIT_REPORTS' AND column_name='AUDIT_REPORT')
 ORDER BY table_name,column_name;

-- New page and API permissions (no changes to operational pages).
SELECT m.id,m.page_name,m.page_path,m.status,g.group_id
  FROM t_menu_pages m
  LEFT JOIN t_menu_pages_groupmap g ON g.page_id=m.id
 WHERE m.id=1000401
 ORDER BY g.group_id;

SELECT a.api_path,a.http_method,a.is_active AS api_active,
       p.role_id,p.is_active AS role_api_active
  FROM t_au_api_master a
  LEFT JOIN t_au_role_api_permission p ON p.api_id=a.api_id
 WHERE a.page_id=1000401
 ORDER BY a.api_path,p.role_id;
