/*
  MANAGEMENT AUDIT NOTIFICATIONS - COMPLETE PRODUCTION DEPLOYMENT
  Commit baseline: 1397b4d65591bb41ef0eda5dd5a40471d2cf2a8d

  This file is intentionally self-contained. It embeds the complete checked-in
  PKG_INQ and PKG_AE specifications/bodies and the complete central notification
  package. No @@ includes are required.

  Schedule configuration is isolated below. Change only these DEFINE values when
  a different cadence is approved; no package/business-code change is required.
*/
SET SERVEROUTPUT ON SIZE UNLIMITED;
SET FEEDBACK ON;
SET VERIFY OFF;
SET SQLBLANKLINES ON;
SET DEFINE ON;
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;

DEFINE MGMT_WEEKLY_SCHEDULE = 'FREQ=WEEKLY;BYDAY=MON;BYHOUR=07;BYMINUTE=00;BYSECOND=00'
DEFINE MGMT_MAPPING_EXCEPTION_SCHEDULE = 'FREQ=WEEKLY;BYDAY=FRI;BYHOUR=07;BYMINUTE=00;BYSECOND=00'
DEFINE IAS_NOTIFICATION_HEALTH_SCHEDULE = 'FREQ=DAILY;BYHOUR=08;BYMINUTE=00;BYSECOND=00'

-- HTML templates contain ampersands. Preserve the schedule variables, then disable substitution.
SET DEFINE OFF;

PROMPT ========================================================================
PROMPT A. PREFLIGHT
PROMPT ========================================================================
DECLARE
  V_MISSING VARCHAR2(4000);
  V_COUNT NUMBER;
  V_NEXT_RUN TIMESTAMP WITH TIME ZONE;
BEGIN
  IF DBMS_DB_VERSION.VERSION < 19 THEN
    RAISE_APPLICATION_ERROR(
      -20801,
      'Oracle 19c or later is required by this deployment. Detected major version='||
      DBMS_DB_VERSION.VERSION||'.'
    );
  END IF;

  SELECT LISTAGG(REQUIRED_NAME, ', ') WITHIN GROUP (ORDER BY REQUIRED_NAME)
    INTO V_MISSING
    FROM (
      SELECT R.REQUIRED_NAME
        FROM (
          SELECT 'T_AUDITEE_ENTITIES' REQUIRED_NAME FROM DUAL UNION ALL
          SELECT 'T_AUDITEE_ENTITIES_MAPING' FROM DUAL UNION ALL
          SELECT 'AIS_T_AU_POST_COMPLIANCE' FROM DUAL UNION ALL
          SELECT 'AIS_T_AU_POST_COMPLIANCE_HISTORY' FROM DUAL UNION ALL
          SELECT 'T_AU_IID_EMAIL_QUEUE' FROM DUAL UNION ALL
          SELECT 'SEQ_AU_IID_EMAIL_QUEUE_ID' FROM DUAL UNION ALL
          SELECT 'T_USER' FROM DUAL UNION ALL
          SELECT 'T_USER_MAPING' FROM DUAL UNION ALL
          SELECT 'T_SYS_LOG' FROM DUAL UNION ALL
          SELECT 'V_GET_AIS_POST_COMPLIANCE' FROM DUAL UNION ALL
          SELECT 'V_SERVICE_EMPLOYEEINFO' FROM DUAL UNION ALL
          SELECT 'PKG_LG' FROM DUAL
        ) R
       WHERE NOT EXISTS (SELECT 1 FROM USER_OBJECTS O WHERE O.OBJECT_NAME=R.REQUIRED_NAME)
    );
  IF V_MISSING IS NOT NULL THEN
    RAISE_APPLICATION_ERROR(-20800,'Missing prerequisite IAS objects: '||V_MISSING);
  END IF;

  SELECT COUNT(*) INTO V_COUNT
    FROM USER_TAB_COLUMNS
   WHERE TABLE_NAME='T_AUDITEE_ENTITIES'
     AND COLUMN_NAME IN ('ENTITY_ID','NAME','EMAIL_ADDRESS','AUDITBY_ID','ACTIVE','AUDITABLE');
  IF V_COUNT<>6 THEN
    RAISE_APPLICATION_ERROR(-20802,'Required T_AUDITEE_ENTITIES notification columns were not found.');
  END IF;

  SELECT COUNT(*) INTO V_COUNT
    FROM USER_TAB_COLUMNS
   WHERE TABLE_NAME='T_AUDITEE_ENTITIES_MAPING'
     AND COLUMN_NAME IN ('ENTITY_ID','PARENT_ID','DIV_OFFICE','REPORTING','B_GROUP','STATUS');
  IF V_COUNT<>6 THEN
    RAISE_APPLICATION_ERROR(-20803,'Required Division/Reporting mapping columns were not found.');
  END IF;

  SELECT COUNT(*) INTO V_COUNT
    FROM USER_TAB_COLUMNS
   WHERE TABLE_NAME='T_AU_IID_EMAIL_QUEUE'
     AND COLUMN_NAME IN
       ('EMAIL_ID','EVENT_CODE','REF_ID1','REF_ID2','MAIL_TO','MAIL_CC','SUBJECT','BODY',
        'STATUS','CREATED_ON','SENT_ON','ERROR_TEXT','RETRY_COUNT','CREATED_BY');
  IF V_COUNT<>14 THEN
    RAISE_APPLICATION_ERROR(-20804,'Required T_AU_IID_EMAIL_QUEUE columns were not found.');
  END IF;

  SELECT COUNT(*) INTO V_COUNT
    FROM USER_OBJECTS
   WHERE OBJECT_NAME='PKG_LG'
     AND OBJECT_TYPE IN ('PACKAGE','PACKAGE BODY')
     AND STATUS='VALID';
  IF V_COUNT<>2 THEN
    RAISE_APPLICATION_ERROR(-20805,'Existing IAS error logger PKG_LG specification/body is not valid.');
  END IF;

  SELECT COUNT(*) INTO V_COUNT
    FROM SESSION_PRIVS
   WHERE PRIVILEGE IN ('CREATE JOB','CREATE ANY JOB');
  IF V_COUNT=0 THEN
    RAISE_APPLICATION_ERROR(-20806,'CREATE JOB privilege is required to configure IAS notification scheduler jobs.');
  END IF;

  DBMS_SCHEDULER.EVALUATE_CALENDAR_STRING(
    'FREQ=DAILY;BYHOUR=08;BYMINUTE=00;BYSECOND=00',
    SYSTIMESTAMP,
    SYSTIMESTAMP,
    V_NEXT_RUN
  );
  IF V_NEXT_RUN IS NULL THEN
    RAISE_APPLICATION_ERROR(-20806,'DBMS_SCHEDULER calendar evaluation is unavailable.');
  END IF;

  SELECT COUNT(*) INTO V_COUNT
    FROM T_USER_MAPING M
    JOIN T_USER U ON U.USERID=M.USERID
    JOIN V_SERVICE_EMPLOYEEINFO E ON E.PPNO=U.PPNO
   WHERE M.ROLE_ID=1
     AND NVL(U.ISACTIVE,'Y')='Y'
     AND TRIM(E.EMAIL) IS NOT NULL;
  IF V_COUNT=0 THEN
    RAISE_APPLICATION_ERROR(-20807,'No active Super User (ROLE_ID=1) email recipient is configured.');
  END IF;

  SELECT COUNT(*) INTO V_COUNT
    FROM T_AUDITEE_ENTITIES E
   WHERE E.ENTITY_ID IN (112242,112248)
     AND TRIM(E.EMAIL_ADDRESS) IS NOT NULL;
  IF V_COUNT<>2 THEN
    RAISE_APPLICATION_ERROR(-20808,'Email addresses must be configured for both Management Audit heads 112242 and 112248.');
  END IF;

  DBMS_OUTPUT.PUT_LINE(
    'Preflight passed: Oracle '||DBMS_DB_VERSION.VERSION||
    '+, queue/schema prerequisites, Super User recipient, audit-head recipients and scheduler capability confirmed.'
  );
END;
/

/*
  Deployment safety gate:
  If these jobs already exist from an earlier deployment, disable them before
  replacing packages/views. A later compilation failure therefore cannot leave
  the Management Audit notification jobs running against a partial deployment.
*/
DECLARE
  V_DISABLED NUMBER := 0;
BEGIN
  FOR R IN (
    SELECT JOB_NAME,ENABLED
      FROM USER_SCHEDULER_JOBS
     WHERE JOB_NAME IN
       ('JOB_MGMT_AUDIT_WEEKLY_NOTIFY',
        'JOB_MGMT_AUDIT_MAPPING_EXCEPT',
        'JOB_IAS_NOTIFICATION_HEALTH')
  ) LOOP
    IF R.ENABLED='TRUE' THEN
      DBMS_SCHEDULER.DISABLE(R.JOB_NAME,FORCE=>TRUE);
      V_DISABLED:=V_DISABLED+1;
    END IF;
  END LOOP;
  DBMS_OUTPUT.PUT_LINE('Scheduler safety gate applied; existing target jobs disabled='||V_DISABLED||'.');
END;
/

PROMPT ========================================================================
PROMPT B. NOTIFICATION CONFIGURATION
PROMPT ========================================================================
BEGIN
  EXECUTE IMMEDIATE q'~
    CREATE TABLE IAS_NOTIFICATION_MASTER
    (
      NOTIFICATION_ID       NUMBER GENERATED BY DEFAULT ON NULL AS IDENTITY,
      NOTIFICATION_CODE     VARCHAR2(100 CHAR) NOT NULL,
      NOTIFICATION_NAME     VARCHAR2(200 CHAR) NOT NULL,
      MODULE_NAME           VARCHAR2(100 CHAR) NOT NULL,
      CALLING_PROCESS       VARCHAR2(300 CHAR) NOT NULL,
      RELATED_PROCEDURE     VARCHAR2(300 CHAR),
      DATA_SOURCE_NAME      VARCHAR2(300 CHAR),
      SUBJECT_TEMPLATE      VARCHAR2(1000 CHAR) NOT NULL,
      TITLE_TEMPLATE        VARCHAR2(500 CHAR) NOT NULL,
      SUMMARY_TEMPLATE      CLOB NOT NULL,
      NOTIFICATION_TYPE     VARCHAR2(30 CHAR) DEFAULT 'TRANSACTIONAL' NOT NULL,
      DELIVERY_MECHANISM    VARCHAR2(100 CHAR) DEFAULT 'PKG_INQ.P_ENQUEUE_EMAIL' NOT NULL,
      CONTENT_TYPE          VARCHAR2(100 CHAR) DEFAULT 'text/html; charset=utf-8' NOT NULL,
      PRIORITY_CODE         VARCHAR2(10 CHAR) DEFAULT 'NORMAL' NOT NULL,
      IS_ACTIVE             CHAR(1 CHAR) DEFAULT 'Y' NOT NULL,
      REQUIRES_ATTACHMENT   CHAR(1 CHAR) DEFAULT 'N' NOT NULL,
      DESCRIPTION           VARCHAR2(1000 CHAR),
      CREATED_BY            VARCHAR2(100 CHAR) DEFAULT USER NOT NULL,
      CREATED_ON            TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
      UPDATED_BY            VARCHAR2(100 CHAR),
      UPDATED_ON            TIMESTAMP,
      CONSTRAINT PK_IAS_NOTIFICATION_MASTER PRIMARY KEY (NOTIFICATION_ID),
      CONSTRAINT UQ_IAS_NOTIFICATION_CODE UNIQUE (NOTIFICATION_CODE),
      CONSTRAINT CK_IAS_NOTIFICATION_ACTIVE CHECK (IS_ACTIVE IN ('Y','N')),
      CONSTRAINT CK_IAS_NOTIFICATION_ATTACH CHECK (REQUIRES_ATTACHMENT IN ('Y','N')),
      CONSTRAINT CK_IAS_NOTIFICATION_TYPE CHECK
        (NOTIFICATION_TYPE IN ('TRANSACTIONAL','SECURITY','TECHNICAL','WORKFLOW')),
      CONSTRAINT CK_IAS_NOTIFICATION_PRIORITY CHECK
        (PRIORITY_CODE IN ('LOW','NORMAL','HIGH'))
    )~';
EXCEPTION
  WHEN OTHERS THEN
    IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

COMMENT ON TABLE IAS_NOTIFICATION_MASTER IS
  'Central runtime registry for every IAS email notification and its authoritative source/process.';

CREATE OR REPLACE VIEW V_IAS_NOTIFICATION_MASTER AS
SELECT NOTIFICATION_ID, NOTIFICATION_CODE, NOTIFICATION_NAME, MODULE_NAME,
       CALLING_PROCESS, RELATED_PROCEDURE, DATA_SOURCE_NAME, SUBJECT_TEMPLATE,
       TITLE_TEMPLATE, SUMMARY_TEMPLATE, NOTIFICATION_TYPE, DELIVERY_MECHANISM,
       CONTENT_TYPE, PRIORITY_CODE, IS_ACTIVE, REQUIRES_ATTACHMENT, DESCRIPTION
  FROM IAS_NOTIFICATION_MASTER;

MERGE INTO IAS_NOTIFICATION_MASTER D
USING (
  SELECT 'MGMT_AUDIT_PARA_STATUS' CODE,'Management Audit Para Status' NAME,'Compliance' MODL,
    'PKG_AE.P_SUBMITPOSTAUDITCOMPLIANCE_REVIEW' CALLER,
    'PKG_IAS_NOTIFICATION.SEND_MGMT_AUDIT_PARA_STATUS' PROC,
    'V_IAS_POST_COMPLIANCE_NOTIFY + AIS_T_AU_POST_COMPLIANCE_HISTORY' SRC,
    'IAS Notification: Audit Para No. {REFERENCE} {STATUS}' SUBJ,
    'Audit Para No. {REFERENCE} {STATUS}' TITLE,' ' SUMMARY,'WORKFLOW' NT,'N' ATT,
    'Management Audit only (AUDITED_BY 112242/112248); queued after the decision commit.' DESCR FROM DUAL
  UNION ALL
  SELECT 'MGMT_AUDIT_WEEKLY_PARA_STATUS','Weekly Management Audit Para Decisions','Compliance',
    'JOB_MGMT_AUDIT_WEEKLY_NOTIFY','PKG_IAS_NOTIFICATION.SEND_MGMT_AUDIT_WEEKLY',
    'Decision history + V_IAS_MGMT_AUDIT_NOTIFY_MAP',
    'IAS Notification: Weekly Management Audit Para Decisions ({REFERENCE})',
    'Weekly Management Audit Para Decisions','Reporting period: {REFERENCE}','WORKFLOW','N',
    'Previous Monday-Sunday digest, one queue item per concerned Divisional Head with valid reporting office/group in CC.' FROM DUAL
  UNION ALL
  SELECT 'MGMT_AUDIT_MAPPING_EXCEPTION','Management Audit Entity Mapping Exceptions','Compliance',
    'JOB_MGMT_AUDIT_MAPPING_EXCEPT','PKG_IAS_NOTIFICATION.SEND_MGMT_AUDIT_MAPPING_EXCEPTIONS',
    'V_IAS_MGMT_AUDIT_NOTIFY_MAP','IAS Notification: Management Audit Entity Mapping Exceptions',
    'Management Audit Entity Mapping Exceptions','Audit domain: {REFERENCE}','WORKFLOW','N',
    'Friday exception report sent separately to the heads of 112242 and 112248 when active/auditable entity mappings are incomplete or conflicting.' FROM DUAL
  UNION ALL
  SELECT 'IAS_NOTIFICATION_HEALTH','IAS Notification Health Report','Administration',
    'JOB_IAS_NOTIFICATION_HEALTH','PKG_IAS_NOTIFICATION.SEND_NOTIFICATION_HEALTH',
    'T_AU_IID_EMAIL_QUEUE + USER_OBJECTS + USER_SCHEDULER_JOBS + USER_ARGUMENTS',
    'IAS Notification: Notification Health Report ({REFERENCE})','IAS Notification Health Report',
    'Operational control snapshot: {REFERENCE}','TECHNICAL','N',
    'Queue, delivery, retry, scheduler, P_GET_EMAIL_QUEUE contract, object-validity and processing-error report for Super Admin.' FROM DUAL
) S
ON (D.NOTIFICATION_CODE=S.CODE)
WHEN MATCHED THEN UPDATE SET
  D.NOTIFICATION_NAME=S.NAME,D.MODULE_NAME=S.MODL,D.CALLING_PROCESS=S.CALLER,
  D.RELATED_PROCEDURE=S.PROC,D.DATA_SOURCE_NAME=S.SRC,D.SUBJECT_TEMPLATE=S.SUBJ,
  D.TITLE_TEMPLATE=S.TITLE,D.SUMMARY_TEMPLATE=TO_CLOB(S.SUMMARY),
  D.NOTIFICATION_TYPE=S.NT,D.REQUIRES_ATTACHMENT=S.ATT,D.UPDATED_BY=USER,D.UPDATED_ON=SYSTIMESTAMP
WHEN NOT MATCHED THEN INSERT
  (NOTIFICATION_CODE,NOTIFICATION_NAME,MODULE_NAME,CALLING_PROCESS,RELATED_PROCEDURE,
   DATA_SOURCE_NAME,SUBJECT_TEMPLATE,TITLE_TEMPLATE,SUMMARY_TEMPLATE,
   NOTIFICATION_TYPE,REQUIRES_ATTACHMENT,IS_ACTIVE,DESCRIPTION)
VALUES
  (S.CODE,S.NAME,S.MODL,S.CALLER,S.PROC,S.SRC,S.SUBJ,S.TITLE,TO_CLOB(S.SUMMARY),
   S.NT,S.ATT,'Y',S.DESCR);

/* Preserve unrelated production customizations; only supply missing central-package prerequisites. */
MERGE INTO IAS_NOTIFICATION_MASTER D
USING (
  SELECT 'AUDIT_TEAM_ASSIGNED' CODE,'Audit Task Assigned' NAME,'Planning' MODL,
    'ApiCallsController.AddEngagementPlan' CALLER,'PKG_PG.P_ADDAUDITENGPLAN' PROC,
    'Existing AddAuditEngagementPlan result + audit-team data source' SRC,
    'IAS Notification: Audit Task Assigned - {REFERENCE}' SUBJ,'Audit Task Assigned' TITLE,
    'An audit task has been allocated to the assigned team. Please review the engagement details and proceed in line with the approved plan.' SUMMARY,
    'WORKFLOW' NT,'N' ATT,'Y' ACTIVE,'Central package prerequisite; inserted only when missing.' DESCR FROM DUAL UNION ALL
  SELECT 'AUDIT_TEAM_JOINED','Audit Team Joined','Audit','DBConnection.AddJoiningReport','PKG_AR.P_ADDJOININGREPORT',
    'PKG_AR.P_ADDJOININGREPORT output cursor','IAS Notification: Audit Team has Joined for {REFERENCE}',
    'Audit Team has Joined','The audit team has officially joined. Please coordinate with the team and provide requested information.',
    'WORKFLOW','N','Y','Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'OBS_SUBMITTED_AUDITEE','Observation Submitted to Auditee','Audit Execution',
    'ApiCallsController.submit_observation_to_auditee','PKG_AR observation submission process','Existing managed-observation data source',
    'IAS Notification: Observation Submitted to Auditee - {REFERENCE}','Observation Submitted to Auditee',
    'An audit observation has been submitted to the auditee for review and response.','WORKFLOW','N','Y',
    'Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'PARA_STATUS_UPDATED','Audit Para Status Updated','Compliance','DBConnection.SubmitPostAuditComplianceReview',
    'PKG_AE.P_SUBMITPOSTAUDITCOMPLIANCE_REVIEW','V_IAS_POST_COMPLIANCE_NOTIFY',
    'IAS Notification: Para No. {REFERENCE} is marked {STATUS}','Audit Para Status Updated',
    'Para No. {REFERENCE} has been marked as {STATUS}.','WORKFLOW','N','Y',
    'Existing non-Management behavior; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'FINAL_REPORT_ISSUED','Final Audit Report Issued','Reporting','FieldAuditReportController finalization',
    'PKG_FRPT final-report process','Existing field-audit report overview data source',
    'IAS Notification: Final Audit Report Issued - {REFERENCE}','Final Audit Report Issued',
    'The final audit report has been issued. The generated PDF is attached by the existing application process.',
    'WORKFLOW','Y','Y','Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'INQUIRY_ASSIGNED_UNIT','Inquiry Assigned to I&I Unit','IID','ApiCallsController.AddHeadReview',
    'PKG_INQ.ADD_HEAD_REVIEW','Existing complaint/head-review/unit data sources',
    'IAS Notification: Inquiry Assigned to I&I Unit - {REFERENCE}','Inquiry Assigned to I&I Unit',
    'A complaint inquiry has been approved and allocated to the relevant I&I Unit.','WORKFLOW','N','Y',
    'Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'PASSWORD_RESET_SUCCESS','Password Reset Successful','Authentication','DBConnection.ResetUserPassword',
    'PKG_AD.RESET_USER_PASSWORD','V_SERVICE_EMPLOYEEINFO + RESET_USER_PASSWORD output cursor',
    'IAS~ Password Reset Successful','Password Reset Successful',
    'Your password has been successfully reset. Change the temporary password immediately after logging in.',
    'SECURITY','N','Y','Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'AUDIT_SAMPLE_ISSUE','Audit Sample Creation Issue','Sampling',
    'DBConnection.CreateSampleDataAfterEngagementApproval','PKG_SM.P_ADD_SAMPLE_DATA','PKG_SM.P_ADD_SAMPLE_DATA output cursor',
    'IAS~Notification: Issue in Audit Sample for Engagement ID: {REFERENCE}','Issue in Audit Sample Creation',
    'An issue was identified while creating the audit sample. Please review and resolve it.','TECHNICAL','N','Y',
    'Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'AUDIT_EXCEPTION_ISSUE','Audit Exception Creation Issue','Exception Monitoring',
    'DBConnection.CreateExceptionDataAfterEngagementApproval','PKG_SM.P_ADD_EXCEPTION_DATA','PKG_SM.P_ADD_EXCEPTION_DATA output cursor',
    'IAS~Notification: Issue in Audit Exception for Engagement ID: {REFERENCE}','Issue in Audit Exception Creation',
    'An issue was identified while creating the audit exception report. Please review and resolve it.','TECHNICAL','N','Y',
    'Central package prerequisite; inserted only when missing.' FROM DUAL UNION ALL
  SELECT 'AUDIT_CRITERIA_SUBMITTED','Audit Criteria Submitted','Planning','DBConnection.SubmitAuditCriteriaForApproval',
    'PKG_PG.P_SUBMITAUDITCRITERIAFORAPPROVAL','Existing planning procedure/data source',
    'IAS~ Notification regarding submission of Audit Criteria','Audit Criteria Submitted',
    'Audit criteria have been submitted for review.','WORKFLOW','N','N',
    'Dormant central package prerequisite; inserted inactive only when missing.' FROM DUAL UNION ALL
  SELECT 'AJAX_APPLICATION_ERROR','AJAX Application Error','Application','AjaxErrorNotificationMiddleware',
    'Application middleware','Application error context + configured recipient',
    'IAS AJAX Error {STATUS} ({REFERENCE})','AJAX Application Error','An AJAX application error was detected.',
    'TECHNICAL','N','Y','Central package prerequisite; inserted only when missing.' FROM DUAL
) S
ON (D.NOTIFICATION_CODE=S.CODE)
WHEN NOT MATCHED THEN INSERT
  (NOTIFICATION_CODE,NOTIFICATION_NAME,MODULE_NAME,CALLING_PROCESS,RELATED_PROCEDURE,
   DATA_SOURCE_NAME,SUBJECT_TEMPLATE,TITLE_TEMPLATE,SUMMARY_TEMPLATE,
   NOTIFICATION_TYPE,REQUIRES_ATTACHMENT,IS_ACTIVE,DESCRIPTION)
VALUES
  (S.CODE,S.NAME,S.MODL,S.CALLER,S.PROC,S.SRC,S.SUBJ,S.TITLE,TO_CLOB(S.SUMMARY),
   S.NT,S.ATT,S.ACTIVE,S.DESCR);

COMMIT;
PROMPT ========================================================================
PROMPT C. VIEWS
PROMPT ========================================================================
/* Exactly one notification source row per COM_ID, independent of mapping-row count. */
CREATE OR REPLACE VIEW V_IAS_POST_COMPLIANCE_NOTIFY AS
WITH C AS
(
  SELECT X.*,
         ROW_NUMBER() OVER (PARTITION BY X.COM_ID ORDER BY X.ENTITY_ID,X.PARA_NO) RN
    FROM V_GET_AIS_POST_COMPLIANCE X
),
PARENT_EMAIL AS
(
  SELECT M.ENTITY_ID,
         LISTAGG(DISTINCT P.EMAIL_ADDRESS,';') WITHIN GROUP (ORDER BY P.EMAIL_ADDRESS) PARENT_EMAILS
    FROM T_AUDITEE_ENTITIES_MAPING M
    JOIN T_AUDITEE_ENTITIES P ON P.ENTITY_ID=M.PARENT_ID
   WHERE NVL(M.STATUS,'Y')='Y' AND TRIM(P.EMAIL_ADDRESS) IS NOT NULL
   GROUP BY M.ENTITY_ID
)
SELECT C.COM_ID,
       C.PARA_NO,
       C.GIST_OF_PARAS,
       C.AUDIT_PERIOD,
       C.RSK AS RISK,
       C.AUDITBY_ID,
       C.ENTITY_ID,
       E.EMAIL_ADDRESS AS TO_EMAIL,
       TRIM(BOTH ';' FROM A.EMAIL_ADDRESS||';'||P.PARENT_EMAILS) AS CC_EMAIL
  FROM C
  LEFT JOIN T_AUDITEE_ENTITIES E ON E.ENTITY_ID=C.ENTITY_ID
  LEFT JOIN T_AUDITEE_ENTITIES A ON A.ENTITY_ID=C.AUDITBY_ID
  LEFT JOIN PARENT_EMAIL P ON P.ENTITY_ID=C.ENTITY_ID
 WHERE C.RN=1;

/*
  Management Audit hierarchy:
  - only ACTIVE=Y and AUDITABLE=Y entities are reportable exceptions;
  - DIV_OFFICE identifies the digest owner/TO recipient;
  - REPORTING, with B_GROUP as the established fallback, identifies CC;
  - a Division resolving to multiple Reporting Offices/Groups is an exception,
    never a multi-recipient CC list.
*/
CREATE OR REPLACE VIEW V_IAS_MGMT_AUDIT_NOTIFY_MAP AS
WITH MAP_RAW AS
(
  SELECT M.ENTITY_ID,
         COUNT(*) MAPPING_ROWS,
         COUNT(DISTINCT M.DIV_OFFICE) DIVISION_COUNT,
         MIN(M.DIV_OFFICE) DIVISION_ID,
         COUNT(DISTINCT NVL(M.REPORTING,M.B_GROUP)) ENTITY_REPORTING_COUNT,
         MIN(NVL(M.REPORTING,M.B_GROUP)) ENTITY_REPORTING_ID
    FROM T_AUDITEE_ENTITIES_MAPING M
   WHERE NVL(M.STATUS,'Y')='Y'
   GROUP BY M.ENTITY_ID
),
ENTITY_MAP AS
(
  SELECT E.ENTITY_ID,E.NAME ENTITY_NAME,E.AUDITBY_ID AUDITED_BY,
         NVL(M.MAPPING_ROWS,0) MAPPING_ROWS,
         NVL(M.DIVISION_COUNT,0) DIVISION_COUNT,
         CASE WHEN M.DIVISION_COUNT=1 THEN M.DIVISION_ID END DIVISION_ID,
         NVL(M.ENTITY_REPORTING_COUNT,0) ENTITY_REPORTING_COUNT,
         CASE WHEN M.ENTITY_REPORTING_COUNT=1 THEN M.ENTITY_REPORTING_ID END ENTITY_REPORTING_ID
    FROM T_AUDITEE_ENTITIES E
    LEFT JOIN MAP_RAW M ON M.ENTITY_ID=E.ENTITY_ID
   WHERE E.AUDITBY_ID IN (112242,112248)
     AND E.ACTIVE='Y'
     AND E.AUDITABLE='Y'
),
DIVISION_MAP AS
(
  SELECT E.DIVISION_ID,
         COUNT(DISTINCT E.ENTITY_REPORTING_ID) DIVISION_REPORTING_COUNT,
         MIN(E.ENTITY_REPORTING_ID) DIVISION_REPORTING_ID,
         SUM(CASE WHEN E.ENTITY_REPORTING_COUNT<>1 THEN 1 ELSE 0 END) INCOMPLETE_ENTITY_COUNT
    FROM ENTITY_MAP E
   WHERE E.DIVISION_ID IS NOT NULL
   GROUP BY E.DIVISION_ID
)
SELECT E.ENTITY_ID,E.ENTITY_NAME,E.AUDITED_BY,E.DIVISION_ID,
       D.NAME DIVISION_NAME,D.EMAIL_ADDRESS DIVISION_EMAIL,
       NVL(DM.DIVISION_REPORTING_COUNT,0) DIVISION_REPORTING_COUNT,
       CASE WHEN DM.DIVISION_REPORTING_COUNT=1 AND DM.INCOMPLETE_ENTITY_COUNT=0
            THEN DM.DIVISION_REPORTING_ID END REPORTING_ID,
       CASE WHEN DM.DIVISION_REPORTING_COUNT=1 AND DM.INCOMPLETE_ENTITY_COUNT=0
            THEN R.NAME END REPORTING_NAME,
       CASE WHEN DM.DIVISION_REPORTING_COUNT=1 AND DM.INCOMPLETE_ENTITY_COUNT=0
            THEN R.EMAIL_ADDRESS END REPORTING_EMAIL,
       CASE WHEN E.MAPPING_ROWS>0 AND E.DIVISION_COUNT=1
                  AND TRIM(D.EMAIL_ADDRESS) IS NOT NULL
                  AND E.ENTITY_REPORTING_COUNT=1
                  AND DM.DIVISION_REPORTING_COUNT=1
                  AND DM.INCOMPLETE_ENTITY_COUNT=0
                  AND TRIM(R.EMAIL_ADDRESS) IS NOT NULL
            THEN 'Y' ELSE 'N' END MAPPING_READY,
       RTRIM(
         CASE WHEN E.MAPPING_ROWS=0 THEN 'Create active entity notification mapping; ' END||
         CASE WHEN E.DIVISION_COUNT=0 THEN 'Assign Divisional Office; '
              WHEN E.DIVISION_COUNT>1 THEN 'Resolve conflicting Divisional Office mappings; ' END||
         CASE WHEN E.DIVISION_COUNT=1 AND TRIM(D.EMAIL_ADDRESS) IS NULL THEN 'Add Divisional Head email; ' END||
         CASE WHEN E.ENTITY_REPORTING_COUNT=0 THEN 'Assign Reporting Office / Group; '
              WHEN E.ENTITY_REPORTING_COUNT>1 THEN 'Resolve conflicting entity Reporting Office / Group mappings; ' END||
         CASE WHEN DM.DIVISION_REPORTING_COUNT>1 THEN 'Resolve Division mapped to multiple Reporting Offices / Groups; ' END||
         CASE WHEN NVL(DM.INCOMPLETE_ENTITY_COUNT,0)>0 AND E.ENTITY_REPORTING_COUNT=1
              THEN 'Complete inconsistent Reporting Office / Group mappings within Division; ' END||
         CASE WHEN DM.DIVISION_REPORTING_COUNT=1 AND DM.INCOMPLETE_ENTITY_COUNT=0
                   AND TRIM(R.EMAIL_ADDRESS) IS NULL THEN 'Add Reporting Office / Group email; ' END,
         '; ') MISSING_REQUIRED_ACTION
  FROM ENTITY_MAP E
  LEFT JOIN T_AUDITEE_ENTITIES D ON D.ENTITY_ID=E.DIVISION_ID
  LEFT JOIN DIVISION_MAP DM ON DM.DIVISION_ID=E.DIVISION_ID
  LEFT JOIN T_AUDITEE_ENTITIES R ON R.ENTITY_ID=DM.DIVISION_REPORTING_ID;
PROMPT ========================================================================
PROMPT D. PACKAGE CHANGES
PROMPT ========================================================================
PROMPT Replacing PKG_INQ with its complete checked-in specification/body.
CREATE OR REPLACE PACKAGE PKG_INQ AS

  TYPE T_CURSOR IS REF CURSOR;

  ------------------------------------------------------------------
  -- LOOKUPS (Controller dropdowns)
  ------------------------------------------------------------------
  PROCEDURE GET_RBH_LIST(P_REGION_ID IN NUMBER, IO_CURSOR OUT T_CURSOR);

  PROCEDURE P_GETINSPECTIONUNITS(IO_CURSOR OUT T_CURSOR);
  ------------------------------------------------------------------
  -- COMPLAINTS
  ------------------------------------------------------------------
  PROCEDURE P_CREATE_COMPLAINT_HDR(P_INTAKE_CHANNEL     IN VARCHAR2, -- 'IAID' or 'FFR'
                                   P_SUBMITTED_BY_PP_NO IN NUMBER,
                                   P_COMPLAINT_ID       OUT NUMBER,
                                   P_COMPLAINT_NO       OUT VARCHAR2);

  PROCEDURE P_SAVE_COMPLAINT_IAID(P_COMPLAINT_ID       IN NUMBER,
                                  P_NATURE             IN VARCHAR2,
                                  P_CATEGORY           IN VARCHAR2,
                                  P_RECEIVED_FROM      IN VARCHAR2,
                                  P_LOCATION_TYPE_ID   IN NUMBER,
                                  P_GM_OFFICE_ID       IN NUMBER,
                                  P_REGION_ID          IN NUMBER,
                                  P_BRANCH_ID          IN NUMBER,
                                  P_COMPLAINANT_NAME   IN VARCHAR2,
                                  P_CNIC               IN VARCHAR2,
                                  P_CELLULAR_NUMBER    IN VARCHAR2,
                                  P_MAILING_ADDRESS    IN VARCHAR2,
                                  P_GENDER             IN VARCHAR2,
                                  P_CONTENTS           IN CLOB,
                                  P_UPLOADED_COMPLAINT IN VARCHAR2,
                                  P_UPLOADED_FFR       IN VARCHAR2,
                                  P_UPLOADED_EVIDENCE  IN VARCHAR2,
                                  P_ACTION_REQUIRED    IN VARCHAR2);

  PROCEDURE P_UPSERT_COMPLAINT_IAID(P_COMPLAINT_ID       IN NUMBER,
                                    P_NATURE             IN VARCHAR2,
                                    P_CATEGORY           IN VARCHAR2,
                                    P_RECEIVED_FROM      IN VARCHAR2,
                                    P_LOCATION_TYPE_ID   IN NUMBER,
                                    P_GM_OFFICE_ID       IN NUMBER,
                                    P_REGION_ID          IN NUMBER,
                                    P_BRANCH_ID          IN NUMBER,
                                    P_CONTENTS           IN CLOB,
                                    P_UPLOADED_COMPLAINT IN VARCHAR2,
                                    P_UPLOADED_FFR       IN VARCHAR2,
                                    P_UPLOADED_EVIDENCE  IN VARCHAR2,
                                    P_ACTION_REQUIRED    IN VARCHAR2);

  PROCEDURE P_ADD_COMPLAINANT(P_COMPLAINT_ID     IN NUMBER,
                              P_COMPLAINANT_NAME IN VARCHAR2,
                              P_CNIC             IN VARCHAR2,
                              P_CELLULAR_NUMBER  IN VARCHAR2,
                              P_MAILING_ADDRESS  IN VARCHAR2,
                              P_GENDER           IN VARCHAR2,
                              P_IS_PRIMARY       IN CHAR,
                              P_BY_PP_NO         IN NUMBER,
                              O_COMPLAINANT_ID   OUT NUMBER);

  PROCEDURE P_UPDATE_COMPLAINANT(P_COMPLAINANT_ID   IN NUMBER,
                                 P_COMPLAINANT_NAME IN VARCHAR2,
                                 P_CNIC             IN VARCHAR2,
                                 P_CELLULAR_NUMBER  IN VARCHAR2,
                                 P_MAILING_ADDRESS  IN VARCHAR2,
                                 P_GENDER           IN VARCHAR2,
                                 P_IS_PRIMARY       IN CHAR,
                                 P_BY_PP_NO         IN NUMBER);

  PROCEDURE P_DELETE_COMPLAINANT(P_COMPLAINANT_ID IN NUMBER,
                                 P_BY_PP_NO       IN NUMBER);

  PROCEDURE P_GET_COMPLAINANTS_BY_COMPLAINT(P_COMPLAINT_ID IN NUMBER,
                                            IO_CURSOR      OUT T_CURSOR);

  PROCEDURE P_SET_PRIMARY_COMPLAINANT(P_COMPLAINT_ID   IN NUMBER,
                                      P_COMPLAINANT_ID IN NUMBER,
                                      P_BY_PP_NO       IN NUMBER);

  PROCEDURE ADD_COMPLAINT(P_NATURE             IN VARCHAR2,
                          P_CATEGORY           IN VARCHAR2,
                          P_SOURCE             IN VARCHAR2,
                          P_SOURCE_OTHER_TEXT  IN VARCHAR2,
                          P_PERTAINS_TO        IN VARCHAR2,
                          P_FIELD_TYPE         IN VARCHAR2,
                          P_HO_UNIT_TYPE_ID    IN NUMBER,
                          P_HO_UNIT_ID         IN NUMBER,
                          P_REGION_ID          IN NUMBER,
                          P_BRANCH_ID          IN NUMBER,
                          P_CONTENTS           IN CLOB,
                          P_UPLOADED_COMPLAINT IN VARCHAR2,
                          P_UPLOADED_FFR       IN VARCHAR2,
                          P_UPLOADED_EVIDENCE  IN VARCHAR2,
                          P_ACTION_REQUIRED    IN VARCHAR2,
                          P_SUBMITTED_BY       IN NUMBER,
                          O_COMPLAINT_ID       OUT NUMBER);

  PROCEDURE P_GET_COMPLAINT_HDR(P_COMPLAINT_ID IN NUMBER,
                                T_CURSOR       OUT T_CURSOR);

  PROCEDURE P_GET_COMPLAINT_IAID(P_COMPLAINT_ID IN NUMBER,
                                 T_CURSOR       OUT T_CURSOR);

  PROCEDURE GET_LATEST_INQUIRY_REPORT_BY_COMPLAINT(p_complaint_id IN NUMBER,
                                                   io_cursor      OUT SYS_REFCURSOR);

  PROCEDURE GET_COMPLAINTS(P_USER_ID IN NUMBER, IO_CURSOR OUT T_CURSOR);

  PROCEDURE GET_COMPLAINTS_WITHOUT_ASSESSMENT(T_CURSOR OUT T_CURSOR);

  PROCEDURE GET_COMPLAINTS_DD(P_PAGE_ID in number, IO_CURSOR OUT T_CURSOR);

  PROCEDURE GET_COMPLAINT(P_COMPLAINT_ID IN NUMBER, IO_CURSOR OUT T_CURSOR);

  PROCEDURE GET_LATEST_PLAN_BY_COMPLAINT(P_COMPLAINT_ID IN NUMBER,
                                         IO_CURSOR      OUT T_CURSOR);

  PROCEDURE P_GET_COMPLAINT_LIST(P_INTAKE_CHANNEL IN VARCHAR2 DEFAULT NULL, -- NULL = all
                                 P_STATUS         IN VARCHAR2 DEFAULT NULL,
                                 P_FROM_DATE      IN DATE DEFAULT NULL,
                                 P_TO_DATE        IN DATE DEFAULT NULL,
                                 T_CURSOR         OUT T_CURSOR);

  PROCEDURE P_GET_COMPLAINT_ID_BY_PLAN(P_PLAN_ID      IN NUMBER,
                                       O_COMPLAINT_ID OUT NUMBER);

  PROCEDURE P_GET_COMPLAINT_ID_BY_REPORT(P_REPORT_ID    IN NUMBER,
                                         O_COMPLAINT_ID OUT NUMBER);

  PROCEDURE P_SAVE_INQ_FINDINGS_REC(P_COMPLAINT_ID   IN NUMBER,
                                    P_FINDINGS       IN CLOB,
                                    P_RECOMMENDATION IN CLOB,
                                    P_UPDATED_BY     IN NUMBER,
                                    IO_CURSOR        OUT T_CURSOR);

  ------------------------------------------------------------------
  -- INITIAL ASSESSMENT
  ------------------------------------------------------------------
  PROCEDURE ADD_ASSESSMENT(P_COMPLAINT_ID     IN NUMBER,
                           P_RECEIVED_BY      IN NUMBER,
                           P_ASSESSMENT       IN CLOB,
                           P_RECOMMENDATION   IN VARCHAR2,
                           P_ASSIGNED_UNIT_ID IN NUMBER,
                           O_ASSESSMENT_ID    OUT NUMBER);

  ------------------------------------------------------------------
  -- HEAD REVIEW
  ------------------------------------------------------------------
  PROCEDURE ADD_HEAD_REVIEW(P_COMPLAINT_ID           IN NUMBER,
                            P_ASSESSMENT_ID          IN NUMBER,
                            P_REVIEWED_BY            IN NUMBER,
                            P_DIRECTIONS             IN CLOB,
                            P_ASSIGNED_TO_UNIT       IN NUMBER,
                            P_TEAM_LEAD              IN NUMBER,
                            P_TEAM_MEMBERS           IN CLOB,
                            P_ASSIGNED_ON            IN VARCHAR2,
                            P_DUE_DATE               IN VARCHAR2,
                            P_REFERRED_BACK_COMMENTS IN CLOB,
                            P_ACTION                 IN VARCHAR2,
                            O_REVIEW_ID              OUT NUMBER);

  ------------------------------------------------------------------
  -- INVESTIGATION PLAN
  ------------------------------------------------------------------
  PROCEDURE ADD_INV_PLAN(P_COMPLAINT_ID    IN NUMBER,
                         P_PLAN_DETAILS    IN CLOB,
                         P_SUBMITTED_BY    IN NUMBER,
                         P_STATUS          IN VARCHAR2,
                         P_INV_RISK        IN VARCHAR2,
                         P_INV_SIZE        IN VARCHAR2,
                         P_NO_OF_DAYS      IN NUMBER,
                         P_TRAVELLING_DAYS IN NUMBER,
                         P_TEAM_LEAD       IN VARCHAR2,
                         P_TEAM_MEMBERS    IN VARCHAR2,
                         P_START_DATE      IN DATE,
                         P_ACTIVITIES_TEXT IN VARCHAR2, -- if you added ACTIVITIES_TEXT column
                         O_PLAN_ID         OUT NUMBER);

  PROCEDURE GET_INV_PLAN(p_complaint_id IN NUMBER, IO_CURSOR OUT T_CURSOR);

  PROCEDURE GET_IID_TASK_LIST(P_UNIT_ID IN NUMBER, IO_CURSOR OUT T_CURSOR);

  ------------------------------------------------------------------
  -- PLAN APPROVAL
  ------------------------------------------------------------------
  PROCEDURE ADD_PLAN_APPROVAL(P_PLAN_ID         IN NUMBER,
                              P_APPROVED_BY     IN NUMBER,
                              P_IS_APPROVED     IN VARCHAR2,
                              P_EDITED_PLAN     IN CLOB,
                              P_FURTHER_ACTIONS IN CLOB,
                              O_APPROVAL_ID     OUT NUMBER);

  ------------------------------------------------------------------
  -- INQUIRY REPORT
  ------------------------------------------------------------------
  PROCEDURE ADD_INQUIRY_REPORT(P_COMPLAINT_ID                  IN NUMBER,
                               P_NAME_COMPLAINANT              IN VARCHAR2,
                               P_NAME_ACCUSED                  IN VARCHAR2,
                               P_GIST                          IN CLOB,
                               P_PROCEEDINGS                   IN CLOB,
                               P_FINDINGS                      IN CLOB,
                               P_RECOMMENDATION                IN CLOB,
                               P_CONCLUSION                    IN CLOB,
                               P_REPORTED_IN_AUDIT_REPORT      IN VARCHAR2,
                               P_AUDIT_REPORT_REFERENCE_DETAIL IN CLOB,
                               P_UPLOADED_REPORT               IN VARCHAR2,
                               P_UPLOADED_EVIDENCE             IN VARCHAR2,
                               P_UPLOADED_DSA                  IN VARCHAR2,
                               P_SUBMITTED_ON                  IN DATE,
                               P_SUBMITTED_BY                  IN NUMBER,
                               O_REPORT_ID                     OUT NUMBER);

  PROCEDURE GET_INQUIRY_REPORT(P_REPORT_ID IN NUMBER,
                               IO_CURSOR   OUT T_CURSOR);

  ------------------------------------------------------------------
  -- ANALYSIS
  ------------------------------------------------------------------
  PROCEDURE ADD_ANALYSIS(P_REPORT_ID             IN NUMBER,
                         P_POLICY_GAPS           IN CLOB,
                         P_CONTROL_GAPS          IN CLOB,
                         P_PROCEDURAL_VIOLATIONS IN CLOB,
                         P_FORWARD_TO            IN VARCHAR2,
                         P_COMMENTS              IN CLOB,
                         P_DECISION              IN VARCHAR2,
                         P_REFER_BACK_COMMENTS   IN CLOB,
                         P_ANALYZED_BY           IN NUMBER,
                         O_ANALYSIS_ID           OUT NUMBER);

  ------------------------------------------------------------------
  -- FINAL APPROVAL
  ------------------------------------------------------------------
  PROCEDURE ADD_FINAL_APPROVAL(P_REPORT_ID         IN NUMBER,
                               P_COMMENTS          IN CLOB,
                               P_APPROVED          IN VARCHAR2,
                               P_APPROVED_BY       IN NUMBER,
                               O_FINAL_APPROVAL_ID OUT NUMBER);

  ------------------------------------------------------------------
  -- CASE STUDY
  ------------------------------------------------------------------
  PROCEDURE ADD_CASE_STUDY(P_COMPLAINT_ID           IN NUMBER,
                           P_ORIGIN_PROCESS_OWNER   IN VARCHAR2,
                           P_NAME_COMPLAINANT       IN VARCHAR2,
                           P_BRANCH                 IN VARCHAR2,
                           P_GIST                   IN CLOB,
                           P_OUTCOME                IN CLOB,
                           P_MODUS_OPERANDI         IN CLOB,
                           P_GAPS                   IN CLOB,
                           P_ROOT_CAUSE             IN CLOB,
                           P_ACTIONS_REC            IN CLOB,
                           P_STATUS                 IN VARCHAR2,
                           P_POLICY_GAPS_IDENTIFIED IN CLOB,
                           P_CONTROL_VIOLATIONS     IN CLOB,
                           P_RISK_IDENTIFIED        IN CLOB,
                           P_REG_COMPLIANCE_FAILURE IN CLOB,
                           O_CASE_ID                OUT NUMBER);

  ------------------------------------------------------------------
  -- REPORTS FILTERING
  ------------------------------------------------------------------
  PROCEDURE GET_REPORTS(P_FILTER          IN VARCHAR2,
                        P_SOURCE          IN VARCHAR2,
                        P_CATEGORY        IN VARCHAR2,
                        P_PERTAINS_TO     IN VARCHAR2,
                        P_DATE_FROM       IN VARCHAR2,
                        P_DATE_TO         IN VARCHAR2,
                        P_REGION_ID       IN NUMBER,
                        P_BRANCH_ID       IN NUMBER,
                        P_HO_UNIT_TYPE_ID IN NUMBER,
                        P_HO_UNIT_ID      IN NUMBER,
                        P_STATUS          IN VARCHAR2,
                        IO_CURSOR         OUT T_CURSOR);

  ---------------------------------------------------------------------
  -- Common result cursor (OK/MESSAGE/ID)
  ----------------------------------------------------------------------
  PROCEDURE P_RESULT_OK(io_cursor OUT t_cursor,
                        p_message IN VARCHAR2,
                        p_id      IN NUMBER DEFAULT NULL);
  PROCEDURE P_RESULT_FAIL(io_cursor OUT t_cursor, p_message IN VARCHAR2);

  ----------------------------------------------------------------------
  -- ACCUSATIONS
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_ACCUSATIONS(p_complaint_id IN NUMBER,
                                  io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_ACCUSATION(p_complaint_id    IN NUMBER,
                                 p_accusation_text IN CLOB,
                                 p_sort_order      IN NUMBER,
                                 p_created_by      IN NUMBER,
                                 io_cursor         OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_ACCUSATION(p_accusation_id   IN NUMBER,
                                    p_accusation_text IN CLOB,
                                    p_sort_order      IN NUMBER,
                                    p_updated_by      IN NUMBER,
                                    io_cursor         OUT t_cursor);
  PROCEDURE P_DELETE_INQ_ACCUSATION(p_accusation_id IN NUMBER,
                                    p_updated_by    IN NUMBER,
                                    io_cursor       OUT t_cursor);

  ----------------------------------------------------------------------
  -- ACCUSED LIST
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_ACCUSED_LIST(p_complaint_id IN NUMBER,
                                   io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_ACCUSED(p_complaint_id IN NUMBER,
                              p_person_name  IN VARCHAR2,
                              p_designation  IN VARCHAR2,
                              p_role_type    IN VARCHAR2, -- MAIN/CO
                              p_ppno_number  IN VARCHAR2,
                              p_cnic         IN VARCHAR2,
                              p_Father_name  IN VARCHAR2,
                              p_remarks      IN VARCHAR2,
                              p_sort_order   IN NUMBER,
                              p_created_by   IN NUMBER,
                              io_cursor      OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_ACCUSED(p_accused_row_id IN NUMBER,
                                 p_person_name    IN VARCHAR2,
                                 p_designation    IN VARCHAR2,
                                 p_role_type      IN VARCHAR2,
                                 p_ppno_number    IN VARCHAR2,
                                 p_cnic           IN VARCHAR2,
                                 p_FATHER_NAME    IN VARCHAR2,
                                 p_remarks        IN VARCHAR2,
                                 p_sort_order     IN NUMBER,
                                 p_updated_by     IN NUMBER,
                                 io_cursor        OUT t_cursor);
  PROCEDURE P_DELETE_INQ_ACCUSED(p_accused_row_id IN NUMBER,
                                 p_updated_by     IN NUMBER,
                                 io_cursor        OUT t_cursor);

  ----------------------------------------------------------------------
  -- RECORD SCRUTINIZED
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_RECORDS(p_complaint_id IN NUMBER,
                              io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_RECORD(p_complaint_id   IN NUMBER,
                             p_record_title   IN VARCHAR2,
                             p_record_details IN VARCHAR2,
                             p_sort_order     IN NUMBER,
                             p_created_by     IN NUMBER,
                             io_cursor        OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_RECORD(p_rec_id         IN NUMBER,
                                p_record_title   IN VARCHAR2,
                                p_record_details IN VARCHAR2,
                                p_sort_order     IN NUMBER,
                                p_updated_by     IN NUMBER,
                                io_cursor        OUT t_cursor);
  PROCEDURE P_DELETE_INQ_RECORD(p_rec_id     IN NUMBER,
                                p_updated_by IN NUMBER,
                                io_cursor    OUT t_cursor);

  ----------------------------------------------------------------------
  -- STATEMENTS REGISTER
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_STATEMENTS(p_complaint_id IN NUMBER,
                                 io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_STATEMENT(p_complaint_id       IN NUMBER,
                                p_person_name        IN VARCHAR2,
                                p_role_type          IN VARCHAR2, -- COMPLAINANT/ACCUSED/WITNESS/OTHER
                                p_ppno_number        IN VARCHAR2, -- UI defaults complainant CNIC here
                                p_cnic               IN VARCHAR2,
                                p_statement_datetime IN DATE,
                                p_place              IN VARCHAR2,
                                p_mode_type          IN VARCHAR2,
                                p_key_points         IN CLOB,
                                P_UPLOADED_STATEMENT in Clob,
                                P_USER_ID            IN NUMBER,
                                io_cursor            OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_STATEMENT(p_statement_id       IN NUMBER,
                                   p_person_name        IN VARCHAR2,
                                   p_role_type          IN VARCHAR2,
                                   p_ppno_number        IN VARCHAR2,
                                   p_cnic               IN VARCHAR2,
                                   p_statement_datetime IN DATE,
                                   p_place              IN VARCHAR2,
                                   p_mode_type          IN VARCHAR2,
                                   p_key_points         IN CLOB,
                                   P_UPLOADED_STATEMENT in Clob,
                                   p_updated_by         IN NUMBER,
                                   io_cursor            OUT t_cursor);
  PROCEDURE P_DELETE_INQ_STATEMENT(p_statement_id IN NUMBER,
                                   p_updated_by   IN NUMBER,
                                   io_cursor      OUT t_cursor);

  ----------------------------------------------------------------------
  -- EVIDENCE FILES
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_EVIDENCE_FILES(p_complaint_id IN NUMBER,
                                     io_cursor      OUT t_cursor);
  PROCEDURE P_ADD_INQ_EVIDENCE_FILE(p_complaint_id  IN NUMBER,
                                    p_evidence_type IN VARCHAR2, -- MATERIAL/CIRCUMSTANTIAL/OTHER
                                    p_description   IN VARCHAR2,
                                    p_file_name     IN VARCHAR2,
                                    p_file_path     IN VARCHAR2,
                                    p_file_ext      IN VARCHAR2,
                                    p_file_size_kb  IN NUMBER,
                                    p_uploaded_by   IN NUMBER,
                                    io_cursor       OUT t_cursor);
  PROCEDURE P_DELETE_INQ_EVIDENCE_FILE(p_evidence_id IN NUMBER,
                                       p_updated_by  IN NUMBER,
                                       io_cursor     OUT t_cursor);

  ----------------------------------------------------------------------
  -- VIOLATIONS (Annex-III)
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_VIOLATIONS(p_complaint_id IN NUMBER,
                                 io_cursor      OUT t_cursor);
  PROCEDURE P_GET_INQ_VIOLATION_STEP(P_COMPLAINT_ID IN NUMBER,
                                     IO_CURSOR      OUT T_CURSOR);
  PROCEDURE P_SAVE_INQ_VIOLATION_STEP(P_COMPLAINT_ID                  IN NUMBER,
                                      P_CONCLUSION                    IN CLOB,
                                      P_REPORTED_IN_AUDIT_REPORT      IN VARCHAR2,
                                      P_AUDIT_REPORT_REFERENCE_DETAIL IN CLOB,
                                      P_UPDATED_BY                    IN NUMBER,
                                      IO_CURSOR                       OUT T_CURSOR);
  PROCEDURE P_ADD_INQ_VIOLATION(p_complaint_id     IN NUMBER,
                                p_category         IN VARCHAR2, -- INTERNAL/POLICY_GAP/CONTROL
                                p_violation_detail IN CLOB,
                                p_reference_text   IN VARCHAR2,
                                p_recommendation   IN CLOB,
                                p_sort_order       IN NUMBER,
                                p_created_by       IN NUMBER,
                                io_cursor          OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_VIOLATION(p_violation_id     IN NUMBER,
                                   p_category         IN VARCHAR2,
                                   p_violation_detail IN CLOB,
                                   p_reference_text   IN VARCHAR2,
                                   p_recommendation   IN CLOB,
                                   p_sort_order       IN NUMBER,
                                   p_updated_by       IN NUMBER,
                                   io_cursor          OUT t_cursor);

  PROCEDURE P_DELETE_INQ_VIOLATION(p_violation_id IN NUMBER,
                                   p_updated_by   IN NUMBER,
                                   io_cursor      OUT t_cursor);
  PROCEDURE GET_EMPLOYEE_INFO(P_PP_NO in number, io_cursor OUT t_cursor);

  PROCEDURE GET_INQ_FIND_RECOMM_STATUS(p_complaint_id IN NUMBER,
                                       io_cursor      OUT t_cursor);
  ----------------------------------------------------------------------
  -- DSA LIST
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_DSA(p_complaint_id IN NUMBER, io_cursor OUT t_cursor);
  PROCEDURE P_ADD_INQ_DSA(p_complaint_id IN NUMBER,
                          p_person_name  IN VARCHAR2,
                          p_designation  IN VARCHAR2,
                          p_ppno_number  IN VARCHAR2,
                          p_cnic         IN VARCHAR2,
                          p_dsa_status   IN VARCHAR2, -- DRAFT/ISSUED/SERVED/CLOSED
                          p_remarks      IN VARCHAR2,
                          p_sort_order   IN NUMBER,
                          p_created_by   IN NUMBER,
                          io_cursor      OUT t_cursor);
  PROCEDURE P_UPDATE_INQ_DSA(p_dsa_id      IN NUMBER,
                             p_person_name IN VARCHAR2,
                             p_designation IN VARCHAR2,
                             p_ppno_number IN VARCHAR2,
                             p_cnic        IN VARCHAR2,
                             p_dsa_status  IN VARCHAR2,
                             p_remarks     IN VARCHAR2,
                             p_sort_order  IN NUMBER,
                             p_updated_by  IN NUMBER,
                             io_cursor     OUT t_cursor);
  PROCEDURE P_DELETE_INQ_DSA(p_dsa_id     IN NUMBER,
                             p_updated_by IN NUMBER,
                             io_cursor    OUT t_cursor);

  PROCEDURE SAVE_INQ_FINDINGS_RECOMM(p_complaint_id  IN NUMBER,
                                     p_accusation_id IN NUMBER, -- 0 = Additional Charges
                                     p_finding_text  IN CLOB,
                                     p_recom_text    IN CLOB,
                                     p_outcome       in varchar2,
                                     p_ppno          IN VARCHAR2,
                                     io_cursor       OUT t_cursor);
  PROCEDURE GET_INQ_FINDINGS_RECOMM(p_complaint_id  IN NUMBER,
                                    p_accusation_id IN NUMBER,
                                    io_cursor       OUT t_cursor);

  PROCEDURE P_GET_INQ_EVIDENCE_STEP(P_COMPLAINT_ID IN NUMBER,
                                    IO_CURSOR      OUT SYS_REFCURSOR);

  PROCEDURE P_SAVE_INQ_EVIDENCE_STEP(P_COMPLAINT_ID                   IN NUMBER,
                                     P_MATERIAL_EVIDENCE_DETAIL       IN CLOB,
                                     P_CIRCUMSTANTIAL_EVIDENCE_DETAIL IN CLOB,
                                     io_cursor                        OUT t_cursor);

  PROCEDURE P_GET_INQ_PROCEEDINGS(P_COMPLAINT_ID IN NUMBER,
                                  IO_CURSOR      OUT SYS_REFCURSOR);

  PROCEDURE P_SAVE_INQ_PROCEEDING(P_PROCEEDING_ID               IN OUT NUMBER,
                                  P_COMPLAINT_ID                IN NUMBER,
                                  P_NOTICE_REFERENCE            IN VARCHAR2,
                                  P_VISIT_DATE                  IN DATE,
                                  P_PLACE_VISITED               IN CLOB,
                                  P_PARTICIPANTS_DETAIL         IN CLOB,
                                  P_MISSING_PARTICIPANTS_REASON IN CLOB,
                                  P_SORT_ORDER                  IN NUMBER,
                                  P_STATUS                      IN VARCHAR2,
                                  P_USER_ID                     IN NUMBER,
                                  io_cursor                     OUT t_cursor);

  PROCEDURE P_DELETE_INQ_PROCEEDING(P_PROCEEDING_ID IN NUMBER,
                                    P_UPDATED_BY    IN NUMBER,
                                    io_cursor       OUT t_cursor);

  PROCEDURE P_FINALIZE_IID_INQUIRY_REPORT(P_COMPLAINT_ID IN NUMBER,
                                          P_UPDATED_BY   IN NUMBER);

  PROCEDURE P_ENQUEUE_EMAIL(P_EVENT_CODE IN VARCHAR2,
                            P_REF_ID1    IN NUMBER,
                            P_REF_ID2    IN NUMBER,
                            P_MAIL_TO    IN VARCHAR2,
                            P_MAIL_CC    IN VARCHAR2,
                            P_SUBJECT    IN VARCHAR2,
                            P_BODY       IN CLOB,
                            O_EMAIL_ID   OUT NUMBER);

  PROCEDURE P_GET_EMAIL_QUEUE(P_STATUS    IN VARCHAR2,
                              P_FROM_DATE IN DATE,
                              P_TO_DATE   IN DATE,
                              IO_CURSOR   OUT T_CURSOR);

  PROCEDURE P_MARK_EMAIL_SENT(P_EMAIL_ID IN NUMBER);

  PROCEDURE P_MARK_EMAIL_FAILED(P_EMAIL_ID   IN NUMBER,
                                P_ERROR_TEXT IN VARCHAR2);

END PKG_INQ;
/

CREATE OR REPLACE PACKAGE BODY PKG_INQ AS

  ------------------------------------------------------------------
  -- Status constants (must align with UI)
  ------------------------------------------------------------------
  C_STATUS_SUBMITTED      CONSTANT VARCHAR2(50) := 'Submitted';
  C_STATUS_IN_ASSESS      CONSTANT VARCHAR2(50) := 'Initial Assessment';
  C_STATUS_HEAD_REVIEW    CONSTANT VARCHAR2(50) := 'Head Review';
  C_STATUS_PLAN_DRAFTED   CONSTANT VARCHAR2(50) := 'Plan Drafted';
  C_STATUS_PLAN_APPROVED  CONSTANT VARCHAR2(50) := 'Plan Approved';
  C_STATUS_REPORT_DRAFT   CONSTANT VARCHAR2(50) := 'Inquiry Report Drafted';
  C_STATUS_UNDER_REVIEW   CONSTANT VARCHAR2(50) := 'Under Review';
  C_STATUS_FINAL_APPROVAL CONSTANT VARCHAR2(50) := 'Final Approval';
  C_STATUS_CLOSED         CONSTANT VARCHAR2(50) := 'Closed/Issued';

  ------------------------------------------------------------------
  -- Helpers
  ------------------------------------------------------------------
  FUNCTION F_GEN_COMPLAINT_NO(P_COMPLAINT_ID IN NUMBER) RETURN VARCHAR2 IS
  BEGIN
    RETURN 'IID-' || TO_CHAR(P_COMPLAINT_ID, 'FM000000');
  END F_GEN_COMPLAINT_NO;

  PROCEDURE SET_CASE_STATUS(P_COMPLAINT_ID IN NUMBER,
                            P_STATUS       IN VARCHAR2,
                            P_UPDATED_BY   IN NUMBER DEFAULT NULL) IS
  BEGIN
    UPDATE T_AU_IID_COMPLAINT_HDR
       SET STATUS           = P_STATUS,
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = NVL(P_UPDATED_BY, UPDATED_BY_PP_NO)
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
  END SET_CASE_STATUS;

  ------------------------------------------------------------------
  -- LOOKUPS
  ------------------------------------------------------------------
  PROCEDURE GET_RBH_LIST(P_REGION_ID IN NUMBER, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT DISTINCT e.CODE, e.NAME
        FROM T_AUDITEE_ENTITIES e
       WHERE e.TYPE_ID = 5
       ORDER BY e.NAME;
  END GET_RBH_LIST;

  PROCEDURE P_GETINSPECTIONUNITS(IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT e.ENTITY_ID   AS I_ID,
             e.CODE        AS I_CODE,
             e.NAME        AS UNIT_NAME,
             e.DESCRIPTION AS DISCRIPTION,
             e.ACTIVE      AS STATUS,
             e.ACTIVE      AS ISACTIVE
        FROM T_AUDITEE_ENTITIES e
       WHERE UPPER(e.NAME) LIKE '%INQ%'
         AND e.ACTIVE = 'Y'
       ORDER BY e.ENTITY_ID;
  END P_GETINSPECTIONUNITS;

  ------------------------------------------------------------------
  -- COMPLAINT HEADER CREATE
  ------------------------------------------------------------------
  PROCEDURE P_CREATE_COMPLAINT_HDR(P_INTAKE_CHANNEL     IN VARCHAR2,
                                   P_SUBMITTED_BY_PP_NO IN NUMBER,
                                   P_COMPLAINT_ID       OUT NUMBER,
                                   P_COMPLAINT_NO       OUT VARCHAR2) IS
    V_COMPLAINT_NO VARCHAR2(50);
  BEGIN
    INSERT INTO T_AU_IID_COMPLAINT_HDR
      (COMPLAINT_NO,
       INTAKE_CHANNEL,
       STATUS,
       SUBMITTED_ON,
       SUBMITTED_BY_PP_NO,
       ASSIGNED_UNIT_ID,
       ACTIVE_FLAG,
       STATUS_ID)
    VALUES
      (NULL,
       UPPER(TRIM(P_INTAKE_CHANNEL)),
       'SUBMITTED',
       SYSDATE,
       P_SUBMITTED_BY_PP_NO,
       0,
       'Y',
       346)
    RETURNING COMPLAINT_ID INTO P_COMPLAINT_ID;

    V_COMPLAINT_NO := F_GEN_COMPLAINT_NO(P_COMPLAINT_ID);

    UPDATE T_AU_IID_COMPLAINT_HDR
       SET COMPLAINT_NO = V_COMPLAINT_NO
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;

    P_COMPLAINT_NO := V_COMPLAINT_NO;
  END P_CREATE_COMPLAINT_HDR;

  ------------------------------------------------------------------
  -- SAVE IAID DETAILS (UPSERT)
  ------------------------------------------------------------------
  PROCEDURE P_UPSERT_COMPLAINT_IAID(P_COMPLAINT_ID       IN NUMBER,
                                    P_NATURE             IN VARCHAR2,
                                    P_CATEGORY           IN VARCHAR2,
                                    P_RECEIVED_FROM      IN VARCHAR2,
                                    P_LOCATION_TYPE_ID   IN NUMBER,
                                    P_GM_OFFICE_ID       IN NUMBER,
                                    P_REGION_ID          IN NUMBER,
                                    P_BRANCH_ID          IN NUMBER,
                                    P_CONTENTS           IN CLOB,
                                    P_UPLOADED_COMPLAINT IN VARCHAR2,
                                    P_UPLOADED_FFR       IN VARCHAR2,
                                    P_UPLOADED_EVIDENCE  IN VARCHAR2,
                                    P_ACTION_REQUIRED    IN VARCHAR2) IS
  BEGIN
    MERGE INTO T_AU_IID_COMPLAINT_IAID t
    USING (SELECT P_COMPLAINT_ID AS COMPLAINT_ID FROM DUAL) s
    ON (t.COMPLAINT_ID = s.COMPLAINT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.NATURE             = P_NATURE,
             t.CATEGORY           = P_CATEGORY,
             t.RECEIVED_FROM      = P_RECEIVED_FROM,
             t.LOCATION_TYPE_ID   = P_LOCATION_TYPE_ID,
             t.GM_OFFICE_ID       = P_GM_OFFICE_ID,
             t.REGION_ID          = P_REGION_ID,
             t.BRANCH_ID          = P_BRANCH_ID,
             t.CONTENTS           = P_CONTENTS,
             t.UPLOADED_COMPLAINT = P_UPLOADED_COMPLAINT,
             t.UPLOADED_FFR       = P_UPLOADED_FFR,
             t.UPLOADED_EVIDENCE  = P_UPLOADED_EVIDENCE,
             t.ACTION_REQUIRED    = P_ACTION_REQUIRED
    WHEN NOT MATCHED THEN
      INSERT
        (COMPLAINT_ID,
         NATURE,
         CATEGORY,
         RECEIVED_FROM,
         LOCATION_TYPE_ID,
         GM_OFFICE_ID,
         REGION_ID,
         BRANCH_ID,
         CONTENTS,
         UPLOADED_COMPLAINT,
         UPLOADED_FFR,
         UPLOADED_EVIDENCE,
         ACTION_REQUIRED)
      VALUES
        (P_COMPLAINT_ID,
         P_NATURE,
         P_CATEGORY,
         P_RECEIVED_FROM,
         P_LOCATION_TYPE_ID,
         P_GM_OFFICE_ID,
         P_REGION_ID,
         P_BRANCH_ID,
         P_CONTENTS,
         P_UPLOADED_COMPLAINT,
         P_UPLOADED_FFR,
         P_UPLOADED_EVIDENCE,
         P_ACTION_REQUIRED);

    UPDATE T_AU_IID_COMPLAINT_HDR
       SET UPDATED_ON = SYSDATE
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
  END P_UPSERT_COMPLAINT_IAID;

  PROCEDURE P_ADD_COMPLAINANT(P_COMPLAINT_ID     IN NUMBER,
                              P_COMPLAINANT_NAME IN VARCHAR2,
                              P_CNIC             IN VARCHAR2,
                              P_CELLULAR_NUMBER  IN VARCHAR2,
                              P_MAILING_ADDRESS  IN VARCHAR2,
                              P_GENDER           IN VARCHAR2,
                              P_IS_PRIMARY       IN CHAR,
                              P_BY_PP_NO         IN NUMBER,
                              O_COMPLAINANT_ID   OUT NUMBER) IS
    L_IS_PRIMARY CHAR(1) := NVL(UPPER(TRIM(P_IS_PRIMARY)), 'N');
  BEGIN
    INSERT INTO T_AU_IID_COMPLAINANT
      (COMPLAINT_ID,
       COMPLAINANT_NAME,
       CNIC,
       CELLULAR_NUMBER,
       MAILING_ADDRESS,
       GENDER,
       IS_PRIMARY,
       ACTIVE_FLAG,
       CREATED_ON,
       CREATED_BY_PP_NO)
    VALUES
      (P_COMPLAINT_ID,
       P_COMPLAINANT_NAME,
       P_CNIC,
       P_CELLULAR_NUMBER,
       P_MAILING_ADDRESS,
       P_GENDER,
       CASE WHEN L_IS_PRIMARY = 'Y' THEN 'Y' ELSE 'N' END,
       'Y',
       SYSDATE,
       P_BY_PP_NO)
    RETURNING COMPLAINANT_ID INTO O_COMPLAINANT_ID;

    IF L_IS_PRIMARY = 'Y' THEN
      UPDATE T_AU_IID_COMPLAINANT
         SET IS_PRIMARY       = 'N',
             UPDATED_ON       = SYSDATE,
             UPDATED_BY_PP_NO = P_BY_PP_NO
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND ACTIVE_FLAG = 'Y'
         AND COMPLAINANT_ID <> O_COMPLAINANT_ID
         AND IS_PRIMARY = 'Y';
    END IF;
  END P_ADD_COMPLAINANT;

  PROCEDURE P_UPDATE_COMPLAINANT(P_COMPLAINANT_ID   IN NUMBER,
                                 P_COMPLAINANT_NAME IN VARCHAR2,
                                 P_CNIC             IN VARCHAR2,
                                 P_CELLULAR_NUMBER  IN VARCHAR2,
                                 P_MAILING_ADDRESS  IN VARCHAR2,
                                 P_GENDER           IN VARCHAR2,
                                 P_IS_PRIMARY       IN CHAR,
                                 P_BY_PP_NO         IN NUMBER) IS
    L_COMPLAINT_ID NUMBER;
    L_IS_PRIMARY   CHAR(1) := NVL(UPPER(TRIM(P_IS_PRIMARY)), 'N');
  BEGIN
    SELECT COMPLAINT_ID
      INTO L_COMPLAINT_ID
      FROM T_AU_IID_COMPLAINANT
     WHERE COMPLAINANT_ID = P_COMPLAINANT_ID;

    UPDATE T_AU_IID_COMPLAINANT
       SET COMPLAINANT_NAME = P_COMPLAINANT_NAME,
           CNIC             = P_CNIC,
           CELLULAR_NUMBER  = P_CELLULAR_NUMBER,
           MAILING_ADDRESS  = P_MAILING_ADDRESS,
           GENDER           = P_GENDER,
           IS_PRIMARY = CASE
                          WHEN L_IS_PRIMARY = 'Y' THEN
                           'Y'
                          ELSE
                           'N'
                        END,
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_BY_PP_NO
     WHERE COMPLAINANT_ID = P_COMPLAINANT_ID;

    IF L_IS_PRIMARY = 'Y' THEN
      UPDATE T_AU_IID_COMPLAINANT
         SET IS_PRIMARY       = 'N',
             UPDATED_ON       = SYSDATE,
             UPDATED_BY_PP_NO = P_BY_PP_NO
       WHERE COMPLAINT_ID = L_COMPLAINT_ID
         AND ACTIVE_FLAG = 'Y'
         AND COMPLAINANT_ID <> P_COMPLAINANT_ID
         AND IS_PRIMARY = 'Y';
    END IF;
  END P_UPDATE_COMPLAINANT;

  PROCEDURE P_DELETE_COMPLAINANT(P_COMPLAINANT_ID IN NUMBER,
                                 P_BY_PP_NO       IN NUMBER) IS
  BEGIN
    UPDATE T_AU_IID_COMPLAINANT
       SET ACTIVE_FLAG      = 'N',
           IS_PRIMARY       = 'N',
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_BY_PP_NO
     WHERE COMPLAINANT_ID = P_COMPLAINANT_ID;
  END P_DELETE_COMPLAINANT;

  PROCEDURE P_GET_COMPLAINANTS_BY_COMPLAINT(P_COMPLAINT_ID IN NUMBER,
                                            IO_CURSOR      OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT COMPLAINANT_ID,
             COMPLAINT_ID,
             COMPLAINANT_NAME,
             CNIC,
             CELLULAR_NUMBER,
             MAILING_ADDRESS,
             GENDER,
             IS_PRIMARY
        FROM T_AU_IID_COMPLAINANT
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND ACTIVE_FLAG = 'Y'
       ORDER BY CASE
                  WHEN IS_PRIMARY = 'Y' THEN
                   0
                  ELSE
                   1
                END,
                COMPLAINANT_ID;
  END P_GET_COMPLAINANTS_BY_COMPLAINT;

  PROCEDURE P_SET_PRIMARY_COMPLAINANT(P_COMPLAINT_ID   IN NUMBER,
                                      P_COMPLAINANT_ID IN NUMBER,
                                      P_BY_PP_NO       IN NUMBER) IS
  BEGIN
    UPDATE T_AU_IID_COMPLAINANT
       SET IS_PRIMARY       = 'N',
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_BY_PP_NO
     WHERE COMPLAINT_ID = P_COMPLAINT_ID
       AND ACTIVE_FLAG = 'Y'
       AND IS_PRIMARY = 'Y';

    UPDATE T_AU_IID_COMPLAINANT
       SET IS_PRIMARY       = 'Y',
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_BY_PP_NO
     WHERE COMPLAINT_ID = P_COMPLAINT_ID
       AND COMPLAINANT_ID = P_COMPLAINANT_ID
       AND ACTIVE_FLAG = 'Y';
  END P_SET_PRIMARY_COMPLAINANT;

  PROCEDURE P_SAVE_COMPLAINT_IAID(P_COMPLAINT_ID       IN NUMBER,
                                  P_NATURE             IN VARCHAR2,
                                  P_CATEGORY           IN VARCHAR2,
                                  P_RECEIVED_FROM      IN VARCHAR2,
                                  P_LOCATION_TYPE_ID   IN NUMBER,
                                  P_GM_OFFICE_ID       IN NUMBER,
                                  P_REGION_ID          IN NUMBER,
                                  P_BRANCH_ID          IN NUMBER,
                                  P_COMPLAINANT_NAME   IN VARCHAR2,
                                  P_CNIC               IN VARCHAR2,
                                  P_CELLULAR_NUMBER    IN VARCHAR2,
                                  P_MAILING_ADDRESS    IN VARCHAR2,
                                  P_GENDER             IN VARCHAR2,
                                  P_CONTENTS           IN CLOB,
                                  P_UPLOADED_COMPLAINT IN VARCHAR2,
                                  P_UPLOADED_FFR       IN VARCHAR2,
                                  P_UPLOADED_EVIDENCE  IN VARCHAR2,
                                  P_ACTION_REQUIRED    IN VARCHAR2) IS
    L_COMPLAINANT_ID NUMBER;
  BEGIN
    P_UPSERT_COMPLAINT_IAID(P_COMPLAINT_ID       => P_COMPLAINT_ID,
                            P_NATURE             => P_NATURE,
                            P_CATEGORY           => P_CATEGORY,
                            P_RECEIVED_FROM      => P_RECEIVED_FROM,
                            P_LOCATION_TYPE_ID   => P_LOCATION_TYPE_ID,
                            P_GM_OFFICE_ID       => P_GM_OFFICE_ID,
                            P_REGION_ID          => P_REGION_ID,
                            P_BRANCH_ID          => P_BRANCH_ID,
                            P_CONTENTS           => P_CONTENTS,
                            P_UPLOADED_COMPLAINT => P_UPLOADED_COMPLAINT,
                            P_UPLOADED_FFR       => P_UPLOADED_FFR,
                            P_UPLOADED_EVIDENCE  => P_UPLOADED_EVIDENCE,
                            P_ACTION_REQUIRED    => P_ACTION_REQUIRED);

    -- Create/Update primary complainant easily for now:
    -- If no primary exists, add as primary. If primary exists, update via P_SET_PRIMARY + P_UPDATE later.
    -- Simplest: always add if missing.
    BEGIN
      SELECT MAX(COMPLAINANT_ID)
        INTO L_COMPLAINANT_ID
        FROM T_AU_IID_COMPLAINANT
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND ACTIVE_FLAG = 'Y'
         AND IS_PRIMARY = 'Y';
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_COMPLAINANT_ID := NULL;
    END;

    IF L_COMPLAINANT_ID IS NULL THEN
      P_ADD_COMPLAINANT(P_COMPLAINT_ID     => P_COMPLAINT_ID,
                        P_COMPLAINANT_NAME => P_COMPLAINANT_NAME,
                        P_CNIC             => P_CNIC,
                        P_CELLULAR_NUMBER  => P_CELLULAR_NUMBER,
                        P_MAILING_ADDRESS  => P_MAILING_ADDRESS,
                        P_GENDER           => P_GENDER,
                        P_IS_PRIMARY       => 'Y',
                        P_BY_PP_NO         => NULL,
                        O_COMPLAINANT_ID   => L_COMPLAINANT_ID);
    ELSE
      P_UPDATE_COMPLAINANT(P_COMPLAINANT_ID   => L_COMPLAINANT_ID,
                           P_COMPLAINANT_NAME => P_COMPLAINANT_NAME,
                           P_CNIC             => P_CNIC,
                           P_CELLULAR_NUMBER  => P_CELLULAR_NUMBER,
                           P_MAILING_ADDRESS  => P_MAILING_ADDRESS,
                           P_GENDER           => P_GENDER,
                           P_IS_PRIMARY       => 'Y',
                           P_BY_PP_NO         => NULL);
    END IF;
  END;

  ------------------------------------------------------------------
  -- LEGACY WRAPPER (keep signature for C# compatibility)
  ------------------------------------------------------------------
  PROCEDURE ADD_COMPLAINT(P_NATURE             IN VARCHAR2,
                          P_CATEGORY           IN VARCHAR2,
                          P_SOURCE             IN VARCHAR2,
                          P_SOURCE_OTHER_TEXT  IN VARCHAR2,
                          P_PERTAINS_TO        IN VARCHAR2,
                          P_FIELD_TYPE         IN VARCHAR2,
                          P_HO_UNIT_TYPE_ID    IN NUMBER,
                          P_HO_UNIT_ID         IN NUMBER,
                          P_REGION_ID          IN NUMBER,
                          P_BRANCH_ID          IN NUMBER,
                          P_CONTENTS           IN CLOB,
                          P_UPLOADED_COMPLAINT IN VARCHAR2,
                          P_UPLOADED_FFR       IN VARCHAR2,
                          P_UPLOADED_EVIDENCE  IN VARCHAR2,
                          P_ACTION_REQUIRED    IN VARCHAR2,
                          P_SUBMITTED_BY       IN NUMBER,
                          O_COMPLAINT_ID       OUT NUMBER) IS
    V_NO             VARCHAR2(50);
    L_LOCATION_TYPE  NUMBER;
    L_COMPLAINANT_ID NUMBER;
    L_RECEIVED_FROM  VARCHAR2(200);
  BEGIN
    -- 1) Create header
    P_CREATE_COMPLAINT_HDR('IAID', P_SUBMITTED_BY, O_COMPLAINT_ID, V_NO);

    -- 2) Derive received_from (use P_SOURCE; if "Other" then store detail)
    L_RECEIVED_FROM := CASE
                         WHEN P_SOURCE IS NULL THEN
                          NULL
                         WHEN UPPER(TRIM(P_SOURCE)) IN ('OTHER', 'OTHERS') AND
                              P_SOURCE_OTHER_TEXT IS NOT NULL THEN
                          TRIM(P_SOURCE_OTHER_TEXT)
                         ELSE
                          TRIM(P_SOURCE)
                       END;

    -- 3) Convert location type if passed as text (safe)
    L_LOCATION_TYPE := CASE
                         WHEN P_FIELD_TYPE IS NULL THEN
                          NULL
                         WHEN REGEXP_LIKE(P_FIELD_TYPE, '^\s*\d+\s*$') THEN
                          TO_NUMBER(TRIM(P_FIELD_TYPE))
                         ELSE
                          NULL
                       END;

    -- 4) Upsert complaint details (IAID table)
    P_UPSERT_COMPLAINT_IAID(P_COMPLAINT_ID       => O_COMPLAINT_ID,
                            P_NATURE             => P_NATURE,
                            P_CATEGORY           => P_CATEGORY,
                            P_RECEIVED_FROM      => L_RECEIVED_FROM,
                            P_LOCATION_TYPE_ID   => L_LOCATION_TYPE,
                            P_GM_OFFICE_ID       => P_HO_UNIT_ID, -- HO/GM office id comes from P_HO_UNIT_ID in this legacy wrapper
                            P_REGION_ID          => P_REGION_ID,
                            P_BRANCH_ID          => P_BRANCH_ID,
                            P_CONTENTS           => P_CONTENTS,
                            P_UPLOADED_COMPLAINT => P_UPLOADED_COMPLAINT,
                            P_UPLOADED_FFR       => P_UPLOADED_FFR,
                            P_UPLOADED_EVIDENCE  => P_UPLOADED_EVIDENCE,
                            P_ACTION_REQUIRED    => P_ACTION_REQUIRED);

    -- 5) Add PRIMARY complainant row (legacy wrapper does not have complainant fields)
    -- If you truly have no complainant details here, insert a minimal row to keep model consistent.
    -- You can later update it through complainant screen.
    P_ADD_COMPLAINANT(P_COMPLAINT_ID     => O_COMPLAINT_ID,
                      P_COMPLAINANT_NAME => NULL,
                      P_CNIC             => NULL,
                      P_CELLULAR_NUMBER  => NULL,
                      P_MAILING_ADDRESS  => NULL,
                      P_GENDER           => NULL,
                      P_IS_PRIMARY       => 'Y',
                      P_BY_PP_NO         => P_SUBMITTED_BY,
                      O_COMPLAINANT_ID   => L_COMPLAINANT_ID);

    COMMIT;
  END ADD_COMPLAINT;

  ------------------------------------------------------------------
  -- GET APIs
  ------------------------------------------------------------------
  PROCEDURE P_GET_COMPLAINT_HDR(P_COMPLAINT_ID IN NUMBER,
                                T_CURSOR       OUT T_CURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             h.INTAKE_CHANNEL,
             h.STATUS,
             h.SUBMITTED_ON,
             h.SUBMITTED_BY_PP_NO,
             h.ASSIGNED_UNIT_ID,
             h.ACTIVE_FLAG
        FROM T_AU_IID_COMPLAINT_HDR h
       WHERE h.COMPLAINT_ID = P_COMPLAINT_ID;
  END P_GET_COMPLAINT_HDR;

  PROCEDURE P_GET_COMPLAINT_IAID(P_COMPLAINT_ID IN NUMBER,
                                 T_CURSOR       OUT T_CURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT d.*
        FROM T_AU_IID_COMPLAINT_IAID d
       WHERE d.COMPLAINT_ID = P_COMPLAINT_ID;
  END P_GET_COMPLAINT_IAID;

  PROCEDURE GET_COMPLAINTS(P_USER_ID IN NUMBER, IO_CURSOR OUT T_CURSOR) IS

    R_ID number;
  BEGIN
    select m.role_id
      into R_ID
      from t_user_maping m
     where m.ppno = P_USER_ID;

    if (R_ID in (1)) then
      OPEN IO_CURSOR FOR
        SELECT h.complaint_id,
               h.COMPLAINT_NO,
               c.complainant_name,
               d.NATURE,
               d.received_from    as Source,
               e.name             as ASSIGNED_UNIT,
               h.STATUS,
               h.updated_on       as SUBMITTED_ON
          FROM T_AU_IID_COMPLAINT_HDR h
         inner JOIN T_AU_IID_COMPLAINT_IAID d
            ON d.COMPLAINT_ID = h.COMPLAINT_ID
         inner join t_au_iid_complainant c
            on c.complaint_id = d.complaint_id
         inner join t_auditee_entities e
            on h.assigned_unit_id = e.entity_id
         WHERE h.status_id between 1 and 7
         ORDER BY h.complaint_id DESC;

    else

      OPEN IO_CURSOR FOR
        SELECT h.complaint_id     COMPLAINT_ID,
               h.COMPLAINT_NO     COMPLAINT_NO,
               c.complainant_name COMPLAINANT_NAME,
               d.NATURE           NATURE,
               d.received_from    as SOURCE,
               h.ASSIGNED_UNIT_ID ASSIGNED_UNIT,
               h.STATUS,
               h.submitted_on
          FROM T_AU_IID_COMPLAINT_HDR h
         inner JOIN T_AU_IID_COMPLAINT_IAID d
            ON d.COMPLAINT_ID = h.COMPLAINT_ID
         inner join t_au_iid_complainant c
            on c.complaint_id = d.complaint_id
        -- WHERE h.SUBMITTED_BY_PP_NO = P_USER_ID
         ORDER BY h.SUBMITTED_ON DESC;
    end if;
  END GET_COMPLAINTS;

  PROCEDURE GET_COMPLAINTS_WITHOUT_ASSESSMENT(T_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             h.STATUS,
             h.SUBMITTED_ON,
             h.ASSIGNED_UNIT_ID,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM,
             d.CONTENTS
        FROM T_AU_IID_COMPLAINT_HDR h
        JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       WHERE NOT EXISTS (SELECT 1
                FROM T_AU_IID_ASSESSMENT a
               WHERE a.COMPLAINT_ID = h.COMPLAINT_ID)
       ORDER BY h.COMPLAINT_ID DESC;
  END GET_COMPLAINTS_WITHOUT_ASSESSMENT;

  PROCEDURE GET_COMPLAINTS_DD(P_PAGE_ID in number, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             d.NATURE AS NATURE,
             c.complainant_name || '-' || s.status_name STATUS
        FROM T_AU_IID_COMPLAINT_HDR h
        LEFT JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
        LEFT JOIN T_AU_IID_COMPLAINANT c
          ON c.COMPLAINT_ID = h.COMPLAINT_ID
        left join T_AU_IID_STATUS_MST s
          on s.status_id = h.status_id
       WHERE h.status_id = case
               when P_PAGE_ID = 420 then
                h.status_id
               else
                P_PAGE_ID
             end
       ORDER BY h.COMPLAINT_ID DESC;
  END GET_COMPLAINTS_DD;

  PROCEDURE GET_LATEST_INQUIRY_REPORT_BY_COMPLAINT(p_complaint_id IN NUMBER,
                                                   io_cursor      OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT c.complaint_id AS report_id,

             /* Complainant name */
             cc.complainant_name AS name_complainant,

             /* Accused names combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                     al.person_name || ', ')
                                          ORDER BY al.accused_row_id) AS CLOB),
                           ', ')
                FROM t_au_iid_inq_accused_list al
               WHERE al.complaint_id = c.complaint_id) AS name_accused,

             /* Gist from complaint main text */
             c.contents AS gist,

             /* Proceedings from statements + record scrutinized combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                     CASE
                                                       WHEN s.role_type = 'COMPLAINANT' THEN
                                                        'Statement of Complainant: '
                                                       WHEN s.role_type = 'ACCUSED' THEN
                                                        'Statement of Accused: '
                                                       ELSE
                                                        'Statement: '
                                                     END ||
                                                     NVL(s.key_points, '') || CASE
                                                       WHEN s.statement_datetime IS NOT NULL THEN
                                                        ' (Dated: ' ||
                                                        TO_CHAR(s.statement_datetime, 'DD-MON-YYYY HH:MI AM') || ')'
                                                       ELSE
                                                        ''
                                                     END || CHR(10)) ORDER BY
                                          s.statement_id) AS CLOB),
                           CHR(10))
                FROM t_au_iid_inq_statements s
               WHERE s.complaint_id = c.complaint_id) || CASE
               WHEN EXISTS (SELECT 1
                       FROM t_au_iid_inq_record_scrutinized rs
                      WHERE rs.complaint_id = c.complaint_id) THEN
                CHR(10) || CHR(10) || 'Record Scrutinized:' || CHR(10) ||
                (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                        NVL(rs.record_details,
                                                            '') || CHR(10))
                                             ORDER BY rs.rec_id) AS CLOB),
                              CHR(10))
                   FROM t_au_iid_inq_record_scrutinized rs
                  WHERE rs.complaint_id = c.complaint_id)
               ELSE
                NULL
             END AS proceedings,

             /* Findings combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                     'Allegation: ' ||
                                                     NVL(a.accusation_text,
                                                         '') || CHR(10) ||
                                                     'Finding: ' ||
                                                     NVL(fr.findings_text, '') ||
                                                     CHR(10) || 'Outcome: ' ||
                                                     NVL(fr.recommendation_text,
                                                         '') || CHR(10) ||
                                                     CHR(10)) ORDER BY
                                          a.accusation_id) AS CLOB),
                           CHR(10))
                FROM t_au_iid_inq_find_recomm fr
                JOIN t_au_iid_inq_accusations a
                  ON a.accusation_id = fr.accusation_id
               WHERE fr.complaint_id = c.complaint_id) AS findings,

             /* Recommendations combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                     'Allegation: ' ||
                                                     NVL(a.accusation_text,
                                                         '') || CHR(10) ||
                                                     'Recommendation: ' ||
                                                     NVL(fr.recommendation_text,
                                                         '') || CHR(10) ||
                                                     CHR(10)) ORDER BY
                                          a.accusation_id) AS CLOB),
                           CHR(10))
                FROM t_au_iid_inq_find_recomm fr
                JOIN t_au_iid_inq_accusations a
                  ON a.accusation_id = fr.accusation_id
               WHERE fr.complaint_id = c.complaint_id) AS recommendation,

             /* Step 9 summary fields */
             NVL(vs.conclusion, '') AS conclusion,
             NVL(vs.reported_in_audit_report, rpt.reported_in_audit_report) AS reported_in_audit_report,
             NVL(vs.audit_report_reference_detail,
                 rpt.audit_report_reference_detail) AS audit_report_reference_detail,

             /* Uploaded report: latest saved report snapshot if available */
             rpt.uploaded_report AS uploaded_report,

             /* Evidence files combined */
             (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e, ef.file_path || ', ')
                                          ORDER BY ef.evidence_id) AS CLOB),
                           ', ')
                FROM t_au_iid_inq_evidence_files ef
               WHERE ef.complaint_id = c.complaint_id) AS uploaded_evidence,

             /* DSA snapshot / fallback */
             NVL(rpt.uploaded_dsa,
                 (SELECT RTRIM(XMLCAST(XMLAGG(XMLELEMENT(e,
                                                         dsa.dsa_status || ', ')
                                              ORDER BY dsa.dsa_id) AS CLOB),
                               ', ')
                    FROM t_au_iid_inq_dsa dsa
                   WHERE dsa.complaint_id = c.complaint_id)) AS uploaded_dsa,

             TO_CHAR(sysdate, 'DD-MON-YYYY HH:MI AM') AS submitted_on

        FROM t_au_iid_complaint_iaid c
       inner join t_au_iid_complainant cc
          on cc.complaint_id = c.complaint_id
        LEFT JOIN (SELECT r1.*
                     FROM t_au_iid_report r1
                    WHERE r1.report_id =
                          (SELECT MAX(r2.report_id)
                             FROM t_au_iid_report r2
                            WHERE r2.complaint_id = r1.complaint_id)) rpt
          ON rpt.complaint_id = c.complaint_id
        LEFT JOIN t_au_iid_inq_violation_step vs
          ON vs.complaint_id = c.complaint_id
         AND NVL(vs.status, 'A') = 'A'
       WHERE c.complaint_id = p_complaint_id;
  END GET_LATEST_INQUIRY_REPORT_BY_COMPLAINT;

  PROCEDURE GET_COMPLAINT(P_COMPLAINT_ID IN NUMBER, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             h.STATUS,
             h.SUBMITTED_ON,
             h.SUBMITTED_BY_PP_NO,
             (Select e.name
                from t_auditee_entities e
               where e.entity_id = a.assigned_unit_id) as assigned_unit,
             a.assigned_unit_id,
             a.assessment_id,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM,
             d.LOCATION_TYPE_ID,
             (Select e.name
                from t_auditee_entities e
               where e.entity_id = d.GM_OFFICE_ID) as GM_OFFICE,
             d.gm_office_id,
             (Select e.name
                from t_auditee_entities e
               where e.entity_id = d.REGION_ID) as REGION,
             d.region_id,
             (Select e.name
                from t_auditee_entities e
               where e.entity_id = d.BRANCH_ID) as BRANCH,
             d.branch_id,
             C.COMPLAINANT_NAME,
             c.CNIC,
             c.CELLULAR_NUMBER,
             c.MAILING_ADDRESS,
             c.GENDER,
             d.CONTENTS,
             d.UPLOADED_COMPLAINT,
             d.UPLOADED_EVIDENCE,
             d.UPLOADED_FFR,
             d.ACTION_REQUIRED,
             a.ASSESSMENT,
             a.RECOMMENDATION
        FROM T_AU_IID_COMPLAINT_HDR h
       inner JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       inner join T_AU_IID_COMPLAINANT c
          on c.complaint_id = d.complaint_id
         and c.is_primary = 'Y'
        LEFT JOIN T_AU_IID_ASSESSMENT a
          ON a.COMPLAINT_ID = h.COMPLAINT_ID
       WHERE h.COMPLAINT_ID = P_COMPLAINT_ID;
  END GET_COMPLAINT;

  PROCEDURE GET_LATEST_PLAN_BY_COMPLAINT(P_COMPLAINT_ID IN NUMBER,
                                         IO_CURSOR      OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT *
        FROM (SELECT p.PLAN_ID,
                     p.COMPLAINT_ID,
                     p.PLAN_DETAILS,
                     p.PLAN_TITLE,
                     p.START_DATE,
                     p.END_DATE,
                     p.STATUS,
                     p.SUBMITTED_BY,
                     p.SUBMITTED_ON
                FROM T_AU_IID_INV_PLAN p
               WHERE p.COMPLAINT_ID = P_COMPLAINT_ID
               ORDER BY p.SUBMITTED_ON DESC NULLS LAST, p.PLAN_ID DESC)
       WHERE ROWNUM = 1;
  END GET_LATEST_PLAN_BY_COMPLAINT;

  PROCEDURE P_GET_COMPLAINT_LIST(P_INTAKE_CHANNEL IN VARCHAR2 DEFAULT NULL,
                                 P_STATUS         IN VARCHAR2 DEFAULT NULL,
                                 P_FROM_DATE      IN DATE DEFAULT NULL,
                                 P_TO_DATE        IN DATE DEFAULT NULL,
                                 T_CURSOR         OUT T_CURSOR) IS
  BEGIN
    OPEN T_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             h.INTAKE_CHANNEL,
             h.STATUS,
             h.SUBMITTED_ON,
             h.SUBMITTED_BY_PP_NO,
             h.ASSIGNED_UNIT_ID,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM
        FROM T_AU_IID_COMPLAINT_HDR h
        LEFT JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       WHERE h.ACTIVE_FLAG = 'Y'
         AND (P_INTAKE_CHANNEL IS NULL OR
             h.INTAKE_CHANNEL = UPPER(TRIM(P_INTAKE_CHANNEL)))
         AND (P_STATUS IS NULL OR h.STATUS = P_STATUS)
         AND (P_FROM_DATE IS NULL OR h.SUBMITTED_ON >= P_FROM_DATE)
         AND (P_TO_DATE IS NULL OR h.SUBMITTED_ON < (P_TO_DATE + 1))
       ORDER BY h.SUBMITTED_ON DESC;
  END P_GET_COMPLAINT_LIST;

  PROCEDURE P_GET_COMPLAINT_ID_BY_PLAN(P_PLAN_ID      IN NUMBER,
                                       O_COMPLAINT_ID OUT NUMBER) IS
  BEGIN
    SELECT COMPLAINT_ID
      INTO O_COMPLAINT_ID
      FROM T_AU_IID_INV_PLAN
     WHERE PLAN_ID = P_PLAN_ID;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      O_COMPLAINT_ID := NULL;
  END P_GET_COMPLAINT_ID_BY_PLAN;

  PROCEDURE P_GET_COMPLAINT_ID_BY_REPORT(P_REPORT_ID    IN NUMBER,
                                         O_COMPLAINT_ID OUT NUMBER) IS
  BEGIN
    -- NOTE: PKG_INQ uses T_AU_IID_REPORT (not T_AU_IID_INQUIRY_REPORT)
    SELECT COMPLAINT_ID
      INTO O_COMPLAINT_ID
      FROM T_AU_IID_REPORT
     WHERE REPORT_ID = P_REPORT_ID;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      O_COMPLAINT_ID := NULL;
  END P_GET_COMPLAINT_ID_BY_REPORT;

  PROCEDURE P_SAVE_INQ_FINDINGS_REC(P_COMPLAINT_ID   IN NUMBER,
                                    P_FINDINGS       IN CLOB,
                                    P_RECOMMENDATION IN CLOB,
                                    P_UPDATED_BY     IN NUMBER,
                                    IO_CURSOR        OUT T_CURSOR) IS
    L_REPORT_ID NUMBER;
  BEGIN
    -- get latest report row for complaint (if any)
    SELECT MAX(REPORT_ID)
      INTO L_REPORT_ID
      FROM T_AU_IID_REPORT
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;

    IF L_REPORT_ID IS NULL THEN
      SELECT SEQ_AU_IID_REPORT_ID.NEXTVAL INTO L_REPORT_ID FROM DUAL;

      INSERT INTO T_AU_IID_REPORT
        (REPORT_ID,
         COMPLAINT_ID,
         FINDINGS,
         RECOMMENDATION,
         SUBMITTED_ON,
         SUBMITTED_BY)
      VALUES
        (L_REPORT_ID,
         P_COMPLAINT_ID,
         P_FINDINGS,
         P_RECOMMENDATION,
         SYSDATE,
         P_UPDATED_BY);
    ELSE
      UPDATE T_AU_IID_REPORT
         SET FINDINGS = P_FINDINGS, RECOMMENDATION = P_RECOMMENDATION
       WHERE REPORT_ID = L_REPORT_ID;
    END IF;

    OPEN IO_CURSOR FOR
      SELECT 'Y' AS OK,
             'Findings & Recommendations saved.' AS MESSAGE,
             L_REPORT_ID AS ID
        FROM DUAL;

  EXCEPTION
    WHEN OTHERS THEN
      DECLARE
        v_err VARCHAR2(4000);
      BEGIN
        v_err := SQLERRM;

        OPEN io_cursor FOR
          SELECT 'N' AS OK, v_err AS MESSAGE, NULL AS ID FROM dual;
      END;
  END P_SAVE_INQ_FINDINGS_REC;

  ------------------------------------------------------------------
  -- INITIAL ASSESSMENT
  ------------------------------------------------------------------
  PROCEDURE ADD_ASSESSMENT(P_COMPLAINT_ID     IN NUMBER,
                           P_RECEIVED_BY      IN NUMBER,
                           P_ASSESSMENT       IN CLOB,
                           P_RECOMMENDATION   IN VARCHAR2,
                           P_ASSIGNED_UNIT_ID IN NUMBER,
                           O_ASSESSMENT_ID    OUT NUMBER) IS
  BEGIN
    SELECT SEQ_AU_IID_ASSESSMENT_ID.NEXTVAL INTO O_ASSESSMENT_ID FROM DUAL;

    INSERT INTO T_AU_IID_ASSESSMENT
      (ASSESSMENT_ID,
       COMPLAINT_ID,
       RECEIVED_BY,
       ASSESSMENT,
       RECOMMENDATION,
       FORWARDED_ON,
       PRELIM_RISK,
       ASSIGNED_UNIT_ID)
    VALUES
      (O_ASSESSMENT_ID,
       P_COMPLAINT_ID,
       P_RECEIVED_BY,
       P_ASSESSMENT,
       P_RECOMMENDATION,
       SYSDATE,
       NULL,
       P_ASSIGNED_UNIT_ID);

    UPDATE T_AU_IID_COMPLAINT_HDR
       SET ASSIGNED_UNIT_ID = P_ASSIGNED_UNIT_ID,
           STATUS_ID        = 345,
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_RECEIVED_BY
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;
    commit;
    SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_HEAD_REVIEW, P_RECEIVED_BY);
  END ADD_ASSESSMENT;

  ------------------------------------------------------------------
  -- HEAD REVIEW
  ------------------------------------------------------------------
  PROCEDURE ADD_HEAD_REVIEW(P_COMPLAINT_ID           IN NUMBER,
                            P_ASSESSMENT_ID          IN NUMBER,
                            P_REVIEWED_BY            IN NUMBER,
                            P_DIRECTIONS             IN CLOB,
                            P_ASSIGNED_TO_UNIT       IN NUMBER,
                            P_TEAM_LEAD              IN NUMBER,
                            P_TEAM_MEMBERS           IN CLOB,
                            P_ASSIGNED_ON            IN VARCHAR2,
                            P_DUE_DATE               IN VARCHAR2,
                            P_REFERRED_BACK_COMMENTS IN CLOB,
                            P_ACTION                 IN VARCHAR2,
                            O_REVIEW_ID              OUT NUMBER) IS
    L_ASSIGNED_ON  DATE;
    L_DUE_DATE     DATE;
    L_EXIST_REVIEW NUMBER;
  BEGIN
    -- Parse Assigned On
    BEGIN
      L_ASSIGNED_ON := TO_DATE(P_ASSIGNED_ON, 'YYYY-MM-DD');
    EXCEPTION
      WHEN OTHERS THEN
        BEGIN
          L_ASSIGNED_ON := TO_DATE(P_ASSIGNED_ON, 'DD-MON-YYYY');
        EXCEPTION
          WHEN OTHERS THEN
            L_ASSIGNED_ON := SYSDATE;
        END;
    END;

    -- Parse Due Date
    BEGIN
      L_DUE_DATE := TO_DATE(P_DUE_DATE, 'YYYY-MM-DD');
    EXCEPTION
      WHEN OTHERS THEN
        BEGIN
          L_DUE_DATE := TO_DATE(P_DUE_DATE, 'DD-MON-YYYY');
        EXCEPTION
          WHEN OTHERS THEN
            L_DUE_DATE := NULL;
        END;
    END;

    -- Find existing review for this complaint (and assessment if provided)
    BEGIN
      SELECT MAX(REVIEW_ID)
        INTO L_EXIST_REVIEW
        FROM T_AU_IID_HEAD_REVIEW
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND (P_ASSESSMENT_ID IS NULL OR ASSESSMENT_ID = P_ASSESSMENT_ID);
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_EXIST_REVIEW := NULL;
    END;

    IF L_EXIST_REVIEW IS NULL THEN
      SELECT SEQ_AU_IID_HEAD_REVIEW_ID.NEXTVAL INTO O_REVIEW_ID FROM DUAL;
    ELSE
      O_REVIEW_ID := L_EXIST_REVIEW;
    END IF;

    MERGE INTO T_AU_IID_HEAD_REVIEW t
    USING (SELECT O_REVIEW_ID AS REVIEW_ID, P_COMPLAINT_ID AS COMPLAINT_ID
             FROM DUAL) s
    ON (t.REVIEW_ID = s.REVIEW_ID AND t.COMPLAINT_ID = s.COMPLAINT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.ASSESSMENT_ID          = P_ASSESSMENT_ID,
             t.REVIEWED_BY            = P_REVIEWED_BY,
             t.DIRECTIONS             = P_DIRECTIONS,
             t.ASSIGNED_TO_UNIT       = P_ASSIGNED_TO_UNIT,
             t.TEAM_LEAD              = P_TEAM_LEAD,
             t.TEAM_MEMBERS           = P_TEAM_MEMBERS,
             t.ASSIGNED_ON            = L_ASSIGNED_ON,
             t.DUE_DATE               = L_DUE_DATE,
             t.REFERRED_BACK_COMMENTS = P_REFERRED_BACK_COMMENTS,
             t.ACTION                 = P_ACTION,
             t.REVIEWED_ON            = SYSDATE,
             t.APPROVED_ON            = CASE
                                          WHEN UPPER(P_ACTION) = 'APPROVE' THEN
                                           SYSDATE
                                          ELSE
                                           NULL
                                        END WHEN NOT MATCHED THEN INSERT(REVIEW_ID, COMPLAINT_ID, ASSESSMENT_ID, REVIEWED_BY, DIRECTIONS, ASSIGNED_TO_UNIT, TEAM_LEAD, TEAM_MEMBERS, ASSIGNED_ON, DUE_DATE, REFERRED_BACK_COMMENTS, ACTION, REVIEWED_ON, APPROVED_ON) VALUES(O_REVIEW_ID, P_COMPLAINT_ID, P_ASSESSMENT_ID, P_REVIEWED_BY, P_DIRECTIONS, P_ASSIGNED_TO_UNIT, P_TEAM_LEAD, P_TEAM_MEMBERS, L_ASSIGNED_ON, L_DUE_DATE, P_REFERRED_BACK_COMMENTS, P_ACTION, SYSDATE,CASE
               WHEN UPPER(P_ACTION) = 'APPROVE' THEN
                SYSDATE
               ELSE
                NULL
             END);

    -- Update header
    UPDATE T_AU_IID_COMPLAINT_HDR
       SET ASSIGNED_UNIT_ID = P_ASSIGNED_TO_UNIT,
           STATUS_ID        = 348,
           UPDATED_ON       = SYSDATE,
           UPDATED_BY_PP_NO = P_REVIEWED_BY
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;

    -- Status transitions
    IF UPPER(P_ACTION) IN ('REFERBACK', 'REFER BACK') THEN
      SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_IN_ASSESS, P_REVIEWED_BY);
    ELSIF UPPER(P_ACTION) = 'CLOSE' THEN
      SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_CLOSED, P_REVIEWED_BY);
    ELSE
      SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_PLAN_DRAFTED, P_REVIEWED_BY);
    END IF;

    COMMIT;
  END ADD_HEAD_REVIEW;

  ------------------------------------------------------------------
  -- INVESTIGATION PLAN
  ------------------------------------------------------------------
  PROCEDURE ADD_INV_PLAN(P_COMPLAINT_ID    IN NUMBER,
                         P_PLAN_DETAILS    IN CLOB,
                         P_SUBMITTED_BY    IN NUMBER,
                         P_STATUS          IN VARCHAR2,
                         P_INV_RISK        IN VARCHAR2,
                         P_INV_SIZE        IN VARCHAR2,
                         P_NO_OF_DAYS      IN NUMBER,
                         P_TRAVELLING_DAYS IN NUMBER,
                         P_TEAM_LEAD       IN VARCHAR2,
                         P_TEAM_MEMBERS    IN VARCHAR2,
                         P_START_DATE      IN DATE,
                         P_ACTIVITIES_TEXT IN VARCHAR2,
                         O_PLAN_ID         OUT NUMBER) IS
    L_EXIST_PLAN_ID NUMBER;
  BEGIN
    -- Find existing plan for this complaint (if your table can have multiple,
    -- this picks the latest by PLAN_ID)
    BEGIN
      SELECT MAX(PLAN_ID)
        INTO L_EXIST_PLAN_ID
        FROM T_AU_IID_INV_PLAN
       WHERE COMPLAINT_ID = P_COMPLAINT_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_EXIST_PLAN_ID := NULL;
    END;

    IF L_EXIST_PLAN_ID IS NULL THEN
      SELECT SEQ_AU_IID_INV_PLAN_ID.NEXTVAL INTO O_PLAN_ID FROM DUAL;
    ELSE
      O_PLAN_ID := L_EXIST_PLAN_ID;
    END IF;

    MERGE INTO T_AU_IID_INV_PLAN t
    USING (SELECT P_COMPLAINT_ID AS COMPLAINT_ID, O_PLAN_ID AS PLAN_ID
             FROM DUAL) s
    ON (t.PLAN_ID = s.PLAN_ID AND t.COMPLAINT_ID = s.COMPLAINT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.PLAN_DETAILS       = P_PLAN_DETAILS,
             t.SUBMITTED_BY       = P_SUBMITTED_BY,
             t.SUBMITTED_ON       = SYSDATE,
             t.STATUS             = P_STATUS,
             t.PLAN_TITLE         = 'Investigation Plan',
             t.START_DATE         = P_START_DATE,
             t.INVESTIGATION_RISK = P_INV_RISK,
             t.INVESTIGATION_SIZE = P_INV_SIZE,
             t.NO_OF_DAYS         = P_NO_OF_DAYS,
             t.TRAVELLING_DAYS    = P_TRAVELLING_DAYS,
             t.TEAM_LEAD          = P_TEAM_LEAD,
             t.TEAM_MEMBERS       = P_TEAM_MEMBERS,
             t.ACTIVITIES_TEXT    = P_ACTIVITIES_TEXT
    WHEN NOT MATCHED THEN
      INSERT
        (PLAN_ID,
         COMPLAINT_ID,
         PLAN_DETAILS,
         SUBMITTED_BY,
         SUBMITTED_ON,
         STATUS,
         PLAN_TITLE,
         START_DATE,
         INVESTIGATION_RISK,
         INVESTIGATION_SIZE,
         NO_OF_DAYS,
         TRAVELLING_DAYS,
         TEAM_LEAD,
         TEAM_MEMBERS,
         ACTIVITIES_TEXT)
      VALUES
        (O_PLAN_ID,
         P_COMPLAINT_ID,
         P_PLAN_DETAILS,
         P_SUBMITTED_BY,
         SYSDATE,
         P_STATUS,
         'Investigation Plan',
         P_START_DATE,
         P_INV_RISK,
         P_INV_SIZE,
         P_NO_OF_DAYS,
         P_TRAVELLING_DAYS,
         P_TEAM_LEAD,
         P_TEAM_MEMBERS,
         P_ACTIVITIES_TEXT);

    -- Status updates (keep these consistent)
    SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_PLAN_DRAFTED, P_SUBMITTED_BY);

    UPDATE T_AU_IID_COMPLAINT_HDR h
       SET h.STATUS_ID        = 349,
           h.UPDATED_ON       = SYSDATE,
           h.UPDATED_BY_PP_NO = P_SUBMITTED_BY
     WHERE h.COMPLAINT_ID = P_COMPLAINT_ID;

    COMMIT;
  END ADD_INV_PLAN;

  PROCEDURE GET_INV_PLAN(p_complaint_id IN NUMBER, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT PLAN_ID,
             COMPLAINT_ID,
             PLAN_DETAILS,
             SUBMITTED_BY,
             SUBMITTED_ON,
             STATUS,
             PLAN_TITLE,
             trunc(START_DATE) as START_DATE,
             INVESTIGATION_RISK,
             INVESTIGATION_SIZE,
             NO_OF_DAYS,
             TRAVELLING_DAYS,
             TEAM_LEAD,
             TEAM_MEMBERS,
             ACTIVITIES_TEXT
        FROM T_AU_IID_INV_PLAN
       WHERE COMPLAINT_ID = p_complaint_id;
  END GET_INV_PLAN;

  PROCEDURE GET_IID_TASK_LIST(P_UNIT_ID IN NUMBER, IO_CURSOR OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM AS SOURCE,
             c.complainant_name,
             ad.approved_on as ASSIGNED_ON,
             h.STATUS,
             h.SUBMITTED_ON,
             h.ASSIGNED_UNIT_ID AS ASSIGNED_TO,
             a.assigned_unit_id,
             p.plan_id AS PLAN_ID,
             (SELECT MAX(r.REPORT_ID)
                FROM T_AU_IID_REPORT r
               WHERE r.COMPLAINT_ID = h.COMPLAINT_ID) AS REPORT_ID
        FROM T_AU_IID_COMPLAINT_HDR h
       inner JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       inner join T_AU_IID_COMPLAINANT c
          on c.complaint_id = d.complaint_id
         and c.is_primary = 'Y'
       inner join t_au_iid_assessment a
          on a.complaint_id = d.complaint_id
       inner join t_au_iid_head_review ad
          on ad.complaint_id = d.complaint_id
       inner join T_AU_IID_INV_PLAN p
          on p.complaint_id = h.complaint_id
       inner join t_au_iid_head_plan_approval ap
          on ap.plan_id = p.plan_id
       WHERE a.assigned_unit_id = P_UNIT_ID
         and h.status_id = 409
      --113191

       ORDER BY h.UPDATED_ON DESC NULLS LAST, h.SUBMITTED_ON DESC;
  END GET_IID_TASK_LIST;

  ------------------------------------------------------------------
  -- PLAN APPROVAL
  ------------------------------------------------------------------
  PROCEDURE ADD_PLAN_APPROVAL(P_PLAN_ID         IN NUMBER,
                              P_APPROVED_BY     IN NUMBER,
                              P_IS_APPROVED     IN VARCHAR2,
                              P_EDITED_PLAN     IN CLOB,
                              P_FURTHER_ACTIONS IN CLOB,
                              O_APPROVAL_ID     OUT NUMBER) IS
    L_COMPLAINT_ID   NUMBER;
    L_EXIST_APPROVAL NUMBER;
    L_IS_APPROVED    VARCHAR2(50);
  BEGIN
    L_IS_APPROVED := UPPER(TRIM(P_IS_APPROVED));

    -- Find existing approval record for this plan (latest)
    BEGIN
      SELECT MAX(APPROVAL_ID)
        INTO L_EXIST_APPROVAL
        FROM T_AU_IID_HEAD_PLAN_APPROVAL
       WHERE PLAN_ID = P_PLAN_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_EXIST_APPROVAL := NULL;
    END;

    IF L_EXIST_APPROVAL IS NULL THEN
      SELECT SEQ_AU_IID_PLAN_APPROVAL_ID.NEXTVAL
        INTO O_APPROVAL_ID
        FROM DUAL;
    ELSE
      O_APPROVAL_ID := L_EXIST_APPROVAL;
    END IF;

    MERGE INTO T_AU_IID_HEAD_PLAN_APPROVAL t
    USING (SELECT O_APPROVAL_ID AS APPROVAL_ID, P_PLAN_ID AS PLAN_ID
             FROM DUAL) s
    ON (t.APPROVAL_ID = s.APPROVAL_ID AND t.PLAN_ID = s.PLAN_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.APPROVED_BY     = P_APPROVED_BY,
             t.IS_APPROVED     = P_IS_APPROVED,
             t.EDITED_PLAN     = P_EDITED_PLAN,
             t.FURTHER_ACTIONS = P_FURTHER_ACTIONS,
             t.APPROVED_ON     = SYSDATE
    WHEN NOT MATCHED THEN
      INSERT
        (APPROVAL_ID,
         PLAN_ID,
         APPROVED_BY,
         IS_APPROVED,
         EDITED_PLAN,
         FURTHER_ACTIONS,
         APPROVED_ON)
      VALUES
        (O_APPROVAL_ID,
         P_PLAN_ID,
         P_APPROVED_BY,
         P_IS_APPROVED,
         P_EDITED_PLAN,
         P_FURTHER_ACTIONS,
         SYSDATE);

    -- Get complaint id
    SELECT COMPLAINT_ID
      INTO L_COMPLAINT_ID
      FROM T_AU_IID_INV_PLAN
     WHERE PLAN_ID = P_PLAN_ID;

    -- Status updates
    IF L_IS_APPROVED IN ('Y', 'YES', 'APPROVE', 'APPROVED') THEN
      SET_CASE_STATUS(L_COMPLAINT_ID,
                      C_STATUS_PLAN_APPROVED,
                      P_APPROVED_BY);

      UPDATE T_AU_IID_COMPLAINT_HDR h
         SET h.STATUS_ID        = 409,
             h.UPDATED_ON       = SYSDATE,
             h.UPDATED_BY_PP_NO = P_APPROVED_BY
       WHERE h.COMPLAINT_ID = L_COMPLAINT_ID;
    ELSE
      SET_CASE_STATUS(L_COMPLAINT_ID, C_STATUS_PLAN_DRAFTED, P_APPROVED_BY);
    END IF;

    COMMIT;
  END ADD_PLAN_APPROVAL;

  ------------------------------------------------------------------
  -- INQUIRY REPORT
  ------------------------------------------------------------------
  PROCEDURE ADD_INQUIRY_REPORT(P_COMPLAINT_ID                  IN NUMBER,
                               P_NAME_COMPLAINANT              IN VARCHAR2,
                               P_NAME_ACCUSED                  IN VARCHAR2,
                               P_GIST                          IN CLOB,
                               P_PROCEEDINGS                   IN CLOB,
                               P_FINDINGS                      IN CLOB,
                               P_RECOMMENDATION                IN CLOB,
                               P_CONCLUSION                    IN CLOB,
                               P_REPORTED_IN_AUDIT_REPORT      IN VARCHAR2,
                               P_AUDIT_REPORT_REFERENCE_DETAIL IN CLOB,
                               P_UPLOADED_REPORT               IN VARCHAR2,
                               P_UPLOADED_EVIDENCE             IN VARCHAR2,
                               P_UPLOADED_DSA                  IN VARCHAR2,
                               P_SUBMITTED_ON                  IN DATE,
                               P_SUBMITTED_BY                  IN NUMBER,
                               O_REPORT_ID                     OUT NUMBER) IS
  BEGIN
    SELECT SEQ_AU_IID_REPORT_ID.NEXTVAL INTO O_REPORT_ID FROM DUAL;

    INSERT INTO T_AU_IID_REPORT
      (REPORT_ID,
       COMPLAINT_ID,
       NAME_COMPLAINANT,
       NAME_ACCUSED,
       GIST,
       PROCEEDINGS,
       FINDINGS,
       RECOMMENDATION,
       CONCLUSION,
       REPORTED_IN_AUDIT_REPORT,
       AUDIT_REPORT_REFERENCE_DETAIL,
       UPLOADED_REPORT,
       UPLOADED_EVIDENCE,
       UPLOADED_DSA,
       SUBMITTED_ON,
       SUBMITTED_BY)
    VALUES
      (O_REPORT_ID,
       P_COMPLAINT_ID,
       P_NAME_COMPLAINANT,
       P_NAME_ACCUSED,
       P_GIST,
       P_PROCEEDINGS,
       P_FINDINGS,
       P_RECOMMENDATION,
       P_CONCLUSION,
       P_REPORTED_IN_AUDIT_REPORT,
       P_AUDIT_REPORT_REFERENCE_DETAIL,
       P_UPLOADED_REPORT,
       P_UPLOADED_EVIDENCE,
       P_UPLOADED_DSA,
       NVL(P_SUBMITTED_ON, SYSDATE),
       P_SUBMITTED_BY);

    SET_CASE_STATUS(P_COMPLAINT_ID, C_STATUS_REPORT_DRAFT, P_SUBMITTED_BY);
  END ADD_INQUIRY_REPORT;

  PROCEDURE GET_INQUIRY_REPORT(P_REPORT_ID IN NUMBER,
                               IO_CURSOR   OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT REPORT_ID,
             CONCLUSION,
             REPORTED_IN_AUDIT_REPORT,
             AUDIT_REPORT_REFERENCE_DETAIL,
             UPLOADED_REPORT,
             UPLOADED_EVIDENCE,
             NVL(UPLOADED_DSA, '') AS UPLOADED_DSA
        FROM T_AU_IID_REPORT
       WHERE REPORT_ID = P_REPORT_ID;
  END GET_INQUIRY_REPORT;

  ------------------------------------------------------------------
  -- ANALYSIS
  ------------------------------------------------------------------
  PROCEDURE ADD_ANALYSIS(P_REPORT_ID             IN NUMBER,
                         P_POLICY_GAPS           IN CLOB,
                         P_CONTROL_GAPS          IN CLOB,
                         P_PROCEDURAL_VIOLATIONS IN CLOB,
                         P_FORWARD_TO            IN VARCHAR2,
                         P_COMMENTS              IN CLOB,
                         P_DECISION              IN VARCHAR2,
                         P_REFER_BACK_COMMENTS   IN CLOB,
                         P_ANALYZED_BY           IN NUMBER,
                         O_ANALYSIS_ID           OUT NUMBER) IS
    L_COMPLAINT_ID   NUMBER;
    L_EXIST_ANALYSIS NUMBER;
  BEGIN
    -- Find existing analysis for this report (pick latest)
    BEGIN
      SELECT MAX(ANALYSIS_ID)
        INTO L_EXIST_ANALYSIS
        FROM T_AU_IID_ANALYSIS
       WHERE REPORT_ID = P_REPORT_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        L_EXIST_ANALYSIS := NULL;
    END;

    IF L_EXIST_ANALYSIS IS NULL THEN
      SELECT SEQ_AU_IID_ANALYSIS_ID.NEXTVAL INTO O_ANALYSIS_ID FROM DUAL;
    ELSE
      O_ANALYSIS_ID := L_EXIST_ANALYSIS;
    END IF;

    MERGE INTO T_AU_IID_ANALYSIS t
    USING (SELECT P_REPORT_ID AS REPORT_ID, O_ANALYSIS_ID AS ANALYSIS_ID
             FROM DUAL) s
    ON (t.ANALYSIS_ID = s.ANALYSIS_ID AND t.REPORT_ID = s.REPORT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.POLICY_GAPS           = P_POLICY_GAPS,
             t.CONTROL_GAPS          = P_CONTROL_GAPS,
             t.PROCEDURAL_VIOLATIONS = P_PROCEDURAL_VIOLATIONS,
             t.FORWARD_TO            = P_FORWARD_TO,
             t.COMMENTS              = P_COMMENTS,
             t.DECISION              = P_DECISION,
             t.REFER_BACK_COMMENTS   = P_REFER_BACK_COMMENTS,
             t.ANALYZED_BY           = P_ANALYZED_BY,
             t.ANALYZED_ON           = SYSDATE
    WHEN NOT MATCHED THEN
      INSERT
        (ANALYSIS_ID,
         REPORT_ID,
         POLICY_GAPS,
         CONTROL_GAPS,
         PROCEDURAL_VIOLATIONS,
         FORWARD_TO,
         COMMENTS,
         DECISION,
         REFER_BACK_COMMENTS,
         ANALYZED_BY,
         ANALYZED_ON)
      VALUES
        (O_ANALYSIS_ID,
         P_REPORT_ID,
         P_POLICY_GAPS,
         P_CONTROL_GAPS,
         P_PROCEDURAL_VIOLATIONS,
         P_FORWARD_TO,
         P_COMMENTS,
         P_DECISION,
         P_REFER_BACK_COMMENTS,
         P_ANALYZED_BY,
         SYSDATE);

    -- Derive complaint id for status update
    SELECT COMPLAINT_ID
      INTO L_COMPLAINT_ID
      FROM T_AU_IID_REPORT
     WHERE REPORT_ID = P_REPORT_ID;

    IF UPPER(P_DECISION) LIKE '%REFER%' THEN
      SET_CASE_STATUS(L_COMPLAINT_ID, C_STATUS_REPORT_DRAFT, P_ANALYZED_BY);
    ELSE
      SET_CASE_STATUS(L_COMPLAINT_ID,
                      C_STATUS_FINAL_APPROVAL,
                      P_ANALYZED_BY);
    END IF;

    COMMIT;
  END ADD_ANALYSIS;

  ------------------------------------------------------------------
  -- FINAL APPROVAL
  ------------------------------------------------------------------
  PROCEDURE ADD_FINAL_APPROVAL(P_REPORT_ID         IN NUMBER,
                               P_COMMENTS          IN CLOB,
                               P_APPROVED          IN VARCHAR2,
                               P_APPROVED_BY       IN NUMBER,
                               O_FINAL_APPROVAL_ID OUT NUMBER) IS
    L_COMPLAINT_ID NUMBER;
  BEGIN
    SELECT SEQ_AU_IID_FINAL_APPROVAL_ID.NEXTVAL
      INTO O_FINAL_APPROVAL_ID
      FROM DUAL;

    INSERT INTO T_AU_IID_FINAL_APPROVAL
      (FINAL_APPROVAL_ID,
       REPORT_ID,
       COMMENTS,
       APPROVED,
       APPROVED_BY,
       APPROVED_ON)
    VALUES
      (O_FINAL_APPROVAL_ID,
       P_REPORT_ID,
       P_COMMENTS,
       P_APPROVED,
       P_APPROVED_BY,
       SYSDATE);

    SELECT COMPLAINT_ID
      INTO L_COMPLAINT_ID
      FROM T_AU_IID_REPORT
     WHERE REPORT_ID = P_REPORT_ID;

    IF UPPER(P_APPROVED) IN ('Y', 'YES', 'APPROVE', 'APPROVED') THEN
      SET_CASE_STATUS(L_COMPLAINT_ID, C_STATUS_CLOSED, P_APPROVED_BY);
    ELSE
      SET_CASE_STATUS(L_COMPLAINT_ID,
                      C_STATUS_FINAL_APPROVAL,
                      P_APPROVED_BY);
    END IF;
  END ADD_FINAL_APPROVAL;

  ------------------------------------------------------------------
  -- CASE STUDY
  ------------------------------------------------------------------
  PROCEDURE ADD_CASE_STUDY(P_COMPLAINT_ID           IN NUMBER,
                           P_ORIGIN_PROCESS_OWNER   IN VARCHAR2,
                           P_NAME_COMPLAINANT       IN VARCHAR2,
                           P_BRANCH                 IN VARCHAR2,
                           P_GIST                   IN CLOB,
                           P_OUTCOME                IN CLOB,
                           P_MODUS_OPERANDI         IN CLOB,
                           P_GAPS                   IN CLOB,
                           P_ROOT_CAUSE             IN CLOB,
                           P_ACTIONS_REC            IN CLOB,
                           P_STATUS                 IN VARCHAR2,
                           P_POLICY_GAPS_IDENTIFIED IN CLOB,
                           P_CONTROL_VIOLATIONS     IN CLOB,
                           P_RISK_IDENTIFIED        IN CLOB,
                           P_REG_COMPLIANCE_FAILURE IN CLOB,
                           O_CASE_ID                OUT NUMBER) IS
  BEGIN
    SELECT SEQ_AU_IID_CASE_STUDY_ID.NEXTVAL INTO O_CASE_ID FROM DUAL;

    INSERT INTO T_AU_IID_CASE_STUDY
      (CASE_ID,
       COMPLAINT_ID,
       ORIGIN_PROCESS_OWNER,
       NAME_COMPLAINANT,
       BRANCH,
       GIST,
       OUTCOME,
       MODUS_OPERANDI,
       GAPS,
       ROOT_CAUSE,
       ACTIONS_REC,
       STATUS,
       POLICY_GAPS_IDENTIFIED,
       CONTROL_VIOLATIONS,
       RISK_IDENTIFIED,
       REG_COMPLIANCE_FAILURE)
    VALUES
      (O_CASE_ID,
       P_COMPLAINT_ID,
       P_ORIGIN_PROCESS_OWNER,
       P_NAME_COMPLAINANT,
       P_BRANCH,
       P_GIST,
       P_OUTCOME,
       P_MODUS_OPERANDI,
       P_GAPS,
       P_ROOT_CAUSE,
       P_ACTIONS_REC,
       P_STATUS,
       P_POLICY_GAPS_IDENTIFIED,
       P_CONTROL_VIOLATIONS,
       P_RISK_IDENTIFIED,
       P_REG_COMPLIANCE_FAILURE);
  END ADD_CASE_STUDY;

  ------------------------------------------------------------------
  -- REPORTS FILTERING
  ------------------------------------------------------------------
  PROCEDURE GET_REPORTS(P_FILTER          IN VARCHAR2,
                        P_SOURCE          IN VARCHAR2,
                        P_CATEGORY        IN VARCHAR2,
                        P_PERTAINS_TO     IN VARCHAR2,
                        P_DATE_FROM       IN VARCHAR2,
                        P_DATE_TO         IN VARCHAR2,
                        P_REGION_ID       IN NUMBER,
                        P_BRANCH_ID       IN NUMBER,
                        P_HO_UNIT_TYPE_ID IN NUMBER,
                        P_HO_UNIT_ID      IN NUMBER,
                        P_STATUS          IN VARCHAR2,
                        IO_CURSOR         OUT T_CURSOR) IS
    L_DATE_FROM DATE;
    L_DATE_TO   DATE;
  BEGIN
    BEGIN
      L_DATE_FROM := CASE
                       WHEN P_DATE_FROM IS NULL OR TRIM(P_DATE_FROM) = '' THEN
                        NULL
                       ELSE
                        TO_DATE(P_DATE_FROM, 'YYYY-MM-DD')
                     END;
    EXCEPTION
      WHEN OTHERS THEN
        L_DATE_FROM := NULL;
    END;

    BEGIN
      L_DATE_TO := CASE
                     WHEN P_DATE_TO IS NULL OR TRIM(P_DATE_TO) = '' THEN
                      NULL
                     ELSE
                      TO_DATE(P_DATE_TO, 'YYYY-MM-DD')
                   END;
    EXCEPTION
      WHEN OTHERS THEN
        L_DATE_TO := NULL;
    END;

    OPEN IO_CURSOR FOR
      SELECT h.COMPLAINT_ID,
             h.COMPLAINT_NO,
             d.NATURE,
             d.CATEGORY,
             d.RECEIVED_FROM AS SOURCE,
             h.STATUS,
             h.SUBMITTED_ON,
             d.REGION_ID,
             d.BRANCH_ID,
             d.GM_OFFICE_ID AS HO_UNIT_ID,
             (SELECT MAX(r.REPORT_ID)
                FROM T_AU_IID_REPORT r
               WHERE r.COMPLAINT_ID = h.COMPLAINT_ID) AS REPORT_ID
        FROM T_AU_IID_COMPLAINT_HDR h
        LEFT JOIN T_AU_IID_COMPLAINT_IAID d
          ON d.COMPLAINT_ID = h.COMPLAINT_ID
       WHERE h.ACTIVE_FLAG = 'Y'
         AND (P_FILTER IS NULL OR TRIM(P_FILTER) = '' OR
             d.NATURE = P_FILTER)
         AND (P_SOURCE IS NULL OR TRIM(P_SOURCE) = '' OR
             d.RECEIVED_FROM = P_SOURCE)
         AND (P_CATEGORY IS NULL OR TRIM(P_CATEGORY) = '' OR
             d.CATEGORY = P_CATEGORY)
         AND (P_STATUS IS NULL OR TRIM(P_STATUS) = '' OR
             h.STATUS = P_STATUS)
         AND (P_REGION_ID IS NULL OR P_REGION_ID = 0 OR
             d.REGION_ID = P_REGION_ID)
         AND (P_BRANCH_ID IS NULL OR P_BRANCH_ID = 0 OR
             d.BRANCH_ID = P_BRANCH_ID)
         AND (P_HO_UNIT_ID IS NULL OR P_HO_UNIT_ID = 0 OR
             d.GM_OFFICE_ID = P_HO_UNIT_ID)
         AND (L_DATE_FROM IS NULL OR h.SUBMITTED_ON >= L_DATE_FROM)
         AND (L_DATE_TO IS NULL OR h.SUBMITTED_ON < (L_DATE_TO + 1))
       ORDER BY h.SUBMITTED_ON DESC;
  END GET_REPORTS;

  PROCEDURE GET_EMPLOYEE_INFO(P_PP_NO in number, io_cursor OUT t_cursor) is

  Begin
    open io_cursor for
      select e.ppno, e.ename, e.fathername, e.CNIC
        from v_get_iid_employee_info e
       where e.ppno = P_PP_NO;

  end;

  PROCEDURE P_RESULT_OK(io_cursor OUT t_cursor,
                        p_message IN VARCHAR2,
                        p_id      IN NUMBER DEFAULT NULL) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT 'Y' AS ok, p_message AS message, p_id AS id FROM dual;
  END;

  PROCEDURE P_RESULT_FAIL(io_cursor OUT t_cursor, p_message IN VARCHAR2) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT 'N' AS ok, p_message AS message, CAST(NULL AS NUMBER) AS id
        FROM dual;
  END;

  ----------------------------------------------------------------------
  -- ACCUSATIONS
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_ACCUSATIONS(p_complaint_id IN NUMBER,
                                  io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT a.accusation_id,
             a.complaint_id,
             a.accusation_text,
             a.sort_order,
             a.status,
             a.created_by,
             a.created_on,
             a.updated_by,
             a.updated_on
        FROM t_au_iid_inq_accusations a
       WHERE a.complaint_id = p_complaint_id
         AND a.status = 'ACTIVE'
       ORDER BY a.sort_order, a.accusation_id;
  END;

  PROCEDURE P_ADD_INQ_ACCUSATION(p_complaint_id    IN NUMBER,
                                 p_accusation_text IN CLOB,
                                 p_sort_order      IN NUMBER,
                                 p_created_by      IN NUMBER,
                                 io_cursor         OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_accusations
      (complaint_id,
       accusation_text,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_accusation_text,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING accusation_id INTO v_id;

    P_RESULT_OK(io_cursor, 'Accusation added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add accusation: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_ACCUSATION(p_accusation_id   IN NUMBER,
                                    p_accusation_text IN CLOB,
                                    p_sort_order      IN NUMBER,
                                    p_updated_by      IN NUMBER,
                                    io_cursor         OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_accusations
       SET accusation_text = p_accusation_text,
           sort_order      = NVL(p_sort_order, sort_order),
           updated_by      = p_updated_by,
           updated_on      = SYSDATE
     WHERE accusation_id = p_accusation_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active accusation found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Accusation updated.', p_accusation_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update accusation: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_ACCUSATION(p_accusation_id IN NUMBER,
                                    p_updated_by    IN NUMBER,
                                    io_cursor       OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_accusations
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE accusation_id = p_accusation_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active accusation found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Accusation deleted.', p_accusation_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete accusation: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- ACCUSED LIST
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_ACCUSED_LIST(p_complaint_id IN NUMBER,
                                   io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT x.accused_row_id,
             x.complaint_id,
             x.person_name,
             x.designation,
             x.role_type,
             x.ppno_number,
             x.cnic,
             x.FATHER_NAME,
             x.remarks,
             x.sort_order,
             x.status,
             x.created_by,
             x.created_on,
             x.updated_by,
             x.updated_on
        FROM t_au_iid_inq_accused_list x
       WHERE x.complaint_id = p_complaint_id
         AND x.status = 'ACTIVE'
       ORDER BY x.sort_order, x.accused_row_id;
  END;

  PROCEDURE P_ADD_INQ_ACCUSED(p_complaint_id IN NUMBER,
                              p_person_name  IN VARCHAR2,
                              p_designation  IN VARCHAR2,
                              p_role_type    IN VARCHAR2,
                              p_ppno_number  IN VARCHAR2,
                              p_cnic         IN VARCHAR2,
                              p_Father_name  IN VARCHAR2,
                              p_remarks      IN VARCHAR2,
                              p_sort_order   IN NUMBER,
                              p_created_by   IN NUMBER,
                              io_cursor      OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_accused_list
      (complaint_id,
       person_name,
       designation,
       role_type,
       ppno_number,
       cnic,
       FATHER_NAME,
       remarks,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_person_name,
       p_designation,
       upper(p_role_type),
       p_ppno_number,
       p_cnic,
       p_Father_name,
       p_remarks,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING accused_row_id INTO v_id;

    P_RESULT_OK(io_cursor, 'Accused row added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add accused row: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_ACCUSED(p_accused_row_id IN NUMBER,
                                 p_person_name    IN VARCHAR2,
                                 p_designation    IN VARCHAR2,
                                 p_role_type      IN VARCHAR2,
                                 p_ppno_number    IN VARCHAR2,
                                 p_cnic           IN VARCHAR2,
                                 p_FATHER_NAME    IN VARCHAR2,
                                 p_remarks        IN VARCHAR2,
                                 p_sort_order     IN NUMBER,
                                 p_updated_by     IN NUMBER,
                                 io_cursor        OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_accused_list
       SET person_name = p_person_name,
           designation = p_designation,
           role_type   = p_role_type,
           ppno_number = p_ppno_number,
           cnic        = p_cnic,
           FATHER_NAME = p_FATHER_NAME,
           remarks     = p_remarks,
           sort_order  = NVL(p_sort_order, sort_order),
           updated_by  = p_updated_by,
           updated_on  = SYSDATE
     WHERE accused_row_id = p_accused_row_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active accused row found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Accused row updated.', p_accused_row_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update accused row: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_ACCUSED(p_accused_row_id IN NUMBER,
                                 p_updated_by     IN NUMBER,
                                 io_cursor        OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_accused_list
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE accused_row_id = p_accused_row_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active accused row found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Accused row deleted.', p_accused_row_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete accused row: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- RECORD SCRUTINIZED
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_RECORDS(p_complaint_id IN NUMBER,
                              io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT r.rec_id,
             r.complaint_id,
             r.record_title,
             r.record_details,
             r.sort_order,
             r.status,
             r.created_by,
             r.created_on,
             r.updated_by,
             r.updated_on
        FROM t_au_iid_inq_record_scrutinized r
       WHERE r.complaint_id = p_complaint_id
         AND r.status = 'ACTIVE'
       ORDER BY r.sort_order, r.rec_id;
  END;

  PROCEDURE P_ADD_INQ_RECORD(p_complaint_id   IN NUMBER,
                             p_record_title   IN VARCHAR2,
                             p_record_details IN VARCHAR2,
                             p_sort_order     IN NUMBER,
                             p_created_by     IN NUMBER,
                             io_cursor        OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_record_scrutinized
      (complaint_id,
       record_title,
       record_details,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_record_title,
       p_record_details,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING rec_id INTO v_id;

    P_RESULT_OK(io_cursor, 'Record added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add record: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_RECORD(p_rec_id         IN NUMBER,
                                p_record_title   IN VARCHAR2,
                                p_record_details IN VARCHAR2,
                                p_sort_order     IN NUMBER,
                                p_updated_by     IN NUMBER,
                                io_cursor        OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_record_scrutinized
       SET record_title   = p_record_title,
           record_details = p_record_details,
           sort_order     = NVL(p_sort_order, sort_order),
           updated_by     = p_updated_by,
           updated_on     = SYSDATE
     WHERE rec_id = p_rec_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active record found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Record updated.', p_rec_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update record: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_RECORD(p_rec_id     IN NUMBER,
                                p_updated_by IN NUMBER,
                                io_cursor    OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_record_scrutinized
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE rec_id = p_rec_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active record found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Record deleted.', p_rec_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete record: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- STATEMENTS REGISTER
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_STATEMENTS(p_complaint_id IN NUMBER,
                                 io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT s.statement_id,
             s.complaint_id,
             s.person_name,
             s.role_type,
             s.ppno_number,
             s.cnic,
             s.statement_datetime,
             s.place,
             s.mode_type,
             s.key_points,
             s.status,
             s.created_by,
             s.created_on,
             s.updated_by,
             s.updated_on
        FROM t_au_iid_inq_statements s
       WHERE s.complaint_id = p_complaint_id
         AND s.status = 'ACTIVE'
       ORDER BY NVL(s.statement_datetime, DATE '1900-01-01'),
                s.statement_id;
  END;

  PROCEDURE P_ADD_INQ_STATEMENT(p_complaint_id       IN NUMBER,
                                p_person_name        IN VARCHAR2,
                                p_role_type          IN VARCHAR2,
                                p_ppno_number        IN VARCHAR2,
                                p_cnic               IN VARCHAR2,
                                p_statement_datetime IN DATE,
                                p_place              IN VARCHAR2,
                                p_mode_type          IN VARCHAR2,
                                p_key_points         IN CLOB,
                                P_UPLOADED_STATEMENT in Clob,
                                P_USER_ID            IN NUMBER,
                                io_cursor            OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    v_id := NULL;

    ------------------------------------------------------------------
    -- 1) If PPNO is available: update existing row for (complaint + ppno)
    ------------------------------------------------------------------
    IF p_ppno_number IS NOT NULL THEN

      UPDATE t_au_iid_inq_statements s
         SET s.person_name        = p_person_name,
             s.role_type          = p_role_type,
             s.cnic               = p_cnic,
             s.statement_datetime = p_statement_datetime,
             s.place              = p_place,
             s.mode_type          = p_mode_type,
             s.key_points         = p_key_points,
             s.uploaded_statement = P_UPLOADED_STATEMENT
       WHERE s.complaint_id = p_complaint_id
         AND s.ppno_number = p_ppno_number
         AND s.status = 'ACTIVE';

      IF SQL%ROWCOUNT > 0 THEN
        SELECT s.statement_id
          INTO v_id
          FROM t_au_iid_inq_statements s
         WHERE s.complaint_id = p_complaint_id
           AND s.ppno_number = p_ppno_number
           AND s.status = 'ACTIVE';

        P_RESULT_OK(io_cursor, 'Statement updated.', v_id);
        RETURN;
      END IF;

      ------------------------------------------------------------------
      -- 2) Else fallback to CNIC: update existing row for (complaint + cnic)
      ------------------------------------------------------------------
    ELSIF p_cnic IS NOT NULL THEN

      UPDATE t_au_iid_inq_statements s
         SET s.person_name        = p_person_name,
             s.role_type          = p_role_type,
             s.ppno_number        = p_ppno_number,
             s.statement_datetime = p_statement_datetime,
             s.place              = p_place,
             s.mode_type          = p_mode_type,
             s.key_points         = p_key_points,
             s.uploaded_statement = P_UPLOADED_STATEMENT
       WHERE s.complaint_id = p_complaint_id
         AND s.cnic = p_cnic
         AND s.status = 'ACTIVE';

      IF SQL%ROWCOUNT > 0 THEN
        SELECT s.statement_id
          INTO v_id
          FROM t_au_iid_inq_statements s
         WHERE s.complaint_id = p_complaint_id
           AND s.cnic = p_cnic
           AND s.status = 'ACTIVE';

        P_RESULT_OK(io_cursor, 'Statement updated.', v_id);
        RETURN;
      END IF;

    END IF;

    ------------------------------------------------------------------
    -- 3) If no existing row found, INSERT new
    ------------------------------------------------------------------
    INSERT INTO t_au_iid_inq_statements
      (statement_id,
       complaint_id,
       person_name,
       role_type,
       ppno_number,
       cnic,
       statement_datetime,
       place,
       mode_type,
       key_points,
       status,
       created_by,
       created_on)
    VALUES
      (seq_iid_inq_statements.nextval,
       p_complaint_id,
       p_person_name,
       upper(p_role_type),
       p_ppno_number,
       p_cnic,
       p_statement_datetime,
       p_place,
       p_mode_type,
       p_key_points,
       'ACTIVE',
       P_USER_ID,
       SYSDATE)
    RETURNING statement_id INTO v_id;

    P_RESULT_OK(io_cursor, 'Statement added.', v_id);

  EXCEPTION
    WHEN TOO_MANY_ROWS THEN
      P_RESULT_FAIL(io_cursor,
                    'Duplicate statement exists for this complaint/person. Please clean duplicates.');
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to add/update statement: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_STATEMENT(p_statement_id       IN NUMBER,
                                   p_person_name        IN VARCHAR2,
                                   p_role_type          IN VARCHAR2,
                                   p_ppno_number        IN VARCHAR2,
                                   p_cnic               IN VARCHAR2,
                                   p_statement_datetime IN DATE,
                                   p_place              IN VARCHAR2,
                                   p_mode_type          IN VARCHAR2,
                                   p_key_points         IN CLOB,
                                   P_UPLOADED_STATEMENT in CLOB,
                                   p_updated_by         IN NUMBER,
                                   io_cursor            OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_statements
       SET person_name        = p_person_name,
           role_type          = p_role_type,
           ppno_number        = p_ppno_number,
           cnic               = p_cnic,
           statement_datetime = p_statement_datetime,
           place              = p_place,
           mode_type          = p_mode_type,
           key_points         = p_key_points,
           uploaded_statement = P_UPLOADED_STATEMENT,
           updated_by         = p_updated_by,
           updated_on         = SYSDATE
     WHERE statement_id = p_statement_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active statement found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Statement updated.', p_statement_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update statement: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_STATEMENT(p_statement_id IN NUMBER,
                                   p_updated_by   IN NUMBER,
                                   io_cursor      OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_statements
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE statement_id = p_statement_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active statement found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Statement deleted.', p_statement_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete statement: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- EVIDENCE FILES
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_EVIDENCE_FILES(p_complaint_id IN NUMBER,
                                     io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT e.evidence_id,
             e.complaint_id,
             e.evidence_type,
             e.description,
             e.file_name,
             e.file_path,
             e.file_ext,
             e.file_size_kb,
             e.status,
             e.uploaded_by,
             e.uploaded_on,
             e.updated_by,
             e.updated_on
        FROM t_au_iid_inq_evidence_files e
       WHERE e.complaint_id = p_complaint_id
         AND e.status = 'ACTIVE'
       ORDER BY e.uploaded_on DESC, e.evidence_id DESC;
  END;

  PROCEDURE P_ADD_INQ_EVIDENCE_FILE(p_complaint_id  IN NUMBER,
                                    p_evidence_type IN VARCHAR2,
                                    p_description   IN VARCHAR2,
                                    p_file_name     IN VARCHAR2,
                                    p_file_path     IN VARCHAR2,
                                    p_file_ext      IN VARCHAR2,
                                    p_file_size_kb  IN NUMBER,
                                    p_uploaded_by   IN NUMBER,
                                    io_cursor       OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_evidence_files
      (complaint_id,
       evidence_type,
       description,
       file_name,
       file_path,
       file_ext,
       file_size_kb,
       status,
       uploaded_by,
       uploaded_on)
    VALUES
      (p_complaint_id,
       p_evidence_type,
       p_description,
       p_file_name,
       p_file_path,
       p_file_ext,
       p_file_size_kb,
       'ACTIVE',
       p_uploaded_by,
       SYSDATE)
    RETURNING evidence_id INTO v_id;

    P_RESULT_OK(io_cursor, 'Evidence file added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add evidence file: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_EVIDENCE_FILE(p_evidence_id IN NUMBER,
                                       p_updated_by  IN NUMBER,
                                       io_cursor     OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_evidence_files
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE evidence_id = p_evidence_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active evidence file found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Evidence file deleted.', p_evidence_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to delete evidence file: ' || SQLERRM);
  END;

  ----------------------------------------------------------------------
  -- VIOLATIONS
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_VIOLATIONS(p_complaint_id IN NUMBER,
                                 io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT v.violation_id,
             v.complaint_id,
             v.category,
             v.violation_detail,
             v.reference_text,
             v.recommendation,
             v.sort_order,
             v.status,
             v.created_by,
             v.created_on,
             v.updated_by,
             v.updated_on
        FROM t_au_iid_inq_violations v
       WHERE v.complaint_id = p_complaint_id
         AND v.status = 'ACTIVE'
       ORDER BY v.category, v.sort_order, v.violation_id;
  END;

  PROCEDURE P_GET_INQ_VIOLATION_STEP(P_COMPLAINT_ID IN NUMBER,
                                     IO_CURSOR      OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT COMPLAINT_ID,
             CONCLUSION,
             REPORTED_IN_AUDIT_REPORT,
             AUDIT_REPORT_REFERENCE_DETAIL,
             STATUS,
             CREATED_BY,
             CREATED_ON,
             UPDATED_BY,
             UPDATED_ON
        FROM T_AU_IID_INQ_VIOLATION_STEP
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND NVL(STATUS, 'A') = 'A';
  END P_GET_INQ_VIOLATION_STEP;

  PROCEDURE P_SAVE_INQ_VIOLATION_STEP(P_COMPLAINT_ID                  IN NUMBER,
                                      P_CONCLUSION                    IN CLOB,
                                      P_REPORTED_IN_AUDIT_REPORT      IN VARCHAR2,
                                      P_AUDIT_REPORT_REFERENCE_DETAIL IN CLOB,
                                      P_UPDATED_BY                    IN NUMBER,
                                      IO_CURSOR                       OUT T_CURSOR) IS
  BEGIN
    MERGE INTO T_AU_IID_INQ_VIOLATION_STEP t
    USING (SELECT P_COMPLAINT_ID AS COMPLAINT_ID FROM dual) s
    ON (t.COMPLAINT_ID = s.COMPLAINT_ID)
    WHEN MATCHED THEN
      UPDATE
         SET t.CONCLUSION                    = P_CONCLUSION,
             t.REPORTED_IN_AUDIT_REPORT      = CASE
                                                 WHEN UPPER(TRIM(P_REPORTED_IN_AUDIT_REPORT)) IN ('Y', 'YES') THEN
                                                  'Y'
                                                 ELSE
                                                  'N'
                                               END,
             t.AUDIT_REPORT_REFERENCE_DETAIL = CASE
                                                 WHEN UPPER(TRIM(P_REPORTED_IN_AUDIT_REPORT)) IN ('Y', 'YES') THEN
                                                  P_AUDIT_REPORT_REFERENCE_DETAIL
                                                 ELSE
                                                  NULL
                                               END,
             t.STATUS                        = 'A',
             t.UPDATED_BY                    = P_UPDATED_BY,
             t.UPDATED_ON                    = SYSDATE WHEN NOT MATCHED THEN INSERT(COMPLAINT_ID, CONCLUSION, REPORTED_IN_AUDIT_REPORT, AUDIT_REPORT_REFERENCE_DETAIL, STATUS, CREATED_BY, CREATED_ON) VALUES(P_COMPLAINT_ID, P_CONCLUSION,CASE
               WHEN UPPER(TRIM(P_REPORTED_IN_AUDIT_REPORT)) IN ('Y', 'YES') THEN
                'Y'
               ELSE
                'N'
             END,CASE
               WHEN UPPER(TRIM(P_REPORTED_IN_AUDIT_REPORT)) IN ('Y', 'YES') THEN
                P_AUDIT_REPORT_REFERENCE_DETAIL
               ELSE
                NULL
             END, 'A', P_UPDATED_BY, SYSDATE);

    P_RESULT_OK(IO_CURSOR, 'Violation step summary saved.', P_COMPLAINT_ID);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(IO_CURSOR,
                    'Failed to save violation step summary: ' || SQLERRM);
  END P_SAVE_INQ_VIOLATION_STEP;

  PROCEDURE GET_INQ_FIND_RECOMM_STATUS(p_complaint_id IN NUMBER,
                                       io_cursor      OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR

    /* 1) Real accusations for this complaint */
      SELECT a.accusation_id,
             a.accusation_text,
             CASE
               WHEN r.complaint_id IS NOT NULL THEN
                'Y'
               ELSE
                'N'
             END AS is_saved,
             NVL(r.updated_on, r.created_on) AS saved_on
        FROM (SELECT accusation_id, accusation_text
                FROM t_au_iid_inq_accusations -- <-- replace with your actual accusations source
               WHERE complaint_id = p_complaint_id) a
        LEFT JOIN t_au_iid_inq_find_recomm r
          ON r.complaint_id = p_complaint_id
         AND r.accusation_id = a.accusation_id
       ORDER BY a.accusation_id;

    /* Optional: include Additional Charges inside cursor as well.
    If you want it in DB output, use UNION ALL approach below instead.
    Otherwise your C# already inserts it if missing. */

  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to load findings/recommendation status: ' ||
                    SQLERRM);
  END;

  PROCEDURE SAVE_INQ_FINDINGS_RECOMM(p_complaint_id  IN NUMBER,
                                     p_accusation_id IN NUMBER, -- 0 = Additional Charges
                                     p_finding_text  IN CLOB,
                                     p_recom_text    IN CLOB,
                                     p_outcome       in varchar2,
                                     p_ppno          IN VARCHAR2,
                                     io_cursor       OUT t_cursor) IS
  BEGIN
    MERGE INTO T_AU_IID_INQ_FIND_RECOMM t
    USING (SELECT p_complaint_id  AS complaint_id,
                  p_accusation_id AS accusation_id
             FROM dual) s
    ON (t.complaint_id = s.complaint_id AND t.accusation_id = s.accusation_id)
    WHEN MATCHED THEN
      UPDATE
         SET t.findings_text       = p_finding_text,
             t.recommendation_text = p_recom_text,
             t.updated_by          = p_ppno,
             t.updated_on          = SYSDATE,
             t.accusation          = p_outcome
    WHEN NOT MATCHED THEN
      INSERT
        (complaint_id,
         accusation_id,
         accusation,
         findings_text,
         recommendation_text,
         status,
         created_by,
         created_on)
      VALUES
        (p_complaint_id,
         p_accusation_id,
         p_outcome,
         p_finding_text,
         p_recom_text,
         'ACTIVE',
         p_ppno,
         SYSDATE);

    P_RESULT_OK(io_cursor,
                'Findings & recommendation saved.',
                p_complaint_id);

  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to save findings/recommendation: ' || SQLERRM);
  END;

  PROCEDURE GET_INQ_FINDINGS_RECOMM(p_complaint_id  IN NUMBER,
                                    p_accusation_id IN NUMBER,
                                    io_cursor       OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT r.complaint_id,
             r.accusation_id,
             r.findings_text AS finding_text,
             r.recommendation_text AS recom_text,
             r.accusation as OUTCOME,
             NVL(r.updated_by, r.created_by) AS ppno,
             NVL(r.updated_on, r.created_on) AS updated_on
        FROM T_AU_IID_INQ_FIND_RECOMM r
       WHERE r.complaint_id = p_complaint_id;

  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor,
                    'Failed to load findings/recommendation: ' || SQLERRM);
  END;

  PROCEDURE P_ADD_INQ_VIOLATION(p_complaint_id     IN NUMBER,
                                p_category         IN VARCHAR2,
                                p_violation_detail IN CLOB,
                                p_reference_text   IN VARCHAR2,
                                p_recommendation   IN CLOB,
                                p_sort_order       IN NUMBER,
                                p_created_by       IN NUMBER,
                                io_cursor          OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_violations
      (complaint_id,
       category,
       violation_detail,
       reference_text,
       recommendation,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_category,
       p_violation_detail,
       p_reference_text,
       p_recommendation,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING violation_id INTO v_id;

    P_RESULT_OK(io_cursor, 'Violation added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add violation: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_VIOLATION(p_violation_id     IN NUMBER,
                                   p_category         IN VARCHAR2,
                                   p_violation_detail IN CLOB,
                                   p_reference_text   IN VARCHAR2,
                                   p_recommendation   IN CLOB,
                                   p_sort_order       IN NUMBER,
                                   p_updated_by       IN NUMBER,
                                   io_cursor          OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_violations
       SET category         = p_category,
           violation_detail = p_violation_detail,
           reference_text   = p_reference_text,
           recommendation   = p_recommendation,
           sort_order       = NVL(p_sort_order, sort_order),
           updated_by       = p_updated_by,
           updated_on       = SYSDATE
     WHERE violation_id = p_violation_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active violation found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'Violation updated.', p_violation_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update violation: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_VIOLATION(p_violation_id IN NUMBER,
                                   p_updated_by   IN NUMBER,
                                   io_cursor      OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_violations
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE violation_id = p_violation_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active violation found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'Violation deleted.', p_violation_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete violation: ' || SQLERRM);
  END;

  PROCEDURE P_GET_ALLOWED_PDF_ENG_DETAILS(P_PP_NO  IN NUMBER,
                                          P_R_ID   IN NUMBER,
                                          P_ENT_ID IN NUMBER,
                                          O_CURSOR OUT SYS_REFCURSOR) IS
  BEGIN
    /*
      Returns list of ENG_IDs allowed for the logged-in user to generate PDF,
      along with engagement/entity details.

      Roles logic:
      - Admin roles (1,2): all FINAL reports
      - Department Head / Incharge (15,16): FINAL reports for P_ENT_ID
      - Others: FINAL reports where user exists in t_au_audit_team_tasklist
    */

    IF P_R_ID IN (1, 2) THEN
      OPEN O_CURSOR FOR
        SELECT m.eng_id AS ENG_ID,

               /* ===== Replace these entity columns if your IAS uses different entity view/table ===== */
               NVL(e.p_name, '-') AS REPORTING_OFFICE,
               e.c_name AS ENTITY_NAME,
               /* ================================================================================ */

               pe.audit_startdate AS AUDIT_START_DATE,
               pe.audit_enddate   AS AUDIT_END_DATE,
               m.report_status    AS REPORT_STATUS,
               m.report_version   AS REPORT_VERSION
          FROM t_frpt_report_meta m
          JOIN t_au_plan_eng pe
            ON pe.eng_id = m.eng_id

        /* ===== Replace this join with your actual entity master/view if needed ===== */
          LEFT JOIN t_auditee_entities_maping e
            ON e.entity_id = pe.entity_id
        /* ======================================================================== */

         WHERE m.report_status = 'FINAL'
         ORDER BY m.eng_id DESC;

    ELSIF P_R_ID IN (15, 16) THEN
      OPEN O_CURSOR FOR
        SELECT m.eng_id AS ENG_ID,
               NVL(e.p_name, '-') AS REPORTING_OFFICE,
               e.c_name AS ENTITY_NAME,
               pe.audit_startdate AS AUDIT_START_DATE,
               pe.audit_enddate AS AUDIT_END_DATE,
               m.report_status AS REPORT_STATUS,
               m.report_version AS REPORT_VERSION
          FROM t_frpt_report_meta m
          JOIN t_au_plan_eng pe
            ON pe.eng_id = m.eng_id
          LEFT JOIN t_auditee_entities_maping e
            ON e.entity_id = pe.entity_id
         WHERE m.report_status = 'FINAL'
           AND pe.entity_id = P_ENT_ID
         ORDER BY m.eng_id DESC;

    ELSE
      OPEN O_CURSOR FOR
        SELECT DISTINCT m.eng_id AS ENG_ID,
                        NVL(e.p_name, '-') AS REPORTING_OFFICE,
                        e.c_name AS ENTITY_NAME,
                        pe.audit_startdate AS AUDIT_START_DATE,
                        pe.audit_enddate AS AUDIT_END_DATE,
                        m.report_status AS REPORT_STATUS,
                        m.report_version AS REPORT_VERSION
          FROM t_frpt_report_meta m
          JOIN t_au_plan_eng pe
            ON pe.eng_id = m.eng_id
          LEFT JOIN t_auditee_entities_maping e
            ON e.entity_id = pe.entity_id
          JOIN t_au_audit_team_tasklist x
            ON x.eng_plan_id = m.eng_id
           AND x.teammember_ppno = P_PP_NO
         WHERE m.report_status = 'FINAL'
           AND NVL(x.isactive, 1) = 1
         ORDER BY m.eng_id DESC;
    END IF;

  EXCEPTION
    WHEN OTHERS THEN
      -- return empty cursor instead of failing UI
      OPEN O_CURSOR FOR
        SELECT CAST(NULL AS NUMBER) AS ENG_ID,
               CAST(NULL AS VARCHAR2(200)) AS REPORTING_OFFICE,
               CAST(NULL AS VARCHAR2(200)) AS ENTITY_NAME,
               CAST(NULL AS DATE) AS AUDIT_START_DATE,
               CAST(NULL AS DATE) AS AUDIT_END_DATE,
               CAST(NULL AS VARCHAR2(20)) AS REPORT_STATUS,
               CAST(NULL AS NUMBER) AS REPORT_VERSION
          FROM dual
         WHERE 1 = 0;
  END P_GET_ALLOWED_PDF_ENG_DETAILS;

  ----------------------------------------------------------------------
  -- DSA
  ----------------------------------------------------------------------
  PROCEDURE P_GET_INQ_DSA(p_complaint_id IN NUMBER, io_cursor OUT t_cursor) IS
  BEGIN
    OPEN io_cursor FOR
      SELECT d.dsa_id,
             d.complaint_id,
             d.person_name,
             d.designation,
             d.ppno_number,
             d.cnic,
             d.dsa_status,
             d.remarks,
             d.sort_order,
             d.status,
             d.created_by,
             d.created_on,
             d.updated_by,
             d.updated_on
        FROM t_au_iid_inq_dsa d
       WHERE d.complaint_id = p_complaint_id
         AND d.status = 'ACTIVE'
       ORDER BY d.sort_order, d.dsa_id;
  END;

  PROCEDURE P_ADD_INQ_DSA(p_complaint_id IN NUMBER,
                          p_person_name  IN VARCHAR2,
                          p_designation  IN VARCHAR2,
                          p_ppno_number  IN VARCHAR2,
                          p_cnic         IN VARCHAR2,
                          p_dsa_status   IN VARCHAR2,
                          p_remarks      IN VARCHAR2,
                          p_sort_order   IN NUMBER,
                          p_created_by   IN NUMBER,
                          io_cursor      OUT t_cursor) IS
    v_id NUMBER;
  BEGIN
    INSERT INTO t_au_iid_inq_dsa
      (complaint_id,
       person_name,
       designation,
       ppno_number,
       cnic,
       dsa_status,
       remarks,
       sort_order,
       status,
       created_by,
       created_on)
    VALUES
      (p_complaint_id,
       p_person_name,
       p_designation,
       p_ppno_number,
       p_cnic,
       NVL(p_dsa_status, 'DRAFT'),
       p_remarks,
       NVL(p_sort_order, 1),
       'ACTIVE',
       p_created_by,
       SYSDATE)
    RETURNING dsa_id INTO v_id;

    P_RESULT_OK(io_cursor, 'DSA row added.', v_id);
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to add DSA row: ' || SQLERRM);
  END;

  PROCEDURE P_UPDATE_INQ_DSA(p_dsa_id      IN NUMBER,
                             p_person_name IN VARCHAR2,
                             p_designation IN VARCHAR2,
                             p_ppno_number IN VARCHAR2,
                             p_cnic        IN VARCHAR2,
                             p_dsa_status  IN VARCHAR2,
                             p_remarks     IN VARCHAR2,
                             p_sort_order  IN NUMBER,
                             p_updated_by  IN NUMBER,
                             io_cursor     OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_dsa
       SET person_name = p_person_name,
           designation = p_designation,
           ppno_number = p_ppno_number,
           cnic        = p_cnic,
           dsa_status  = p_dsa_status,
           remarks     = p_remarks,
           sort_order  = NVL(p_sort_order, sort_order),
           updated_by  = p_updated_by,
           updated_on  = SYSDATE
     WHERE dsa_id = p_dsa_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active DSA row found to update.');
    ELSE
      P_RESULT_OK(io_cursor, 'DSA row updated.', p_dsa_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to update DSA row: ' || SQLERRM);
  END;

  PROCEDURE P_DELETE_INQ_DSA(p_dsa_id     IN NUMBER,
                             p_updated_by IN NUMBER,
                             io_cursor    OUT t_cursor) IS
    v_cnt NUMBER;
  BEGIN
    UPDATE t_au_iid_inq_dsa
       SET status     = 'DELETED',
           updated_by = p_updated_by,
           updated_on = SYSDATE
     WHERE dsa_id = p_dsa_id
       AND status = 'ACTIVE';

    v_cnt := SQL%ROWCOUNT;
    IF v_cnt = 0 THEN
      P_RESULT_FAIL(io_cursor, 'No active DSA row found to delete.');
    ELSE
      P_RESULT_OK(io_cursor, 'DSA row deleted.', p_dsa_id);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_RESULT_FAIL(io_cursor, 'Failed to delete DSA row: ' || SQLERRM);
  END;

  PROCEDURE P_GET_INQ_EVIDENCE_STEP(P_COMPLAINT_ID IN NUMBER,
                                    IO_CURSOR      OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT COMPLAINT_ID,
             MATERIAL_EVIDENCE_DETAIL,
             CIRCUMSTANTIAL_EVIDENCE_DETAIL,
             STATUS,
             CREATED_BY,
             CREATED_ON,
             UPDATED_BY,
             UPDATED_ON
        FROM T_AU_IID_INQ_EVIDENCE_STEP
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND NVL(STATUS, 'A') = 'A';
  END P_GET_INQ_EVIDENCE_STEP;

  PROCEDURE P_SAVE_INQ_EVIDENCE_STEP(P_COMPLAINT_ID                   IN NUMBER,
                                     P_MATERIAL_EVIDENCE_DETAIL       IN CLOB,
                                     P_CIRCUMSTANTIAL_EVIDENCE_DETAIL IN CLOB,
                                     io_cursor                        OUT t_cursor) AS
    V_EXISTS NUMBER := 0;
    v_msg    VARCHAR2(4000);
  BEGIN
    SELECT COUNT(*)
      INTO V_EXISTS
      FROM T_AU_IID_INQ_EVIDENCE_STEP
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;

    IF V_EXISTS = 0 THEN
      INSERT INTO T_AU_IID_INQ_EVIDENCE_STEP
        (COMPLAINT_ID,
         MATERIAL_EVIDENCE_DETAIL,
         CIRCUMSTANTIAL_EVIDENCE_DETAIL,
         STATUS,
         CREATED_BY,
         CREATED_ON)
      VALUES
        (P_COMPLAINT_ID,
         P_MATERIAL_EVIDENCE_DETAIL,
         P_CIRCUMSTANTIAL_EVIDENCE_DETAIL,
         'A',
         0,
         SYSDATE);
    ELSE
      UPDATE T_AU_IID_INQ_EVIDENCE_STEP
         SET MATERIAL_EVIDENCE_DETAIL       = P_MATERIAL_EVIDENCE_DETAIL,
             CIRCUMSTANTIAL_EVIDENCE_DETAIL = P_CIRCUMSTANTIAL_EVIDENCE_DETAIL,
             STATUS                         = 'A',
             UPDATED_BY                     = 0,
             UPDATED_ON                     = SYSDATE
       WHERE COMPLAINT_ID = P_COMPLAINT_ID;
    END IF;

    v_msg := 'SUCCESS';

    OPEN io_cursor FOR
      SELECT v_msg AS msg, P_COMPLAINT_ID AS complaint_id FROM dual;

  EXCEPTION
    WHEN OTHERS THEN
      v_msg := 'ERROR: ' || SQLERRM;

      OPEN io_cursor FOR
        SELECT v_msg AS msg, P_COMPLAINT_ID AS complaint_id FROM dual;
      RAISE;
  END P_SAVE_INQ_EVIDENCE_STEP;

  PROCEDURE P_GET_INQ_PROCEEDINGS(P_COMPLAINT_ID IN NUMBER,
                                  IO_CURSOR      OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT PROCEEDING_ID,
             COMPLAINT_ID,
             NOTICE_REFERENCE,
             VISIT_DATE,
             PLACE_VISITED,
             PARTICIPANTS_DETAIL,
             MISSING_PARTICIPANTS_REASON,
             SORT_ORDER,
             STATUS,
             CREATED_BY,
             CREATED_ON,
             UPDATED_BY,
             UPDATED_ON
        FROM T_AU_IID_INQ_PROCEEDINGS
       WHERE COMPLAINT_ID = P_COMPLAINT_ID
         AND NVL(STATUS, 'A') = 'A'
       ORDER BY NVL(SORT_ORDER, 0), PROCEEDING_ID;
  END P_GET_INQ_PROCEEDINGS;

  PROCEDURE P_SAVE_INQ_PROCEEDING(P_PROCEEDING_ID               IN OUT NUMBER,
                                  P_COMPLAINT_ID                IN NUMBER,
                                  P_NOTICE_REFERENCE            IN VARCHAR2,
                                  P_VISIT_DATE                  IN DATE,
                                  P_PLACE_VISITED               IN CLOB,
                                  P_PARTICIPANTS_DETAIL         IN CLOB,
                                  P_MISSING_PARTICIPANTS_REASON IN CLOB,
                                  P_SORT_ORDER                  IN NUMBER,
                                  P_STATUS                      IN VARCHAR2,
                                  P_USER_ID                     IN NUMBER,
                                  io_cursor                     OUT t_cursor) AS
    v_msg VARCHAR2(4000);
  BEGIN
    IF NVL(P_PROCEEDING_ID, 0) = 0 THEN
      P_PROCEEDING_ID := SEQ_IID_INQ_PROCEEDINGS.NEXTVAL;

      INSERT INTO T_AU_IID_INQ_PROCEEDINGS
        (PROCEEDING_ID,
         COMPLAINT_ID,
         NOTICE_REFERENCE,
         VISIT_DATE,
         PLACE_VISITED,
         PARTICIPANTS_DETAIL,
         MISSING_PARTICIPANTS_REASON,
         SORT_ORDER,
         STATUS,
         CREATED_BY,
         CREATED_ON)
      VALUES
        (P_PROCEEDING_ID,
         P_COMPLAINT_ID,
         P_NOTICE_REFERENCE,
         P_VISIT_DATE,
         P_PLACE_VISITED,
         P_PARTICIPANTS_DETAIL,
         P_MISSING_PARTICIPANTS_REASON,
         NVL(P_SORT_ORDER, 0),
         NVL(P_STATUS, 'A'),
         P_USER_ID,
         SYSDATE);
    ELSE
      UPDATE T_AU_IID_INQ_PROCEEDINGS
         SET NOTICE_REFERENCE            = P_NOTICE_REFERENCE,
             VISIT_DATE                  = P_VISIT_DATE,
             PLACE_VISITED               = P_PLACE_VISITED,
             PARTICIPANTS_DETAIL         = P_PARTICIPANTS_DETAIL,
             MISSING_PARTICIPANTS_REASON = P_MISSING_PARTICIPANTS_REASON,
             SORT_ORDER                  = NVL(P_SORT_ORDER, 0),
             STATUS                      = NVL(P_STATUS, 'A'),
             UPDATED_BY                  = P_USER_ID,
             UPDATED_ON                  = SYSDATE
       WHERE PROCEEDING_ID = P_PROCEEDING_ID;
    END IF;

    v_msg := 'SUCCESS';

    OPEN io_cursor FOR
      SELECT v_msg AS msg, P_PROCEEDING_ID AS proceeding_id FROM dual;

  EXCEPTION
    WHEN OTHERS THEN
      v_msg := 'ERROR: ' || SQLERRM;

      OPEN io_cursor FOR
        SELECT v_msg AS msg, P_PROCEEDING_ID AS proceeding_id FROM dual;
      RAISE;
  END P_SAVE_INQ_PROCEEDING;

  PROCEDURE P_DELETE_INQ_PROCEEDING(P_PROCEEDING_ID IN NUMBER,
                                    P_UPDATED_BY    IN NUMBER,
                                    io_cursor       OUT t_cursor) AS
    v_msg VARCHAR2(4000);
  BEGIN
    UPDATE T_AU_IID_INQ_PROCEEDINGS
       SET STATUS = 'D', UPDATED_BY = P_UPDATED_BY, UPDATED_ON = SYSDATE
     WHERE PROCEEDING_ID = P_PROCEEDING_ID;

    v_msg := 'SUCCESS';

    OPEN io_cursor FOR
      SELECT v_msg AS msg, P_PROCEEDING_ID AS proceeding_id FROM dual;

  EXCEPTION
    WHEN OTHERS THEN
      v_msg := 'ERROR: ' || SQLERRM;

      OPEN io_cursor FOR
        SELECT v_msg AS msg, P_PROCEEDING_ID AS proceeding_id FROM dual;
      RAISE;
  END P_DELETE_INQ_PROCEEDING;

  PROCEDURE P_FINALIZE_IID_INQUIRY_REPORT(P_COMPLAINT_ID IN NUMBER,
                                          P_UPDATED_BY   IN NUMBER) AS
  BEGIN
    UPDATE T_AU_IID_COMPLAINT_HDR
       SET IS_FINALIZED     = 'Y',
           FINALIZED_ON     = SYSDATE,
           UPDATED_BY_PP_NO = P_UPDATED_BY,
           UPDATED_ON       = SYSDATE
     WHERE COMPLAINT_ID = P_COMPLAINT_ID;

    IF SQL%ROWCOUNT = 0 THEN
      RAISE_APPLICATION_ERROR(-20001,
                              'No IID Inquiry Report found for the given Complaint ID.');
    END IF;
  END P_FINALIZE_IID_INQUIRY_REPORT;

  PROCEDURE P_ENQUEUE_EMAIL(P_EVENT_CODE IN VARCHAR2,
                            P_REF_ID1    IN NUMBER,
                            P_REF_ID2    IN NUMBER,
                            P_MAIL_TO    IN VARCHAR2,
                            P_MAIL_CC    IN VARCHAR2,
                            P_SUBJECT    IN VARCHAR2,
                            P_BODY       IN CLOB,
                            O_EMAIL_ID   OUT NUMBER) IS
  BEGIN
    SELECT SEQ_AU_IID_EMAIL_QUEUE_ID.NEXTVAL INTO O_EMAIL_ID FROM DUAL;

    INSERT INTO T_AU_IID_EMAIL_QUEUE
      (EMAIL_ID,
       EVENT_CODE,
       REF_ID1,
       REF_ID2,
       MAIL_TO,
       MAIL_CC,
       SUBJECT,
       BODY,
       STATUS,
       CREATED_ON,
       SENT_ON,
       ERROR_TEXT,
       RETRY_COUNT,
       CREATED_BY)
    VALUES
      (O_EMAIL_ID,
       TRIM(P_EVENT_CODE),
       P_REF_ID1,
       P_REF_ID2,
       TRIM(P_MAIL_TO),
       TRIM(P_MAIL_CC),
       P_SUBJECT,
       P_BODY,
       'PENDING',
       SYSDATE,
       NULL,
       NULL,
       0,
       NULL);
  END P_ENQUEUE_EMAIL;

  PROCEDURE P_GET_EMAIL_QUEUE(P_STATUS    IN VARCHAR2,
                              P_FROM_DATE IN DATE,
                              P_TO_DATE   IN DATE,
                              IO_CURSOR   OUT T_CURSOR) IS
  BEGIN
    OPEN IO_CURSOR FOR
      SELECT EMAIL_ID,
             EVENT_CODE,
             REF_ID1,
             REF_ID2,
             MAIL_TO,
             MAIL_CC,
             SUBJECT,
             BODY,
             STATUS,
             CREATED_ON,
             SENT_ON,
             ERROR_TEXT
        FROM T_AU_IID_EMAIL_QUEUE
       WHERE (P_STATUS IS NULL OR
             UPPER(STATUS) = UPPER(TRIM(P_STATUS)))
         AND (P_FROM_DATE IS NULL OR CREATED_ON >= TRUNC(P_FROM_DATE))
         AND (P_TO_DATE IS NULL OR CREATED_ON < TRUNC(P_TO_DATE) + 1)
       ORDER BY CASE UPPER(STATUS)
                  WHEN 'PENDING' THEN
                   1
                  WHEN 'FAILED' THEN
                   2
                  WHEN 'SENT' THEN
                   3
                  ELSE
                   4
                END,
                CREATED_ON,
                EMAIL_ID;
  END P_GET_EMAIL_QUEUE;

  PROCEDURE P_MARK_EMAIL_SENT(P_EMAIL_ID IN NUMBER) IS
  BEGIN
    UPDATE T_AU_IID_EMAIL_QUEUE
       SET STATUS = 'SENT', SENT_ON = SYSDATE, ERROR_TEXT = NULL
     WHERE EMAIL_ID = P_EMAIL_ID;
  END P_MARK_EMAIL_SENT;

  PROCEDURE P_MARK_EMAIL_FAILED(P_EMAIL_ID   IN NUMBER,
                                P_ERROR_TEXT IN VARCHAR2) IS
  BEGIN
    UPDATE T_AU_IID_EMAIL_QUEUE
       SET STATUS      = 'FAILED',
           ERROR_TEXT  = SUBSTR(P_ERROR_TEXT, 1, 2000),
           RETRY_COUNT = NVL(RETRY_COUNT, 0) + 1
     WHERE EMAIL_ID = P_EMAIL_ID;
  END P_MARK_EMAIL_FAILED;

END PKG_INQ;
/

PROMPT Replacing PKG_IAS_NOTIFICATION with its complete specification/body.
CREATE OR REPLACE PACKAGE PKG_IAS_NOTIFICATION AS
  PROCEDURE SEND_AUDIT_TEAM_ASSIGNED(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,
    P_REFERENCE VARCHAR2,P_ENTITY VARCHAR2,P_PERIOD VARCHAR2,P_TEAM VARCHAR2,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_AUDIT_TEAM_JOINED(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,
    P_ENTITY VARCHAR2,P_TEAM_LEAD VARCHAR2,P_TEAM_MEMBERS VARCHAR2,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_OBS_SUBMITTED_AUDITEE(P_ENG_ID NUMBER,P_OBS_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,
    P_REFERENCE VARCHAR2,P_ENTITY VARCHAR2,P_PERIOD VARCHAR2,P_HEADING VARCHAR2,P_SUMMARY CLOB,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_PARA_STATUS_UPDATED(P_COM_ID NUMBER,P_STATUS VARCHAR2,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_MGMT_AUDIT_PARA_STATUS(P_COM_ID NUMBER,P_DECISION_HIST_ID NUMBER,
    P_STATUS VARCHAR2,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_MGMT_AUDIT_WEEKLY(P_FROM_DATE DATE DEFAULT NULL,P_TO_DATE DATE DEFAULT NULL);
  PROCEDURE SEND_MGMT_AUDIT_MAPPING_EXCEPTIONS(P_REPORT_DATE DATE DEFAULT NULL);
  PROCEDURE SEND_NOTIFICATION_HEALTH(P_REPORT_DATE DATE DEFAULT NULL);
  PROCEDURE SEND_FINAL_REPORT_ISSUED(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,
    P_REFERENCE VARCHAR2,P_ENTITY VARCHAR2,P_PERIOD VARCHAR2,P_VERSION VARCHAR2,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_INQUIRY_ASSIGNED_UNIT(P_COMPLAINT_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,
    P_REFERENCE VARCHAR2,P_NATURE VARCHAR2,P_UNIT VARCHAR2,P_DUE_DATE VARCHAR2,P_DIRECTIONS CLOB,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_PASSWORD_RESET_SUCCESS(P_PPNO NUMBER,P_TEMP_PASSWORD VARCHAR2,P_MAIL_CC VARCHAR2,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_AUDIT_SAMPLE_ISSUE(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_AUDIT_EXCEPTION_ISSUE(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_AUDIT_CRITERIA_SUBMITTED(P_ENTITY_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,P_MESSAGE CLOB,O_EMAIL_ID OUT NUMBER);
  PROCEDURE SEND_AJAX_APPLICATION_ERROR(P_REFERENCE VARCHAR2,P_STATUS NUMBER,P_ENDPOINT VARCHAR2,
    P_ACTION VARCHAR2,P_USER_LABEL VARCHAR2,P_DETAILS CLOB,P_MAIL_TO VARCHAR2,O_EMAIL_ID OUT NUMBER);
END PKG_IAS_NOTIFICATION;
/

CREATE OR REPLACE PACKAGE BODY PKG_IAS_NOTIFICATION AS
  C_HEADER CONSTANT VARCHAR2(100) := 'Internal Audit System (IAS)';
  C_FOOTER CONSTANT VARCHAR2(500) := 'This is a system-generated notification from Internal Audit System (IAS). Please do not reply unless required under official process.';

  FUNCTION ESC(P_VALUE VARCHAR2) RETURN VARCHAR2 IS
  BEGIN
    RETURN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(NVL(P_VALUE,''),'&','&amp;'),'<','&lt;'),'>','&gt;'),'"','&quot;'),'''','&#39;');
  END;

  FUNCTION ROW_HTML(P_LABEL VARCHAR2,P_VALUE VARCHAR2) RETURN CLOB IS
  BEGIN
    IF TRIM(P_VALUE) IS NULL THEN RETURN EMPTY_CLOB(); END IF;
    RETURN TO_CLOB('<tr><td style="padding:12px 14px;border-bottom:1px solid #d9e2ec;font-size:13px;font-weight:bold;color:#102a43;vertical-align:top">')||
      ESC(P_LABEL)||'</td><td style="padding:12px 14px;border-bottom:1px solid #d9e2ec;font-size:13px;color:#334e68;vertical-align:top">'||
      REPLACE(ESC(P_VALUE),CHR(10),'<br />')||'</td></tr>';
  END;

  FUNCTION STANDARD_HTML(P_TITLE VARCHAR2,P_SUMMARY CLOB,P_ROWS CLOB) RETURN CLOB IS
  BEGIN
    RETURN TO_CLOB('<!DOCTYPE html><html><head><meta charset="utf-8" /></head><body style="margin:0;padding:0;background:#f4f6f8;font-family:Arial,Helvetica,sans-serif;color:#1f2933">')||
      '<table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f4f6f8;padding:24px 0"><tr><td align="center"><table role="presentation" width="680" cellspacing="0" cellpadding="0" style="width:680px;max-width:680px;background:#fff;border-collapse:collapse;border:1px solid #d9e2ec">'||
      '<tr><td style="padding:18px 28px;background:#173f5f;color:#fff;font-size:20px;font-weight:bold">'||ESC(C_HEADER)||'</td></tr>'||
      '<tr><td style="padding:28px 28px 16px;font-size:24px;font-weight:bold;color:#102a43">'||ESC(P_TITLE)||'</td></tr>'||
      CASE WHEN TRIM(DBMS_LOB.SUBSTR(P_SUMMARY,32767,1)) IS NOT NULL THEN
        '<tr><td style="padding:0 28px 20px;font-size:15px;line-height:1.7;color:#334e68">'||ESC(DBMS_LOB.SUBSTR(P_SUMMARY,32767,1))||'</td></tr>' END||
      CASE WHEN DBMS_LOB.GETLENGTH(NVL(P_ROWS,EMPTY_CLOB()))>0 THEN '<tr><td style="padding:0 28px 28px"><table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;border:1px solid #d9e2ec">'||P_ROWS||'</table></td></tr>' END||
      '<tr><td style="padding:18px 28px;background:#f8fafc;border-top:1px solid #d9e2ec;font-size:12px;line-height:1.6;color:#52606d">'||ESC(C_FOOTER)||'</td></tr></table></td></tr></table></body></html>';
  END;

  PROCEDURE ENQUEUE(P_CODE VARCHAR2,P_REF1 NUMBER,P_REF2 NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,
    P_REFERENCE VARCHAR2,P_STATUS VARCHAR2,P_ROWS CLOB,O_EMAIL_ID OUT NUMBER) IS
    V_M IAS_NOTIFICATION_MASTER%ROWTYPE; V_SUBJECT VARCHAR2(1000); V_TITLE VARCHAR2(500); V_SUMMARY CLOB; V_BODY CLOB;
  BEGIN
    SELECT * INTO V_M FROM IAS_NOTIFICATION_MASTER WHERE NOTIFICATION_CODE=UPPER(TRIM(P_CODE));
    IF V_M.IS_ACTIVE='N' THEN O_EMAIL_ID:=NULL; RETURN; END IF;
    V_SUBJECT:=REPLACE(REPLACE(V_M.SUBJECT_TEMPLATE,'{REFERENCE}',NVL(P_REFERENCE,'')),'{STATUS}',NVL(P_STATUS,''));
    V_TITLE:=REPLACE(REPLACE(V_M.TITLE_TEMPLATE,'{REFERENCE}',NVL(P_REFERENCE,'')),'{STATUS}',NVL(P_STATUS,''));
    V_SUMMARY:=REPLACE(REPLACE(V_M.SUMMARY_TEMPLATE,'{REFERENCE}',NVL(P_REFERENCE,'')),'{STATUS}',NVL(P_STATUS,''));
    V_BODY:=STANDARD_HTML(V_TITLE,V_SUMMARY,P_ROWS);
    IF TRIM(P_MAIL_TO) IS NULL THEN RAISE_APPLICATION_ERROR(-20101,'No TO recipient for '||P_CODE); END IF;
    PKG_INQ.P_ENQUEUE_EMAIL(P_CODE,P_REF1,P_REF2,P_MAIL_TO,P_MAIL_CC,V_SUBJECT,V_BODY,O_EMAIL_ID);
  END;

  PROCEDURE ENQUEUE_HTML(P_CODE VARCHAR2,P_REF1 NUMBER,P_REF2 NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,
    P_REFERENCE VARCHAR2,P_BODY CLOB,O_EMAIL_ID OUT NUMBER) IS
    V_M IAS_NOTIFICATION_MASTER%ROWTYPE; V_SUBJECT VARCHAR2(1000);
  BEGIN
    SELECT * INTO V_M FROM IAS_NOTIFICATION_MASTER WHERE NOTIFICATION_CODE=UPPER(TRIM(P_CODE));
    IF V_M.IS_ACTIVE='N' THEN O_EMAIL_ID:=NULL; RETURN; END IF;
    V_SUBJECT:=REPLACE(V_M.SUBJECT_TEMPLATE,'{REFERENCE}',NVL(P_REFERENCE,''));
    IF TRIM(P_MAIL_TO) IS NULL THEN RAISE_APPLICATION_ERROR(-20101,'No TO recipient for '||P_CODE); END IF;
    PKG_INQ.P_ENQUEUE_EMAIL(P_CODE,P_REF1,P_REF2,P_MAIL_TO,P_MAIL_CC,V_SUBJECT,P_BODY,O_EMAIL_ID);
  END;

  PROCEDURE SEND_AUDIT_TEAM_ASSIGNED(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,P_REFERENCE VARCHAR2,P_ENTITY VARCHAR2,P_PERIOD VARCHAR2,P_TEAM VARCHAR2,O_EMAIL_ID OUT NUMBER) IS V CLOB;
  BEGIN V:=ROW_HTML('Engagement',P_REFERENCE)||ROW_HTML('Entity',P_ENTITY)||ROW_HTML('Audit Period',P_PERIOD)||ROW_HTML('Team',P_TEAM);
    ENQUEUE('AUDIT_TEAM_ASSIGNED',P_ENG_ID,NULL,P_MAIL_TO,P_MAIL_CC,P_REFERENCE,NULL,V,O_EMAIL_ID); END;

  -- Called by PKG_AR.P_ADDJOININGREPORT with its existing output cursor values.
  PROCEDURE SEND_AUDIT_TEAM_JOINED(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,P_ENTITY VARCHAR2,P_TEAM_LEAD VARCHAR2,P_TEAM_MEMBERS VARCHAR2,O_EMAIL_ID OUT NUMBER) IS V CLOB;
  BEGIN V:=ROW_HTML('Entity',P_ENTITY)||ROW_HTML('Team Lead',P_TEAM_LEAD)||ROW_HTML('Team Members',P_TEAM_MEMBERS);
    ENQUEUE('AUDIT_TEAM_JOINED',P_ENG_ID,NULL,P_MAIL_TO,P_MAIL_CC,P_ENTITY,NULL,V,O_EMAIL_ID); END;

  PROCEDURE SEND_OBS_SUBMITTED_AUDITEE(P_ENG_ID NUMBER,P_OBS_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,P_REFERENCE VARCHAR2,P_ENTITY VARCHAR2,P_PERIOD VARCHAR2,P_HEADING VARCHAR2,P_SUMMARY CLOB,O_EMAIL_ID OUT NUMBER) IS V CLOB;
  BEGIN V:=ROW_HTML('Reference',P_REFERENCE)||ROW_HTML('Entity',P_ENTITY)||ROW_HTML('Audit Period',P_PERIOD)||ROW_HTML('Observation Heading',P_HEADING)||ROW_HTML('Status','Submitted to Auditee')||ROW_HTML('Observation Summary',DBMS_LOB.SUBSTR(P_SUMMARY,500,1));
    ENQUEUE('OBS_SUBMITTED_AUDITEE',P_ENG_ID,P_OBS_ID,P_MAIL_TO,P_MAIL_CC,P_REFERENCE,NULL,V,O_EMAIL_ID); END;

  -- Call from PKG_AE.P_SUBMITPOSTAUDITCOMPLIANCE_REVIEW after successful status update.
  -- V_GET_AIS_POST_COMPLIANCE supplies para/entity data; entity email resolution is
  -- performed once here, not repeated in the workflow procedure.
  PROCEDURE SEND_PARA_STATUS_UPDATED(P_COM_ID NUMBER,P_STATUS VARCHAR2,O_EMAIL_ID OUT NUMBER) IS
    V_REF VARCHAR2(200); V_GIST VARCHAR2(4000); V_TO VARCHAR2(2000); V_CC VARCHAR2(2000); V_ROWS CLOB;
  BEGIN
    SELECT TO_CHAR(C.PARA_NO),C.GIST_OF_PARAS,C.TO_EMAIL,C.CC_EMAIL
      INTO V_REF,V_GIST,V_TO,V_CC
      FROM V_IAS_POST_COMPLIANCE_NOTIFY C
     WHERE C.COM_ID=P_COM_ID;
    V_ROWS:=ROW_HTML('Para No.',V_REF)||ROW_HTML('Status',P_STATUS)||ROW_HTML('Gist of Para',V_GIST);
    ENQUEUE('PARA_STATUS_UPDATED',P_COM_ID,NULL,V_TO,V_CC,V_REF,P_STATUS,V_ROWS,O_EMAIL_ID);
  END;

  PROCEDURE SEND_MGMT_AUDIT_PARA_STATUS(P_COM_ID NUMBER,P_DECISION_HIST_ID NUMBER,
    P_STATUS VARCHAR2,O_EMAIL_ID OUT NUMBER) IS
    V_REF VARCHAR2(200); V_GIST VARCHAR2(4000); V_YEAR VARCHAR2(1000);
    V_RISK VARCHAR2(200); V_AUDITBY NUMBER; V_TO VARCHAR2(2000); V_CC VARCHAR2(2000);
    V_REASON VARCHAR2(1000); V_STATUS VARCHAR2(20); V_ROWS CLOB;
  BEGIN
    V_STATUS:=CASE WHEN UPPER(TRIM(P_STATUS))='SETTLED' THEN 'Settled'
                   WHEN UPPER(TRIM(P_STATUS))='REJECTED' THEN 'Rejected' END;
    IF V_STATUS IS NULL THEN RAISE_APPLICATION_ERROR(-20102,'Invalid Management Audit para status.'); END IF;
    SELECT TO_CHAR(C.PARA_NO),C.GIST_OF_PARAS,C.AUDIT_PERIOD,C.RISK,C.AUDITBY_ID,
           C.TO_EMAIL,C.CC_EMAIL
      INTO V_REF,V_GIST,V_YEAR,V_RISK,V_AUDITBY,V_TO,V_CC
      FROM V_IAS_POST_COMPLIANCE_NOTIFY C
     WHERE C.COM_ID=P_COM_ID;
    IF V_AUDITBY NOT IN (112242,112248) THEN
      RAISE_APPLICATION_ERROR(-20103,'Management Audit notification requested for another audit area.');
    END IF;
    SELECT CASE WHEN V_STATUS='Rejected' THEN H.COMMENTS END
      INTO V_REASON
      FROM AIS_T_AU_POST_COMPLIANCE_HISTORY H
     WHERE H.HIST_ID=P_DECISION_HIST_ID AND H.COM_ID=P_COM_ID;
    V_ROWS:=ROW_HTML('Para No.',V_REF)||ROW_HTML('Audit Year',V_YEAR)||
      ROW_HTML('Risk',V_RISK)||ROW_HTML('Status',V_STATUS)||ROW_HTML('Gist of Para',V_GIST)||
      CASE WHEN V_STATUS='Rejected' THEN ROW_HTML('Reason for Rejection',NVL(V_REASON,'Not recorded')) ELSE EMPTY_CLOB() END;
    ENQUEUE('MGMT_AUDIT_PARA_STATUS',P_COM_ID,P_DECISION_HIST_ID,V_TO,V_CC,V_REF,V_STATUS,V_ROWS,O_EMAIL_ID);
  END;

  PROCEDURE SEND_MGMT_AUDIT_WEEKLY(P_FROM_DATE DATE,P_TO_DATE DATE) IS
    C_CODE CONSTANT VARCHAR2(50):='MGMT_AUDIT_WEEKLY_PARA_STATUS';
    V_FROM DATE; V_TO_DATE DATE; V_SETTLED CLOB; V_REJECTED CLOB; V_BODY CLOB;
    V_SN NUMBER; V_RN NUMBER; V_EMAIL_ID NUMBER; V_REF1 NUMBER; V_EXISTING NUMBER;
    FUNCTION CELL(P_VALUE VARCHAR2,P_HEAD BOOLEAN DEFAULT FALSE) RETURN CLOB IS
      V_TAG VARCHAR2(2):=CASE WHEN P_HEAD THEN 'th' ELSE 'td' END;
      V_BG VARCHAR2(60):=CASE WHEN P_HEAD THEN 'background:#eef2f6;font-weight:bold;' ELSE '' END;
    BEGIN RETURN TO_CLOB('<'||V_TAG||' style="padding:8px 10px;border-bottom:1px solid #d9e2ec;font-size:12px;color:#334e68;'||V_BG||'">')||ESC(P_VALUE)||'</'||V_TAG||'>'; END;
    FUNCTION FMT(P_DATE DATE) RETURN VARCHAR2 IS BEGIN RETURN TO_CHAR(P_DATE,'DD-MON-YYYY'); END;
  BEGIN
    IF (P_FROM_DATE IS NULL AND P_TO_DATE IS NOT NULL) OR (P_FROM_DATE IS NOT NULL AND P_TO_DATE IS NULL) THEN
      RAISE_APPLICATION_ERROR(-20104,'Supply both weekly reporting dates or neither.');
    END IF;
    V_FROM:=TRUNC(NVL(P_FROM_DATE,TRUNC(SYSDATE,'IW')-7));
    V_TO_DATE:=TRUNC(NVL(P_TO_DATE,TRUNC(SYSDATE,'IW')-1));
    IF V_TO_DATE<V_FROM THEN RAISE_APPLICATION_ERROR(-20105,'Invalid weekly reporting period.'); END IF;
    V_REF1:=TO_NUMBER(TO_CHAR(V_FROM,'YYYYMMDD'));

    FOR RCP IN (
      SELECT M.DIVISION_ID,M.DIVISION_NAME,M.DIVISION_EMAIL MAIL_TO,
             LISTAGG(DISTINCT M.REPORTING_EMAIL,';') WITHIN GROUP (ORDER BY M.REPORTING_EMAIL) MAIL_CC
        FROM AIS_T_AU_POST_COMPLIANCE PC
        JOIN AIS_T_AU_POST_COMPLIANCE_HISTORY H ON H.COM_ID=PC.COM_ID
        JOIN V_IAS_MGMT_AUDIT_NOTIFY_MAP M ON M.ENTITY_ID=PC.ENTITY_ID
         AND M.DIVISION_ID IS NOT NULL AND TRIM(M.DIVISION_EMAIL) IS NOT NULL
       WHERE PC.AUDITED_BY IN (112242,112248)
         AND H.COM_STATUS IN (16,12,15,18)
         AND H.COMMENT_ON>=V_FROM AND H.COMMENT_ON<V_TO_DATE+1
       GROUP BY M.DIVISION_ID,M.DIVISION_NAME,M.DIVISION_EMAIL
    ) LOOP
      SELECT COUNT(*) INTO V_EXISTING FROM T_AU_IID_EMAIL_QUEUE Q
       WHERE Q.EVENT_CODE=C_CODE AND Q.REF_ID1=V_REF1 AND Q.REF_ID2=RCP.DIVISION_ID;
      IF V_EXISTING>0 THEN
        CONTINUE;
      END IF;
      V_SETTLED:='<tr>'||CELL('Sr.',TRUE)||CELL('Entity',TRUE)||CELL('Audit Year',TRUE)||CELL('Para No.',TRUE)||
        CELL('Title',TRUE)||CELL('Compliance Submitted On',TRUE)||CELL('Decision On',TRUE)||'</tr>';
      V_REJECTED:='<tr>'||CELL('Sr.',TRUE)||CELL('Entity',TRUE)||CELL('Audit Year',TRUE)||CELL('Para No.',TRUE)||
        CELL('Title',TRUE)||CELL('Compliance Submitted On',TRUE)||CELL('Decision On',TRUE)||CELL('Reason',TRUE)||'</tr>';
      V_SN:=0; V_RN:=0;
      FOR DCS IN (
        SELECT E.NAME ENTITY_NAME,PC.AUDIT_PERIOD,PC.PARA_NO,PC.GIST_OF_PARAS TITLE,
               (SELECT MAX(S.COMMENT_ON) FROM AIS_T_AU_POST_COMPLIANCE_HISTORY S
                 WHERE S.COM_ID=H.COM_ID AND S.COM_CYCLE=H.COM_CYCLE AND S.COM_STATUS=10
                   AND S.COMMENT_ON<=H.COMMENT_ON) SUBMITTED_ON,
               H.COMMENT_ON DECISION_ON,H.COMMENTS REASON,H.COM_STATUS
          FROM AIS_T_AU_POST_COMPLIANCE PC
          JOIN AIS_T_AU_POST_COMPLIANCE_HISTORY H ON H.COM_ID=PC.COM_ID
          JOIN T_AUDITEE_ENTITIES E ON E.ENTITY_ID=PC.ENTITY_ID
          JOIN V_IAS_MGMT_AUDIT_NOTIFY_MAP M ON M.ENTITY_ID=PC.ENTITY_ID
           AND M.DIVISION_ID IS NOT NULL AND TRIM(M.DIVISION_EMAIL) IS NOT NULL
         WHERE PC.AUDITED_BY IN (112242,112248) AND M.DIVISION_ID=RCP.DIVISION_ID
           AND H.COM_STATUS IN (16,12,15,18)
           AND H.COMMENT_ON>=V_FROM AND H.COMMENT_ON<V_TO_DATE+1
         ORDER BY H.COMMENT_ON,H.HIST_ID
      ) LOOP
        IF DCS.COM_STATUS=16 THEN
          V_SN:=V_SN+1; V_SETTLED:=V_SETTLED||'<tr>'||CELL(TO_CHAR(V_SN))||CELL(DCS.ENTITY_NAME)||
            CELL(DCS.AUDIT_PERIOD)||CELL(DCS.PARA_NO)||CELL(DCS.TITLE)||CELL(FMT(DCS.SUBMITTED_ON))||CELL(FMT(DCS.DECISION_ON))||'</tr>';
        ELSE
          V_RN:=V_RN+1; V_REJECTED:=V_REJECTED||'<tr>'||CELL(TO_CHAR(V_RN))||CELL(DCS.ENTITY_NAME)||
            CELL(DCS.AUDIT_PERIOD)||CELL(DCS.PARA_NO)||CELL(DCS.TITLE)||CELL(FMT(DCS.SUBMITTED_ON))||CELL(FMT(DCS.DECISION_ON))||CELL(DCS.REASON)||'</tr>';
        END IF;
      END LOOP;
      IF V_SN=0 AND V_RN=0 THEN CONTINUE; END IF;
      IF V_SN=0 THEN V_SETTLED:=V_SETTLED||'<tr><td colspan="7" style="padding:10px">No settled paras.</td></tr>'; END IF;
      IF V_RN=0 THEN V_REJECTED:=V_REJECTED||'<tr><td colspan="8" style="padding:10px">No rejected paras.</td></tr>'; END IF;
      V_BODY:=TO_CLOB('<!DOCTYPE html><html><body style="margin:0;background:#f4f6f8;font-family:Arial;color:#1f2933"><table width="100%"><tr><td align="center"><table width="900" style="background:#fff;border-collapse:collapse"><tr><td style="padding:18px 28px;background:#173f5f;color:#fff;font-size:20px;font-weight:bold">Internal Audit System (IAS)</td></tr><tr><td style="padding:28px;font-size:22px;font-weight:bold">Weekly Management Audit Para Decisions</td></tr><tr><td style="padding:0 28px 18px">Reporting period: ')||ESC(FMT(V_FROM)||' to '||FMT(V_TO_DATE))||'<br>Division: '||ESC(RCP.DIVISION_NAME)||'</td></tr><tr><td style="padding:8px 28px;font-weight:bold">SETTLED PARAS</td></tr><tr><td style="padding:0 28px 18px"><table width="100%" style="border-collapse:collapse">'||V_SETTLED||'</table></td></tr><tr><td style="padding:8px 28px;font-weight:bold">REJECTED PARAS</td></tr><tr><td style="padding:0 28px 18px"><table width="100%" style="border-collapse:collapse">'||V_REJECTED||'</table></td></tr><tr><td style="padding:18px 28px;background:#f8fafc;font-size:12px">'||ESC(C_FOOTER)||'</td></tr></table></td></tr></table></body></html>';
      ENQUEUE_HTML(C_CODE,V_REF1,RCP.DIVISION_ID,RCP.MAIL_TO,RCP.MAIL_CC,
        FMT(V_FROM)||' to '||FMT(V_TO_DATE),V_BODY,V_EMAIL_ID);
    END LOOP;
  EXCEPTION WHEN OTHERS THEN
    PKG_LG.LOG_ERROR('IAS_NOTIFICATION','DBMS_SCHEDULER','SEND_MGMT_AUDIT_WEEKLY',
      'Weekly Management Audit notification failed',SQLERRM);
    RAISE;
  END;

  PROCEDURE SEND_MGMT_AUDIT_MAPPING_EXCEPTIONS(P_REPORT_DATE DATE) IS
    C_CODE CONSTANT VARCHAR2(50):='MGMT_AUDIT_MAPPING_EXCEPTION';
    V_DATE DATE:=TRUNC(NVL(P_REPORT_DATE,SYSDATE)); V_REF1 NUMBER; V_ROWS CLOB;
    V_BODY CLOB; V_EMAIL_ID NUMBER; V_COUNT NUMBER; V_EXISTING NUMBER; V_SN NUMBER;
    FUNCTION CELL(P_VALUE VARCHAR2,P_HEAD BOOLEAN DEFAULT FALSE) RETURN CLOB IS
      V_TAG VARCHAR2(2):=CASE WHEN P_HEAD THEN 'th' ELSE 'td' END;
      V_BG VARCHAR2(60):=CASE WHEN P_HEAD THEN 'background:#eef2f6;font-weight:bold;' ELSE '' END;
    BEGIN RETURN TO_CLOB('<'||V_TAG||' style="padding:8px 10px;border-bottom:1px solid #d9e2ec;font-size:12px;color:#334e68;'||V_BG||'">')||ESC(P_VALUE)||'</'||V_TAG||'>'; END;
  BEGIN
    V_REF1:=TO_NUMBER(TO_CHAR(V_DATE,'YYYYMMDD'));
    FOR HD IN (
      SELECT X.AUDITED_BY,H.NAME HEAD_NAME,H.EMAIL_ADDRESS MAIL_TO
        FROM (SELECT 112242 AUDITED_BY FROM DUAL UNION ALL SELECT 112248 FROM DUAL) X
        LEFT JOIN T_AUDITEE_ENTITIES H ON H.ENTITY_ID=X.AUDITED_BY
    ) LOOP
      SELECT COUNT(*) INTO V_COUNT FROM V_IAS_MGMT_AUDIT_NOTIFY_MAP M
       WHERE M.AUDITED_BY=HD.AUDITED_BY AND M.MAPPING_READY='N';
      IF V_COUNT=0 THEN CONTINUE; END IF;
      IF TRIM(HD.MAIL_TO) IS NULL THEN
        PKG_LG.LOG_ERROR('IAS_NOTIFICATION','DBMS_SCHEDULER','SEND_MGMT_AUDIT_MAPPING_EXCEPTIONS',
          'Mapping exceptions exist but audit head email is missing','AUDITED_BY='||HD.AUDITED_BY);
        CONTINUE;
      END IF;
      SELECT COUNT(*) INTO V_EXISTING FROM T_AU_IID_EMAIL_QUEUE Q
       WHERE Q.EVENT_CODE=C_CODE AND Q.REF_ID1=V_REF1 AND Q.REF_ID2=HD.AUDITED_BY;
      IF V_EXISTING>0 THEN CONTINUE; END IF;
      V_ROWS:='<tr>'||CELL('Sr.',TRUE)||CELL('Entity',TRUE)||CELL('Entity ID',TRUE)||CELL('Audited By',TRUE)||
        CELL('Divisional Office',TRUE)||CELL('Divisional Head Email',TRUE)||
        CELL('Reporting Office / Group',TRUE)||CELL('Reporting Office / Group Email',TRUE)||
        CELL('Missing / Required Action',TRUE)||'</tr>';
      V_SN:=0;
      FOR E IN (
        SELECT * FROM V_IAS_MGMT_AUDIT_NOTIFY_MAP M
         WHERE M.AUDITED_BY=HD.AUDITED_BY AND M.MAPPING_READY='N'
         ORDER BY M.ENTITY_NAME,M.ENTITY_ID
      ) LOOP
        V_SN:=V_SN+1;
        V_ROWS:=V_ROWS||'<tr>'||CELL(TO_CHAR(V_SN))||CELL(E.ENTITY_NAME)||CELL(TO_CHAR(E.ENTITY_ID))||
          CELL(TO_CHAR(E.AUDITED_BY))||CELL(NVL(E.DIVISION_NAME,'Missing'))||CELL(NVL(E.DIVISION_EMAIL,'Missing'))||
          CELL(NVL(E.REPORTING_NAME,'Missing'))||CELL(NVL(E.REPORTING_EMAIL,'Missing'))||
          CELL(E.MISSING_REQUIRED_ACTION)||'</tr>';
      END LOOP;
      V_BODY:=TO_CLOB('<!DOCTYPE html><html><body style="margin:0;background:#f4f6f8;font-family:Arial;color:#1f2933"><table width="100%"><tr><td align="center"><table width="1100" style="background:#fff;border-collapse:collapse"><tr><td style="padding:18px 28px;background:#173f5f;color:#fff;font-size:20px;font-weight:bold">Internal Audit System (IAS)</td></tr><tr><td style="padding:28px;font-size:22px;font-weight:bold">Management Audit Entity Mapping Exceptions</td></tr><tr><td style="padding:0 28px 18px">Audit domain: ')||ESC(TO_CHAR(HD.AUDITED_BY)||' - '||NVL(HD.HEAD_NAME,'Unknown'))||'</td></tr><tr><td style="padding:0 28px 28px"><table width="100%" style="border-collapse:collapse">'||V_ROWS||'</table></td></tr><tr><td style="padding:18px 28px;background:#f8fafc;font-size:12px">'||ESC(C_FOOTER)||'</td></tr></table></td></tr></table></body></html>';
      ENQUEUE_HTML(C_CODE,V_REF1,HD.AUDITED_BY,HD.MAIL_TO,NULL,TO_CHAR(HD.AUDITED_BY),V_BODY,V_EMAIL_ID);
    END LOOP;
  EXCEPTION WHEN OTHERS THEN
    PKG_LG.LOG_ERROR('IAS_NOTIFICATION','DBMS_SCHEDULER','SEND_MGMT_AUDIT_MAPPING_EXCEPTIONS',
      'Management Audit mapping exception notification failed',SQLERRM);
    RAISE;
  END;

  PROCEDURE SEND_NOTIFICATION_HEALTH(P_REPORT_DATE DATE) IS
    C_CODE CONSTANT VARCHAR2(50):='IAS_NOTIFICATION_HEALTH';
    V_DATE DATE:=TRUNC(NVL(P_REPORT_DATE,SYSDATE)); V_REF1 NUMBER; V_EXISTING NUMBER;
    V_TO VARCHAR2(4000); V_ROWS CLOB; V_SUMMARY CLOB; V_ERRORS CLOB; V_BODY CLOB;
    V_EMAIL_ID NUMBER; V_PENDING NUMBER; V_FAILED NUMBER; V_RETRIES NUMBER; V_ERROR_ROWS NUMBER:=0;
    V_OLDEST DATE; V_LAST_SENT DATE; V_OBJECTS VARCHAR2(4000); V_IMMEDIATE VARCHAR2(1000);
    V_WEEKLY VARCHAR2(1000); V_MAPPING VARCHAR2(1000); V_HEALTH VARCHAR2(1000); V_CI VARCHAR2(500);
    V_QUEUE_CONTRACT VARCHAR2(100); V_QUEUE_TYPE VARCHAR2(30); V_QUEUE_VALID NUMBER;
    FUNCTION CELL(P_VALUE VARCHAR2,P_HEAD BOOLEAN DEFAULT FALSE) RETURN CLOB IS
      V_TAG VARCHAR2(2):=CASE WHEN P_HEAD THEN 'th' ELSE 'td' END;
      V_BG VARCHAR2(60):=CASE WHEN P_HEAD THEN 'background:#eef2f6;font-weight:bold;' ELSE '' END;
    BEGIN RETURN TO_CLOB('<'||V_TAG||' style="padding:8px 10px;border-bottom:1px solid #d9e2ec;font-size:12px;color:#334e68;'||V_BG||'">')||ESC(P_VALUE)||'</'||V_TAG||'>'; END;
    FUNCTION FMT(P_DATE DATE) RETURN VARCHAR2 IS BEGIN RETURN NVL(TO_CHAR(P_DATE,'DD-MON-YYYY HH24:MI:SS'),'None'); END;
  BEGIN
    V_REF1:=TO_NUMBER(TO_CHAR(V_DATE,'YYYYMMDD'));
    SELECT COUNT(*) INTO V_EXISTING FROM T_AU_IID_EMAIL_QUEUE
     WHERE EVENT_CODE=C_CODE AND REF_ID1=V_REF1;
    IF V_EXISTING>0 THEN RETURN; END IF;

    SELECT LISTAGG(DISTINCT E.EMAIL,';') WITHIN GROUP (ORDER BY E.EMAIL)
      INTO V_TO
      FROM T_USER_MAPING M JOIN T_USER U ON U.USERID=M.USERID
      JOIN V_SERVICE_EMPLOYEEINFO E ON E.PPNO=U.PPNO
     WHERE M.ROLE_ID=1 AND NVL(U.ISACTIVE,'Y')='Y' AND TRIM(E.EMAIL) IS NOT NULL;
    IF TRIM(V_TO) IS NULL THEN RAISE_APPLICATION_ERROR(-20106,'No active Super Admin email recipient is configured.'); END IF;

    SELECT SUM(CASE WHEN STATUS='PENDING' THEN 1 ELSE 0 END),
           SUM(CASE WHEN STATUS='FAILED' THEN 1 ELSE 0 END),NVL(SUM(RETRY_COUNT),0),
           MIN(CASE WHEN STATUS='PENDING' THEN CREATED_ON END),MAX(SENT_ON)
      INTO V_PENDING,V_FAILED,V_RETRIES,V_OLDEST,V_LAST_SENT
      FROM T_AU_IID_EMAIL_QUEUE;
    SELECT LISTAGG(OBJECT_NAME||' ('||STATUS||')',', ') WITHIN GROUP (ORDER BY OBJECT_NAME)
      INTO V_OBJECTS FROM USER_OBJECTS
     WHERE OBJECT_NAME IN ('PKG_INQ','PKG_AE','PKG_IAS_NOTIFICATION','V_IAS_POST_COMPLIANCE_NOTIFY','V_IAS_MGMT_AUDIT_NOTIFY_MAP')
       AND OBJECT_TYPE IN ('PACKAGE','PACKAGE BODY','VIEW');
    SELECT NVL(MAX(DATA_TYPE),'MISSING') INTO V_QUEUE_TYPE
      FROM USER_ARGUMENTS
     WHERE PACKAGE_NAME='PKG_INQ' AND OBJECT_NAME='P_GET_EMAIL_QUEUE'
       AND ARGUMENT_NAME='P_STATUS' AND DATA_LEVEL=0;
    SELECT COUNT(*) INTO V_QUEUE_VALID FROM USER_OBJECTS
     WHERE OBJECT_NAME='PKG_INQ' AND OBJECT_TYPE IN ('PACKAGE','PACKAGE BODY') AND STATUS='VALID';
    V_QUEUE_CONTRACT:=V_QUEUE_TYPE||' / '||
      CASE WHEN V_QUEUE_TYPE='VARCHAR2' AND V_QUEUE_VALID=2 THEN 'VALID' ELSE 'INVALID' END;
    SELECT 'Generated='||COUNT(*)||', Sent='||SUM(CASE WHEN STATUS='SENT' THEN 1 ELSE 0 END)||
           ', Pending='||SUM(CASE WHEN STATUS='PENDING' THEN 1 ELSE 0 END)||', Failed='||SUM(CASE WHEN STATUS='FAILED' THEN 1 ELSE 0 END)||
           ', Last sent='||NVL(TO_CHAR(MAX(SENT_ON),'DD-MON-YYYY HH24:MI:SS'),'None')
      INTO V_IMMEDIATE FROM T_AU_IID_EMAIL_QUEUE WHERE EVENT_CODE='MGMT_AUDIT_PARA_STATUS';
    BEGIN
      SELECT 'Enabled='||ENABLED||', State='||STATE||', Next run='||TO_CHAR(NEXT_RUN_DATE,'DD-MON-YYYY HH24:MI TZH:TZM')||
             ', Failures='||FAILURE_COUNT
        INTO V_WEEKLY FROM USER_SCHEDULER_JOBS WHERE JOB_NAME='JOB_MGMT_AUDIT_WEEKLY_NOTIFY';
    EXCEPTION WHEN NO_DATA_FOUND THEN V_WEEKLY:='Job missing'; END;
    BEGIN
      SELECT 'Enabled='||ENABLED||', State='||STATE||', Next run='||TO_CHAR(NEXT_RUN_DATE,'DD-MON-YYYY HH24:MI TZH:TZM')||
             ', Failures='||FAILURE_COUNT
        INTO V_MAPPING FROM USER_SCHEDULER_JOBS WHERE JOB_NAME='JOB_MGMT_AUDIT_MAPPING_EXCEPT';
    EXCEPTION WHEN NO_DATA_FOUND THEN V_MAPPING:='Job missing'; END;
    BEGIN
      SELECT 'Enabled='||ENABLED||', State='||STATE||', Next run='||TO_CHAR(NEXT_RUN_DATE,'DD-MON-YYYY HH24:MI TZH:TZM')||
             ', Failures='||FAILURE_COUNT
        INTO V_HEALTH FROM USER_SCHEDULER_JOBS WHERE JOB_NAME='JOB_IAS_NOTIFICATION_HEALTH';
    EXCEPTION WHEN NO_DATA_FOUND THEN V_HEALTH:='Job missing'; END;
    V_CI:='No authoritative runtime CI/CD integration is present; build and Oracle compilation are deployment-time controls.';

    V_SUMMARY:=ROW_HTML('Email queue worker status / last successful processing',
        'No authoritative worker heartbeat is instrumented; last successful queue send: '||FMT(V_LAST_SENT))||
      ROW_HTML('Pending email count',TO_CHAR(NVL(V_PENDING,0)))||ROW_HTML('Failed email count',TO_CHAR(NVL(V_FAILED,0)))||
      ROW_HTML('Retry status / retry count','Failed rows remain retryable in the existing queue; cumulative retry count: '||TO_CHAR(V_RETRIES))||
      ROW_HTML('Oldest pending email',FMT(V_OLDEST))||ROW_HTML('Last successfully sent notification',FMT(V_LAST_SENT))||
      ROW_HTML('Management Audit immediate notification status',V_IMMEDIATE)||
      ROW_HTML('Management Audit weekly scheduler status and next run',V_WEEKLY)||
      ROW_HTML('Mapping exception scheduler status and next run',V_MAPPING)||
      ROW_HTML('Notification health scheduler status and next run',V_HEALTH)||
      ROW_HTML('P_GET_EMAIL_QUEUE.P_STATUS contract',V_QUEUE_CONTRACT)||
      ROW_HTML('Notification package/object validity',V_OBJECTS)||ROW_HTML('CI / build verification',V_CI);

    V_ROWS:='<tr>'||CELL('Notification Type',TRUE)||CELL('Generated',TRUE)||CELL('Sent Successfully',TRUE)||
      CELL('Pending',TRUE)||CELL('Failed',TRUE)||CELL('Retried Successfully',TRUE)||CELL('Last Successful Send',TRUE)||'</tr>';
    FOR S IN (
      SELECT X.EVENT_CODE,COUNT(Q.EMAIL_ID) GENERATED_COUNT,
             SUM(CASE WHEN Q.STATUS='SENT' THEN 1 ELSE 0 END) SENT_OK,
             SUM(CASE WHEN Q.STATUS='PENDING' THEN 1 ELSE 0 END) PENDING,
             SUM(CASE WHEN Q.STATUS='FAILED' THEN 1 ELSE 0 END) FAILED,
             SUM(CASE WHEN Q.STATUS='SENT' AND NVL(Q.RETRY_COUNT,0)>0 THEN 1 ELSE 0 END) RETRIED_OK,
             MAX(Q.SENT_ON) LAST_SENT
        FROM (SELECT 'MGMT_AUDIT_PARA_STATUS' EVENT_CODE FROM DUAL UNION ALL
              SELECT 'MGMT_AUDIT_WEEKLY_PARA_STATUS' FROM DUAL UNION ALL
              SELECT 'MGMT_AUDIT_MAPPING_EXCEPTION' FROM DUAL) X
        LEFT JOIN T_AU_IID_EMAIL_QUEUE Q ON Q.EVENT_CODE=X.EVENT_CODE
       GROUP BY X.EVENT_CODE ORDER BY X.EVENT_CODE
    ) LOOP
      V_ROWS:=V_ROWS||'<tr>'||CELL(S.EVENT_CODE)||CELL(TO_CHAR(S.GENERATED_COUNT))||CELL(TO_CHAR(S.SENT_OK))||
        CELL(TO_CHAR(S.PENDING))||CELL(TO_CHAR(S.FAILED))||CELL(TO_CHAR(S.RETRIED_OK))||CELL(FMT(S.LAST_SENT))||'</tr>';
    END LOOP;
    V_ERRORS:='<tr>'||CELL('Reference',TRUE)||CELL('Notification Type',TRUE)||CELL('Created On',TRUE)||CELL('Retry Count',TRUE)||CELL('Error',TRUE)||'</tr>';
    FOR E IN (SELECT * FROM (SELECT EMAIL_ID,EVENT_CODE,CREATED_ON,RETRY_COUNT,ERROR_TEXT FROM T_AU_IID_EMAIL_QUEUE
                              WHERE STATUS='FAILED' ORDER BY CREATED_ON DESC) WHERE ROWNUM<=10) LOOP
      V_ERROR_ROWS:=V_ERROR_ROWS+1;
      V_ERRORS:=V_ERRORS||'<tr>'||CELL(TO_CHAR(E.EMAIL_ID))||CELL(E.EVENT_CODE)||CELL(FMT(E.CREATED_ON))||
        CELL(TO_CHAR(E.RETRY_COUNT))||CELL(E.ERROR_TEXT)||'</tr>';
    END LOOP;
    FOR L IN (SELECT * FROM (SELECT LOG_TIME,ACTION,MESSAGE FROM T_SYS_LOG
                              WHERE LOG_LEVEL='ERROR' AND
                                (MODULE='IAS_NOTIFICATION' OR
                                 (CONTROLLER='PKG_AE' AND ACTION='P_SUBMITPOSTAUDITCOMPLIANCE_REVIEW'
                                  AND MESSAGE LIKE 'Management Audit para notification%'))
                              ORDER BY LOG_TIME DESC) WHERE ROWNUM<=10) LOOP
      V_ERROR_ROWS:=V_ERROR_ROWS+1;
      V_ERRORS:=V_ERRORS||'<tr>'||CELL('System log')||CELL(L.ACTION)||CELL(TO_CHAR(L.LOG_TIME,'DD-MON-YYYY HH24:MI:SS'))||
        CELL('-')||CELL(L.MESSAGE)||'</tr>';
    END LOOP;
    IF V_ERROR_ROWS=0 THEN V_ERRORS:=V_ERRORS||'<tr><td colspan="5" style="padding:10px">No notification processing errors recorded.</td></tr>'; END IF;
    V_BODY:=TO_CLOB('<!DOCTYPE html><html><body style="margin:0;background:#f4f6f8;font-family:Arial;color:#1f2933"><table width="100%"><tr><td align="center"><table width="1000" style="background:#fff;border-collapse:collapse"><tr><td style="padding:18px 28px;background:#173f5f;color:#fff;font-size:20px;font-weight:bold">Internal Audit System (IAS)</td></tr><tr><td style="padding:28px;font-size:22px;font-weight:bold">IAS Notification Health Report</td></tr><tr><td style="padding:0 28px 20px"><table width="100%" style="border-collapse:collapse">')||V_SUMMARY||'</table></td></tr><tr><td style="padding:8px 28px;font-weight:bold">SUCCESSFUL DELIVERY SUMMARY</td></tr><tr><td style="padding:0 28px 20px"><table width="100%" style="border-collapse:collapse">'||V_ROWS||'</table></td></tr><tr><td style="padding:8px 28px;font-weight:bold">NOTIFICATION PROCESSING ERRORS</td></tr><tr><td style="padding:0 28px 28px"><table width="100%" style="border-collapse:collapse">'||V_ERRORS||'</table></td></tr><tr><td style="padding:18px 28px;background:#f8fafc;font-size:12px">'||ESC(C_FOOTER)||'</td></tr></table></td></tr></table></body></html>';
    ENQUEUE_HTML(C_CODE,V_REF1,NULL,V_TO,NULL,TO_CHAR(V_DATE,'DD-MON-YYYY'),V_BODY,V_EMAIL_ID);
  EXCEPTION WHEN OTHERS THEN
    PKG_LG.LOG_ERROR('IAS_NOTIFICATION','DBMS_SCHEDULER','SEND_NOTIFICATION_HEALTH',
      'IAS notification health report failed',SQLERRM);
    RAISE;
  END;

  PROCEDURE SEND_FINAL_REPORT_ISSUED(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,P_REFERENCE VARCHAR2,P_ENTITY VARCHAR2,P_PERIOD VARCHAR2,P_VERSION VARCHAR2,O_EMAIL_ID OUT NUMBER) IS V CLOB;
  BEGIN V:=ROW_HTML('Engagement',P_REFERENCE)||ROW_HTML('Entity',P_ENTITY)||ROW_HTML('Audit Period',P_PERIOD)||ROW_HTML('Report Version',P_VERSION);
    ENQUEUE('FINAL_REPORT_ISSUED',P_ENG_ID,NULL,P_MAIL_TO,P_MAIL_CC,P_REFERENCE,NULL,V,O_EMAIL_ID); END;

  PROCEDURE SEND_INQUIRY_ASSIGNED_UNIT(P_COMPLAINT_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,P_REFERENCE VARCHAR2,P_NATURE VARCHAR2,P_UNIT VARCHAR2,P_DUE_DATE VARCHAR2,P_DIRECTIONS CLOB,O_EMAIL_ID OUT NUMBER) IS V CLOB;
  BEGIN V:=ROW_HTML('Inquiry Reference',P_REFERENCE)||ROW_HTML('Inquiry Nature',P_NATURE)||ROW_HTML('Assigned Unit',P_UNIT)||ROW_HTML('Due Date',P_DUE_DATE)||ROW_HTML('Directions',DBMS_LOB.SUBSTR(P_DIRECTIONS,500,1));
    ENQUEUE('INQUIRY_ASSIGNED_UNIT',P_COMPLAINT_ID,NULL,P_MAIL_TO,P_MAIL_CC,P_REFERENCE,NULL,V,O_EMAIL_ID); END;

  PROCEDURE SEND_PASSWORD_RESET_SUCCESS(P_PPNO NUMBER,P_TEMP_PASSWORD VARCHAR2,P_MAIL_CC VARCHAR2,O_EMAIL_ID OUT NUMBER) IS V_TO VARCHAR2(320); V_NAME VARCHAR2(300); V CLOB;
  BEGIN
    SELECT EMAIL,TRIM(EMPLOYEEFIRSTNAME||' '||EMPLOYEELASTNAME) INTO V_TO,V_NAME FROM V_SERVICE_EMPLOYEEINFO WHERE PPNO=P_PPNO;
    V:=ROW_HTML('User',V_NAME)||ROW_HTML('Username',TO_CHAR(P_PPNO))||ROW_HTML('Temporary Password',P_TEMP_PASSWORD);
    ENQUEUE('PASSWORD_RESET_SUCCESS',P_PPNO,NULL,V_TO,P_MAIL_CC,TO_CHAR(P_PPNO),NULL,V,O_EMAIL_ID);
  END;

  -- PKG_SM procedures already return EMAIL/EMAIL_CC; pass those values directly.
  PROCEDURE SEND_AUDIT_SAMPLE_ISSUE(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,O_EMAIL_ID OUT NUMBER) IS V CLOB;
  BEGIN V:=ROW_HTML('Engagement ID',TO_CHAR(P_ENG_ID)); ENQUEUE('AUDIT_SAMPLE_ISSUE',P_ENG_ID,NULL,P_MAIL_TO,P_MAIL_CC,TO_CHAR(P_ENG_ID),NULL,V,O_EMAIL_ID); END;
  PROCEDURE SEND_AUDIT_EXCEPTION_ISSUE(P_ENG_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,O_EMAIL_ID OUT NUMBER) IS V CLOB;
  BEGIN V:=ROW_HTML('Engagement ID',TO_CHAR(P_ENG_ID)); ENQUEUE('AUDIT_EXCEPTION_ISSUE',P_ENG_ID,NULL,P_MAIL_TO,P_MAIL_CC,TO_CHAR(P_ENG_ID),NULL,V,O_EMAIL_ID); END;

  PROCEDURE SEND_AUDIT_CRITERIA_SUBMITTED(P_ENTITY_ID NUMBER,P_MAIL_TO VARCHAR2,P_MAIL_CC VARCHAR2,P_MESSAGE CLOB,O_EMAIL_ID OUT NUMBER) IS V CLOB;
  BEGIN V:=ROW_HTML('Entity ID',TO_CHAR(P_ENTITY_ID))||ROW_HTML('Message',DBMS_LOB.SUBSTR(P_MESSAGE,2000,1)); ENQUEUE('AUDIT_CRITERIA_SUBMITTED',P_ENTITY_ID,NULL,P_MAIL_TO,P_MAIL_CC,TO_CHAR(P_ENTITY_ID),NULL,V,O_EMAIL_ID); END;

  PROCEDURE SEND_AJAX_APPLICATION_ERROR(P_REFERENCE VARCHAR2,P_STATUS NUMBER,P_ENDPOINT VARCHAR2,P_ACTION VARCHAR2,P_USER_LABEL VARCHAR2,P_DETAILS CLOB,P_MAIL_TO VARCHAR2,O_EMAIL_ID OUT NUMBER) IS V CLOB;
  BEGIN V:=ROW_HTML('Reference',P_REFERENCE)||ROW_HTML('Status',TO_CHAR(P_STATUS))||ROW_HTML('Endpoint',P_ENDPOINT)||ROW_HTML('Action',P_ACTION)||ROW_HTML('User',P_USER_LABEL)||ROW_HTML('Details',DBMS_LOB.SUBSTR(P_DETAILS,2000,1)); ENQUEUE('AJAX_APPLICATION_ERROR',NULL,NULL,P_MAIL_TO,NULL,P_REFERENCE,TO_CHAR(P_STATUS),V,O_EMAIL_ID); END;
END PKG_IAS_NOTIFICATION;
/

--------------------------------------------------------------------------------

PROMPT Replacing PKG_AE with its complete checked-in specification/body.
create or replace package PKG_AE is
  TYPE t_cursor IS REF CURSOR;

  procedure P_GetAuditeeAssignedEntities(ENTITID   in number,
                                         io_cursor OUT t_cursor);

  procedure p_GetCOSORisks(io_cursor OUT t_cursor);

  procedure P_GetCCQsEntities(PPNO in number, io_cursor OUT t_cursor);

  procedure p_GetAssignedObservations(ENT_ID    in number,
                                      P_NO      in number,
                                      R_ID      in number,
                                      ENGID     in number,
                                      io_cursor OUT t_cursor);

  procedure P_GetObservationResponsible(OBSID     in number,
                                        E_ID      in number,
                                        io_cursor OUT t_cursor);

  procedure p_GetAssignedObservationstext(OBSID     in number,
                                          io_cursor OUT t_cursor);

  procedure P_GetAssignedObservationsForBranch(entityid  in number,
                                               io_cursor OUT t_cursor);

  procedure P_GetObservationText(OBS_ID    in number,
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor);

  procedure P_AUDITEE_OBSERVATION_RESPONSE(AUOBSID   IN number,
                                           REPLYDATA IN clob,
                                           REPLIEDBY IN number,
                                           OBSTEXTID IN number,
                                           REPLYROLE IN number,
                                           REMARKS   IN varchar2,
                                           SUBMITTED IN varchar2,
                                           ENT_ID    in number,
                                           P_NO      in number,
                                           R_ID      in number,
                                           io_cursor OUT t_cursor);

  procedure P_AUDITEE_OBSERVATION_RESPONSE_evidences(respid    in t_au_observations_auditee_evidences.respid%type,
                                                     AUOBSID   IN t_au_observations_auditee_evidences.memoid%type,
                                                     filename  IN t_au_observations_auditee_evidences.file_name%type,
                                                     filetype  IN t_au_observations_auditee_evidences.file_type%type,
                                                     length    in t_au_observations_auditee_evidences.length%type,
                                                     enteredby IN t_au_observations_auditee_evidences.enteredby%type,
                                                     filedata  IN t_au_observations_auditee_evidences.file_data%type,
                                                     sequence  IN t_au_observations_auditee_evidences.sequence%type,
                                                     text_id   in t_au_observations_auditee_evidences.text_id%type);

  procedure P_GetAuditeeOldParasFAD(EntityID  in number,
                                    io_cursor OUT t_cursor);

  procedure P_GetAuditeeAllParasFAD(ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    io_cursor OUT t_cursor);

  procedure P_UpdateOldParasStatus(PPNO       in number,
                                   PID        IN varchar2,
                                   NEW_STATUS in number);
  Procedure P_updateoldparamanagement(Paraid       in number,
                                      VCATID       in number,
                                      VCATNATUREID in number,
                                      RISKID       in number,
                                      ParaText     in clob,
                                      CREATEDBY    IN NUMBER,
                                      io_cursor    OUT t_cursor);

  procedure P_GetAuditeeOldParasentities(EntityID  in number,
                                         io_cursor OUT t_cursor);

  procedure P_GetAuditeeOldParasentitiesFAD(ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor);

  procedure P_GetAuditeeOldParas(ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor);

  procedure P_GetAuditeeOldParastext(paraid    in number,
                                     io_cursor OUT t_cursor);

  Procedure P_UpdateAuditeeOldParasresponse(Paraid    in number,
                                            cdate     in date,
                                            Text      in clob,
                                            PPNO      in number,
                                            Remarks   in clob,
                                            imprec    in varchar2,
                                            io_cursor OUT t_cursor);

  Procedure P_SubmitAuditeeOldParasresponse(Paraid    in number,
                                            io_cursor OUT t_cursor);

  procedure P_SubmitPostAuditCompliance_Evidence(TEXT_ID  in varchar2,
                                                 filename IN Varchar2,
                                                 len_id   in number,
                                                 enter_by IN number,
                                                 filetype in varchar2,
                                                 filedata IN clob,
                                                 seq_id   IN number);

  procedure P_GetPostAuditCompliance_Evidence(TEXT_ID   in varchar2,
                                              io_cursor OUT t_cursor);
  procedure P_GetPostAuditCompliance_Evidence_FileData(FILE_ID   in varchar2,
                                                       io_cursor OUT t_cursor);
  procedure P_GetParasForComplianceByAuditee(P_NO      in number,
                                             ENT_ID    in number,
                                             R_ID      in number,
                                             io_cursor OUT t_cursor);

  procedure P_GetParasForCompliancereview(P_NO      in number,
                                          ENT_ID    in number,
                                          R_ID      in number,
                                          io_cursor OUT t_cursor);

  procedure P_GetParasForComplianceByAuditee_text(Old_id    in number,
                                                  new_id    in number,
                                                  IND       in varchar2,
                                                  io_cursor OUT t_cursor);

  procedure P_GetParasForCompliancehistory(comp_id   in number,
                                           io_cursor OUT t_cursor);

  procedure P_GetParasForComplianceforhistory(c_cycle   in number,
                                              comp_id   in number,
                                              io_cursor OUT t_cursor);

  Procedure P_SubmitPostAuditCompliance(Old_id      number,
                                        new_id      number,
                                        ENT_ID      number,
                                        P_NO        number,
                                        R_ID        number,
                                        Auditee_COM clob,
                                        A_COMMENTS  varchar2,
                                        P_IND       varchar2,
                                        io_cursor   OUT t_cursor);

  Procedure P_SubmitPostAuditCompliance_Review(Old_id     number,
                                               new_id     number,
                                               ENT_ID     number,
                                               P_NO       number,
                                               R_ID       number,
                                               A_COMMENTS varchar2,
                                               P_IND      varchar2,
                                               io_cursor  OUT t_cursor);

  procedure P_GetComplianceByAuditee(EntityID  in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     ENT_ID    in Number,
                                     io_cursor OUT t_cursor);

  procedure p_GetParaComplianceResponsible(Old_id    in number,
                                           new_id    in number,
                                           IND       in varchar2,
                                           io_cursor OUT t_cursor);

  procedure P_GetOldParasForResponse(UserEntityID in number,
                                     entityId     in number,
                                     io_cursor    OUT t_cursor);

  procedure P_GetParasForComplianceByCAU(P_NO      in number,
                                         ENT_ID    in number,
                                         R_ID      in number,
                                         io_cursor OUT t_cursor);

  procedure P_GetrealtionshiptypeforCAU(io_cursor OUT t_cursor);

  procedure P_GetparentrepofficeforCAU(rid       in number,
                                       ENT_ID    in number,
                                       io_cursor OUT t_cursor);

  procedure P_GetchildpostingforCAU(P_ENT_ID  in number,
                                    io_cursor OUT t_cursor);

  Procedure P_FORWARD_CAU_PARA_TO_BRANCH(C_ID         number,
                                         ENT_ID       number,
                                         P_NO         number,
                                         R_ID         number,
                                         B_ENT_ID     number,
                                         CAU_COMMENTS varchar2,
                                         io_cursor    OUT t_cursor);

  procedure P_GetParasForComplianceByCAU_BY_BRANCH(P_NO      in number,
                                                   ENT_ID    in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor);

  procedure P_GetParasForCompliance_CAU_para_text(c_id      number,
                                                  IND       varchar2,
                                                  io_cursor OUT t_cursor);

  Procedure P_SubmitPostAuditCompliance_BY_BRANCH(C_ID        number,
                                                  T_ID        number,
                                                  ENT_ID      number,
                                                  P_NO        number,
                                                  R_ID        number,
                                                  Auditee_COM clob,
                                                  -- A_COMMENTS  varchar2,
                                                  io_cursor OUT t_cursor);

  procedure P_SubmitPostAuditCompliance_Evidence_By_BRANCH(TEXT_ID  in varchar2,
                                                           filename IN Varchar2,
                                                           len_id   in number,
                                                           enter_by IN number,
                                                           filetype in varchar2,
                                                           filedata IN clob,
                                                           seq_id   IN number);

  procedure P_GetAllCompliance_Evidence_CAU(TEXT_ID   in varchar2,
                                            io_cursor OUT t_cursor);
  procedure P_GetPostAuditCompliance_Evidence_FileData_CAU(FILE_ID   in varchar2,
                                                           io_cursor OUT t_cursor);

  procedure P_GetParasForComplianceByCAU_FOR_REVIEW(P_NO      in number,
                                                    ENT_ID    in number,
                                                    R_ID      in number,
                                                    io_cursor OUT t_cursor);

  Procedure p_GetPostAuditComplianceSecuritySnapshot(CM_ID     in number,
                                                     io_cursor OUT t_cursor);

  Procedure P_HasActiveUserContextAssignment(P_NO        in number,
                                             ENT_ID      in number,
                                             R_ID        in number,
                                             USER_CON_ID in number,
                                             io_cursor   OUT t_cursor);

end PKG_AE;
/
create or replace package body PKG_AE is


  procedure P_GetAuditeeAssignedEntities(ENTITID   in number,
                                         io_cursor OUT t_cursor) is

  begin
    if (ENTITID is not null) then
      open io_cursor for
        select distinct t.name || ' ( ' || e.operation_startdate || ' to ' ||
                        e.operation_enddate || ' )' as name,
                        t.code,
                        t.entity_id,
                        o.engplanid
          from t_au_observation o
         inner join t_au_plan_eng e
            on e.eng_id = o.engplanid
         inner join t_auditee_entities t
            on t.entity_id = e.entity_id
         inner join t_au_period p
            on e.period_id = p.auditperiodid
         inner join t_au_observation_assignedto ot
            on o.id = ot.obs_id
         where p.status_id = 2
           and (e.entity_id = ENTITID or ot.entity_id = ENTITID)
           and e.status < 14;
    else
      open io_cursor for
        select distinct t.name || ' ( ' || e.operation_startdate || ' to ' ||
                        e.operation_enddate || ' )' as name,
                        t.code,
                        t.entity_id,
                        o.engplanid
          from t_au_observation o
         inner join t_au_plan_eng e
            on e.eng_id = o.engplanid
         inner join t_auditee_entities t
            on t.entity_id = e.entity_id
         inner join t_au_period p
            on e.period_id = p.auditperiodid
         inner join t_au_observation_assignedto ot
            on o.id = ot.obs_id
         where p.status_id = 2
           and e.status < 14;
    end if;
  end P_GetAuditeeAssignedEntities;

  procedure p_GetCOSORisks(io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select * from T_COSO_RISK R order by R.R_ID;
  end p_GetCOSORisks;

  procedure P_GetCCQsEntities(PPNO in number, io_cursor OUT t_cursor) is

  begin
    if (PPNO is not null) then
      open io_cursor for
        select distinct e.code, e.name, e.entity_id
          from t_au_ccq c
         inner join t_auditee_entities e
            on e.entity_id = c.entity_id
         inner join t_user t
            on t.entity_id = e.auditby_id
         where t.ppno = PPNO;
    else
      open io_cursor for
        select distinct e.code, e.name, e.entity_id
          from t_au_ccq c
         inner join t_auditee_entities e
            on e.entity_id = c.entity_id;
    end if;
  end P_GetCCQsEntities;

  procedure p_GetAssignedObservations(ENT_ID    in number,
                                      P_NO      in number,
                                      R_ID      in number,
                                      ENGID     in number,
                                      io_cursor OUT t_cursor) is
  begin

    OPEN io_Cursor FOR
      select o.id as OBS_ID,
             ot.id as OBS_TEXT_ID,
             O.MEMO_NUMBER AS MEMO_NUMBER,
             o.Memo_Date,
             NVL(o.replydate, null) as replydate,
             ot.headings as gist,
             nvl(ob.id, 0) as response_id,
             t.entity_id,
             ee.name as entity_name,
             s.statusname as STATUS,
             o.STATUS as STATUS_ID,
             pe.description as Audit_year,
             (case
               when o.entity_id = ENT_ID and o.status = 2 then
                1
               else
                (case
                  when o.entity_id = ENT_ID and o.status < 8 and o.status != 23 then
                   2
                  else
                   0
                end)

             end) as canreply,
             (case
               when o.status < 8 and o.status != 23 then
                1
               else
                0
             end) as editable
        from t_au_observation_assignedto t
       inner join t_au_observation o
          on o.id = t.obs_id
       inner join t_au_observation_status s
          on o.status = s.statusid
       inner join t_au_plan_eng ep
          on ep.eng_id = o.engplanid
       inner join t_au_period pe
          on pe.auditperiodid = ep.period_id
       inner join t_auditee_entities ee
          on ee.entity_id = t.entity_id
        LEFT join t_au_observations_auditee_response ob
          on ob.au_obs_id = o.id
       inner join t_au_observation_text ot
          on o.id = ot.observatsion_id
       WHERE ENT_ID = case
               when ep.entity_type = 25 then
                ENT_ID
               else
                t.entity_id
             end
         and ep.eng_id = engid
       order by t.OBS_ID asc;
  end p_GetAssignedObservations;
  procedure P_GetObservationResponsible(OBSID     in number,
                                        E_ID      in number,
                                        io_cursor OUT t_cursor) is

  begin
    OPEN io_Cursor FOR
      select ot.resp_row_id,
             ot.pp_no,
             em.EMPLOYEEFIRSTNAME || '  ' || em.EMPLOYEELASTNAME as EMP_NAME,
             ot.LOAN_CASE as LOANCASE,
             ot.lc_amount as LCAMOUNT,
             ot.account_number as ACCNUMBER,
             ot.ac_amount as ACAMOUNT
        from v_get_auditee_pp_responsibility ot
       inner join v_service_employeeinfo em
          on em.PPNO = ot.pp_no
       where ot.au_obs_id = OBSID;

  end P_GetObservationResponsible;

  procedure p_GetAssignedObservationstext(OBSID     in number,
                                          io_cursor OUT t_cursor) is

  begin
    OPEN io_Cursor FOR
      select O.MEMO_NUMBER AS MEMO_NUMBER,
             ot.text       as OBSERVATION_TEXT,
             ot.headings   as OBSERVATION_TEXT_PLAIN,
             ob.reply      as replytext,
             o.id          as resp_id
        from t_au_observation_assignedto t
       inner join t_au_observation o
          on o.id = t.obs_id
       inner join t_au_observation_text ot
          on ot.id = t.obs_text_id
        LEFT join t_au_observations_auditee_response ob
          on ob.au_obs_id = o.id
       WHERE o.id = OBSID
       order by t.OBS_ID asc;

  end p_GetAssignedObservationstext;

  procedure P_GetAssignedObservationsForBranch(entityid  in number,
                                               io_cursor OUT t_cursor) is

  begin
    OPEN io_Cursor FOR
      select vc.v_name              as Violation,
             vcs.sub_v_name         AS NATURE,
             o.Memo_Date,
             o.replydate,
             t.*,
             ot.text                as OBSERVATION_TEXT,
             ot.headings            as OBSERVATION_TEXT_PLAIN,
             ob.reply               as replytext,
             s.statusname           as STATUS,
             o.STATUS               as STATUS_ID,
             e.name                 AS ENTITY_NAME,
             ep.audit_startdate     as AUDIT_STARTDATE,
             ep.audit_enddate       as AUDIT_ENDDATE,
             pe.description         as Audit_year,
             ob.id                  as resp_id,
             ep.operation_startdate,
             ep.operation_enddate
        from t_au_observation_assignedto t
       inner join t_au_observation o
          on o.id = t.obs_id
       inner join t_au_observation_text ot
          on ot.id = t.obs_text_id
       inner join t_au_observation_status s
          on o.status = s.statusid
       inner join t_auditee_entities e
          on e.entity_id = t.ENTITY_ID
       inner join t_control_violation vc
          on o.v_cat_id = vc.id
       inner join t_control_violation_sub vcs
          on o.v_cat_nature_id = vcs.id
       inner join t_au_plan_eng ep
          on ep.entity_id = e.entity_id
       inner join t_au_period pe
          on pe.auditperiodid = ep.period_id
        left join t_au_observations_auditee_response ob
          on ob.au_obs_id = o.id

       WHERE o.entity_id = entityid
       order by t.OBS_ID asc;

  end P_GetAssignedObservationsForBranch;

  procedure P_GetObservationText(OBS_ID    in number,
                                 ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor) is

    Z_B number := 0;
  begin

    commit;
    commit;

    OPEN io_Cursor FOR
      select ot.text, o.reference_id
        from T_AU_OBSERVATION_TEXT ot
       inner join t_au_observation o
          on o.id = ot.observatsion_id
       where ot.OBSERVATSION_ID = OBS_ID;

  end P_GetObservationText;

  procedure P_AUDITEE_OBSERVATION_RESPONSE(AUOBSID   IN number,
                                           REPLYDATA IN clob,
                                           REPLIEDBY IN number,
                                           OBSTEXTID IN number,
                                           REPLYROLE IN number,
                                           REMARKS   IN varchar2,
                                           SUBMITTED IN varchar2,
                                           ENT_ID    in number,
                                           P_NO      in number,
                                           R_ID      in number,
                                           io_cursor OUT t_cursor) is
    Z_B             number := 0;
    Already_Replied number := 0;
  begin

    commit;
    commit;

    select NVL(MAX(l.id), 0)
      into Already_Replied
      from t_au_observations_auditee_response l
     where l.au_obs_id = AUOBSID;
    if (Already_Replied = 0) THEN
      INSERT INTO T_AU_OBSERVATIONS_AUDITEE_RESPONSE o
        (o.ID,
         o.AU_OBS_ID,
         o.REPLY,
         o.REPLIEDBY,
         o.REPLIEDDATE,
         o.OBS_TEXT_ID,
         o.REPLY_ROLE,
         o.REMARKS,
         o.SUBMITTED)
      VALUES
        ((select COALESCE(max(acc.ID) + 1, 1)
           from T_AU_OBSERVATIONS_AUDITEE_RESPONSE acc),
         AUOBSID,
         REPLYDATA,
         REPLIEDBY,
         trunc(SYSDATE),
         OBSTEXTID,
         REPLYROLE,
         REMARKS,
         SUBMITTED);

      commit;

      UPDATE T_AU_OBSERVATION_ASSIGNEDTO
         SET REPLIED = 'Y'
       WHERE OBS_ID = AUOBSID
         and OBS_TEXT_ID = OBSTEXTID;
      commit;
      UPDATE t_au_observation T
         SET t.STATUS          = 3,
             T.LASTREPLYBY     = REPLIEDBY,
             t.MEMO_REPLY_DATE = trunc(SYSDATE)
       WHERE ID = AUOBSID;

    ELSE
      UPDATE T_AU_OBSERVATIONS_AUDITEE_RESPONSE AR
         SET AR.REPLY       = REPLYDATA,
             AR.REPLIEDBY   = REPLIEDBY,
             AR.REPLIEDDATE = trunc(SYSDATE)
       WHERE AR.AU_OBS_ID = AUOBSID;
      commit;
    END IF;

    open io_cursor for
      select r.au_obs_id as ob_id, r.id as resp_id
        from T_AU_OBSERVATIONS_AUDITEE_RESPONSE r
       where r.au_obs_id = AUOBSID;
    commit;

  end P_AUDITEE_OBSERVATION_RESPONSE;

  procedure P_AUDITEE_OBSERVATION_RESPONSE_evidences(respid    in t_au_observations_auditee_evidences.respid%type,
                                                     AUOBSID   IN t_au_observations_auditee_evidences.memoid%type,
                                                     filename  IN t_au_observations_auditee_evidences.file_name%type,
                                                     filetype  IN t_au_observations_auditee_evidences.file_type%type,
                                                     length    in t_au_observations_auditee_evidences.length%type,
                                                     enteredby IN t_au_observations_auditee_evidences.enteredby%type,
                                                     filedata  IN t_au_observations_auditee_evidences.file_data%type,
                                                     sequence  IN t_au_observations_auditee_evidences.sequence%type,
                                                     text_id   in t_au_observations_auditee_evidences.text_id%type) is
  begin

    insert into t_au_observations_auditee_evidences
      (id,
       file_name,
       file_type,
       length,
       file_data,
       memoid,
       enteredby,
       entereddate,
       sequence,
       status,
       text_id,
       respid)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from t_au_observations_auditee_evidences acc),
       filename,
       filetype,
       length,
       filedata,
       AUOBSID,
       enteredby,
       trunc(SYSDATE),
       sequence,
       'Y',
       text_id,
       respid);
    commit;

  end P_AUDITEE_OBSERVATION_RESPONSE_evidences;
  -- Ali & Asfand from here
  procedure P_GetAuditeeOldParasFAD(EntityID  in number,
                                    io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select f.audit_period,
             f.name,
             f.para_no,
             f.id,
             f.au_obs_id,
             f.ref_p,
             f.gist_of_paras,
             f.vol_i_ii,
             f.amount,
             f.para_category,
             f.audited_by

        from v_get_P_GetAuditeeOldParasFAD f
       where f.ENTITY_ID = EntityID
       order by f.audit_period, f.para_no;
    --and f.c_status ;

  end P_GetAuditeeOldParasFAD;
  -- auditee portal field

  procedure P_GetAuditeeAllParasFAD(ENT_ID    in number,
                                    P_NO      in number,
                                    R_ID      in number,
                                    io_cursor OUT t_cursor) as

    Z_R number := 0;
  begin

    commit;
    commit;

    OPEN io_cursor FOR
      select f.audit_period,
             e.name,
             f.para_no,
             f.id,
             f.ref_p,
             f.gist_of_paras,
             f.vol_i_ii,
             fd.remarks        as reviewer_remarks,
             f.amount_involved as amount
        FROM t_au_old_paras_fad f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
        left join v_get_P_GetAuditeeOldParasFAD_ref fd
          on fd.ref_p = f.ref_p --and fd.c_status IN (12)
       WHERE fd.audited_by = ENT_ID

       order by f.audit_period, e.name, f.para_no;

  end P_GetAuditeeAllParasFAD;

  procedure P_UpdateOldParasStatus(PPNO       in number,
                                   PID        IN varchar2,
                                   NEW_STATUS in number) as
  begin

    UPDATE T_AU_OLD_PARAS_FAD al
       SET al.Para_Status = NEW_STATUS, al.parastatusupdatedby = PPNO
     WHERE al.ref_p = PID;
    commit;
  end P_UpdateOldParasStatus;

  Procedure P_updateoldparamanagement(Paraid       in number,
                                      VCATID       in number,
                                      VCATNATUREID in number,
                                      RISKID       in number,
                                      ParaText     in clob,
                                      CREATEDBY    IN NUMBER,
                                      io_cursor    OUT t_cursor) is

  begin
    If (VCATID is null or VCATNATUREID is null or RISKID is null) then
      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 9;
    else
      update T_AU_OBSERVATION_OLD_CAD_PARAS t
         set t.v_cat_id        = VCATID,
             t.v_cat_nature_id = VCATNATUREID,
             t.risk_id         = RISKID,
             T.ENTERED_BY      = CREATEDBY,
             T.ENTERED_ON      = SYSDATE,
             t.status          = 1
       where t.para_id = paraid;
      commit;
      insert into T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT
        (ID, OBSERVATSION_ID, TEXT, ENTEREDBY, ENTEREDDATE)
      values
        ((select COALESCE(max(acc.ID) + 1, 1)
           from T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT acc),
         paraid,
         paratext,
         CREATEDBY,
         sysdate);
      commit;

      open io_cursor for
        select r.ref, r.remarks from t_au_remarks r where r.id = 15;
    end if;
  end P_updateoldparamanagement;

  procedure P_GetAuditeeOldParasentities(EntityID  in number,
                                         io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select distinct e.entity_id, e.name
        from T_AU_OBSERVATION_OLD_CAD_PARAS s
       inner join t_auditee_entities e
          on s.entity_id = e.entity_id
      --   WHERE e.entity_id = EntityID
       order by e.name;

  end P_GetAuditeeOldParasentities;

  procedure P_GetAuditeeOldParasentitiesFAD(ENT_ID    in number,
                                            P_NO      in number,
                                            R_ID      in number,
                                            io_cursor OUT t_cursor) as

    Z_B number := 0;
  begin

    commit;
    commit;

    OPEN io_cursor FOR
      select distinct s.entity_id, s.name
        from v_get_P_GetAuditeeOldParasentitiesFAD s
       WHERE s.entity_id = ENT_ID
       order by s.name;

  end P_GetAuditeeOldParasentitiesFAD;

  procedure P_GetAuditeeOldParas(ENT_ID    in number,
                                 P_NO      in number,
                                 R_ID      in number,
                                 io_cursor OUT t_cursor) as

    Z_B number := 0;
  begin

    commit;
    commit;
    OPEN io_cursor FOR
      select e.name as ENTITY_NAME,
             s.para_id as id,
             s.entity_id as ENTITY_CODE,
             '2022' as AUDIT_PERIOD,
             s.para_no,
             s.gist_of_paras,
             'A' as AUDITEE_RESPONSE,
             'A' as AUDITOR_REMARKS,
             sysdate as DATE_OF_LAST_COMPLIANCE_RECEIVED,
             s.audited_by,
             '14' as TYPE_ID,
             'Department' as entitytypedesc,
             v.v_name,
             vs.sub_v_name,
             r.description as risk
        from T_AU_OBSERVATION_OLD_CAD_PARAS s
       inner join t_auditee_entities e
          on s.entity_id = e.entity_id
       inner join t_control_violation v
          on s.v_cat_id = v.id
       inner join t_control_violation_sub vs
          on vs.id = s.v_cat_nature_id
       inner join t_coso_risk r
          on r.r_id = vs.risk_id
       WHERE e.entity_id = ENT_ID
         and s.status = 2
       order by s.Entity_Name, s.para_no;

  end P_GetAuditeeOldParas;

  procedure P_GetAuditeeOldParastext(paraid    in number,
                                     io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select t.*
        from t_au_observation_old_cad_paras_text t
       where t.observatsion_id = paraid
       order by t.id;

  end P_GetAuditeeOldParastext;

  Procedure P_UpdateAuditeeOldParasresponse(Paraid    in number,
                                            cdate     in date,
                                            Text      in clob,
                                            PPNO      in number,
                                            Remarks   in clob,
                                            imprec    in varchar2,
                                            io_cursor OUT t_cursor) is
    V_F number := 0;
  begin
    select nvl(max(r.replyno), 0)
      into V_F
      from T_AU_OBSERVATION_OLD_CAD_PARAS_Response r
     where r.au_obs_id = paraid;
    insert into T_AU_OBSERVATION_OLD_CAD_PARAS_Response
      (Id,
       replyno,
       Au_Obs_Id,
       Reply,
       Repliedby,
       Replieddate,
       C_I_Remarks,
       imp_recommendation,
       status)
    VALUES
      ((select COALESCE(max(acc.ID) + 1, 1)
         from T_AU_OBSERVATION_OLD_CAD_PARAS_Response acc),
       (V_F + 1),
       Paraid,
       Text,
       PPNO,
       cdate,
       Remarks,
       imprec,
       '1');
    commit;
    update t_au_observation_old_cad_paras t
       set t.status = 2
     where t.para_id = Paraid;
    commit;
    open io_cursor for
      select r.ref, r.remarks from t_au_remarks r where r.id = 15;

  end P_UpdateAuditeeOldParasresponse;

  Procedure P_SubmitAuditeeOldParasresponse(Paraid    in number,
                                            io_cursor OUT t_cursor) is

  begin
    update T_AU_OBSERVATION_OLD_CAD_PARAS_Response t
       set t.status = 2
     where t.au_obs_id = paraid;
    commit;
    open io_cursor for
      select r.ref, r.remarks from t_au_remarks r where r.id = 15;

  end P_SubmitAuditeeOldParasresponse;

  procedure P_GetParasForComplianceByAuditee(P_NO      in number,
                                             ENT_ID    in number,
                                             R_ID      in number,
                                             io_cursor OUT t_cursor) as

  begin
    if (ENT_ID in (113176, 113182)) then
      OPEN io_cursor FOR
        select C.AUDIT_PERIOD,
               c.name,
               C.PARA_NO,
               C.NEW_PARAID AS NEW_PARA_ID,
               C.OLD_PARA_ID,
               C.GIST_OF_PARAS,
               C.AUDITBY_ID,
               '' AS AUDIT_REPLY,
               c.REC_FROM,
               C.NEXT_R_ID,
               C.PER_R_ID,
               C.C_STATUS_UP,
               C.C_STATUS_DOWN,
               c.IND,
               c.Rsk as Risk,
               c.com_id,
               C.AUDIT_PERIOD,
               C.START_DATE || ' - ' || C.END_DATE AS AUDIT_DATE
          FROM V_GET_AIS_POST_COMPLIANCE C
         inner join t_auditee_entities_maping m
            on m.entity_id = c.entity_id
         where ((ENT_ID = 113176 AND
               c.ENTITY_ID IN (113176, 113173, 112935, 112933)) OR
               (ENT_ID = 113182 AND c.ENTITY_ID IN (113182, 113105)))
           and C.com_stage = R_ID
           and C.com_status != 16
         order by C.audit_period desc, C.para_no asc;
    else
      OPEN io_cursor FOR
        select C.AUDIT_PERIOD,
               c.name,
               C.PARA_NO,
               C.NEW_PARAID AS NEW_PARA_ID,
               C.OLD_PARA_ID,
               C.GIST_OF_PARAS,
               C.AUDITBY_ID,
               '' AS AUDIT_REPLY,
               c.REC_FROM,
               C.NEXT_R_ID,
               C.PER_R_ID,
               C.C_STATUS_UP,
               C.C_STATUS_DOWN,
               c.IND,
               c.Rsk as Risk,
               c.com_id,
               C.AUDIT_PERIOD,
               C.START_DATE || ' - ' || C.END_DATE AS AUDIT_DATE
          FROM V_GET_AIS_POST_COMPLIANCE C
         inner join t_auditee_entities_maping m
            on m.entity_id = c.entity_id
          left join t_auditee_entities_maping_com e
            on e.com_key = c.com_key
         where c.ENTITY_ID = ENT_ID
           and C.com_stage = ---CASE ADDED because GM USERS can't view their own PARAS ( CASES SHOULD BE ADDED FOR OTHER USERS AS WELL )
               (case
                 when R_ID = 39 then
                  21
                 else
                  R_ID
               end)
           and C.com_status != 16
         order by C.audit_period desc, C.para_no asc;
    end if;
  end P_GetParasForComplianceByAuditee;

  procedure P_GetParasForCompliancereview(P_NO      in number,
                                          ENT_ID    in number,
                                          R_ID      in number,
                                          io_cursor OUT t_cursor) as

    N_F number := 0;
  begin

    select NVL(max(m.ppno), 0)
      into N_F
      from t_user_context_assignment m

     where m.ppno = P_NO
       and m.entity_id = ent_id
       and m.role_id = R_ID
       and R_ID != 13;

    if (N_F != 0) then
      IF (R_ID in (41)) then
        OPEN io_cursor FOR
          select C.AUDIT_PERIOD,
                 '' AS NAME,
                 C.PARA_NO,
                 C.NEW_PARAID AS NEW_PARA_ID,
                 C.OLD_PARA_ID,
                 C.GIST_OF_PARAS,
                 C.AUDITBY_ID,
                 '' AS AUDIT_REPLY,
                 c.REC_FROM,
                 C.NEXT_R_ID,
                 C.PER_R_ID,
                 C.C_STATUS_UP,
                 C.C_STATUS_DOWN,
                 c.IND,
                 c.Rsk,
                 c.Rsk as Risk,
                 c.com_id,
                 C.START_DATE AS AUDIT_START_DATE,
                 C.END_DATE AS AUDIT_END_DATE,
                 c.OPS_START_DATE,
                 c.OPS_END_DATE
            FROM V_GET_AIS_POST_COMPLIANCE C
           inner join t_auditee_entities_maping m
              on m.entity_id = c.entity_id
           where c.com_stage = R_ID
             and c.com_status != 16
          -- and trunc(c.STELLED_ON) between '01-Jan-2025' and '31-Dec-25'
           order by c.audit_period desc, c.para_no asc;

      else

        IF (R_ID in (43, 44)) then
          OPEN io_cursor FOR
            select C.AUDIT_PERIOD,
                   '' AS NAME,
                   C.PARA_NO,
                   C.NEW_PARAID AS NEW_PARA_ID,
                   C.OLD_PARA_ID,
                   C.GIST_OF_PARAS,
                   C.AUDITBY_ID,
                   '' AS AUDIT_REPLY,
                   c.REC_FROM,
                   C.NEXT_R_ID,
                   C.PER_R_ID,
                   C.C_STATUS_UP,
                   C.C_STATUS_DOWN,
                   c.IND,
                   c.Rsk,
                   c.Rsk as Risk,
                   c.com_id,
                   C.START_DATE AS AUDIT_START_DATE,
                   C.END_DATE AS AUDIT_END_DATE,
                   c.OPS_START_DATE,
                   c.OPS_END_DATE
              FROM V_GET_AIS_POST_COMPLIANCE C
             inner join t_auditee_entities_maping m
                on m.entity_id = c.entity_id
             inner join t_auditee_entities_maping_com e
                on e.com_key = c.com_key
             where P_NO = case
                     when R_ID = 43 then
                      e.reviewer_ppno
                     when r_id = 44 then
                      e.approver_ppno
                     WHEN r_id = 41 THEN
                      p_no
                   end
               and c.com_stage = R_ID
               and c.com_status != 16
             order by c.audit_period desc, c.para_no asc;

        else
          OPEN io_cursor FOR
            select C.AUDIT_PERIOD,
                   '' AS NAME,
                   C.PARA_NO,
                   C.NEW_PARAID AS NEW_PARA_ID,
                   C.OLD_PARA_ID,
                   C.GIST_OF_PARAS,
                   C.AUDITBY_ID,
                   '' AS AUDIT_REPLY,
                   C.REC_FROM,
                   C.NEXT_R_ID,
                   C.PER_R_ID,
                   C.C_STATUS_UP,
                   C.C_STATUS_DOWN,
                   c.IND,
                   c.Rsk,
                   c.Rsk as Risk,
                   c.com_id,
                   C.START_DATE AS AUDIT_START_DATE,
                   C.END_DATE AS AUDIT_END_DATE,
                   c.OPS_START_DATE,
                   c.OPS_END_DATE
              FROM V_GET_AIS_POST_COMPLIANCE C
             inner join t_auditee_entities_maping m
                on m.entity_id = c.entity_id
              left join t_auditee_entities_maping_com e
                on e.com_key = c.com_key
             where Ent_ID in case
                     when R_ID in (21, 45) then
                      m.parent_id
                     when R_ID in (2, 9, 7, 6, 15, 16, 11) then
                      c.auditby_id
                   end
               and c.com_stage = R_ID
               and c.com_status != 16
             order by c.audit_period desc, c.para_no asc;
        end if;
      end if;
    else

      open io_cursor for
        select 'System Issue, Please contact System Administrator on 051-2002110' as remarks
          from dual;
    end if;

  end P_GetParasForCompliancereview;

  procedure P_GetParasForComplianceByAuditee_text(Old_id    in number,
                                                  new_id    in number,
                                                  IND       in varchar2,
                                                  io_cursor OUT t_cursor) as

  begin
    if (IND = 'O') then
      OPEN io_cursor FOR
        select f.gist_of_paras, t.para_text as text, t.id as text_id
          from t_au_old_paras_fad f
          left join t_au_old_paras_fad_text t
            on t.ref_p = f.ref_p
         where f.id = Old_id;
    else
      if (IND = 'A') then
        OPEN io_cursor FOR
          select at.headings as gist_of_paras, at.text, at.id as text_id
            from t_au_observation af
           inner join t_au_observation_text at
              on at.observatsion_id = af.id
           where af.id = new_id;
      else
        if (IND = 'C') then
          OPEN io_cursor FOR
            select cf.gist_of_paras, ct.text, ct.id as text_id
              from t_au_observation_old_cad_paras cf
             inner join t_au_observation_old_cad_paras_text ct
                on cf.para_id = ct.observatsion_id
             where cf.para_id = new_id;
        end if;
      end if;
    end if;

  end P_GetParasForComplianceByAuditee_text;

  procedure P_GetParasForCompliancehistory(comp_id   in number,
                                           io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select f.hist_id,
             f.com_id,
             f.com_cycle,
             f.com_status,
             f.com_stage,
             f.comment_by_role,
             f.comment_by_ppno as PP_NO,
             e.employeefirstname || ' ' || e.employeelastname as name,
             f.comment_on,
             f.comments,
             f.com_flow

        from AIS_T_AU_POST_COMPLIANCE_HISTORY f
        left join v_service_employeeinfo e
          on e.ppno = f.comment_by_PPNO
       where f.com_id = comp_id
       order by f.com_cycle, f.comment_on, f.comment_on asc;

  end P_GetParasForCompliancehistory;

  procedure P_GetParasForComplianceforhistory(c_cycle   in number,
                                              comp_id   in number,
                                              io_cursor OUT t_cursor) as
  begin
    open io_cursor for
      select t.reply,
             t.c_txt_id as text_id,
             (case
               when o.IND = 'A' then
                pt.text
               else
                (case
                  when o.IND = 'C' then
                   ct.text
                  else
                   (case
                     when o.IND = 'O' then
                      ft.para_text
                   end)
                end)
             end) as para_text
        from AIS_T_AU_POST_COMPLIANCE_text t
       inner join AIS_T_AU_POST_COMPLIANCE p
          on t.com_id = p.com_id
       inner join T_AU_OBSERVATION_FAD o
          on (o.new_paraid = p.new_para_id or o.old_para_id = p.old_para_id)
        left join t_au_observation_text pt
          on o.new_paraid = pt.observatsion_id
         and o.IND = 'A'
        left join t_au_old_paras_fad_text ft
          on ft.ref_p = o.Old_para_ref
        left join t_au_observation_old_cad_paras_text ct
          on ct.observatsion_id = o.new_paraid
         and o.IND = 'C'
       where t.com_id = comp_ID
         and t.com_cycle = c_cycle;
  end P_GetParasForComplianceforhistory;

  Procedure P_SubmitPostAuditCompliance(Old_id      number,
                                        new_id      number,
                                        ENT_ID      number,
                                        P_NO        number,
                                        R_ID        number,
                                        Auditee_COM clob,
                                        A_COMMENTS  varchar2,
                                        P_IND       varchar2,
                                        io_cursor   OUT t_cursor) as
    P_F varchar2(50);

    cursor V is
      select C.AUDIT_PERIOD,
             c.name,
             C.PARA_NO,
             C.NEW_PARAID,
             C.OLD_PARA_ID,
             C.GIST_OF_PARAS,
             C.AUDITBY_ID,
             c.REC_FROM,
             C.NEXT_R_ID,
             C.PER_R_ID,
             C.C_STATUS_UP,
             C.C_STATUS_DOWN,
             c.IND,
             c.com_CYCLE,
             c.com_id,
             c.com_stage,
             C.START_DATE || ' - ' || C.END_DATE AS AUDIT_DATE
        FROM V_GET_AIS_POST_COMPLIANCE C
       inner join t_auditee_entities_maping m
          on m.entity_id = c.entity_id
        left join t_auditee_entities_maping_com e
          on e.com_key = c.com_key
       where (C.OLD_PARA_ID = Old_id or
             (C.NEW_PARAID = new_id and c.IND = P_F));

    Vr1 V%rowtype;
    V_F number := 0;
     text_id number;
    Z_B number := 0;
    N_F number := 0;
    C_F number := 0;
  begin
    select NVL(max(cad.para_id), 0)
      into C_F
      from t_au_observation_old_cad_paras cad
     where cad.para_id = new_id;
    --and cad.entity_id = ENT_ID;
    select (case
             when et.audit_type = 'B' then
              'A'
             else
              (case
                when C_F != 0 then
                 'C'
                else
                 'A'
              end)
           end)
      into P_F
      from t_auditee_entities en
     inner join t_auditee_ent_types et
        on en.type_id = et.autid
     where en.entity_id = ENT_ID;

    Open V;
    Fetch V
      into vr1;
    Close v;

    IF ((R_ID = vr1.com_stage) OR (R_ID IN (39))) THEN
      select NVL(max(m.ppno), 0)
        into N_F
       from t_user_context_assignment m

       where m.ppno = P_NO
         and m.entity_id = ENT_ID
         and m.role_id = R_ID;

      if (N_F != 0) then
        commit;

        commit;

        update AIS_T_AU_POST_COMPLIANCE c
           set c.com_cycle  = vr1.com_cycle + 1,
               c.com_stage  = vr1.next_r_id,
               c.com_status = 10
         where c.com_id = vr1.com_id;
        commit;

        update AIS_T_AU_POST_COMPLIANCE_text t
           set t.status = 'N'
         where t.com_id = vr1.com_id
           and t.com_cycle = vr1.com_cycle;
        commit;

        select max(ts.c_txt_id + 1)
          into V_F
          from AIS_T_AU_POST_COMPLIANCE_text ts;

        insert into AIS_T_AU_POST_COMPLIANCE_text
          (c_Txt_Id, Com_Id, Reply, Com_Cycle, Status)
        values
          (V_F, vr1.com_id, Auditee_COM, vr1.com_cycle + 1, 'Y');
        commit;

        insert into AIS_T_AU_POST_COMPLIANCE_HISTORY
          (HIST_ID,
           COM_ID,
           COM_CYCLE,
           COM_STATUS,
           COM_STAGE,
           COMMENT_BY_ROLE,
           COMMENT_BY_PPNO,
           COMMENT_ON,
           COMMENTS,
           COM_FLOW)
        values
          ((select COALESCE(max(ct.HIST_ID) + 1, 1)
             From AIS_T_AU_POST_COMPLIANCE_HISTORY ct),
           vr1.com_id,
           vr1.com_cycle + 1,
           10,
           R_ID,
           (select g.group_name from t_groups g where g.group_id = R_ID),
           P_NO,
           sysdate,
           'Compliance Submitted',
           'Y');
        commit;

        open io_cursor for
          select 'Complaince Submitted' as remarks, V_F as text_id
            from dual;

      else
        open io_cursor for
          select 'Sorry you connection is lost, logout and login again' as remarks,
                 0 as text_id
            from dual;
      end if;

    else
      open io_cursor for
        select 'Compliance Already Submitted, If you face the same issue, Contact on  051-2002110' as remarks,
               0 as text_id
          from dual;
    end if;

  end P_SubmitPostAuditCompliance;

  procedure P_SubmitPostAuditCompliance_Evidence(TEXT_ID  in varchar2,
                                                 filename IN Varchar2,
                                                 len_id   in number,
                                                 enter_by IN number,
                                                 filetype in varchar2,
                                                 filedata IN clob,
                                                 seq_id   IN number) as

    C_M          NUMBER;
    PREV_TEXT_ID NUMBER;
  BEGIN
    -- Get comp_id for the given text_id
    SELECT t.com_id
      INTO C_M
      FROM ais_t_au_post_compliance_text t
     WHERE t.c_txt_id = TEXT_ID;

    -- Find the immediately previous text_id for the same comp_id
    SELECT MAX(c_txt_id)
      INTO PREV_TEXT_ID
      FROM ais_t_au_post_compliance_text
     WHERE com_id = C_M
       AND c_txt_id < TEXT_ID;

    insert into AIS_T_AU_POST_COMPLIANCE_EVIDENCE_archive
      select *
        from ais_t_au_post_compliance_evidence c
       where c.comp_id = C_M
         and c.textid = PREV_TEXT_ID;
    commit;
    -- Delete evidence for the previous text_id, if it exists
    IF PREV_TEXT_ID IS NOT NULL THEN
      DELETE FROM ais_t_au_post_compliance_evidence
       WHERE textid = PREV_TEXT_ID;
      commit;
    END IF;

    insert into ais_t_au_post_compliance_evidence
      (id,
       file_name,
       length,
       file_data,
       textid,
       enteredby,
       entereddate,
       sequence,
       description,
       COMP_ID)

    VALUES
      ((select COALESCE(max(ac.ID) + 1, 1)
         from ais_t_au_post_compliance_evidence ac),
       filename,
       len_id,
       filedata,
       TEXT_ID,
       enter_by,
       sysdate,
       seq_id,
       filetype,
       C_M);
    commit;

  end P_SubmitPostAuditCompliance_Evidence;

  PROCEDURE P_SubmitPostAuditCompliance_Review(Old_id     NUMBER,
                                               new_id     NUMBER,
                                               ENT_ID     NUMBER,
                                               P_NO       NUMBER,
                                               R_ID       NUMBER,
                                               A_COMMENTS VARCHAR2,
                                               P_IND      VARCHAR2,
                                               io_cursor  OUT t_cursor) AS

    ------------------------------------------------------------------
    -- Local variables
    ------------------------------------------------------------------
    P_F VARCHAR2(50);
    N_F NUMBER := 0;
    Z_B NUMBER := 0;
    C_F NUMBER := 0;

    v_current_stage NUMBER;
    v_savepoint_set BOOLEAN := FALSE;
    v_history_id NUMBER;
    v_notification_email_id NUMBER;

    ------------------------------------------------------------------
    -- Compliance cursor
    ------------------------------------------------------------------
    CURSOR V IS
      SELECT C.AUDIT_PERIOD,
             C.NAME,
             C.PARA_NO,
             C.NEW_PARAID,
             C.OLD_PARA_ID,
             C.GIST_OF_PARAS,
             C.AUDITBY_ID,
             C.REC_FROM,
             ET.EMAIL_ADDRESS AS TO_EMAIL,
             AD.EMAIL_ADDRESS AS CC_EMAIL,
             MT.EMAIL_ADDRESS AS CC_EMAIL2,

             CASE
               WHEN P_IND = 'U' THEN
                C.NEXT_R_ID
               ELSE
                C.PER_R_ID
             END AS ROLE_ID,

             CASE
               WHEN P_IND = 'U' THEN
                C.C_STATUS_UP
               ELSE
                C.C_STATUS_DOWN
             END AS STATUS_ID,

             C.IND,
             C.COM_STAGE,
             C.COM_CYCLE,
             C.COM_ID AS COMID,
             C.START_DATE || ' - ' || C.END_DATE AS AUDIT_DATE

       FROM V_GET_AIS_POST_COMPLIANCE C

       INNER JOIN T_AUDITEE_ENTITIES_MAPING M
          ON M.ENTITY_ID = C.ENTITY_ID

       INNER JOIN T_AUDITEE_ENTITIES ET
          ON ET.ENTITY_ID = M.ENTITY_ID
       INNER JOIN T_AUDITEE_ENTITIES MT
          ON MT.ENTITY_ID = M.PARENT_ID
       INNER JOIN T_AUDITEE_ENTITIES AD
          ON AD.ENTITY_ID = ET.AUDITBY_ID

        LEFT JOIN T_AUDITEE_ENTITIES_MAPING_COM E
          ON E.COM_KEY = C.COM_KEY

       WHERE C.OLD_PARA_ID = Old_id
          OR (C.NEW_PARAID = new_id AND C.IND = P_F);

    Vr1 V%ROWTYPE;

  BEGIN

    ------------------------------------------------------------------
    -- 1. Determine whether para belongs to normal observation
    --    or old CAD para
    ------------------------------------------------------------------
    SELECT NVL(MAX(CAD.PARA_ID), 0)
      INTO C_F
      FROM T_AU_OBSERVATION_OLD_CAD_PARAS CAD
     WHERE CAD.PARA_ID = new_id
       AND CAD.AUDITED_BY = ENT_ID;

    SELECT CASE
             WHEN ET.AUDIT_TYPE = 'B' THEN
              'A'
             ELSE
              CASE
                WHEN C_F <> 0 THEN
                 'C'
                ELSE
                 'A'
              END
           END
      INTO P_F
      FROM T_AUDITEE_ENTITIES EN
     INNER JOIN T_AUDITEE_ENT_TYPES ET
        ON EN.TYPE_ID = ET.AUTID
     WHERE EN.ENTITY_ID = ENT_ID;

    ------------------------------------------------------------------
    -- 2. Fetch compliance record
    ------------------------------------------------------------------
    OPEN V;

    FETCH V
      INTO Vr1;

    IF V%NOTFOUND THEN

      CLOSE V;

      OPEN io_cursor FOR
        SELECT 'Compliance record not found. Please refresh and try again.' AS remarks,
               '' AS para_no,
               '' AS para_status,
               '' AS GIST_OF_PARAS,
               '' AS TO_EMAIL,
               '' AS CC_EMAIL,
               '' AS CC_EMAIL2
          FROM DUAL;

      RETURN;

    END IF;

    CLOSE V;

    ------------------------------------------------------------------
    -- 3. Validate user/context before starting transaction
    ------------------------------------------------------------------
    SELECT NVL(MAX(M.PPNO), 0)
      INTO N_F
      FROM T_USER U
     INNER JOIN T_USER_CONTEXT_ASSIGNMENT M
        ON M.PPNO = U.PPNO
     WHERE M.PPNO = P_NO
       AND M.ENTITY_ID = ENT_ID
       AND M.ROLE_ID = R_ID
       AND R_ID <> 13;

    IF N_F = 0 THEN

      OPEN io_cursor FOR
        SELECT 'Sorry you connection is lost, logout and login again' AS remarks,
               Vr1.PARA_NO AS PARA_NO,
               '' AS para_status,
               '' AS GIST_OF_PARAS,
               '' AS TO_EMAIL,
               '' AS CC_EMAIL,
               '' AS CC_EMAIL2
          FROM DUAL;

      RETURN;

    END IF;

    ------------------------------------------------------------------
    -- 4. Start protected transaction
    ------------------------------------------------------------------
    SAVEPOINT SP_POST_COMPLIANCE;

    v_savepoint_set := TRUE;

    ------------------------------------------------------------------
    -- 5. Lock only this compliance record.
    --
    -- Prevents two users from moving the same para simultaneously.
    -- Other COM_IDs remain available to other users.
    ------------------------------------------------------------------
    SELECT C.COM_STAGE
      INTO v_current_stage
      FROM AIS_T_AU_POST_COMPLIANCE C
     WHERE C.COM_ID = Vr1.COMID
       FOR UPDATE;

    ------------------------------------------------------------------
    -- 6. Revalidate stage AFTER locking the row
    ------------------------------------------------------------------
    IF R_ID <> v_current_stage THEN

      ROLLBACK TO SP_POST_COMPLIANCE;

      OPEN io_cursor FOR
        SELECT 'Compliance Already Submitted, If the para no ' ||
               Vr1.PARA_NO ||
               ' still showing on the grid, Contact on  051-2002110' AS remarks,
               '' AS PARA_NO,
               '' AS para_status,
               '' AS GIST_OF_PARAS,
               '' AS TO_EMAIL,
               '' AS CC_EMAIL,
               '' AS CC_EMAIL2
          FROM DUAL;

      RETURN;

    END IF;

    ------------------------------------------------------------------
    -- 7. Close previous activity
    ------------------------------------------------------------------
    SELECT SEQ_POST_COMPLIANCE_HISTORY.NEXTVAL
      INTO v_history_id
      FROM DUAL;

    INSERT INTO AIS_T_AU_POST_COMPLIANCE_HISTORY
      (HIST_ID,
       COM_ID,
       COM_CYCLE,
       COM_STATUS,
       COM_STAGE,
       COMMENT_BY_ROLE,
       COMMENT_BY_PPNO,
       COMMENT_ON,
       COMMENTS,
       COM_FLOW)
    VALUES
      (v_history_id,
       Vr1.COMID,
       Vr1.COM_CYCLE,
       Vr1.STATUS_ID,
       R_ID,

       (SELECT G.GROUP_NAME FROM T_GROUPS G WHERE G.GROUP_ID = R_ID),

       P_NO,
       SYSDATE,
       A_COMMENTS,
       'Y');

    ------------------------------------------------------------------
    -- 10. Settlement
    ------------------------------------------------------------------
    IF Vr1.STATUS_ID = 16 AND R_ID IN (6, 7, 44, 41) THEN

      --------------------------------------------------------------
      -- Update post-compliance master
      --------------------------------------------------------------
      UPDATE AIS_T_AU_POST_COMPLIANCE C
         SET C.COM_CYCLE   = Vr1.COM_CYCLE,
             C.COM_STAGE   = Vr1.ROLE_ID,
             C.COM_STATUS  = Vr1.STATUS_ID,
             C.PARA_STATUS = 9,
             C.SETTELED_ON = SYSDATE,
             C.SETTELED_BY = P_NO
       WHERE C.COM_ID = Vr1.COMID;

      --------------------------------------------------------------
      -- Update originating para according to para type
      --------------------------------------------------------------
      CASE Vr1.IND

      ----------------------------------------------------------
      -- Current observation
      ----------------------------------------------------------
        WHEN 'A' THEN

          UPDATE T_AU_OBSERVATION O
             SET O.STATUS = 9, O.STELLED_ON = SYSDATE, O.SETTLED_BY = P_NO
           WHERE O.ID = new_id;

      ----------------------------------------------------------
      -- Old FAD para
      ----------------------------------------------------------
        WHEN 'O' THEN

          UPDATE T_AU_OLD_PARAS_FAD FD
             SET FD.PARA_STATUS    = 6,
                 FD.SETTLED_BY     = P_NO,
                 FD.PARASETTELEDON = SYSDATE
           WHERE FD.ID = Old_id;

      ----------------------------------------------------------
      -- Old CAD para
      ----------------------------------------------------------
        WHEN 'C' THEN

          UPDATE T_AU_OBSERVATION_OLD_CAD_PARAS CD
             SET CD.PARA_STATUS = 9,
                 CD.SETTELED_BY = P_NO,
                 CD.SETTELED_ON = SYSDATE
           WHERE CD.PARA_ID = new_id;

      ----------------------------------------------------------
      -- No originating table update required
      ----------------------------------------------------------
        ELSE
          NULL;

      END CASE;

    ELSE

      ----------------------------------------------------------------
      -- 11. Normal forwarding / referring back
      ----------------------------------------------------------------
      UPDATE AIS_T_AU_POST_COMPLIANCE C
         SET C.COM_CYCLE  = Vr1.COM_CYCLE,
             C.COM_STAGE  = Vr1.ROLE_ID,
             C.COM_STATUS = Vr1.STATUS_ID
       WHERE C.COM_ID = Vr1.COMID;

    END IF;

    ------------------------------------------------------------------
    -- 12. Entire business transaction succeeded
    ------------------------------------------------------------------
    COMMIT;

    v_savepoint_set := FALSE;

    ------------------------------------------------------------------
    -- 13. Management Audit notifications use the central IAS queue.
    --     This runs after the business commit and is isolated so an
    --     enqueue failure cannot roll back the decision.
    ------------------------------------------------------------------
    IF Vr1.AUDITBY_ID IN (112242, 112248)
       AND (Vr1.STATUS_ID = 16 OR P_IND <> 'U') THEN
      SAVEPOINT SP_MGMT_AUDIT_NOTIFICATION;
      BEGIN
        PKG_IAS_NOTIFICATION.SEND_MGMT_AUDIT_PARA_STATUS(
          Vr1.COMID,
          v_history_id,
          CASE WHEN Vr1.STATUS_ID = 16 THEN 'Settled' ELSE 'Rejected' END,
          v_notification_email_id);
        COMMIT;
      EXCEPTION
        WHEN OTHERS THEN
          ROLLBACK TO SP_MGMT_AUDIT_NOTIFICATION;
          PKG_LG.LOG_ERROR(
            'COMPLIANCE',
            'PKG_AE',
            'P_SUBMITPOSTAUDITCOMPLIANCE_REVIEW',
            'Management Audit para notification enqueue failed.',
            'COM_ID=' || Vr1.COMID || '; HIST_ID=' || v_history_id ||
            '; ERROR=' || SQLERRM,
            NULL,
            NULL,
            P_NO);
      END;
    END IF;

    ------------------------------------------------------------------
    -- 14. Return response to application
    ------------------------------------------------------------------
    IF Vr1.STATUS_ID = 16 THEN

      OPEN io_cursor FOR
        SELECT 'Para no ' || Vr1.PARA_NO ||
               ' is marked as settled, Please inform the auditee ' AS remarks,

               Vr1.PARA_NO AS para_no,
               'Settled' AS para_status,
               Vr1.GIST_OF_PARAS AS GIST_OF_PARAS,
               CASE WHEN Vr1.AUDITBY_ID IN (112242, 112248) THEN '' ELSE Vr1.TO_EMAIL END AS TO_EMAIL,
               CASE WHEN Vr1.AUDITBY_ID IN (112242, 112248) THEN '' ELSE Vr1.CC_EMAIL END AS CC_EMAIL,
               CASE WHEN Vr1.AUDITBY_ID IN (112242, 112248) THEN '' ELSE Vr1.CC_EMAIL2 END AS CC_EMAIL2

          FROM DUAL;

    ELSE

      IF P_IND = 'U' THEN

        OPEN io_cursor FOR
          SELECT 'Complaince Forwarded' AS remarks,
                 Vr1.PARA_NO AS para_no,
                 '' AS para_status,
                 '' AS GIST_OF_PARAS,
                 '' AS TO_EMAIL,
                 '' AS CC_EMAIL,
                 '' AS CC_EMAIL2
            FROM DUAL;

      ELSE

        OPEN io_cursor FOR
          SELECT 'Complaince Rejected/Referred Back' AS remarks,
                 Vr1.PARA_NO AS para_no,
                 'Rejected/Referred Back' AS para_status,
                 '' AS GIST_OF_PARAS,
                 '' AS TO_EMAIL,
                 '' AS CC_EMAIL,
                 '' AS CC_EMAIL2
            FROM DUAL;

      END IF;

    END IF;

  EXCEPTION

    ------------------------------------------------------------------
    -- Any failure in history/workflow/settlement means the complete
    -- submission is rolled back.
    ------------------------------------------------------------------
    WHEN OTHERS THEN

      IF v_savepoint_set THEN
        ROLLBACK TO SP_POST_COMPLIANCE;
      END IF;

      RAISE;

  END P_SubmitPostAuditCompliance_Review;
  procedure P_GetPostAuditCompliance_Evidence(TEXT_ID   in varchar2,
                                              io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select id,
             file_name,
             length,
             '' as file_data,
             textid,
             enteredby,
             entereddate,
             sequence,
             description,
             COMP_ID
        from ais_t_au_post_compliance_evidence
       where textid = TEXT_ID
       order by sequence asc;

    commit;

  end P_GetPostAuditCompliance_Evidence;

  procedure P_GetPostAuditCompliance_Evidence_FileData(FILE_ID   in varchar2,
                                                       io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select id,
             file_name,
             length,
             file_data,
             textid,
             enteredby,
             entereddate,
             sequence,
             description,
             COMP_ID
        from ais_t_au_post_compliance_evidence
       where id = FILE_ID;

    commit;

  end P_GetPostAuditCompliance_Evidence_FileData;

  procedure P_GetComplianceByAuditee(EntityID  in number,
                                     P_NO      in number,
                                     R_ID      in number,
                                     ENT_ID    in Number,
                                     io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select f.audit_period,
             e.name,
             f.para_no,
             f.old_para_id,
             f.new_para_id,
             f.gist_of_paras,
             e.auditby_id,
             f.IND,
             f.com_id,
             f.com_status,
             f.com_cycle,
             fl.next_r_id,
             fl.per_r_id,
             fl.c_status_up,
             fl.c_status_down
        from AIS_T_AU_POST_COMPLIANCE f
       inner join t_auditee_entities e
          on e.entity_id = f.entity_id
       inner join t_au_post_compliance_flow fl
          on fl.entity_type = e.type_id
         and fl.role_id = f.com_stage
       where f.ENTITY_ID = EntityID
         and f.com_stage = R_ID
       order by f.audit_period desc;

  end P_GetComplianceByAuditee;

  procedure p_GetParaComplianceResponsible(Old_id    in number,
                                           new_id    in number,
                                           IND       in varchar2,
                                           io_cursor OUT t_cursor) as

  begin
    if (IND = 'O') then
      open io_cursor for
        select f.pp_no,
               e.EMPLOYEEFIRSTNAME || '  ' || e.EMPLOYEELASTNAME as emp_name,
               f.loan_case as LOANCASE,
               f.lc_amount as LCAMOUNT,
               f.account_number as ACCNUMBER,
               f.ac_amount as ACAMOUNT
          from v_get_auditee_PP_responsibility f
         inner join v_service_employeeinfo e
            on e.PPNO = f.pp_no
         WHERE F.OLD_PARA_ID = Old_id
           and f.status = 'Y';
    else
      if (IND = 'N') then
        open io_cursor for
          select f.pp_no,
                 e.EMPLOYEEFIRSTNAME || '  ' || e.EMPLOYEELASTNAME as emp_name,
                 f.loan_case as LOANCASE,
                 f.lc_amount as LCAMOUNT,
                 f.account_number as ACCNUMBER,
                 f.ac_amount as ACAMOUNT
            from v_get_auditee_PP_responsibility f
           inner join v_service_employeeinfo e
              on e.PPNO = f.pp_no
           WHERE F.au_obs_id = new_id
             and f.status = 'Y';
      end if;
    end if;
  end p_GetParaComplianceResponsible;

  procedure P_GetOldParasForResponse(UserEntityID in number,
                                     entityId     in number,
                                     io_cursor    OUT t_cursor) is
  begin
    open io_cursor for

      SELECT f.*,
             c.heading   as Process_Des,
             cc.heading  as Sub_process_Des,
             csb.heading AS Check_List_Detail_Des
        FROM t_au_old_paras_fad f
       inner join t_audit_checklist_details csb
          on csb.id = f.process_detail
       inner join t_audit_checklist_sub cc
          on cc.s_id = csb.s_id
       inner join t_audit_checklist c
          on c.t_id = cc.t_id
       WHERE f.audited_by = UserEntityID
         and f.entity_id = entityId
       order by f.ID;
  end P_GetOldParasForResponse;

  procedure P_GetParasForComplianceByCAU(P_NO      in number,
                                         ENT_ID    in number,
                                         R_ID      in number,
                                         io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select c.com_id,
             c.old_para_id,
             c.new_para_id,
             c.audit_period,
             c.para_no,
             c.gist_of_paras,
             c.ind,
             c.cau_status,
             c.cau_assigned_ent_id
        FROM AIS_T_AU_POST_COMPLIANCE C
       inner join t_auditee_entities_maping m
          on m.entity_id = c.entity_id
       where c.ENTITY_ID = ENT_ID
         and C.com_stage = R_ID
         and C.com_status != 16
         and c.para_status = 8
         and c.cau_status is null
       order by C.audit_period desc, C.para_no asc;

  end P_GetParasForComplianceByCAU;

  procedure P_GetrealtionshiptypeforCAU(io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select f.entity_realtion_id,
             f.parent_name || '   TO   ' || f.chlid_name as field_name
        from t_auditee_ent_relation f
       where f.status = 'Y'
         and f.id is not null
         and f.child_entity_typeid in (6, 28)
       order by f.id;
  end P_GetrealtionshiptypeforCAU;

  procedure P_GetparentrepofficeforCAU(rid       in number,
                                       ENT_ID    in number,
                                       io_cursor OUT t_cursor) is

    A_F number := 0;
    N_F number := 0;
  begin
    select e.type_id
      into N_F
      from t_auditee_entities e
     where e.entity_id = ENT_ID;
    if (N_F = 25) then
      select e.auditby_id
        into A_F
        from t_auditee_entities e
       where e.entity_id = ENT_ID;
      open io_cursor for
        select Distinct (r.p_name) as DESCRIPTION,
                        r.parent_id as ENTITY_ID,
                        r.relation_type_id as ENTITY_REALTION_ID,
                        t.entitytypedesc as ENTITYTYPEDESC,
                        r.status as ACTIVE,
                        r.p_type_id as typeid
          from t_auditee_ent_relation e,
               t_auditee_ent_types    t,
               v_get_parent_office    r
         where t.autid = r.relation_type_id
           and r.p_type_id = e.parent_entity_typeid
           and r.c_type_id = e.child_entity_typeid
           and r.relation_type_id = rid
           and r.auditedby = A_F
           and r.parent_id is not null
         order by r.p_name;
    end if;
  end P_GetparentrepofficeforCAU;

  procedure P_GetchildpostingforCAU(P_ENT_ID  in number,
                                    io_cursor OUT t_cursor) is

  begin
    open io_cursor for
      select distinct (r.c_name),
                      r.entity_id,
                      r.c_name,
                      r.status,
                      r.c_type_id as typeid,
                      R.COMPLICE_BY,
                      R.AUDIT_BY

        from v_get_parent_office r
       inner join t_auditee_ent_types t
          on t.autid = r.relation_type_id
       where r.parent_id = P_ENT_ID
       order by r.c_name;
  end P_GetchildpostingforCAU;

  Procedure P_FORWARD_CAU_PARA_TO_BRANCH(C_ID         number,
                                         ENT_ID       number,
                                         P_NO         number,
                                         R_ID         number,
                                         B_ENT_ID     number,
                                         CAU_COMMENTS varchar2,
                                         io_cursor    OUT t_cursor) as
    N_F number := 0;
    Z_B number := 0;
    T_F number := 0;
    E_F number := 0;
  begin
    select e.type_id
      into E_F
      from t_auditee_entities e
     where e.entity_id = ENT_ID;
    if (E_F = 25) then
      select NVL(max(u.ppno), 0)
        into N_F
        from t_user u
       inner join t_user_maping m
          on m.ppno = u.ppno
       where u.ppno = P_NO
         and u.entity_id = ENT_ID
         and m.role_id = R_ID;

      if (N_F != 0) then
        commit;



        update AIS_T_AU_POST_COMPLIANCE c
           set c.cau_status          = 1,
               c.cau_assigned_by     = P_NO,
               c.cau_assigned_on     = sysdate,
               c.cau_assigned_ent_id = B_ENT_ID
         where c.com_id = c_id;
        commit;

        select nvl(max(c.c_txt_id), 0)
          into T_F
          from AIS_T_AU_POST_COMPLIANCE_text_CAU c
         where c.com_id = C_ID;

        if (T_F = 0) then
          insert into AIS_T_AU_POST_COMPLIANCE_text_CAU
            (c_Txt_Id, Com_ID, Cau_Instructions, Status)
          values
            ((select COALESCE(max(ct.c_Txt_Id) + 1, 1)
               From AIS_T_AU_POST_COMPLIANCE_text_cau ct),
             C_ID,
             CAU_COMMENTS,
             'F');
          commit;
        else
          update AIS_T_AU_POST_COMPLIANCE_text_CAU c
             set c.cau_instructions = CAU_COMMENTS
           where c.com_id = C_ID;
          commit;
        end if;
        open io_cursor for
          select 'Para Forwarded to Branch' as remarks from dual;
      else
        open io_cursor for
          select 'Sorry you connection is lost, logout and login again' as remarks
            from dual;
      end if;
    else
      open io_cursor for
        select 'Sorry this facility is only for CAU' as remarks from dual;
    end if;

  end P_FORWARD_CAU_PARA_TO_BRANCH;

  procedure P_GetParasForComplianceByCAU_BY_BRANCH(P_NO      in number,
                                                   ENT_ID    in number,
                                                   R_ID      in number,
                                                   io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select c.com_id,
             e.name as CAU_NAME,
             c.old_para_id,
             c.new_para_id,
             c.audit_period,
             c.para_no,
             c.gist_of_paras,
             cau.cau_instructions,
             c.ind
        FROM AIS_T_AU_POST_COMPLIANCE C
       inner join t_auditee_entities e
          on e.entity_id = c.entity_id
       inner join ais_t_au_post_compliance_text_cau cau
          on cau.com_id = c.com_id
       where c.para_status = 8
         and c.cau_assigned_ent_id = ENT_ID
         and c.cau_status = 1
       order by C.audit_period desc, C.para_no asc;

  end P_GetParasForComplianceByCAU_BY_BRANCH;

  procedure P_GetParasForCompliance_CAU_para_text(c_id      number,
                                                  IND       varchar2,
                                                  io_cursor OUT t_cursor) as
  begin
    if (IND = 'O') then
      open io_cursor for
        select o.gist_of_paras as gist,
               pt.para_text as para_text,
               t.reply,
               nvl(t.c_txt_id, 0) as text_id,
               t.cau_instructions
          from AIS_T_AU_POST_COMPLIANCE p
          left join AIS_T_AU_POST_COMPLIANCE_text_CAU t
            on t.com_id = p.com_id
         inner join t_au_old_paras_fad o
            on o.id = p.old_para_id
         inner join t_au_old_paras_fad_text pt
            on o.ref_p = Pt.Ref_p
         where p.com_id = C_ID;
    else
      open io_cursor for
        select pt.headings as gist,
               pt.text as para_text,
               t.reply,
               nvl(t.c_txt_id, 0) as text_id,
               t.cau_instructions
          from AIS_T_AU_POST_COMPLIANCE p
          left join AIS_T_AU_POST_COMPLIANCE_text_CAU t
            on t.com_id = p.com_id
         inner join t_au_observation_text pt
            on p.new_para_id = pt.observatsion_id
         where p.com_id = C_ID;
    end if;

  end P_GetParasForCompliance_CAU_para_text;

  Procedure P_SubmitPostAuditCompliance_BY_BRANCH(C_ID        number,
                                                  T_ID        number,
                                                  ENT_ID      number,
                                                  P_NO        number,
                                                  R_ID        number,
                                                  Auditee_COM clob,
                                                  --  A_COMMENTS  varchar2,
                                                  io_cursor OUT t_cursor) as
    N_F number := 0;
    Z_B number := 0;
  begin

    select NVL(max(u.ppno), 0)
      into N_F
      from t_user u
     inner join t_user_context_assignment m
        on m.ppno = u.ppno
     where u.ppno = P_NO
       and u.entity_id = ENT_ID
       and m.role_id = R_ID;

    if (N_F != 0) then
      commit;

      commit;

      update AIS_T_AU_POST_COMPLIANCE c
         set c.cau_status     = 2,
             c.br_response_by = P_NO,
             c.br_response_on = sysdate
       where c.com_id = c_id;
      commit;

      Update AIS_T_AU_POST_COMPLIANCE_text_CAU c
         set c.reply = Auditee_COM, c.status = 'R'
       where c.com_id = C_ID
         and c.c_txt_id = T_ID;
      commit;

      open io_cursor for
        select 'Complaince Submitted to CAU' as remarks from dual;

    else
      open io_cursor for
        select 'Sorry you connection is lost, logout and login again' as remarks
          from dual;
    end if;
  end P_SubmitPostAuditCompliance_BY_BRANCH;

  procedure P_SubmitPostAuditCompliance_Evidence_By_BRANCH(TEXT_ID  in varchar2,
                                                           filename IN Varchar2,
                                                           len_id   in number,
                                                           enter_by IN number,
                                                           filetype in varchar2,
                                                           filedata IN clob,
                                                           seq_id   IN number) as

  begin
    insert into ais_t_au_post_compliance_evidence_cau
      (id,
       file_name,
       length,
       file_data,
       textid,
       enteredby,
       entereddate,
       sequence,
       description,
       COMP_ID)

    VALUES
      ((select COALESCE(max(ac.ID) + 1, 1)
         from ais_t_au_post_compliance_evidence_cau ac),
       filename,
       len_id,
       filedata,
       TEXT_ID,
       enter_by,
       sysdate,
       seq_id,
       filetype,
       (select max(t.com_id)
          from ais_t_au_post_compliance_text t
         where t.c_txt_id = TEXT_ID));
    commit;

  end P_SubmitPostAuditCompliance_Evidence_By_BRANCH;
  procedure P_GetAllCompliance_Evidence_CAU(TEXT_ID   in varchar2,
                                            io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select id,
             file_name,
             length,
             '' as file_data,
             textid,
             enteredby,
             entereddate,
             sequence,
             description,
             COMP_ID
        from ais_t_au_post_compliance_evidence_cau
       where textid = TEXT_ID;

    commit;

  end P_GetAllCompliance_Evidence_CAU;
  procedure P_GetPostAuditCompliance_Evidence_FileData_CAU(FILE_ID   in varchar2,
                                                           io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select id,
             file_name,
             length,
             file_data,
             textid,
             enteredby,
             entereddate,
             sequence,
             description,
             COMP_ID
        from ais_t_au_post_compliance_evidence_cau
       where id = FILE_ID;

    commit;

  end P_GetPostAuditCompliance_Evidence_FileData_CAU;

  procedure P_GetParasForComplianceByCAU_FOR_REVIEW(P_NO      in number,
                                                    ENT_ID    in number,
                                                    R_ID      in number,
                                                    io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR
      select c.com_id,
             c.ind,
             c.old_para_id,
             c.new_para_id,
             c.audit_period,
             c.para_no,
             c.gist_of_paras,
             c.cau_status,
             c.cau_assigned_ent_id
        FROM AIS_T_AU_POST_COMPLIANCE C
       where c.ENTITY_ID = ENT_ID
            --and C.com_stage = R_ID
         and C.com_status != 16
         and c.para_status = 8
         and c.cau_status = 2
       order by C.audit_period desc, C.para_no asc;

  end P_GetParasForComplianceByCAU_FOR_REVIEW;

  Procedure p_GetPostAuditComplianceSecuritySnapshot(CM_ID     in number,
                                                     io_cursor OUT t_cursor) as

  begin
    OPEN io_cursor FOR

      select C.COM_ID,
             C.ENTITY_ID,
             C.NAME,
             C.AUDIT_PERIOD,
             C.PARA_NO,
             C.NEW_PARAID,
             C.OLD_PARA_ID,
             C.COM_STAGE,
             C.COM_STATUS,
             C.COM_CYCLE,
             C.NEXT_R_ID,
             C.PER_R_ID,
             C.IND,
             C.REC_FROM
        from V_GET_AIS_POST_COMPLIANCE C
       where C.COM_ID = CM_ID;
  end;

  Procedure P_HasActiveUserContextAssignment(P_NO        in number,
                                             ENT_ID      in number,
                                             R_ID        in number,
                                             USER_CON_ID in number,
                                             io_cursor   OUT t_cursor) as

  begin
    OPEN io_cursor FOR

      select count(1)
        from T_USER_CONTEXT_ASSIGNMENT uca
       where uca.PPNO = P_NO
         and uca.ENTITY_ID = ENT_ID
         and uca.ROLE_ID = R_ID
         and uca.IS_ACTIVE = 'Y';

  end;
end PKG_AE;
/

/* Fail before creating operational controls if any package/view did not compile. */
DECLARE
  V_ERRORS NUMBER;
BEGIN
  SELECT COUNT(*) INTO V_ERRORS
    FROM USER_ERRORS
   WHERE NAME IN ('PKG_INQ','PKG_IAS_NOTIFICATION','PKG_AE',
                  'V_IAS_POST_COMPLIANCE_NOTIFY','V_IAS_MGMT_AUDIT_NOTIFY_MAP');
  IF V_ERRORS>0 THEN
    FOR R IN (SELECT NAME,TYPE,LINE,POSITION,TEXT FROM USER_ERRORS
               WHERE NAME IN ('PKG_INQ','PKG_IAS_NOTIFICATION','PKG_AE',
                              'V_IAS_POST_COMPLIANCE_NOTIFY','V_IAS_MGMT_AUDIT_NOTIFY_MAP')
               ORDER BY NAME,SEQUENCE) LOOP
      DBMS_OUTPUT.PUT_LINE(R.NAME||' '||R.TYPE||' line '||R.LINE||':'||R.POSITION||' '||R.TEXT);
    END LOOP;
    RAISE_APPLICATION_ERROR(-20810,'Package/view compilation failed. Operational controls were not created.');
  END IF;
END;
/

PROMPT ========================================================================
PROMPT E. QUEUE / IDEMPOTENCY CONTROLS
PROMPT ========================================================================
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX UQ_IID_EMAIL_MGMT_WEEKLY';
EXCEPTION WHEN OTHERS THEN IF SQLCODE<>-1418 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX UQ_IID_EMAIL_SCHED_NOTIFY';
EXCEPTION WHEN OTHERS THEN IF SQLCODE<>-1418 THEN RAISE; END IF;
END;
/
BEGIN
  EXECUTE IMMEDIATE q'~CREATE UNIQUE INDEX UQ_IID_EMAIL_SCHED_NOTIFY ON T_AU_IID_EMAIL_QUEUE
    (CASE WHEN EVENT_CODE IN ('MGMT_AUDIT_WEEKLY_PARA_STATUS','MGMT_AUDIT_MAPPING_EXCEPTION','IAS_NOTIFICATION_HEALTH') THEN EVENT_CODE END,
     CASE WHEN EVENT_CODE IN ('MGMT_AUDIT_WEEKLY_PARA_STATUS','MGMT_AUDIT_MAPPING_EXCEPTION','IAS_NOTIFICATION_HEALTH') THEN REF_ID1 END,
     CASE WHEN EVENT_CODE IN ('MGMT_AUDIT_WEEKLY_PARA_STATUS','MGMT_AUDIT_MAPPING_EXCEPTION','IAS_NOTIFICATION_HEALTH') THEN NVL(REF_ID2,-1) END)~';
END;
/

PROMPT ========================================================================
PROMPT F. COMPILATION / QUEUE VALIDATION
PROMPT ========================================================================
DECLARE
  V_OBJECT_COUNT NUMBER; V_INVALID NUMBER; V_ERRORS NUMBER; V_STATUS_TYPE VARCHAR2(30);
  V_INDEX_COUNT NUMBER; V_ENABLED_JOBS NUMBER;
BEGIN
  SELECT COUNT(*) INTO V_OBJECT_COUNT FROM USER_OBJECTS
   WHERE OBJECT_NAME IN ('PKG_INQ','PKG_AE','PKG_IAS_NOTIFICATION','V_IAS_POST_COMPLIANCE_NOTIFY','V_IAS_MGMT_AUDIT_NOTIFY_MAP')
     AND OBJECT_TYPE IN ('PACKAGE','PACKAGE BODY','VIEW');
  SELECT COUNT(*) INTO V_INVALID FROM USER_OBJECTS
   WHERE OBJECT_NAME IN ('PKG_INQ','PKG_AE','PKG_IAS_NOTIFICATION','V_IAS_POST_COMPLIANCE_NOTIFY','V_IAS_MGMT_AUDIT_NOTIFY_MAP')
     AND OBJECT_TYPE IN ('PACKAGE','PACKAGE BODY','VIEW') AND STATUS<>'VALID';
  SELECT COUNT(*) INTO V_ERRORS FROM USER_ERRORS
   WHERE NAME IN ('PKG_INQ','PKG_AE','PKG_IAS_NOTIFICATION','V_IAS_POST_COMPLIANCE_NOTIFY','V_IAS_MGMT_AUDIT_NOTIFY_MAP');
  SELECT DATA_TYPE INTO V_STATUS_TYPE FROM USER_ARGUMENTS
   WHERE PACKAGE_NAME='PKG_INQ' AND OBJECT_NAME='P_GET_EMAIL_QUEUE'
     AND ARGUMENT_NAME='P_STATUS' AND DATA_LEVEL=0;
  SELECT COUNT(*) INTO V_INDEX_COUNT FROM USER_INDEXES
   WHERE INDEX_NAME='UQ_IID_EMAIL_SCHED_NOTIFY' AND STATUS='VALID';
  SELECT COUNT(*) INTO V_ENABLED_JOBS FROM USER_SCHEDULER_JOBS
   WHERE JOB_NAME IN ('JOB_MGMT_AUDIT_WEEKLY_NOTIFY','JOB_MGMT_AUDIT_MAPPING_EXCEPT','JOB_IAS_NOTIFICATION_HEALTH')
     AND ENABLED='TRUE';

  IF V_OBJECT_COUNT<>8 OR V_INVALID>0 OR V_ERRORS>0 OR V_STATUS_TYPE<>'VARCHAR2'
     OR V_INDEX_COUNT<>1 OR V_ENABLED_JOBS<>0 THEN
    RAISE_APPLICATION_ERROR(
      -20811,
      'Pre-scheduler validation failed: objects='||V_OBJECT_COUNT||'/8, invalid='||V_INVALID||
      ', errors='||V_ERRORS||', P_STATUS='||V_STATUS_TYPE||
      ', valid_index='||V_INDEX_COUNT||', prematurely_enabled_jobs='||V_ENABLED_JOBS
    );
  END IF;
  DBMS_OUTPUT.PUT_LINE(
    'Pre-scheduler validation passed. Packages/views/index valid, P_GET_EMAIL_QUEUE.P_STATUS=VARCHAR2, target jobs disabled.'
  );
END;
/

/* Transactional queue lifecycle smoke test; no SMTP delivery is claimed and the row is rolled back. */
DECLARE
  V_EMAIL_ID NUMBER; V_TO VARCHAR2(320); V_STATUS VARCHAR2(20); V_RETRY NUMBER;
BEGIN
  SELECT MIN(E.EMAIL) INTO V_TO
    FROM T_USER_MAPING M
    JOIN T_USER U ON U.USERID=M.USERID
    JOIN V_SERVICE_EMPLOYEEINFO E ON E.PPNO=U.PPNO
   WHERE M.ROLE_ID=1
     AND NVL(U.ISACTIVE,'Y')='Y'
     AND TRIM(E.EMAIL) IS NOT NULL;

  IF V_TO IS NULL THEN
    RAISE_APPLICATION_ERROR(-20812,'No active Super User email for queue validation.');
  END IF;

  SAVEPOINT QUEUE_API_TEST;
  PKG_INQ.P_ENQUEUE_EMAIL(
    'DEPLOYMENT_QUEUE_VALIDATION',NULL,NULL,V_TO,NULL,
    'IAS deployment queue validation',
    'Rolled back after API state-transition validation.',
    V_EMAIL_ID
  );

  SELECT STATUS INTO V_STATUS
    FROM T_AU_IID_EMAIL_QUEUE
   WHERE EMAIL_ID=V_EMAIL_ID;
  IF V_STATUS<>'PENDING' THEN
    RAISE_APPLICATION_ERROR(-20813,'PENDING transition failed.');
  END IF;

  PKG_INQ.P_MARK_EMAIL_FAILED(V_EMAIL_ID,'Deployment retry validation');
  SELECT STATUS,RETRY_COUNT INTO V_STATUS,V_RETRY
    FROM T_AU_IID_EMAIL_QUEUE
   WHERE EMAIL_ID=V_EMAIL_ID;
  IF V_STATUS<>'FAILED' OR V_RETRY<1 THEN
    RAISE_APPLICATION_ERROR(-20814,'FAILED/retry transition failed.');
  END IF;

  PKG_INQ.P_MARK_EMAIL_SENT(V_EMAIL_ID);
  SELECT STATUS INTO V_STATUS
    FROM T_AU_IID_EMAIL_QUEUE
   WHERE EMAIL_ID=V_EMAIL_ID;
  IF V_STATUS<>'SENT' THEN
    RAISE_APPLICATION_ERROR(-20815,'SENT transition failed.');
  END IF;

  ROLLBACK TO QUEUE_API_TEST;
  DBMS_OUTPUT.PUT_LINE('Queue API lifecycle passed: PENDING -> FAILED/retry -> SENT; test row rolled back.');
END;
/

COMMIT;

PROMPT ========================================================================
PROMPT G. SCHEDULER CONFIGURATION / ENABLEMENT
PROMPT ========================================================================
SET DEFINE ON;

DECLARE
  PROCEDURE UPSERT_JOB_DISABLED(
    P_NAME VARCHAR2,
    P_ACTION VARCHAR2,
    P_SCHEDULE VARCHAR2,
    P_COMMENTS VARCHAR2
  ) IS
    V_COUNT NUMBER;
    V_START TIMESTAMP WITH TIME ZONE;
  BEGIN
    DBMS_SCHEDULER.EVALUATE_CALENDAR_STRING(P_SCHEDULE,SYSTIMESTAMP,SYSTIMESTAMP,V_START);

    SELECT COUNT(*) INTO V_COUNT
      FROM USER_SCHEDULER_JOBS
     WHERE JOB_NAME=UPPER(P_NAME);

    IF V_COUNT=0 THEN
      DBMS_SCHEDULER.CREATE_JOB(
        JOB_NAME=>P_NAME,
        JOB_TYPE=>'PLSQL_BLOCK',
        JOB_ACTION=>P_ACTION,
        START_DATE=>V_START,
        REPEAT_INTERVAL=>P_SCHEDULE,
        ENABLED=>FALSE,
        AUTO_DROP=>FALSE,
        COMMENTS=>P_COMMENTS
      );
    ELSE
      DBMS_SCHEDULER.DISABLE(P_NAME,FORCE=>TRUE);
      DBMS_SCHEDULER.SET_ATTRIBUTE(P_NAME,'job_action',P_ACTION);
      DBMS_SCHEDULER.SET_ATTRIBUTE(P_NAME,'repeat_interval',P_SCHEDULE);
      DBMS_SCHEDULER.SET_ATTRIBUTE(P_NAME,'comments',P_COMMENTS);
    END IF;
  END;
BEGIN
  UPSERT_JOB_DISABLED(
    'JOB_MGMT_AUDIT_WEEKLY_NOTIFY',
    'BEGIN PKG_IAS_NOTIFICATION.SEND_MGMT_AUDIT_WEEKLY; END;',
    &MGMT_WEEKLY_SCHEDULE,
    'Queues the previous Monday-Sunday digest separately for each Divisional Head.'
  );
  UPSERT_JOB_DISABLED(
    'JOB_MGMT_AUDIT_MAPPING_EXCEPT',
    'BEGIN PKG_IAS_NOTIFICATION.SEND_MGMT_AUDIT_MAPPING_EXCEPTIONS; END;',
    &MGMT_MAPPING_EXCEPTION_SCHEDULE,
    'Queues Friday mapping exceptions separately for heads 112242 and 112248.'
  );
  UPSERT_JOB_DISABLED(
    'JOB_IAS_NOTIFICATION_HEALTH',
    'BEGIN PKG_IAS_NOTIFICATION.SEND_NOTIFICATION_HEALTH; END;',
    &IAS_NOTIFICATION_HEALTH_SCHEDULE,
    'Queues the IAS notification operational health report for Super Admin.'
  );
END;
/

DECLARE
  V_COUNT NUMBER;
BEGIN
  SELECT COUNT(*) INTO V_COUNT
    FROM USER_SCHEDULER_JOBS
   WHERE ENABLED='FALSE'
     AND (
       (JOB_NAME='JOB_MGMT_AUDIT_WEEKLY_NOTIFY' AND REPEAT_INTERVAL=&MGMT_WEEKLY_SCHEDULE)
       OR
       (JOB_NAME='JOB_MGMT_AUDIT_MAPPING_EXCEPT' AND REPEAT_INTERVAL=&MGMT_MAPPING_EXCEPTION_SCHEDULE)
       OR
       (JOB_NAME='JOB_IAS_NOTIFICATION_HEALTH' AND REPEAT_INTERVAL=&IAS_NOTIFICATION_HEALTH_SCHEDULE)
     );

  IF V_COUNT<>3 THEN
    RAISE_APPLICATION_ERROR(-20816,'Scheduler definition validation failed while jobs were disabled.');
  END IF;

  DBMS_OUTPUT.PUT_LINE('Scheduler definitions validated in DISABLED state.');
END;
/

BEGIN
  BEGIN
    DBMS_SCHEDULER.ENABLE('JOB_MGMT_AUDIT_WEEKLY_NOTIFY');
    DBMS_SCHEDULER.ENABLE('JOB_MGMT_AUDIT_MAPPING_EXCEPT');
    DBMS_SCHEDULER.ENABLE('JOB_IAS_NOTIFICATION_HEALTH');
  EXCEPTION
    WHEN OTHERS THEN
      FOR R IN (
        SELECT JOB_NAME,ENABLED
          FROM USER_SCHEDULER_JOBS
         WHERE JOB_NAME IN
           ('JOB_MGMT_AUDIT_WEEKLY_NOTIFY',
            'JOB_MGMT_AUDIT_MAPPING_EXCEPT',
            'JOB_IAS_NOTIFICATION_HEALTH')
      ) LOOP
        IF R.ENABLED='TRUE' THEN
          BEGIN
            DBMS_SCHEDULER.DISABLE(R.JOB_NAME,FORCE=>TRUE);
          EXCEPTION WHEN OTHERS THEN NULL;
          END;
        END IF;
      END LOOP;
      RAISE;
  END;
END;
/

DECLARE
  V_COUNT NUMBER;
BEGIN
  SELECT COUNT(*) INTO V_COUNT
    FROM USER_SCHEDULER_JOBS
   WHERE JOB_NAME IN
     ('JOB_MGMT_AUDIT_WEEKLY_NOTIFY',
      'JOB_MGMT_AUDIT_MAPPING_EXCEPT',
      'JOB_IAS_NOTIFICATION_HEALTH')
     AND ENABLED='TRUE';

  IF V_COUNT<>3 THEN
    FOR R IN (
      SELECT JOB_NAME,ENABLED
        FROM USER_SCHEDULER_JOBS
       WHERE JOB_NAME IN
         ('JOB_MGMT_AUDIT_WEEKLY_NOTIFY',
          'JOB_MGMT_AUDIT_MAPPING_EXCEPT',
          'JOB_IAS_NOTIFICATION_HEALTH')
    ) LOOP
      IF R.ENABLED='TRUE' THEN
        BEGIN
          DBMS_SCHEDULER.DISABLE(R.JOB_NAME,FORCE=>TRUE);
        EXCEPTION WHEN OTHERS THEN NULL;
        END;
      END IF;
    END LOOP;
    RAISE_APPLICATION_ERROR(-20817,'Scheduler enablement validation failed; target jobs were disabled.');
  END IF;

  DBMS_OUTPUT.PUT_LINE('Scheduler enablement passed: all three target jobs are enabled.');
END;
/

SET DEFINE OFF;
COMMIT;

PROMPT ========================================================================
PROMPT H. POST-DEPLOYMENT VERIFICATION
PROMPT ========================================================================
/* Read-only post-deployment evidence. Run after queue workers have processed test notifications. */
SET PAGESIZE 200;
SET LINESIZE 240;
SET SERVEROUTPUT ON;

PROMPT 1-4. Immediate Management Audit notifications by audit domain and decision
SELECT PC.AUDITED_BY,
       CASE WHEN H.COM_STATUS=16 THEN 'SETTLED' ELSE 'REJECTED' END DECISION,
       COUNT(*) QUEUED, SUM(CASE WHEN Q.STATUS='SENT' THEN 1 ELSE 0 END) SENT,
       SUM(CASE WHEN Q.STATUS='PENDING' THEN 1 ELSE 0 END) PENDING,
       SUM(CASE WHEN Q.STATUS='FAILED' THEN 1 ELSE 0 END) FAILED
  FROM T_AU_IID_EMAIL_QUEUE Q
  JOIN AIS_T_AU_POST_COMPLIANCE PC ON PC.COM_ID=Q.REF_ID1
  JOIN AIS_T_AU_POST_COMPLIANCE_HISTORY H ON H.HIST_ID=Q.REF_ID2 AND H.COM_ID=PC.COM_ID
 WHERE Q.EVENT_CODE='MGMT_AUDIT_PARA_STATUS' AND PC.AUDITED_BY IN (112242,112248)
 GROUP BY PC.AUDITED_BY,CASE WHEN H.COM_STATUS=16 THEN 'SETTLED' ELSE 'REJECTED' END
 ORDER BY PC.AUDITED_BY,DECISION;

PROMPT 5. Audit Year, Risk and decision-specific rejection reason
SELECT PC.AUDITED_BY,PC.COM_ID,PC.AUDIT_PERIOD AUDIT_YEAR,PC.RSK RISK,PC.PARA_NO,
       H.HIST_ID,H.COMMENT_ON DECISION_ON,H.COMMENTS REJECTION_REASON,Q.EMAIL_ID,Q.STATUS
  FROM T_AU_IID_EMAIL_QUEUE Q
  JOIN AIS_T_AU_POST_COMPLIANCE PC ON PC.COM_ID=Q.REF_ID1
  JOIN AIS_T_AU_POST_COMPLIANCE_HISTORY H ON H.HIST_ID=Q.REF_ID2 AND H.COM_ID=PC.COM_ID
 WHERE Q.EVENT_CODE='MGMT_AUDIT_PARA_STATUS'
 ORDER BY Q.CREATED_ON DESC;

PROMPT 6-7. One weekly queue item per Division; Division TO and Reporting/Group CC evidence
SELECT Q.REF_ID1 PERIOD_START_KEY,Q.REF_ID2 DIVISION_ID,Q.MAIL_TO,Q.MAIL_CC,Q.STATUS,
       COUNT(*) OVER (PARTITION BY Q.REF_ID1,Q.REF_ID2) ITEMS_FOR_DIVISION_PERIOD
  FROM T_AU_IID_EMAIL_QUEUE Q
 WHERE Q.EVENT_CODE='MGMT_AUDIT_WEEKLY_PARA_STATUS'
 ORDER BY Q.REF_ID1 DESC,Q.REF_ID2;
SELECT * FROM (
  SELECT ENTITY_ID,ENTITY_NAME,AUDITED_BY,DIVISION_ID,DIVISION_NAME,DIVISION_EMAIL,
         REPORTING_ID,REPORTING_NAME,REPORTING_EMAIL,MAPPING_READY
    FROM V_IAS_MGMT_AUDIT_NOTIFY_MAP WHERE MAPPING_READY='Y' ORDER BY ENTITY_ID
) WHERE ROWNUM<=3;

PROMPT 8-9. Friday exceptions and current dynamic exception population
SELECT EMAIL_ID,REF_ID1 REPORT_DATE_KEY,REF_ID2 AUDITED_BY,MAIL_TO,STATUS,RETRY_COUNT,CREATED_ON,SENT_ON
  FROM T_AU_IID_EMAIL_QUEUE WHERE EVENT_CODE='MGMT_AUDIT_MAPPING_EXCEPTION'
 ORDER BY CREATED_ON DESC;
SELECT AUDITED_BY,ENTITY_ID,ENTITY_NAME,DIVISION_NAME,DIVISION_EMAIL,REPORTING_NAME,REPORTING_EMAIL,
       MISSING_REQUIRED_ACTION
  FROM V_IAS_MGMT_AUDIT_NOTIFY_MAP WHERE MAPPING_READY='N'
 ORDER BY AUDITED_BY,ENTITY_NAME;
PROMPT A corrected entity disappears from the preceding live view on the next run; no exception snapshot table is used.

PROMPT 10-11. Delivery/retry evidence
SELECT STATUS,COUNT(*) ROWS_IN_STATE,MIN(CREATED_ON) OLDEST,MAX(SENT_ON) LAST_SENT,
       SUM(NVL(RETRY_COUNT,0)) RETRY_COUNT
  FROM T_AU_IID_EMAIL_QUEUE GROUP BY STATUS ORDER BY STATUS;
SELECT EVENT_CODE,COUNT(*) RETRIED_THEN_SENT,MAX(SENT_ON) LAST_RETRY_SUCCESS
  FROM T_AU_IID_EMAIL_QUEUE WHERE STATUS='SENT' AND NVL(RETRY_COUNT,0)>0
 GROUP BY EVENT_CODE ORDER BY EVENT_CODE;

PROMPT 12. P_GET_EMAIL_QUEUE status parameter contract
SELECT PACKAGE_NAME,OBJECT_NAME,ARGUMENT_NAME,IN_OUT,DATA_TYPE
  FROM USER_ARGUMENTS
 WHERE PACKAGE_NAME='PKG_INQ' AND OBJECT_NAME='P_GET_EMAIL_QUEUE' AND ARGUMENT_NAME='P_STATUS';

PROMPT 13-14. Configured and enabled Monday/Friday scheduler jobs
SELECT JOB_NAME,ENABLED,STATE,REPEAT_INTERVAL,NEXT_RUN_DATE,LAST_START_DATE,LAST_RUN_DURATION,FAILURE_COUNT
  FROM USER_SCHEDULER_JOBS
 WHERE JOB_NAME IN ('JOB_MGMT_AUDIT_WEEKLY_NOTIFY','JOB_MGMT_AUDIT_MAPPING_EXCEPT','JOB_IAS_NOTIFICATION_HEALTH')
 ORDER BY JOB_NAME;

PROMPT 15. Super Admin health-report queue and operational summary
SELECT EMAIL_ID,MAIL_TO,STATUS,RETRY_COUNT,CREATED_ON,SENT_ON,ERROR_TEXT
  FROM T_AU_IID_EMAIL_QUEUE WHERE EVENT_CODE='IAS_NOTIFICATION_HEALTH'
 ORDER BY CREATED_ON DESC;
SELECT EVENT_CODE,COUNT(*) GENERATED_COUNT,
       SUM(CASE WHEN STATUS='SENT' THEN 1 ELSE 0 END) SENT,
       SUM(CASE WHEN STATUS='PENDING' THEN 1 ELSE 0 END) PENDING,
       SUM(CASE WHEN STATUS='FAILED' THEN 1 ELSE 0 END) FAILED,
       SUM(CASE WHEN STATUS='SENT' AND NVL(RETRY_COUNT,0)>0 THEN 1 ELSE 0 END) RETRIED_SUCCESSFULLY,
       MAX(SENT_ON) LAST_SUCCESSFUL_SEND
  FROM T_AU_IID_EMAIL_QUEUE
 WHERE EVENT_CODE IN ('MGMT_AUDIT_PARA_STATUS','MGMT_AUDIT_WEEKLY_PARA_STATUS','MGMT_AUDIT_MAPPING_EXCEPTION')
 GROUP BY EVENT_CODE ORDER BY EVENT_CODE;

PROMPT 16. Oracle object, index and scheduler validity (.NET build is a deployment-host control)
SELECT OBJECT_NAME,OBJECT_TYPE,STATUS FROM USER_OBJECTS
 WHERE OBJECT_NAME IN ('PKG_INQ','PKG_AE','PKG_IAS_NOTIFICATION','V_IAS_POST_COMPLIANCE_NOTIFY','V_IAS_MGMT_AUDIT_NOTIFY_MAP')
 ORDER BY OBJECT_NAME,OBJECT_TYPE;
SELECT INDEX_NAME,STATUS FROM USER_INDEXES WHERE INDEX_NAME='UQ_IID_EMAIL_SCHED_NOTIFY';
SELECT NAME,TYPE,LINE,POSITION,TEXT FROM USER_ERRORS
 WHERE NAME IN ('PKG_INQ','PKG_AE','PKG_IAS_NOTIFICATION','V_IAS_POST_COMPLIANCE_NOTIFY','V_IAS_MGMT_AUDIT_NOTIFY_MAP')
 ORDER BY NAME,SEQUENCE;

PROMPT Exactly one V_IAS_POST_COMPLIANCE_NOTIFY row per COM_ID
SELECT COM_ID,COUNT(*) ROW_COUNT
  FROM V_IAS_POST_COMPLIANCE_NOTIFY
 GROUP BY COM_ID HAVING COUNT(*)<>1;

PROMPT Friday exception source excludes inactive or non-auditable entities (must return zero)
SELECT COUNT(*) NON_OPERATIONAL_ROWS
  FROM V_IAS_MGMT_AUDIT_NOTIFY_MAP M
  JOIN T_AUDITEE_ENTITIES E ON E.ENTITY_ID=M.ENTITY_ID
 WHERE NVL(E.ACTIVE,'N')<>'Y' OR NVL(E.AUDITABLE,'N')<>'Y';

PROMPT Division-level Reporting Office/Group conflicts (must appear as mapping exceptions and have no CC)
SELECT DIVISION_ID,DIVISION_NAME,MAX(DIVISION_REPORTING_COUNT) SOURCE_REPORTING_COUNT,
       COUNT(DISTINCT REPORTING_ID) RESOLVED_REPORTING_COUNT,
       SUM(CASE WHEN MAPPING_READY='N' THEN 1 ELSE 0 END) EXCEPTION_ENTITIES
  FROM V_IAS_MGMT_AUDIT_NOTIFY_MAP
 GROUP BY DIVISION_ID,DIVISION_NAME
 ORDER BY DIVISION_NAME;

PROMPT Deployment complete. Review all result sets above before release sign-off.
