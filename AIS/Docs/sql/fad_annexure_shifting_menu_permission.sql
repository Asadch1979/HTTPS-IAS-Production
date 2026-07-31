-- Creates the dedicated page/permission key. Set the two deployment values after
-- confirming the target environment's Field Audit menu and authorized group.
-- This script intentionally does not grant the page to every role.
DEFINE FAD_MENU_ID = 0
DEFINE FAD_ADMIN_GROUP_ID = 0

DECLARE
  v_page_id NUMBER;
BEGIN
  IF &FAD_MENU_ID <= 0 OR &FAD_ADMIN_GROUP_ID <= 0 THEN
    RAISE_APPLICATION_ERROR(
      -20010,
      'Set FAD_MENU_ID and FAD_ADMIN_GROUP_ID to reviewed target-environment IDs.');
  END IF;

  SELECT NVL(MAX(ID), 0)
    INTO v_page_id
    FROM T_MENU_PAGES
   WHERE PAGE_KEY = 'FAD_ANNEXURE_SHIFT_CONFIG';

  IF v_page_id = 0 THEN
    SELECT NVL(MAX(ID), 0) + 1 INTO v_page_id FROM T_MENU_PAGES;
    INSERT INTO T_MENU_PAGES
      (ID, MENU_ID, PAGE_NAME, PAGE_PATH, PAGE_ORDER, STATUS, HIDE_MENU,
       SUB_MENU, PAGE_KEY, PAGE_URL)
    VALUES
      (v_page_id, &FAD_MENU_ID, 'Annexure Shifting Configuration',
       'FadAnnexureConfiguration/AnnexureConfiguration',
       (SELECT NVL(MAX(PAGE_ORDER), 0) + 1
          FROM T_MENU_PAGES WHERE MENU_ID = &FAD_MENU_ID),
       'A', 0, NULL, 'FAD_ANNEXURE_SHIFT_CONFIG',
       '/FadAnnexureConfiguration/AnnexureConfiguration');
  END IF;

  MERGE INTO T_MENU_PAGES_GROUPMAP gm
  USING (SELECT &FAD_ADMIN_GROUP_ID AS GROUP_ID, v_page_id AS PAGE_ID FROM DUAL) src
     ON (gm.GROUP_ID = src.GROUP_ID AND gm.PAGE_ID = src.PAGE_ID)
  WHEN NOT MATCHED THEN
    INSERT (GROUPMAP_ID, GROUP_ID, PAGE_ID)
    VALUES ((SELECT NVL(MAX(GROUPMAP_ID), 0) + 1 FROM T_MENU_PAGES_GROUPMAP),
            src.GROUP_ID, src.PAGE_ID);

  COMMIT;
END;
/
