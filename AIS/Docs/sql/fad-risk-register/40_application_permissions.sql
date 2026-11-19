-- SOURCE ONLY. DBA review required. Never run at application startup.
-- Supply an allocated unused page ID and the EXISTING FAD menu ID.
-- Existing grants/activation are never changed; new APIs default inactive.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
DEFINE risk_page_id = &allocated_risk_page_id
DEFINE fad_menu_id = &existing_fad_menu_id
DECLARE
 p NUMBER := &risk_page_id; m NUMBER := &fad_menu_id; n NUMBER;
 PROCEDURE api(action_name VARCHAR2,verb VARCHAR2) IS
  path VARCHAR2(300):='/FADRiskControlPanel/'||action_name;
 BEGIN
  SELECT COUNT(*) INTO n FROM t_au_api_master WHERE LOWER(TRIM(BOTH '/' FROM api_path))=LOWER(TRIM(BOTH '/' FROM path)) AND UPPER(http_method)=verb AND page_id<>p;
  IF n>0 THEN RAISE_APPLICATION_ERROR(-20040,'Existing API uses another page anchor; review before registration.'); END IF;
  INSERT INTO t_au_api_master(action_name,page_id,controller_name,api_path,http_method,is_active,created_on)
   SELECT action_name,p,'FADRiskControlPanel',path,verb,'N',SYSDATE FROM dual WHERE NOT EXISTS
   (SELECT 1 FROM t_au_api_master WHERE LOWER(TRIM(BOTH '/' FROM api_path))=LOWER(TRIM(BOTH '/' FROM path)) AND UPPER(http_method)=verb);
 END;
BEGIN
 SELECT COUNT(*) INTO n FROM t_menu WHERE menu_id=m;
 IF n<>1 OR p<=0 THEN RAISE_APPLICATION_ERROR(-20040,'Verify allocated page and existing FAD menu.'); END IF;
 SELECT COUNT(*) INTO n FROM t_menu_pages WHERE id=p AND LOWER(TRIM(BOTH '/' FROM page_path))<>'fadriskcontrolpanel/assessment';
 IF n>0 THEN RAISE_APPLICATION_ERROR(-20040,'Page ID collision.'); END IF;
 SELECT COUNT(*) INTO n FROM t_menu_pages WHERE id<>p AND LOWER(TRIM(BOTH '/' FROM page_path))='fadriskcontrolpanel/assessment';
 IF n>0 THEN RAISE_APPLICATION_ERROR(-20040,'Route already registered with another page ID.'); END IF;
 INSERT INTO t_menu_pages(id,menu_id,page_name,page_path,page_order,status,hide_menu,page_key,page_url)
  SELECT p,m,'FAD Risk Assessment','FADRiskControlPanel/Assessment',90,'A',1,'FADRiskControlPanel/Assessment','/FADRiskControlPanel/Assessment'
  FROM dual WHERE NOT EXISTS(SELECT 1 FROM t_menu_pages WHERE id=p);
 api('AssessmentCatalog','GET'); api('ReadAssessment','GET');
 api('SaveAssessment','POST'); api('TransitionAssessment','POST');
 -- No page/group assignments, role/API grants, risk-access grants or activation.
 -- Review and apply these explicitly via existing IAS administration, then commit.
END;
/
