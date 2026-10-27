-- Deploy AFTER logging DDL and the three updated/added package scripts compile.
-- Creates a genuine Oracle Scheduler job, initially DISABLED for safe rollout.
-- Requires CREATE JOB and EXECUTE on DBMS_SCHEDULER in the execution schema.
DECLARE
  V_COUNT NUMBER;
BEGIN
  SELECT COUNT(*) INTO V_COUNT
    FROM USER_SCHEDULER_JOBS
   WHERE JOB_NAME = 'IAS_DATABASE_JOB_POLLER';

  IF V_COUNT = 0 THEN
    DBMS_SCHEDULER.CREATE_JOB(
      JOB_NAME        => 'IAS_DATABASE_JOB_POLLER',
      JOB_TYPE        => 'STORED_PROCEDURE',
      JOB_ACTION      => 'PKG_IAS_DB_JOB_WORKER.P_POLL',
      START_DATE      => SYSTIMESTAMP,
      REPEAT_INTERVAL => 'FREQ=MINUTELY;INTERVAL=1',
      ENABLED         => FALSE,
      AUTO_DROP       => FALSE,
      COMMENTS        => 'Oracle-only executor of allowlisted IAS DATABASE jobs; APPLICATION jobs remain separate.');
  END IF;
END;
/

-- Confirm package validity, schedule and worker state before enabling.
SELECT OBJECT_NAME, OBJECT_TYPE, STATUS
  FROM USER_OBJECTS
 WHERE OBJECT_NAME IN ('PKG_DAILY', 'PKG_IAS_SCHEDULER',
                       'PKG_IAS_DB_JOB_WORKER')
 ORDER BY OBJECT_NAME, OBJECT_TYPE;

SELECT NAME, TYPE, LINE, POSITION, TEXT
  FROM USER_ERRORS
 WHERE NAME IN ('PKG_DAILY', 'PKG_IAS_SCHEDULER',
                'PKG_IAS_DB_JOB_WORKER')
 ORDER BY NAME, SEQUENCE;

SELECT JOB_NAME, ENABLED, STATE, REPEAT_INTERVAL, NEXT_RUN_DATE
  FROM USER_SCHEDULER_JOBS
 WHERE JOB_NAME = 'IAS_DATABASE_JOB_POLLER';

-- ONLY after successful QA: enable periodic, fully Oracle-level execution.
-- BEGIN
--   DBMS_SCHEDULER.ENABLE('IAS_DATABASE_JOB_POLLER');
-- END;
-- /

-- On an approved QA environment, for a Run Now request already queued:
-- BEGIN
--   DBMS_SCHEDULER.RUN_JOB('IAS_DATABASE_JOB_POLLER', USE_CURRENT_SESSION => TRUE);
-- END;
-- /

-- To inspect Oracle poller errors:
-- SELECT LOG_DATE, STATUS, ERROR#, ADDITIONAL_INFO
--   FROM USER_SCHEDULER_JOB_RUN_DETAILS
--  WHERE JOB_NAME = 'IAS_DATABASE_JOB_POLLER'
--  ORDER BY LOG_DATE DESC;
