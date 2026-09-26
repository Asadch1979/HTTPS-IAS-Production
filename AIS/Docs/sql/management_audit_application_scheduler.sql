/*
Management Audit weekly notification database cutover (Oracle 18c compatible).

Required deployment order:
  1. Run this database script to create the execution ledger, then deploy the complete controlled
     PKG_IAS_NOTIFICATION specification/body from
     ias_notification_centralization.sql.
  2. Deploy the ASP.NET application while ManagementAuditWeekly:Enabled=false.
  3. After database and application validation, set
     ManagementAuditWeekly:Enabled=true and restart the application.

The application host must remain running for the configured weekly schedule.
Do not run an older script that recreates JOB_MGMT_AUDIT_WEEKLY_NOTIFY.
DDL commits implicitly; run during an application maintenance window.
*/

DECLARE
  N NUMBER;
BEGIN
  SELECT COUNT(*) INTO N
    FROM USER_TABLES
   WHERE TABLE_NAME='T_IAS_NOTIFY_EXECUTION';

  IF N=0 THEN
    EXECUTE IMMEDIATE 'CREATE TABLE T_IAS_NOTIFY_EXECUTION (
      EXECUTION_KEY VARCHAR2(100 CHAR) PRIMARY KEY,
      FINGERPRINT VARCHAR2(64 CHAR) NOT NULL,
      STATUS VARCHAR2(16 CHAR) NOT NULL,
      CREATED_ON TIMESTAMP WITH TIME ZONE DEFAULT SYSTIMESTAMP NOT NULL,
      UPDATED_ON TIMESTAMP WITH TIME ZONE,
      RETRY_COUNT NUMBER DEFAULT 0 NOT NULL,
      RESPONSE CLOB,
      CONSTRAINT CK_IAS_NOTIFY_EXEC_STATUS CHECK (STATUS IN (''RUNNING'',''COMPLETE'',''FAILED'')))';
  END IF;
END;
/

/*
The table is an execution ledger, not an email queue. A RUNNING request with
an unknown outcome must be reconciled against history and SMTP logs before it
is reset.
*/
MERGE INTO T_IAS_NOTIFY_EXECUTION D
USING (
  SELECT 'WEEKLY:'||TO_CHAR(REF_ID1,'FM00000000')||':'||TO_CHAR(REF_ID2,'TM9') EXECUTION_KEY,
         CASE WHEN MAX(CASE WHEN STATUS='SENT' THEN 1 ELSE 0 END)=1
              THEN 'COMPLETE' ELSE 'RUNNING' END STATUS
    FROM T_EMAIL_QUEUE
   WHERE EVENT_CODE='MGMT_AUDIT_WEEKLY_PARA_STATUS'
     AND REF_ID1 IS NOT NULL
     AND REF_ID2 IS NOT NULL
   GROUP BY REF_ID1,REF_ID2
) Q
ON (D.EXECUTION_KEY=Q.EXECUTION_KEY)
WHEN NOT MATCHED THEN INSERT
  (EXECUTION_KEY,FINGERPRINT,STATUS,RESPONSE)
VALUES
  (Q.EXECUTION_KEY,'WEEKLY',Q.STATUS,
   'Legacy weekly queue record: reconcile with T_EMAIL_QUEUE before retry.');

COMMIT;

/* Retire every weekly Oracle job. Mapping and health jobs are unchanged. */
BEGIN
  FOR J IN (
    SELECT JOB_NAME
      FROM USER_SCHEDULER_JOBS
     WHERE JOB_NAME='JOB_MGMT_AUDIT_WEEKLY_NOTIFY'
        OR UPPER(JOB_ACTION) LIKE '%SEND_MGMT_AUDIT_WEEKLY%'
  ) LOOP
    DBMS_SCHEDULER.DROP_JOB(J.JOB_NAME,FORCE=>TRUE);
  END LOOP;
END;
/

UPDATE IAS_NOTIFICATION_MASTER
   SET RELATED_PROCEDURE='ManagementAuditWeeklyService.RunPeriodAsync',
       CALLING_PROCESS='ASP.NET BackgroundService',
       DATA_SOURCE_NAME='V_IAS_MGMT_WEEKLY_DATA + T_IAS_NOTIFY_EXECUTION',
       DELIVERY_MECHANISM='EmailNotification -> EmailConfiguration SMTP',
       UPDATED_BY=USER,
       UPDATED_ON=SYSTIMESTAMP
 WHERE NOTIFICATION_CODE='MGMT_AUDIT_WEEKLY_PARA_STATUS';

UPDATE IAS_NOTIFICATION_MASTER
   SET RELATED_PROCEDURE='EmailNotification.NotifyManagementAuditParaStatus',
       DELIVERY_MECHANISM='EmailConfiguration SMTP',
       UPDATED_BY=USER,
       UPDATED_ON=SYSTIMESTAMP
 WHERE NOTIFICATION_CODE='MGMT_AUDIT_PARA_STATUS';

COMMIT;

DECLARE
  INVALID_OBJECTS NUMBER;
  WEEKLY_JOBS NUMBER;
BEGIN
  SELECT COUNT(*) INTO INVALID_OBJECTS
    FROM USER_OBJECTS
   WHERE OBJECT_NAME='T_IAS_NOTIFY_EXECUTION'
     AND STATUS<>'VALID';

  SELECT COUNT(*) INTO WEEKLY_JOBS
    FROM USER_SCHEDULER_JOBS
   WHERE JOB_NAME='JOB_MGMT_AUDIT_WEEKLY_NOTIFY'
      OR UPPER(JOB_ACTION) LIKE '%SEND_MGMT_AUDIT_WEEKLY%';

  IF INVALID_OBJECTS>0 THEN
    RAISE_APPLICATION_ERROR(-20811,
      'Management Audit weekly execution ledger validation failed.');
  END IF;

  IF WEEKLY_JOBS>0 THEN
    RAISE_APPLICATION_ERROR(-20812,
      'The retired Management Audit weekly Oracle Scheduler job still exists.');
  END IF;
END;
/

SELECT EXECUTION_KEY,STATUS,RETRY_COUNT,CREATED_ON,UPDATED_ON
  FROM T_IAS_NOTIFY_EXECUTION
 WHERE EXECUTION_KEY LIKE 'WEEKLY:%'
 ORDER BY CREATED_ON DESC;
