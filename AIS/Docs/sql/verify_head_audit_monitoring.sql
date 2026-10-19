-- Read-only DBA checks, after approved installation. Do not run against production without approval.
SELECT object_name,object_type,status FROM user_objects
 WHERE object_name IN ('PKG_HEAD_AUDIT_MONITOR','V_HEAD_AUDIT_ROLE_DEPT','V_HEAD_AUDIT_ENG_SCOPE');
SELECT name,type,line,position,text FROM user_errors WHERE name='PKG_HEAD_AUDIT_MONITOR' ORDER BY sequence;
SELECT g.role_id,g.group_id,g.description,g.status FROM t_groups g WHERE role_id IN (1,5,6,7);
SELECT deptid,entity_id,deptname,status FROM t_audit_departments ORDER BY deptname;
SELECT * FROM v_head_audit_role_dept ORDER BY role_id;
SELECT dept_id,COUNT(*) engagements FROM v_head_audit_eng_scope GROUP BY dept_id;
SELECT id,status FROM t_au_plan_eng_status ORDER BY id;
SELECT statusid,statusname FROM t_au_observation_status ORDER BY statusid;
SELECT id,menu_id,page_name,page_path FROM t_menu_pages WHERE id=1000401;
SELECT * FROM t_menu_pages_groupmap WHERE page_id=1000401;
SELECT a.api_path,a.http_method,a.is_active,p.role_id,p.is_active grant_active
 FROM t_au_api_master a LEFT JOIN t_au_role_api_permission p ON p.api_id=a.api_id
 WHERE a.page_id=1000401 ORDER BY a.api_path,p.role_id;

-- Bind these to an approved test Head's authenticated context and a known foreign engagement.
-- Expected -20152 for each call; role/member/inactive/expired context must return -20151.
-- VARIABLE rc REFCURSOR;
-- EXEC PKG_HEAD_AUDIT_MONITOR.P_GET_WORK(:user_id,:head_role,:dept_id,:context_id,:foreign_eng_id,'Plan',:rc);
-- EXEC PKG_HEAD_AUDIT_MONITOR.P_GET_EVIDENCE(:user_id,:head_role,:dept_id,:context_id,:foreign_eng_id,:file_id,:rc);
-- EXEC PKG_HEAD_AUDIT_MONITOR.P_GET_RESPONSE_EVIDENCE(:user_id,:head_role,:dept_id,:context_id,:foreign_eng_id,:file_id,:rc);
-- EXEC PKG_HEAD_AUDIT_MONITOR.P_GET_REPORT(:user_id,:head_role,:dept_id,:context_id,:foreign_eng_id,:file_id,:rc);
-- Also try a foreign file ID paired with a permitted engagement: empty cursor, never a file.
-- Repeat Work for every section shown on the review page, including Workpapers subcursors.
-- Verify a new saved workpaper/observation is visible before any concluding/finalization action.
