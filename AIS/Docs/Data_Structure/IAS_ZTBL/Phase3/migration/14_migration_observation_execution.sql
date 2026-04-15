/*
  IAS_ZTBL Phase 3 observation and execution migration

  Scope
  - observations
  - observation detail text
  - assignments and responses
  - evidence, updated references, DSA rows
  - working-paper placeholders
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Observations
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
            TO_CHAR(o.ID) AS source_id,
            'OBS_' || TO_CHAR(o.ID) AS observation_no,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AU_PLAN_ENG'
                   AND m.source_pk_value = TO_CHAR(o.ENGPLANID)
                   AND m.target_table_name = 'TBL_ENGAGEMENT'
            ) AS engagement_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(o.ENTITY_ID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS entity_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'OBSERVATION_STATUS'
                   AND lv.lookup_code = CASE
                                          WHEN NVL(o.STATUS, 0) = 0 THEN 'DRAFT'
                                          WHEN o.STATUS IN (1, 2) THEN 'SUBMITTED_TO_AUDITEE'
                                          WHEN o.STATUS IN (3, 4) THEN 'AUDITEE_RESPONDED'
                                          WHEN o.STATUS IN (5, 6) THEN 'CONCLUDED'
                                          ELSE 'SETTLED'
                                        END
            ) AS observation_status_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'SEVERITY'
                   AND lv.lookup_code = CASE NVL(o.SEVERITY, 2) WHEN 1 THEN 'LOW' WHEN 2 THEN 'MEDIUM' WHEN 3 THEN 'HIGH' ELSE 'CRITICAL' END
            ) AS severity_id,
            CASE
                WHEN REGEXP_LIKE(NVL(o.AMOUNT_INVOLVED, '0'), '^[0-9]+(\.[0-9]+)?$')
                THEN TO_NUMBER(o.AMOUNT_INVOLVED)
                ELSE NULL
            END AS amount_involved,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'RISK_RATING'
                   AND lv.lookup_code = CASE NVL(o.RISKMODEL_ID, 2) WHEN 1 THEN 'LOW' WHEN 2 THEN 'MEDIUM' WHEN 3 THEN 'HIGH' ELSE 'CRITICAL' END
            ) AS risk_rating_id,
            o.NO_OF_INSTANCES AS instance_count,
            o.MEMO_DATE AS memo_date,
            TO_CHAR(o.MEMO_NUMBER) AS memo_no,
            o.DRAFT_PARA_NO AS draft_para_no,
            o.FINAL_PARA_NO AS final_para_no,
            o.STELLED_ON AS settled_on,
            o.SETTLED_BY AS settled_by,
            NVL(o.ENTEREDBY, 0) AS created_by,
            NVL(o.ENTEREDDATE, SYSDATE) AS created_on,
            o.LASTUPDATEDBY AS modified_by,
            o.LASTUPDATEDDATE AS modified_on
        FROM ZTBLAIS_PROD.AIS_T_AU_OBSERVATION o
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'AIS_T_AU_OBSERVATION'
               AND m.source_pk_value = TO_CHAR(o.ID)
               AND m.target_table_name = 'TBL_OBSERVATION'
        )
    ) LOOP
        l_new_id := seq_observation.NEXTVAL;

        INSERT INTO tbl_observation (
            observation_id, observation_no, engagement_id, entity_id, observation_status_id,
            severity_id, amount_involved, risk_rating_id, instance_count, memo_date, memo_no,
            draft_para_no, final_para_no, settled_on, settled_by, is_active,
            created_by, created_on, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.observation_no, rec.engagement_id, rec.entity_id, rec.observation_status_id,
            rec.severity_id, rec.amount_involved, rec.risk_rating_id, rec.instance_count, rec.memo_date, rec.memo_no,
            rec.draft_para_no, rec.final_para_no, rec.settled_on, rec.settled_by, 'Y',
            rec.created_by, rec.created_on, rec.modified_by, rec.modified_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'AIS_T_AU_OBSERVATION',
            'ID', rec.source_id, 'TBL_OBSERVATION', 'OBSERVATION_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Observation migrated.', rec.created_by
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(t.ID) AS source_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'AIS_T_AU_OBSERVATION'
                   AND m.source_pk_value = TO_CHAR(t.OBSERVATSION_ID)
                   AND m.target_table_name = 'TBL_OBSERVATION'
            ) AS observation_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'OBSERVATION_DETAIL_TYPE'
                   AND lv.lookup_code = 'TEXT'
            ) AS detail_type_id,
            t.HEADINGS AS heading_text,
            t.TEXT AS detail_text,
            t.REMARKS AS remarks,
            NVL(t.ID, 0) AS sequence_no,
            NVL(t.ENTEREDBY, 0) AS created_by,
            NVL(t.ENTEREDDATE, SYSDATE) AS created_on,
            t.LASTUPDATEDBY AS modified_by,
            t.LASTUPDATEDDATE AS modified_on
        FROM ZTBLAIS_PROD.T_AU_OBSERVATION_TEXT t
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_OBSERVATION_TEXT'
               AND m.source_pk_value = TO_CHAR(t.ID)
               AND m.target_table_name = 'TBL_OBSERVATION_DETAIL'
        )
    ) LOOP
        l_new_id := seq_observation_detail.NEXTVAL;

        INSERT INTO tbl_observation_detail (
            observation_detail_id, observation_id, detail_type_id, heading_text, detail_text,
            remarks, sequence_no, is_active, created_by, created_on, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.observation_id, rec.detail_type_id, rec.heading_text, rec.detail_text,
            rec.remarks, rec.sequence_no, 'Y', rec.created_by, rec.created_on, rec.modified_by, rec.modified_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_OBSERVATION_TEXT',
            'ID', rec.source_id, 'TBL_OBSERVATION_DETAIL', 'OBSERVATION_DETAIL_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Observation detail migrated.', rec.created_by
        );
    END LOOP;
END;
/

-------------------------------------------------------------------------------
-- Working paper placeholders from observation text
-------------------------------------------------------------------------------

INSERT INTO tbl_working_paper (
    working_paper_id, module_code, engagement_id, observation_id, user_id,
    working_paper_no, title, detail_text, status_id, is_active, created_by
)
SELECT
    NULL,
    'OBSERVATION',
    o.engagement_id,
    o.observation_id,
    od.created_by,
    'WP-OBS-' || TO_CHAR(od.observation_detail_id),
    NVL(od.heading_text, 'Observation Working Paper'),
    od.detail_text,
    (
        SELECT lv.lookup_value_id
          FROM tbl_lookup_value lv
          JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
         WHERE lt.lookup_type_code = 'SECURITY_STATUS'
           AND lv.lookup_code = 'ACTIVE'
    ),
    'Y',
    NVL(od.created_by, 0)
FROM tbl_observation_detail od
JOIN tbl_observation o
  ON o.observation_id = od.observation_id
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_working_paper wp
     WHERE wp.module_code = 'OBSERVATION'
       AND wp.working_paper_no = 'WP-OBS-' || TO_CHAR(od.observation_detail_id)
);

-------------------------------------------------------------------------------
-- Assignments
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
            TO_CHAR(a.ID) AS source_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'AIS_T_AU_OBSERVATION'
                   AND m.source_pk_value = TO_CHAR(a.OBS_ID)
                   AND m.target_table_name = 'TBL_OBSERVATION'
            ) AS observation_id,
            (
                SELECT u.user_id
                  FROM tbl_user u
                 WHERE u.pp_no = a.PP_NO
            ) AS assignee_user_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'TASK_STATUS'
                   AND lv.lookup_code = CASE WHEN NVL(a.IS_ACTIVE, 'Y') = 'Y' THEN 'OPEN' ELSE 'CANCELLED' END
            ) AS assignment_status_id,
            a.ASSIGNEDBY AS assigned_by,
            SYSDATE AS assigned_on,
            a.REMARKS AS remarks,
            TO_CHAR(a.ACCOUNT_NUMBER) AS account_number,
            a.AC_AMOUNT AS account_amount,
            TO_CHAR(a.LOAN_CASE) AS loan_case_no,
            'LC_AMOUNT=' || NVL(TO_CHAR(a.LC_AMOUNT), 'NULL') || ';COM_ID=' || NVL(TO_CHAR(a.COM_ID), 'NULL') AS detail_text,
            CASE WHEN NVL(a.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            NVL(a.ASSIGNEDBY, 0) AS created_by,
            a.LASTUPDATEDBY AS modified_by,
            a.LASTUPDATEDDATE AS modified_on
        FROM ZTBLAIS_PROD.T_AU_OBSERVATION_RESPONIBILITY_ASSIGNED a
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_OBSERVATION_RESPONIBILITY_ASSIGNED'
               AND m.source_pk_value = TO_CHAR(a.ID)
               AND m.target_table_name = 'TBL_OBSERVATION_ASSIGNMENT'
        )
    ) LOOP
        l_new_id := seq_observation_assignment.NEXTVAL;

        INSERT INTO tbl_observation_assignment (
            observation_assignment_id, observation_id, assignee_user_id, assignment_status_id,
            assigned_by, assigned_on, remarks, account_number, account_amount, loan_case_no,
            detail_text, is_active, created_by, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.observation_id, rec.assignee_user_id, rec.assignment_status_id,
            rec.assigned_by, rec.assigned_on, rec.remarks, rec.account_number, rec.account_amount, rec.loan_case_no,
            rec.detail_text, rec.is_active, rec.created_by, rec.modified_by, rec.modified_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_OBSERVATION_RESPONIBILITY_ASSIGNED',
            'ID', rec.source_id, 'TBL_OBSERVATION_ASSIGNMENT', 'OBSERVATION_ASSIGNMENT_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Observation assignment migrated.', rec.created_by
        );
    END LOOP;
END;
/

-------------------------------------------------------------------------------
-- Responses from auditee and auditor flows
-------------------------------------------------------------------------------

INSERT INTO tbl_observation_response (
    observation_response_id, observation_id, observation_detail_id, response_stage_id,
    response_status_id, response_text, respondent_user_id, responded_on, remarks,
    submitted_flag, response_role_id, detail_text, created_by
)
SELECT
    NULL,
    obs_map.target_pk_value,
    detail_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'RESPONSE_STAGE' AND lv.lookup_code = 'AUDITEE'),
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'OBSERVATION_STATUS' AND lv.lookup_code = 'AUDITEE_RESPONDED'),
    r.REPLY,
    r.REPLIEDBY,
    r.REPLIEDDATE,
    r.REMARKS,
    CASE WHEN NVL(r.SUBMITTED, 'N') = 'Y' THEN 'Y' ELSE 'N' END,
    r.REPLY_ROLE,
    NULL,
    NVL(r.REPLIEDBY, 0)
FROM ZTBLAIS_PROD.T_AU_OBSERVATIONS_AUDITEE_RESPONSE r
JOIN tbl_legacy_key_map obs_map
  ON obs_map.source_table_name = 'AIS_T_AU_OBSERVATION'
 AND obs_map.source_pk_value = TO_CHAR(r.AU_OBS_ID)
 AND obs_map.target_table_name = 'TBL_OBSERVATION'
LEFT JOIN tbl_legacy_key_map detail_map
  ON detail_map.source_table_name = 'T_AU_OBSERVATION_TEXT'
 AND detail_map.source_pk_value = TO_CHAR(r.OBS_TEXT_ID)
 AND detail_map.target_table_name = 'TBL_OBSERVATION_DETAIL';

INSERT INTO tbl_observation_response (
    observation_response_id, observation_id, observation_detail_id, response_stage_id,
    response_status_id, response_text, respondent_user_id, responded_on, submitted_flag,
    response_role_id, recommendation_text, created_by
)
SELECT
    NULL,
    obs_map.target_pk_value,
    detail_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'RESPONSE_STAGE' AND lv.lookup_code = 'AUDITOR'),
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'OBSERVATION_STATUS' AND lv.lookup_code = 'PRE_CONCLUDING'),
    ar.RECOMMENDATION,
    ar.RECO_BY,
    ar.RECO_DATE,
    CASE WHEN NVL(ar.SUBMITTED, 'N') = 'Y' THEN 'Y' ELSE 'N' END,
    NULL,
    ar.RECOMMENDATION,
    NVL(ar.RECO_BY, 0)
FROM ZTBLAIS_PROD.T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION ar
JOIN tbl_legacy_key_map obs_map
  ON obs_map.source_table_name = 'AIS_T_AU_OBSERVATION'
 AND obs_map.source_pk_value = TO_CHAR(ar.AU_OBS_ID)
 AND obs_map.target_table_name = 'TBL_OBSERVATION'
LEFT JOIN tbl_legacy_key_map detail_map
  ON detail_map.source_table_name = 'T_AU_OBSERVATION_TEXT'
 AND detail_map.source_pk_value = TO_CHAR(ar.OBS_TEXT_ID)
 AND detail_map.target_table_name = 'TBL_OBSERVATION_DETAIL';

-------------------------------------------------------------------------------
-- Observation evidence and references
-------------------------------------------------------------------------------

INSERT INTO tbl_observation_evidence (
    observation_evidence_id, observation_id, observation_detail_id, name, description,
    detail_text, evidence_type_id, evidence_status_id, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    od.observation_id,
    detail_map.target_pk_value,
    e.FILE_NAME,
    e.FILE_TYPE,
    e.FILE_DATA,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'EVIDENCE_TYPE' AND lv.lookup_code = 'DOCUMENT'),
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'EVIDENCE_STATUS' AND lv.lookup_code = 'RECEIVED'),
    NVL(e.ENTEREDBY, 0),
    NVL(e.ENTEREDDATE, SYSDATE),
    e.LASTUPDATEDBY,
    e.LASTUPDATEDDATE
FROM ZTBLAIS_PROD.T_AU_OBSERVATIONS_AUDITEE_EVIDENCES e
LEFT JOIN tbl_legacy_key_map detail_map
  ON detail_map.source_table_name = 'T_AU_OBSERVATION_TEXT'
 AND detail_map.source_pk_value = TO_CHAR(e.TEXT_ID)
 AND detail_map.target_table_name = 'TBL_OBSERVATION_DETAIL'
LEFT JOIN tbl_observation_detail od
  ON od.observation_detail_id = detail_map.target_pk_value
WHERE od.observation_id IS NOT NULL;

INSERT INTO tbl_observation_reference (
    observation_reference_id, observation_id, reference_document_version_id, reference_type_id,
    reference_notes, is_primary_reference, heading_text, detail_text, amount_involved,
    risk_rating_id, instance_count, audit_period_label, authorized_by, authorized_on,
    indicator_code, legacy_para_no, reference_status_id, is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    obs_map.target_pk_value,
    ref_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'REFERENCE_TYPE' AND lv.lookup_code = 'MANUAL'),
    r.GIST_OF_PARAS,
    'Y',
    'Legacy Reference',
    r.PARA_TEXT,
    r.AMOUNT,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'RISK_RATING' AND lv.lookup_code = CASE NVL(r.RISK, 2) WHEN 1 THEN 'LOW' WHEN 2 THEN 'MEDIUM' WHEN 3 THEN 'HIGH' ELSE 'CRITICAL' END),
    r.NO_INSTANCES,
    r.AUDIT_PERIOD,
    r.AUTHORIZED_BY,
    r.AUTHORIZED_ON,
    r.IND,
    r.PARA_NO,
    NULL,
    'Y',
    NVL(r.UPDATED_BY, 0),
    NVL(r.UPDATED_ON, SYSDATE),
    r.UPDATED_BY,
    r.UPDATED_ON
FROM ZTBLAIS_PROD.T_AU_OBSERVATION_UPDATED_REFERENCE r
JOIN tbl_legacy_key_map obs_map
  ON obs_map.source_table_name = 'AIS_T_AU_OBSERVATION'
 AND obs_map.source_pk_value = TO_CHAR(r.N_PARA_ID)
 AND obs_map.target_table_name = 'TBL_OBSERVATION'
LEFT JOIN tbl_legacy_key_map ref_map
  ON ref_map.source_table_name = 'T_MANUAL_INDEX'
 AND ref_map.source_pk_value = TO_CHAR(r.ANNEXURE_REF_ID)
 AND ref_map.target_table_name = 'TBL_REFERENCE_DOCUMENT_VERSION'
WHERE ref_map.target_pk_value IS NOT NULL;

-------------------------------------------------------------------------------
-- Observation DSA
-------------------------------------------------------------------------------

INSERT INTO tbl_observation_dsa (
    observation_dsa_id, observation_id, dsa_status_id, description, detail_text,
    approved_by, approved_on, is_active, created_by, created_on
)
SELECT
    NULL,
    obs_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'OBSERVATION_STATUS' AND lv.lookup_code = 'PRE_CONCLUDING'),
    ds.DESCRIPTION,
    dt.DSA_BODY,
    ga.ENTERED_BY,
    ga.ENTERED_ON,
    'Y',
    NVL(ga.ENTERED_BY, 0),
    NVL(ga.ENTERED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_AU_DSA_STATUS ds
LEFT JOIN ZTBLAIS_PROD.T_AU_DSA_TEXT dt
  ON dt.DSA_ID = ds.ID
LEFT JOIN ZTBLAIS_PROD.T_AU_DSA_GUIDELINES_ADDED ga
  ON ga.DSA_ID = ds.ID
LEFT JOIN tbl_legacy_key_map obs_map
  ON obs_map.source_table_name = 'AIS_T_AU_OBSERVATION'
 AND obs_map.source_pk_value = TO_CHAR(ds.ID)
 AND obs_map.target_table_name = 'TBL_OBSERVATION'
WHERE obs_map.target_pk_value IS NOT NULL;

-------------------------------------------------------------------------------
-- Observation migration issues
-------------------------------------------------------------------------------

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'AIS_T_AU_OBSERVATION',
    'ID',
    'MULTIPLE_ROWS',
    'V_CAT_ID/V_CAT_NATURE_ID/CHECKLISTDETAIL_ID/ANNEX',
    'TBL_OBSERVATION',
    'MANUAL_REVIEW',
    'Legacy category, annex, and checklist-detail fields need explicit execution-model normalization before final migration freeze.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'AIS_T_AU_OBSERVATION'
       AND source_column_name = 'V_CAT_ID/V_CAT_NATURE_ID/CHECKLISTDETAIL_ID/ANNEX'
);
