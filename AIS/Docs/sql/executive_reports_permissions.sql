-- IAS Executive Reports A-E: SUPER USER ONLY production evaluation.
-- Run manually as IAS schema owner after reviewing current master API/grants.
-- Explicitly limited to GET /api/ExecutiveReports/GetReport[A-E].
-- NO modifications to old report routes or any other IAS APIs.
--
-- Verified Super User role ID = 1; all other grants to these five new report
-- GET URLs are set INACTIVE ('N'). Existing report API master IS_ACTIVE values
-- are PRESERVED. Newly created report API master records start INACTIVE.
-- This avoids the previous script's dangerous ON MATCH UPDATE to 'N'.
--
-- To make previously disabled reports available for authorized Super User
-- evaluation after validating Oracle report history, independently approve
-- and set V_ACTIVATE_VERIFIED_REPORTS to 'Y' below. The default 'N'
-- does NOT disable existing reports and does NOT activate unverified ones.
--
-- No production database operation occurs merely by merging this source file.
-- Execute as SQL Developer F5 script/SQL*Plus, not individual F8 statements.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET SERVEROUTPUT ON;
DECLARE
  C_SUPER_USER CONSTANT NUMBER := 1;
  C_NEW_API_ACTIVE CONSTANT CHAR(1) := 'N';
  V_ACTIVATE_VERIFIED_REPORTS CHAR(1) := 'N';
  V_API_ID NUMBER;
  V_FOUND NUMBER;
  V_CONFLICTS NUMBER;
  V_NEW_APIS NUMBER := 0;
  V_REVOKED_GRANTS NUMBER := 0;
  V_REGISTERED_COUNT NUMBER := 0;
  V_ACTIVATED_COUNT NUMBER := 0;
BEGIN
  LOCK TABLE T_AU_API_MASTER IN EXCLUSIVE MODE;
  LOCK TABLE T_AU_ROLE_API_PERMISSION IN EXCLUSIVE MODE;

  IF V_ACTIVATE_VERIFIED_REPORTS NOT IN ('N','Y')
     OR V_ACTIVATE_VERIFIED_REPORTS IS NULL THEN
    RAISE_APPLICATION_ERROR(-20810,'Report activation must be N or Y.');
  END IF;

  SELECT COUNT(*) INTO V_FOUND
    FROM T_MENU_PAGES
   WHERE ID=1000300
     AND LOWER(TRIM(BOTH '/' FROM TRIM(PAGE_PATH)))='executiveoverview/top';
  IF V_FOUND<>1 THEN
    RAISE_APPLICATION_ERROR(-20811,
      'The verified executive TOP page 1000300 is required first.');
  END IF;

  FOR r IN (
    SELECT 'GetReportA' ACTION_NAME FROM dual
    UNION ALL SELECT 'GetReportB' FROM dual
    UNION ALL SELECT 'GetReportC' FROM dual
    UNION ALL SELECT 'GetReportD' FROM dual
    UNION ALL SELECT 'GetReportE' FROM dual
  ) LOOP
    -- AuditParaSnap/GetReportA-E is a DIFFERENT controller and is not touched.
    SELECT COUNT(*), MIN(API_ID) INTO V_FOUND,V_API_ID
      FROM T_AU_API_MASTER
     WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH))) =
           LOWER('api/ExecutiveReports/' || r.ACTION_NAME)
       AND UPPER(TRIM(HTTP_METHOD))='GET';

    IF V_FOUND>1 THEN
      RAISE_APPLICATION_ERROR(-20812,
        'Duplicate executive report GET API: ' || r.ACTION_NAME);
    ELSIF V_FOUND=0 THEN
      INSERT INTO T_AU_API_MASTER
        (ACTION_NAME, PAGE_ID, CONTROLLER_NAME,
         API_PATH, HTTP_METHOD, IS_ACTIVE, CREATED_ON)
      VALUES
        (r.ACTION_NAME, 1000300, 'ExecutiveReports',
         '/api/ExecutiveReports/' || r.ACTION_NAME,
         'GET', C_NEW_API_ACTIVE, SYSDATE);
      SELECT API_ID INTO V_API_ID FROM T_AU_API_MASTER
       WHERE LOWER(TRIM(BOTH '/' FROM TRIM(API_PATH))) =
             LOWER('api/ExecutiveReports/' || r.ACTION_NAME)
         AND UPPER(TRIM(HTTP_METHOD))='GET';
      V_NEW_APIS := V_NEW_APIS+1;
    ELSE
      SELECT COUNT(*) INTO V_CONFLICTS
        FROM T_AU_API_MASTER
       WHERE API_ID=V_API_ID
         AND (NVL(PAGE_ID,-1)<>1000300
              OR UPPER(NVL(TRIM(CONTROLLER_NAME),'#'))<>'EXECUTIVEREPORTS'
              OR UPPER(NVL(TRIM(ACTION_NAME),'#'))<>UPPER(r.ACTION_NAME));
      IF V_CONFLICTS>0 THEN
        RAISE_APPLICATION_ERROR(-20813,
          'Existing ExecutiveReports GET metadata conflict: '||r.ACTION_NAME);
      END IF;
    END IF;

    -- Revoke ONLY the five new Report A-E GETs for roles other than
    -- Super User. Do not delete older IAS report/page permissions.
    UPDATE T_AU_ROLE_API_PERMISSION
       SET IS_ACTIVE='N'
     WHERE API_ID=V_API_ID
       AND (ROLE_ID IS NULL OR ROLE_ID<>C_SUPER_USER)
       AND NVL(IS_ACTIVE,'N')<>'N';
    V_REVOKED_GRANTS := V_REVOKED_GRANTS + SQL%ROWCOUNT;

    MERGE INTO T_AU_ROLE_API_PERMISSION p
    USING (SELECT C_SUPER_USER ROLE_ID,V_API_ID API_ID FROM dual) s
       ON (p.ROLE_ID=s.ROLE_ID AND p.API_ID=s.API_ID)
    WHEN MATCHED THEN UPDATE SET p.IS_ACTIVE='Y'
    WHEN NOT MATCHED THEN
      INSERT (ROLE_ID,API_ID,IS_ACTIVE,CREATED_BY)
      VALUES (s.ROLE_ID,s.API_ID,'Y',NULL);

    -- Opt-in only after validating the report source and history in IAS.
    -- This never disables an existing API; if the switch is N, do nothing.
    IF V_ACTIVATE_VERIFIED_REPORTS='Y' THEN
      UPDATE T_AU_API_MASTER SET IS_ACTIVE='Y'
       WHERE API_ID=V_API_ID AND NVL(IS_ACTIVE,'N')<>'Y';
      V_ACTIVATED_COUNT := V_ACTIVATED_COUNT + SQL%ROWCOUNT;
    END IF;
    V_REGISTERED_COUNT := V_REGISTERED_COUNT+1;
    DBMS_OUTPUT.PUT_LINE('Super User only GET /api/ExecutiveReports/'||r.ACTION_NAME);
  END LOOP;

  IF V_REGISTERED_COUNT<>5 THEN
    RAISE_APPLICATION_ERROR(-20814,'Expected exactly five ExecutiveReports GET APIs.');
  END IF;

  DBMS_OUTPUT.PUT_LINE(
    'New report APIs: '||V_NEW_APIS||
    ', disabled non-Super-User grants: '||V_REVOKED_GRANTS||
    ', explicitly activated verified APIs: '||V_ACTIVATED_COUNT);
  COMMIT;
EXCEPTION WHEN OTHERS THEN
  ROLLBACK;
  RAISE;
END;
/
