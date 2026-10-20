-- Manual deployment. Move the existing page only; create no page or API grants.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
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
  UPDATE t_menu_pages SET menu_id=v_menu, sub_menu=NULL, page_name='Audit Teams Monitoring'
  WHERE id=1000401 AND LOWER(TRIM(BOTH '/' FROM TRIM(page_path)))=
    'headauditdepartment/auditteamsmonitoring';
  IF SQL%ROWCOUNT<>1 THEN
    RAISE_APPLICATION_ERROR(-20156,'Existing monitoring page registration missing or mismatched.');
  END IF;
END;
/
COMMIT;
