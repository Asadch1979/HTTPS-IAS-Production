CREATE OR REPLACE PACKAGE pkg_email AS
  /* Writer (upsert) routines */
  PROCEDURE upsert_event(p_event_key    IN VARCHAR2,
                         p_display_name IN VARCHAR2,
                         p_description  IN VARCHAR2 DEFAULT NULL,
                         p_is_enabled   IN NUMBER DEFAULT 1);

  PROCEDURE upsert_template(p_event_key    IN VARCHAR2,
                            p_culture      IN VARCHAR2 DEFAULT 'en',
                            p_subject_html IN CLOB,
                            p_body_html    IN CLOB,
                            p_updated_by   IN VARCHAR2 DEFAULT NULL);

  PROCEDURE upsert_rules(p_event_key    IN VARCHAR2,
                         p_sendto_roles IN VARCHAR2 DEFAULT NULL,
                         p_cc_roles     IN VARCHAR2 DEFAULT NULL,
                         p_bcc_roles    IN VARCHAR2 DEFAULT NULL,
                         p_sendto_users IN VARCHAR2 DEFAULT NULL,
                         p_cc_users     IN VARCHAR2 DEFAULT NULL,
                         p_bcc_users    IN VARCHAR2 DEFAULT NULL,
                         p_is_active    IN NUMBER DEFAULT 1);

  PROCEDURE add_or_replace_placeholder(p_event_key    IN VARCHAR2,
                                       p_token        IN VARCHAR2,
                                       p_resolver_key IN VARCHAR2,
                                       p_sample_value IN CLOB DEFAULT NULL);

  PROCEDURE add_or_replace_attachment_source(p_event_key      IN VARCHAR2,
                                             p_source_type    IN VARCHAR2,
                                             p_source_ref     IN VARCHAR2,
                                             p_file_name_pat  IN VARCHAR2 DEFAULT NULL,
                                             p_as_inline_html IN NUMBER DEFAULT 0);

  PROCEDURE log_email(p_event_key      IN VARCHAR2,
                      p_correlation_id IN VARCHAR2,
                      p_subject_sent   IN CLOB,
                      p_body_sent      IN CLOB,
                      p_to_csv         IN VARCHAR2,
                      p_cc_csv         IN VARCHAR2 DEFAULT NULL,
                      p_bcc_csv        IN VARCHAR2 DEFAULT NULL,
                      p_status         IN VARCHAR2,
                      p_error_message  IN CLOB DEFAULT NULL);

  /* Readers (return SYS_REFCURSOR) */
  PROCEDURE get_events(p_event_key IN VARCHAR2 DEFAULT NULL,
                       o_cur       OUT SYS_REFCURSOR);

  PROCEDURE get_templates_by_event(p_event_key IN VARCHAR2,
                                   o_cur       OUT SYS_REFCURSOR);

  PROCEDURE get_rules_by_event(p_event_key IN VARCHAR2,
                               o_cur       OUT SYS_REFCURSOR);

  PROCEDURE get_placeholders_by_event(p_event_key IN VARCHAR2,
                                      o_cur       OUT SYS_REFCURSOR);

  PROCEDURE get_attachment_sources_by_event(p_event_key IN VARCHAR2,
                                            o_cur       OUT SYS_REFCURSOR);

  PROCEDURE get_email_log(p_event_key      IN VARCHAR2 DEFAULT NULL,
                          p_from_utc       IN TIMESTAMP WITH TIME ZONE DEFAULT NULL,
                          p_to_utc         IN TIMESTAMP WITH TIME ZONE DEFAULT NULL,
                          p_status         IN VARCHAR2 DEFAULT NULL,
                          p_email_contains IN VARCHAR2 DEFAULT NULL,
                          o_cur            OUT SYS_REFCURSOR);

  /* Seed helper (calls the upsert routines; no direct INSERTS) */
  PROCEDURE seed_defaults;

