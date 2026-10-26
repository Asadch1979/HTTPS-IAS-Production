-- Execute manually in UAT after verifying the unused page ID and IAS menu.
-- Automatically grants the approval page only to verified active IAS role-1 Super User groups.
-- Other authorizer groups must be explicitly reviewed and assigned by IAS administration.
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
  -- IAS role 1 is the verified Super User role. Grant only its active
  -- groups, rather than incorrectly assuming that GROUP_ID must equal 1.
  FOR GR IN (
    SELECT GROUP_ID FROM T_GROUPS
     WHERE ROLE_ID=1 AND STATUS='Y'
  ) LOOP
    SELECT COUNT(*) INTO V_COUNT FROM T_MENU_PAGES_GROUPMAP
     WHERE GROUP_ID=GR.GROUP_ID AND PAGE_ID=V_PAGE_ID;
    IF V_COUNT=0 THEN
      INSERT INTO T_MENU_PAGES_GROUPMAP(GROUPMAP_ID,GROUP_ID,PAGE_ID)
      SELECT NVL(MAX(GROUPMAP_ID),0)+1,GR.GROUP_ID,V_PAGE_ID
        FROM T_MENU_PAGES_GROUPMAP;
    END IF;
  END LOOP;
  COMMIT;
END;
/
-- Grant this page through existing IAS menu/group administration
-- only to an explicitly approved authorizing group, after segregation review.
