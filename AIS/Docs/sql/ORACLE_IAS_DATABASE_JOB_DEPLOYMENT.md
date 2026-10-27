# Oracle-only IAS DATABASE Scheduler: install and verify

This change needs **zero ASP.NET/C# or frontend files**. It preserves all existing
scheduler UI and the `T_IAS_JOB_EXECUTION` history.

## Root cause of original failure

The existing ASP.NET background worker calls
`PKG_IAS_SCHEDULER.P_CLAIM_DUE_JOB(IO_CURSOR => ...)` and has no approved
`IIasSchedulerDatabaseJobExecutor` registered for
`PKG_DAILY.SESSION_KILL_DAY_END`. After claiming, it marks the run as failed
without calling Oracle. Merely marking `EXECUTION_TYPE='DATABASE'` never
created an Oracle DBMS_SCHEDULER job.

## Routing after deployment

- The unchanged ASP.NET worker still calls
  `P_CLAIM_DUE_JOB(IO_CURSOR => ...)`, which now defaults to `APPLICATION`.
- The new Oracle `IAS_DATABASE_JOB_POLLER` calls
  `PKG_IAS_DB_JOB_WORKER.P_POLL`, which claims exclusively `DATABASE` runs
  (including outstanding Run Now requests).
- Only `PKG_DAILY.SESSION_KILL_DAY_END` is permitted inside the worker.
  An unapproved database procedure is failed safely and **never executed**.
- `P_COMPLETE_JOB` and `P_FAIL_JOB` continue to manage regular
  `T_IAS_JOB_EXECUTION` status, retries, periods and history.
- `T_IAS_DAY_END_RUN` and `T_IAS_DAY_END_STEP` record the real
  statement-level times and seven `SQL%ROWCOUNT` counts.

## Execute in this order (QA first)

1. Back up current Oracle package source and capture metadata for job 2/schedule 2.
2. Run `ias_day_end_job_logging.sql` **once**. It contains DDL and SELECTs.
3. Compile `PKG_DAILY.sql` (package declaration, then body).
4. Compile `deploy_ias_scheduler_package_only.sql` (spec and body only).
   Do **not** execute the historical schema-creation DDL at the beginning of
   `PKG_IAS_SCHEDULER.sql`. The updated spec accepts optional
   `P_EXECUTION_TYPE`; existing app callers are unchanged.
5. Compile `PKG_IAS_DB_JOB_WORKER.sql`.
6. Check `USER_ERRORS` for all three package objects, and ensure Oracle
   `CREATE JOB` privilege is available to the schema owner.
7. Run `install_oracle_ias_db_poller.sql`. The job is created **DISABLED**.
8. On **isolated QA**, create a Run Now request from the existing UI for
   schedule 2. The unchanged app worker will now ignore DATABASE runs.
9. In the Oracle QA session, manually invoke
   `DBMS_SCHEDULER.RUN_JOB('IAS_DATABASE_JOB_POLLER',
   USE_CURRENT_SESSION => TRUE)` while the poller is disabled.
   This does process the queued Run Now request.
10. Verify the steps and rollback scenario, row totals, and both scheduler
    statuses. Also check that normal APPLICATION jobs are still processed.
11. Only after satisfactory QA, enable the DBMS scheduler job with
    `DBMS_SCHEDULER.ENABLE('IAS_DATABASE_JOB_POLLER')` in the intended database.

**Never Run Now during active office use**: the job deactivates every active
`T_USER_SESSION` row. The HR/employee data rebuild is also substantial.
Do not enable the Oracle poller until all packages compile.

**Do not rerun the old failed attempt**. It remains FAILED in history.
Issue a fresh Run Now request after the database changes are deployed.

## Verification queries

```sql
-- Existing IAS run status, in chronological order
SELECT E.EXECUTION_ID, E.RUN_SOURCE, E.STATUS,
       E.STARTED_ON, E.COMPLETED_ON,
       E.RECORDS_PROCESSED, E.ERROR_MESSAGE
  FROM T_IAS_JOB_EXECUTION E
 WHERE E.SCHEDULE_ID = 2
 ORDER BY E.EXECUTION_ID DESC
 FETCH FIRST 20 ROWS ONLY;

-- Oracle run-level logging correlated with IAS execution
SELECT R.RUN_ID, R.IAS_EXECUTION_ID, R.STATUS,
       R.STARTED_ON, R.COMPLETED_ON,
       R.TOTAL_ROWS_AFFECTED, R.ERROR_MESSAGE
  FROM T_IAS_DAY_END_RUN R
 ORDER BY R.RUN_ID DESC
 FETCH FIRST 20 ROWS ONLY;

-- Detail counts: seven DML statements with precise timestamps
SELECT S.RUN_ID, S.STEP_CODE, S.ACTION_TYPE, S.ROWS_AFFECTED,
       S.STARTED_ON, S.COMPLETED_ON, S.STATUS, S.ERROR_MESSAGE
  FROM T_IAS_DAY_END_STEP S
 WHERE S.RUN_ID = :RUN_ID
 ORDER BY S.STEP_ID;

-- Native Oracle Scheduler status (not app logs)
SELECT JOB_NAME, ENABLED, STATE, LAST_START_DATE, NEXT_RUN_DATE
  FROM USER_SCHEDULER_JOBS
 WHERE JOB_NAME = 'IAS_DATABASE_JOB_POLLER';

SELECT LOG_DATE, STATUS, ERROR#, ADDITIONAL_INFO
  FROM USER_SCHEDULER_JOB_RUN_DETAILS
 WHERE JOB_NAME = 'IAS_DATABASE_JOB_POLLER'
 ORDER BY LOG_DATE DESC;
```

## Transaction and failure semantics

Unlike the original `PKG_DAILY`, the seven DML statements are committed
together. If a downstream insert fails, Oracle rolls back all seven; the
autonomous logging entries are tagged `ROLLED_BACK` with zero committed
row total, and the scheduler receives `FAILED`.

If work *might* have committed but the scheduler completion cannot safely be
recorded, the worker sets `RECONCILIATION_REQUIRED` (including manual
request status) rather than claiming it failed and allowing unsafe reruns.

A row count is a **DML affected rows count**, not a unique-person count.
For example, a DELETE of 10,000 followed by INSERT of 10,000 counts 20,000
affected rows. The previous session cursor counted today's active logins;
that did not match the UPDATE, which closes **all** active sessions.

The 1-minute Oracle polling job is independent of the ASP.NET process.
It does not require browser sessions or application handlers. The
existing `MAX_RETRY=0` for schedule 2 is preserved.

## Validation limitations

These are committed source/migration changes only. They have **not** been
compiled in a real Oracle 19c instance, run against HR data or deployed to
production. QA should test procedure grants, definer rights, views and
triggers, sufficient UNDO capacity for a single-transaction refresh, stale
execution handling and Oracle Scheduler privileges.
