-- Manual DBA review only. No production execution is authorized by this commit.
-- Install PKG_HEAD_AUDIT_MONITOR.sql first. This registers only the dedicated Head page/APIs.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON;
DECLARE
  n NUMBER;
  v_menu_id NUMBER;
  v_api_id NUMBER;
  PROCEDURE grant_page(role_id NUMBER) AS
  BEGIN
    INSERT INTO t_menu_pages_groupmap(groupmap_id,group_id,page_id)
    SELECT (SELECT NVL(MAX(groupmap_id),0)+1 FROM t_menu_pages_groupmap),role_id,1000401
      FROM dual WHERE NOT EXISTS (SELECT 1 FROM t_menu_pages_groupmap g WHERE g.group_id=role_id AND g.page_id=1000401);
  END;
BEGIN
  LOCK TABLE t_menu IN EXCLUSIVE MODE;
  LOCK TABLE t_menu_pages IN EXCLUSIVE MODE;
  LOCK TABLE t_menu_pages_groupmap IN EXCLUSIVE MODE;
  LOCK TABLE t_au_api_master IN EXCLUSIVE MODE;
  LOCK TABLE t_au_role_api_permission IN EXCLUSIVE MODE;

  -- Every enabled Head role must have exactly one verified audit department.
  -- A mismatch stops registration; inspect actual role/department names instead of guessing IDs.
  FOR r IN (SELECT 5 role_id FROM dual UNION ALL SELECT 6 FROM dual UNION ALL SELECT 7 FROM dual) LOOP
    SELECT COUNT(*) INTO n FROM v_head_audit_role_dept d JOIN t_groups g ON g.role_id=d.role_id
     WHERE d.role_id=r.role_id AND g.group_id=r.role_id;
    IF n<>1 THEN RAISE_APPLICATION_ERROR(-20153,'Verify role/group and audit department mapping for Head role '||r.role_id); END IF;
  END LOOP;
  SELECT COUNT(*) INTO n FROM t_groups WHERE role_id=1 AND group_id=1 AND status='Y' AND UPPER(TRIM(group_name))='SUPER USER';
  IF n<>1 THEN RAISE_APPLICATION_ERROR(-20153,'Verify Super User role/group 1.'); END IF;
  SELECT COUNT(*) INTO n FROM t_menu_pages WHERE
    (id=1000401 AND (NVL(page_key,'#')<>'HEAD_AUDIT_TEAMS_MONITORING' OR LOWER(TRIM(BOTH '/' FROM page_path))<>'headauditdepartment/auditteamsmonitoring')) OR
    (id<>1000401 AND (page_key='HEAD_AUDIT_TEAMS_MONITORING' OR LOWER(TRIM(BOTH '/' FROM page_path))='headauditdepartment/auditteamsmonitoring'));
  IF n<>0 THEN RAISE_APPLICATION_ERROR(-20154,'Head dashboard page ID/path collision.'); END IF;

  SELECT COUNT(*),MIN(m.menu_id) INTO n,v_menu_id FROM t_menu m WHERE m.menu_name='Head Audit Department';
  IF n>1 THEN RAISE_APPLICATION_ERROR(-20154,'Duplicate Head Audit Department menus.'); END IF;
  IF n=0 THEN
    SELECT NVL(MAX(m.menu_id),0)+1 INTO v_menu_id FROM t_menu m;
    INSERT INTO t_menu(menu_id,menu_name,menu_order,menu_description,isactive)
      VALUES(v_menu_id,'Head Audit Department',90,'Department audit teams monitoring','Y');
  END IF;
  INSERT INTO t_menu_pages(id,menu_id,page_name,page_path,page_order,status,hide_menu,page_key,page_url)
  SELECT 1000401,v_menu_id,'Audit Teams Monitoring','HeadAuditDepartment/AuditTeamsMonitoring',1,'A',0,
    'HEAD_AUDIT_TEAMS_MONITORING','/HeadAuditDepartment/AuditTeamsMonitoring' FROM dual
    WHERE NOT EXISTS (SELECT 1 FROM t_menu_pages WHERE id=1000401);

  -- Remove any accidental grants on this new page only; operational permissions remain intact.
  DELETE FROM t_menu_pages_groupmap WHERE page_id=1000401 AND group_id NOT IN (1,5,6,7);
  FOR r IN (SELECT 1 role_id FROM dual UNION ALL SELECT role_id FROM v_head_audit_role_dept) LOOP
    grant_page(r.role_id);
  END LOOP;
  FOR a IN (SELECT 'Engagements' action FROM dual UNION ALL SELECT 'Work' FROM dual UNION ALL
            SELECT 'Evidence' FROM dual UNION ALL SELECT 'Report' FROM dual UNION ALL SELECT 'ResponseEvidence' FROM dual) LOOP
    SELECT COUNT(*),MIN(m.api_id) INTO n,v_api_id FROM t_au_api_master m
     WHERE LOWER(api_path)=LOWER('/api/HeadAuditDepartment/'||a.action) AND UPPER(http_method)='GET';
    IF n>1 THEN RAISE_APPLICATION_ERROR(-20154,'Duplicate Head monitoring API.'); END IF;
    IF n=0 THEN
      INSERT INTO t_au_api_master(action_name,page_id,controller_name,api_path,http_method,is_active,created_on)
        VALUES(a.action,1000401,'HeadAuditDepartment','/api/HeadAuditDepartment/'||a.action,'GET','Y',SYSDATE);
      SELECT m.api_id INTO v_api_id FROM t_au_api_master m WHERE LOWER(api_path)=LOWER('/api/HeadAuditDepartment/'||a.action) AND UPPER(http_method)='GET';
    ELSE
      SELECT COUNT(*) INTO n FROM t_au_api_master m WHERE m.api_id=v_api_id AND page_id=1000401;
      IF n<>1 THEN RAISE_APPLICATION_ERROR(-20154,'Head API registered to another page.'); END IF;
    END IF;
    UPDATE t_au_role_api_permission p SET is_active='N' WHERE p.api_id=v_api_id AND p.role_id NOT IN (1,5,6,7);
    FOR r IN (SELECT 1 role_id FROM dual UNION ALL SELECT role_id FROM v_head_audit_role_dept) LOOP
      INSERT INTO t_au_role_api_permission(role_id,api_id,is_active,created_by)
      SELECT r.role_id,v_api_id,'Y',NULL FROM dual WHERE NOT EXISTS
        (SELECT 1 FROM t_au_role_api_permission p WHERE p.role_id=r.role_id AND p.api_id=v_api_id);
    END LOOP;
  END LOOP;
  COMMIT;
END;
/
