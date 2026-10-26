-- Execute manually in UAT after verifying the unused page ID and IAS menu.
-- No approval roles are granted automatically. Assign selected group in IAS admin.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
DECLARE
  V_PAGE_ID NUMBER := 460;
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
  -- Known Super User group: grant only when group 1 / role 1 is confirmed.
  SELECT COUNT(*) INTO V_COUNT FROM T_GROUPS
   WHERE GROUP_ID=1 AND ROLE_ID=1 AND STATUS='Y';
  IF V_COUNT=1 THEN
    SELECT COUNT(*) INTO V_COUNT FROM T_MENU_PAGES_GROUPMAP
     WHERE GROUP_ID=1 AND PAGE_ID=V_PAGE_ID;
    IF V_COUNT=0 THEN
      INSERT INTO T_MENU_PAGES_GROUPMAP(GROUPMAP_ID,GROUP_ID,PAGE_ID)
      SELECT NVL(MAX(GROUPMAP_ID),0)+1,1,V_PAGE_ID
        FROM T_MENU_PAGES_GROUPMAP;
    END IF;
  END IF;
  COMMIT;
END;
/
-- Grant this page through existing IAS menu/group administration
-- only to an explicitly approved authorizing group, after segregation review.
