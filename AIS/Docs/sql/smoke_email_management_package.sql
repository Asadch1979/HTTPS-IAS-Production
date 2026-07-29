-- Smoke-tests PKG_EMAIL without sending SMTP mail.
-- Rolls back data changes so it can be run safely in non-production validation sessions.

SET SERVEROUTPUT ON

DECLARE
    v_event_id NUMBER;
    v_template_id NUMBER;
    v_template_id_2 NUMBER;
    v_rule_id NUMBER;
    v_placeholder_id NUMBER;
    v_attachment_id NUMBER;
    v_log_id NUMBER;
    v_count NUMBER;
    v_key VARCHAR2(100) := 'CODEX_SMOKE_' || TO_CHAR(SYSTIMESTAMP, 'YYYYMMDDHH24MISSFF3');
BEGIN
    PKG_EMAIL.UPSERT_EVENT(v_event_id, v_key, 'Codex smoke event', 'Created by smoke_email_management_package.sql', 1, 'codex-smoke');
    PKG_EMAIL.UPSERT_TEMPLATE(v_template_id, v_event_id, 'Default', 'en', 'Hello {User.Name}', '<p>Hello {User.Name}</p>', 1, 'codex-smoke');
    PKG_EMAIL.UPSERT_TEMPLATE(v_template_id_2, v_event_id, 'Default', 'en', 'Hello again {User.Name}', '<p>Hello again {User.Name}</p>', 1, 'codex-smoke');
    PKG_EMAIL.SET_ACTIVE_TEMPLATE(v_event_id, v_template_id_2, 'codex-smoke');
    PKG_EMAIL.UPSERT_RULE(v_rule_id, v_event_id, 'to@example.test', 'cc@example.test', NULL, 1, 'codex-smoke');
    PKG_EMAIL.UPSERT_PLACEHOLDER(v_placeholder_id, v_event_id, '{User.Name}', 'User name', 'Smoke User', 1, 'codex-smoke');
    PKG_EMAIL.UPSERT_ATTACHMENT(v_attachment_id, v_event_id, 'Deferred attachment', 'NONE', 'formal-deferral', NULL, 0, 'codex-smoke');
    PKG_EMAIL.CREATE_LOG(v_log_id, v_key, v_template_id_2, 2, 'Hello Smoke User', '<p>Hello Smoke User</p>',
        'to@example.test', 'cc@example.test', NULL, '[]', 'PENDING', NULL, 1, 'smoke', 'codex-smoke', v_key, 'smoke-ref', NULL);
    PKG_EMAIL.COMPLETE_LOG(v_log_id, 'SKIPPED', NULL, 'Smoke test skip.');

    SELECT COUNT(*) INTO v_count
      FROM EM_EMAIL_TEMPLATE
     WHERE EVENT_ID = v_event_id
       AND TEMPLATE_NAME = 'Default'
       AND VERSION_NO IN (1, 2);

    IF v_count <> 2 THEN
        RAISE_APPLICATION_ERROR(-20991, 'Template version smoke check failed.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('Standalone email-management smoke test passed for event ' || v_key || '.');
    ROLLBACK;
END;
/
