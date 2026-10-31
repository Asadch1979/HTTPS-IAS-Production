-- Restore the existing legacy-para pages to Execution; retain all group grants.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
BEGIN
  UPDATE T_MENU_PAGES
     SET MENU_ID = 4, PAGE_NAME = 'Add Legacy Para',
         PAGE_PATH = 'Execution/Add_Legacy_Para',
         PAGE_URL = '/Execution/Add_Legacy_Para', STATUS = 'A', HIDE_MENU = 0
   WHERE ID = 192;
  IF SQL%ROWCOUNT <> 1 THEN
    RAISE_APPLICATION_ERROR(-20231, 'Expected existing legacy para page 192.');
  END IF;
  UPDATE T_MENU_PAGES
     SET MENU_ID = 4, PAGE_NAME = 'Authorize Legacy Para Addition',
         PAGE_PATH = 'Execution/Authorize_Adding_Legacy_Para',
         PAGE_URL = '/Execution/Authorize_Adding_Legacy_Para', STATUS = 'A', HIDE_MENU = 0
   WHERE ID = 193;
  IF SQL%ROWCOUNT <> 1 THEN
    RAISE_APPLICATION_ERROR(-20232, 'Expected existing legacy para page 193.');
  END IF;
  COMMIT;
END;
/
-- Users retain existing page 192/193 permissions. Sign in again after deployment.
