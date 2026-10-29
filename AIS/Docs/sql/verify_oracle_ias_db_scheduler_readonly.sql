-- IAS DATABASE scheduler: read-only preflight, safe for production inspection.
-- This does not run day-end work, change statuses, create jobs or grant privileges.
-- The existing ASP.NET dispatcher does NOT execute PKG_DAILY.SESSION_KILL_DAY_END.
-- Only PKG_IAS_DB_JOB_WORKER.P_POLL may claim approved DATABASE work.
--
-- 1) Confirm actual account and objects. Package VALID alone is insufficient.
SELECT USER AS connected_as,
       SYS_CONTEXT('USERENV','CURRENT_SCHEMA') AS current_schema
  FROM dual;

SELECT object_name,object_type,status
  FROM user_objects
 WHERE object_name IN ('PKG_IAS_SCHEDULER','PKG_IAS_DB_JOB_WORKER',
                       'PKG_DAILY','T_IAS_DAY_END_RUN','T_IAS_DAY_END_STEP',
                       'T_IAS_JOB_EXECUTION','T_IAS_JOB_DEFINITION')
 ORDER BY object_name,object_type;

SELECT name,type,line,position,text
  FROM user_errors
 WHERE name IN ('PKG_IAS_SCHEDULER','PKG_IAS_DB_JOB_WORKER','PKG_DAILY')
 ORDER BY name,sequence;

-- 2) The database claim routine must accept P_EXECUTION_TYPE.
--    App callers omit it (default APPLICATION); Oracle worker supplies DATABASE.
SELECT package_name,object_name,argument_name,position,in_out,data_type,defaulted
  FROM user_arguments
 WHERE package_name = 'PKG_IAS_SCHEDULER'
   AND object_name = 'P_CLAIM_DUE_JOB'
 ORDER BY sequence;

-- 3) Inspect the exact allowlisted job and schedule metadata.
SELECT job_id,job_code,job_name,execution_type,
       package_name,procedure_name,application_handler,is_active
  FROM t_ias_job_definition
 WHERE UPPER(package_name)='PKG_DAILY'
   AND UPPER(procedure_name)='SESSION_KILL_DAY_END';

SELECT s.schedule_id,s.job_id,s.frequency_type,s.is_active,
       s.last_run_on,s.last_status,s.last_error,s.next_due_on,s.max_retry
  FROM t_ias_job_schedule s
 WHERE s.job_id IN
       (SELECT d.job_id FROM t_ias_job_definition d
         WHERE UPPER(d.package_name)='PKG_DAILY'
           AND UPPER(d.procedure_name)='SESSION_KILL_DAY_END');

-- 4) Oracle-native worker and poller: do not auto-create or enable it here.
SELECT job_name,enabled,state,job_type,job_action,repeat_interval,
       last_start_date,next_run_date
  FROM user_scheduler_jobs
 WHERE job_name='IAS_DATABASE_JOB_POLLER';

SELECT log_date,status,error#,additional_info
  FROM user_scheduler_job_run_details
 WHERE job_name='IAS_DATABASE_JOB_POLLER'
 ORDER BY log_date DESC FETCH FIRST 20 ROWS ONLY;

-- 5) Existing failed executions are historical evidence, not pending work.
--    Do NOT reset these to PENDING or RUNNING.
SELECT execution_id,job_id,schedule_id,run_source,status,
       started_on,completed_on,records_processed,error_message
  FROM t_ias_job_execution
 WHERE job_id IN
       (SELECT d.job_id FROM t_ias_job_definition d
         WHERE UPPER(d.package_name)='PKG_DAILY'
           AND UPPER(d.procedure_name)='SESSION_KILL_DAY_END')
 ORDER BY execution_id DESC FETCH FIRST 20 ROWS ONLY;

-- 6) Database-only per-step logging after the first approved QA run.
--    Skip these queries if the logging tables have not yet been installed.
-- SELECT run_id,ias_execution_id,status,started_on,completed_on,
--        total_rows_affected,error_message
--   FROM t_ias_day_end_run ORDER BY run_id DESC FETCH FIRST 20 ROWS ONLY;
-- SELECT run_id,step_code,action_type,rows_affected,status,started_on,
--        completed_on,error_message
--   FROM t_ias_day_end_step ORDER BY step_id DESC FETCH FIRST 30 ROWS ONLY;
--
-- REMEDIATION IN APPROVED QA ONLY, never run automatically:
-- AIS/Docs/sql/ias_day_end_job_logging.sql (one-time DDL)
-- AIS/Docs/sql/PKG_DAILY.sql
-- AIS/Docs/sql/deploy_ias_scheduler_package_only.sql
-- AIS/Docs/sql/PKG_IAS_DB_JOB_WORKER.sql
-- AIS/Docs/sql/install_oracle_ias_db_poller.sql (creates disabled)
--
-- Ensure P_CLAIM_DUE_JOB defaults to APPLICATION in the installed package.
-- Only then can ASP.NET stop claiming DATABASE work. Confirm no stale RUNNING
-- attempts and no duplicate pending requests before manual QA acceptance.
-- APPROVAL GATE: do not run the disabled poller or enable it in production
-- until authorized tests prove safe session shutdown, HR refresh, rollback,
-- step counts, idempotency, schedule timezone and audit history.
