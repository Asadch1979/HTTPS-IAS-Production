# Management notifications and AR draft correction — 5 October 2026

## Findings and changes

* Immediate decisions: `PKG_AE.P_SubmitPostAuditCompliance_Review` commits the
  decision before returning its cursor. The C# reader required `REPORT`, `DEPT`
  and `DIV`, which the checked-in procedure does not return. Missing display
  fields could therefore throw after the decision committed and before email
  delivery. The reader now tolerates those optional fields. The Management Audit
  renderer accepts only Settled or recognized Referred Back/Rejected decisions;
  forwarding/empty/unknown statuses cannot become rejection notifications.
* Consolidated delivery: the scheduler handler never assigned its service
  provider, disabling database email-attempt logging. It now receives the
  scheduler scope. A claim rejected because an execution is FAILED or RUNNING
  is reported as outstanding instead of silently completing the scheduler job.
  Completed divisions are still skipped, and uncertain deliveries remain RUNNING.
* Consolidated content: the no-compliance detail query included decisions which
  the summary excluded, and required a legacy mapping email even though recipients
  come from the administration tables. Summary and detail now use the same open
  para source and exclusions. Last submission dates are mapped, decision dates
  are checked against the reporting interval, and the displayed end date is
  inclusive (exclusive database end minus one day).
* AR Add to Draft: both UI and procedure depended on `ISTEAMLEAD = 'Y'`.
  Assignment and lead classification were conflated. The permitted fallback now
  allows either assigned team role without depending on this flag. The rendered
  permission comes from the AR engagement dropdown query; the procedure checks
  the same member/task/engagement relationship using the authenticated PPNO.
  Only status 3 observations in active dashboard engagements can move to draft.
  Settlement, editing, and other actions retain their previous permissions.
  The exact affected user's deployed assignment data has not been inspected.

## Sending, recipients and duplicate controls reviewed

Immediate Management Audit notifications apply to audit entities 112242/112248.
TO and both CC values remain those returned by the review procedure. Review
request IDs are claimed in `T_IAS_NOTIFY_EXECUTION`; replaying the same request
returns its previous response without executing the decision or send again.

The current consolidated trigger is `IasSchedulerBackgroundService` (one-minute
poll) -> `PKG_IAS_SCHEDULER` claim -> `ManagementAuditWeeklyService`. The database
job supplies its configured period; weekly frequency is a scheduler setting,
not an independent application timer. Each period/division has its own execution
claim. Active TO/CC/BCC recipients are read from the division summary, never from
detail rows. `EmailConfiguration` deduplicates recipients across TO/CC/BCC.

Both paths send directly through `EmailConfiguration`/SMTP with database attempt
logging. They do not enqueue these messages in `T_EMAIL_QUEUE`. The consolidated
division list plus execution ledger is the application work queue. Existing
retry limits and uncertain-outcome protection remain in place. SMTP acceptance
does not confirm delivery to the recipient's inbox.

## Verification performed

* Application and scheduler regression project compile successfully.
* `node tests/notification-controls/add-to-draft.cjs` passes: both assigned roles
  see and submit Add to Draft; unassigned users and other statuses cannot submit;
  Team Member settlement stays hidden. This executes the actual JavaScript in a
  simulated DOM, not a browser connected to Oracle.
* `node tests/notification-controls/repeated-click.cjs` passes: pending repeat
  clicks are suppressed and retries preserve the request ID.
* `git diff --check` passes.
* The baseline scheduler suite reached and exposed the incorrect inclusive email
  end date. The fix is included. The final suite, including new local SMTP,
  duplicate, failure, period-boundary and logging-provider regressions, compiles
  but could not run: Windows Application Control refused to load the rebuilt
  `AIS.dll` (0x800711C7), including on an approved unsandboxed retry.

Live Oracle changes, SMTP delivery and a real authenticated dashboard session
have **not** been verified. No test environment or disposable records were
designated during this run; no production workflow records were changed.

## Deployment and remaining end-to-end verification

1. Merge the changed `P_Add_Observation_To_Draft` body from `sql/PKG_AR.sql` into
   the target package and deploy `sql/pkg_mgmt_audit_weekly.sql`. Check package
   compilation errors. Preserve any unrelated differences in deployed packages.
2. Deploy the application. The draft backend permission change requires the
   Oracle package update; deploying JavaScript alone is insufficient.
3. On a host permitted to run the binaries, run
   `dotnet run --project AIS.SchedulerIntegration.Tests` and both Node checks.
   The SMTP regressions use loopback and example.test recipients only.
4. In a designated test database, verify assigned lead/member success on separate
   status-3 observations, outsider denial, other-state denial, duplicate draft
   number rejection, persisted status/remarks and audit logging.
5. With test recipients, settle and refer back disposable Management Audit
   compliances; replay the request ID and check one send per action. Verify
   cursor mapping, TO/CC and email-attempt status.
6. Run the scheduler for a known weekly period with settled, referred and
   no-compliance records. Check summary/detail counts, TO/CC/BCC, ledger and
   email-attempt logs; replay the period and verify no duplicate sends. Exercise
   failed and uncertain deliveries separately. Confirm the target scheduler's
   enabled weekly schedule and recipient configuration.

## Files changed

* `AIS/DBConnection.AE.cs`
* `AIS/DBConnection.ManagementAuditWeekly.cs`
* `AIS/EmailNotification.cs`
* `AIS/Services/ManagementAuditWeeklyService.cs`
* `AIS/Services/NotificationExecutionStore.cs`
* `AIS/Docs/sql/pkg_mgmt_audit_weekly.sql`
* `AIS/Controllers/FieldAuditController.cs`
* `AIS/Models/FieldAuditWorkflow/FieldAuditGridReplicaViewModel.cs`
* `AIS/Views/FieldAudit/_ManageObservationBranches.cshtml`
* `AIS/wwwroot/js/fieldAudit/manageObservationBranchesReplica.js`
* `AIS/Docs/sql/PKG_AR.sql`
* `AIS.SchedulerIntegration.Tests/Program.cs`
* `AIS.SchedulerIntegration.Tests/ManagementAuditRegressionTests.cs`
* `tests/notification-controls/add-to-draft.cjs`
* This verification report.

Pre-existing local settings, publish artifacts, notification-control test edits,
`.vs` and `App_Data` are excluded from the commit.
