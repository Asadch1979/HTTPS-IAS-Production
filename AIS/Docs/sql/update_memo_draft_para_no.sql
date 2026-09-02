/*
  Execution > Update Memo / Draft Para No.

  This release script creates supporting objects and the menu entry.
  The package procedures are incorporated in AIS/Docs/sql/PKG_AR.sql:

    P_Get_Memo_Draft_Update_Obs(P_ENG_ID, P_P_NO, P_R_ID, IO_CURSOR OUT SYS_REFCURSOR)
    P_Update_Memo_Draft_Para_No(P_ENG_ID, P_OBS_ID, P_MEMO_NO, P_DRAFT_PARA_NO,
                                P_P_NO, P_R_ID, P_STATUS OUT, P_REMARKS OUT)

  Set the two deployment values after confirming the target environment IDs.
*/

DEFINE EXECUTION_MENU_ID = 0
DEFINE TEAM_LEAD_GROUP_ID = 0

DECLARE
  V_COUNT NUMBER;
BEGIN
  SELECT COUNT(*)
    INTO V_COUNT
    FROM USER_TABLES
   WHERE TABLE_NAME = 'T_AU_MEMO_DRAFT_PARA_LOG';

  IF V_COUNT = 0 THEN
    EXECUTE IMMEDIATE '
      CREATE TABLE T_AU_MEMO_DRAFT_PARA_LOG
      (
        ID                       NUMBER PRIMARY KEY,
        ENG_ID                   NUMBER NOT NULL,
        OBS_ID                   NUMBER NOT NULL,
        PREVIOUS_MEMO_NO         NUMBER,
        NEW_MEMO_NO              NUMBER NOT NULL,
        PREVIOUS_DRAFT_PARA_NO   NUMBER,
        NEW_DRAFT_PARA_NO        NUMBER NOT NULL,
        UPDATED_BY_PPNO          NUMBER NOT NULL,
        UPDATED_BY_ROLE_ID       NUMBER NOT NULL,
        UPDATED_ON               DATE DEFAULT SYSDATE NOT NULL
      )';
  END IF;
END;
/

DECLARE
  V_COUNT NUMBER;
BEGIN
  SELECT COUNT(*)
    INTO V_COUNT
    FROM USER_SEQUENCES
   WHERE SEQUENCE_NAME = 'SEQ_AU_MEMO_DRAFT_PARA_LOG';

  IF V_COUNT = 0 THEN
    EXECUTE IMMEDIATE 'CREATE SEQUENCE SEQ_AU_MEMO_DRAFT_PARA_LOG START WITH 1 INCREMENT BY 1 NOCACHE';
  END IF;
END;
/

DECLARE
  V_PAGE_ID NUMBER;
BEGIN
  IF &EXECUTION_MENU_ID <= 0 OR &TEAM_LEAD_GROUP_ID <= 0 THEN
    RAISE_APPLICATION_ERROR(
      -20030,
      'Set EXECUTION_MENU_ID and TEAM_LEAD_GROUP_ID to reviewed target-environment IDs.');
  END IF;

  SELECT NVL(MAX(ID), 0)
    INTO V_PAGE_ID
    FROM T_MENU_PAGES
   WHERE PAGE_KEY = 'EXEC_UPDATE_MEMO_DRAFT_PARA_NO'
      OR LOWER(TRIM(PAGE_PATH)) = 'execution/update_memo_draft_para_no';

  IF V_PAGE_ID = 0 THEN
    SELECT NVL(MAX(ID), 0) + 1 INTO V_PAGE_ID FROM T_MENU_PAGES;
    INSERT INTO T_MENU_PAGES
      (ID, MENU_ID, PAGE_NAME, PAGE_PATH, PAGE_ORDER, STATUS, HIDE_MENU,
       SUB_MENU, PAGE_KEY, PAGE_URL)
    VALUES
      (V_PAGE_ID, &EXECUTION_MENU_ID, 'Update Memo / Draft Para No.',
       'Execution/update_memo_draft_para_no',
       (SELECT NVL(MAX(PAGE_ORDER), 0) + 1
          FROM T_MENU_PAGES WHERE MENU_ID = &EXECUTION_MENU_ID),
       'A', 0, NULL, 'EXEC_UPDATE_MEMO_DRAFT_PARA_NO',
       '/Execution/update_memo_draft_para_no');
  ELSE
    UPDATE T_MENU_PAGES
       SET MENU_ID = &EXECUTION_MENU_ID,
           PAGE_NAME = 'Update Memo / Draft Para No.',
           PAGE_PATH = 'Execution/update_memo_draft_para_no',
           STATUS = 'A',
           HIDE_MENU = 0,
           PAGE_KEY = 'EXEC_UPDATE_MEMO_DRAFT_PARA_NO',
           PAGE_URL = '/Execution/update_memo_draft_para_no'
     WHERE ID = V_PAGE_ID;
  END IF;

  MERGE INTO T_MENU_PAGES_GROUPMAP GM
  USING (SELECT &TEAM_LEAD_GROUP_ID AS GROUP_ID, V_PAGE_ID AS PAGE_ID FROM DUAL) SRC
     ON (GM.GROUP_ID = SRC.GROUP_ID AND GM.PAGE_ID = SRC.PAGE_ID)
  WHEN NOT MATCHED THEN
    INSERT (GROUPMAP_ID, GROUP_ID, PAGE_ID)
    VALUES ((SELECT NVL(MAX(GROUPMAP_ID), 0) + 1 FROM T_MENU_PAGES_GROUPMAP),
            SRC.GROUP_ID, SRC.PAGE_ID);

  COMMIT;
END;
/

ALTER PACKAGE PKG_AR COMPILE;
ALTER PACKAGE PKG_AR COMPILE BODY;
