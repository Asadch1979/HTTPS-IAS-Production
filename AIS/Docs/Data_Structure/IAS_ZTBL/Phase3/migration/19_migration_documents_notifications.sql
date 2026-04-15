/*
  IAS_ZTBL Phase 3 documents and notifications migration

  Scope
  - notification events, templates, rules, queue entries
  - filesystem/document migration queue entries
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Notification events
-------------------------------------------------------------------------------

DECLARE
    l_batch_id NUMBER;
    l_new_id   NUMBER;
BEGIN
    SELECT migration_batch_id
      INTO l_batch_id
      FROM tbl_migration_batch
     WHERE batch_code = 'PHASE3_BASELINE_01';

    FOR rec IN (
        SELECT
            TO_CHAR(e.EVENTID) AS source_id,
            SUBSTR(e.EVENTKEY, 1, 50) AS event_code,
            SUBSTR(e.DISPLAYNAME, 1, 150) AS event_name,
            e.DESCRIPTION AS description,
            CASE WHEN NVL(e.ISENABLED, 1) = 1 THEN 'Y' ELSE 'N' END AS is_active
        FROM ZTBLAIS_PROD.NOTIFICATIONEVENTS e
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'NOTIFICATIONEVENTS'
               AND m.source_pk_value = TO_CHAR(e.EVENTID)
               AND m.target_table_name = 'TBL_NOTIFICATION_EVENT'
        )
    ) LOOP
        l_new_id := seq_notification_event.NEXTVAL;

        INSERT INTO tbl_notification_event (
            notification_event_id, event_code, event_name, module_code, description, is_active, created_by
        )
        VALUES (
            l_new_id, rec.event_code, rec.event_name, 'NOTIFY', rec.description, rec.is_active, 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'NOTIFICATIONEVENTS',
            'EVENTID', rec.source_id, 'TBL_NOTIFICATION_EVENT', 'NOTIFICATION_EVENT_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Notification event migrated.', 0
        );
    END LOOP;
END;
/

INSERT INTO tbl_notification_event (
    notification_event_id, event_code, event_name, module_code, description, is_active, created_by
)
SELECT
    seq_notification_event.NEXTVAL,
    'LEGACY_EMAIL_QUEUE',
    'Legacy Email Queue',
    'NOTIFY',
    'Fallback notification event for queue rows without an event code.',
    'Y',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_notification_event e
     WHERE e.event_code = 'LEGACY_EMAIL_QUEUE'
);

INSERT INTO tbl_notification_event (
    notification_event_id, event_code, event_name, module_code, description, is_active, created_by
)
SELECT
    seq_notification_event.NEXTVAL,
    SUBSTR(q.event_code, 1, 50),
    SUBSTR('Legacy Queue Event - ' || q.event_code, 1, 150),
    'NOTIFY',
    'Derived from active legacy email queue event codes not present in NOTIFICATIONEVENTS.',
    'Y',
    0
FROM (
    SELECT DISTINCT SUBSTR(TRIM(EVENT_CODE), 1, 50) AS event_code
      FROM ZTBLAIS_PROD.T_AU_EMAIL_QUEUE
     WHERE EVENT_CODE IS NOT NULL
) q
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_notification_event e
     WHERE e.event_code = q.event_code
);

-------------------------------------------------------------------------------
-- Notification templates and rules
-------------------------------------------------------------------------------

INSERT INTO tbl_notification_template (
    notification_template_id, notification_event_id, culture_code,
    subject_template, body_template, is_active, created_by, modified_by, modified_on
)
SELECT
    NULL,
    evt_map.target_pk_value,
    t.CULTURE,
    t.SUBJECTHTML,
    t.BODYHTML,
    'Y',
    0,
    0,
    CAST(t.LASTUPDATEDON AS DATE)
FROM ZTBLAIS_PROD.EMAILTEMPLATES t
JOIN tbl_legacy_key_map evt_map
  ON evt_map.source_table_name = 'NOTIFICATIONEVENTS'
 AND evt_map.source_pk_value = TO_CHAR(t.EVENTID)
 AND evt_map.target_table_name = 'TBL_NOTIFICATION_EVENT'
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_notification_template x
     WHERE x.notification_event_id = evt_map.target_pk_value
       AND x.culture_code = t.CULTURE
);

INSERT INTO tbl_notification_rule (
    notification_rule_id, notification_event_id, recipient_type_id,
    recipient_expression, cc_expression, is_active, created_by
)
SELECT
    NULL,
    evt_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'RECIPIENT_TYPE' AND lv.lookup_code = CASE WHEN NVL(r.SENDTOROLESCSV, '') <> '' AND NVL(r.SENDTOUSERSCSV, '') <> '' THEN 'MIXED' WHEN NVL(r.SENDTOROLESCSV, '') <> '' THEN 'ROLE' ELSE 'USER' END),
    SUBSTR('ROLES=' || NVL(r.SENDTOROLESCSV, '') || ';USERS=' || NVL(r.SENDTOUSERSCSV, ''), 1, 500),
    SUBSTR('CC_ROLES=' || NVL(r.CCTOROLESCSV, '') || ';CC_USERS=' || NVL(r.CCTOUSERSCSV, '') || ';BCC_ROLES=' || NVL(r.BCCTOROLESCSV, '') || ';BCC_USERS=' || NVL(r.BCCTOUSERSCSV, ''), 1, 500),
    CASE WHEN NVL(r.ISACTIVE, 1) = 1 THEN 'Y' ELSE 'N' END,
    0
FROM ZTBLAIS_PROD.NOTIFICATIONRULES r
JOIN tbl_legacy_key_map evt_map
  ON evt_map.source_table_name = 'NOTIFICATIONEVENTS'
 AND evt_map.source_pk_value = TO_CHAR(r.EVENTID)
 AND evt_map.target_table_name = 'TBL_NOTIFICATION_EVENT'
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_notification_rule x
     WHERE x.notification_event_id = evt_map.target_pk_value
       AND x.recipient_type_id = (
            SELECT lv.lookup_value_id
              FROM tbl_lookup_value lv
              JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
             WHERE lt.lookup_type_code = 'RECIPIENT_TYPE'
               AND lv.lookup_code = CASE
                                      WHEN NVL(r.SENDTOROLESCSV, '') <> '' AND NVL(r.SENDTOUSERSCSV, '') <> '' THEN 'MIXED'
                                      WHEN NVL(r.SENDTOROLESCSV, '') <> '' THEN 'ROLE'
                                      ELSE 'USER'
                                    END
       )
       AND x.recipient_expression = SUBSTR('ROLES=' || NVL(r.SENDTOROLESCSV, '') || ';USERS=' || NVL(r.SENDTOUSERSCSV, ''), 1, 500)
);

-------------------------------------------------------------------------------
-- Notification queue
-------------------------------------------------------------------------------

DECLARE
    l_batch_id NUMBER;
    l_new_id   NUMBER;
BEGIN
    SELECT migration_batch_id
      INTO l_batch_id
      FROM tbl_migration_batch
     WHERE batch_code = 'PHASE3_BASELINE_01';

    FOR rec IN (
        SELECT
            TO_CHAR(q.EMAIL_ID) AS source_id,
            evt.notification_event_id AS notification_event_id,
            q.REF_ID1 AS source_entity_id,
            NVL(q.MAIL_TO, 'unknown@legacy.local') AS recipient_to,
            q.MAIL_CC AS recipient_cc,
            SUBSTR(NVL(q.SUBJECT, 'Legacy Queue Email ' || TO_CHAR(q.EMAIL_ID)), 1, 500) AS subject_text,
            NVL(q.BODY, 'Legacy queue email body was NULL in source.') AS body_text,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'NOTIFICATION_QUEUE_STATUS'
                   AND lv.lookup_code = CASE UPPER(NVL(q.STATUS, 'QUEUED'))
                                          WHEN 'SENT' THEN 'SENT'
                                          WHEN 'FAILED' THEN 'FAILED'
                                          ELSE 'QUEUED'
                                        END
            ) AS queue_status_id,
            NVL(q.CREATED_ON, SYSDATE) AS queued_on,
            q.SENT_ON AS processed_on,
            q.ERROR_TEXT AS error_text
        FROM ZTBLAIS_PROD.T_AU_EMAIL_QUEUE q
        JOIN tbl_notification_event evt
          ON evt.event_code = SUBSTR(NVL(TRIM(q.EVENT_CODE), 'LEGACY_EMAIL_QUEUE'), 1, 50)
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_EMAIL_QUEUE'
               AND m.source_pk_value = TO_CHAR(q.EMAIL_ID)
               AND m.target_table_name = 'TBL_NOTIFICATION_QUEUE'
        )
    ) LOOP
        l_new_id := seq_notification_queue.NEXTVAL;

        INSERT INTO tbl_notification_queue (
            notification_queue_id, notification_event_id, source_entity_name, source_entity_id,
            recipient_to, recipient_cc, subject_text, body_text, queue_status_id, queued_on,
            processed_on, error_text, is_active, created_by
        )
        VALUES (
            l_new_id, rec.notification_event_id, 'LEGACY_EMAIL_QUEUE', rec.source_entity_id,
            rec.recipient_to, rec.recipient_cc, rec.subject_text, rec.body_text, rec.queue_status_id, rec.queued_on,
            rec.processed_on, rec.error_text, 'Y', 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_EMAIL_QUEUE',
            'EMAIL_ID', rec.source_id, 'TBL_NOTIFICATION_QUEUE', 'NOTIFICATION_QUEUE_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Legacy email queue row migrated.', 0
        );
    END LOOP;
END;
/

-------------------------------------------------------------------------------
-- Legacy document queue
-------------------------------------------------------------------------------

INSERT INTO tbl_document_migration_queue (
    document_migration_queue_id, migration_batch_id, source_table_name, source_pk_value,
    source_column_name, legacy_file_path, legacy_file_name, target_table_name, target_pk_value,
    attachment_link_type_code, queue_status_code, remarks, is_active, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_IID_REPORT',
    TO_CHAR(r.REPORT_ID),
    'UPLOADED_REPORT',
    r.UPLOADED_REPORT,
    r.UPLOADED_REPORT,
    'TBL_IID_CASE',
    case_map.target_pk_value,
    'IID',
    'QUEUED',
    'Legacy IID uploaded report path queued for document migration at IID case level.',
    'Y',
    NVL(r.SUBMITTED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_REPORT r
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(r.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
WHERE r.UPLOADED_REPORT IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
        FROM tbl_document_migration_queue q
       WHERE q.source_table_name = 'T_AU_IID_REPORT'
         AND q.source_pk_value = TO_CHAR(r.REPORT_ID)
         AND q.source_column_name = 'UPLOADED_REPORT'
  );

INSERT INTO tbl_document_migration_queue (
    document_migration_queue_id, migration_batch_id, source_table_name, source_pk_value,
    source_column_name, legacy_file_path, legacy_file_name, target_table_name, target_pk_value,
    attachment_link_type_code, queue_status_code, remarks, is_active, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_IID_REPORT',
    TO_CHAR(r.REPORT_ID),
    'UPLOADED_EVIDENCE',
    r.UPLOADED_EVIDENCE,
    r.UPLOADED_EVIDENCE,
    'TBL_IID_CASE',
    case_map.target_pk_value,
    'IID',
    'QUEUED',
    'Legacy IID evidence path queued for document migration at IID case level.',
    'Y',
    NVL(r.SUBMITTED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_REPORT r
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(r.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
WHERE r.UPLOADED_EVIDENCE IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
        FROM tbl_document_migration_queue q
       WHERE q.source_table_name = 'T_AU_IID_REPORT'
         AND q.source_pk_value = TO_CHAR(r.REPORT_ID)
         AND q.source_column_name = 'UPLOADED_EVIDENCE'
  );

INSERT INTO tbl_document_migration_queue (
    document_migration_queue_id, migration_batch_id, source_table_name, source_pk_value,
    source_column_name, legacy_file_path, legacy_file_name, target_table_name, target_pk_value,
    attachment_link_type_code, queue_status_code, remarks, is_active, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_IID_REPORT',
    TO_CHAR(r.REPORT_ID),
    'UPLOADED_DSA',
    r.UPLOADED_DSA,
    r.UPLOADED_DSA,
    'TBL_IID_CASE',
    case_map.target_pk_value,
    'IID',
    'QUEUED',
    'Legacy IID DSA path queued for document migration at IID case level.',
    'Y',
    NVL(r.SUBMITTED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_REPORT r
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(r.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
WHERE r.UPLOADED_DSA IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
        FROM tbl_document_migration_queue q
       WHERE q.source_table_name = 'T_AU_IID_REPORT'
         AND q.source_pk_value = TO_CHAR(r.REPORT_ID)
         AND q.source_column_name = 'UPLOADED_DSA'
  );

INSERT INTO tbl_document_migration_queue (
    document_migration_queue_id, migration_batch_id, source_table_name, source_pk_value,
    source_column_name, legacy_file_name, target_table_name,
    attachment_link_type_code, queue_status_code, remarks, is_active, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'ATTACHMENTSOURCES',
    TO_CHAR(a.ATTACHMENTID),
    a.SOURCETYPE,
    a.FILENAMEPATTERN,
    'TBL_ATTACHMENT',
    'REPORT',
    'QUEUED',
    'Legacy attachment source pattern captured for later filesystem resolution: ' || a.SOURCEREF,
    'Y',
    0
FROM ZTBLAIS_PROD.ATTACHMENTSOURCES a
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_document_migration_queue q
     WHERE q.source_table_name = 'ATTACHMENTSOURCES'
       AND q.source_pk_value = TO_CHAR(a.ATTACHMENTID)
       AND q.source_column_name = a.SOURCETYPE
);

INSERT INTO tbl_document_migration_queue (
    document_migration_queue_id, migration_batch_id, source_table_name, source_pk_value,
    source_column_name, legacy_file_name, target_table_name,
    attachment_link_type_code, queue_status_code, remarks, is_active, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_IID_EXC_ACCOUNT_DOC',
    TO_CHAR(d.ACCOUNT_DOC_ID),
    'DOC_IMAGE',
    d.DOC_NAME,
    'TBL_ATTACHMENT',
    'IID',
    'QUEUED',
    'Legacy IID account exception document stored as BLOB; export and attachment-link resolution required for account ' || d.ACCOUNT_NO,
    'Y',
    NVL(d.CREATED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_EXC_ACCOUNT_DOC d
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_document_migration_queue q
     WHERE q.source_table_name = 'T_AU_IID_EXC_ACCOUNT_DOC'
       AND q.source_pk_value = TO_CHAR(d.ACCOUNT_DOC_ID)
       AND q.source_column_name = 'DOC_IMAGE'
);

INSERT INTO tbl_document_migration_queue (
    document_migration_queue_id, migration_batch_id, source_table_name, source_pk_value,
    source_column_name, legacy_file_name, target_table_name,
    attachment_link_type_code, queue_status_code, remarks, is_active, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_IID_EXC_LOAN_DOC',
    TO_CHAR(d.LOAN_DOC_ID),
    'IMAGE_ID',
    d.DOC_NAME,
    'TBL_ATTACHMENT',
    'IID',
    'QUEUED',
    'Legacy IID loan exception document references BLOB image export and attachment-link resolution for loan ' || d.LOAN_DISB_ID,
    'Y',
    NVL(d.CREATED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_EXC_LOAN_DOC d
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_document_migration_queue q
     WHERE q.source_table_name = 'T_AU_IID_EXC_LOAN_DOC'
       AND q.source_pk_value = TO_CHAR(d.LOAN_DOC_ID)
       AND q.source_column_name = 'IMAGE_ID'
);

INSERT INTO tbl_document_migration_queue (
    document_migration_queue_id, migration_batch_id, source_table_name, source_pk_value,
    source_column_name, legacy_file_name, target_table_name,
    attachment_link_type_code, queue_status_code, remarks, is_active, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_IID_EXC_LOAN_DOC_IMG',
    TO_CHAR(i.IMAGE_ID),
    'IMAGE_DATA',
    i.FILE_NAME,
    'TBL_ATTACHMENT',
    'IID',
    'QUEUED',
    'Legacy IID loan document image stored as BLOB and requires binary export.',
    'Y',
    NVL(i.CREATED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_EXC_LOAN_DOC_IMG i
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_document_migration_queue q
     WHERE q.source_table_name = 'T_AU_IID_EXC_LOAN_DOC_IMG'
       AND q.source_pk_value = TO_CHAR(i.IMAGE_ID)
       AND q.source_column_name = 'IMAGE_DATA'
);

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'FILESYSTEM',
    'N/A',
    'N/A',
    'wwwroot/Audit_Report,wwwroot/PostCompliance_Evidences,wwwroot/Auditee_Evidences,wwwroot/CAU_Evidences',
    'TBL_DOCUMENT_MIGRATION_QUEUE',
    'MANUAL_REVIEW',
    'Application-side filesystem evidence/report folders must be inventoried separately from database rows before cutover.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'FILESYSTEM'
);

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_IID_EXC_ACCOUNT_DOC/T_AU_IID_EXC_LOAN_DOC/T_AU_IID_EXC_LOAN_DOC_IMG',
    'N/A',
    'N/A',
    'DOC_IMAGE/IMAGE_DATA',
    'TBL_ATTACHMENT',
    'MANUAL_REVIEW',
    'IID exception document blobs require external export tooling and attachment-link reconciliation before cutover.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'T_AU_IID_EXC_ACCOUNT_DOC/T_AU_IID_EXC_LOAN_DOC/T_AU_IID_EXC_LOAN_DOC_IMG'
);
