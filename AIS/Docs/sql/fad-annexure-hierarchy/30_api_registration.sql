-- DATA ONLY. Reuse the existing Index page anchor, never invent a page ID.
-- New API remains inactive; IAS administrators assign existing intended rights.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
DECLARE p NUMBER; n NUMBER;
BEGIN
 SELECT id INTO p FROM t_menu_pages
 WHERE LOWER(TRIM(BOTH '/' FROM page_path))='fadriskcontrolpanel/index';
 SELECT COUNT(*) INTO n FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/registerhierarchy'
 AND UPPER(http_method)='GET' AND (page_id IS NULL OR page_id<>p);
 IF n>0 THEN RAISE_APPLICATION_ERROR(-20103,'RegisterHierarchy API page anchor conflicts'); END IF;
 INSERT INTO t_au_api_master(action_name,page_id,controller_name,api_path,http_method,is_active,created_on)
 SELECT 'RegisterHierarchy',p,'FADRiskControlPanel','/FADRiskControlPanel/RegisterHierarchy','GET','N',SYSDATE
 FROM dual WHERE NOT EXISTS (SELECT 1 FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/registerhierarchy' AND UPPER(http_method)='GET');
 COMMIT;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