END pkg_email;
/
CREATE OR REPLACE PACKAGE BODY pkg_email AS

  /* Helper: fetch EventId by key (raises if missing when needed) */
  FUNCTION get_event_id(p_event_key IN VARCHAR2, p_required IN NUMBER := 1)
    RETURN NUMBER IS
    v_id NUMBER;
  BEGIN
    SELECT EventId
      INTO v_id
      FROM NotificationEvents
     WHERE EventKey = p_event_key;
    RETURN v_id;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      IF p_required = 1 THEN
        RAISE_APPLICATION_ERROR(-20001,
                                'EventKey not found: ' || p_event_key);
      ELSE
        RETURN NULL;
      END IF;
  END;

  /* ========== Writers ========== */

  PROCEDURE upsert_event(p_event_key    IN VARCHAR2,
                         p_display_name IN VARCHAR2,
                         p_description  IN VARCHAR2,
                         p_is_enabled   IN NUMBER) IS
  BEGIN
    MERGE INTO NotificationEvents t
    USING (SELECT p_event_key AS EventKey FROM dual) s
    ON (t.EventKey = s.EventKey)
    WHEN MATCHED THEN
      UPDATE
         SET t.DisplayName = p_display_name,
             t.Description = p_description,
             t.IsEnabled   = NVL(p_is_enabled, 1)
    WHEN NOT MATCHED THEN
      INSERT
        (EventKey, DisplayName, IsEnabled, Description)
      VALUES
        (p_event_key, p_display_name, NVL(p_is_enabled, 1), p_description);
  END upsert_event;

  PROCEDURE upsert_template(p_event_key    IN VARCHAR2,
                            p_culture      IN VARCHAR2,
                            p_subject_html IN CLOB,
                            p_body_html    IN CLOB,
                            p_updated_by   IN VARCHAR2) IS
    v_event_id NUMBER;
  BEGIN
    v_event_id := get_event_id(p_event_key, 1);
  
    MERGE INTO EmailTemplates t
    USING (SELECT v_event_id AS EventId, p_culture AS Culture FROM dual) s
    ON (t.EventId = s.EventId AND t.Culture = s.Culture)
    WHEN MATCHED THEN
      UPDATE
         SET t.SubjectHtml   = p_subject_html,
             t.BodyHtml      = p_body_html,
             t.LastUpdatedBy = p_updated_by,
             t.LastUpdatedOn = SYSTIMESTAMP
    WHEN NOT MATCHED THEN
      INSERT
        (EventId, Culture, SubjectHtml, BodyHtml, LastUpdatedBy)
      VALUES
        (v_event_id, p_culture, p_subject_html, p_body_html, p_updated_by);
  END upsert_template;

  PROCEDURE upsert_rules(p_event_key    IN VARCHAR2,
                         p_sendto_roles IN VARCHAR2,
                         p_cc_roles     IN VARCHAR2,
                         p_bcc_roles    IN VARCHAR2,
                         p_sendto_users IN VARCHAR2,
                         p_cc_users     IN VARCHAR2,
                         p_bcc_users    IN VARCHAR2,
                         p_is_active    IN NUMBER) IS
    v_event_id NUMBER;
  BEGIN
    v_event_id := get_event_id(p_event_key, 1);
  
    MERGE INTO NotificationRules t
    USING (SELECT v_event_id AS EventId FROM dual) s
    ON (t.EventId = s.EventId)
    WHEN MATCHED THEN
      UPDATE
         SET t.SendToRolesCsv = p_sendto_roles,
             t.CcToRolesCsv   = p_cc_roles,
             t.BccToRolesCsv  = p_bcc_roles,
             t.SendToUsersCsv = p_sendto_users,
             t.CcToUsersCsv   = p_cc_users,
             t.BccToUsersCsv  = p_bcc_users,
             t.IsActive       = NVL(p_is_active, 1)
    WHEN NOT MATCHED THEN
      INSERT
        (EventId,
         SendToRolesCsv,
         CcToRolesCsv,
         BccToRolesCsv,
         SendToUsersCsv,
         CcToUsersCsv,
         BccToUsersCsv,
         IsActive)
      VALUES
        (v_event_id,
         p_sendto_roles,
         p_cc_roles,
         p_bcc_roles,
         p_sendto_users,
         p_cc_users,
         p_bcc_users,
         NVL(p_is_active, 1));
  END upsert_rules;

  PROCEDURE add_or_replace_placeholder(p_event_key    IN VARCHAR2,
                                       p_token        IN VARCHAR2,
                                       p_resolver_key IN VARCHAR2,
                                       p_sample_value IN CLOB) IS
    v_event_id NUMBER;
  BEGIN
    v_event_id := get_event_id(p_event_key, 1);
  
    MERGE INTO TemplatePlaceholders t
    USING (SELECT v_event_id AS EventId, p_token AS Token FROM dual) s
    ON (t.EventId = s.EventId AND t.Token = s.Token)
    WHEN MATCHED THEN
      UPDATE
         SET t.ResolverKey = p_resolver_key, t.SampleValue = p_sample_value
    WHEN NOT MATCHED THEN
      INSERT
        (EventId, Token, ResolverKey, SampleValue)
      VALUES
        (v_event_id, p_token, p_resolver_key, p_sample_value);
  END add_or_replace_placeholder;

  PROCEDURE add_or_replace_attachment_source(p_event_key      IN VARCHAR2,
                                             p_source_type    IN VARCHAR2,
                                             p_source_ref     IN VARCHAR2,
                                             p_file_name_pat  IN VARCHAR2,
                                             p_as_inline_html IN NUMBER) IS
    v_event_id NUMBER;
  BEGIN
    v_event_id := get_event_id(p_event_key, 1);
  
    MERGE INTO AttachmentSources t
    USING (SELECT v_event_id    AS EventId,
                  p_source_type AS SourceType,
                  p_source_ref  AS SourceRef
             FROM dual) s
    ON (t.EventId = s.EventId AND t.SourceType = s.SourceType AND t.SourceRef = s.SourceRef)
    WHEN MATCHED THEN
      UPDATE
         SET t.FileNamePattern = p_file_name_pat,
             t.AsInlineHtml    = NVL(p_as_inline_html, 0)
    WHEN NOT MATCHED THEN
      INSERT
        (EventId, SourceType, SourceRef, FileNamePattern, AsInlineHtml)
      VALUES
        (v_event_id,
         p_source_type,
         p_source_ref,
         p_file_name_pat,
         NVL(p_as_inline_html, 0));
  END add_or_replace_attachment_source;

  /* Autonomous logging so we capture failures even if caller rolls back */
  PROCEDURE log_email(p_event_key      IN VARCHAR2,
                      p_correlation_id IN VARCHAR2,
                      p_subject_sent   IN CLOB,
                      p_body_sent      IN CLOB,
                      p_to_csv         IN VARCHAR2,
                      p_cc_csv         IN VARCHAR2,
                      p_bcc_csv        IN VARCHAR2,
                      p_status         IN VARCHAR2,
                      p_error_message  IN CLOB) IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    INSERT INTO EmailLog
      (EventKey,
       CorrelationId,
       SubjectSent,
       BodySent,
       ToCsv,
       CcCsv,
       BccCsv,
       Status,
       ErrorMessage,
       SentOnUtc)
    VALUES
      (p_event_key,
       p_correlation_id,
       p_subject_sent,
       p_body_sent,
       p_to_csv,
       p_cc_csv,
       p_bcc_csv,
       p_status,
       p_error_message,
       SYS_EXTRACT_UTC(SYSTIMESTAMP));
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN
      -- Last resort: swallow logging errors (to not break caller)
      ROLLBACK;
  END log_email;

  /* ========== Readers ========== */

  PROCEDURE get_events(p_event_key IN VARCHAR2, o_cur OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN o_cur FOR
      SELECT EventId, EventKey, DisplayName, IsEnabled, Description
        FROM NotificationEvents
       WHERE (p_event_key IS NULL OR EventKey = p_event_key)
       ORDER BY DisplayName;
  END get_events;

  PROCEDURE get_templates_by_event(p_event_key IN VARCHAR2,
                                   o_cur       OUT SYS_REFCURSOR) IS
    v_event_id NUMBER;
  BEGIN
    v_event_id := get_event_id(p_event_key, 1);
    OPEN o_cur FOR
      SELECT TemplateId,
             EventId,
             Culture,
             SubjectHtml,
             BodyHtml,
             LastUpdatedBy,
             LastUpdatedOn
        FROM EmailTemplates
       WHERE EventId = v_event_id
       ORDER BY Culture;
  END get_templates_by_event;

  PROCEDURE get_rules_by_event(p_event_key IN VARCHAR2,
                               o_cur       OUT SYS_REFCURSOR) IS
    v_event_id NUMBER;
  BEGIN
    v_event_id := get_event_id(p_event_key, 1);
    OPEN o_cur FOR
      SELECT RuleId,
             EventId,
             SendToRolesCsv,
             CcToRolesCsv,
             BccToRolesCsv,
             SendToUsersCsv,
             CcToUsersCsv,
             BccToUsersCsv,
             IsActive
        FROM NotificationRules
       WHERE EventId = v_event_id;
  END get_rules_by_event;

  PROCEDURE get_placeholders_by_event(p_event_key IN VARCHAR2,
                                      o_cur       OUT SYS_REFCURSOR) IS
    v_event_id NUMBER;
  BEGIN
    v_event_id := get_event_id(p_event_key, 1);
    OPEN o_cur FOR
      SELECT PlaceholderId, EventId, Token, ResolverKey, SampleValue
        FROM TemplatePlaceholders
       WHERE EventId = v_event_id
       ORDER BY Token;
  END get_placeholders_by_event;

  PROCEDURE get_attachment_sources_by_event(p_event_key IN VARCHAR2,
                                            o_cur       OUT SYS_REFCURSOR) IS
    v_event_id NUMBER;
  BEGIN
    v_event_id := get_event_id(p_event_key, 1);
    OPEN o_cur FOR
      SELECT AttachmentId,
             EventId,
             SourceType,
             SourceRef,
             FileNamePattern,
             AsInlineHtml
        FROM AttachmentSources
       WHERE EventId = v_event_id
       ORDER BY AttachmentId;
  END get_attachment_sources_by_event;

  PROCEDURE get_email_log(p_event_key      IN VARCHAR2,
                          p_from_utc       IN TIMESTAMP WITH TIME ZONE,
                          p_to_utc         IN TIMESTAMP WITH TIME ZONE,
                          p_status         IN VARCHAR2,
                          p_email_contains IN VARCHAR2,
                          o_cur            OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN o_cur FOR
      SELECT LogId,
             EventKey,
             CorrelationId,
             SubjectSent,
             BodySent,
             ToCsv,
             CcCsv,
             BccCsv,
             Status,
             ErrorMessage,
             SentOnUtc
        FROM EmailLog
       WHERE (p_event_key IS NULL OR EventKey = p_event_key)
         AND (p_from_utc IS NULL OR SentOnUtc >= p_from_utc)
         AND (p_to_utc IS NULL OR SentOnUtc < p_to_utc)
         AND (p_status IS NULL OR Status = p_status)
         AND (p_email_contains IS NULL OR
             INSTR(ToCsv, p_email_contains) > 0 OR
             INSTR(CcCsv, p_email_contains) > 0 OR
             INSTR(BccCsv, p_email_contains) > 0)
       ORDER BY SentOnUtc DESC, LogId DESC;
  END get_email_log;

  /* ========== Seed (via upserts) ========== */

  PROCEDURE seed_defaults IS
  BEGIN
    -- Events
    upsert_event('AUDIT_CONCLUDED',
                 'Audit Concluded',
                 'Fire when audit is marked concluded',
                 1);
    upsert_event('MEMO_SUBMITTED',
                 'Memo Submitted',
                 'When a memo is submitted',
                 1);
    upsert_event('MEMO_REPLIED',
                 'Memo Replied',
                 'When a reply is posted',
                 1);
    upsert_event('REPORT_ISSUED',
                 'Report Issued',
                 'When report is issued',
                 1);
    upsert_event('REMINDER_OUTSTANDING_PARAS',
                 'Outstanding Paras Reminder',
                 '15-day reminder',
                 1);
  
    -- Templates
    upsert_template(p_event_key    => 'REPORT_ISSUED',
                    p_culture      => 'en',
                    p_subject_html => 'Report issued for {Entity.Name} – {Audit.Period}',
                    p_body_html    => '<p>Dear {ReportingOffice.Name},</p><p>The audit report for <strong>{Entity.Name}</strong> ({Entity.Code}) has been issued.</p>{Report.TableHtml}<p>Regards,<br/>Internal Audit</p>',
                    p_updated_by   => 'seed');
  
    upsert_template(p_event_key    => 'AUDIT_CONCLUDED',
                    p_culture      => 'en',
                    p_subject_html => 'Audit concluded for {Entity.Name}',
                    p_body_html    => '<p>Audit for <strong>{Entity.Name}</strong> has been concluded.</p><p>Audit Period: {Audit.Period}</p>',
                    p_updated_by   => 'seed');
  
    -- Rules (examples)
    upsert_rules(p_event_key    => 'REPORT_ISSUED',
                 p_sendto_roles => 'Reporting_Office',
                 p_cc_roles     => 'Auditor,Head_AZ',
                 p_bcc_roles    => NULL,
                 p_sendto_users => NULL,
                 p_cc_users     => NULL,
                 p_bcc_users    => NULL,
                 p_is_active    => 1);
  
    upsert_rules(p_event_key    => 'AUDIT_CONCLUDED',
                 p_sendto_roles => 'Auditee',
                 p_cc_roles     => 'Auditor,Reporting_Office',
                 p_bcc_roles    => NULL,
                 p_sendto_users => NULL,
                 p_cc_users     => NULL,
                 p_bcc_users    => NULL,
                 p_is_active    => 1);
  
    -- Placeholders
    add_or_replace_placeholder(p_event_key    => 'REPORT_ISSUED',
                               p_token        => '{Report.TableHtml}',
                               p_resolver_key => 'ReportTableResolver',
                               p_sample_value => '<table><tr><th>No</th><th>Title</th></tr><tr><td>1</td><td>Sample</td></tr></table>');
  
    add_or_replace_placeholder(p_event_key    => 'REPORT_ISSUED',
                               p_token        => '{Entity.Name}',
                               p_resolver_key => 'EntityResolver',
                               p_sample_value => 'KANDH KOT');
  
    add_or_replace_placeholder(p_event_key    => 'MEMO_SUBMITTED',
                               p_token        => '{Observation.Number}',
                               p_resolver_key => 'ObservationResolver',
                               p_sample_value => '20605');
  
    -- Inline table source (example)
    add_or_replace_attachment_source(p_event_key      => 'REPORT_ISSUED',
                                     p_source_type    => 'VIEW',
                                     p_source_ref     => 'VW_AUDIT_PARAS_TABLE',
                                     p_file_name_pat  => NULL,
                                     p_as_inline_html => 1);
  END seed_defaults;

END pkg_email;
