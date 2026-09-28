CREATE TABLE T_IAS_JOB_DEFINITION
(
    JOB_ID               NUMBER          NOT NULL,
    JOB_CODE             VARCHAR2(100)   NOT NULL,
    JOB_NAME             VARCHAR2(200)   NOT NULL,

    EXECUTION_TYPE       VARCHAR2(20)    NOT NULL,

    PACKAGE_NAME         VARCHAR2(128),
    PROCEDURE_NAME       VARCHAR2(128),

    APPLICATION_HANDLER  VARCHAR2(150),

    DESCRIPTION          VARCHAR2(1000),

    CREATED_BY           VARCHAR2(100)   DEFAULT USER NOT NULL,
    CREATED_ON           DATE            DEFAULT SYSDATE NOT NULL,
    UPDATED_BY           VARCHAR2(100),
    UPDATED_ON           DATE,

    CONSTRAINT PK_IAS_JOB_DEFINITION
        PRIMARY KEY (JOB_ID),

    CONSTRAINT UQ_IAS_JOB_DEFINITION_CODE
        UNIQUE (JOB_CODE),

    CONSTRAINT CK_IAS_JOB_EXEC_TYPE
        CHECK (EXECUTION_TYPE IN ('DATABASE','APPLICATION')),

    CONSTRAINT CK_IAS_JOB_EXEC_TARGET
        CHECK
        (
            (EXECUTION_TYPE = 'DATABASE'
             AND PROCEDURE_NAME IS NOT NULL
             AND APPLICATION_HANDLER IS NULL)
            OR
            (EXECUTION_TYPE = 'APPLICATION'
             AND APPLICATION_HANDLER IS NOT NULL)
        )
);

CREATE SEQUENCE SEQ_IAS_JOB_DEFINITION
START WITH 1
INCREMENT BY 1
NOCACHE
NOCYCLE;


CREATE TABLE T_IAS_JOB_SCHEDULE
(
    SCHEDULE_ID            NUMBER            NOT NULL,
    JOB_ID                 NUMBER            NOT NULL,

    FREQUENCY_TYPE         VARCHAR2(20)      NOT NULL,

    RUN_HOUR               NUMBER(2)         DEFAULT 0 NOT NULL,
    RUN_MINUTE             NUMBER(2)         DEFAULT 5 NOT NULL,

    DAY_OF_WEEK            NUMBER(1),
    DAY_OF_MONTH           NUMBER(2),

    PERIOD_TYPE            VARCHAR2(30)      DEFAULT 'NONE' NOT NULL,

    EMAIL_REQUIRED         CHAR(1)           DEFAULT 'N' NOT NULL,
    IS_ACTIVE              CHAR(1)           DEFAULT 'Y' NOT NULL,

    EFFECTIVE_FROM         DATE              DEFAULT TRUNC(SYSDATE) NOT NULL,
    EFFECTIVE_TO           DATE,

    LAST_RUN_ON            TIMESTAMP WITH TIME ZONE,
    LAST_STATUS            VARCHAR2(20),

    NEXT_DUE_ON            TIMESTAMP WITH TIME ZONE NOT NULL,

    RETRY_COUNT            NUMBER            DEFAULT 0 NOT NULL,
    MAX_RETRY              NUMBER            DEFAULT 3 NOT NULL,
    RETRY_INTERVAL_MINUTES NUMBER            DEFAULT 30 NOT NULL,

    LAST_ERROR             VARCHAR2(2000),

    REMARKS                VARCHAR2(1000),

    CREATED_BY             VARCHAR2(100)     DEFAULT USER NOT NULL,
    CREATED_ON             DATE              DEFAULT SYSDATE NOT NULL,
    UPDATED_BY             VARCHAR2(100),
    UPDATED_ON             DATE,

    CONSTRAINT PK_IAS_JOB_SCHEDULE
        PRIMARY KEY (SCHEDULE_ID),

    CONSTRAINT FK_IAS_JOB_SCHEDULE_JOB
        FOREIGN KEY (JOB_ID)
        REFERENCES T_IAS_JOB_DEFINITION (JOB_ID),

    CONSTRAINT CK_IAS_JOB_FREQUENCY
        CHECK
        (
            FREQUENCY_TYPE IN
            ('DAILY','WEEKLY','MONTHLY','MANUAL')
        ),

    CONSTRAINT CK_IAS_JOB_PERIOD
        CHECK
        (
            PERIOD_TYPE IN
            (
                'NONE',
                'PREVIOUS_DAY',
                'PREVIOUS_WEEK',
                'PREVIOUS_MONTH'
            )
        ),

    CONSTRAINT CK_IAS_JOB_EMAIL
        CHECK (EMAIL_REQUIRED IN ('Y','N')),

    CONSTRAINT CK_IAS_JOB_ACTIVE
        CHECK (IS_ACTIVE IN ('Y','N')),

    CONSTRAINT CK_IAS_JOB_HOUR
        CHECK (RUN_HOUR BETWEEN 0 AND 23),

    CONSTRAINT CK_IAS_JOB_MINUTE
        CHECK (RUN_MINUTE BETWEEN 0 AND 59),

    CONSTRAINT CK_IAS_JOB_WEEKDAY
        CHECK (DAY_OF_WEEK IS NULL OR DAY_OF_WEEK BETWEEN 1 AND 7),

    CONSTRAINT CK_IAS_JOB_MONTHDAY
        CHECK (DAY_OF_MONTH IS NULL OR DAY_OF_MONTH BETWEEN 1 AND 31),

    CONSTRAINT CK_IAS_JOB_SCHEDULE_DATES
        CHECK
        (
            EFFECTIVE_TO IS NULL
            OR EFFECTIVE_TO >= EFFECTIVE_FROM
        )
);

CREATE SEQUENCE SEQ_IAS_JOB_SCHEDULE
START WITH 1
INCREMENT BY 1
NOCACHE
NOCYCLE;

CREATE TABLE T_IAS_JOB_EXECUTION
(
    EXECUTION_ID       NUMBER               NOT NULL,
    SCHEDULE_ID        NUMBER               NOT NULL,
    JOB_ID             NUMBER               NOT NULL,

    EXECUTION_KEY      VARCHAR2(200)        NOT NULL,

    SCHEDULED_FOR      TIMESTAMP WITH TIME ZONE NOT NULL,

    PERIOD_FROM        DATE,
    PERIOD_TO          DATE,

    STATUS             VARCHAR2(20)         NOT NULL,

    STARTED_ON         TIMESTAMP WITH TIME ZONE,
    COMPLETED_ON       TIMESTAMP WITH TIME ZONE,

    RETRY_NO           NUMBER               DEFAULT 0 NOT NULL,

    RECORDS_PROCESSED  NUMBER,

    RESPONSE_MESSAGE   VARCHAR2(2000),
    ERROR_MESSAGE      VARCHAR2(4000),

    CREATED_ON         TIMESTAMP WITH TIME ZONE
                       DEFAULT SYSTIMESTAMP NOT NULL,

    CONSTRAINT PK_IAS_JOB_EXECUTION
        PRIMARY KEY (EXECUTION_ID),

    CONSTRAINT FK_IAS_JOB_EXEC_SCHEDULE
        FOREIGN KEY (SCHEDULE_ID)
        REFERENCES T_IAS_JOB_SCHEDULE (SCHEDULE_ID),

    CONSTRAINT FK_IAS_JOB_EXEC_JOB
        FOREIGN KEY (JOB_ID)
        REFERENCES T_IAS_JOB_DEFINITION (JOB_ID),

    CONSTRAINT UQ_IAS_JOB_EXECUTION_KEY
        UNIQUE (EXECUTION_KEY),

    CONSTRAINT CK_IAS_JOB_EXEC_STATUS
        CHECK
        (
            STATUS IN
            ('RUNNING','SUCCESS','FAILED','SKIPPED')
        )
);

CREATE SEQUENCE SEQ_IAS_JOB_EXECUTION
START WITH 1
INCREMENT BY 1
NOCACHE
NOCYCLE;

CREATE OR REPLACE VIEW V_IAS_JOB_SCHEDULE_ADMIN AS
SELECT D.JOB_ID,
       D.JOB_CODE,
       D.JOB_NAME,
       D.EXECUTION_TYPE,
       D.PACKAGE_NAME,
       D.PROCEDURE_NAME,
       D.APPLICATION_HANDLER,

       S.SCHEDULE_ID,
       S.FREQUENCY_TYPE,
       S.RUN_HOUR,
       S.RUN_MINUTE,
       S.DAY_OF_WEEK,
       S.DAY_OF_MONTH,
       S.PERIOD_TYPE,
       S.EMAIL_REQUIRED,
       S.IS_ACTIVE,
       S.EFFECTIVE_FROM,
       S.EFFECTIVE_TO,
       S.LAST_RUN_ON,
       S.LAST_STATUS,
       S.NEXT_DUE_ON,
       S.RETRY_COUNT,
       S.MAX_RETRY,
       S.LAST_ERROR,
       S.REMARKS

  FROM T_IAS_JOB_DEFINITION D

  JOIN T_IAS_JOB_SCHEDULE S
    ON S.JOB_ID = D.JOB_ID;
    
    
    CREATE OR REPLACE VIEW V_IAS_JOB_DUE AS
