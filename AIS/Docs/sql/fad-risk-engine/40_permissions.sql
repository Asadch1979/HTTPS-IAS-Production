-- DATA ONLY. Reuse the existing Index page anchor, never invent a page ID.
-- New API remains inactive; IAS administrators assign existing intended rights.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
DECLARE p NUMBER; n NUMBER;
BEGIN
 SELECT id INTO p FROM t_menu_pages
 WHERE LOWER(TRIM(BOTH '/' FROM page_path))='fadriskcontrolpanel/index';
 SELECT COUNT(*) INTO n FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/enginecatalog'
 AND UPPER(http_method)='GET' AND (page_id IS NULL OR page_id<>p);
 IF n>0 THEN RAISE_APPLICATION_ERROR(-20103,'EngineCatalog API page anchor conflicts'); END IF;
 INSERT INTO t_au_api_master(action_name,page_id,controller_name,api_path,http_method,is_active,created_on)
 SELECT 'EngineCatalog',p,'FADRiskControlPanel','/FADRiskControlPanel/EngineCatalog','GET','N',SYSDATE
 FROM dual WHERE NOT EXISTS (SELECT 1 FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/enginecatalog' AND UPPER(http_method)='GET');
 COMMIT;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/

-- DATA ONLY. Reuse the existing Index page anchor, never invent a page ID.
-- New API remains inactive; IAS administrators assign existing intended rights.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
DECLARE p NUMBER; n NUMBER;
BEGIN
 SELECT id INTO p FROM t_menu_pages
 WHERE LOWER(TRIM(BOTH '/' FROM page_path))='fadriskcontrolpanel/index';
 SELECT COUNT(*) INTO n FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/engineresults'
 AND UPPER(http_method)='GET' AND (page_id IS NULL OR page_id<>p);
 IF n>0 THEN RAISE_APPLICATION_ERROR(-20103,'EngineResults API page anchor conflicts'); END IF;
 INSERT INTO t_au_api_master(action_name,page_id,controller_name,api_path,http_method,is_active,created_on)
 SELECT 'EngineResults',p,'FADRiskControlPanel','/FADRiskControlPanel/EngineResults','GET','N',SYSDATE
 FROM dual WHERE NOT EXISTS (SELECT 1 FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/engineresults' AND UPPER(http_method)='GET');
 COMMIT;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/

-- DATA ONLY. Reuse the existing Index page anchor, never invent a page ID.
-- New API remains inactive; IAS administrators assign existing intended rights.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
DECLARE p NUMBER; n NUMBER;
BEGIN
 SELECT id INTO p FROM t_menu_pages
 WHERE LOWER(TRIM(BOTH '/' FROM page_path))='fadriskcontrolpanel/index';
 SELECT COUNT(*) INTO n FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/runengineperiod'
 AND UPPER(http_method)='POST' AND (page_id IS NULL OR page_id<>p);
 IF n>0 THEN RAISE_APPLICATION_ERROR(-20103,'RunEnginePeriod API page anchor conflicts'); END IF;
 INSERT INTO t_au_api_master(action_name,page_id,controller_name,api_path,http_method,is_active,created_on)
 SELECT 'RunEnginePeriod',p,'FADRiskControlPanel','/FADRiskControlPanel/RunEnginePeriod','POST','N',SYSDATE
 FROM dual WHERE NOT EXISTS (SELECT 1 FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/runengineperiod' AND UPPER(http_method)='POST');
 COMMIT;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/

-- DATA ONLY. Reuse the existing Index page anchor, never invent a page ID.
-- New API remains inactive; IAS administrators assign existing intended rights.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
DECLARE p NUMBER; n NUMBER;
BEGIN
 SELECT id INTO p FROM t_menu_pages
 WHERE LOWER(TRIM(BOTH '/' FROM page_path))='fadriskcontrolpanel/index';
 SELECT COUNT(*) INTO n FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/runengineentity'
 AND UPPER(http_method)='POST' AND (page_id IS NULL OR page_id<>p);
 IF n>0 THEN RAISE_APPLICATION_ERROR(-20103,'RunEngineEntity API page anchor conflicts'); END IF;
 INSERT INTO t_au_api_master(action_name,page_id,controller_name,api_path,http_method,is_active,created_on)
 SELECT 'RunEngineEntity',p,'FADRiskControlPanel','/FADRiskControlPanel/RunEngineEntity','POST','N',SYSDATE
 FROM dual WHERE NOT EXISTS (SELECT 1 FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM api_path))='fadriskcontrolpanel/runengineentity' AND UPPER(http_method)='POST');
 COMMIT;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
