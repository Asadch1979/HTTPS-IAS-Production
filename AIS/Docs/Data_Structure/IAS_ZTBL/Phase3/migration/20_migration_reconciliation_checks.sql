/*
  IAS_ZTBL Phase 3 migration reconciliation checks

  Purpose
  - verify migration batch registration and status
  - compare legacy row populations against mapped IAS_ZTBL targets
  - summarize unresolved migration issues and document queue load
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

PROMPT === Registered Migration Batch ===
SELECT migration_batch_id,
       batch_code,
       batch_name,
       source_schema_name,
       batch_status_code,
       started_on,
       completed_on,
       remarks
  FROM tbl_migration_batch
 WHERE batch_code = 'PHASE3_BASELINE_01';

PROMPT === Legacy To Target Count Comparison ===
WITH comparisons AS (
    SELECT 'ENTITY_TYPE' AS domain_name,
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AUDITEE_ENT_TYPES) AS legacy_count,
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AUDITEE_ENT_TYPES' AND target_table_name = 'TBL_ENTITY_TYPE') AS target_count
      FROM dual
    UNION ALL
    SELECT 'ENTITY',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AUDITEE_ENTITIES),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AUDITEE_ENTITIES' AND target_table_name = 'TBL_ENTITY')
      FROM dual
    UNION ALL
    SELECT 'AUDIT_PERIOD',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_PERIOD),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AU_PERIOD' AND target_table_name = 'TBL_AUDIT_PERIOD')
      FROM dual
    UNION ALL
    SELECT 'REFERENCE_DOCUMENT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_MANUAL_MASTER),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_MANUAL_MASTER' AND target_table_name = 'TBL_REFERENCE_DOCUMENT')
      FROM dual
    UNION ALL
    SELECT 'REFERENCE_DOCUMENT_VERSION',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_MANUAL_INDEX),
           (SELECT COUNT(*) FROM tbl_reference_document_version)
      FROM dual
    UNION ALL
    SELECT 'ROLE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_GROUPS),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_GROUPS' AND target_table_name = 'TBL_ROLE')
      FROM dual
    UNION ALL
    SELECT 'USER',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_USER),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_USER' AND target_table_name = 'TBL_USER')
      FROM dual
    UNION ALL
    SELECT 'APPLICATION_PAGE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_MENU_PAGES),
           (SELECT COUNT(*) FROM tbl_application_page)
      FROM dual
    UNION ALL
    SELECT 'API_ENDPOINT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_API_MASTER),
           (SELECT COUNT(*) FROM tbl_api_endpoint)
      FROM dual
    UNION ALL
    SELECT 'AUDIT_PLAN',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_PLAN),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AU_PLAN' AND target_table_name = 'TBL_AUDIT_PLAN')
      FROM dual
    UNION ALL
    SELECT 'ENGAGEMENT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_PLAN_ENG),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AU_PLAN_ENG' AND target_table_name = 'TBL_ENGAGEMENT')
      FROM dual
    UNION ALL
    SELECT 'OBSERVATION',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.AIS_T_AU_OBSERVATION),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'AIS_T_AU_OBSERVATION' AND target_table_name = 'TBL_OBSERVATION')
      FROM dual
    UNION ALL
    SELECT 'OBSERVATION_DETAIL',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_OBSERVATION_TEXT),
           (SELECT COUNT(*) FROM tbl_observation_detail)
      FROM dual
    UNION ALL
    SELECT 'OBSERVATION_ASSIGNMENT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_OBSERVATION_RESPONIBILITY_ASSIGNED),
           (SELECT COUNT(*) FROM tbl_observation_assignment)
      FROM dual
    UNION ALL
    SELECT 'OBSERVATION_RESPONSE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_OBSERVATIONS_AUDITEE_RESPONSE) + (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION),
           (SELECT COUNT(*) FROM tbl_observation_response)
      FROM dual
    UNION ALL
    SELECT 'COMPLIANCE_CASE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.AIS_T_AU_POST_COMPLIANCE),
           (SELECT COUNT(*) FROM tbl_compliance_case)
      FROM dual
    UNION ALL
    SELECT 'COMPLIANCE_CASE_HISTORY',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.AIS_T_AU_POST_COMPLIANCE_HISTORY),
           (SELECT COUNT(*) FROM tbl_compliance_case_history)
      FROM dual
    UNION ALL
    SELECT 'PARA_CASE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_OBSERVATION_OLD_CAD_PARAS),
           (SELECT COUNT(*) FROM tbl_para_case)
      FROM dual
    UNION ALL
    SELECT 'PARA_STATUS_HISTORY',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_OLD_PARAS_POST_COMPLIANCE_HISTORY),
           (SELECT COUNT(*) FROM tbl_para_status_history)
      FROM dual
    UNION ALL
    SELECT 'IID_CASE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_IID_COMPLAINT_HDR),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AU_IID_COMPLAINT_HDR' AND target_table_name = 'TBL_IID_CASE')
      FROM dual
    UNION ALL
    SELECT 'IID_COMPLAINANT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_IID_COMPLAINANT),
           (SELECT COUNT(*) FROM tbl_iid_complainant)
      FROM dual
    UNION ALL
    SELECT 'IID_SUBJECT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_IID_INQ_ACCUSED_LIST),
           (SELECT COUNT(*) FROM tbl_iid_subject)
      FROM dual
    UNION ALL
    SELECT 'IID_STATEMENT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_IID_INQ_STATEMENTS),
           (SELECT COUNT(*) FROM tbl_iid_statement)
      FROM dual
    UNION ALL
    SELECT 'IID_EXCEPTION_ACCOUNT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_IID_EXC_ACCOUNT),
           (SELECT COUNT(*) FROM tbl_iid_exception_item WHERE account_no IS NOT NULL)
      FROM dual
    UNION ALL
    SELECT 'IID_EXCEPTION_LOAN',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_IID_EXC_LOAN),
           (SELECT COUNT(*) FROM tbl_iid_exception_item WHERE loan_no IS NOT NULL)
      FROM dual
    UNION ALL
    SELECT 'COMMERCIAL_OM',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_COM_AUDIT_OM),
           (SELECT COUNT(*) FROM tbl_commercial_om)
      FROM dual
    UNION ALL
    SELECT 'COMMERCIAL_PDP',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_COM_AUDIT_PDP),
           (SELECT COUNT(*) FROM tbl_commercial_pdp)
      FROM dual
    UNION ALL
    SELECT 'COMMERCIAL_ARPSE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_COM_AUDIT_ARPSE),
           (SELECT COUNT(*) FROM tbl_commercial_arpse)
      FROM dual
    UNION ALL
    SELECT 'REPORT_ROOT',
           (
             SELECT COUNT(*)
               FROM (
                     SELECT ENG_ID FROM ZTBLAIS_PROD.T_FRPT_TEXT_BLOCKS
                     UNION
                     SELECT ENG_ID FROM ZTBLAIS_PROD.T_FRPT_PARA_NARRATIVE
                     UNION
                     SELECT ENG_ID FROM ZTBLAIS_PROD.T_FRPT_OVERALL_CONCLUSION
                     UNION
                     SELECT ENG_ID FROM ZTBLAIS_PROD.T_FRPT_PDF_STATISTICS
                    )
           ),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'FRPT_ENG' AND target_table_name = 'TBL_REPORT')
      FROM dual
    UNION ALL
    SELECT 'REPORT_SECTION',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_FRPT_TEXT_BLOCKS) + (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_FRPT_PARA_NARRATIVE) + (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_FRPT_OVERALL_CONCLUSION) + (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_FRPT_INCOME_LEAKAGE),
           (SELECT COUNT(*) FROM tbl_report_section)
      FROM dual
    UNION ALL
    SELECT 'REPORT_SNAPSHOT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_FRPT_KPI_SNAPSHOT) + (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_FRPT_NPL_SNAPSHOT) + (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_FRPT_STAFF_SNAPSHOT) + (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_FRPT_PDF_STATISTICS),
           (SELECT COUNT(*) FROM tbl_report_snapshot)
      FROM dual
    UNION ALL
    SELECT 'NOTIFICATION_EVENT',
           (
             (SELECT COUNT(*) FROM ZTBLAIS_PROD.NOTIFICATIONEVENTS)
             + (SELECT COUNT(*)
                  FROM (
                        SELECT DISTINCT SUBSTR(TRIM(q.EVENT_CODE), 1, 50) AS event_code
                          FROM ZTBLAIS_PROD.T_AU_EMAIL_QUEUE q
                         WHERE q.EVENT_CODE IS NOT NULL
                           AND NOT EXISTS (
                               SELECT 1
                                 FROM ZTBLAIS_PROD.NOTIFICATIONEVENTS e
                                WHERE SUBSTR(e.EVENTKEY, 1, 50) = SUBSTR(TRIM(q.EVENT_CODE), 1, 50)
                           )
                       )
               )
             + (SELECT CASE WHEN EXISTS (SELECT 1 FROM ZTBLAIS_PROD.T_AU_EMAIL_QUEUE q WHERE q.EVENT_CODE IS NULL) THEN 1 ELSE 0 END FROM dual)
           ),
           (SELECT COUNT(*) FROM tbl_notification_event)
      FROM dual
    UNION ALL
    SELECT 'NOTIFICATION_TEMPLATE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.EMAILTEMPLATES),
           (SELECT COUNT(*) FROM tbl_notification_template)
      FROM dual
    UNION ALL
    SELECT 'NOTIFICATION_RULE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.NOTIFICATIONRULES),
           (SELECT COUNT(*) FROM tbl_notification_rule)
      FROM dual
    UNION ALL
    SELECT 'NOTIFICATION_QUEUE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_EMAIL_QUEUE),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AU_EMAIL_QUEUE' AND target_table_name = 'TBL_NOTIFICATION_QUEUE')
      FROM dual
)
SELECT domain_name,
       legacy_count,
       target_count,
       target_count - legacy_count AS delta_count
  FROM comparisons
 ORDER BY domain_name;

PROMPT === Count Mismatches Only ===
WITH comparisons AS (
    SELECT 'ENTITY_TYPE' AS domain_name,
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AUDITEE_ENT_TYPES) AS legacy_count,
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AUDITEE_ENT_TYPES' AND target_table_name = 'TBL_ENTITY_TYPE') AS target_count
      FROM dual
    UNION ALL
    SELECT 'ENTITY',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AUDITEE_ENTITIES),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AUDITEE_ENTITIES' AND target_table_name = 'TBL_ENTITY')
      FROM dual
    UNION ALL
    SELECT 'AUDIT_PLAN',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_PLAN),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AU_PLAN' AND target_table_name = 'TBL_AUDIT_PLAN')
      FROM dual
    UNION ALL
    SELECT 'ENGAGEMENT',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_PLAN_ENG),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AU_PLAN_ENG' AND target_table_name = 'TBL_ENGAGEMENT')
      FROM dual
    UNION ALL
    SELECT 'OBSERVATION',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.AIS_T_AU_OBSERVATION),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'AIS_T_AU_OBSERVATION' AND target_table_name = 'TBL_OBSERVATION')
      FROM dual
    UNION ALL
    SELECT 'COMPLIANCE_CASE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.AIS_T_AU_POST_COMPLIANCE),
           (SELECT COUNT(*) FROM tbl_compliance_case)
      FROM dual
    UNION ALL
    SELECT 'IID_CASE',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_AU_IID_COMPLAINT_HDR),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'T_AU_IID_COMPLAINT_HDR' AND target_table_name = 'TBL_IID_CASE')
      FROM dual
    UNION ALL
    SELECT 'COMMERCIAL_OM',
           (SELECT COUNT(*) FROM ZTBLAIS_PROD.T_COM_AUDIT_OM),
           (SELECT COUNT(*) FROM tbl_commercial_om)
      FROM dual
    UNION ALL
    SELECT 'REPORT_ROOT',
           (
             SELECT COUNT(*)
               FROM (
                     SELECT ENG_ID FROM ZTBLAIS_PROD.T_FRPT_TEXT_BLOCKS
                     UNION
                     SELECT ENG_ID FROM ZTBLAIS_PROD.T_FRPT_PARA_NARRATIVE
                     UNION
                     SELECT ENG_ID FROM ZTBLAIS_PROD.T_FRPT_OVERALL_CONCLUSION
                     UNION
                     SELECT ENG_ID FROM ZTBLAIS_PROD.T_FRPT_PDF_STATISTICS
                    )
           ),
           (SELECT COUNT(*) FROM tbl_legacy_key_map WHERE source_table_name = 'FRPT_ENG' AND target_table_name = 'TBL_REPORT')
      FROM dual
)
SELECT domain_name,
       legacy_count,
       target_count,
       target_count - legacy_count AS delta_count
  FROM comparisons
 WHERE legacy_count <> target_count
 ORDER BY domain_name;

PROMPT === Legacy Mapping Summary ===
SELECT source_table_name,
       target_table_name,
       COUNT(*) AS mapping_count
  FROM tbl_legacy_key_map
 WHERE migration_batch_id = (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01')
 GROUP BY source_table_name, target_table_name
 ORDER BY source_table_name, target_table_name;

PROMPT === Migration Issues By Type ===
SELECT issue_type_code,
       COUNT(*) AS issue_count
  FROM tbl_migration_issue
 WHERE migration_batch_id = (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01')
 GROUP BY issue_type_code
 ORDER BY issue_type_code;

PROMPT === Migration Issues By Source Table ===
SELECT source_table_name,
       COUNT(*) AS issue_count
  FROM tbl_migration_issue
 WHERE migration_batch_id = (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01')
 GROUP BY source_table_name
 ORDER BY source_table_name;

PROMPT === Document Migration Queue Summary ===
SELECT queue_status_code,
       COUNT(*) AS queue_count
  FROM tbl_document_migration_queue
 GROUP BY queue_status_code
 ORDER BY queue_status_code;

PROMPT === Document Queue By Source Table ===
SELECT source_table_name,
       COUNT(*) AS queue_count
  FROM tbl_document_migration_queue
 GROUP BY source_table_name
 ORDER BY source_table_name;

PROMPT === Document Queue Rows Missing Target Metadata ===
SELECT source_table_name,
       source_pk_value,
       source_column_name,
       target_table_name,
       target_pk_value,
       remarks
  FROM tbl_document_migration_queue
 WHERE target_table_name IS NULL
    OR (target_table_name <> 'TBL_ATTACHMENT' AND target_pk_value IS NULL)
 ORDER BY source_table_name, source_pk_value, source_column_name;

PROMPT === Notification Queue Status Summary ===
SELECT lv.lookup_code AS queue_status_code,
       COUNT(*) AS queue_count
  FROM tbl_notification_queue nq
  JOIN tbl_lookup_value lv
    ON lv.lookup_value_id = nq.queue_status_id
 GROUP BY lv.lookup_code
 ORDER BY lv.lookup_code;

PROMPT === Current / History Table Presence ===
SELECT 'TBL_OBSERVATION' AS table_name, COUNT(*) AS row_count FROM tbl_observation
UNION ALL
SELECT 'TBL_OBSERVATION_EVIDENCE', COUNT(*) FROM tbl_observation_evidence
UNION ALL
SELECT 'TBL_COMPLIANCE_CASE', COUNT(*) FROM tbl_compliance_case
UNION ALL
SELECT 'TBL_COMPLIANCE_CASE_HISTORY', COUNT(*) FROM tbl_compliance_case_history
UNION ALL
SELECT 'TBL_PARA_CASE', COUNT(*) FROM tbl_para_case
UNION ALL
SELECT 'TBL_PARA_STATUS_HISTORY', COUNT(*) FROM tbl_para_status_history
UNION ALL
SELECT 'TBL_PARA_SETTLEMENT_HISTORY', COUNT(*) FROM tbl_para_settlement_history
UNION ALL
SELECT 'TBL_IID_CASE', COUNT(*) FROM tbl_iid_case
UNION ALL
SELECT 'TBL_IID_WORKFLOW_HISTORY', COUNT(*) FROM tbl_iid_workflow_history
UNION ALL
SELECT 'TBL_REPORT', COUNT(*) FROM tbl_report
UNION ALL
SELECT 'TBL_WORKFLOW_EVENT', COUNT(*) FROM tbl_workflow_event;