SELECT D.JOB_CODE,
       D.JOB_NAME,
       D.EXECUTION_TYPE,
       D.PACKAGE_NAME,
       D.PROCEDURE_NAME,
       D.APPLICATION_HANDLER,

       S.SCHEDULE_ID,
       S.FREQUENCY_TYPE,
       S.PERIOD_TYPE,
       S.EMAIL_REQUIRED,
       S.NEXT_DUE_ON,
       S.LAST_STATUS

  FROM T_IAS_JOB_DEFINITION D

  JOIN T_IAS_JOB_SCHEDULE S
    ON S.JOB_ID = D.JOB_ID

 WHERE S.IS_ACTIVE = 'Y'
   AND S.EFFECTIVE_FROM <= TRUNC(SYSDATE)
   AND
       (
           S.EFFECTIVE_TO IS NULL
           OR S.EFFECTIVE_TO >= TRUNC(SYSDATE)
       )
   AND S.NEXT_DUE_ON <= SYSTIMESTAMP
   AND NVL(S.LAST_STATUS,'READY') <> 'RUNNING';
   
   ALTER TABLE T_IAS_JOB_SCHEDULE
ADD
(
    LAST_PERIOD_FROM DATE,
    LAST_PERIOD_TO   DATE
);
/
CREATE OR REPLACE PACKAGE PKG_IAS_SCHEDULER AS

  PROCEDURE P_GET_SCHEDULES
  (
      IO_CURSOR OUT SYS_REFCURSOR
  );

  PROCEDURE P_GET_EXECUTION_HISTORY
  (
      P_SCHEDULE_ID IN NUMBER DEFAULT NULL,
      P_FROM_DATE    IN DATE   DEFAULT NULL,
      P_TO_DATE      IN DATE   DEFAULT NULL,
      IO_CURSOR      OUT SYS_REFCURSOR
  );

  PROCEDURE P_SAVE_JOB_DEFINITION
  (
      P_JOB_ID              IN NUMBER,
      P_JOB_CODE            IN VARCHAR2,
      P_JOB_NAME            IN VARCHAR2,
      P_EXECUTION_TYPE      IN VARCHAR2,
      P_PACKAGE_NAME        IN VARCHAR2,
      P_PROCEDURE_NAME      IN VARCHAR2,
      P_APPLICATION_HANDLER IN VARCHAR2,
      P_DESCRIPTION         IN VARCHAR2,
      P_UPDATED_BY          IN VARCHAR2,
      P_SAVED_JOB_ID        OUT NUMBER
  );

  PROCEDURE P_SAVE_SCHEDULE
  (
      P_SCHEDULE_ID            IN NUMBER,
      P_JOB_ID                 IN NUMBER,
      P_FREQUENCY_TYPE         IN VARCHAR2,
      P_RUN_HOUR               IN NUMBER,
      P_RUN_MINUTE             IN NUMBER,
      P_DAY_OF_WEEK            IN NUMBER,
      P_DAY_OF_MONTH           IN NUMBER,
      P_PERIOD_TYPE            IN VARCHAR2,
      P_EMAIL_REQUIRED         IN CHAR,
      P_IS_ACTIVE              IN CHAR,
      P_EFFECTIVE_FROM         IN DATE,
      P_EFFECTIVE_TO           IN DATE,
      P_MAX_RETRY              IN NUMBER,
      P_RETRY_INTERVAL_MINUTES IN NUMBER,
      P_NEXT_DUE_ON            IN TIMESTAMP WITH TIME ZONE,
      P_REMARKS                IN VARCHAR2,
      P_UPDATED_BY             IN VARCHAR2,
      P_SAVED_SCHEDULE_ID      OUT NUMBER
  );

  PROCEDURE P_SET_ACTIVE
  (
      P_SCHEDULE_ID IN NUMBER,
      P_IS_ACTIVE   IN CHAR,
      P_UPDATED_BY  IN VARCHAR2
  );

  PROCEDURE P_CLAIM_DUE_JOB
  (
      IO_CURSOR OUT SYS_REFCURSOR
  );

  PROCEDURE P_COMPLETE_JOB
  (
      P_EXECUTION_ID      IN NUMBER,
      P_RECORDS_PROCESSED IN NUMBER,
      P_RESPONSE_MESSAGE  IN VARCHAR2
  );

  PROCEDURE P_FAIL_JOB
  (
      P_EXECUTION_ID IN NUMBER,
      P_ERROR_MESSAGE IN VARCHAR2
  );

END PKG_IAS_SCHEDULER;
/

