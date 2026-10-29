/*
  IAS_ZTBL Phase 3 planning and engagement migration

  Scope
  - audit plans
  - engagements
  - engagement teams and members
  - criteria/history placeholders for controlled follow-up
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Audit plans, engagements, and teams
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
            TO_CHAR(p.ID) AS source_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AU_PERIOD'
                   AND m.source_pk_value = TO_CHAR(p.AUDITPERIODID)
                   AND m.target_table_name = 'TBL_AUDIT_PERIOD'
            ) AS audit_period_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(p.ENTITY_ID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS entity_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENT_TYPES'
                   AND m.source_pk_value = TO_CHAR(p.ENTITY_TYPEID)
                   AND m.target_table_name = 'TBL_ENTITY_TYPE'
            ) AS entity_type_id,
            'PLAN_' || TO_CHAR(p.ID) AS plan_code,
            'Legacy Plan ' || TO_CHAR(p.ID) AS plan_name,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'PLAN_STATUS'
                   AND lv.lookup_code = CASE WHEN NVL(p.STATUS, 0) = 0 THEN 'DRAFT' WHEN p.STATUS IN (1, 2) THEN 'SUBMITTED' ELSE 'APPROVED' END
            ) AS plan_status_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'RISK_RATING'
                   AND lv.lookup_code = CASE NVL(p.AUDITEE_RISK, 2) WHEN 1 THEN 'LOW' WHEN 2 THEN 'MEDIUM' WHEN 3 THEN 'HIGH' ELSE 'CRITICAL' END
            ) AS risk_rating_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'SIZE_BAND'
                   AND lv.lookup_code = CASE NVL(p.AUDITEE_SIZE, 2) WHEN 1 THEN 'SMALL' WHEN 2 THEN 'MEDIUM' ELSE 'LARGE' END
            ) AS size_band_id,
            p.NATURE_ID AS nature_id,
            TO_CHAR(p.COST_CENTER) AS cost_center_code,
            p.NO_OF_DAYS AS planned_days,
            'F_ID=' || NVL(TO_CHAR(p.F_ID), 'NULL') || ';FR_ID=' || NVL(TO_CHAR(p.FR_ID), 'NULL') || ';ROW_ID=' || NVL(TO_CHAR(p.ROW_ID), 'NULL') AS legacy_row_reference
        FROM ZTBLAIS_PROD.T_AU_PLAN p
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_PLAN'
               AND m.source_pk_value = TO_CHAR(p.ID)
               AND m.target_table_name = 'TBL_AUDIT_PLAN'
        )
    ) LOOP
        l_new_id := seq_audit_plan.NEXTVAL;

        INSERT INTO tbl_audit_plan (
            audit_plan_id, audit_period_id, entity_id, entity_type_id, plan_code, plan_name,
            plan_status_id, risk_rating_id, size_band_id, nature_id, cost_center_code,
            planned_days, legacy_row_reference, is_active, created_by
        )
        VALUES (
            l_new_id, rec.audit_period_id, rec.entity_id, rec.entity_type_id, rec.plan_code, rec.plan_name,
            rec.plan_status_id, rec.risk_rating_id, rec.size_band_id, rec.nature_id, rec.cost_center_code,
            rec.planned_days, rec.legacy_row_reference, 'Y', 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_PLAN',
            'ID', rec.source_id, 'TBL_AUDIT_PLAN', 'AUDIT_PLAN_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Audit plan migrated.', 0
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(e.ENG_ID) AS source_id,
            'ENG_' || TO_CHAR(e.ENG_ID) AS engagement_no,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AU_PERIOD'
                   AND m.source_pk_value = TO_CHAR(e.PERIOD_ID)
                   AND m.target_table_name = 'TBL_AUDIT_PERIOD'
            ) AS audit_period_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(e.ENTITY_ID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS entity_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'ENGAGEMENT_TYPE'
                   AND lv.lookup_code = 'FIELD_AUDIT'
            ) AS engagement_type_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AU_PLAN'
                   AND m.source_pk_value = TO_CHAR(e.PLAN_ID)
                   AND m.target_table_name = 'TBL_AUDIT_PLAN'
            ) AS audit_plan_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'ENGAGEMENT_STATUS'
                   AND lv.lookup_code = CASE WHEN NVL(e.STATUS, 0) = 0 THEN 'PLANNED' WHEN e.STATUS IN (1, 2) THEN 'IN_FIELD' ELSE 'UNDER_REVIEW' END
            ) AS engagement_status_id,
            e.TEAM_NAME AS team_name,
            e.AUDIT_STARTDATE AS audit_start_on,
            e.AUDIT_ENDDATE AS audit_end_on,
            e.OPERATION_STARTDATE AS field_start_on,
            e.OPERATION_ENDDATE AS field_end_on,
            e.TRAVEL_DAY AS travel_days,
            e.DISCUSSION_DAY AS discussion_days,
            'Legacy branch code=' || e.BRANCH_CODE AS scope_summary,
            e.BRANCH_CODE AS branch_code,
            'Y' AS is_active,
            NVL(e.CREATEDBY, 0) AS created_by,
            NVL(e.CREATED_ON, SYSDATE) AS created_on,
            e.LASTUPDATEDBY AS modified_by,
            e.LASTUPDATEDDATE AS modified_on
        FROM ZTBLAIS_PROD.T_AU_PLAN_ENG e
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_PLAN_ENG'
               AND m.source_pk_value = TO_CHAR(e.ENG_ID)
               AND m.target_table_name = 'TBL_ENGAGEMENT'
        )
    ) LOOP
        l_new_id := seq_engagement.NEXTVAL;

        INSERT INTO tbl_engagement (
            engagement_id, engagement_no, audit_period_id, entity_id, engagement_type_id,
            plan_criteria_id, engagement_status_id, audit_plan_id, team_name,
            audit_start_on, audit_end_on, field_start_on, field_end_on,
            travel_days, discussion_days, scope_summary, branch_code, is_active, created_by, created_on, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.engagement_no, rec.audit_period_id, rec.entity_id, rec.engagement_type_id,
            NULL, rec.engagement_status_id, rec.audit_plan_id, rec.team_name,
            rec.audit_start_on, rec.audit_end_on, rec.field_start_on, rec.field_end_on,
            rec.travel_days, rec.discussion_days, rec.scope_summary, rec.branch_code, rec.is_active, rec.created_by, rec.created_on, rec.modified_by, rec.modified_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_PLAN_ENG',
            'ENG_ID', rec.source_id, 'TBL_ENGAGEMENT', 'ENGAGEMENT_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Engagement migrated.', rec.created_by
        );
    END LOOP;

    FOR rec IN (
        SELECT DISTINCT
            TO_CHAR(NVL(e.TEAM_ID, e.ENG_ID)) AS source_id,
            eng_map.target_pk_value AS engagement_id,
            NVL(e.TEAM_NAME, 'Legacy Team ' || TO_CHAR(e.ENG_ID)) AS team_name,
            (
                SELECT MIN(u.user_id)
                  FROM tbl_user u
                  JOIN ZTBLAIS_PROD.T_AU_TEAM_MEMBERS tm
                    ON tm.ENG_ID = e.ENG_ID
                   AND tm.ISTEAMLEAD = 'Y'
                   AND u.pp_no = tm.MEMBER_PPNO
            ) AS lead_user_id,
            'Legacy TEAM_ID=' || NVL(TO_CHAR(e.TEAM_ID), 'NULL') AS detail_text
        FROM ZTBLAIS_PROD.T_AU_PLAN_ENG e
        JOIN tbl_legacy_key_map eng_map
          ON eng_map.source_table_name = 'T_AU_PLAN_ENG'
         AND eng_map.source_pk_value = TO_CHAR(e.ENG_ID)
         AND eng_map.target_table_name = 'TBL_ENGAGEMENT'
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_PLAN_ENG'
               AND m.source_pk_name = 'TEAM_ID'
               AND m.source_pk_value = TO_CHAR(NVL(e.TEAM_ID, e.ENG_ID))
               AND m.target_table_name = 'TBL_ENGAGEMENT_TEAM'
        )
    ) LOOP
        l_new_id := seq_engagement_team.NEXTVAL;

        INSERT INTO tbl_engagement_team (
            engagement_team_id, engagement_id, team_name, lead_user_id, detail_text, is_active, created_by
        )
        VALUES (
            l_new_id, rec.engagement_id, rec.team_name, rec.lead_user_id, rec.detail_text, 'Y', 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_PLAN_ENG',
            'TEAM_ID', rec.source_id, 'TBL_ENGAGEMENT_TEAM', 'ENGAGEMENT_TEAM_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Engagement team migrated.', 0
        );
    END LOOP;
END;
/

-------------------------------------------------------------------------------
-- Engagement members
-------------------------------------------------------------------------------

INSERT INTO tbl_engagement_member (
    engagement_member_id, engagement_id, user_id, member_role_id, member_sequence_no,
    is_team_lead, assigned_on, is_active, created_by
)
SELECT
    NULL,
    eng_map.target_pk_value,
    u.user_id,
    (
        SELECT MIN(ur.role_id)
          FROM tbl_user_role ur
         WHERE ur.user_id = u.user_id
    ) AS member_role_id,
    NVL(tm.ID, NVL(tm.T_CODE, ROWNUM)),
    CASE WHEN NVL(tm.ISTEAMLEAD, 'N') = 'Y' THEN 'Y' ELSE 'N' END,
    SYSDATE,
    CASE WHEN NVL(tm.STATUS, 'Y') IN ('Y', 'ACTIVE') THEN 'Y' ELSE 'N' END,
    0
FROM ZTBLAIS_PROD.T_AU_TEAM_MEMBERS tm
JOIN tbl_legacy_key_map eng_map
  ON eng_map.source_table_name = 'T_AU_PLAN_ENG'
 AND eng_map.source_pk_value = TO_CHAR(tm.ENG_ID)
 AND eng_map.target_table_name = 'TBL_ENGAGEMENT'
JOIN tbl_user u
  ON u.pp_no = tm.MEMBER_PPNO
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_engagement_member x
     WHERE x.engagement_id = eng_map.target_pk_value
       AND x.user_id = u.user_id
);

-------------------------------------------------------------------------------
-- Criteria/history review registration
-------------------------------------------------------------------------------

INSERT INTO tbl_engagement_criteria_history (
    engagement_criteria_history_id, engagement_id, status_id, detail_text, changed_by, created_by
)
SELECT
    NULL,
    eng_map.target_pk_value,
    (
        SELECT lv.lookup_value_id
          FROM tbl_lookup_value lv
          JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
         WHERE lt.lookup_type_code = 'PLAN_STATUS'
           AND lv.lookup_code = CASE WHEN NVL(p.STATUS, 0) = 0 THEN 'DRAFT' WHEN p.STATUS IN (1, 2) THEN 'SUBMITTED' ELSE 'APPROVED' END
    ),
    'Legacy criteria_id=' || NVL(TO_CHAR(p.CRITERIA_ID), 'NULL') || ' needs explicit plan_criteria normalization.',
    0,
    0
FROM ZTBLAIS_PROD.T_AU_PLAN p
JOIN ZTBLAIS_PROD.T_AU_PLAN_ENG e
  ON e.PLAN_ID = p.ID
JOIN tbl_legacy_key_map eng_map
  ON eng_map.source_table_name = 'T_AU_PLAN_ENG'
 AND eng_map.source_pk_value = TO_CHAR(e.ENG_ID)
 AND eng_map.target_table_name = 'TBL_ENGAGEMENT'
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_engagement_criteria_history x
     WHERE x.engagement_id = eng_map.target_pk_value
);

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_PLAN',
    'ID',
    'MULTIPLE_ROWS',
    'CRITERIA_ID/F_ID/FR_ID/ROW_ID/AUDITEDBY',
    'TBL_AUDIT_PLAN',
    'MANUAL_REVIEW',
    'Legacy plan criteria and source references require dedicated planning normalization before production migration.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'T_AU_PLAN'
       AND source_column_name = 'CRITERIA_ID/F_ID/FR_ID/ROW_ID/AUDITEDBY'
);
