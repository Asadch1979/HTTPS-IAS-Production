/*
Management Audit notification administration package.

This script advances the existing recipient sequence beyond populated IDs and
replaces only PKG_MGMT_AUDIT_NOTIFY_ADMIN. It does not recreate tables,
constraints, mappings, scheduler jobs, or notification history.
*/
DECLARE
  V_MAX_RECIPIENT_ID NUMBER;
  V_NEXT_RECIPIENT_ID NUMBER;
BEGIN
  SELECT NVL(MAX(RECIPIENT_ID), 0)
    INTO V_MAX_RECIPIENT_ID
    FROM T_IAS_MGMT_NOTIFY_RECIPIENT;

  LOOP
    SELECT SEQ_IAS_MGMT_NOTIFY_RECIPIENT.NEXTVAL
      INTO V_NEXT_RECIPIENT_ID
      FROM DUAL;
    EXIT WHEN V_NEXT_RECIPIENT_ID > V_MAX_RECIPIENT_ID;
  END LOOP;
END;
/
CREATE OR REPLACE PACKAGE PKG_MGMT_AUDIT_NOTIFY_ADMIN AS

  PROCEDURE P_GET_CONFIGURATIONS(IO_DIVISIONS  OUT SYS_REFCURSOR,
                                 IO_RECIPIENTS OUT SYS_REFCURSOR);

  PROCEDURE P_SAVE_DIVISION(P_DIVISION_ID          IN NUMBER,
                            P_DIVISION_NAME        IN VARCHAR2,
                            P_IS_ACTIVE            IN CHAR,
                            P_WEEKLY_EMAIL_ENABLED IN CHAR,
                            P_EFFECTIVE_FROM       IN DATE,
                            P_EFFECTIVE_TO         IN DATE,
                            P_REMARKS              IN VARCHAR2,
                            P_UPDATED_BY           IN VARCHAR2);

  PROCEDURE P_SAVE_RECIPIENT(P_RECIPIENT_ID       IN NUMBER,
                             P_DIVISION_ID        IN NUMBER,
                             P_RECIPIENT_TYPE     IN VARCHAR2,
                             P_EMAIL_ADDRESS      IN VARCHAR2,
                             P_PERSON_NAME        IN VARCHAR2,
                             P_DESIGNATION        IN VARCHAR2,
                             P_IS_ACTIVE          IN CHAR,
                             P_EFFECTIVE_FROM     IN DATE,
                             P_EFFECTIVE_TO       IN DATE,
                             P_DISPLAY_ORDER      IN NUMBER,
                             P_UPDATED_BY         IN VARCHAR2,
                             P_SAVED_RECIPIENT_ID OUT NUMBER);

  PROCEDURE P_REMOVE_RECIPIENT(P_RECIPIENT_ID IN NUMBER,
                               P_UPDATED_BY   IN VARCHAR2);