CREATE OR REPLACE PACKAGE BODY PKG_IAS_SCHEDULER AS

  C_TIME_ZONE CONSTANT VARCHAR2(64) := 'Asia/Karachi';

  ------------------------------------------------------------------
  -- Convert a timestamp-with-time-zone into IAS local DATE.
  ------------------------------------------------------------------
  FUNCTION F_LOCAL_DATE
  (
      P_VALUE IN TIMESTAMP WITH TIME ZONE
  )
  RETURN DATE
  IS
      V_DATE DATE;
  BEGIN
      SELECT CAST(P_VALUE AT TIME ZONE C_TIME_ZONE AS DATE)
        INTO V_DATE
        FROM DUAL;

      RETURN V_DATE;
  END F_LOCAL_DATE;

  ------------------------------------------------------------------
  -- Build an IAS-local timestamp.
  ------------------------------------------------------------------
  FUNCTION F_MAKE_TIMESTAMP
  (
      P_DAY    IN DATE,
      P_HOUR   IN NUMBER,
      P_MINUTE IN NUMBER
  )
  RETURN TIMESTAMP WITH TIME ZONE
  IS
  BEGIN
      RETURN FROM_TZ
      (
          CAST
          (
              TRUNC(P_DAY)
              + ((P_HOUR * 60) + P_MINUTE) / 1440
              AS TIMESTAMP
          ),
          C_TIME_ZONE
      );
  END F_MAKE_TIMESTAMP;

  ------------------------------------------------------------------
  -- Return requested day within month.
  -- If DAY_OF_MONTH exceeds month length, use month end.
  ------------------------------------------------------------------
  FUNCTION F_MONTH_DAY
  (
      P_MONTH_START IN DATE,
      P_DAY_OF_MONTH IN NUMBER
  )
  RETURN DATE
  IS
      V_LAST_DAY_NUMBER NUMBER;
  BEGIN

      V_LAST_DAY_NUMBER :=
          TO_NUMBER(TO_CHAR(LAST_DAY(P_MONTH_START), 'DD'));

      RETURN TRUNC(P_MONTH_START, 'MM')
             + LEAST(P_DAY_OF_MONTH, V_LAST_DAY_NUMBER)
             - 1;

  END F_MONTH_DAY;

  ------------------------------------------------------------------
  -- Validate Y/N.
  ------------------------------------------------------------------
  PROCEDURE VALIDATE_FLAG
  (
      P_VALUE      IN CHAR,
      P_FIELD_NAME IN VARCHAR2
  )
  IS
  BEGIN

      IF P_VALUE IS NULL OR UPPER(P_VALUE) NOT IN ('Y', 'N') THEN
          RAISE_APPLICATION_ERROR
          (
              -20200,
              P_FIELD_NAME || ' must be Y or N.'
          );
      END IF;

  END VALIDATE_FLAG;

  ------------------------------------------------------------------
  -- Validate schedule dates.
  ------------------------------------------------------------------
  PROCEDURE VALIDATE_DATES
  (
      P_EFFECTIVE_FROM IN DATE,
      P_EFFECTIVE_TO   IN DATE
  )
  IS
  BEGIN

      IF P_EFFECTIVE_FROM IS NULL THEN
          RAISE_APPLICATION_ERROR
          (
              -20201,
              'Effective From is required.'
          );
      END IF;

      IF P_EFFECTIVE_TO IS NOT NULL
         AND TRUNC(P_EFFECTIVE_TO) < TRUNC(P_EFFECTIVE_FROM) THEN

          RAISE_APPLICATION_ERROR
          (
              -20202,
              'Effective To cannot be earlier than Effective From.'
          );

      END IF;

  END VALIDATE_DATES;

  ------------------------------------------------------------------
  -- Validate frequency-specific values.
  --
  -- DAY_OF_WEEK:
  -- 1 = Monday
  -- 2 = Tuesday
  -- ...
  -- 7 = Sunday
  ------------------------------------------------------------------
  PROCEDURE VALIDATE_SCHEDULE
  (
      P_FREQUENCY_TYPE IN VARCHAR2,
      P_RUN_HOUR       IN NUMBER,
      P_RUN_MINUTE     IN NUMBER,
      P_DAY_OF_WEEK    IN NUMBER,
      P_DAY_OF_MONTH   IN NUMBER,
      P_PERIOD_TYPE    IN VARCHAR2,
      P_NEXT_DUE_ON    IN TIMESTAMP WITH TIME ZONE
  )
  IS
      V_FREQUENCY VARCHAR2(20) := UPPER(TRIM(P_FREQUENCY_TYPE));
      V_PERIOD    VARCHAR2(30) := UPPER(TRIM(P_PERIOD_TYPE));
  BEGIN

      IF V_FREQUENCY NOT IN
         (
             'DAILY',
             'WEEKLY',
             'MONTHLY',
             'MANUAL'
         ) THEN

          RAISE_APPLICATION_ERROR
          (
              -20203,
              'Frequency Type must be DAILY, WEEKLY, MONTHLY or MANUAL.'
          );

      END IF;

      IF P_RUN_HOUR IS NULL
         OR P_RUN_HOUR < 0
         OR P_RUN_HOUR > 23 THEN

          RAISE_APPLICATION_ERROR
          (
              -20204,
              'Run Hour must be between 0 and 23.'
          );

      END IF;

      IF P_RUN_MINUTE IS NULL
         OR P_RUN_MINUTE < 0
         OR P_RUN_MINUTE > 59 THEN

          RAISE_APPLICATION_ERROR
          (
              -20205,
              'Run Minute must be between 0 and 59.'
          );

      END IF;

      IF V_FREQUENCY = 'WEEKLY' THEN

          IF P_DAY_OF_WEEK IS NULL
             OR P_DAY_OF_WEEK < 1
             OR P_DAY_OF_WEEK > 7 THEN

              RAISE_APPLICATION_ERROR
              (
                  -20206,
                  'Weekly schedule requires Day Of Week between 1 and 7.'
              );

          END IF;

      END IF;

      IF V_FREQUENCY = 'MONTHLY' THEN

          IF P_DAY_OF_MONTH IS NULL
             OR P_DAY_OF_MONTH < 1
             OR P_DAY_OF_MONTH > 31 THEN

              RAISE_APPLICATION_ERROR
              (
                  -20207,
                  'Monthly schedule requires Day Of Month between 1 and 31.'
              );

          END IF;

      END IF;

      IF V_FREQUENCY = 'MANUAL'
         AND P_NEXT_DUE_ON IS NULL THEN

          RAISE_APPLICATION_ERROR
          (
              -20208,
              'Manual schedule requires Next Due On.'
          );

      END IF;

      IF V_PERIOD NOT IN
         (
             'NONE',
             'PREVIOUS_DAY',
             'PREVIOUS_WEEK',
             'PREVIOUS_MONTH'
         ) THEN

          RAISE_APPLICATION_ERROR
          (
              -20209,
              'Invalid Period Type.'
          );

      END IF;

  END VALIDATE_SCHEDULE;

  ------------------------------------------------------------------
  -- Calculate first due date when a schedule is created/activated.
  ------------------------------------------------------------------
  FUNCTION F_FIRST_DUE
  (
      P_FREQUENCY_TYPE IN VARCHAR2,
      P_RUN_HOUR       IN NUMBER,
      P_RUN_MINUTE     IN NUMBER,
      P_DAY_OF_WEEK    IN NUMBER,
      P_DAY_OF_MONTH   IN NUMBER,
      P_EFFECTIVE_FROM IN DATE,
      P_REFERENCE      IN TIMESTAMP WITH TIME ZONE
  )
  RETURN TIMESTAMP WITH TIME ZONE
  IS
      V_FREQUENCY VARCHAR2(20) :=
          UPPER(TRIM(P_FREQUENCY_TYPE));

      V_REFERENCE_DATE DATE;
      V_SEED_DATE      DATE;
      V_MONTH_START    DATE;

      V_CANDIDATE TIMESTAMP WITH TIME ZONE;
  BEGIN

      V_REFERENCE_DATE := F_LOCAL_DATE(P_REFERENCE);

      V_SEED_DATE :=
          GREATEST
          (
              TRUNC(V_REFERENCE_DATE),
              TRUNC(P_EFFECTIVE_FROM)
          );

      IF V_FREQUENCY = 'DAILY' THEN

          V_CANDIDATE :=
              F_MAKE_TIMESTAMP
              (
                  V_SEED_DATE,
                  P_RUN_HOUR,
                  P_RUN_MINUTE
              );

          IF V_CANDIDATE < P_REFERENCE THEN

              V_CANDIDATE :=
                  F_MAKE_TIMESTAMP
                  (
                      V_SEED_DATE + 1,
                      P_RUN_HOUR,
                      P_RUN_MINUTE
                  );

          END IF;

          RETURN V_CANDIDATE;

      ELSIF V_FREQUENCY = 'WEEKLY' THEN

          V_CANDIDATE :=
              F_MAKE_TIMESTAMP
              (
                  TRUNC(V_SEED_DATE, 'IW')
                  + P_DAY_OF_WEEK - 1,
                  P_RUN_HOUR,
                  P_RUN_MINUTE
              );

          IF V_CANDIDATE < P_REFERENCE
             OR TRUNC(F_LOCAL_DATE(V_CANDIDATE))
                < TRUNC(P_EFFECTIVE_FROM) THEN

              V_CANDIDATE :=
                  V_CANDIDATE
                  + NUMTODSINTERVAL(7, 'DAY');

          END IF;

          RETURN V_CANDIDATE;

      ELSIF V_FREQUENCY = 'MONTHLY' THEN

          V_MONTH_START :=
              TRUNC(V_SEED_DATE, 'MM');

          V_CANDIDATE :=
              F_MAKE_TIMESTAMP
              (
                  F_MONTH_DAY
                  (
                      V_MONTH_START,
                      P_DAY_OF_MONTH
                  ),
                  P_RUN_HOUR,
                  P_RUN_MINUTE
              );

          IF V_CANDIDATE < P_REFERENCE
             OR TRUNC(F_LOCAL_DATE(V_CANDIDATE))
                < TRUNC(P_EFFECTIVE_FROM) THEN

              V_MONTH_START :=
                  ADD_MONTHS(V_MONTH_START, 1);

              V_CANDIDATE :=
                  F_MAKE_TIMESTAMP
                  (
                      F_MONTH_DAY
                      (
                          V_MONTH_START,
                          P_DAY_OF_MONTH
                      ),
                      P_RUN_HOUR,
                      P_RUN_MINUTE
                  );

          END IF;

          RETURN V_CANDIDATE;

      END IF;

      RETURN NULL;

  END F_FIRST_DUE;

  ------------------------------------------------------------------
  -- Calculate normal next scheduled execution from the ORIGINAL
  -- scheduled slot, not from the actual completion time.
  ------------------------------------------------------------------
  FUNCTION F_NEXT_DUE_AFTER
  (
      P_FREQUENCY_TYPE IN VARCHAR2,
      P_RUN_HOUR       IN NUMBER,
      P_RUN_MINUTE     IN NUMBER,
      P_DAY_OF_WEEK    IN NUMBER,
      P_DAY_OF_MONTH   IN NUMBER,
      P_SCHEDULED_FOR  IN TIMESTAMP WITH TIME ZONE
  )
  RETURN TIMESTAMP WITH TIME ZONE
  IS
      V_FREQUENCY VARCHAR2(20) :=
          UPPER(TRIM(P_FREQUENCY_TYPE));

      V_LOCAL_DATE  DATE;
      V_MONTH_START DATE;

      V_CANDIDATE TIMESTAMP WITH TIME ZONE;
  BEGIN

      V_LOCAL_DATE :=
          F_LOCAL_DATE(P_SCHEDULED_FOR);

      IF V_FREQUENCY = 'DAILY' THEN

          RETURN
              F_MAKE_TIMESTAMP
              (
                  TRUNC(V_LOCAL_DATE) + 1,
                  P_RUN_HOUR,
                  P_RUN_MINUTE
              );

      ELSIF V_FREQUENCY = 'WEEKLY' THEN

          RETURN
              F_MAKE_TIMESTAMP
              (
                  TRUNC(V_LOCAL_DATE, 'IW')
                  + P_DAY_OF_WEEK - 1
                  + 7,
                  P_RUN_HOUR,
                  P_RUN_MINUTE
              );

      ELSIF V_FREQUENCY = 'MONTHLY' THEN

          V_MONTH_START :=
              ADD_MONTHS
              (
                  TRUNC(V_LOCAL_DATE, 'MM'),
                  1
              );

          RETURN
              F_MAKE_TIMESTAMP
              (
                  F_MONTH_DAY
                  (
                      V_MONTH_START,
                      P_DAY_OF_MONTH
                  ),
                  P_RUN_HOUR,
                  P_RUN_MINUTE
              );

      END IF;

      RETURN NULL;

  END F_NEXT_DUE_AFTER;

  ------------------------------------------------------------------
  -- Calculate the BUSINESS / COMPLIANCE REVIEW PERIOD.
  --
  -- PERIOD_TO is exclusive.
  --
  -- Example:
  -- Monthly scheduled for 01-Oct-2026:
  -- PERIOD_FROM = 01-Sep-2026
  -- PERIOD_TO   = 01-Oct-2026
  --
  -- Displayed to user as:
  -- 01-Sep-2026 to 30-Sep-2026
  ------------------------------------------------------------------
  PROCEDURE CALCULATE_PERIOD
  (
      P_PERIOD_TYPE   IN VARCHAR2,
      P_SCHEDULED_FOR IN TIMESTAMP WITH TIME ZONE,
      P_PERIOD_FROM   OUT DATE,
      P_PERIOD_TO     OUT DATE
  )
  IS
      V_PERIOD     VARCHAR2(30) :=
          UPPER(TRIM(P_PERIOD_TYPE));

      V_LOCAL_DATE DATE;
  BEGIN

      P_PERIOD_FROM := NULL;
      P_PERIOD_TO   := NULL;

      IF V_PERIOD = 'NONE' THEN
          RETURN;
      END IF;

      V_LOCAL_DATE :=
          F_LOCAL_DATE(P_SCHEDULED_FOR);

      IF V_PERIOD = 'PREVIOUS_DAY' THEN

          P_PERIOD_TO :=
              TRUNC(V_LOCAL_DATE);

          P_PERIOD_FROM :=
              P_PERIOD_TO - 1;

      ELSIF V_PERIOD = 'PREVIOUS_WEEK' THEN

          P_PERIOD_TO :=
              TRUNC(V_LOCAL_DATE);

          P_PERIOD_FROM :=
              P_PERIOD_TO - 7;

      ELSIF V_PERIOD = 'PREVIOUS_MONTH' THEN

          P_PERIOD_TO :=
              TRUNC(V_LOCAL_DATE, 'MM');

          P_PERIOD_FROM :=
              ADD_MONTHS(P_PERIOD_TO, -1);

      ELSE

          RAISE_APPLICATION_ERROR
          (
              -20210,
              'Unsupported Period Type.'
          );

      END IF;

  END CALCULATE_PERIOD;

  ------------------------------------------------------------------
  -- Save controlled job definition.
  ------------------------------------------------------------------
  PROCEDURE P_SAVE_JOB_DEFINITION
  (
      P_JOB_ID              IN NUMBER,
      P_JOB_CODE            IN VARCHAR2,
      P_JOB_NAME            IN VARCHAR2,
      P_EXECUTION_TYPE      IN VARCHAR2,
      P_PACKAGE_NAME        IN VARCHAR2,
      P_PROCEDURE_NAME      IN VARCHAR2,
      P_APPLICATION_HANDLER IN VARCHAR2,
      P_DESCRIPTION         IN VARCHAR2,
      P_UPDATED_BY          IN VARCHAR2,
      P_SAVED_JOB_ID        OUT NUMBER
  )
  IS
      V_EXECUTION_TYPE VARCHAR2(20) :=
          UPPER(TRIM(P_EXECUTION_TYPE));

      V_PACKAGE_NAME   VARCHAR2(128);
      V_PROCEDURE_NAME VARCHAR2(128);
      V_HANDLER        VARCHAR2(150);
  BEGIN

      IF TRIM(P_JOB_CODE) IS NULL THEN
          RAISE_APPLICATION_ERROR
          (
              -20211,
              'Job Code is required.'
          );
      END IF;

      IF TRIM(P_JOB_NAME) IS NULL THEN
          RAISE_APPLICATION_ERROR
          (
              -20212,
              'Job Name is required.'
          );
      END IF;

      IF V_EXECUTION_TYPE NOT IN
         (
             'DATABASE',
             'APPLICATION'
         ) THEN

          RAISE_APPLICATION_ERROR
          (
              -20213,
              'Execution Type must be DATABASE or APPLICATION.'
          );

      END IF;

      IF V_EXECUTION_TYPE = 'DATABASE' THEN

          IF TRIM(P_PROCEDURE_NAME) IS NULL THEN
              RAISE_APPLICATION_ERROR
              (
                  -20214,
                  'Database job requires Procedure Name.'
              );
          END IF;

          IF TRIM(P_PACKAGE_NAME) IS NOT NULL THEN
              V_PACKAGE_NAME :=
                  UPPER
                  (
                      DBMS_ASSERT.SIMPLE_SQL_NAME
                      (
                          TRIM(P_PACKAGE_NAME)
                      )
                  );
          END IF;

          V_PROCEDURE_NAME :=
              UPPER
              (
                  DBMS_ASSERT.SIMPLE_SQL_NAME
                  (
                      TRIM(P_PROCEDURE_NAME)
                  )
              );

          V_HANDLER := NULL;

      ELSE

          IF TRIM(P_APPLICATION_HANDLER) IS NULL THEN
              RAISE_APPLICATION_ERROR
              (
                  -20215,
                  'Application job requires Application Handler.'
              );
          END IF;

          V_PACKAGE_NAME   := NULL;
          V_PROCEDURE_NAME := NULL;

          V_HANDLER :=
              UPPER(TRIM(P_APPLICATION_HANDLER));

      END IF;

      IF P_JOB_ID IS NULL THEN

          P_SAVED_JOB_ID :=
              SEQ_IAS_JOB_DEFINITION.NEXTVAL;

          INSERT INTO T_IAS_JOB_DEFINITION
          (
              JOB_ID,
              JOB_CODE,
              JOB_NAME,
              EXECUTION_TYPE,
              PACKAGE_NAME,
              PROCEDURE_NAME,
              APPLICATION_HANDLER,
              DESCRIPTION,
              CREATED_BY,
              CREATED_ON
          )
          VALUES
          (
              P_SAVED_JOB_ID,
              UPPER(TRIM(P_JOB_CODE)),
              TRIM(P_JOB_NAME),
              V_EXECUTION_TYPE,
              V_PACKAGE_NAME,
              V_PROCEDURE_NAME,
              V_HANDLER,
              TRIM(P_DESCRIPTION),
              NVL(TRIM(P_UPDATED_BY), USER),
              SYSDATE
          );

      ELSE

          UPDATE T_IAS_JOB_DEFINITION
             SET JOB_CODE =
                     UPPER(TRIM(P_JOB_CODE)),
                 JOB_NAME =
                     TRIM(P_JOB_NAME),
                 EXECUTION_TYPE =
                     V_EXECUTION_TYPE,
                 PACKAGE_NAME =
                     V_PACKAGE_NAME,
                 PROCEDURE_NAME =
                     V_PROCEDURE_NAME,
                 APPLICATION_HANDLER =
                     V_HANDLER,
                 DESCRIPTION =
                     TRIM(P_DESCRIPTION),
                 UPDATED_BY =
                     NVL(TRIM(P_UPDATED_BY), USER),
                 UPDATED_ON =
                     SYSDATE
           WHERE JOB_ID = P_JOB_ID;

          IF SQL%ROWCOUNT = 0 THEN
              RAISE_APPLICATION_ERROR
              (
                  -20216,
                  'Job Definition was not found.'
              );
          END IF;

          P_SAVED_JOB_ID := P_JOB_ID;

      END IF;

      COMMIT;

  END P_SAVE_JOB_DEFINITION;

  ------------------------------------------------------------------
  -- Save / update schedule.
  ------------------------------------------------------------------
  PROCEDURE P_SAVE_SCHEDULE
  (
      P_SCHEDULE_ID            IN NUMBER,
      P_JOB_ID                 IN NUMBER,
      P_FREQUENCY_TYPE         IN VARCHAR2,
      P_RUN_HOUR               IN NUMBER,
      P_RUN_MINUTE             IN NUMBER,
      P_DAY_OF_WEEK            IN NUMBER,
      P_DAY_OF_MONTH           IN NUMBER,
      P_PERIOD_TYPE            IN VARCHAR2,
      P_EMAIL_REQUIRED         IN CHAR,
      P_IS_ACTIVE              IN CHAR,
      P_EFFECTIVE_FROM         IN DATE,
      P_EFFECTIVE_TO           IN DATE,
      P_MAX_RETRY              IN NUMBER,
      P_RETRY_INTERVAL_MINUTES IN NUMBER,
      P_NEXT_DUE_ON            IN TIMESTAMP WITH TIME ZONE,
      P_REMARKS                IN VARCHAR2,
      P_UPDATED_BY             IN VARCHAR2,
      P_SAVED_SCHEDULE_ID      OUT NUMBER
  )
  IS
      V_COUNT        NUMBER;
      V_LAST_STATUS  VARCHAR2(20);

      V_FREQUENCY VARCHAR2(20) :=
          UPPER(TRIM(P_FREQUENCY_TYPE));

      V_PERIOD VARCHAR2(30) :=
          UPPER(TRIM(P_PERIOD_TYPE));

      V_NEXT_DUE TIMESTAMP WITH TIME ZONE;

      V_NOW TIMESTAMP WITH TIME ZONE :=
          SYSTIMESTAMP;
  BEGIN

      SELECT COUNT(*)
        INTO V_COUNT
        FROM T_IAS_JOB_DEFINITION
       WHERE JOB_ID = P_JOB_ID;

      IF V_COUNT = 0 THEN
          RAISE_APPLICATION_ERROR
          (
              -20217,
              'Job Definition was not found.'
          );
      END IF;

      VALIDATE_FLAG
      (
          UPPER(P_EMAIL_REQUIRED),
          'Email Required'
      );

      VALIDATE_FLAG
      (
          UPPER(P_IS_ACTIVE),
          'Active status'
      );

      VALIDATE_DATES
      (
          P_EFFECTIVE_FROM,
          P_EFFECTIVE_TO
      );

      VALIDATE_SCHEDULE
      (
          V_FREQUENCY,
          P_RUN_HOUR,
          P_RUN_MINUTE,
          P_DAY_OF_WEEK,
          P_DAY_OF_MONTH,
          V_PERIOD,
          P_NEXT_DUE_ON
      );

      IF NVL(P_MAX_RETRY, 0) < 0 THEN
          RAISE_APPLICATION_ERROR
          (
              -20218,
              'Max Retry cannot be negative.'
          );
      END IF;

      IF NVL(P_RETRY_INTERVAL_MINUTES, 0) <= 0 THEN
          RAISE_APPLICATION_ERROR
          (
              -20219,
              'Retry Interval Minutes must be greater than zero.'
          );
      END IF;

      IF P_NEXT_DUE_ON IS NOT NULL THEN

          V_NEXT_DUE :=
              P_NEXT_DUE_ON;

      ELSIF V_FREQUENCY <> 'MANUAL' THEN

          V_NEXT_DUE :=
              F_FIRST_DUE
              (
                  V_FREQUENCY,
                  P_RUN_HOUR,
                  P_RUN_MINUTE,
                  P_DAY_OF_WEEK,
                  P_DAY_OF_MONTH,
                  P_EFFECTIVE_FROM,
                  V_NOW
              );

      END IF;

      IF V_NEXT_DUE IS NULL THEN
          RAISE_APPLICATION_ERROR
          (
              -20220,
              'Unable to calculate Next Due On.'
          );
      END IF;

      IF P_SCHEDULE_ID IS NULL THEN

          P_SAVED_SCHEDULE_ID :=
              SEQ_IAS_JOB_SCHEDULE.NEXTVAL;

          INSERT INTO T_IAS_JOB_SCHEDULE
          (
              SCHEDULE_ID,
              JOB_ID,
              FREQUENCY_TYPE,
              RUN_HOUR,
              RUN_MINUTE,
              DAY_OF_WEEK,
              DAY_OF_MONTH,
              PERIOD_TYPE,
              EMAIL_REQUIRED,
              IS_ACTIVE,
              EFFECTIVE_FROM,
              EFFECTIVE_TO,
              NEXT_DUE_ON,
              RETRY_COUNT,
              MAX_RETRY,
              RETRY_INTERVAL_MINUTES,
              REMARKS,
              CREATED_BY,
              CREATED_ON
          )
          VALUES
          (
              P_SAVED_SCHEDULE_ID,
              P_JOB_ID,
              V_FREQUENCY,
              P_RUN_HOUR,
              P_RUN_MINUTE,
              CASE
                  WHEN V_FREQUENCY = 'WEEKLY'
                  THEN P_DAY_OF_WEEK
                  ELSE NULL
              END,
              CASE
                  WHEN V_FREQUENCY = 'MONTHLY'
                  THEN P_DAY_OF_MONTH
                  ELSE NULL
              END,
              V_PERIOD,
              UPPER(P_EMAIL_REQUIRED),
              UPPER(P_IS_ACTIVE),
              TRUNC(P_EFFECTIVE_FROM),
              CASE
                  WHEN P_EFFECTIVE_TO IS NULL
                  THEN NULL
                  ELSE TRUNC(P_EFFECTIVE_TO)
              END,
              V_NEXT_DUE,
              0,
              NVL(P_MAX_RETRY, 3),
              NVL(P_RETRY_INTERVAL_MINUTES, 30),
              TRIM(P_REMARKS),
              NVL(TRIM(P_UPDATED_BY), USER),
              SYSDATE
          );

      ELSE

          BEGIN

              SELECT LAST_STATUS
                INTO V_LAST_STATUS
                FROM T_IAS_JOB_SCHEDULE
               WHERE SCHEDULE_ID = P_SCHEDULE_ID
               FOR UPDATE;

          EXCEPTION
              WHEN NO_DATA_FOUND THEN
                  RAISE_APPLICATION_ERROR
                  (
                      -20221,
                      'Schedule was not found.'
                  );
          END;

          IF V_LAST_STATUS = 'RUNNING' THEN
              RAISE_APPLICATION_ERROR
              (
                  -20222,
                  'A running schedule cannot be modified.'
              );
          END IF;

          UPDATE T_IAS_JOB_SCHEDULE
             SET JOB_ID =
                     P_JOB_ID,
                 FREQUENCY_TYPE =
                     V_FREQUENCY,
                 RUN_HOUR =
                     P_RUN_HOUR,
                 RUN_MINUTE =
                     P_RUN_MINUTE,
                 DAY_OF_WEEK =
                     CASE
                         WHEN V_FREQUENCY = 'WEEKLY'
                         THEN P_DAY_OF_WEEK
                         ELSE NULL
                     END,
                 DAY_OF_MONTH =
                     CASE
                         WHEN V_FREQUENCY = 'MONTHLY'
                         THEN P_DAY_OF_MONTH
                         ELSE NULL
                     END,
                 PERIOD_TYPE =
                     V_PERIOD,
                 EMAIL_REQUIRED =
                     UPPER(P_EMAIL_REQUIRED),
                 IS_ACTIVE =
                     UPPER(P_IS_ACTIVE),
                 EFFECTIVE_FROM =
                     TRUNC(P_EFFECTIVE_FROM),
                 EFFECTIVE_TO =
                     CASE
                         WHEN P_EFFECTIVE_TO IS NULL
                         THEN NULL
                         ELSE TRUNC(P_EFFECTIVE_TO)
                     END,
                 NEXT_DUE_ON =
                     V_NEXT_DUE,
                 RETRY_COUNT =
                     0,
                 MAX_RETRY =
                     NVL(P_MAX_RETRY, 3),
                 RETRY_INTERVAL_MINUTES =
                     NVL(P_RETRY_INTERVAL_MINUTES, 30),
                 LAST_ERROR =
                     NULL,
                 REMARKS =
                     TRIM(P_REMARKS),
                 UPDATED_BY =
                     NVL(TRIM(P_UPDATED_BY), USER),
                 UPDATED_ON =
                     SYSDATE
           WHERE SCHEDULE_ID = P_SCHEDULE_ID;

          P_SAVED_SCHEDULE_ID :=
              P_SCHEDULE_ID;

      END IF;

      COMMIT;

  END P_SAVE_SCHEDULE;

  ------------------------------------------------------------------
  -- Activate / deactivate schedule.
  ------------------------------------------------------------------
  PROCEDURE P_SET_ACTIVE
  (
      P_SCHEDULE_ID IN NUMBER,
      P_IS_ACTIVE   IN CHAR,
      P_UPDATED_BY  IN VARCHAR2
  )
  IS
      V_FREQUENCY     VARCHAR2(20);
      V_RUN_HOUR      NUMBER;
      V_RUN_MINUTE    NUMBER;
      V_DAY_OF_WEEK   NUMBER;
      V_DAY_OF_MONTH  NUMBER;
      V_EFFECTIVE_FROM DATE;
      V_EFFECTIVE_TO   DATE;
      V_LAST_STATUS    VARCHAR2(20);

      V_NEXT_DUE TIMESTAMP WITH TIME ZONE;

      V_NOW TIMESTAMP WITH TIME ZONE :=
          SYSTIMESTAMP;

      V_TODAY DATE :=
          TRUNC(F_LOCAL_DATE(SYSTIMESTAMP));
  BEGIN

      VALIDATE_FLAG
      (
          UPPER(P_IS_ACTIVE),
          'Active status'
      );

      BEGIN

          SELECT FREQUENCY_TYPE,
                 RUN_HOUR,
                 RUN_MINUTE,
                 DAY_OF_WEEK,
                 DAY_OF_MONTH,
                 EFFECTIVE_FROM,
                 EFFECTIVE_TO,
                 LAST_STATUS
            INTO V_FREQUENCY,
                 V_RUN_HOUR,
                 V_RUN_MINUTE,
                 V_DAY_OF_WEEK,
                 V_DAY_OF_MONTH,
                 V_EFFECTIVE_FROM,
                 V_EFFECTIVE_TO,
                 V_LAST_STATUS
            FROM T_IAS_JOB_SCHEDULE
           WHERE SCHEDULE_ID = P_SCHEDULE_ID
           FOR UPDATE;

      EXCEPTION
          WHEN NO_DATA_FOUND THEN
              RAISE_APPLICATION_ERROR
              (
                  -20221,
                  'Schedule was not found.'
              );
      END;

      IF V_LAST_STATUS = 'RUNNING' THEN
          RAISE_APPLICATION_ERROR
          (
              -20223,
              'A running schedule cannot be activated or deactivated.'
          );
      END IF;

      IF UPPER(P_IS_ACTIVE) = 'Y' THEN

          IF V_EFFECTIVE_TO IS NOT NULL
             AND TRUNC(V_EFFECTIVE_TO) < V_TODAY THEN

              RAISE_APPLICATION_ERROR
              (
                  -20224,
                  'Schedule Effective To date has already expired.'
              );

          END IF;

          IF V_FREQUENCY <> 'MANUAL' THEN

              V_NEXT_DUE :=
                  F_FIRST_DUE
                  (
                      V_FREQUENCY,
                      V_RUN_HOUR,
                      V_RUN_MINUTE,
                      V_DAY_OF_WEEK,
                      V_DAY_OF_MONTH,
                      V_EFFECTIVE_FROM,
                      V_NOW
                  );

              UPDATE T_IAS_JOB_SCHEDULE
                 SET IS_ACTIVE = 'Y',
                     NEXT_DUE_ON = V_NEXT_DUE,
                     RETRY_COUNT = 0,
                     LAST_ERROR = NULL,
                     UPDATED_BY =
                         NVL(TRIM(P_UPDATED_BY), USER),
                     UPDATED_ON =
                         SYSDATE
               WHERE SCHEDULE_ID =
                     P_SCHEDULE_ID;

          ELSE

              UPDATE T_IAS_JOB_SCHEDULE
                 SET IS_ACTIVE = 'Y',
                     RETRY_COUNT = 0,
                     LAST_ERROR = NULL,
                     UPDATED_BY =
                         NVL(TRIM(P_UPDATED_BY), USER),
                     UPDATED_ON =
                         SYSDATE
               WHERE SCHEDULE_ID =
                     P_SCHEDULE_ID;

          END IF;

      ELSE

          UPDATE T_IAS_JOB_SCHEDULE
             SET IS_ACTIVE = 'N',
                 RETRY_COUNT = 0,
                 UPDATED_BY =
                     NVL(TRIM(P_UPDATED_BY), USER),
                 UPDATED_ON =
                     SYSDATE
           WHERE SCHEDULE_ID =
                 P_SCHEDULE_ID;

      END IF;

      COMMIT;

  END P_SET_ACTIVE;

  ------------------------------------------------------------------
  -- Return one due job and atomically claim it.
  --
  -- ASP.NET should call this repeatedly until the returned
  -- cursor contains no row.
  --
  -- FOR UPDATE SKIP LOCKED prevents two application instances
  -- from claiming the same schedule.
  ------------------------------------------------------------------
  PROCEDURE P_CLAIM_DUE_JOB
  (
      IO_CURSOR OUT SYS_REFCURSOR
  )
  IS
      V_NOW TIMESTAMP WITH TIME ZONE :=
          SYSTIMESTAMP;

      V_TODAY DATE :=
          TRUNC(F_LOCAL_DATE(SYSTIMESTAMP));

      CURSOR C_DUE IS
          SELECT S.SCHEDULE_ID,
                 S.JOB_ID,
                 S.FREQUENCY_TYPE,
                 S.RUN_HOUR,
                 S.RUN_MINUTE,
                 S.DAY_OF_WEEK,
                 S.DAY_OF_MONTH,
                 S.PERIOD_TYPE,
                 S.EMAIL_REQUIRED,
                 S.NEXT_DUE_ON,
                 S.RETRY_COUNT,
                 S.MAX_RETRY,
                 S.RETRY_INTERVAL_MINUTES,
                 S.LAST_STATUS,

                 D.JOB_CODE,
                 D.JOB_NAME,
                 D.EXECUTION_TYPE,
                 D.PACKAGE_NAME,
                 D.PROCEDURE_NAME,
                 D.APPLICATION_HANDLER

            FROM T_IAS_JOB_SCHEDULE S

            JOIN T_IAS_JOB_DEFINITION D
              ON D.JOB_ID = S.JOB_ID

           WHERE S.IS_ACTIVE = 'Y'

             AND S.EFFECTIVE_FROM <= V_TODAY

             AND
                 (
                     S.EFFECTIVE_TO IS NULL
                     OR S.EFFECTIVE_TO >= V_TODAY
                 )

             AND S.NEXT_DUE_ON <= V_NOW

             AND NVL(S.LAST_STATUS, 'READY') <> 'RUNNING'

           ORDER BY S.NEXT_DUE_ON,
                    S.SCHEDULE_ID

           FOR UPDATE OF S.LAST_STATUS SKIP LOCKED;

      V_SCHEDULE_ID            NUMBER;
      V_JOB_ID                 NUMBER;
      V_FREQUENCY_TYPE         VARCHAR2(20);
      V_RUN_HOUR               NUMBER;
      V_RUN_MINUTE             NUMBER;
      V_DAY_OF_WEEK            NUMBER;
      V_DAY_OF_MONTH           NUMBER;
      V_PERIOD_TYPE            VARCHAR2(30);
      V_EMAIL_REQUIRED         CHAR(1);
      V_NEXT_DUE_ON            TIMESTAMP WITH TIME ZONE;
      V_RETRY_COUNT            NUMBER;
      V_MAX_RETRY              NUMBER;
      V_RETRY_INTERVAL_MINUTES NUMBER;
      V_LAST_STATUS            VARCHAR2(20);

      V_JOB_CODE            VARCHAR2(100);
      V_JOB_NAME            VARCHAR2(200);
      V_EXECUTION_TYPE      VARCHAR2(20);
      V_PACKAGE_NAME        VARCHAR2(128);
      V_PROCEDURE_NAME      VARCHAR2(128);
      V_APPLICATION_HANDLER VARCHAR2(150);

      V_EXECUTION_ID NUMBER;
      V_EXECUTION_KEY VARCHAR2(200);

      V_SCHEDULED_FOR TIMESTAMP WITH TIME ZONE;

      V_PERIOD_FROM DATE;
      V_PERIOD_TO   DATE;

      V_RETRY_NO NUMBER;
  BEGIN

      OPEN C_DUE;

      FETCH C_DUE
       INTO V_SCHEDULE_ID,
            V_JOB_ID,
            V_FREQUENCY_TYPE,
            V_RUN_HOUR,
            V_RUN_MINUTE,
            V_DAY_OF_WEEK,
            V_DAY_OF_MONTH,
            V_PERIOD_TYPE,
            V_EMAIL_REQUIRED,
            V_NEXT_DUE_ON,
            V_RETRY_COUNT,
            V_MAX_RETRY,
            V_RETRY_INTERVAL_MINUTES,
            V_LAST_STATUS,
            V_JOB_CODE,
            V_JOB_NAME,
            V_EXECUTION_TYPE,
            V_PACKAGE_NAME,
            V_PROCEDURE_NAME,
            V_APPLICATION_HANDLER;

      IF C_DUE%NOTFOUND THEN

          CLOSE C_DUE;

          OPEN IO_CURSOR FOR
              SELECT E.EXECUTION_ID,
                     E.SCHEDULE_ID,
                     E.JOB_ID,
                     D.JOB_CODE,
                     D.JOB_NAME,
                     D.EXECUTION_TYPE,
                     D.PACKAGE_NAME,
                     D.PROCEDURE_NAME,
                     D.APPLICATION_HANDLER,
                     S.EMAIL_REQUIRED,
                     E.EXECUTION_KEY,
                     E.SCHEDULED_FOR,
                     E.PERIOD_FROM,
                     E.PERIOD_TO,
                     E.RETRY_NO
                FROM T_IAS_JOB_EXECUTION E
                JOIN T_IAS_JOB_DEFINITION D
                  ON D.JOB_ID = E.JOB_ID
                JOIN T_IAS_JOB_SCHEDULE S
                  ON S.SCHEDULE_ID = E.SCHEDULE_ID
               WHERE 1 = 0;

          RETURN;

      END IF;

      CLOSE C_DUE;

      V_RETRY_NO :=
          NVL(V_RETRY_COUNT, 0);

      ----------------------------------------------------------------
      -- Retry must retain the ORIGINAL scheduled slot and period.
      ----------------------------------------------------------------
      IF V_LAST_STATUS = 'FAILED'
         AND V_RETRY_NO > 0 THEN

          BEGIN

              SELECT SCHEDULED_FOR,
                     PERIOD_FROM,
                     PERIOD_TO
                INTO V_SCHEDULED_FOR,
                     V_PERIOD_FROM,
                     V_PERIOD_TO
                FROM
                (
                    SELECT SCHEDULED_FOR,
                           PERIOD_FROM,
                           PERIOD_TO
                      FROM T_IAS_JOB_EXECUTION
                     WHERE SCHEDULE_ID =
                           V_SCHEDULE_ID
                       AND STATUS = 'FAILED'
                     ORDER BY EXECUTION_ID DESC
                )
               WHERE ROWNUM = 1;

          EXCEPTION
              WHEN NO_DATA_FOUND THEN

                  ROLLBACK;

                  RAISE_APPLICATION_ERROR
                  (
                      -20225,
                      'Retry state is inconsistent: previous failed execution was not found.'
                  );
          END;

      ELSE

          V_SCHEDULED_FOR :=
              V_NEXT_DUE_ON;

          CALCULATE_PERIOD
          (
              V_PERIOD_TYPE,
              V_SCHEDULED_FOR,
              V_PERIOD_FROM,
              V_PERIOD_TO
          );

      END IF;

      V_EXECUTION_ID :=
          SEQ_IAS_JOB_EXECUTION.NEXTVAL;

      V_EXECUTION_KEY :=
          'SCH:'
          || V_SCHEDULE_ID
          || ':'
          || TO_CHAR
             (
                 F_LOCAL_DATE(V_SCHEDULED_FOR),
                 'YYYYMMDDHH24MISS'
             )
          || ':R'
          || TO_CHAR(V_RETRY_NO);

      BEGIN

          INSERT INTO T_IAS_JOB_EXECUTION
          (
              EXECUTION_ID,
              SCHEDULE_ID,
              JOB_ID,
              EXECUTION_KEY,
              SCHEDULED_FOR,
              PERIOD_FROM,
              PERIOD_TO,
              STATUS,
              STARTED_ON,
              RETRY_NO,
              CREATED_ON
          )
          VALUES
          (
              V_EXECUTION_ID,
              V_SCHEDULE_ID,
              V_JOB_ID,
              V_EXECUTION_KEY,
              V_SCHEDULED_FOR,
              V_PERIOD_FROM,
              V_PERIOD_TO,
              'RUNNING',
              V_NOW,
              V_RETRY_NO,
              V_NOW
          );

      EXCEPTION
          WHEN DUP_VAL_ON_INDEX THEN

              ROLLBACK;

              RAISE_APPLICATION_ERROR
              (
                  -20226,
                  'This scheduled execution has already been claimed.'
              );
      END;

      UPDATE T_IAS_JOB_SCHEDULE
         SET LAST_STATUS =
                 'RUNNING',
             LAST_ERROR =
                 NULL,
             UPDATED_BY =
                 'PKG_IAS_SCHEDULER',
             UPDATED_ON =
                 SYSDATE
       WHERE SCHEDULE_ID =
             V_SCHEDULE_ID;

      COMMIT;

      OPEN IO_CURSOR FOR
          SELECT E.EXECUTION_ID,
                 E.SCHEDULE_ID,
                 E.JOB_ID,
                 D.JOB_CODE,
                 D.JOB_NAME,
                 D.EXECUTION_TYPE,
                 D.PACKAGE_NAME,
                 D.PROCEDURE_NAME,
                 D.APPLICATION_HANDLER,
                 S.EMAIL_REQUIRED,
                 E.EXECUTION_KEY,
                 E.SCHEDULED_FOR,
                 E.PERIOD_FROM,
                 E.PERIOD_TO,
                 E.RETRY_NO

            FROM T_IAS_JOB_EXECUTION E

            JOIN T_IAS_JOB_DEFINITION D
              ON D.JOB_ID = E.JOB_ID

            JOIN T_IAS_JOB_SCHEDULE S
              ON S.SCHEDULE_ID = E.SCHEDULE_ID

           WHERE E.EXECUTION_ID =
                 V_EXECUTION_ID;

  EXCEPTION
      WHEN OTHERS THEN

          IF C_DUE%ISOPEN THEN
              CLOSE C_DUE;
          END IF;

          ROLLBACK;
          RAISE;

  END P_CLAIM_DUE_JOB;

  ------------------------------------------------------------------
  -- Mark execution successful.
  ------------------------------------------------------------------
  PROCEDURE P_COMPLETE_JOB
  (
      P_EXECUTION_ID      IN NUMBER,
      P_RECORDS_PROCESSED IN NUMBER,
      P_RESPONSE_MESSAGE  IN VARCHAR2
  )
  IS
      V_SCHEDULE_ID   NUMBER;
      V_STATUS        VARCHAR2(20);

      V_SCHEDULED_FOR TIMESTAMP WITH TIME ZONE;

      V_PERIOD_FROM DATE;
      V_PERIOD_TO   DATE;

      V_FREQUENCY_TYPE VARCHAR2(20);
      V_RUN_HOUR       NUMBER;
      V_RUN_MINUTE     NUMBER;
      V_DAY_OF_WEEK    NUMBER;
      V_DAY_OF_MONTH   NUMBER;
      V_EFFECTIVE_TO   DATE;

      V_NEXT_DUE TIMESTAMP WITH TIME ZONE;

      V_NEXT_ACTIVE CHAR(1) := 'Y';

      V_NOW TIMESTAMP WITH TIME ZONE :=
          SYSTIMESTAMP;
  BEGIN

      BEGIN

          SELECT SCHEDULE_ID,
                 STATUS,
                 SCHEDULED_FOR,
                 PERIOD_FROM,
                 PERIOD_TO
            INTO V_SCHEDULE_ID,
                 V_STATUS,
                 V_SCHEDULED_FOR,
                 V_PERIOD_FROM,
                 V_PERIOD_TO
            FROM T_IAS_JOB_EXECUTION
           WHERE EXECUTION_ID =
                 P_EXECUTION_ID
           FOR UPDATE;

      EXCEPTION
          WHEN NO_DATA_FOUND THEN
              RAISE_APPLICATION_ERROR
              (
                  -20227,
                  'Execution was not found.'
              );
      END;

      IF V_STATUS <> 'RUNNING' THEN
          RAISE_APPLICATION_ERROR
          (
              -20228,
              'Only a RUNNING execution can be completed.'
          );
      END IF;

      SELECT FREQUENCY_TYPE,
             RUN_HOUR,
             RUN_MINUTE,
             DAY_OF_WEEK,
             DAY_OF_MONTH,
             EFFECTIVE_TO

        INTO V_FREQUENCY_TYPE,
             V_RUN_HOUR,
             V_RUN_MINUTE,
             V_DAY_OF_WEEK,
             V_DAY_OF_MONTH,
             V_EFFECTIVE_TO

        FROM T_IAS_JOB_SCHEDULE

       WHERE SCHEDULE_ID =
             V_SCHEDULE_ID

       FOR UPDATE;

      UPDATE T_IAS_JOB_EXECUTION
         SET STATUS =
                 'SUCCESS',
             COMPLETED_ON =
                 V_NOW,
             RECORDS_PROCESSED =
                 P_RECORDS_PROCESSED,
             RESPONSE_MESSAGE =
                 SUBSTR(P_RESPONSE_MESSAGE, 1, 2000),
             ERROR_MESSAGE =
                 NULL
       WHERE EXECUTION_ID =
             P_EXECUTION_ID;

      IF V_FREQUENCY_TYPE = 'MANUAL' THEN

          V_NEXT_DUE :=
              V_SCHEDULED_FOR;

          V_NEXT_ACTIVE :=
              'N';

      ELSE

          V_NEXT_DUE :=
              F_NEXT_DUE_AFTER
              (
                  V_FREQUENCY_TYPE,
                  V_RUN_HOUR,
                  V_RUN_MINUTE,
                  V_DAY_OF_WEEK,
                  V_DAY_OF_MONTH,
                  V_SCHEDULED_FOR
              );

          IF V_EFFECTIVE_TO IS NOT NULL
             AND TRUNC(F_LOCAL_DATE(V_NEXT_DUE))
                 > TRUNC(V_EFFECTIVE_TO) THEN

              V_NEXT_ACTIVE :=
                  'N';

          END IF;

      END IF;

      UPDATE T_IAS_JOB_SCHEDULE
         SET LAST_RUN_ON =
                 V_NOW,
             LAST_STATUS =
                 'SUCCESS',
             LAST_PERIOD_FROM =
                 V_PERIOD_FROM,
             LAST_PERIOD_TO =
                 V_PERIOD_TO,
             NEXT_DUE_ON =
                 V_NEXT_DUE,
             RETRY_COUNT =
                 0,
             LAST_ERROR =
                 NULL,
             IS_ACTIVE =
                 V_NEXT_ACTIVE,
             UPDATED_BY =
                 'PKG_IAS_SCHEDULER',
             UPDATED_ON =
                 SYSDATE
       WHERE SCHEDULE_ID =
             V_SCHEDULE_ID;

      COMMIT;

  EXCEPTION
      WHEN OTHERS THEN
          ROLLBACK;
          RAISE;

  END P_COMPLETE_JOB;

  ------------------------------------------------------------------
  -- Mark execution failed.
  --
  -- Retry uses NEXT_DUE_ON only as next ATTEMPT time.
  -- Original Scheduled For / Compliance Review Period are retained
  -- from the failed execution.
  ------------------------------------------------------------------
  PROCEDURE P_FAIL_JOB
  (
      P_EXECUTION_ID IN NUMBER,
      P_ERROR_MESSAGE IN VARCHAR2
  )
  IS
      V_SCHEDULE_ID   NUMBER;
      V_STATUS        VARCHAR2(20);
      V_RETRY_NO      NUMBER;

      V_SCHEDULED_FOR TIMESTAMP WITH TIME ZONE;

      V_FREQUENCY_TYPE VARCHAR2(20);
      V_RUN_HOUR       NUMBER;
      V_RUN_MINUTE     NUMBER;
      V_DAY_OF_WEEK    NUMBER;
      V_DAY_OF_MONTH   NUMBER;

      V_MAX_RETRY              NUMBER;
      V_RETRY_INTERVAL_MINUTES NUMBER;
      V_EFFECTIVE_TO           DATE;

      V_NEXT_DUE TIMESTAMP WITH TIME ZONE;

      V_NEXT_ACTIVE CHAR(1) := 'Y';

      V_NOW TIMESTAMP WITH TIME ZONE :=
          SYSTIMESTAMP;
  BEGIN

      BEGIN

          SELECT SCHEDULE_ID,
                 STATUS,
                 RETRY_NO,
                 SCHEDULED_FOR
            INTO V_SCHEDULE_ID,
                 V_STATUS,
                 V_RETRY_NO,
                 V_SCHEDULED_FOR
            FROM T_IAS_JOB_EXECUTION
           WHERE EXECUTION_ID =
                 P_EXECUTION_ID
           FOR UPDATE;

      EXCEPTION
          WHEN NO_DATA_FOUND THEN
              RAISE_APPLICATION_ERROR
              (
                  -20227,
                  'Execution was not found.'
              );
      END;

      IF V_STATUS <> 'RUNNING' THEN
          RAISE_APPLICATION_ERROR
          (
              -20229,
              'Only a RUNNING execution can be failed.'
          );
      END IF;

      SELECT FREQUENCY_TYPE,
             RUN_HOUR,
             RUN_MINUTE,
             DAY_OF_WEEK,
             DAY_OF_MONTH,
             MAX_RETRY,
             RETRY_INTERVAL_MINUTES,
             EFFECTIVE_TO

        INTO V_FREQUENCY_TYPE,
             V_RUN_HOUR,
             V_RUN_MINUTE,
             V_DAY_OF_WEEK,
             V_DAY_OF_MONTH,
             V_MAX_RETRY,
             V_RETRY_INTERVAL_MINUTES,
             V_EFFECTIVE_TO

        FROM T_IAS_JOB_SCHEDULE

       WHERE SCHEDULE_ID =
             V_SCHEDULE_ID

       FOR UPDATE;

      UPDATE T_IAS_JOB_EXECUTION
         SET STATUS =
                 'FAILED',
             COMPLETED_ON =
                 V_NOW,
             ERROR_MESSAGE =
                 SUBSTR(P_ERROR_MESSAGE, 1, 4000)
       WHERE EXECUTION_ID =
             P_EXECUTION_ID;

      ----------------------------------------------------------------
      -- Retry same business period.
      ----------------------------------------------------------------
      IF V_RETRY_NO < NVL(V_MAX_RETRY, 0) THEN

          V_NEXT_DUE :=
              V_NOW
              + NUMTODSINTERVAL
                (
                    NVL(V_RETRY_INTERVAL_MINUTES, 30),
                    'MINUTE'
                );

          UPDATE T_IAS_JOB_SCHEDULE
             SET LAST_STATUS =
                     'FAILED',
                 RETRY_COUNT =
                     V_RETRY_NO + 1,
                 NEXT_DUE_ON =
                     V_NEXT_DUE,
                 LAST_ERROR =
                     SUBSTR(P_ERROR_MESSAGE, 1, 2000),
                 UPDATED_BY =
                     'PKG_IAS_SCHEDULER',
                 UPDATED_ON =
                     SYSDATE
           WHERE SCHEDULE_ID =
                 V_SCHEDULE_ID;

      ----------------------------------------------------------------
      -- Retry limit exhausted.
      -- Advance to next NORMAL scheduled slot.
      ----------------------------------------------------------------
      ELSE

          IF V_FREQUENCY_TYPE = 'MANUAL' THEN

              V_NEXT_DUE :=
                  V_SCHEDULED_FOR;

              V_NEXT_ACTIVE :=
                  'N';

          ELSE

              V_NEXT_DUE :=
                  F_NEXT_DUE_AFTER
                  (
                      V_FREQUENCY_TYPE,
                      V_RUN_HOUR,
                      V_RUN_MINUTE,
                      V_DAY_OF_WEEK,
                      V_DAY_OF_MONTH,
                      V_SCHEDULED_FOR
                  );

              IF V_EFFECTIVE_TO IS NOT NULL
                 AND TRUNC(F_LOCAL_DATE(V_NEXT_DUE))
                     > TRUNC(V_EFFECTIVE_TO) THEN

                  V_NEXT_ACTIVE :=
                      'N';

              END IF;

          END IF;

          UPDATE T_IAS_JOB_SCHEDULE
             SET LAST_STATUS =
                     'FAILED',
                 RETRY_COUNT =
                     0,
                 NEXT_DUE_ON =
                     V_NEXT_DUE,
                 LAST_ERROR =
                     SUBSTR(P_ERROR_MESSAGE, 1, 2000),
                 IS_ACTIVE =
                     V_NEXT_ACTIVE,
                 UPDATED_BY =
                     'PKG_IAS_SCHEDULER',
                 UPDATED_ON =
                     SYSDATE
           WHERE SCHEDULE_ID =
                 V_SCHEDULE_ID;

      END IF;

      COMMIT;

  EXCEPTION
      WHEN OTHERS THEN
          ROLLBACK;
          RAISE;

  END P_FAIL_JOB;

  ------------------------------------------------------------------
  -- Administration / monitoring list.
  ------------------------------------------------------------------
  PROCEDURE P_GET_SCHEDULES
  (
      IO_CURSOR OUT SYS_REFCURSOR
  )
  IS
  BEGIN

      OPEN IO_CURSOR FOR
          SELECT D.JOB_ID,
                 D.JOB_CODE,
                 D.JOB_NAME,
                 D.EXECUTION_TYPE,
                 D.PACKAGE_NAME,
                 D.PROCEDURE_NAME,
                 D.APPLICATION_HANDLER,
                 D.DESCRIPTION,

                 S.SCHEDULE_ID,
                 S.FREQUENCY_TYPE,
                 S.RUN_HOUR,
                 S.RUN_MINUTE,
                 S.DAY_OF_WEEK,
                 S.DAY_OF_MONTH,
                 S.PERIOD_TYPE,
                 S.EMAIL_REQUIRED,
                 S.IS_ACTIVE,
                 S.EFFECTIVE_FROM,
                 S.EFFECTIVE_TO,

                 S.LAST_RUN_ON,
                 S.LAST_STATUS,
                 S.LAST_PERIOD_FROM,
                 S.LAST_PERIOD_TO,

                 S.NEXT_DUE_ON,

                 S.RETRY_COUNT,
                 S.MAX_RETRY,
                 S.RETRY_INTERVAL_MINUTES,

                 S.LAST_ERROR,
                 S.REMARKS,

                 S.CREATED_BY,
                 S.CREATED_ON,
                 S.UPDATED_BY,
                 S.UPDATED_ON,

                 CASE
                     WHEN S.IS_ACTIVE = 'Y'
                      AND S.NEXT_DUE_ON <= SYSTIMESTAMP
                      AND NVL(S.LAST_STATUS, 'READY') <> 'RUNNING'
                     THEN 'Y'
                     ELSE 'N'
                 END AS IS_DUE

            FROM T_IAS_JOB_DEFINITION D

            JOIN T_IAS_JOB_SCHEDULE S
              ON S.JOB_ID = D.JOB_ID

           ORDER BY S.IS_ACTIVE DESC,
                    S.NEXT_DUE_ON,
                    D.JOB_NAME;

  END P_GET_SCHEDULES;

  ------------------------------------------------------------------
  -- Execution history for future administration screen.
  ------------------------------------------------------------------
  PROCEDURE P_GET_EXECUTION_HISTORY
  (
      P_SCHEDULE_ID IN NUMBER DEFAULT NULL,
      P_FROM_DATE    IN DATE   DEFAULT NULL,
      P_TO_DATE      IN DATE   DEFAULT NULL,
      IO_CURSOR      OUT SYS_REFCURSOR
  )
  IS
  BEGIN

      OPEN IO_CURSOR FOR
          SELECT E.EXECUTION_ID,
                 E.SCHEDULE_ID,
                 E.JOB_ID,

                 D.JOB_CODE,
                 D.JOB_NAME,

                 E.EXECUTION_KEY,
                 E.SCHEDULED_FOR,

                 E.PERIOD_FROM,
                 E.PERIOD_TO,

                 E.STATUS,
                 E.STARTED_ON,
                 E.COMPLETED_ON,
                 E.RETRY_NO,

                 E.RECORDS_PROCESSED,
                 E.RESPONSE_MESSAGE,
                 E.ERROR_MESSAGE,

                 E.CREATED_ON

            FROM T_IAS_JOB_EXECUTION E

            JOIN T_IAS_JOB_DEFINITION D
              ON D.JOB_ID = E.JOB_ID

           WHERE
                 (
                     P_SCHEDULE_ID IS NULL
                     OR E.SCHEDULE_ID = P_SCHEDULE_ID
                 )

             AND
                 (
                     P_FROM_DATE IS NULL
                     OR CAST
                        (
                            E.CREATED_ON
                            AT TIME ZONE C_TIME_ZONE
                            AS DATE
                        ) >= P_FROM_DATE
                 )

             AND
                 (
                     P_TO_DATE IS NULL
                     OR CAST
                        (
                            E.CREATED_ON
                            AT TIME ZONE C_TIME_ZONE
                            AS DATE
                        ) < P_TO_DATE
                 )

           ORDER BY E.EXECUTION_ID DESC;

  END P_GET_EXECUTION_HISTORY;

END PKG_IAS_SCHEDULER;
/