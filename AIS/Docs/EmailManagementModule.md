# Standalone Email Management Module

## Isolation boundary

The module is an administration and manual-testing facility only. It has no calls
from existing AIS controllers, workflows, notification helpers, database
procedures, queues, or scheduled jobs.

It uses:

- `EmailManagementController`
- `StandaloneEmailManagementService`
- the Email Management methods appended to the `DBConnection` partial class
- `EmailManagement:Smtp` configuration
- `PKG_EMAIL`
- database objects prefixed with `EM_EMAIL_`

It does not use or modify the behavior of `EmailConfiguration`,
`EmailNotification`, `T_AU_EMAIL_QUEUE`, `T_AU_IID_EMAIL_QUEUE`,
`T_AU_EMAIL_TRIGGER_LOG`, or `EMAILLOG`.

## Deployment

1. Review and run `Docs/sql/email_management_deploy.sql` as the AIS schema owner.
2. Confirm both `PACKAGE` and `PACKAGE BODY` are valid:

```sql
SELECT object_name, object_type, status
FROM user_objects
WHERE object_name = 'PKG_EMAIL';

SELECT line, position, text
FROM user_errors
WHERE name = 'PKG_EMAIL'
ORDER BY sequence;
```

3. Supply the standalone SMTP secret through deployment configuration, normally
   `EmailManagement__Smtp__Password`. Do not store it in source control.
4. Browse to `/EmailManagement`. Only authenticated role-1 administrators are
   authorized.

The checked-in configuration is deliberately incomplete, so test sends are
recorded as `SKIPPED` until standalone SMTP settings are provided.

## Processing

An administrator creates a normalized event, creates one or more versioned
templates, defines test placeholders and optional default recipient metadata,
then assigns an active template to the event.

Preview loads active placeholders, substitutes their test values into subject
and body tokens, and sanitizes the resolved HTML. Unknown tokens remain visible
to make incomplete configuration obvious.

Manual test send validates that the event and template are enabled, normalizes
To/CC/BCC addresses, creates an autonomous `PENDING` log, and submits the message
using the standalone SMTP configuration. SMTP acceptance changes the status to
`SENT_TO_SMTP`; it is not represented as delivery confirmation.

Logging failures are written to the application logger and do not crash the
manual-send request. SMTP credentials and passwords are never written to the
database log.

## Statuses

- `PENDING`: the test attempt was recorded before SMTP submission.
- `SENT_TO_SMTP`: SMTP accepted the message for onward transmission.
- `SEND_FAILED`: SMTP submission or message preparation failed.
- `SKIPPED`: sending was intentionally not attempted, such as disabled event,
  missing configuration, missing template, or missing recipient.
- `BOUNCED`: reserved for future reliable bounce processing.
- `DELIVERED`: reserved for future reliable delivery confirmation.

## Rollback

Run `Docs/sql/email_management_rollback.sql` from the `Docs/sql` directory. It
drops only `EM_EMAIL_*` objects and restores the repository's original
`pkg_email.sql` package implementation.
