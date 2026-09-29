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

  PROCEDURE P_GET_CONFIGURATIONS(
      IO_DIVISIONS  OUT SYS_REFCURSOR,
      IO_RECIPIENTS OUT SYS_REFCURSOR
  );

  PROCEDURE P_GET_DIVISION_OPTIONS(
      IO_DIVISIONS OUT SYS_REFCURSOR
  );

  PROCEDURE P_SAVE_DIVISION(
      P_DIVISION_ID          IN NUMBER,
      P_DIVISION_NAME        IN VARCHAR2,
      P_IS_ACTIVE            IN CHAR,
      P_WEEKLY_EMAIL_ENABLED IN CHAR,
      P_EFFECTIVE_FROM       IN DATE,
      P_EFFECTIVE_TO         IN DATE,
      P_REMARKS              IN VARCHAR2,
      P_UPDATED_BY           IN VARCHAR2
  );

  PROCEDURE P_SAVE_RECIPIENT(
      P_RECIPIENT_ID       IN NUMBER,
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
      P_SAVED_RECIPIENT_ID OUT NUMBER
  );

  PROCEDURE P_REMOVE_RECIPIENT(
      P_RECIPIENT_ID IN NUMBER,
      P_UPDATED_BY   IN VARCHAR2
  );

END PKG_MGMT_AUDIT_NOTIFY_ADMIN;
/

