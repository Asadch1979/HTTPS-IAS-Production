-- Entity Shifting History page registration.
-- Dedicated page id matches the application route mapping in Page ID - Archive.csv.
-- Role grants are copied only from the existing Entity Shifting page.
DECLARE
  C_PAGE_ID CONSTANT NUMBER := 1000218;
  V_SOURCE_PAGE_ID T_MENU_PAGES.ID%TYPE;
  V_COLLISION_COUNT NUMBER;
BEGIN
  SELECT COUNT(*)
    INTO V_COLLISION_COUNT
    FROM T_MENU_PAGES
   WHERE ID = C_PAGE_ID
     AND UPPER(NVL(PAGE_KEY, '#')) <> 'ENTITY_SHIFTING_HISTORY';

  IF V_COLLISION_COUNT > 0 THEN
    RAISE_APPLICATION_ERROR(-20020, 'Page ID 1000218 is already assigned to another page.');
  END IF;

  BEGIN
    SELECT ID
      INTO V_SOURCE_PAGE_ID
      FROM T_MENU_PAGES
     WHERE UPPER(PAGE_KEY) = 'ENTITY_SHIFTING'
        OR UPPER(REPLACE(PAGE_PATH, '\', '/')) =
           'ADMINISTRATIONPANEL/ENTITY_SHIFTING'
     FETCH FIRST 1 ROW ONLY;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      RAISE_APPLICATION_ERROR(-20021, 'Existing Entity Shifting menu page was not found.');
  END;

  MERGE INTO T_MENU_PAGES P
  USING
  (
    SELECT C_PAGE_ID AS ID,
           S.MENU_ID,
           S.SUB_MENU,
           NVL((SELECT MAX(PAGE_ORDER) + 1
                  FROM T_MENU_PAGES
                 WHERE MENU_ID = S.MENU_ID
                   AND NVL(SUB_MENU, -1) = NVL(S.SUB_MENU, -1)), 1) AS PAGE_ORDER
      FROM T_MENU_PAGES S
     WHERE S.ID = V_SOURCE_PAGE_ID
  ) SRC
  ON (P.ID = SRC.ID)
  WHEN MATCHED THEN UPDATE SET
    P.PAGE_NAME = 'Entity Shifting History',
    P.PAGE_PATH = 'AdministrationPanel/EntityShiftingHistory',
    P.STATUS = 'A',
    P.HIDE_MENU = 0,
    P.PAGE_KEY = 'ENTITY_SHIFTING_HISTORY',
    P.PAGE_URL = '/AdministrationPanel/EntityShiftingHistory'
  WHEN NOT MATCHED THEN INSERT
    (ID, MENU_ID, PAGE_NAME, PAGE_PATH, PAGE_ORDER, STATUS, HIDE_MENU,
     SUB_MENU, PAGE_KEY, PAGE_URL)
  VALUES
    (SRC.ID, SRC.MENU_ID, 'Entity Shifting History',
     'AdministrationPanel/EntityShiftingHistory', SRC.PAGE_ORDER, 'A', 0,
     SRC.SUB_MENU, 'ENTITY_SHIFTING_HISTORY',
     '/AdministrationPanel/EntityShiftingHistory');

  MERGE INTO T_MENU_PAGES_GROUPMAP TARGET
  USING
  (
    SELECT SOURCE_GRANTS.GROUP_ID,
           C_PAGE_ID AS PAGE_ID,
           (SELECT NVL(MAX(GROUPMAP_ID), 0) FROM T_MENU_PAGES_GROUPMAP) +
             ROW_NUMBER() OVER (ORDER BY SOURCE_GRANTS.GROUP_ID) AS GROUPMAP_ID
      FROM
      (
        SELECT DISTINCT GROUP_ID
          FROM T_MENU_PAGES_GROUPMAP
         WHERE PAGE_ID = V_SOURCE_PAGE_ID
      ) SOURCE_GRANTS
  ) SRC
  ON (TARGET.GROUP_ID = SRC.GROUP_ID AND TARGET.PAGE_ID = SRC.PAGE_ID)
  WHEN NOT MATCHED THEN INSERT
    (GROUPMAP_ID, GROUP_ID, PAGE_ID)
  VALUES
    (SRC.GROUPMAP_ID, SRC.GROUP_ID, SRC.PAGE_ID);

  COMMIT;
EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/
