/*
  IAS_ZTBL Phase 3 FRPT/report migration

  Scope
  - FRPT report roots
  - text blocks, narrative, conclusions, and leakage lines into report sections
  - KPI/NPL/staff/statistics snapshots
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Report roots
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
            TO_CHAR(src.ENG_ID) AS source_id,
            eng_map.target_pk_value AS engagement_id,
            (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'REPORT_TYPE' AND lv.lookup_code = 'FRPT') AS report_type_id,
            'Field Audit Report - ' || TO_CHAR(src.ENG_ID) AS report_title,
            NVL(src.REPORT_VERSION, 1) AS report_version_no,
            (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'REPORT_STATE' AND lv.lookup_code = 'IN_PREPARATION') AS report_status_id,
            SYSDATE AS prepared_on,
            0 AS created_by
        FROM (
            SELECT ENG_ID, 1 AS REPORT_VERSION FROM ZTBLAIS_PROD.T_FRPT_TEXT_BLOCKS
            UNION
            SELECT ENG_ID, 1 FROM ZTBLAIS_PROD.T_FRPT_PARA_NARRATIVE
            UNION
            SELECT ENG_ID, REPORT_VERSION FROM ZTBLAIS_PROD.T_FRPT_OVERALL_CONCLUSION
            UNION
            SELECT ENG_ID, REPORT_VERSION FROM ZTBLAIS_PROD.T_FRPT_PDF_STATISTICS
        ) src
        JOIN tbl_legacy_key_map eng_map
          ON eng_map.source_table_name = 'T_AU_PLAN_ENG'
         AND eng_map.source_pk_value = TO_CHAR(src.ENG_ID)
         AND eng_map.target_table_name = 'TBL_ENGAGEMENT'
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'FRPT_ENG'
               AND m.source_pk_value = TO_CHAR(src.ENG_ID)
               AND m.target_table_name = 'TBL_REPORT'
        )
    ) LOOP
        l_new_id := seq_report.NEXTVAL;

        INSERT INTO tbl_report (
            report_id, engagement_id, report_type_id, report_title, report_version_no,
            report_status_id, prepared_on, is_active, created_by
        )
        VALUES (
            l_new_id, rec.engagement_id, rec.report_type_id, rec.report_title, rec.report_version_no,
            rec.report_status_id, rec.prepared_on, 'Y', rec.created_by
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'FRPT_ENG',
            'ENG_ID', rec.source_id, 'TBL_REPORT', 'REPORT_ID',
            l_new_id, 'IAS_LEGACY', 'DERIVED', rec.created_by, 'FRPT report root created.', rec.created_by
        );
    END LOOP;
END;
/

-------------------------------------------------------------------------------
-- Report sections
-------------------------------------------------------------------------------

INSERT INTO tbl_report_section (
    report_section_id, report_id, section_code, section_title, sequence_no,
    section_text, is_active, created_by, created_on
)
SELECT
    NULL,
    rpt.report_id,
    tb.SECTION_CODE,
    tb.SECTION_CODE,
    tb.FRPT_TEXT_ID,
    tb.SECTION_TEXT,
    'Y',
    NVL(tb.CREATED_BY, 0),
    NVL(tb.CREATED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_FRPT_TEXT_BLOCKS tb
JOIN tbl_legacy_key_map rpt_map
  ON rpt_map.source_table_name = 'FRPT_ENG'
 AND rpt_map.source_pk_value = TO_CHAR(tb.ENG_ID)
 AND rpt_map.target_table_name = 'TBL_REPORT'
JOIN tbl_report rpt
  ON rpt.report_id = rpt_map.target_pk_value
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_report_section x
     WHERE x.report_id = rpt.report_id
       AND x.section_code = tb.SECTION_CODE
);

INSERT INTO tbl_report_section (
    report_section_id, report_id, section_code, section_title, sequence_no,
    section_text, implications_text, recommendations_text, management_comments,
    auditor_comments, svp_remarks, finalized_by, finalized_on, is_finalized_flag,
    lock_version, update_required_flag, is_active, created_by, modified_by, modified_on
)
SELECT
    NULL,
    rpt.report_id,
    'PARA_' || TO_CHAR(p.PARA_ID),
    'Para Narrative',
    p.PARA_ID,
    NULL,
    p.IMPLICATIONS,
    p.RECOMMENDATIONS,
    p.MGMT_COMMENTS,
    p.AUDITOR_COMMENTS,
    p.SVP_REMARKS,
    p.FINALIZED_BY,
    p.FINALIZED_ON,
    CASE WHEN NVL(p.IS_FINALIZED, 0) = 1 THEN 'Y' ELSE 'N' END,
    p.LOCK_VERSION,
    CASE WHEN NVL(p.UPDATED_REQUIRED, 0) = 1 THEN 'Y' ELSE 'N' END,
    'Y',
    NVL(p.UPDATED_BY, 0),
    p.UPDATED_BY,
    p.UPDATED_ON
FROM ZTBLAIS_PROD.T_FRPT_PARA_NARRATIVE p
JOIN tbl_legacy_key_map rpt_map
  ON rpt_map.source_table_name = 'FRPT_ENG'
 AND rpt_map.source_pk_value = TO_CHAR(p.ENG_ID)
 AND rpt_map.target_table_name = 'TBL_REPORT'
JOIN tbl_report rpt
  ON rpt.report_id = rpt_map.target_pk_value;

INSERT INTO tbl_report_section (
    report_section_id, report_id, section_code, section_title, sequence_no,
    section_text, entity_id, report_version_no, is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    rpt.report_id,
    'OVERALL_CONCLUSION',
    'Overall Conclusion',
    9990,
    NVL(c.OVERALL_CONCLUSION_HTML, NULL),
    ent_map.target_pk_value,
    c.REPORT_VERSION,
    'Y',
    NVL(c.CREATED_BY, 0),
    NVL(c.CREATED_ON, SYSDATE),
    c.UPDATED_BY,
    c.UPDATED_ON
FROM ZTBLAIS_PROD.T_FRPT_OVERALL_CONCLUSION c
JOIN tbl_legacy_key_map rpt_map
  ON rpt_map.source_table_name = 'FRPT_ENG'
 AND rpt_map.source_pk_value = TO_CHAR(c.ENG_ID)
 AND rpt_map.target_table_name = 'TBL_REPORT'
JOIN tbl_report rpt
  ON rpt.report_id = rpt_map.target_pk_value
LEFT JOIN tbl_legacy_key_map ent_map
  ON ent_map.source_table_name = 'T_AUDITEE_ENTITIES'
 AND ent_map.source_pk_value = TO_CHAR(c.ENTITY_ID)
 AND ent_map.target_table_name = 'TBL_ENTITY'
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_report_section x
     WHERE x.report_id = rpt.report_id
       AND x.section_code = 'OVERALL_CONCLUSION'
);

INSERT INTO tbl_report_section (
    report_section_id, report_id, section_code, section_title, sequence_no,
    entity_id, description, line_no, report_version_no, section_text, is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    rpt.report_id,
    'INCOME_LEAKAGE_' || TO_CHAR(l.FRPT_LEAK_ID),
    'Income Leakage',
    l.LINE_NO,
    ent_map.target_pk_value,
    l.AREA || ': ' || l.DESCRIPTION,
    l.LINE_NO,
    l.REPORT_VERSION,
    l.DESCRIPTION,
    'Y',
    NVL(l.CREATED_BY, 0),
    NVL(l.CREATED_ON, SYSDATE),
    l.UPDATED_BY,
    l.UPDATED_ON
FROM ZTBLAIS_PROD.T_FRPT_INCOME_LEAKAGE l
JOIN tbl_legacy_key_map rpt_map
  ON rpt_map.source_table_name = 'FRPT_ENG'
 AND rpt_map.source_pk_value = TO_CHAR(l.ENG_ID)
 AND rpt_map.target_table_name = 'TBL_REPORT'
JOIN tbl_report rpt
  ON rpt.report_id = rpt_map.target_pk_value
LEFT JOIN tbl_legacy_key_map ent_map
  ON ent_map.source_table_name = 'T_AUDITEE_ENTITIES'
 AND ent_map.source_pk_value = TO_CHAR(l.ENTITY_ID)
 AND ent_map.target_table_name = 'TBL_ENTITY';

-------------------------------------------------------------------------------
-- Snapshots
-------------------------------------------------------------------------------

INSERT INTO tbl_report_snapshot (
    report_snapshot_id, report_id, snapshot_type_id, snapshot_key, snapshot_value,
    sequence_no, entity_id, snapshot_name, period_end, actual_value, target_value, unit_code,
    is_active, created_by, created_on
)
SELECT
    NULL,
    rpt.report_id,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'SNAPSHOT_TYPE' AND lv.lookup_code = 'KPI'),
    k.KPI_CODE,
    TO_CHAR(k.ACTUAL_VALUE),
    k.FRPT_KPI_ID,
    ent_map.target_pk_value,
    k.KPI_LABEL,
    k.PERIOD_END,
    k.ACTUAL_VALUE,
    k.TARGET_VALUE,
    k.UNIT,
    'Y',
    NVL(k.CREATED_BY, 0),
    NVL(k.CREATED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_FRPT_KPI_SNAPSHOT k
JOIN tbl_legacy_key_map rpt_map
  ON rpt_map.source_table_name = 'FRPT_ENG'
 AND rpt_map.source_pk_value = TO_CHAR(k.ENG_ID)
 AND rpt_map.target_table_name = 'TBL_REPORT'
JOIN tbl_report rpt
  ON rpt.report_id = rpt_map.target_pk_value
LEFT JOIN tbl_legacy_key_map ent_map
  ON ent_map.source_table_name = 'T_AUDITEE_ENTITIES'
 AND ent_map.source_pk_value = TO_CHAR(k.ENTITY_ID)
 AND ent_map.target_table_name = 'TBL_ENTITY';

INSERT INTO tbl_report_snapshot (
    report_snapshot_id, report_id, snapshot_type_id, snapshot_key, sequence_no,
    entity_id, period_end, cases_count, outstanding_amount, provision_amount,
    is_active, created_by, created_on
)
SELECT
    NULL,
    rpt.report_id,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'SNAPSHOT_TYPE' AND lv.lookup_code = 'NPL'),
    n.NPL_BUCKET,
    n.FRPT_NPL_ID,
    ent_map.target_pk_value,
    n.PERIOD_END,
    n.CASES_COUNT,
    n.OUTSTANDING_AMT,
    n.PROVISION_AMT,
    'Y',
    NVL(n.CREATED_BY, 0),
    NVL(n.CREATED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_FRPT_NPL_SNAPSHOT n
JOIN tbl_legacy_key_map rpt_map
  ON rpt_map.source_table_name = 'FRPT_ENG'
 AND rpt_map.source_pk_value = TO_CHAR(n.ENG_ID)
 AND rpt_map.target_table_name = 'TBL_REPORT'
JOIN tbl_report rpt
  ON rpt.report_id = rpt_map.target_pk_value
LEFT JOIN tbl_legacy_key_map ent_map
  ON ent_map.source_table_name = 'T_AUDITEE_ENTITIES'
 AND ent_map.source_pk_value = TO_CHAR(n.ENTITY_ID)
 AND ent_map.target_table_name = 'TBL_ENTITY';

INSERT INTO tbl_report_snapshot (
    report_snapshot_id, report_id, snapshot_type_id, snapshot_key, sequence_no,
    entity_id, snapshot_name, snapshot_value, is_source_active, is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    rpt.report_id,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'SNAPSHOT_TYPE' AND lv.lookup_code = 'STAFF'),
    s.PP_NO,
    s.FRPT_STAFF_ID,
    ent_map.target_pk_value,
    s.STAFF_NAME,
    s.DESIGNATION,
    CASE WHEN NVL(s.IS_ACTIVE, 1) = 1 THEN 'Y' ELSE 'N' END,
    'Y',
    0,
    NVL(s.CREATED_ON, SYSDATE),
    0,
    s.UPDATED_ON
FROM ZTBLAIS_PROD.T_FRPT_STAFF_SNAPSHOT s
JOIN tbl_legacy_key_map rpt_map
  ON rpt_map.source_table_name = 'FRPT_ENG'
 AND rpt_map.source_pk_value = TO_CHAR(s.ENG_ID)
 AND rpt_map.target_table_name = 'TBL_REPORT'
JOIN tbl_report rpt
  ON rpt.report_id = rpt_map.target_pk_value
LEFT JOIN tbl_legacy_key_map ent_map
  ON ent_map.source_table_name = 'T_AUDITEE_ENTITIES'
 AND ent_map.source_pk_value = TO_CHAR(s.ENTITY_ID)
 AND ent_map.target_table_name = 'TBL_ENTITY';

INSERT INTO tbl_report_snapshot (
    report_snapshot_id, report_id, snapshot_type_id, snapshot_key, sequence_no,
    entity_id, report_version_no, risk_level_code, reported_count, rectified_count, outstanding_count,
    remarks, is_active, created_by, created_on
)
SELECT
    NULL,
    rpt.report_id,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'SNAPSHOT_TYPE' AND lv.lookup_code = 'STATISTICS'),
    'PDF_STATS',
    p.FRPT_STAT_ID,
    ent_map.target_pk_value,
    p.REPORT_VERSION,
    p.RISK_LEVEL,
    p.REPORTED_COUNT,
    p.RECTIFIED_COUNT,
    p.OUTSTANDING_COUNT,
    p.REMARKS,
    'Y',
    NVL(p.CREATED_BY, 0),
    NVL(p.CREATED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_FRPT_PDF_STATISTICS p
JOIN tbl_legacy_key_map rpt_map
  ON rpt_map.source_table_name = 'FRPT_ENG'
 AND rpt_map.source_pk_value = TO_CHAR(p.ENG_ID)
 AND rpt_map.target_table_name = 'TBL_REPORT'
JOIN tbl_report rpt
  ON rpt.report_id = rpt_map.target_pk_value
LEFT JOIN tbl_legacy_key_map ent_map
  ON ent_map.source_table_name = 'T_AUDITEE_ENTITIES'
 AND ent_map.source_pk_value = TO_CHAR(p.ENTITY_ID)
 AND ent_map.target_table_name = 'TBL_ENTITY';

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_FRPT_OVERALL_CONCLUSION',
    'FRPT_CONC_ID',
    'MULTIPLE_ROWS',
    'NON_ADDRESSABLE_HTML/FRAUD_PRONE_HTML/REGULATORY_HTML/SAFETY_SECURITY_HTML',
    'TBL_REPORT_SECTION',
    'MANUAL_REVIEW',
    'Additional FRPT conclusion facets should be modeled into dedicated report-section or snapshot structures before UAT cutover.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'T_FRPT_OVERALL_CONCLUSION'
       AND source_column_name = 'NON_ADDRESSABLE_HTML/FRAUD_PRONE_HTML/REGULATORY_HTML/SAFETY_SECURITY_HTML'
);
