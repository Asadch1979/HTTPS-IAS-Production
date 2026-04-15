/*
  IAS_ZTBL Phase 3 compliance and para migration

  Scope
  - legacy para cases and para text/history
  - current post-compliance cases and evidence/history
  - para settlement and assignment evidence
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Legacy para cases
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
            TO_CHAR(p.PARA_ID) AS source_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(p.ENTITY_ID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS entity_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'RISK_RATING'
                   AND lv.lookup_code = CASE NVL(p.RISK_ID, 2) WHEN 1 THEN 'LOW' WHEN 2 THEN 'MEDIUM' WHEN 3 THEN 'HIGH' ELSE 'CRITICAL' END
            ) AS risk_rating_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'PARA_CASE_STATUS'
                   AND lv.lookup_code = CASE WHEN NVL(p.STATUS, 1) = 0 THEN 'ARCHIVED' ELSE 'ACTIVE' END
            ) AS case_status_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'PARA_STATUS'
                   AND lv.lookup_code = CASE WHEN NVL(p.PARA_STATUS, 0) IN (0, 1) THEN 'OPEN' WHEN p.PARA_STATUS IN (2, 3) THEN 'UNDER_COMPLIANCE' ELSE 'SETTLED' END
            ) AS para_status_id,
            p.PARA_NO AS para_no,
            p.PARA_NO AS legacy_reference_no,
            p.ENTITY_NAME AS name,
            p.GIST_OF_PARAS AS gist_text,
            p.PARA_TEXT AS detail_text,
            NULL AS instance_count,
            p.SETTELED_BY AS settled_by,
            p.SETTELED_ON AS settled_on,
            NVL(p.ENTERED_BY, 0) AS created_by,
            NVL(p.ENTERED_ON, SYSDATE) AS created_on,
            p.UPDATED_BY AS modified_by,
            p.UPDATED_ON AS modified_on
        FROM ZTBLAIS_PROD.T_AU_OBSERVATION_OLD_CAD_PARAS p
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_OBSERVATION_OLD_CAD_PARAS'
               AND m.source_pk_value = TO_CHAR(p.PARA_ID)
               AND m.target_table_name = 'TBL_PARA_CASE'
        )
    ) LOOP
        l_new_id := seq_para_case.NEXTVAL;

        INSERT INTO tbl_para_case (
            para_case_id, entity_id, risk_rating_id, case_status_id, para_status_id,
            para_no, legacy_reference_no, name, gist_text, detail_text, instance_count,
            settled_by, settled_on, is_active, created_by, created_on, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.entity_id, rec.risk_rating_id, rec.case_status_id, rec.para_status_id,
            rec.para_no, rec.legacy_reference_no, rec.name, rec.gist_text, rec.detail_text, rec.instance_count,
            rec.settled_by, rec.settled_on, 'Y', rec.created_by, rec.created_on, rec.modified_by, rec.modified_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_OBSERVATION_OLD_CAD_PARAS',
            'PARA_ID', rec.source_id, 'TBL_PARA_CASE', 'PARA_CASE_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Legacy para migrated.', rec.created_by
        );
    END LOOP;
END;
/

INSERT INTO tbl_para_case_text (
    para_case_text_id, para_case_id, heading_text, detail_text,
    sequence_no, version_no, is_current_version, created_by, created_on
)
SELECT
    NULL,
    m.target_pk_value,
    'Legacy Para',
    p.PARA_TEXT,
    1,
    1,
    'Y',
    NVL(p.ENTERED_BY, 0),
    NVL(p.ENTERED_ON, SYSDATE)
FROM ZTBLAIS_PROD.T_AU_OBSERVATION_OLD_CAD_PARAS p
JOIN tbl_legacy_key_map m
  ON m.source_table_name = 'T_AU_OBSERVATION_OLD_CAD_PARAS'
 AND m.source_pk_value = TO_CHAR(p.PARA_ID)
 AND m.target_table_name = 'TBL_PARA_CASE';

-------------------------------------------------------------------------------
-- Legacy para assignments and status history
-------------------------------------------------------------------------------

INSERT INTO tbl_para_assignment (
    para_assignment_id, para_case_id, assignee_user_id, assignment_status_id, remarks, is_active, created_by
)
SELECT
    NULL,
    pc.para_case_id,
    u.user_id,
    (
        SELECT lv.lookup_value_id
          FROM tbl_lookup_value lv
          JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
         WHERE lt.lookup_type_code = 'TASK_STATUS'
           AND lv.lookup_code = CASE WHEN NVL(a.STATUS, 'Y') IN ('Y', 'ACTIVE') THEN 'OPEN' ELSE 'CANCELLED' END
    ),
    'DEL_STATUS=' || NVL(a.DEL_STATUS, 'N'),
    'Y',
    0
FROM ZTBLAIS_PROD.T_AU_OBSERVATION_OLD_PARAS_RESPONIBILITY_ASSIGNED a
JOIN tbl_para_case pc
  ON pc.legacy_reference_no = a.REF_P
LEFT JOIN tbl_user u
  ON u.pp_no = a.PP_NO;

INSERT INTO tbl_para_status_history (
    para_status_history_id, para_case_id, to_status_id, detail_text, changed_by, changed_on, is_active, created_by
)
SELECT
    NULL,
    pc.para_case_id,
    (
        SELECT lv.lookup_value_id
          FROM tbl_lookup_value lv
          JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
         WHERE lt.lookup_type_code = 'PARA_STATUS'
           AND lv.lookup_code = CASE WHEN NVL(h.C_STATUS, 0) IN (0,1) THEN 'OPEN' WHEN h.C_STATUS IN (2,3) THEN 'UNDER_COMPLIANCE' ELSE 'SETTLED' END
    ),
    'Stage=' || NVL(h.STAGE, 'N/A') || ';Remarks=' || NVL(h.REMARKS, 'N/A'),
    h.ATTENDED_BY,
    h.ATTENDED_ON,
    'Y',
    NVL(h.ATTENDED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_OLD_PARAS_POST_COMPLIANCE_HISTORY h
JOIN tbl_para_case pc
  ON pc.legacy_reference_no = h.REF_P;

-------------------------------------------------------------------------------
-- Compliance cases
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
            TO_CHAR(c.COM_ID) AS source_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'AIS_T_AU_OBSERVATION'
                   AND m.source_pk_value = TO_CHAR(c.NEW_PARA_ID)
                   AND m.target_table_name = 'TBL_OBSERVATION'
            ) AS observation_id,
            c.COM_CYCLE AS compliance_cycle_no,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'COMPLIANCE_STATUS'
                   AND lv.lookup_code = CASE NVL(c.COM_STATUS, 0) WHEN 0 THEN 'PENDING' WHEN 1 THEN 'IN_PROGRESS' WHEN 2 THEN 'PARTIALLY_COMPLIED' WHEN 3 THEN 'FULLY_COMPLIED' ELSE 'SETTLED' END
            ) AS compliance_status_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'COMPLIANCE_STAGE'
                   AND lv.lookup_code = CASE NVL(c.COM_STAGE, 0) WHEN 0 THEN 'OPEN' WHEN 1 THEN 'SUBMITTED' WHEN 2 THEN 'UNDER_REVIEW' ELSE 'APPROVED' END
            ) AS compliance_stage_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(c.ENTITY_ID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS responsible_entity_id,
            NVL(c.PARA_ADDED_ON, SYSDATE) AS opened_on,
            c.SETTELED_ON AS closed_on,
            CASE WHEN REGEXP_LIKE(NVL(c.AMOUNT, '0'), '^[0-9]+(\.[0-9]+)?$') THEN TO_NUMBER(c.AMOUNT) END AS amount_involved,
            c.GIST_OF_PARAS AS gist_text,
            c.NO_OF_INSTANCES AS instance_count,
            c.PARA_NO AS legacy_para_no,
            c.AUDIT_PERIOD AS audit_period_label,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(c.AUDITED_BY)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS audited_by_entity_id,
            c.BR_RESPONSE_BY AS branch_response_by,
            c.BR_RESPONSE_ON AS branch_response_on,
            c.CAU_ASSIGNED_BY AS cau_assigned_by,
            c.CAU_ASSIGNED_ON AS cau_assigned_on,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'COMPLIANCE_STATUS'
                   AND lv.lookup_code = CASE NVL(c.CAU_STATUS, 0) WHEN 0 THEN 'PENDING' WHEN 1 THEN 'IN_PROGRESS' ELSE 'PARTIALLY_COMPLIED' END
            ) AS cau_status_id,
            c.IND AS indicator_code,
            c.PARA_ADDED_ON AS para_added_on,
            CASE WHEN NVL(c.REFERENCE_REVIEWED, 0) IN (1, '1') THEN 'Y' ELSE 'N' END AS reference_reviewed_flag
        FROM ZTBLAIS_PROD.AIS_T_AU_POST_COMPLIANCE c
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'AIS_T_AU_POST_COMPLIANCE'
               AND m.source_pk_value = TO_CHAR(c.COM_ID)
               AND m.target_table_name = 'TBL_COMPLIANCE_CASE'
        )
    ) LOOP
        l_new_id := seq_compliance_case.NEXTVAL;

        INSERT INTO tbl_compliance_case (
            compliance_case_id, observation_id, compliance_cycle_no, compliance_status_id, compliance_stage_id,
            responsible_entity_id, opened_on, closed_on, amount_involved, gist_text, instance_count,
            legacy_para_no, audit_period_label, audited_by_entity_id, branch_response_by, branch_response_on,
            cau_assigned_by, cau_assigned_on, cau_status_id, indicator_code, para_added_on, reference_reviewed_flag,
            is_active, created_by
        )
        VALUES (
            l_new_id, rec.observation_id, rec.compliance_cycle_no, rec.compliance_status_id, rec.compliance_stage_id,
            rec.responsible_entity_id, rec.opened_on, rec.closed_on, rec.amount_involved, rec.gist_text, rec.instance_count,
            rec.legacy_para_no, rec.audit_period_label, rec.audited_by_entity_id, rec.branch_response_by, rec.branch_response_on,
            rec.cau_assigned_by, rec.cau_assigned_on, rec.cau_status_id, rec.indicator_code, rec.para_added_on, rec.reference_reviewed_flag,
            'Y', 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'AIS_T_AU_POST_COMPLIANCE',
            'COM_ID', rec.source_id, 'TBL_COMPLIANCE_CASE', 'COMPLIANCE_CASE_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Compliance case migrated.', 0
        );
    END LOOP;
END;
/

-------------------------------------------------------------------------------
-- Compliance evidence and history
-------------------------------------------------------------------------------

INSERT INTO tbl_compliance_evidence (
    compliance_evidence_id, compliance_case_id, name, description, detail_text,
    evidence_type_id, evidence_status_id, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    comp_map.target_pk_value,
    e.FILE_NAME,
    e.DESCRIPTION,
    e.FILE_DATA,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'EVIDENCE_TYPE' AND lv.lookup_code = 'DOCUMENT'),
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'EVIDENCE_STATUS' AND lv.lookup_code = 'RECEIVED'),
    NVL(e.ENTEREDBY, 0),
    NVL(e.ENTEREDDATE, SYSDATE),
    e.LASTUPDATEDBY,
    e.LASTUPDATEDDATE
FROM ZTBLAIS_PROD.AIS_T_AU_POST_COMPLIANCE_EVIDENCE e
JOIN tbl_legacy_key_map comp_map
  ON comp_map.source_table_name = 'AIS_T_AU_POST_COMPLIANCE'
 AND comp_map.source_pk_value = TO_CHAR(e.COMP_ID)
 AND comp_map.target_table_name = 'TBL_COMPLIANCE_CASE';

INSERT INTO tbl_compliance_case_history (
    compliance_case_history_id, compliance_case_id, compliance_stage_id, comment_text,
    user_id, commented_on, detail_text, is_active, created_by
)
SELECT
    NULL,
    comp_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'COMPLIANCE_STAGE' AND lv.lookup_code = CASE NVL(h.COM_STAGE, 0) WHEN 0 THEN 'OPEN' WHEN 1 THEN 'SUBMITTED' WHEN 2 THEN 'UNDER_REVIEW' ELSE 'APPROVED' END),
    h.COMMENTS,
    h.COMMENT_BY_PPNO,
    h.COMMENT_ON,
    'Role=' || NVL(h.COMMENT_BY_ROLE, 'N/A') || ';Flow=' || NVL(h.COM_FLOW, 'N/A'),
    'Y',
    NVL(h.COMMENT_BY_PPNO, 0)
FROM ZTBLAIS_PROD.AIS_T_AU_POST_COMPLIANCE_HISTORY h
JOIN tbl_legacy_key_map comp_map
  ON comp_map.source_table_name = 'AIS_T_AU_POST_COMPLIANCE'
 AND comp_map.source_pk_value = TO_CHAR(h.COM_ID)
 AND comp_map.target_table_name = 'TBL_COMPLIANCE_CASE';

-------------------------------------------------------------------------------
-- Settlement history
-------------------------------------------------------------------------------

INSERT INTO tbl_para_settlement_history (
    para_settlement_history_id, para_case_id, compliance_case_id, entity_id,
    settlement_status_id, settlement_stage_id, settlement_remarks,
    settled_by, settled_on, approved_by, approved_on, is_active, created_by
)
SELECT
    NULL,
    pc.para_case_id,
    (
        SELECT cc.compliance_case_id
          FROM tbl_compliance_case cc
         WHERE cc.observation_id = (
             SELECT om.target_pk_value
               FROM tbl_legacy_key_map om
              WHERE om.source_table_name = 'AIS_T_AU_OBSERVATION'
                AND om.source_pk_value = TO_CHAR(s.AU_OBS_ID)
                AND om.target_table_name = 'TBL_OBSERVATION'
         )
    ) AS compliance_case_id,
    ent_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'SETTLEMENT_STATUS' AND lv.lookup_code = 'APPROVED'),
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'SETTLEMENT_STAGE' AND lv.lookup_code = 'CAU'),
    s.REMARKS,
    s.SETTLED_BY,
    s.SETTLED_ON,
    s.REVIEWED_BY,
    s.REVIEWED_ON,
    'Y',
    NVL(s.SETTLED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY s
LEFT JOIN tbl_para_case pc
  ON pc.legacy_reference_no = s.REF_P
LEFT JOIN tbl_legacy_key_map ent_map
  ON ent_map.source_table_name = 'T_AUDITEE_ENTITIES'
 AND ent_map.source_pk_value = TO_CHAR(s.ENTITY_ID)
 AND ent_map.target_table_name = 'TBL_ENTITY'
WHERE pc.para_case_id IS NOT NULL;

-------------------------------------------------------------------------------
-- Compliance and para migration issues
-------------------------------------------------------------------------------

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'AIS_T_AU_POST_COMPLIANCE',
    'COM_ID',
    'MULTIPLE_ROWS',
    'CAU_ASSIGNED_ENT_ID/ANNEX/ANNEX_REF_ID/PARA_STATUS',
    'TBL_COMPLIANCE_CASE',
    'MANUAL_REVIEW',
    'Legacy annex/reference linkage and some CAU assignment fields need final business validation before cutover.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'AIS_T_AU_POST_COMPLIANCE'
       AND source_column_name = 'CAU_ASSIGNED_ENT_ID/ANNEX/ANNEX_REF_ID/PARA_STATUS'
);