END PKG_MGMT_AUDIT_NOTIFY_ADMIN;
/
CREATE OR REPLACE PACKAGE BODY PKG_MGMT_AUDIT_NOTIFY_ADMIN AS

  PROCEDURE VALIDATE_FLAG(P_VALUE IN CHAR, P_FIELD_NAME IN VARCHAR2) IS
  BEGIN
    IF P_VALUE IS NULL OR P_VALUE NOT IN ('Y', 'N') THEN
      RAISE_APPLICATION_ERROR(-20140, P_FIELD_NAME || ' must be Y or N.');
    END IF;
  END VALIDATE_FLAG;

  PROCEDURE VALIDATE_DATES(P_EFFECTIVE_FROM IN DATE,
                           P_EFFECTIVE_TO   IN DATE) IS
  BEGIN
    IF P_EFFECTIVE_FROM IS NULL THEN
      RAISE_APPLICATION_ERROR(-20141, 'Effective From is required.');
    END IF;
  
    IF P_EFFECTIVE_TO IS NOT NULL AND
       TRUNC(P_EFFECTIVE_TO) < TRUNC(P_EFFECTIVE_FROM) THEN
      RAISE_APPLICATION_ERROR(-20142,
                              'Effective To cannot be earlier than Effective From.');
    END IF;
  END VALIDATE_DATES;

  PROCEDURE P_GET_CONFIGURATIONS(IO_DIVISIONS  OUT SYS_REFCURSOR,
                                 IO_RECIPIENTS OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN IO_DIVISIONS FOR
      SELECT D.DIVISION_ID,
             D.DIVISION_NAME,
             NVL(H.REPORTING_OFFICE, '') AS REPORTING_OFFICE,
             D.IS_ACTIVE,
             D.WEEKLY_EMAIL_ENABLED,
             D.EFFECTIVE_FROM,
             D.EFFECTIVE_TO,
             D.REMARKS,
             D.CREATED_BY,
             D.CREATED_ON,
             D.UPDATED_BY,
             D.UPDATED_ON
        FROM T_IAS_MGMT_NOTIFY_DIVISION D
        LEFT JOIN (SELECT X.DIVISION_ID,
                          LISTAGG(X.REPORTING_OFFICE, '; ') WITHIN GROUP(ORDER BY X.REPORTING_OFFICE) AS REPORTING_OFFICE
                     FROM (SELECT DISTINCT M.ENTITY_ID AS DIVISION_ID,
                                           R.NAME      AS REPORTING_OFFICE
                             FROM T_AUDITEE_ENTITIES_MAPING M
                             JOIN T_AUDITEE_ENTITIES R
                               ON R.ENTITY_ID = M.PARENT_ID
                            WHERE R.NAME IS NOT NULL) X
                    GROUP BY X.DIVISION_ID) H
          ON H.DIVISION_ID = D.DIVISION_ID
       ORDER BY D.DIVISION_NAME;
  
    OPEN IO_RECIPIENTS FOR
      SELECT R.RECIPIENT_ID,
             R.DIVISION_ID,
             R.RECIPIENT_TYPE,
             R.EMAIL_ADDRESS,
             R.PERSON_NAME,
             R.DESIGNATION,
             R.IS_ACTIVE,
             R.EFFECTIVE_FROM,
             R.EFFECTIVE_TO,
             NVL(R.DISPLAY_ORDER, 1) AS DISPLAY_ORDER,
             R.CREATED_BY,
             R.CREATED_ON,
             R.UPDATED_BY,
             R.UPDATED_ON
        FROM T_IAS_MGMT_NOTIFY_RECIPIENT R
       ORDER BY R.DIVISION_ID,
                CASE R.RECIPIENT_TYPE
                  WHEN 'TO' THEN
                   1
                  ELSE
                   2
                END,
                NVL(R.DISPLAY_ORDER, 1),
                R.EMAIL_ADDRESS;
  END P_GET_CONFIGURATIONS;

  PROCEDURE P_SAVE_DIVISION(P_DIVISION_ID          IN NUMBER,
                            P_DIVISION_NAME        IN VARCHAR2,
                            P_IS_ACTIVE            IN CHAR,
                            P_WEEKLY_EMAIL_ENABLED IN CHAR,
                            P_EFFECTIVE_FROM       IN DATE,
                            P_EFFECTIVE_TO         IN DATE,
                            P_REMARKS              IN VARCHAR2,
                            P_UPDATED_BY           IN VARCHAR2) IS
  BEGIN
    IF P_DIVISION_ID IS NULL OR P_DIVISION_ID <= 0 THEN
      RAISE_APPLICATION_ERROR(-20143, 'Division is required.');
    END IF;
  
    IF TRIM(P_DIVISION_NAME) IS NULL THEN
      RAISE_APPLICATION_ERROR(-20144, 'Division name is required.');
    END IF;
  
    VALIDATE_FLAG(P_IS_ACTIVE, 'Active status');
    VALIDATE_FLAG(P_WEEKLY_EMAIL_ENABLED, 'Weekly Email Enabled');
    VALIDATE_DATES(P_EFFECTIVE_FROM, P_EFFECTIVE_TO);
  
    MERGE INTO T_IAS_MGMT_NOTIFY_DIVISION D
    USING (SELECT P_DIVISION_ID AS DIVISION_ID FROM DUAL) S
    ON (D.DIVISION_ID = S.DIVISION_ID)
    WHEN MATCHED THEN
      UPDATE
         SET D.DIVISION_NAME        = TRIM(P_DIVISION_NAME),
             D.IS_ACTIVE            = P_IS_ACTIVE,
             D.WEEKLY_EMAIL_ENABLED = P_WEEKLY_EMAIL_ENABLED,
             D.EFFECTIVE_FROM       = TRUNC(P_EFFECTIVE_FROM),
             D.EFFECTIVE_TO         = CASE
                                        WHEN P_EFFECTIVE_TO IS NULL THEN
                                         NULL
                                        ELSE
                                         TRUNC(P_EFFECTIVE_TO)
                                      END,
             D.REMARKS              = TRIM(P_REMARKS),
             D.UPDATED_BY           = NVL(TRIM(P_UPDATED_BY), USER),
             D.UPDATED_ON           = SYSDATE WHEN NOT MATCHED THEN INSERT(DIVISION_ID, DIVISION_NAME, IS_ACTIVE, WEEKLY_EMAIL_ENABLED, EFFECTIVE_FROM, EFFECTIVE_TO, REMARKS, CREATED_BY, CREATED_ON) VALUES(P_DIVISION_ID, TRIM(P_DIVISION_NAME), P_IS_ACTIVE, P_WEEKLY_EMAIL_ENABLED, TRUNC(P_EFFECTIVE_FROM),CASE
               WHEN P_EFFECTIVE_TO IS NULL THEN
                NULL
               ELSE
                TRUNC(P_EFFECTIVE_TO)
             END, TRIM(P_REMARKS), NVL(TRIM(P_UPDATED_BY), USER), SYSDATE);
  
    COMMIT;
  END P_SAVE_DIVISION;

  PROCEDURE P_SAVE_RECIPIENT(P_RECIPIENT_ID       IN NUMBER,
                             P_DIVISION_ID        IN NUMBER,
                             P_RECIPIENT_TYPE     IN VARCHAR2,
                             P_EMAIL_ADDRESS      IN VARCHAR2,
                             P_PERSON_NAME        IN VARCHAR2,
                             P_DESIGNATION        IN VARCHAR2,
                             P_IS_ACTIVE          IN CHAR,
                             P_EFFECTIVE_FROM     IN DATE,
                             P_EFFECTIVE_TO       IN DATE,
                             P_DISPLAY_ORDER      IN NUMBER,
                             P_UPDATED_BY         IN VARCHAR2,
                             P_SAVED_RECIPIENT_ID OUT NUMBER) IS
    V_RECIPIENT_TYPE  VARCHAR2(10) := UPPER(TRIM(P_RECIPIENT_TYPE));
    V_DUPLICATE_COUNT NUMBER;
  BEGIN
    IF P_DIVISION_ID IS NULL OR P_DIVISION_ID <= 0 THEN
      RAISE_APPLICATION_ERROR(-20143, 'Division is required.');
    END IF;
  
    IF V_RECIPIENT_TYPE NOT IN ('TO', 'CC') THEN
      RAISE_APPLICATION_ERROR(-20145, 'Recipient type must be TO or CC.');
    END IF;
  
    IF TRIM(P_EMAIL_ADDRESS) IS NULL THEN
      RAISE_APPLICATION_ERROR(-20146, 'Email address is required.');
    END IF;
  
    VALIDATE_FLAG(P_IS_ACTIVE, 'Recipient active status');
    VALIDATE_DATES(P_EFFECTIVE_FROM, P_EFFECTIVE_TO);
  
    SELECT COUNT(*)
      INTO V_DUPLICATE_COUNT
      FROM T_IAS_MGMT_NOTIFY_RECIPIENT R
     WHERE R.DIVISION_ID = P_DIVISION_ID
       AND R.RECIPIENT_TYPE = V_RECIPIENT_TYPE
       AND LOWER(TRIM(R.EMAIL_ADDRESS)) = LOWER(TRIM(P_EMAIL_ADDRESS))
       AND R.RECIPIENT_ID <> NVL(P_RECIPIENT_ID, -1);
  
    IF V_DUPLICATE_COUNT > 0 THEN
      RAISE_APPLICATION_ERROR(-20147,
                              'This email address is already configured for the selected recipient type.');
    END IF;
  
    IF P_RECIPIENT_ID IS NULL THEN
      P_SAVED_RECIPIENT_ID := SEQ_IAS_MGMT_NOTIFY_RECIPIENT.NEXTVAL;
    
      INSERT INTO T_IAS_MGMT_NOTIFY_RECIPIENT
        (RECIPIENT_ID,
         DIVISION_ID,
         RECIPIENT_TYPE,
         EMAIL_ADDRESS,
         PERSON_NAME,
         DESIGNATION,
         IS_ACTIVE,
         EFFECTIVE_FROM,
         EFFECTIVE_TO,
         DISPLAY_ORDER,
         CREATED_BY,
         CREATED_ON)
      VALUES
        (P_SAVED_RECIPIENT_ID,
         P_DIVISION_ID,
         V_RECIPIENT_TYPE,
         TRIM(P_EMAIL_ADDRESS),
         TRIM(P_PERSON_NAME),
         TRIM(P_DESIGNATION),
         P_IS_ACTIVE,
         TRUNC(P_EFFECTIVE_FROM),
         CASE WHEN P_EFFECTIVE_TO IS NULL THEN NULL ELSE
         TRUNC(P_EFFECTIVE_TO) END,
         NVL(P_DISPLAY_ORDER, 1),
         NVL(TRIM(P_UPDATED_BY), USER),
         SYSDATE);
    ELSE
      UPDATE T_IAS_MGMT_NOTIFY_RECIPIENT R
         SET R.DIVISION_ID    = P_DIVISION_ID,
             R.RECIPIENT_TYPE = V_RECIPIENT_TYPE,
             R.EMAIL_ADDRESS  = TRIM(P_EMAIL_ADDRESS),
             R.PERSON_NAME    = TRIM(P_PERSON_NAME),
             R.DESIGNATION    = TRIM(P_DESIGNATION),
             R.IS_ACTIVE      = P_IS_ACTIVE,
             R.EFFECTIVE_FROM = TRUNC(P_EFFECTIVE_FROM),
             R.EFFECTIVE_TO = CASE
                                WHEN P_EFFECTIVE_TO IS NULL THEN
                                 NULL
                                ELSE
                                 TRUNC(P_EFFECTIVE_TO)
                              END,
             R.DISPLAY_ORDER  = NVL(P_DISPLAY_ORDER, 1),
             R.UPDATED_BY     = NVL(TRIM(P_UPDATED_BY), USER),
             R.UPDATED_ON     = SYSDATE
       WHERE R.RECIPIENT_ID = P_RECIPIENT_ID;
    
      IF SQL%ROWCOUNT = 0 THEN
        RAISE_APPLICATION_ERROR(-20148,
                                'Recipient configuration was not found.');
      END IF;
    
      P_SAVED_RECIPIENT_ID := P_RECIPIENT_ID;
    END IF;
  
    COMMIT;
  END P_SAVE_RECIPIENT;

  PROCEDURE P_REMOVE_RECIPIENT(P_RECIPIENT_ID IN NUMBER,
                               P_UPDATED_BY   IN VARCHAR2) IS
  BEGIN
    IF P_RECIPIENT_ID IS NULL OR P_RECIPIENT_ID <= 0 THEN
      RAISE_APPLICATION_ERROR(-20149, 'Recipient is required.');
    END IF;
  
    UPDATE T_IAS_MGMT_NOTIFY_RECIPIENT R
       SET R.IS_ACTIVE    = 'N',
           R.EFFECTIVE_TO = CASE
                              WHEN R.EFFECTIVE_FROM > TRUNC(SYSDATE) THEN
                               R.EFFECTIVE_FROM
                              ELSE
                               TRUNC(SYSDATE)
                            END,
           R.UPDATED_BY   = NVL(TRIM(P_UPDATED_BY), USER),
           R.UPDATED_ON   = SYSDATE
     WHERE R.RECIPIENT_ID = P_RECIPIENT_ID;
  
    IF SQL%ROWCOUNT = 0 THEN
      RAISE_APPLICATION_ERROR(-20148,
                              'Recipient configuration was not found.');
    END IF;
  
    COMMIT;
  END P_REMOVE_RECIPIENT;

END PKG_MGMT_AUDIT_NOTIFY_ADMIN;