CREATE OR REPLACE PACKAGE BODY PKG_MGMT_AUDIT_NOTIFY_ADMIN AS

  PROCEDURE VALIDATE_FLAG(
      P_VALUE      IN CHAR,
      P_FIELD_NAME IN VARCHAR2
  ) IS
  BEGIN
    IF P_VALUE IS NULL OR P_VALUE NOT IN ('Y', 'N') THEN
      RAISE_APPLICATION_ERROR(
          -20140,
          P_FIELD_NAME || ' must be Y or N.'
      );
    END IF;
  END VALIDATE_FLAG;


  PROCEDURE VALIDATE_DATES(
      P_EFFECTIVE_FROM IN DATE,
      P_EFFECTIVE_TO   IN DATE
  ) IS
  BEGIN
    IF P_EFFECTIVE_FROM IS NULL THEN
      RAISE_APPLICATION_ERROR(
          -20141,
          'Effective From is required.'
      );
    END IF;

    IF P_EFFECTIVE_TO IS NOT NULL
       AND TRUNC(P_EFFECTIVE_TO) < TRUNC(P_EFFECTIVE_FROM) THEN

      RAISE_APPLICATION_ERROR(
          -20142,
          'Effective To cannot be earlier than Effective From.'
      );

    END IF;
  END VALIDATE_DATES;


  FUNCTION NORMALIZE_EMAIL_LIST(
      P_EMAIL_ADDRESS IN VARCHAR2
  ) RETURN VARCHAR2 IS

      V_EMAIL      VARCHAR2(500);
      V_NORMALIZED VARCHAR2(500);
      V_SEEN       VARCHAR2(32767) := ';';
      V_INDEX      PLS_INTEGER := 1;

  BEGIN
    IF TRIM(P_EMAIL_ADDRESS) IS NULL THEN
      RAISE_APPLICATION_ERROR(
          -20146,
          'Email address is required.'
      );
    END IF;

    IF INSTR(P_EMAIL_ADDRESS, ',') > 0
       OR INSTR(P_EMAIL_ADDRESS, CHR(10)) > 0
       OR INSTR(P_EMAIL_ADDRESS, CHR(13)) > 0
       OR REGEXP_LIKE(P_EMAIL_ADDRESS, '(^|;)[[:space:]]*(;|$)') THEN
      RAISE_APPLICATION_ERROR(
          -20146,
          'Enter valid email addresses separated by semicolons.'
      );
    END IF;

    LOOP
      V_EMAIL := TRIM(
          REGEXP_SUBSTR(P_EMAIL_ADDRESS, '[^;]+', 1, V_INDEX)
      );

      EXIT WHEN V_EMAIL IS NULL;

      IF NOT REGEXP_LIKE(
          V_EMAIL,
          '^[A-Za-z0-9.!#$%&''*+/=?^_`{|}~-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)+$'
      ) THEN
        RAISE_APPLICATION_ERROR(
            -20146,
            'Every email address must be valid.'
        );
      END IF;

      IF INSTR(V_SEEN, ';' || LOWER(V_EMAIL) || ';') = 0 THEN
        IF V_NORMALIZED IS NULL THEN
          V_NORMALIZED := V_EMAIL;
        ELSE
          V_NORMALIZED := V_NORMALIZED || '; ' || V_EMAIL;
        END IF;

        V_SEEN := V_SEEN || LOWER(V_EMAIL) || ';';
      END IF;

      V_INDEX := V_INDEX + 1;
    END LOOP;

    IF V_NORMALIZED IS NULL OR LENGTH(V_NORMALIZED) > 500 THEN
      RAISE_APPLICATION_ERROR(
          -20146,
          'Email addresses must not exceed 500 characters.'
      );
    END IF;

    RETURN V_NORMALIZED;

  END NORMALIZE_EMAIL_LIST;


  PROCEDURE P_GET_DIVISION_OPTIONS(
      IO_DIVISIONS OUT SYS_REFCURSOR
  ) IS
  BEGIN
    OPEN IO_DIVISIONS FOR
      SELECT
          M.DIVISION_ID,
          MAX(TRIM(M.DIVISION)) AS DIVISION_NAME
      FROM T_IAS_MGMT_NOTIFY_DEPARTMENTS M
      WHERE M.DIVISION_ID IS NOT NULL
        AND TRIM(M.DIVISION) IS NOT NULL
      GROUP BY M.DIVISION_ID
      ORDER BY MAX(TRIM(M.DIVISION));
  END P_GET_DIVISION_OPTIONS;


  PROCEDURE P_GET_CONFIGURATIONS(
      IO_DIVISIONS  OUT SYS_REFCURSOR,
      IO_RECIPIENTS OUT SYS_REFCURSOR
  ) IS
  BEGIN

    OPEN IO_DIVISIONS FOR

      SELECT
          D.DIVISION_ID,

          NVL(
              O.DIVISION_NAME,
              D.DIVISION_NAME
          ) AS DIVISION_NAME,

          NVL(
              O.REPORTING_OFFICE,
              ''
          ) AS REPORTING_OFFICE,

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

      LEFT JOIN
      (
          SELECT
              X.DIVISION_ID,
              MAX(X.DIVISION) AS DIVISION_NAME,

              LISTAGG(
                  X.GROUP_HEAD,
                  '; '
              ) WITHIN GROUP (
                  ORDER BY X.GROUP_HEAD
              ) AS REPORTING_OFFICE

          FROM
          (
              SELECT DISTINCT
                  M.DIVISION_ID,
                  TRIM(M.DIVISION)   AS DIVISION,
                  TRIM(M.GROUP_HEAD) AS GROUP_HEAD

              FROM T_IAS_MGMT_NOTIFY_DEPARTMENTS M

              WHERE M.DIVISION_ID IS NOT NULL
          ) X

          GROUP BY X.DIVISION_ID

      ) O
        ON O.DIVISION_ID = D.DIVISION_ID

      ORDER BY
          NVL(
              O.DIVISION_NAME,
              D.DIVISION_NAME
          );


    OPEN IO_RECIPIENTS FOR

      SELECT
          R.RECIPIENT_ID,
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

      ORDER BY
          R.DIVISION_ID,

          CASE R.RECIPIENT_TYPE
              WHEN 'TO' THEN 1
              WHEN 'CC' THEN 2
              WHEN 'BCC' THEN 3
              ELSE 4
          END,

          NVL(R.DISPLAY_ORDER, 1),
          R.EMAIL_ADDRESS;

  END P_GET_CONFIGURATIONS;


  PROCEDURE P_SAVE_DIVISION(
      P_DIVISION_ID          IN NUMBER,
      P_DIVISION_NAME        IN VARCHAR2,
      P_IS_ACTIVE            IN CHAR,
      P_WEEKLY_EMAIL_ENABLED IN CHAR,
      P_EFFECTIVE_FROM       IN DATE,
      P_EFFECTIVE_TO         IN DATE,
      P_REMARKS              IN VARCHAR2,
      P_UPDATED_BY           IN VARCHAR2
  ) IS

      V_DIVISION_NAME VARCHAR2(200);

  BEGIN

    IF P_DIVISION_ID IS NULL OR P_DIVISION_ID <= 0 THEN
      RAISE_APPLICATION_ERROR(
          -20143,
          'Division is required.'
      );
    END IF;


    SELECT MAX(TRIM(M.DIVISION))
      INTO V_DIVISION_NAME
      FROM T_IAS_MGMT_NOTIFY_DEPARTMENTS M
     WHERE M.DIVISION_ID = P_DIVISION_ID;


    IF V_DIVISION_NAME IS NULL THEN
      RAISE_APPLICATION_ERROR(
          -20144,
          'Selected Division does not exist in Management Audit notification departments.'
      );
    END IF;


    VALIDATE_FLAG(
        P_IS_ACTIVE,
        'Active status'
    );

    VALIDATE_FLAG(
        P_WEEKLY_EMAIL_ENABLED,
        'Notification Enabled'
    );

    VALIDATE_DATES(
        P_EFFECTIVE_FROM,
        P_EFFECTIVE_TO
    );


    MERGE INTO T_IAS_MGMT_NOTIFY_DIVISION D

    USING
    (
        SELECT
            P_DIVISION_ID AS DIVISION_ID
        FROM DUAL
    ) S

    ON (
        D.DIVISION_ID = S.DIVISION_ID
    )

    WHEN MATCHED THEN
      UPDATE SET
          D.DIVISION_NAME        = V_DIVISION_NAME,
          D.IS_ACTIVE            = P_IS_ACTIVE,
          D.WEEKLY_EMAIL_ENABLED = P_WEEKLY_EMAIL_ENABLED,
          D.EFFECTIVE_FROM       = TRUNC(P_EFFECTIVE_FROM),

          D.EFFECTIVE_TO =
              CASE
                  WHEN P_EFFECTIVE_TO IS NULL THEN NULL
                  ELSE TRUNC(P_EFFECTIVE_TO)
              END,

          D.REMARKS    = TRIM(P_REMARKS),
          D.UPDATED_BY = NVL(TRIM(P_UPDATED_BY), USER),
          D.UPDATED_ON = SYSDATE

    WHEN NOT MATCHED THEN

      INSERT
      (
          DIVISION_ID,
          DIVISION_NAME,
          IS_ACTIVE,
          WEEKLY_EMAIL_ENABLED,
          EFFECTIVE_FROM,
          EFFECTIVE_TO,
          REMARKS,
          CREATED_BY,
          CREATED_ON
      )

      VALUES
      (
          P_DIVISION_ID,
          V_DIVISION_NAME,
          P_IS_ACTIVE,
          P_WEEKLY_EMAIL_ENABLED,
          TRUNC(P_EFFECTIVE_FROM),

          CASE
              WHEN P_EFFECTIVE_TO IS NULL THEN NULL
              ELSE TRUNC(P_EFFECTIVE_TO)
          END,

          TRIM(P_REMARKS),
          NVL(TRIM(P_UPDATED_BY), USER),
          SYSDATE
      );


    COMMIT;

  END P_SAVE_DIVISION;


  PROCEDURE P_SAVE_RECIPIENT(
      P_RECIPIENT_ID       IN NUMBER,
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
      P_SAVED_RECIPIENT_ID OUT NUMBER
  ) IS

      V_RECIPIENT_TYPE  VARCHAR2(10)
          := UPPER(TRIM(P_RECIPIENT_TYPE));

      V_EMAIL_ADDRESS   VARCHAR2(500);
      V_DUPLICATE_COUNT NUMBER;
      V_DIVISION_COUNT  NUMBER;

  BEGIN

    IF P_DIVISION_ID IS NULL OR P_DIVISION_ID <= 0 THEN
      RAISE_APPLICATION_ERROR(
          -20143,
          'Division is required.'
      );
    END IF;


    SELECT COUNT(*)
      INTO V_DIVISION_COUNT
      FROM T_IAS_MGMT_NOTIFY_DEPARTMENTS M
     WHERE M.DIVISION_ID = P_DIVISION_ID;


    IF V_DIVISION_COUNT = 0 THEN
      RAISE_APPLICATION_ERROR(
          -20144,
          'Selected Division does not exist in Management Audit notification departments.'
      );
    END IF;


    IF V_RECIPIENT_TYPE NOT IN ('TO', 'CC', 'BCC') THEN
      RAISE_APPLICATION_ERROR(
          -20145,
          'Recipient type must be TO, CC or BCC.'
      );
    END IF;


    V_EMAIL_ADDRESS := NORMALIZE_EMAIL_LIST(P_EMAIL_ADDRESS);


    VALIDATE_FLAG(
        P_IS_ACTIVE,
        'Recipient active status'
    );

    VALIDATE_DATES(
        P_EFFECTIVE_FROM,
        P_EFFECTIVE_TO
    );


    SELECT COUNT(*)
      INTO V_DUPLICATE_COUNT
      FROM T_IAS_MGMT_NOTIFY_RECIPIENT R
     WHERE R.DIVISION_ID = P_DIVISION_ID
       AND R.RECIPIENT_TYPE = V_RECIPIENT_TYPE
       AND LOWER(TRIM(R.EMAIL_ADDRESS))
           = LOWER(V_EMAIL_ADDRESS)
       AND R.RECIPIENT_ID <> NVL(P_RECIPIENT_ID, -1);


    IF V_DUPLICATE_COUNT > 0 THEN

      RAISE_APPLICATION_ERROR(
          -20147,
          'This email address is already configured for the selected recipient type.'
      );

    END IF;


    IF P_RECIPIENT_ID IS NULL THEN

      P_SAVED_RECIPIENT_ID :=
          SEQ_IAS_MGMT_NOTIFY_RECIPIENT.NEXTVAL;


      INSERT INTO T_IAS_MGMT_NOTIFY_RECIPIENT
      (
          RECIPIENT_ID,
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
          CREATED_ON
      )

      VALUES
      (
          P_SAVED_RECIPIENT_ID,
          P_DIVISION_ID,
          V_RECIPIENT_TYPE,
          V_EMAIL_ADDRESS,
          TRIM(P_PERSON_NAME),
          TRIM(P_DESIGNATION),
          P_IS_ACTIVE,
          TRUNC(P_EFFECTIVE_FROM),

          CASE
              WHEN P_EFFECTIVE_TO IS NULL THEN NULL
              ELSE TRUNC(P_EFFECTIVE_TO)
          END,

          NVL(P_DISPLAY_ORDER, 1),
          NVL(TRIM(P_UPDATED_BY), USER),
          SYSDATE
      );

    ELSE

      UPDATE T_IAS_MGMT_NOTIFY_RECIPIENT R

         SET R.DIVISION_ID    = P_DIVISION_ID,
             R.RECIPIENT_TYPE = V_RECIPIENT_TYPE,
             R.EMAIL_ADDRESS  = V_EMAIL_ADDRESS,
             R.PERSON_NAME    = TRIM(P_PERSON_NAME),
             R.DESIGNATION    = TRIM(P_DESIGNATION),
             R.IS_ACTIVE      = P_IS_ACTIVE,
             R.EFFECTIVE_FROM = TRUNC(P_EFFECTIVE_FROM),

             R.EFFECTIVE_TO =
                 CASE
                     WHEN P_EFFECTIVE_TO IS NULL THEN NULL
                     ELSE TRUNC(P_EFFECTIVE_TO)
                 END,

             R.DISPLAY_ORDER =
                 NVL(P_DISPLAY_ORDER, 1),

             R.UPDATED_BY =
                 NVL(TRIM(P_UPDATED_BY), USER),

             R.UPDATED_ON =
                 SYSDATE

       WHERE R.RECIPIENT_ID =
             P_RECIPIENT_ID;


      IF SQL%ROWCOUNT = 0 THEN

        RAISE_APPLICATION_ERROR(
            -20148,
            'Recipient configuration was not found.'
        );

      END IF;


      P_SAVED_RECIPIENT_ID :=
          P_RECIPIENT_ID;

    END IF;


    COMMIT;

  END P_SAVE_RECIPIENT;


  PROCEDURE P_REMOVE_RECIPIENT(
      P_RECIPIENT_ID IN NUMBER,
      P_UPDATED_BY   IN VARCHAR2
  ) IS
  BEGIN

    IF P_RECIPIENT_ID IS NULL
       OR P_RECIPIENT_ID <= 0 THEN

      RAISE_APPLICATION_ERROR(
          -20149,
          'Recipient is required.'
      );

    END IF;


    UPDATE T_IAS_MGMT_NOTIFY_RECIPIENT R

       SET R.IS_ACTIVE = 'N',

           R.EFFECTIVE_TO =
               CASE
                   WHEN R.EFFECTIVE_FROM > TRUNC(SYSDATE)
                       THEN R.EFFECTIVE_FROM
                   ELSE TRUNC(SYSDATE)
               END,

           R.UPDATED_BY =
               NVL(TRIM(P_UPDATED_BY), USER),

           R.UPDATED_ON =
               SYSDATE

     WHERE R.RECIPIENT_ID =
           P_RECIPIENT_ID;


    IF SQL%ROWCOUNT = 0 THEN

      RAISE_APPLICATION_ERROR(
          -20148,
          'Recipient configuration was not found.'
      );

    END IF;


    COMMIT;

  END P_REMOVE_RECIPIENT;

END PKG_MGMT_AUDIT_NOTIFY_ADMIN;
/
