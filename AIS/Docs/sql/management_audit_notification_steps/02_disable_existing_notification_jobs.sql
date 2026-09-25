/*
Step 02 - Safety gate.
Disables only the three Management Audit notification jobs if they already exist.
*/
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
