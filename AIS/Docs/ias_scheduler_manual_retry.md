# Deploying manual Run Now retry

Deploy the package change before enabling the updated application. Pause scheduler
workers during package replacement, then resume them after compilation succeeds.
For an existing scheduler installation, execute only the `CREATE OR REPLACE PACKAGE
PKG_IAS_SCHEDULER` specification and `CREATE OR REPLACE PACKAGE BODY
PKG_IAS_SCHEDULER` blocks from `sql/PKG_IAS_SCHEDULER.sql`. Terminate each block with
`/` in SQL*Plus or SQLcl. Do not rerun the table/sequence creation statements in
that bootstrap file. This change needs no table migration or data reset.

Verify both package objects are VALID in USER_OBJECTS and that USER_ERRORS has no
errors for PKG_IAS_SCHEDULER before resuming workers.

Retry accepts only FAILED requests, locks the original request, and queues it
without changing its stored period, original request context or normal Next Due.
The current EXECUTION_ID remains visible until the next claim replaces that
pointer; every historical execution row is retained. The claim derives the next
retry number from MAX(RETRY_NO) for RUN_REQUEST_ID while holding the request lock.
Execution insertion and CLAIMED transition commit together.

Only the Management Audit handler for a claimed MANUAL execution with a request
identity and RETRY_NO > 0 bypasses notification retry delay and the automatic
five-attempt limit. This allows another explicit recovery after repeated failures.
The atomic notification update still requires FAILED. COMPLETE deliveries are
skipped; RUNNING deliveries remain protected and cause the retry to report
unfinished work until their uncertain outcome is reconciled. Other automatic
notification callers retain their existing delay and attempt limit.

In a staging database, verify two concurrent Retry actions accept exactly one
transition; a failed claim leaves the request PENDING; R0, R1 and R2 remain in
history with identical stored periods; COMPLETE Division checkpoints produce no
additional emails; FAILED checkpoints can be reclaimed immediately; RUNNING
checkpoints cannot be reclaimed; and successful recovery sets the original
request to SUCCESS without changing NEXT_DUE_ON. These Oracle/SMTP checks require
a configured staging environment and are not covered by the local build.
