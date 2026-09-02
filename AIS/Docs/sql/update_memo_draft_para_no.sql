/*
  Deploys Execution > Update Memo / Draft Para No.
  Run in IAS Oracle schema after adding the matching application changes.
*/

DEFINE EXECUTION_MENU_ID = 0
DEFINE TEAM_LEAD_GROUP_ID = 0

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
);

CREATE SEQUENCE SEQ_AU_MEMO_DRAFT_PARA_LOG START WITH 1 INCREMENT BY 1 NOCACHE;

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

/*
 /* ============================================================
  1. PACKAGE SPECIFICATION
  ============================================================ */

  PROCEDURE P_Get_Memo_Draft_Update_Obs(P_ENG_ID  IN NUMBER,
                                        P_P_NO    IN NUMBER,
                                        P_R_ID    IN NUMBER,
                                        IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_Update_Memo_Draft_Para_No(P_ENG_ID        IN NUMBER,
                                        P_OBS_ID        IN NUMBER,
                                        P_MEMO_NO       IN VARCHAR2,
                                        P_DRAFT_PARA_NO IN VARCHAR2,
                                        P_P_NO          IN NUMBER,
                                        P_R_ID          IN NUMBER,
                                        IO_CURSOR       OUT T_CURSOR);
*/

/*
Add these implementations inside PKG_AR package body before the final END PKG_AR:

 PROCEDURE P_Get_Memo_Draft_Update_Obs(P_ENG_ID  IN NUMBER,
                                        P_P_NO    IN NUMBER,
                                        P_R_ID    IN NUMBER,
                                        IO_CURSOR OUT T_CURSOR) IS
    V_IS_TEAM_LEAD VARCHAR2(1);
  BEGIN
    SELECT NVL(MAX(M.ISTEAMLEAD), 'N')
      INTO V_IS_TEAM_LEAD
      FROM T_AU_TEAM_MEMBERS M
      JOIN T_AU_AUDIT_TEAM_TASKLIST T
        ON T.TEAM_ID = M.T_ID
       AND T.TEAMMEMBER_PPNO = M.MEMBER_PPNO
     WHERE M.MEMBER_PPNO = P_P_NO
       AND T.ENG_PLAN_ID = P_ENG_ID;
  
    IF V_IS_TEAM_LEAD <> 'Y' THEN
      RAISE_APPLICATION_ERROR(-20031,
                              'Selected engagement is not accessible to the assigned Team Lead.');
    END IF;
  
    OPEN IO_CURSOR FOR
      SELECT O.ENGPLANID AS ENG_ID,
             O.ID AS OBS_ID,
             O.MEMO_NUMBER AS MEMO_NO,
             O.DRAFT_PARA_NO AS DRAFT_PARA_NO,
             t.headings AS OBS_TITLE,
             E.NAME AS ENTITY_NAME,
             O.STATUS AS STATUS_ID,
             S.STATUSNAME AS STATUS_NAME,
             CASE
               WHEN O.STATUS = C_STATUS_FINAL OR O.FINAL_PARA_NO IS NOT NULL 
                 THEN
                1
               ELSE
                0
             END AS IS_FINALIZED
        FROM T_AU_OBSERVATION O
       Inner join t_au_observation_text t
          on t.observatsion_id = o.id
        LEFT JOIN T_AUDITEE_ENTITIES E
          ON E.ENTITY_ID = O.ENTITY_ID
        LEFT JOIN T_AU_OBSERVATION_STATUS S
          ON S.STATUSID = O.STATUS
       WHERE O.ENGPLANID = P_ENG_ID
         AND O.DRAFT_PARA_NO IS NOT NULL
         AND O.STATUS <> C_STATUS_FINAL
         AND O.FINAL_PARA_NO IS NULL
       ORDER BY O.DRAFT_PARA_NO, O.MEMO_NUMBER, O.ID;
  END;

  PROCEDURE P_Update_Memo_Draft_Para_No(P_ENG_ID        IN NUMBER,
                                        P_OBS_ID        IN NUMBER,
                                        P_MEMO_NO       IN VARCHAR2,
                                        P_DRAFT_PARA_NO IN VARCHAR2,
                                        P_P_NO          IN NUMBER,
                                        P_R_ID          IN NUMBER,
                                        IO_CURSOR       OUT T_CURSOR) IS
    V_IS_TEAM_LEAD      VARCHAR2(1);
    V_OLD_MEMO_NO       NUMBER;
    V_OLD_DRAFT_PARA_NO NUMBER;
    V_STATUS            NUMBER;
    V_FINAL_PARA_NO     NUMBER;
    V_DUP_COUNT         NUMBER := 0;
  BEGIN
    IF P_ENG_ID IS NULL OR P_ENG_ID <= 0 OR P_OBS_ID IS NULL OR
       P_OBS_ID <= 0 THEN
      RAISE_APPLICATION_ERROR(-20040,
                              'Valid engagement and observation are required.');
    END IF;
  
    IF TRIM(P_MEMO_NO) IS NULL OR TRIM(P_DRAFT_PARA_NO) IS NULL THEN
      RAISE_APPLICATION_ERROR(-20041,
                              'Memo Number and Draft Para Number are required.');
    END IF;
  
    IF NOT REGEXP_LIKE(TRIM(P_MEMO_NO), '^[0-9]+$') THEN
      RAISE_APPLICATION_ERROR(-20042,
                              'Memo Number must contain digits only.');
    END IF;
  
    IF NOT REGEXP_LIKE(TRIM(P_DRAFT_PARA_NO), '^[0-9]+$') THEN
      RAISE_APPLICATION_ERROR(-20043,
                              'Draft Para Number must contain digits only.');
    END IF;
  
    SELECT NVL(MAX(M.ISTEAMLEAD), 'N')
      INTO V_IS_TEAM_LEAD
      FROM T_AU_TEAM_MEMBERS M
      JOIN T_AU_AUDIT_TEAM_TASKLIST T
        ON T.TEAM_ID = M.T_ID
       AND T.TEAMMEMBER_PPNO = M.MEMBER_PPNO
     WHERE M.MEMBER_PPNO = P_P_NO
       AND T.ENG_PLAN_ID = P_ENG_ID;
  
    IF V_IS_TEAM_LEAD <> 'Y' THEN
      RAISE_APPLICATION_ERROR(-20044,
                              'Only the assigned Team Lead can update Memo and Draft Para numbers.');
    END IF;
  
    SELECT O.MEMO_NUMBER, O.DRAFT_PARA_NO, O.STATUS, O.FINAL_PARA_NO
      INTO V_OLD_MEMO_NO, V_OLD_DRAFT_PARA_NO, V_STATUS, V_FINAL_PARA_NO
      FROM T_AU_OBSERVATION O
     WHERE O.ID = P_OBS_ID
       AND O.ENGPLANID = P_ENG_ID
       FOR UPDATE;
  
    IF V_STATUS = C_STATUS_FINAL OR V_FINAL_PARA_NO IS NOT NULL THEN
      RAISE_APPLICATION_ERROR(-20045, 'Finalized paras cannot be updated.');
    END IF;
  
    SELECT COUNT(*)
      INTO V_DUP_COUNT
      FROM T_AU_OBSERVATION O
     WHERE O.ENGPLANID = P_ENG_ID
       AND O.ID <> P_OBS_ID
       AND TRIM(UPPER(O.DRAFT_PARA_NO)) = TRIM(UPPER(P_DRAFT_PARA_NO));
  
    IF V_DUP_COUNT > 0 THEN
      OPEN IO_CURSOR FOR
        SELECT '0' AS REF,
               'Draft Para Number already exists against another observation.' AS REMARKS
          FROM DUAL;
      RETURN;
    END IF;
  
    UPDATE T_AU_OBSERVATION O
       SET O.MEMO_NUMBER         = TO_NUMBER(TRIM(P_MEMO_NO)),
           O.DRAFT_PARA_NO       = TO_NUMBER(TRIM(P_DRAFT_PARA_NO)),
           O.DRAFT_PARA_ADDED_ON = NVL(O.DRAFT_PARA_ADDED_ON, SYSDATE)
     WHERE O.ID = P_OBS_ID
       AND O.ENGPLANID = P_ENG_ID;
  
    INSERT INTO T_AU_MEMO_DRAFT_PARA_LOG
      (ID,
       ENG_ID,
       OBS_ID,
       PREVIOUS_MEMO_NO,
       NEW_MEMO_NO,
       PREVIOUS_DRAFT_PARA_NO,
       NEW_DRAFT_PARA_NO,
       UPDATED_BY_PPNO,
       UPDATED_BY_ROLE_ID,
       UPDATED_ON)
    VALUES
      (SEQ_AU_MEMO_DRAFT_PARA_LOG.NEXTVAL,
       P_ENG_ID,
       P_OBS_ID,
       V_OLD_MEMO_NO,
       TO_NUMBER(TRIM(P_MEMO_NO)),
       V_OLD_DRAFT_PARA_NO,
       TO_NUMBER(TRIM(P_DRAFT_PARA_NO)),
       P_P_NO,
       P_R_ID,
       SYSDATE);
  
    COMMIT;
  
    OPEN IO_CURSOR FOR
      SELECT '1' AS REF,
             'Memo and Draft Para numbers updated successfully.' AS REMARKS
        FROM DUAL;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      ROLLBACK;
      RAISE_APPLICATION_ERROR(-20046,
                              'Observation was not found for the selected engagement.');
    WHEN OTHERS THEN
      ROLLBACK;
      RAISE;
  END;

Then compile the package:

ALTER PACKAGE PKG_AR COMPILE;
ALTER PACKAGE PKG_AR COMPILE BODY;
*/
