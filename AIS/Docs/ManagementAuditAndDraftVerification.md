# IAS functional review — 6 October 2026

This completion used static verification only: code paths, method signatures,
SQL cursor contracts, package specification/body consistency and git diff checks.
No tests, Node scripts, SMTP simulations or legacy harnesses were run. Earlier
test edits and generated build/test logs are excluded from the main integration.

## Scheduler manual retry

Retry requires an authenticated superuser with page permission and an anti-forgery
token. `P_RETRY_RUN_REQUEST` locks the original request, accepts only FAILED
requests and rejects active or uncertain executions. Request identity, job,
schedule, period, original requester and remarks are retained. Claiming allocates
a new execution with `MAX(RETRY_NO)+1`, atomically with the CLAIMED transition.
Manual completion/failure/reconciliation return before schedule updates,
preserving `NEXT_DUE_ON`.

Explicit Management Audit manual retries reclaim only FAILED checkpoints without
the automatic delay/limit. COMPLETE divisions remain skipped; RUNNING/uncertain
checkpoints stay protected. Outstanding checkpoints cause failure rather than
incorrectly reporting scheduler success.

## Management Audit emails

Immediate emails accept only Settled and recognized Referred Back/Rejected
decisions. Unknown/forwarded statuses are ignored. TO and CC remain supplied by
the review procedure. Optional REPORT/DEPT/DIV columns are read only when present,
preventing a missing display field from breaking the response after the decision
commits. Referred Back wording and reason are used consistently.

Consolidated delivery remains owned by the centralized IAS scheduler. Its claimed
job supplies the period. The database end is exclusive; email display subtracts
one day. The handler retains its scoped service provider for email attempt logs.
SMTP sending is direct; these messages do not enter the generic Oracle email
queue. Execution checkpoints prevent duplicates and preserve uncertain outcomes.

Summary and detail select decisions from the same weekly view with matching
period/status/latest-event rules. No-compliance queries use the same open-para
population and exclude decisions already included in the period. The decision
cursor returns RISK as required by C#. Last submission dates are mapped and
displayed. The weekly view no longer excludes records due to missing legacy
mapping emails; active Management Audit configuration governs TO/CC/BCC values
and eligibility.

## AR Add to Draft

Visibility and package authorization use engagement assignment instead of the
Team Lead flag. Assigned leads and members may move status-3 observations to
Draft. The package checks authenticated PPNO against the same member/task/active
engagement relationship used by the dropdown. Unassigned users and other states
are rejected. Settlement/edit/other action permissions are preserved. Deploy the
changed Oracle body together with the application.

## Post Compliance buttons

The production implementation already recalculates labels, visibility and
disabled state for the selected para through `setReviewButtonAvailability`.
Recommend/Refer Back/Close buttons are hidden rather than permanently removed
from the shared modal. This fix is preserved in main.

## Deployment and outstanding live verification

Deploy `PKG_IAS_SCHEDULER` specification/body, the changed
`PKG_AR.P_Add_Observation_To_Draft` body, and `PKG_MGMT_AUDIT_WEEKLY`
specification/body. Replace `V_IAS_MGMT_WEEKLY_DATA` using its updated definition
in `sql/ias_notification_centralization.sql` before consolidated delivery resumes.
Preserve unrelated deployed package differences. Verify USER_ERRORS and
package/view validity, then deploy/restart IAS under IIS.

Live checks remain necessary for retry sequence/concurrency and unchanged Next
Due; lead/member/outsider/status authorization; actual cursor schemas, risk/date
values and summary/detail counts; configured TO/CC/BCC and SMTP delivery;
attempt/checkpoint logging and duplicate/uncertain outcomes; and repeated use of
the shared Post Compliance modal. No live Oracle/IIS or SMTP verification is
claimed by this static review.
