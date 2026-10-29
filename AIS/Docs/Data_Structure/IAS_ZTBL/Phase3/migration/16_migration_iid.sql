/*
  IAS_ZTBL Phase 3 IID / inquiry migration

  Scope
  - IID cases and complainants
  - subjects, accusations, statements
  - investigation plans and approvals
  - reports, findings, analysis, case studies, proceedings
  - exception-report scaffolding
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- IID cases
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
            TO_CHAR(c.COMPLAINT_ID) AS source_id,
            NVL(c.COMPLAINT_NO, 'IID-' || TO_CHAR(c.COMPLAINT_ID)) AS case_no,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'COMPLAINT_SOURCE'
                   AND lv.lookup_code = CASE
                                          WHEN UPPER(NVL(c.INTAKE_CHANNEL, 'INTERNAL')) LIKE '%ANON%' THEN 'ANONYMOUS'
                                          WHEN UPPER(NVL(c.INTAKE_CHANNEL, 'INTERNAL')) LIKE '%EXTERNAL%' THEN 'EXTERNAL'
                                          ELSE 'INTERNAL'
                                        END
            ) AS intake_channel_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'COMPLAINT_SOURCE'
                   AND lv.lookup_code = CASE
                                          WHEN UPPER(NVL(c.INTAKE_CHANNEL, 'INTERNAL')) LIKE '%ANON%' THEN 'ANONYMOUS'
                                          WHEN UPPER(NVL(c.INTAKE_CHANNEL, 'INTERNAL')) LIKE '%EXTERNAL%' THEN 'EXTERNAL'
                                          ELSE 'INTERNAL'
                                        END
            ) AS complaint_source_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'COMPLAINT_TYPE'
                   AND lv.lookup_code = CASE WHEN UPPER(NVL(c.COMPLAINT_NO, '')) LIKE 'INQ%' THEN 'INQUIRY' ELSE 'COMPLAINT' END
            ) AS complaint_type_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'IID_CASE_STATUS'
                   AND lv.lookup_code = CASE
                                          WHEN NVL(c.IS_FINALIZED, 'N') = 'Y' THEN 'FINALIZED'
                                          WHEN NVL(c.STATUS_ID, 0) IN (5, 6) THEN 'REPORT_DRAFTED'
                                          WHEN NVL(c.STATUS_ID, 0) IN (4) THEN 'UNDER_HEAD_REVIEW'
                                          WHEN NVL(c.STATUS_ID, 0) IN (3) THEN 'INVESTIGATION_PLANNED'
                                          WHEN NVL(c.STATUS_ID, 0) IN (2) THEN 'UNDER_ASSESSMENT'
                                          WHEN NVL(c.STATUS_ID, 0) IN (1) THEN 'UNDER_ANALYSIS'
                                          ELSE 'RECEIVED'
                                        END
            ) AS case_status_id,
            'Legacy IID status=' || NVL(c.STATUS, 'N/A') AS case_summary,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(c.ASSIGNED_UNIT_ID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS assigned_entity_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'IID_PRIORITY'
                   AND lv.lookup_code = CASE WHEN NVL(c.STATUS_ID, 0) >= 4 THEN 'HIGH' ELSE 'MEDIUM' END
            ) AS priority_id,
            NVL(c.SUBMITTED_ON, SYSDATE) AS submitted_on,
            c.SUBMITTED_BY_PP_NO AS submitted_by,
            c.FINALIZED_ON AS finalized_on,
            CASE WHEN NVL(c.ACTIVE_FLAG, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            NVL(c.SUBMITTED_BY_PP_NO, 0) AS created_by,
            c.UPDATED_BY_PP_NO AS modified_by,
            c.UPDATED_ON AS modified_on
        FROM ZTBLAIS_PROD.T_AU_IID_COMPLAINT_HDR c
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
               AND m.source_pk_value = TO_CHAR(c.COMPLAINT_ID)
               AND m.target_table_name = 'TBL_IID_CASE'
        )
    ) LOOP
        l_new_id := seq_iid_case.NEXTVAL;

        INSERT INTO tbl_iid_case (
            iid_case_id, case_no, intake_channel_id, complaint_source_id, complaint_type_id,
            case_status_id, case_summary, assigned_entity_id, priority_id,
            submitted_on, submitted_by, finalized_on, is_active, created_by, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.case_no, rec.intake_channel_id, rec.complaint_source_id, rec.complaint_type_id,
            rec.case_status_id, rec.case_summary, rec.assigned_entity_id, rec.priority_id,
            rec.submitted_on, rec.submitted_by, rec.finalized_on, rec.is_active, rec.created_by, rec.modified_by, rec.modified_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_IID_COMPLAINT_HDR',
            'COMPLAINT_ID', rec.source_id, 'TBL_IID_CASE', 'IID_CASE_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.submitted_by, 'IID case migrated.', rec.submitted_by
        );
    END LOOP;
END;
/

-------------------------------------------------------------------------------
-- IID complainants and subjects
-------------------------------------------------------------------------------

INSERT INTO tbl_iid_complainant (
    iid_complainant_id, iid_case_id, complainant_name, contact_no, cnic_no, mailing_address,
    gender_code, is_primary_complainant, is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    case_map.target_pk_value,
    c.COMPLAINANT_NAME,
    c.CELLULAR_NUMBER,
    c.CNIC,
    c.MAILING_ADDRESS,
    c.GENDER,
    CASE WHEN NVL(c.IS_PRIMARY, 'N') = 'Y' THEN 'Y' ELSE 'N' END,
    CASE WHEN NVL(c.ACTIVE_FLAG, 'Y') = 'Y' THEN 'Y' ELSE 'N' END,
    NVL(c.CREATED_BY_PP_NO, 0),
    NVL(c.CREATED_ON, SYSDATE),
    c.UPDATED_BY_PP_NO,
    c.UPDATED_ON
FROM ZTBLAIS_PROD.T_AU_IID_COMPLAINANT c
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(c.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE';

INSERT INTO tbl_iid_subject (
    iid_subject_id, iid_case_id, subject_type_id, subject_name, subject_pp_no,
    allegation_summary, cnic_no, designation_title, father_name, remarks,
    role_type_code, sort_order, subject_status_id, accusation_text, is_primary_subject,
    is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    case_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'SUBJECT_TYPE' AND lv.lookup_code = 'ACCUSED'),
    a.PERSON_NAME,
    TO_NUMBER(NULLIF(a.PPNO_NUMBER, '')),
    NULL,
    a.CNIC,
    a.DESIGNATION,
    a.FATHER_NAME,
    a.REMARKS,
    a.ROLE_TYPE,
    NVL(a.SORT_ORDER, 0),
    NULL,
    NULL,
    CASE WHEN NVL(a.SORT_ORDER, 1) = 1 THEN 'Y' ELSE 'N' END,
    CASE WHEN NVL(a.STATUS, 'Y') IN ('Y', 'ACTIVE') THEN 'Y' ELSE 'N' END,
    NVL(a.CREATED_BY, 0),
    NVL(a.CREATED_ON, SYSDATE),
    a.UPDATED_BY,
    a.UPDATED_ON
FROM ZTBLAIS_PROD.T_AU_IID_INQ_ACCUSED_LIST a
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(a.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE';

-------------------------------------------------------------------------------
-- Investigation plan and approval
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
            TO_CHAR(p.PLAN_ID) AS source_id,
            case_map.target_pk_value AS iid_case_id,
            NVL(p.PLAN_TITLE, 'Legacy Plan ' || TO_CHAR(p.PLAN_ID)) AS plan_title,
            p.PLAN_DETAILS AS plan_details,
            (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'PLAN_STATUS' AND lv.lookup_code = CASE WHEN UPPER(NVL(p.STATUS, 'DRAFT')) LIKE '%APPROV%' THEN 'APPROVED' WHEN UPPER(NVL(p.STATUS, 'DRAFT')) LIKE '%SUBMIT%' THEN 'SUBMITTED' ELSE 'DRAFT' END) AS plan_status_id,
            p.START_DATE AS start_date,
            p.END_DATE AS end_date,
            (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'RISK_RATING' AND lv.lookup_code = CASE WHEN UPPER(NVL(p.INVESTIGATION_RISK, 'MEDIUM')) LIKE '%LOW%' THEN 'LOW' WHEN UPPER(NVL(p.INVESTIGATION_RISK, 'MEDIUM')) LIKE '%HIGH%' THEN 'HIGH' ELSE 'MEDIUM' END) AS risk_level_id,
            (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'SIZE_BAND' AND lv.lookup_code = CASE WHEN UPPER(NVL(p.INVESTIGATION_SIZE, 'MEDIUM')) LIKE '%SMALL%' THEN 'SMALL' WHEN UPPER(NVL(p.INVESTIGATION_SIZE, 'MEDIUM')) LIKE '%LARGE%' THEN 'LARGE' ELSE 'MEDIUM' END) AS size_band_id,
            p.NO_OF_DAYS AS duration_days,
            p.PLAN_TITLE AS heading_text,
            p.ACTIVITIES_TEXT AS activities_text,
            p.TEAM_LEAD AS team_lead_text,
            p.TEAM_MEMBERS AS team_members_text,
            p.TRAVELLING_DAYS AS travelling_days,
            p.SUBMITTED_BY AS submitted_by,
            NVL(p.SUBMITTED_ON, SYSDATE) AS submitted_on
        FROM ZTBLAIS_PROD.T_AU_IID_INV_PLAN p
        JOIN tbl_legacy_key_map case_map
          ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
         AND case_map.source_pk_value = TO_CHAR(p.COMPLAINT_ID)
         AND case_map.target_table_name = 'TBL_IID_CASE'
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_IID_INV_PLAN'
               AND m.source_pk_value = TO_CHAR(p.PLAN_ID)
               AND m.target_table_name = 'TBL_IID_INVESTIGATION_PLAN'
        )
    ) LOOP
        l_new_id := seq_iid_investigation_plan.NEXTVAL;

        INSERT INTO tbl_iid_investigation_plan (
            iid_investigation_plan_id, iid_case_id, plan_title, plan_details, plan_status_id,
            start_date, end_date, risk_level_id, size_band_id, duration_days,
            heading_text, activities_text, team_lead_text, team_members_text, travelling_days,
            submitted_by, submitted_on, is_active, created_by
        )
        VALUES (
            l_new_id, rec.iid_case_id, rec.plan_title, rec.plan_details, rec.plan_status_id,
            rec.start_date, rec.end_date, rec.risk_level_id, rec.size_band_id, rec.duration_days,
            rec.heading_text, rec.activities_text, rec.team_lead_text, rec.team_members_text, rec.travelling_days,
            rec.submitted_by, rec.submitted_on, 'Y', rec.submitted_by
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_IID_INV_PLAN',
            'PLAN_ID', rec.source_id, 'TBL_IID_INVESTIGATION_PLAN', 'IID_INVESTIGATION_PLAN_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.submitted_by, 'IID plan migrated.', rec.submitted_by
        );
    END LOOP;
END;
/

INSERT INTO tbl_iid_plan_approval (
    iid_plan_approval_id, iid_investigation_plan_id, approval_status_id, approval_remarks,
    approved_by, approved_on, is_current_level, is_active, created_by
)
SELECT
    NULL,
    plan_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'PLAN_STATUS' AND lv.lookup_code = CASE WHEN UPPER(NVL(p.STATUS, 'DRAFT')) LIKE '%APPROV%' THEN 'APPROVED' WHEN UPPER(NVL(p.STATUS, 'DRAFT')) LIKE '%SUBMIT%' THEN 'SUBMITTED' ELSE 'DRAFT' END),
    'Migrated from legacy IID plan status.',
    p.SUBMITTED_BY,
    p.SUBMITTED_ON,
    'Y',
    'Y',
    NVL(p.SUBMITTED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_INV_PLAN p
JOIN tbl_legacy_key_map plan_map
  ON plan_map.source_table_name = 'T_AU_IID_INV_PLAN'
 AND plan_map.source_pk_value = TO_CHAR(p.PLAN_ID)
 AND plan_map.target_table_name = 'TBL_IID_INVESTIGATION_PLAN';

-------------------------------------------------------------------------------
-- Statements, violations, findings, and reports
-------------------------------------------------------------------------------

INSERT INTO tbl_iid_statement (
    iid_statement_id, iid_case_id, statement_type_id, person_name, person_identifier,
    statement_text, recorded_by, recorded_on, cnic_no, role_type_code, statement_status_id,
    statement_datetime, statement_mode_code, statement_place, key_points, uploaded_statement_text,
    is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    case_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'STATEMENT_TYPE' AND lv.lookup_code = 'WRITTEN'),
    s.PERSON_NAME,
    s.PPNO_NUMBER,
    s.KEY_POINTS,
    s.CREATED_BY,
    NVL(s.CREATED_ON, SYSDATE),
    s.CNIC,
    s.ROLE_TYPE,
    NULL,
    s.STATEMENT_DATETIME,
    s.MODE_TYPE,
    s.PLACE,
    s.KEY_POINTS,
    s.UPLOADED_STATEMENT,
    CASE WHEN NVL(s.STATUS, 'Y') IN ('Y', 'ACTIVE') THEN 'Y' ELSE 'N' END,
    NVL(s.CREATED_BY, 0),
    NVL(s.CREATED_ON, SYSDATE),
    s.UPDATED_BY,
    s.UPDATED_ON
FROM ZTBLAIS_PROD.T_AU_IID_INQ_STATEMENTS s
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(s.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE';

INSERT INTO tbl_iid_violation (
    iid_violation_id, iid_case_id, violation_type_id, detail_text, violation_status_id, is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    case_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'FINDING_TYPE' AND lv.lookup_code = 'VIOLATION'),
    a.ACCUSATION_TEXT,
    NULL,
    CASE WHEN NVL(a.STATUS, 'Y') IN ('Y', 'ACTIVE') THEN 'Y' ELSE 'N' END,
    NVL(a.CREATED_BY, 0),
    NVL(a.CREATED_ON, SYSDATE),
    a.UPDATED_BY,
    a.UPDATED_ON
FROM ZTBLAIS_PROD.T_AU_IID_INQ_ACCUSATIONS a
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(a.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE';

INSERT INTO tbl_iid_report (
    iid_report_id, iid_case_id, report_no, report_version_no, report_status_id,
    executive_summary, proceedings_text, findings_text, recommendation_text,
    complainant_name, accused_name, gist_text, submitted_by, submitted_on,
    uploaded_dsa_path, uploaded_evidence_path, uploaded_report_path,
    is_active, created_by, created_on
)
SELECT
    NULL,
    case_map.target_pk_value,
    'IID-RPT-' || TO_CHAR(r.REPORT_ID),
    1,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'REPORT_STATE' AND lv.lookup_code = CASE WHEN r.SUBMITTED_ON IS NULL THEN 'DRAFT' ELSE 'FINALIZED' END),
    r.GIST,
    r.PROCEEDINGS,
    r.FINDINGS,
    r.RECOMMENDATION,
    r.NAME_COMPLAINANT,
    r.NAME_ACCUSED,
    r.GIST,
    r.SUBMITTED_BY,
    NVL(r.SUBMITTED_ON, SYSDATE),
    r.UPLOADED_DSA,
    r.UPLOADED_EVIDENCE,
    r.UPLOADED_REPORT,
    'Y',
    NVL(r.SUBMITTED_BY, 0),
    NVL(r.SUBMITTED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_AU_IID_REPORT r
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(r.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE';

INSERT INTO tbl_iid_finding (
    iid_finding_id, iid_case_id, finding_type_id, finding_status_id, finding_text,
    allegation_text, recommendation_text, severity_id, is_active, created_by, created_on
)
SELECT
    NULL,
    case_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'FINDING_TYPE' AND lv.lookup_code = 'EXCEPTION'),
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'IID_CASE_STATUS' AND lv.lookup_code = 'REPORT_DRAFTED'),
    r.FINDINGS,
    r.GIST,
    r.RECOMMENDATION,
    NULL,
    'Y',
    NVL(r.SUBMITTED_BY, 0),
    NVL(r.SUBMITTED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_AU_IID_REPORT r
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(r.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE';

INSERT INTO tbl_iid_analysis (
    iid_analysis_id, iid_case_id, analysis_summary, detail_text, status_id, prepared_by, prepared_on, is_active, created_by
)
SELECT
    NULL,
    case_map.target_pk_value,
    'Legacy IID analysis',
    NVL(r.GIST, r.FINDINGS),
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'IID_CASE_STATUS' AND lv.lookup_code = 'UNDER_ANALYSIS'),
    r.SUBMITTED_BY,
    NVL(r.SUBMITTED_ON, SYSDATE),
    'Y',
    NVL(r.SUBMITTED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_REPORT r
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(r.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE';

INSERT INTO tbl_iid_case_study (
    iid_case_study_id, iid_case_id, case_study_title, case_study_text, case_study_status_id,
    prepared_by, prepared_on, is_active, created_by
)
SELECT
    NULL,
    case_map.target_pk_value,
    'Legacy Case Study',
    r.GIST,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'IID_CASE_STATUS' AND lv.lookup_code = 'UNDER_ANALYSIS'),
    r.SUBMITTED_BY,
    NVL(r.SUBMITTED_ON, SYSDATE),
    'Y',
    NVL(r.SUBMITTED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_REPORT r
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(r.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE';

INSERT INTO tbl_iid_proceeding (
    iid_proceeding_id, iid_case_id, proceeding_on, detail_text, is_active, created_by
)
SELECT
    NULL,
    case_map.target_pk_value,
    NVL(r.SUBMITTED_ON, SYSDATE),
    r.PROCEEDINGS,
    'Y',
    NVL(r.SUBMITTED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_REPORT r
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(r.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
WHERE r.PROCEEDINGS IS NOT NULL;

INSERT INTO tbl_iid_head_review (
    iid_head_review_id, iid_case_id, review_summary, reviewed_by, reviewed_on, approved_by, approved_on, is_active, created_by
)
SELECT
    NULL,
    case_map.target_pk_value,
    'Legacy finalized case imported for head review traceability.',
    c.UPDATED_BY_PP_NO,
    c.UPDATED_ON,
    c.UPDATED_BY_PP_NO,
    c.FINALIZED_ON,
    'Y',
    NVL(c.UPDATED_BY_PP_NO, 0)
FROM ZTBLAIS_PROD.T_AU_IID_COMPLAINT_HDR c
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(c.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
WHERE NVL(c.IS_FINALIZED, 'N') = 'Y';

INSERT INTO tbl_iid_workflow_history (
    iid_workflow_history_id, iid_case_id, to_status_id, action_code, remarks, is_active, created_by
)
SELECT
    NULL,
    case_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'IID_CASE_STATUS' AND lv.lookup_code = CASE
                                  WHEN NVL(c.IS_FINALIZED, 'N') = 'Y' THEN 'FINALIZED'
                                  WHEN NVL(c.STATUS_ID, 0) IN (5, 6) THEN 'REPORT_DRAFTED'
                                  WHEN NVL(c.STATUS_ID, 0) IN (4) THEN 'UNDER_HEAD_REVIEW'
                                  WHEN NVL(c.STATUS_ID, 0) IN (3) THEN 'INVESTIGATION_PLANNED'
                                  WHEN NVL(c.STATUS_ID, 0) IN (2) THEN 'UNDER_ASSESSMENT'
                                  WHEN NVL(c.STATUS_ID, 0) IN (1) THEN 'UNDER_ANALYSIS'
                                  ELSE 'RECEIVED'
                                END),
    'LEGACY_STATUS_LOAD',
    'Legacy IID status imported.',
    'Y',
    NVL(c.SUBMITTED_BY_PP_NO, 0)
FROM ZTBLAIS_PROD.T_AU_IID_COMPLAINT_HDR c
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(c.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE';

INSERT INTO tbl_iid_record (
    iid_record_id, iid_case_id, record_type_id, heading_text, detail_text, is_active, created_by
)
SELECT
    NULL,
    case_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'DOCUMENT_TYPE' AND lv.lookup_code = 'EVIDENCE'),
    'Legacy Uploaded Paths',
    'REPORT=' || NVL(r.UPLOADED_REPORT, 'NULL') || ';EVIDENCE=' || NVL(r.UPLOADED_EVIDENCE, 'NULL') || ';DSA=' || NVL(r.UPLOADED_DSA, 'NULL'),
    'Y',
    NVL(r.SUBMITTED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_REPORT r
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(r.COMPLAINT_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
WHERE r.UPLOADED_REPORT IS NOT NULL OR r.UPLOADED_EVIDENCE IS NOT NULL OR r.UPLOADED_DSA IS NOT NULL;

INSERT INTO tbl_iid_assessment (
    iid_assessment_id, iid_case_id, assessment_status_id, detail_text, assessed_by, assessed_on, is_active, created_by
)
SELECT
    NULL,
    plan_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'IID_CASE_STATUS' AND lv.lookup_code = 'UNDER_ASSESSMENT'),
    'Risk=' || NVL(p.INVESTIGATION_RISK, 'N/A') || ';Size=' || NVL(p.INVESTIGATION_SIZE, 'N/A'),
    p.SUBMITTED_BY,
    NVL(p.SUBMITTED_ON, SYSDATE),
    'Y',
    NVL(p.SUBMITTED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_INV_PLAN p
JOIN tbl_legacy_key_map plan_map
  ON plan_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND plan_map.source_pk_value = TO_CHAR(p.COMPLAINT_ID)
 AND plan_map.target_table_name = 'TBL_IID_CASE';

-------------------------------------------------------------------------------
-- Exception-report scaffolding
-------------------------------------------------------------------------------

INSERT INTO tbl_iid_exception_report (
    iid_exception_report_id, iid_case_id, report_code, report_name, report_type_id, source_system_name, is_active, created_by
)
SELECT DISTINCT
    NULL,
    case_map.target_pk_value,
    NVL(rpt_mst.REPORT_CODE, 'EXC_ACC_' || TO_CHAR(a.REPORT_ID)),
    NVL(rpt_mst.REPORT_TITLE, 'Legacy Account Exception Report'),
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'REPORT_TYPE' AND lv.lookup_code = 'IID'),
    'IAS_LEGACY',
    'Y',
    NVL(a.CREATED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_EXC_ACCOUNT a
LEFT JOIN ZTBLAIS_PROD.T_AU_IID_EXC_REPORT_MST rpt_mst
  ON rpt_mst.REPORT_ID = a.REPORT_ID
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(a.INQUIRY_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_iid_exception_report r
     WHERE r.iid_case_id = case_map.target_pk_value
       AND r.report_code = NVL(rpt_mst.REPORT_CODE, 'EXC_ACC_' || TO_CHAR(a.REPORT_ID))
);

INSERT INTO tbl_iid_exception_report (
    iid_exception_report_id, iid_case_id, report_code, report_name, report_type_id, source_system_name, is_active, created_by
)
SELECT DISTINCT
    NULL,
    case_map.target_pk_value,
    NVL(rpt_mst.REPORT_CODE, 'EXC_LOAN_' || TO_CHAR(l.REPORT_ID)),
    NVL(rpt_mst.REPORT_TITLE, 'Legacy Loan Exception Report'),
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'REPORT_TYPE' AND lv.lookup_code = 'IID'),
    'IAS_LEGACY',
    'Y',
    NVL(l.CREATED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_IID_EXC_LOAN l
LEFT JOIN ZTBLAIS_PROD.T_AU_IID_EXC_REPORT_MST rpt_mst
  ON rpt_mst.REPORT_ID = l.REPORT_ID
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(l.INQUIRY_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_iid_exception_report r
     WHERE r.iid_case_id = case_map.target_pk_value
       AND r.report_code = NVL(rpt_mst.REPORT_CODE, 'EXC_LOAN_' || TO_CHAR(l.REPORT_ID))
);

INSERT INTO tbl_iid_exception_column (
    iid_exception_column_id, iid_exception_report_id, column_name, column_label,
    data_type_name, column_sequence_no, is_key_column, is_active, created_by
)
WITH account_reports AS (
    SELECT DISTINCT
        rpt.iid_exception_report_id
    FROM ZTBLAIS_PROD.T_AU_IID_EXC_ACCOUNT a
    LEFT JOIN ZTBLAIS_PROD.T_AU_IID_EXC_REPORT_MST rpt_mst
      ON rpt_mst.REPORT_ID = a.REPORT_ID
    JOIN tbl_legacy_key_map case_map
      ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
     AND case_map.source_pk_value = TO_CHAR(a.INQUIRY_ID)
     AND case_map.target_table_name = 'TBL_IID_CASE'
    JOIN tbl_iid_exception_report rpt
      ON rpt.iid_case_id = case_map.target_pk_value
     AND rpt.report_code = NVL(rpt_mst.REPORT_CODE, 'EXC_ACC_' || TO_CHAR(a.REPORT_ID))
),
account_columns AS (
    SELECT 'ACCOUNT_NO' AS column_name, 'Account No' AS column_label, 'VARCHAR2' AS data_type_name, 10 AS column_sequence_no, 'Y' AS is_key_column FROM dual UNION ALL
    SELECT 'ACCOUNT_TITLE', 'Account Title', 'VARCHAR2', 20, 'N' FROM dual UNION ALL
    SELECT 'CUSTOMER_NAME', 'Customer Name', 'VARCHAR2', 30, 'N' FROM dual UNION ALL
    SELECT 'CNIC', 'CNIC', 'VARCHAR2', 40, 'N' FROM dual UNION ALL
    SELECT 'BRANCH_CODE', 'Branch Code', 'VARCHAR2', 50, 'N' FROM dual UNION ALL
    SELECT 'SOURCE_REF_NO', 'Source Ref No', 'VARCHAR2', 60, 'N' FROM dual UNION ALL
    SELECT 'EXCEPTION_DETAIL', 'Exception Detail', 'VARCHAR2', 70, 'N' FROM dual
)
SELECT
    NULL,
    ar.iid_exception_report_id,
    ac.column_name,
    ac.column_label,
    ac.data_type_name,
    ac.column_sequence_no,
    ac.is_key_column,
    'Y',
    0
FROM account_reports ar
JOIN account_columns ac
  ON 1 = 1
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_iid_exception_column c
     WHERE c.iid_exception_report_id = ar.iid_exception_report_id
       AND c.column_name = ac.column_name
);

INSERT INTO tbl_iid_exception_column (
    iid_exception_column_id, iid_exception_report_id, column_name, column_label,
    data_type_name, column_sequence_no, is_key_column, is_active, created_by
)
WITH loan_reports AS (
    SELECT DISTINCT
        rpt.iid_exception_report_id
    FROM ZTBLAIS_PROD.T_AU_IID_EXC_LOAN l
    LEFT JOIN ZTBLAIS_PROD.T_AU_IID_EXC_REPORT_MST rpt_mst
      ON rpt_mst.REPORT_ID = l.REPORT_ID
    JOIN tbl_legacy_key_map case_map
      ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
     AND case_map.source_pk_value = TO_CHAR(l.INQUIRY_ID)
     AND case_map.target_table_name = 'TBL_IID_CASE'
    JOIN tbl_iid_exception_report rpt
      ON rpt.iid_case_id = case_map.target_pk_value
     AND rpt.report_code = NVL(rpt_mst.REPORT_CODE, 'EXC_LOAN_' || TO_CHAR(l.REPORT_ID))
),
loan_columns AS (
    SELECT 'LOAN_DISB_ID' AS column_name, 'Loan Disbursement Id' AS column_label, 'VARCHAR2' AS data_type_name, 10 AS column_sequence_no, 'Y' AS is_key_column FROM dual UNION ALL
    SELECT 'TYPE', 'Type', 'VARCHAR2', 20, 'N' FROM dual UNION ALL
    SELECT 'SCHEME', 'Scheme', 'VARCHAR2', 30, 'N' FROM dual UNION ALL
    SELECT 'CUSTOMER_NAME', 'Customer Name', 'VARCHAR2', 40, 'N' FROM dual UNION ALL
    SELECT 'CNIC', 'CNIC', 'VARCHAR2', 50, 'N' FROM dual UNION ALL
    SELECT 'LC_NO', 'LC No', 'VARCHAR2', 60, 'N' FROM dual UNION ALL
    SELECT 'OUTSTANDING', 'Outstanding', 'NUMBER', 70, 'N' FROM dual UNION ALL
    SELECT 'EXCEPTION_DETAIL', 'Exception Detail', 'VARCHAR2', 80, 'N' FROM dual
)
SELECT
    NULL,
    lr.iid_exception_report_id,
    lc.column_name,
    lc.column_label,
    lc.data_type_name,
    lc.column_sequence_no,
    lc.is_key_column,
    'Y',
    0
FROM loan_reports lr
JOIN loan_columns lc
  ON 1 = 1
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_iid_exception_column c
     WHERE c.iid_exception_report_id = lr.iid_exception_report_id
       AND c.column_name = lc.column_name
);

INSERT INTO tbl_iid_exception_item (
    iid_exception_item_id, iid_exception_report_id, item_type_id, account_no, customer_identifier,
    item_reference_no, item_notes, extracted_on, is_active, created_by, created_on
)
SELECT
    NULL,
    rpt.iid_exception_report_id,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'FINDING_TYPE' AND lv.lookup_code = 'EXCEPTION'),
    a.ACCOUNT_NO,
    a.CNIC,
    a.SOURCE_REF_NO,
    SUBSTR(a.EXCEPTION_DETAIL, 1, 1000),
    a.CREATED_ON,
    'Y',
    NVL(a.CREATED_BY, 0),
    NVL(a.CREATED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_AU_IID_EXC_ACCOUNT a
LEFT JOIN ZTBLAIS_PROD.T_AU_IID_EXC_REPORT_MST rpt_mst
  ON rpt_mst.REPORT_ID = a.REPORT_ID
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(a.INQUIRY_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
JOIN tbl_iid_exception_report rpt
  ON rpt.iid_case_id = case_map.target_pk_value
 AND rpt.report_code = NVL(rpt_mst.REPORT_CODE, 'EXC_ACC_' || TO_CHAR(a.REPORT_ID))
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_iid_exception_item i
     WHERE i.iid_exception_report_id = rpt.iid_exception_report_id
       AND NVL(i.account_no, '~') = NVL(a.ACCOUNT_NO, '~')
       AND NVL(i.item_reference_no, '~') = NVL(a.SOURCE_REF_NO, '~')
);

INSERT INTO tbl_iid_exception_item (
    iid_exception_item_id, iid_exception_report_id, item_type_id, loan_no, customer_identifier,
    item_reference_no, item_notes, extracted_on, is_active, created_by, created_on
)
SELECT
    NULL,
    rpt.iid_exception_report_id,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'FINDING_TYPE' AND lv.lookup_code = 'EXCEPTION'),
    l.LOAN_DISB_ID,
    l.CNIC,
    l.LC_NO,
    SUBSTR('TYPE=' || NVL(l.TYPE, 'N/A') || ';SCHEME=' || NVL(l.SCHEME, 'N/A') || ';PURPOSE=' || NVL(l.L_PURPOSE, 'N/A') || ';DEV_AMOUNT=' || NVL(TO_CHAR(l.DEV_AMOUNT), 'N/A') || ';OUTSTANDING=' || NVL(TO_CHAR(l.OUTSTANDING), 'N/A') || ';DETAIL=' || NVL(l.EXCEPTION_DETAIL, 'N/A'), 1, 1000),
    l.CREATED_ON,
    'Y',
    NVL(l.CREATED_BY, 0),
    NVL(l.CREATED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_AU_IID_EXC_LOAN l
LEFT JOIN ZTBLAIS_PROD.T_AU_IID_EXC_REPORT_MST rpt_mst
  ON rpt_mst.REPORT_ID = l.REPORT_ID
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(l.INQUIRY_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
JOIN tbl_iid_exception_report rpt
  ON rpt.iid_case_id = case_map.target_pk_value
 AND rpt.report_code = NVL(rpt_mst.REPORT_CODE, 'EXC_LOAN_' || TO_CHAR(l.REPORT_ID))
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_iid_exception_item i
     WHERE i.iid_exception_report_id = rpt.iid_exception_report_id
       AND NVL(i.loan_no, '~') = NVL(l.LOAN_DISB_ID, '~')
       AND NVL(i.item_reference_no, '~') = NVL(l.LC_NO, '~')
);

INSERT INTO tbl_iid_exception_item_txn (
    iid_exception_item_txn_id, iid_exception_item_id, branch_code, txn_reference_no,
    txn_date, debit_amount, credit_amount, narrative_text, is_active, created_by, created_on
)
SELECT
    NULL,
    item.iid_exception_item_id,
    x.TO_ACC_BRANCH_ID,
    x.TRANSACTION_MASTER_CODE,
    x.TRANSACTION_DATE,
    x.DR_AMOUNT,
    x.CR_AMOUNT,
    x.DESCRIPTION,
    'Y',
    NVL(x.CREATED_BY, 0),
    NVL(x.CREATED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_AU_IID_EXC_ACCOUNT_TXN x
JOIN ZTBLAIS_PROD.T_AU_IID_EXC_ACCOUNT a
  ON a.INQUIRY_ID = x.INQUIRY_ID
 AND a.ACCOUNT_NO = x.ACCOUNT_NO
LEFT JOIN ZTBLAIS_PROD.T_AU_IID_EXC_REPORT_MST rpt_mst
  ON rpt_mst.REPORT_ID = a.REPORT_ID
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(x.INQUIRY_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
JOIN tbl_iid_exception_report rpt
  ON rpt.iid_case_id = case_map.target_pk_value
 AND rpt.report_code = NVL(rpt_mst.REPORT_CODE, 'EXC_ACC_' || TO_CHAR(a.REPORT_ID))
JOIN tbl_iid_exception_item item
  ON item.iid_exception_report_id = rpt.iid_exception_report_id
 AND item.account_no = x.ACCOUNT_NO
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_iid_exception_item_txn t
     WHERE t.iid_exception_item_id = item.iid_exception_item_id
       AND NVL(t.txn_reference_no, '~') = NVL(x.TRANSACTION_MASTER_CODE, '~')
       AND NVL(t.txn_date, DATE '1900-01-01') = NVL(x.TRANSACTION_DATE, DATE '1900-01-01')
);

INSERT INTO tbl_iid_exception_item_txn (
    iid_exception_item_txn_id, iid_exception_item_id, branch_code, txn_reference_no,
    txn_date, debit_amount, credit_amount, narrative_text, is_active, created_by, created_on
)
SELECT
    NULL,
    item.iid_exception_item_id,
    x.LN_ACCOUNT_ID,
    x.MANUAL_VOUCHER_NO,
    x.TRANSACTION_DATE,
    x.DR_AMOUNT,
    x.CR_AMOUNT,
    SUBSTR(NVL(x.DESCRIPTION, 'N/A') || ';REMARKS=' || NVL(x.REMARKS, 'N/A'), 1, 1000),
    'Y',
    NVL(x.CREATED_BY, 0),
    NVL(x.CREATED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_AU_IID_EXC_LOAN_TXN x
JOIN ZTBLAIS_PROD.T_AU_IID_EXC_LOAN l
  ON l.INQUIRY_ID = x.INQUIRY_ID
 AND l.LOAN_DISB_ID = x.LOAN_DISB_ID
LEFT JOIN ZTBLAIS_PROD.T_AU_IID_EXC_REPORT_MST rpt_mst
  ON rpt_mst.REPORT_ID = l.REPORT_ID
JOIN tbl_legacy_key_map case_map
  ON case_map.source_table_name = 'T_AU_IID_COMPLAINT_HDR'
 AND case_map.source_pk_value = TO_CHAR(x.INQUIRY_ID)
 AND case_map.target_table_name = 'TBL_IID_CASE'
JOIN tbl_iid_exception_report rpt
  ON rpt.iid_case_id = case_map.target_pk_value
 AND rpt.report_code = NVL(rpt_mst.REPORT_CODE, 'EXC_LOAN_' || TO_CHAR(l.REPORT_ID))
JOIN tbl_iid_exception_item item
  ON item.iid_exception_report_id = rpt.iid_exception_report_id
 AND item.loan_no = x.LOAN_DISB_ID
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_iid_exception_item_txn t
     WHERE t.iid_exception_item_id = item.iid_exception_item_id
       AND NVL(t.txn_reference_no, '~') = NVL(x.MANUAL_VOUCHER_NO, '~')
       AND NVL(t.txn_date, DATE '1900-01-01') = NVL(x.TRANSACTION_DATE, DATE '1900-01-01')
);

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_IID_EXC_REPORT_MST/T_AU_IID_EXC_REPORT_COL_MST',
    'N/A',
    'N/A',
    'ALL',
    'TBL_IID_EXCEPTION_COLUMN',
    'MANUAL_REVIEW',
    'Standardized first-pass IID exception column scaffolding has been applied from active legacy account/loan structures. Validate it against live DBA metadata before final cutover.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'T_AU_IID_EXC_REPORT_MST/T_AU_IID_EXC_REPORT_COL_MST'
);
