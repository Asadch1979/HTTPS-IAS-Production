CREATE OR REPLACE PACKAGE PKG_SYS AS
  /* Toggle / read application maintenance state (single row: T_APP_MAINTENANCE ID=1) */

  -- Setter
  PROCEDURE P_SET_MAINTENANCE(
    P_IS_ON            IN NUMBER,        -- 0 or 1
    P_MESSAGE          IN VARCHAR2,      -- up to 1000 chars
    P_RETRY_AFTER_MIN  IN NUMBER,        -- 0..10080 (7 days), nullable
    P_ALLOW_IPS        IN VARCHAR2,      -- CSV up to 2000 chars, nullable
    P_END_AT           IN DATE,          -- nullable
    P_UPDATED_BY       IN VARCHAR2       -- who changed it
  );

  -- Getter (OUT params for easy ODP.NET mapping)
  PROCEDURE P_GET_MAINTENANCE(
    O_IS_ON            OUT NUMBER,
    O_MESSAGE          OUT VARCHAR2,
    O_RETRY_AFTER_MIN  OUT NUMBER,
    O_ALLOW_IPS        OUT VARCHAR2,
    O_END_AT           OUT DATE,
    O_UPDATED_ON       OUT DATE,
    O_UPDATED_BY       OUT VARCHAR2
  );

  -- Optional: same as a ref cursor (useful for ad-hoc queries)
  PROCEDURE P_GET_MAINTENANCE_RC(
    IO_CURSOR          OUT SYS_REFCURSOR
  );
END PKG_SYS;

CREATE OR REPLACE PACKAGE BODY PKG_SYS AS


  /* Normalize comma-separated IP list:
     - trim spaces around commas
     - collapse multiple commas
     - remove leading/trailing commas */
  FUNCTION FN_NORMALIZE_IPS(P_IPS IN VARCHAR2) RETURN VARCHAR2 IS
    V_OUT VARCHAR2(2000);
  BEGIN
    IF P_IPS IS NULL THEN
      RETURN NULL;
    END IF;
    V_OUT := REGEXP_REPLACE(P_IPS, '\s*,\s*', ',');
    V_OUT := REGEXP_REPLACE(V_OUT, ',{2,}', ',');
    V_OUT := REGEXP_REPLACE(V_OUT, '(^,)|(,$)', '');
    RETURN V_OUT;
  END FN_NORMALIZE_IPS;

  PROCEDURE P_SET_MAINTENANCE(
    P_IS_ON            IN NUMBER,
    P_MESSAGE          IN VARCHAR2,
    P_RETRY_AFTER_MIN  IN NUMBER,
    P_ALLOW_IPS        IN VARCHAR2,
    P_END_AT           IN DATE,
    P_UPDATED_BY       IN VARCHAR2
  ) IS
    V_EXISTS   NUMBER;
    V_IS_ON    NUMBER := NVL(P_IS_ON, 0);
    V_MESSAGE  VARCHAR2(1000) := SUBSTR(P_MESSAGE, 1, 1000);
    V_RETRY    NUMBER := P_RETRY_AFTER_MIN;
    V_IPS      VARCHAR2(2000) := FN_NORMALIZE_IPS(P_ALLOW_IPS);
    V_END_AT   DATE := P_END_AT;
  BEGIN
    -- Validate
    IF V_IS_ON NOT IN (0,1) THEN
      RAISE_APPLICATION_ERROR(-20001, 'P_IS_ON must be 0 or 1');
    END IF;

    IF V_RETRY IS NOT NULL AND (V_RETRY < 0 OR V_RETRY > 10080) THEN
      RAISE_APPLICATION_ERROR(-20002, 'P_RETRY_AFTER_MIN must be between 0 and 10080');
    END IF;

    -- If end time already passed, force to online
    IF V_IS_ON = 1 AND V_END_AT IS NOT NULL AND V_END_AT < SYSDATE THEN
      V_IS_ON := 0;
    END IF;

    SELECT COUNT(*) INTO V_EXISTS FROM T_APP_MAINTENANCE WHERE ID = 1;

    IF V_EXISTS = 0 THEN
      INSERT INTO T_APP_MAINTENANCE
        (ID, IS_ON, MESSAGE, RETRY_AFTER_MIN, ALLOW_IPS, END_AT, UPDATED_BY, UPDATED_ON)
      VALUES
        (1,  V_IS_ON, V_MESSAGE, V_RETRY, V_IPS, V_END_AT, P_UPDATED_BY, SYSDATE);
    ELSE
      UPDATE T_APP_MAINTENANCE
         SET IS_ON            = V_IS_ON,
             MESSAGE          = V_MESSAGE,
             RETRY_AFTER_MIN  = V_RETRY,
             ALLOW_IPS        = V_IPS,
             END_AT           = V_END_AT,
             UPDATED_BY       = P_UPDATED_BY,
             UPDATED_ON       = SYSDATE
       WHERE ID = 1;
    END IF;

    COMMIT;
  END P_SET_MAINTENANCE;

  PROCEDURE P_GET_MAINTENANCE(
    O_IS_ON            OUT NUMBER,
    O_MESSAGE          OUT VARCHAR2,
    O_RETRY_AFTER_MIN  OUT NUMBER,
    O_ALLOW_IPS        OUT VARCHAR2,
    O_END_AT           OUT DATE,
    O_UPDATED_ON       OUT DATE,
    O_UPDATED_BY       OUT VARCHAR2
  ) IS
  BEGIN
    SELECT IS_ON,
           MESSAGE,
           RETRY_AFTER_MIN,
           ALLOW_IPS,
           CASE
             WHEN IS_ON = 1 AND END_AT IS NOT NULL AND END_AT < SYSDATE THEN NULL
             ELSE END_AT
           END AS END_AT,
           UPDATED_ON,
           UPDATED_BY
      INTO O_IS_ON,
           O_MESSAGE,
           O_RETRY_AFTER_MIN,
           O_ALLOW_IPS,
           O_END_AT,
           O_UPDATED_ON,
           O_UPDATED_BY
      FROM T_APP_MAINTENANCE
     WHERE ID = 1;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      O_IS_ON := 0;
      O_MESSAGE := NULL;
      O_RETRY_AFTER_MIN := NULL;
      O_ALLOW_IPS := NULL;
      O_END_AT := NULL;
      O_UPDATED_ON := NULL;
      O_UPDATED_BY := NULL;
  END P_GET_MAINTENANCE;

  PROCEDURE P_GET_MAINTENANCE_RC(
    IO_CURSOR OUT SYS_REFCURSOR
  ) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT IS_ON,
             MESSAGE,
             RETRY_AFTER_MIN,
             ALLOW_IPS,
             CASE
               WHEN IS_ON = 1 AND END_AT IS NOT NULL AND END_AT < SYSDATE THEN NULL
               ELSE END_AT
             END AS END_AT,
             UPDATED_ON,
             UPDATED_BY
        FROM T_APP_MAINTENANCE
       WHERE ID = 1;
  END P_GET_MAINTENANCE_RC;

END PKG_SYS;

--GRANT EXECUTE ON PKG_SYS TO <APP_SCHEMA>;

