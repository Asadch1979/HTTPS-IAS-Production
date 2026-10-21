-- F8: execute this file alone in PL/SQL Developer SQL Window.
-- Manual deployment. Move the existing page only; create no page or API grants.

DECLARE
  v_menu NUMBER;
  v_count NUMBER;
BEGIN
  SELECT COUNT(DISTINCT menu_id), MIN(menu_id) INTO v_count,v_menu
  FROM t_menu_pages
  WHERE LOWER(TRIM(BOTH '/' FROM TRIM(page_path)))='dashboard/entity_wise_obs';
  IF v_count<>1 OR v_menu IS NULL THEN
    RAISE_APPLICATION_ERROR(-20156,'Dashboard menu missing or ambiguous.');
  END IF;
  SELECT COUNT(*) INTO v_count FROM t_menu_pages WHERE id<>1000401 AND
    LOWER(TRIM(BOTH '/' FROM TRIM(page_path))) IN
      ('headauditdepartment/auditteamsmonitoring','headauditdepartment/review');
  IF v_count<>0 THEN RAISE_APPLICATION_ERROR(-20156,'Conflicting monitoring page registration; review manually.'); END IF;
  UPDATE t_menu_pages SET menu_id=v_menu, sub_menu=NULL, page_name='Head of Audit Dashboard',
    page_path='HeadAuditDepartment/AuditTeamsMonitoring',
    page_url='/HeadAuditDepartment/AuditTeamsMonitoring',
    page_key='HeadAuditDepartment/AuditTeamsMonitoring'
  WHERE id=1000401 AND LOWER(TRIM(BOTH '/' FROM TRIM(page_path)))=
    CASE LOWER(TRIM(BOTH '/' FROM TRIM(page_path)))
      WHEN 'headauditdepartment/auditteamsmonitoring' THEN 'headauditdepartment/auditteamsmonitoring'
      WHEN 'headauditdepartment/review' THEN 'headauditdepartment/review' END;
  IF SQL%ROWCOUNT<>1 THEN
    RAISE_APPLICATION_ERROR(-20156,'Existing monitoring page registration missing or mismatched.');
  END IF;
END;
