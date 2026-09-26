/*
RETIRED: legacy IAS notification deployment artifact.

Do not execute this file. It is intentionally non-deployable because it used
an Inquiry-owned email queue and/or the retired weekly Oracle Scheduler flow.
Use AIS/Docs/sql/management_audit_notification_deployment.md for the controlled
generic email architecture and application-service deployment order.
*/
BEGIN
  RAISE_APPLICATION_ERROR(-20998,
    'Retired notification deployment artifact. Use management_audit_notification_deployment.md.');
END;
/
