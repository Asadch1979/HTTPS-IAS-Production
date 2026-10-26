-- Execute manually in UAT after verifying the unused page ID and IAS menu.
-- No approval roles are granted automatically. Assign selected group in IAS admin.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
DECLARE
  V_PAGE_ID NUMBER := 1000416;
  V_MENU_ID NUMBER;
  V_COUNT NUMBER;
BEGIN
  SELECT MAX(MENU_ID) INTO V_MENU_ID FROM T_MENU_PAGES
    WHERE LOWER(TRIM(BOTH '/' FROM PAGE_PATH)) IN
      ('planning/planning','planning/tentative_audit_plan');
  IF V_MENU_ID IS NULL THEN
    RAISE_APPLICATION_ERROR(-20231,'Planning menu not registered.');
  END IF;
  SELECT COUNT(*) INTO V_COUNT FROM T_MENU_PAGES
   WHERE (ID=V_PAGE_ID AND LOWER(TRIM(BOTH '/' FROM PAGE_PATH))
             <> 'planning/audit_plan_deletion_authorization')
      OR (ID<>V_PAGE_ID AND LOWER(TRIM(BOTH '/' FROM PAGE_PATH))
             = 'planning/audit_plan_deletion_authorization');
  IF V_COUNT<>0 THEN
     RAISE_APPLICATION_ERROR(-20232,'Page ID or path collision.');
  END IF;
  SELECT COUNT(*) INTO V_COUNT FROM T_MENU_PAGES WHERE ID=V_PAGE_ID;
  IF V_COUNT=0 THEN
    INSERT INTO T_MENU_PAGES
      (ID,MENU_ID,PAGE_NAME,PAGE_PATH,PAGE_ORDER,STATUS,HIDE_MENU,PAGE_KEY,PAGE_URL)
    VALUES(V_PAGE_ID,V_MENU_ID,'Audit Plan Deletion Authorization',
       'Planning/audit_plan_deletion_authorization',82,'A',0,
       'AUDIT_PLAN_DELETION_AUTHORIZATION',
       '/Planning/audit_plan_deletion_authorization');
  END IF;
  COMMIT;
END;
/
-- Grant this page through existing IAS menu/group administration
-- only to an explicitly approved authorizing group, after segregation review.
