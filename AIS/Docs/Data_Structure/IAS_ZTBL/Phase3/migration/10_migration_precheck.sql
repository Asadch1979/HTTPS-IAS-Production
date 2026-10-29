/*
  IAS_ZTBL Phase 3 migration precheck

  Purpose
  - validate source schema accessibility
  - register the migration batch used by scripts 11-19
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

DECLARE
    l_missing_count NUMBER;
BEGIN
    SELECT COUNT(*)
      INTO l_missing_count
      FROM (
            SELECT 'T_USER' table_name FROM dual UNION ALL
            SELECT 'T_GROUPS' FROM dual UNION ALL
            SELECT 'T_AUDITEE_ENT_TYPES' FROM dual UNION ALL
            SELECT 'T_AUDITEE_ENTITIES' FROM dual UNION ALL
            SELECT 'T_AU_PERIOD' FROM dual UNION ALL
            SELECT 'T_AU_PLAN' FROM dual UNION ALL
            SELECT 'T_AU_PLAN_ENG' FROM dual UNION ALL
            SELECT 'AIS_T_AU_OBSERVATION' FROM dual UNION ALL
            SELECT 'AIS_T_AU_POST_COMPLIANCE' FROM dual UNION ALL
            SELECT 'T_AU_IID_COMPLAINT_HDR' FROM dual UNION ALL
            SELECT 'T_AU_IID_EXC_ACCOUNT' FROM dual UNION ALL
            SELECT 'T_AU_IID_EXC_ACCOUNT_TXN' FROM dual UNION ALL
            SELECT 'T_AU_IID_EXC_LOAN' FROM dual UNION ALL
            SELECT 'T_AU_IID_EXC_LOAN_TXN' FROM dual UNION ALL
            SELECT 'T_AU_IID_EXC_REPORT_MST' FROM dual UNION ALL
            SELECT 'T_AU_IID_EXC_ACCOUNT_DOC' FROM dual UNION ALL
            SELECT 'T_AU_IID_EXC_LOAN_DOC' FROM dual UNION ALL
            SELECT 'T_AU_IID_EXC_LOAN_DOC_IMG' FROM dual UNION ALL
            SELECT 'T_COM_AUDIT_OM' FROM dual UNION ALL
            SELECT 'T_FRPT_TEXT_BLOCKS' FROM dual UNION ALL
            SELECT 'EMAILTEMPLATES' FROM dual UNION ALL
            SELECT 'NOTIFICATIONEVENTS' FROM dual UNION ALL
            SELECT 'NOTIFICATIONRULES' FROM dual UNION ALL
            SELECT 'ATTACHMENTSOURCES' FROM dual UNION ALL
            SELECT 'T_AU_EMAIL_QUEUE' FROM dual
           ) req
      LEFT JOIN all_tables t
        ON t.owner = 'ZTBLAIS_PROD'
       AND t.table_name = req.table_name
     WHERE t.table_name IS NULL;

    IF l_missing_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Required legacy source tables are missing or not granted to IAS_ZTBL.');
    END IF;
END;
/

INSERT INTO tbl_migration_batch (
    migration_batch_id,
    batch_code,
    batch_name,
    source_schema_name,
    source_system_code,
    batch_status_code,
    started_on,
    executed_by,
    remarks,
    created_by
)
SELECT
    NULL,
    'PHASE3_BASELINE_01',
    'IAS_ZTBL Phase 3 Baseline Rehearsal',
    'ZTBLAIS_PROD',
    'IAS_LEGACY',
    'REGISTERED',
    SYSDATE,
    0,
    'Registered by migration precheck script.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_batch
     WHERE batch_code = 'PHASE3_BASELINE_01'
);

PROMPT Migration batch PHASE3_BASELINE_01 is ready.
