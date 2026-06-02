CREATE OR REPLACE PACKAGE PKG_USER_CONTEXT AS
  TYPE T_CURSOR IS REF CURSOR;

  PROCEDURE P_GET_USER_BASE(PPNumber IN NUMBER,
                            enc_pass IN VARCHAR2,
                            IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_GET_USER_CONTEXTS(P_USER_ID IN NUMBER,
                                IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_GET_USER_CONTEXTS_BY_PPNO(P_PPNO    IN NUMBER,
                                        IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_GET_DEFAULT_USER_CONTEXT(P_USER_ID         IN NUMBER,
                                       O_USER_CONTEXT_ID OUT NUMBER,
                                       O_ROLE_ID         OUT NUMBER,
                                       O_GROUP_ID        OUT NUMBER,
                                       O_ENTITY_ID       OUT NUMBER,
                                       O_STATUS          OUT VARCHAR2,
                                       O_MESSAGE         OUT VARCHAR2);

  PROCEDURE P_VALIDATE_USER_CONTEXT(P_PPNO            IN NUMBER,
                                    P_USER_CONTEXT_ID IN NUMBER,
                                    IO_CURSOR         OUT T_CURSOR);

  PROCEDURE P_SAVE_USER_CONTEXT_ASSIGNMENT(P_USER_CONTEXT_ID IN OUT NUMBER,
                                           P_USER_ID         IN NUMBER,
                                           P_PPNO            IN NUMBER,
                                           P_ROLE_ID         IN NUMBER,
                                           P_GROUP_ID        IN NUMBER,
                                           P_ENTITY_ID       IN NUMBER,
                                           P_IS_DEFAULT      IN CHAR,
                                           P_IS_ACTIVE       IN CHAR,
                                           P_ASSIGNMENT_TYPE IN VARCHAR2,
                                           P_EFFECTIVE_FROM  IN DATE,
                                           P_EFFECTIVE_TO    IN DATE,
                                           P_REMARKS         IN VARCHAR2,
                                           P_ACTION_BY       IN VARCHAR2,
                                           O_STATUS          OUT VARCHAR2,
                                           O_MESSAGE         OUT VARCHAR2);

  PROCEDURE P_SET_DEFAULT_USER_CONTEXT(P_USER_ID         IN NUMBER,
                                       P_USER_CONTEXT_ID IN NUMBER,
                                       P_ACTION_BY       IN VARCHAR2,
                                       O_STATUS          OUT VARCHAR2,
                                       O_MESSAGE         OUT VARCHAR2);

  PROCEDURE P_DISABLE_USER_CONTEXT_ASSIGNMENT(P_USER_CONTEXT_ID IN NUMBER,
                                              P_ACTION_BY       IN VARCHAR2,
                                              O_STATUS          OUT VARCHAR2,
                                              O_MESSAGE         OUT VARCHAR2);

  PROCEDURE P_GET_USER_CONTEXT_ASSIGNMENTS(P_USER_ID IN NUMBER,
                                           P_PPNO    IN NUMBER,
                                           IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_SEARCH_USERS_WITH_CONTEXT(P_ENTITY_ID IN NUMBER,
                                        P_EMAIL     IN VARCHAR2,
                                        P_GROUP_ID  IN NUMBER,
                                        P_PPNO      IN NUMBER,
                                        P_LOGINNAME IN VARCHAR2,
                                        IO_CURSOR   OUT T_CURSOR);

  PROCEDURE P_SYNC_USER_DEFAULT_CONTEXT(P_USER_ID   IN NUMBER,
                                        P_ACTION_BY IN VARCHAR2,
                                        O_STATUS    OUT VARCHAR2,
                                        O_MESSAGE   OUT VARCHAR2);

  PROCEDURE P_BACKFILL_USER_CONTEXTS(P_CREATED_BY IN VARCHAR2,
                                     O_STATUS     OUT VARCHAR2,
                                     O_MESSAGE    OUT VARCHAR2);

  PROCEDURE P_SAVE_USER_ACCOUNT(P_USER_ID       IN OUT NUMBER,
                                P_PPNO          IN NUMBER,
                                P_PASSWORD      IN VARCHAR2,
                                P_EMAIL_ADDRESS IN VARCHAR2,
                                P_ISACTIVE      IN VARCHAR2,
                                P_ACTION_BY     IN VARCHAR2,
                                O_STATUS        OUT VARCHAR2,
                                O_MESSAGE       OUT VARCHAR2);
END PKG_USER_CONTEXT;

CREATE OR REPLACE PACKAGE BODY PKG_USER_CONTEXT AS

    PROCEDURE P_WRITE_HISTORY
    (
        P_USER_CONTEXT_ID   IN NUMBER,
        P_ACTION_TYPE       IN VARCHAR2,
        P_ACTION_BY         IN VARCHAR2
    )
    IS
    BEGIN
        INSERT INTO T_USER_CONTEXT_ASSIGNMENT_HIST
        (
            HIST_ID, USER_CONTEXT_ID, USER_ID, PPNO, ROLE_ID, GROUP_ID, ENTITY_ID,
            IS_DEFAULT, IS_ACTIVE, ASSIGNMENT_TYPE, EFFECTIVE_FROM, EFFECTIVE_TO,
            REMARKS, ACTION_TYPE, ACTION_BY, ACTION_ON
        )
        SELECT SEQ_T_USER_CONTEXT_ASSIGNMENT_HIST.NEXTVAL,
               USER_CONTEXT_ID, USER_ID, PPNO, ROLE_ID, GROUP_ID, ENTITY_ID,
               IS_DEFAULT, IS_ACTIVE, ASSIGNMENT_TYPE, EFFECTIVE_FROM, EFFECTIVE_TO,
               REMARKS, P_ACTION_TYPE, NVL(P_ACTION_BY, 'SYSTEM'), SYSDATE
          FROM T_USER_CONTEXT_ASSIGNMENT
         WHERE USER_CONTEXT_ID = P_USER_CONTEXT_ID;
    END P_WRITE_HISTORY;

    PROCEDURE P_GET_USER_BASE
    (
        PPNumber  IN NUMBER,
        enc_pass  IN VARCHAR2,
        IO_CURSOR  OUT T_CURSOR
    )
    IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT U.PASSWORD_CHANGE_REQ,
                   U.USERID,
                   HR.EMPLOYEEFIRSTNAME,
                   HR.EMPLOYEELASTNAME,
                   U.LOGIN_NAME,
                   U.PPNO,
                   U.USER_LOCATION_TYPE,
                   U.ISACTIVE,
                   U.DIVISIONID,
                   U.DEPARTMENTID,
                   U.ZONEID,
                   U.BRANCHID,
                   U.AUDIT_ZONEID
              FROM T_USER U
              LEFT JOIN TBL_T_HR_EMPLOYEEINFORMATION HR
                ON HR.PPNO = U.PPNO
             WHERE U.PPNO = PPNumber
               AND U.PASSWORD = enc_pass
               AND NVL(U.ISACTIVE,'Y') = 'Y';
    END P_GET_USER_BASE;

    PROCEDURE P_OPEN_CONTEXT_CURSOR
    (
        P_USER_ID          IN NUMBER,
        P_PPNO             IN NUMBER,
        P_USER_CONTEXT_ID  IN NUMBER,
        P_ACTIVE_ONLY      IN CHAR,
        IO_CURSOR          OUT T_CURSOR
    )
    IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT UCA.USER_CONTEXT_ID,
                   UCA.USER_CONTEXT_ID AS ASSIGNMENT_ID,
                   UCA.USER_ID AS USERID,
                   UCA.PPNO,
                   UCA.ROLE_ID,
                   NVL(UCA.GROUP_ID, UCA.ROLE_ID) AS GROUP_ID,
                   G.GROUP_NAME,
                   UCA.ENTITY_ID,
                   COALESCE(M.C_NAME, AE.NAME) AS ENT_NAME,
                   M.PARENT_ID AS PARENT_ENTITY_ID,
                   M.P_NAME AS PARENT_ENTITY_NAME,
                   M.RELATION_TYPE_ID AS RELATIONSHIP_TYPE_ID,
                   CAST(NULL AS VARCHAR2(200)) AS RELATIONSHIP_TYPE_NAME,
                   NVL(M.C_TYPE_ID, AE.TYPE_ID) AS ENTITY_TYPE_ID,
                   M.P_TYPE_ID AS PARENT_ENTITY_TYPE_ID,
                   NVL(M.CHILD_CODE, AE.CODE) AS ENTITY_CODE,
                   M.PARENT_CODE AS PARENT_ENTITY_CODE,
                   UCA.IS_DEFAULT,
                   UCA.IS_DEFAULT AS IS_DEFAULT_ASSIGNMENT,
                   UCA.IS_ACTIVE,
                   UCA.IS_ACTIVE AS IS_ACTIVE_ASSIGNMENT,
                   CASE WHEN NVL(M.C_TYPE_ID, AE.TYPE_ID) = 6 THEN M.PARENT_CODE ELSE NULL END AS USER_POSTING_ZONE,
                   CASE WHEN NVL(M.C_TYPE_ID, AE.TYPE_ID) = 6 THEN M.PARENT_CODE ELSE NULL END AS POSTING_ZONE,
                   CASE WHEN NVL(M.C_TYPE_ID, AE.TYPE_ID) = 6 THEN NVL(M.CHILD_CODE, AE.CODE) ELSE NULL END AS USER_POSTING_BRANCH,
                   CASE WHEN NVL(M.C_TYPE_ID, AE.TYPE_ID) = 6 THEN NVL(M.CHILD_CODE, AE.CODE) ELSE NULL END AS POSTING_BRANCH,
                   CASE WHEN NVL(M.C_TYPE_ID, AE.TYPE_ID) = 6 THEN NULL ELSE M.PARENT_CODE END AS USER_POSTING_DIV,
                   CASE WHEN NVL(M.C_TYPE_ID, AE.TYPE_ID) = 6 THEN NULL ELSE M.PARENT_CODE END AS POSTING_DIV,
                   CASE WHEN NVL(M.C_TYPE_ID, AE.TYPE_ID) = 6 THEN NULL ELSE NVL(M.CHILD_CODE, AE.CODE) END AS USER_POSTING_DEPT,
                   CASE WHEN NVL(M.C_TYPE_ID, AE.TYPE_ID) = 6 THEN NULL ELSE NVL(M.CHILD_CODE, AE.CODE) END AS POSTING_DEPT,
                   CAST(NULL AS NUMBER) AS USER_POSTING_AUDIT_ZONE,
                   CAST(NULL AS NUMBER) AS POSTING_AZ,
                   CASE WHEN NVL(M.C_TYPE_ID, AE.TYPE_ID) = 6 THEN 'BRANCH' ELSE 'DEPARTMENT' END AS USER_LOCATION_TYPE,
                   UCA.ASSIGNMENT_TYPE,
                   UCA.EFFECTIVE_FROM,
                   UCA.EFFECTIVE_TO,
                   UCA.REMARKS
              FROM T_USER_CONTEXT_ASSIGNMENT UCA
              LEFT JOIN (
                    SELECT GROUP_ID, MAX(ROLE_ID) AS ROLE_ID, MAX(GROUP_NAME) AS GROUP_NAME
                      FROM T_GROUPS
                     GROUP BY GROUP_ID
              ) G
                ON G.GROUP_ID = NVL(UCA.GROUP_ID, UCA.ROLE_ID)
              LEFT JOIN T_AUDITEE_ENTITIES AE
                ON AE.ENTITY_ID = UCA.ENTITY_ID
              LEFT JOIN (
                    SELECT *
                      FROM (
                            SELECT EM.*,
                                   ROW_NUMBER() OVER
                                   (
                                       PARTITION BY EM.ENTITY_ID
                                       ORDER BY CASE WHEN NVL(EM.STATUS,'Y') IN ('Y','A','1') THEN 0 ELSE 1 END,
                                                EM.RELATION_TYPE_ID,
                                                EM.PARENT_ID
                                   ) AS RN
                              FROM T_AUDITEE_ENTITIES_MAPING EM
                           )
                     WHERE RN = 1
              ) M
                ON M.ENTITY_ID = UCA.ENTITY_ID
             WHERE (P_USER_ID IS NULL OR UCA.USER_ID = P_USER_ID)
               AND (P_PPNO IS NULL OR UCA.PPNO = P_PPNO)
               AND (P_USER_CONTEXT_ID IS NULL OR UCA.USER_CONTEXT_ID = P_USER_CONTEXT_ID)
               AND (NVL(P_ACTIVE_ONLY,'N') <> 'Y'
                    OR (
                        UCA.IS_ACTIVE = 'Y'
                        AND (UCA.EFFECTIVE_FROM IS NULL OR UCA.EFFECTIVE_FROM <= TRUNC(SYSDATE))
                        AND (UCA.EFFECTIVE_TO   IS NULL OR UCA.EFFECTIVE_TO   >= TRUNC(SYSDATE))
                    ))
             ORDER BY CASE WHEN UCA.IS_DEFAULT = 'Y' THEN 0 ELSE 1 END,
                      G.GROUP_NAME,
                      COALESCE(M.C_NAME, AE.NAME);
    END P_OPEN_CONTEXT_CURSOR;

    PROCEDURE P_GET_USER_CONTEXTS
    (
        P_USER_ID    IN NUMBER,
        IO_CURSOR    OUT T_CURSOR
    )
    IS
    BEGIN
        P_OPEN_CONTEXT_CURSOR(P_USER_ID, NULL, NULL, 'N', IO_CURSOR);
    END P_GET_USER_CONTEXTS;

    PROCEDURE P_GET_USER_CONTEXTS_BY_PPNO
    (
        P_PPNO       IN NUMBER,
        IO_CURSOR    OUT T_CURSOR
    )
    IS
    BEGIN
        P_OPEN_CONTEXT_CURSOR(NULL, P_PPNO, NULL, 'Y', IO_CURSOR);
    END P_GET_USER_CONTEXTS_BY_PPNO;

    PROCEDURE P_GET_DEFAULT_USER_CONTEXT
    (
        P_USER_ID           IN NUMBER,
        O_USER_CONTEXT_ID   OUT NUMBER,
        O_ROLE_ID           OUT NUMBER,
        O_GROUP_ID          OUT NUMBER,
        O_ENTITY_ID         OUT NUMBER,
        O_STATUS            OUT VARCHAR2,
        O_MESSAGE           OUT VARCHAR2
    )
    IS
    BEGIN
        SELECT USER_CONTEXT_ID, ROLE_ID, NVL(GROUP_ID, ROLE_ID), ENTITY_ID
          INTO O_USER_CONTEXT_ID, O_ROLE_ID, O_GROUP_ID, O_ENTITY_ID
          FROM (
                SELECT USER_CONTEXT_ID, ROLE_ID, GROUP_ID, ENTITY_ID
                  FROM T_USER_CONTEXT_ASSIGNMENT
                 WHERE USER_ID = P_USER_ID
                   AND IS_ACTIVE = 'Y'
                   AND (EFFECTIVE_FROM IS NULL OR EFFECTIVE_FROM <= TRUNC(SYSDATE))
                   AND (EFFECTIVE_TO   IS NULL OR EFFECTIVE_TO   >= TRUNC(SYSDATE))
                 ORDER BY CASE WHEN IS_DEFAULT = 'Y' THEN 0 ELSE 1 END, USER_CONTEXT_ID
               )
         WHERE ROWNUM = 1;

        O_STATUS := 'OK';
        O_MESSAGE := 'Default context loaded successfully.';
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            O_USER_CONTEXT_ID := NULL;
            O_ROLE_ID := NULL;
            O_GROUP_ID := NULL;
            O_ENTITY_ID := NULL;
            O_STATUS := 'FAIL';
            O_MESSAGE := 'No active context found for the supplied user.';
    END P_GET_DEFAULT_USER_CONTEXT;

    PROCEDURE P_VALIDATE_USER_CONTEXT
    (
        P_PPNO              IN NUMBER,
        P_USER_CONTEXT_ID   IN NUMBER,
        IO_CURSOR           OUT T_CURSOR
    )
    IS
    BEGIN
        P_OPEN_CONTEXT_CURSOR(NULL, P_PPNO, P_USER_CONTEXT_ID, 'Y', IO_CURSOR);
    END P_VALIDATE_USER_CONTEXT;

    PROCEDURE P_SAVE_USER_CONTEXT_ASSIGNMENT
    (
        P_USER_CONTEXT_ID   IN OUT NUMBER,
        P_USER_ID           IN NUMBER,
        P_PPNO              IN NUMBER,
        P_ROLE_ID           IN NUMBER,
        P_GROUP_ID          IN NUMBER,
        P_ENTITY_ID         IN NUMBER,
        P_IS_DEFAULT        IN CHAR,
        P_IS_ACTIVE         IN CHAR,
        P_ASSIGNMENT_TYPE   IN VARCHAR2,
        P_EFFECTIVE_FROM    IN DATE,
        P_EFFECTIVE_TO      IN DATE,
        P_REMARKS           IN VARCHAR2,
        P_ACTION_BY         IN VARCHAR2,
        O_STATUS            OUT VARCHAR2,
        O_MESSAGE           OUT VARCHAR2
    )
    IS
        V_EXISTS      NUMBER := 0;
        V_IS_DEFAULT  CHAR(1) := CASE WHEN NVL(UPPER(TRIM(P_IS_DEFAULT)),'N') = 'Y' THEN 'Y' ELSE 'N' END;
        V_IS_ACTIVE   CHAR(1) := CASE WHEN NVL(UPPER(TRIM(P_IS_ACTIVE)),'Y') = 'N' THEN 'N' ELSE 'Y' END;
    BEGIN
      
        IF P_USER_ID IS NULL OR P_PPNO IS NULL OR P_ROLE_ID IS NULL OR P_ENTITY_ID IS NULL THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := 'USER_ID, PPNO, ROLE_ID and ENTITY_ID are required.';
            RETURN;
        END IF;

        SELECT COUNT(*)
          INTO V_EXISTS
          FROM T_USER_CONTEXT_ASSIGNMENT
         WHERE USER_ID = P_USER_ID
           AND ROLE_ID = P_ROLE_ID
           AND ENTITY_ID = P_ENTITY_ID
           AND NVL(GROUP_ID,-1) = NVL(P_GROUP_ID,-1)
           AND USER_CONTEXT_ID <> NVL(P_USER_CONTEXT_ID,-1);

        IF V_EXISTS > 0 THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := 'Duplicate role/entity assignment already exists for this user.';
            RETURN;
        END IF;

        IF V_IS_DEFAULT = 'Y' THEN
            UPDATE T_USER_CONTEXT_ASSIGNMENT
               SET IS_DEFAULT = 'N',
                   MODIFIED_BY = NVL(P_ACTION_BY,'SYSTEM'),
                   MODIFIED_ON = SYSDATE
             WHERE USER_ID = P_USER_ID;
        END IF;

        IF P_USER_CONTEXT_ID IS NULL OR P_USER_CONTEXT_ID = 0 THEN
            P_USER_CONTEXT_ID := SEQ_T_USER_CONTEXT_ASSIGNMENT.NEXTVAL;

            INSERT INTO T_USER_CONTEXT_ASSIGNMENT
            (
                USER_CONTEXT_ID, USER_ID, PPNO, ROLE_ID, GROUP_ID, ENTITY_ID,
                IS_DEFAULT, IS_ACTIVE, ASSIGNMENT_TYPE, EFFECTIVE_FROM, EFFECTIVE_TO,
                REMARKS, CREATED_BY, CREATED_ON
            )
            VALUES
            (
                P_USER_CONTEXT_ID, P_USER_ID, P_PPNO, P_ROLE_ID, P_GROUP_ID, P_ENTITY_ID,
                V_IS_DEFAULT, V_IS_ACTIVE, NVL(P_ASSIGNMENT_TYPE,'MANUAL'), P_EFFECTIVE_FROM, P_EFFECTIVE_TO,
                P_REMARKS, NVL(P_ACTION_BY,'SYSTEM'), SYSDATE
            );

            P_WRITE_HISTORY(P_USER_CONTEXT_ID, 'INSERT', P_ACTION_BY);
        ELSE
            UPDATE T_USER_CONTEXT_ASSIGNMENT
               SET PPNO            = P_PPNO,
                   ROLE_ID         = P_ROLE_ID,
                   GROUP_ID        = P_GROUP_ID,
                   ENTITY_ID       = P_ENTITY_ID,
                   IS_DEFAULT      = V_IS_DEFAULT,
                   IS_ACTIVE       = V_IS_ACTIVE,
                   ASSIGNMENT_TYPE = NVL(P_ASSIGNMENT_TYPE,'MANUAL'),
                   EFFECTIVE_FROM  = P_EFFECTIVE_FROM,
                   EFFECTIVE_TO    = P_EFFECTIVE_TO,
                   REMARKS         = P_REMARKS,
                   MODIFIED_BY     = NVL(P_ACTION_BY,'SYSTEM'),
                   MODIFIED_ON     = SYSDATE
             WHERE USER_CONTEXT_ID = P_USER_CONTEXT_ID;

            IF SQL%ROWCOUNT = 0 THEN
                O_STATUS := 'FAIL';
                O_MESSAGE := 'Context assignment not found.';
                RETURN;
            END IF;

            P_WRITE_HISTORY(P_USER_CONTEXT_ID, 'UPDATE', P_ACTION_BY);
        END IF;
        
    IF V_IS_ACTIVE = 'N' then
      Delete from T_USER_CONTEXT_ASSIGNMENT d 
      where d.ppno = P_PPNO and d.role_id = P_ROLE_ID and d.group_id = P_GROUP_ID
      and d.entity_id = P_ENTITY_ID;
      commit;
    end if;

        O_STATUS := 'OK';
        O_MESSAGE := 'User context assignment saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := SQLERRM;
    END P_SAVE_USER_CONTEXT_ASSIGNMENT;

    PROCEDURE P_SET_DEFAULT_USER_CONTEXT
    (
        P_USER_ID           IN NUMBER,
        P_USER_CONTEXT_ID   IN NUMBER,
        P_ACTION_BY         IN VARCHAR2,
        O_STATUS            OUT VARCHAR2,
        O_MESSAGE           OUT VARCHAR2
    )
    IS
        V_EXISTS NUMBER := 0;
    BEGIN
        SELECT COUNT(*)
          INTO V_EXISTS
          FROM T_USER_CONTEXT_ASSIGNMENT
         WHERE USER_ID = P_USER_ID
           AND USER_CONTEXT_ID = P_USER_CONTEXT_ID
           AND IS_ACTIVE = 'Y'
           AND (EFFECTIVE_FROM IS NULL OR EFFECTIVE_FROM <= TRUNC(SYSDATE))
           AND (EFFECTIVE_TO   IS NULL OR EFFECTIVE_TO   >= TRUNC(SYSDATE));

        IF V_EXISTS = 0 THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := 'Active context assignment not found.';
            RETURN;
        END IF;

        UPDATE T_USER_CONTEXT_ASSIGNMENT
           SET IS_DEFAULT = 'N',
               MODIFIED_BY = NVL(P_ACTION_BY,'SYSTEM'),
               MODIFIED_ON = SYSDATE
         WHERE USER_ID = P_USER_ID;

        UPDATE T_USER_CONTEXT_ASSIGNMENT
           SET IS_DEFAULT = 'Y',
               MODIFIED_BY = NVL(P_ACTION_BY,'SYSTEM'),
               MODIFIED_ON = SYSDATE
         WHERE USER_ID = P_USER_ID
           AND USER_CONTEXT_ID = P_USER_CONTEXT_ID;

        P_WRITE_HISTORY(P_USER_CONTEXT_ID, 'DEFAULT_CHANGE', P_ACTION_BY);
        P_SYNC_USER_DEFAULT_CONTEXT(P_USER_ID, P_ACTION_BY, O_STATUS, O_MESSAGE);
    EXCEPTION
        WHEN OTHERS THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := SQLERRM;
    END P_SET_DEFAULT_USER_CONTEXT;

    PROCEDURE P_DISABLE_USER_CONTEXT_ASSIGNMENT
    (
        P_USER_CONTEXT_ID   IN NUMBER,
        P_ACTION_BY         IN VARCHAR2,
        O_STATUS            OUT VARCHAR2,
        O_MESSAGE           OUT VARCHAR2
    )
    IS
        V_USER_ID NUMBER;
    BEGIN
        SELECT USER_ID
          INTO V_USER_ID
          FROM T_USER_CONTEXT_ASSIGNMENT
         WHERE USER_CONTEXT_ID = P_USER_CONTEXT_ID;

        UPDATE T_USER_CONTEXT_ASSIGNMENT
           SET IS_ACTIVE = 'N',
               IS_DEFAULT = 'N',
               MODIFIED_BY = NVL(P_ACTION_BY,'SYSTEM'),
               MODIFIED_ON = SYSDATE
         WHERE USER_CONTEXT_ID = P_USER_CONTEXT_ID;

        P_WRITE_HISTORY(P_USER_CONTEXT_ID, 'DISABLE', P_ACTION_BY);
        O_STATUS := 'OK';
        O_MESSAGE := 'User context assignment disabled successfully.';
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := 'Context assignment not found.';
        WHEN OTHERS THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := SQLERRM;
    END P_DISABLE_USER_CONTEXT_ASSIGNMENT;

    PROCEDURE P_GET_USER_CONTEXT_ASSIGNMENTS
    (
        P_USER_ID     IN NUMBER,
        P_PPNO        IN NUMBER,
        IO_CURSOR     OUT T_CURSOR
    )
    IS
    BEGIN
        P_OPEN_CONTEXT_CURSOR(P_USER_ID, P_PPNO, NULL, 'N', IO_CURSOR);
    END P_GET_USER_CONTEXT_ASSIGNMENTS;

    PROCEDURE P_SEARCH_USERS_WITH_CONTEXT
    (
        P_ENTITY_ID   IN NUMBER,
        P_EMAIL       IN VARCHAR2,
        P_GROUP_ID    IN NUMBER,
        P_PPNO        IN NUMBER,
        P_LOGINNAME   IN VARCHAR2,
        IO_CURSOR     OUT T_CURSOR
    )
    IS
    BEGIN
        OPEN IO_CURSOR FOR
            SELECT U.USERID,
                   U.PPNO,
                   HR.EMPLOYEEFIRSTNAME,
                   HR.EMPLOYEELASTNAME,
                   HR.EMAIL,
                   UCA.ENTITY_ID,
                   NVL(M.CHILD_CODE, AE.CODE) AS CODE,
                   NVL(M.C_TYPE_ID, AE.TYPE_ID) AS TYPE_ID,
                   COALESCE(M.C_NAME, AE.NAME) AS C_NAME,
                   M.PARENT_ID,
                   M.PARENT_CODE,
                   M.P_TYPE_ID,
                   M.P_NAME,
                   NVL(M.C_TYPE_ID, AE.TYPE_ID) AS C_TYPE_ID,
                   M.RELATION_TYPE_ID,
                   NVL(UCA.GROUP_ID,UCA.ROLE_ID) AS GROUP_ID,
                   G.GROUP_NAME,
                   U.ISACTIVE,
                   COUNT(*) OVER (PARTITION BY U.USERID) AS ASSIGNMENT_COUNT
              FROM T_USER U
              JOIN T_USER_CONTEXT_ASSIGNMENT UCA
                ON UCA.USER_ID = U.USERID
              LEFT JOIN TBL_T_HR_EMPLOYEEINFORMATION HR
                ON HR.PPNO = U.PPNO
              LEFT JOIN (
                    SELECT GROUP_ID, MAX(ROLE_ID) AS ROLE_ID, MAX(GROUP_NAME) AS GROUP_NAME
                      FROM T_GROUPS
                     GROUP BY GROUP_ID
              ) G
                ON G.GROUP_ID = NVL(UCA.GROUP_ID,UCA.ROLE_ID)
              LEFT JOIN T_AUDITEE_ENTITIES AE
                ON AE.ENTITY_ID = UCA.ENTITY_ID
              LEFT JOIN (
                    SELECT *
                      FROM (
                            SELECT EM.*,
                                   ROW_NUMBER() OVER
                                   (
                                       PARTITION BY EM.ENTITY_ID
                                       ORDER BY CASE WHEN NVL(EM.STATUS,'Y') IN ('Y','A','1') THEN 0 ELSE 1 END,
                                                EM.RELATION_TYPE_ID,
                                                EM.PARENT_ID
                                   ) AS RN
                              FROM T_AUDITEE_ENTITIES_MAPING EM
                           )
                     WHERE RN = 1
              ) M
                ON M.ENTITY_ID = UCA.ENTITY_ID
             WHERE (P_ENTITY_ID IS NULL OR UCA.ENTITY_ID = P_ENTITY_ID)
               AND (P_GROUP_ID IS NULL OR NVL(UCA.GROUP_ID,UCA.ROLE_ID) = P_GROUP_ID)
               AND (P_PPNO IS NULL OR U.PPNO = P_PPNO)
               AND (P_EMAIL IS NULL OR HR.EMAIL = P_EMAIL OR U.LOGIN_NAME = SUBSTR(P_EMAIL,1,15))
               AND (P_LOGINNAME IS NULL OR U.LOGIN_NAME = P_LOGINNAME)
             ORDER BY U.PPNO, G.GROUP_NAME, COALESCE(M.C_NAME, AE.NAME);
    END P_SEARCH_USERS_WITH_CONTEXT;

    PROCEDURE P_SYNC_USER_DEFAULT_CONTEXT
    (
        P_USER_ID      IN NUMBER,
        P_ACTION_BY    IN VARCHAR2,
        O_STATUS       OUT VARCHAR2,
        O_MESSAGE      OUT VARCHAR2
    )
    IS
        V_USER_CONTEXT_ID NUMBER;
        V_PPNO            NUMBER;
        V_ENTITY_ID       NUMBER;
        V_ROLE_ID         NUMBER;
        V_GROUP_ID        NUMBER;
        V_ENTITY_TYPE_ID  NUMBER;
        V_ENTITY_CODE     NUMBER;
        V_PARENT_CODE     NUMBER;
        V_EXISTS          NUMBER := 0;
    BEGIN
        SELECT USER_CONTEXT_ID,
               PPNO,
               ENTITY_ID,
               ROLE_ID,
               NVL(GROUP_ID, ROLE_ID),
               ENTITY_TYPE_ID,
               ENTITY_CODE,
               PARENT_ENTITY_CODE
          INTO V_USER_CONTEXT_ID,
               V_PPNO,
               V_ENTITY_ID,
               V_ROLE_ID,
               V_GROUP_ID,
               V_ENTITY_TYPE_ID,
               V_ENTITY_CODE,
               V_PARENT_CODE
          FROM (
                SELECT UCA.USER_CONTEXT_ID,
                       UCA.PPNO,
                       UCA.ENTITY_ID,
                       UCA.ROLE_ID,
                       UCA.GROUP_ID,
                       NVL(M.C_TYPE_ID, AE.TYPE_ID) AS ENTITY_TYPE_ID,
                       NVL(M.CHILD_CODE, AE.CODE) AS ENTITY_CODE,
                       M.PARENT_CODE AS PARENT_ENTITY_CODE
                  FROM T_USER_CONTEXT_ASSIGNMENT UCA
                  LEFT JOIN T_AUDITEE_ENTITIES AE
                    ON AE.ENTITY_ID = UCA.ENTITY_ID
                  LEFT JOIN (
                        SELECT *
                          FROM (
                                SELECT EM.*,
                                       ROW_NUMBER() OVER
                                       (
                                           PARTITION BY EM.ENTITY_ID
                                           ORDER BY CASE WHEN NVL(EM.STATUS,'Y') IN ('Y','A','1') THEN 0 ELSE 1 END,
                                                    EM.RELATION_TYPE_ID,
                                                    EM.PARENT_ID
                                       ) AS RN
                                  FROM T_AUDITEE_ENTITIES_MAPING EM
                               )
                         WHERE RN = 1
                  ) M
                    ON M.ENTITY_ID = UCA.ENTITY_ID
                 WHERE UCA.USER_ID = P_USER_ID
                   AND UCA.IS_ACTIVE = 'Y'
                   AND (UCA.EFFECTIVE_FROM IS NULL OR UCA.EFFECTIVE_FROM <= TRUNC(SYSDATE))
                   AND (UCA.EFFECTIVE_TO   IS NULL OR UCA.EFFECTIVE_TO   >= TRUNC(SYSDATE))
                 ORDER BY CASE WHEN UCA.IS_DEFAULT = 'Y' THEN 0 ELSE 1 END, UCA.USER_CONTEXT_ID
               )
         WHERE ROWNUM = 1;

        UPDATE T_USER
           SET ENTITY_ID = V_ENTITY_ID,
               USER_LOCATION_TYPE = CASE WHEN V_ENTITY_TYPE_ID = 6 THEN 'BRANCH' ELSE 'DEPARTMENT' END,
               ZONEID = CASE WHEN V_ENTITY_TYPE_ID = 6 THEN V_PARENT_CODE ELSE ZONEID END,
               BRANCHID = CASE WHEN V_ENTITY_TYPE_ID = 6 THEN V_ENTITY_CODE ELSE BRANCHID END,
               DIVISIONID = CASE WHEN V_ENTITY_TYPE_ID = 6 THEN DIVISIONID ELSE V_PARENT_CODE END,
               DEPARTMENTID = CASE WHEN V_ENTITY_TYPE_ID = 6 THEN DEPARTMENTID ELSE V_ENTITY_CODE END
         WHERE USERID = P_USER_ID;

        SELECT COUNT(*)
          INTO V_EXISTS
          FROM T_USER_MAPING
         WHERE USERID = P_USER_ID;

        IF V_EXISTS = 0 THEN
            INSERT INTO T_USER_MAPING (USERID, PPNO, GROUP_ID, ROLE_ID)
            VALUES (P_USER_ID, V_PPNO, V_GROUP_ID, V_ROLE_ID);
        ELSE
            UPDATE T_USER_MAPING
               SET PPNO = V_PPNO,
                   GROUP_ID = V_GROUP_ID,
                   ROLE_ID = V_ROLE_ID
             WHERE USERID = P_USER_ID;
        END IF;

        P_WRITE_HISTORY(V_USER_CONTEXT_ID, 'SYNC', P_ACTION_BY);
        O_STATUS := 'OK';
        O_MESSAGE := 'Default context synchronized successfully.';
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := 'No active default context found to sync.';
        WHEN OTHERS THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := SQLERRM;
    END P_SYNC_USER_DEFAULT_CONTEXT;

    PROCEDURE P_BACKFILL_USER_CONTEXTS
    (
        P_CREATED_BY   IN VARCHAR2,
        O_STATUS       OUT VARCHAR2,
        O_MESSAGE      OUT VARCHAR2
    )
    IS
    BEGIN
        INSERT INTO T_USER_CONTEXT_ASSIGNMENT
        (
            USER_CONTEXT_ID, USER_ID, PPNO, ROLE_ID, GROUP_ID, ENTITY_ID,
            IS_DEFAULT, IS_ACTIVE, ASSIGNMENT_TYPE, CREATED_BY, CREATED_ON
        )
        SELECT SEQ_T_USER_CONTEXT_ASSIGNMENT.NEXTVAL,
               U.USERID,
               U.PPNO,
               M.ROLE_ID,
               M.GROUP_ID,
               U.ENTITY_ID,
               'Y',
               NVL(U.ISACTIVE,'Y'),
               'BACKFILL',
               NVL(P_CREATED_BY,'SYSTEM'),
               SYSDATE
          FROM T_USER U
          JOIN T_USER_MAPING M
            ON M.USERID = U.USERID
         WHERE U.ENTITY_ID IS NOT NULL
           AND NOT EXISTS
               (
                   SELECT 1
                     FROM T_USER_CONTEXT_ASSIGNMENT X
                    WHERE X.USER_ID = U.USERID
               );

        O_STATUS := 'OK';
        O_MESSAGE := 'User context backfill completed successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := SQLERRM;
    END P_BACKFILL_USER_CONTEXTS;

    PROCEDURE P_SAVE_USER_ACCOUNT
    (
        P_USER_ID        IN OUT NUMBER,
        P_PPNO           IN NUMBER,
        P_PASSWORD       IN VARCHAR2,
        P_EMAIL_ADDRESS  IN VARCHAR2,
        P_ISACTIVE       IN VARCHAR2,
        P_ACTION_BY      IN VARCHAR2,
        O_STATUS         OUT VARCHAR2,
        O_MESSAGE        OUT VARCHAR2
    )
    IS
        V_USER_ID NUMBER := P_USER_ID;
        V_PP      NUMBER := P_PPNO;
    BEGIN
        IF V_PP IS NULL THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := 'PP number is required.';
            RETURN;
        END IF;

        IF V_USER_ID IS NULL OR V_USER_ID = 0 THEN
            BEGIN
                SELECT USERID
                  INTO V_USER_ID
                  FROM T_USER
                 WHERE PPNO = V_PP;

                UPDATE T_USER
                   SET PPNO = V_PP,
                       PASSWORD = NVL(P_PASSWORD, PASSWORD),
                       LOGIN_NAME = CASE WHEN P_EMAIL_ADDRESS IS NULL THEN LOGIN_NAME ELSE SUBSTR(P_EMAIL_ADDRESS,1,15) END,
                       ISACTIVE = NVL(P_ISACTIVE, ISACTIVE)
                 WHERE USERID = V_USER_ID;
            EXCEPTION
                WHEN NO_DATA_FOUND THEN
                    SELECT NVL(MAX(USERID),0) + 1 INTO V_USER_ID FROM T_USER;
                    INSERT INTO T_USER
                    (
                        USERID, LOGIN_NAME, PASSWORD, PPNO, ISACTIVE, PASSWORD_CHANGE_REQ
                    )
                    VALUES
                    (
                        V_USER_ID, NVL(SUBSTR(P_EMAIL_ADDRESS,1,15), TO_CHAR(V_PP)), P_PASSWORD, V_PP, NVL(P_ISACTIVE,'Y'), 'N'
                    );
            END;
        ELSE
            V_USER_ID := P_USER_ID;
            UPDATE T_USER
               SET PPNO = V_PP,
                   PASSWORD = NVL(P_PASSWORD, PASSWORD),
                   LOGIN_NAME = CASE WHEN P_EMAIL_ADDRESS IS NULL THEN LOGIN_NAME ELSE SUBSTR(P_EMAIL_ADDRESS,1,15) END,
                   ISACTIVE = NVL(P_ISACTIVE, ISACTIVE)
             WHERE USERID = V_USER_ID;

            IF SQL%ROWCOUNT = 0 THEN
                O_STATUS := 'FAIL';
                O_MESSAGE := 'User account not found.';
                RETURN;
            END IF;
        END IF;

        P_USER_ID := V_USER_ID;
        O_STATUS := 'OK';
        O_MESSAGE := 'User account saved successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            O_STATUS := 'FAIL';
            O_MESSAGE := SQLERRM;
    END P_SAVE_USER_ACCOUNT;

END PKG_USER_CONTEXT;
