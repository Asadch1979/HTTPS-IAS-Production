# Management Audit Notification Deployment

This is the only supported deployment path for Management Audit notifications.
The generic Oracle email queue is owned by `PKG_EMAIL`; weekly delivery is owned
by the ASP.NET `ManagementAuditWeeklyService`.

## Required order

1. Keep `ManagementAuditWeekly:Enabled` set to `false`.
2. Run `email_notification_architecture_refactor.sql` to create or validate the
   generic `T_EMAIL_QUEUE` foundation.
3. Run the complete `pkg_email.sql` package specification and body.
4. Run `management_audit_application_scheduler.sql` to create the application
   execution ledger/view and drop every retired weekly Oracle Scheduler job.
5. Run the complete `ias_notification_centralization.sql` package script.
6. Confirm `PKG_EMAIL`, `PKG_IAS_NOTIFICATION`, `T_IAS_NOTIFY_EXECUTION`, and
   `V_IAS_MGMT_WEEKLY_DATA` are valid, and confirm no Oracle job invokes the
   retired weekly procedure.
7. Deploy the ASP.NET application and complete its notification-control tests
   and build while weekly execution remains disabled.
8. Set `ManagementAuditWeekly:Enabled` to `true`, restart the application, and
   keep the application host running for the configured weekly schedule.

The files under `management_audit_notification_steps` and the dated Management
Audit deployment, rollback, schedule, and verification SQL files are retained
only as fail-fast retirement markers. They must not be used for deployment.
