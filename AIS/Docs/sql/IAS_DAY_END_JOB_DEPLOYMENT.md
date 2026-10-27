# IAS Day-End Database Job: deployment and verification

Applies to scheduled DATABASE job 2 / schedule 2:
`PKG_DAILY.SESSION_KILL_DAY_END` at 23:59 Asia/Karachi.

## Important architecture

The existing IAS scheduler polls in ASP.NET every minute and claims the due
execution in `PKG_IAS_SCHEDULER.P_CLAIM_DUE_JOB`. DATABASE tasks still require
an explicitly registered `IIasSchedulerDatabaseJobExecutor`; this change
registers ONLY `PKG_DAILY.SESSION_KILL_DAY_END`, with a fixed ODP.NET
stored procedure command. The actual day-end processing and all audit logs
remain inside Oracle; no application handler/email/user UI is involved.

## Deployment order

1. Test on an isolated development/QA database with representative copies
   of the seven affected data sets (this job logs out all active sessions).
2. Apply `AIS/Docs/sql/ias_day_end_job_logging.sql` ONCE. Its SELECTs at
   the bottom show existing history; initially they return zero rows.
3. Compile `AIS/Docs/sql/PKG_DAILY.sql` (package spec and body).
4. Check `USER_ERRORS` for both package and package body, and verify SELECT,
   INSERT, UPDATE and DELETE privileges on the existing sources/targets and
   INSERT/UPDATE on the new log tables.
5. Deploy the ASP.NET code (new typed executor, DB connection method, DI).
   No change is needed to the existing job name, procedure name or schedule.
6. Validate `T_IAS_JOB_DEFINITION` and `T_IAS_JOB_SCHEDULE` for Job ID 2
   and Schedule ID 2. Confirm 23:59 Karachi and that `IS_ACTIVE='Y'`.
7. Only after the above, test a scheduled or deliberate Run Now execution
   outside business hours. Run Now invokes the full refresh and deactivates
   all sessions. Do not test it during active user hours.
8. Review `T_IAS_JOB_EXECUTION` alongside `T_IAS_DAY_END_RUN` and
   `T_IAS_DAY_END_STEP`; verify counts, states, and each timestamp.

```sql
SELECT d.job_id, d.job_name, d.execution_type, d.package_name,
       d.procedure_name, s.schedule_id, s.is_active, s.next_due_on,
       s.last_status, s.last_error
  FROM t_ias_job_definition d
  JOIN t_ias_job_schedule s ON s.job_id = d.job_id
 WHERE s.schedule_id = 2;

SELECT e.execution_id, e.status, e.started_on, e.completed_on,
       e.records_processed, e.response_message, e.error_message
  FROM t_ias_job_execution e
 WHERE e.schedule_id = 2
 ORDER BY e.execution_id DESC
 FETCH FIRST 20 ROWS ONLY;

SELECT r.run_id, r.ias_execution_id, r.status,
       r.started_on, r.completed_on, r.total_rows_affected, r.error_message
  FROM t_ias_day_end_run r
 ORDER BY r.run_id DESC
 FETCH FIRST 20 ROWS ONLY;

SELECT s.run_id, s.step_code, s.action_type, s.rows_affected,
       s.started_on, s.completed_on, s.status, s.error_message
  FROM t_ias_day_end_step s
 WHERE s.run_id = :run_id
 ORDER BY s.step_id;
```

## Meaning of status and totals

- **COMMITTED**: DML row counts for a successful fully committed run.
- **ROLLED_BACK**: the SQL step executed but its changes were rolled back.
  Its attempted count must not be treated as a committed modification.
- **FAILED**: a SQL operation failed; its actual successful row count was
  not established and is logged as zero with the Oracle error.
- **RECONCILIATION_REQUIRED**: the data transaction committed, but recording
  final success encountered an error. Investigate before any manual rerun.
- **TOTAL_ROWS_AFFECTED**: the sum of seven DML statements, including
  DELETE+INSERT pairs. This is NOT a distinct employee count.

The modified package commits the seven DML operations as one transaction
instead of committing after each DELETE/INSERT. This avoids leaving refresh
tables empty if a later step fails, but needs DBA/QA validation for undo
capacity and any trigger/remote database effects.

The source UPDATE changes *all* rows with `SESSION_ACTIVE='Y'`, regardless of
login date. The earlier cursor counted only today's logins; the new count
reflects the actual rows updated.

The current schedule has `MAX_RETRY=0`; one failure does not produce an
automatic retry. A run marked `RECONCILIATION_REQUIRED` must be investigated
before additional executions are initiated.

## Test limitations

Unit-style integration tests cover registration and code invariants.
A real Oracle 19c integration test and full database restore/rollback exercise
are required before production. GitHub commits alone do not update the Oracle
package or create its tables.
