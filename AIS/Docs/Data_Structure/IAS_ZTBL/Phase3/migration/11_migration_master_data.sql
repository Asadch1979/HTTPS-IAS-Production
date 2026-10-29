/*
  IAS_ZTBL Phase 3 master-data migration

  Scope
  - entity types
  - entities
  - audit periods
  - manual/reference documents
  - manual index rows into reference document versions
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Entity types, entities, periods, and reference documents
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
            TO_CHAR(s.AUTID) AS source_id,
            UPPER(NVL(TRIM(s.ENTITYCODE), 'ETYPE_' || TO_CHAR(s.AUTID))) AS entity_type_code,
            NVL(TRIM(s.ENTITYTYPEDESC), 'Legacy Entity Type ' || TO_CHAR(s.AUTID)) AS entity_type_name,
            CASE WHEN NVL(s.AUDITABLE, 'N') = 'Y' THEN 'Y' ELSE 'N' END AS is_auditable,
            CASE WHEN NVL(TO_CHAR(s.REPORTING), '0') IN ('1', 'Y') THEN 'Y' ELSE 'N' END AS is_reporting_unit,
            CASE WHEN NVL(TO_CHAR(s.CONTROLLING), '0') IN ('1', 'Y') THEN 'Y' ELSE 'N' END AS is_controlling_unit,
            'Legacy audit type=' || NVL(s.AUDIT_TYPE, 'N/A') AS description,
            'Y' AS is_active
        FROM ZTBLAIS_PROD.T_AUDITEE_ENT_TYPES s
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AUDITEE_ENT_TYPES'
               AND m.source_pk_value = TO_CHAR(s.AUTID)
               AND m.target_table_name = 'TBL_ENTITY_TYPE'
        )
    ) LOOP
        l_new_id := seq_entity_type.NEXTVAL;

        INSERT INTO tbl_entity_type (
            entity_type_id, entity_type_code, entity_type_name, is_auditable,
            is_reporting_unit, is_controlling_unit, description, is_active, created_by
        )
        VALUES (
            l_new_id, rec.entity_type_code, rec.entity_type_name, rec.is_auditable,
            rec.is_reporting_unit, rec.is_controlling_unit, rec.description, rec.is_active, 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AUDITEE_ENT_TYPES',
            'AUTID', rec.source_id, 'TBL_ENTITY_TYPE', 'ENTITY_TYPE_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Entity type migrated.', 0
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(s.ENTITY_ID) AS source_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENT_TYPES'
                   AND m.source_pk_value = TO_CHAR(s.TYPE_ID)
                   AND m.target_table_name = 'TBL_ENTITY_TYPE'
            ) AS entity_type_id,
            TO_CHAR(s.CODE) AS entity_code,
            NVL(TRIM(s.NAME), 'Legacy Entity ' || TO_CHAR(s.ENTITY_ID)) AS entity_name,
            s.DESCRIPTION AS entity_description,
            TO_CHAR(s.COST_CENTER) AS cost_center_code,
            s.ADDRESS AS address_line,
            s.TELEPHONE AS telephone_no,
            s.EMAIL_ADDRESS AS email_address,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'RISK_RATING'
                   AND lv.lookup_code = CASE NVL(s.RISK_ID, 2) WHEN 1 THEN 'LOW' WHEN 2 THEN 'MEDIUM' WHEN 3 THEN 'HIGH' ELSE 'CRITICAL' END
            ) AS risk_rating_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'SIZE_BAND'
                   AND lv.lookup_code = CASE NVL(s.SIZE_ID, 2) WHEN 1 THEN 'SMALL' WHEN 2 THEN 'MEDIUM' ELSE 'LARGE' END
            ) AS size_band_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'ENTITY_STATUS'
                   AND lv.lookup_code = CASE WHEN NVL(s.ACTIVE, 'Y') = 'Y' THEN 'ACTIVE' ELSE 'INACTIVE' END
            ) AS entity_status_id,
            CASE WHEN NVL(s.AUDITABLE, 'N') IN ('Y', 'YES', '1') THEN 'Y' ELSE 'N' END AS is_auditable,
            CASE WHEN NVL(s.ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            s.CODE AS legacy_numeric_code,
            s.COMPLICE_BY AS compliance_by_text,
            CASE WHEN NVL(s.AUDITOR, 'N') = 'Y' THEN 'Y' ELSE 'N' END AS is_auditor_flag,
            CASE WHEN NVL(s.IAD, 'N') = 'Y' THEN 'Y' ELSE 'N' END AS is_iad_flag
        FROM ZTBLAIS_PROD.T_AUDITEE_ENTITIES s
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
               AND m.source_pk_value = TO_CHAR(s.ENTITY_ID)
               AND m.target_table_name = 'TBL_ENTITY'
        )
    ) LOOP
        l_new_id := seq_entity.NEXTVAL;

        INSERT INTO tbl_entity (
            entity_id, entity_type_id, entity_code, entity_name, entity_description,
            cost_center_code, address_line, telephone_no, email_address,
            risk_rating_id, size_band_id, entity_status_id, is_auditable, is_active,
            legacy_numeric_code, compliance_by_text, is_auditor_flag, is_iad_flag, created_by
        )
        VALUES (
            l_new_id, rec.entity_type_id, rec.entity_code, rec.entity_name, rec.entity_description,
            rec.cost_center_code, rec.address_line, rec.telephone_no, rec.email_address,
            rec.risk_rating_id, rec.size_band_id, rec.entity_status_id, rec.is_auditable, rec.is_active,
            rec.legacy_numeric_code, rec.compliance_by_text, rec.is_auditor_flag, rec.is_iad_flag, 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AUDITEE_ENTITIES',
            'ENTITY_ID', rec.source_id, 'TBL_ENTITY', 'ENTITY_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Entity migrated.', 0
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(s.AUDITPERIODID) AS source_id,
            'PERIOD_' || TO_CHAR(s.AUDITPERIODID) AS period_code,
            NVL(TRIM(s.DESCRIPTION), 'Legacy Period ' || TO_CHAR(s.AUDITPERIODID)) AS period_name,
            s.START_DATE AS start_date,
            s.END_DATE AS end_date,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'PERIOD_STATUS'
                   AND lv.lookup_code = CASE WHEN NVL(s.STATUS_ID, 1) = 1 THEN 'OPEN' ELSE 'CLOSED' END
            ) AS period_status_id
        FROM ZTBLAIS_PROD.T_AU_PERIOD s
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_PERIOD'
               AND m.source_pk_value = TO_CHAR(s.AUDITPERIODID)
               AND m.target_table_name = 'TBL_AUDIT_PERIOD'
        )
    ) LOOP
        l_new_id := seq_audit_period.NEXTVAL;

        INSERT INTO tbl_audit_period (
            audit_period_id, period_code, period_name, start_date, end_date, period_status_id,
            is_current_period, is_active, created_by
        )
        VALUES (
            l_new_id, rec.period_code, rec.period_name, rec.start_date, rec.end_date, rec.period_status_id,
            'N', 'Y', 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_PERIOD',
            'AUDITPERIODID', rec.source_id, 'TBL_AUDIT_PERIOD', 'AUDIT_PERIOD_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Audit period migrated.', 0
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(s.MANUAL_ID) AS source_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'DOCUMENT_TYPE'
                   AND lv.lookup_code = 'MANUAL'
            ) AS reference_type_id,
            'MANUAL_' || TO_CHAR(s.MANUAL_ID) AS document_code,
            NVL(TRIM(s.MANUAL_NAME), 'Legacy Manual ' || TO_CHAR(s.MANUAL_ID)) || NVL2(s.VOLUME_NAME, ' - ' || s.VOLUME_NAME, '') AS document_title,
            CASE WHEN NVL(s.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            NVL(s.CREATED_BY, 0) AS created_by,
            NVL(s.CREATED_DATE, SYSDATE) AS created_on
        FROM ZTBLAIS_PROD.T_MANUAL_MASTER s
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_MANUAL_MASTER'
               AND m.source_pk_value = TO_CHAR(s.MANUAL_ID)
               AND m.target_table_name = 'TBL_REFERENCE_DOCUMENT'
        )
    ) LOOP
        l_new_id := seq_reference_document.NEXTVAL;

        INSERT INTO tbl_reference_document (
            reference_document_id, reference_type_id, document_code, document_title,
            issuing_authority, issue_date, effective_date, current_version_no, is_active, created_by, created_on
        )
        VALUES (
            l_new_id, rec.reference_type_id, rec.document_code, rec.document_title,
            'LEGACY_MANUAL', rec.created_on, rec.created_on, 1, rec.is_active, rec.created_by, rec.created_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_MANUAL_MASTER',
            'MANUAL_ID', rec.source_id, 'TBL_REFERENCE_DOCUMENT', 'REFERENCE_DOCUMENT_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Manual migrated to reference document.', rec.created_by
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(s.INDEX_ID) AS source_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_MANUAL_MASTER'
                   AND m.source_pk_value = TO_CHAR(s.MANUAL_ID)
                   AND m.target_table_name = 'TBL_REFERENCE_DOCUMENT'
            ) AS reference_document_id,
            NVL(s.DISPLAY_ORDER, s.INDEX_ID) AS version_no,
            'INDEX-' || TO_CHAR(s.INDEX_ID) AS version_label,
            s.HEADING AS heading_text,
            s.SECTION AS section_code,
            s.CHAPTER_NO AS chapter_no,
            s.CHAPTER_TITLE AS chapter_title,
            s.SUB_SECTION_NO AS sub_section_no,
            s.PAGE_NO AS page_no,
            s.DISPLAY_ORDER AS display_order,
            CASE WHEN NVL(s.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            NVL(s.CREATED_BY, 0) AS created_by,
            NVL(s.CREATED_DATE, SYSDATE) AS created_on
        FROM ZTBLAIS_PROD.T_MANUAL_INDEX s
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_MANUAL_INDEX'
               AND m.source_pk_value = TO_CHAR(s.INDEX_ID)
               AND m.target_table_name = 'TBL_REFERENCE_DOCUMENT_VERSION'
        )
    ) LOOP
        l_new_id := seq_reference_document_version.NEXTVAL;

        INSERT INTO tbl_reference_document_version (
            reference_document_version_id, reference_document_id, version_no, version_label,
            heading_text, section_code, chapter_no, chapter_title, sub_section_no,
            page_no, display_order, is_current_version, is_active, created_by, created_on
        )
        VALUES (
            l_new_id, rec.reference_document_id, rec.version_no, rec.version_label,
            rec.heading_text, rec.section_code, rec.chapter_no, rec.chapter_title, rec.sub_section_no,
            rec.page_no, rec.display_order, 'Y', rec.is_active, rec.created_by, rec.created_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_MANUAL_INDEX',
            'INDEX_ID', rec.source_id, 'TBL_REFERENCE_DOCUMENT_VERSION', 'REFERENCE_DOCUMENT_VERSION_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Manual index migrated to document version row.', rec.created_by
        );
    END LOOP;
END;
/

UPDATE tbl_entity t
   SET compliance_entity_id = (
       SELECT m2.target_pk_value
         FROM tbl_legacy_key_map self_map
         JOIN ZTBLAIS_PROD.T_AUDITEE_ENTITIES s
           ON s.ENTITY_ID = TO_NUMBER(self_map.source_pk_value)
         JOIN tbl_legacy_key_map m2
           ON m2.source_table_name = 'T_AUDITEE_ENTITIES'
          AND m2.source_pk_value = TO_CHAR(s.COMPLIANCE_UNIT)
          AND m2.target_table_name = 'TBL_ENTITY'
        WHERE self_map.target_table_name = 'TBL_ENTITY'
          AND self_map.target_pk_value = t.entity_id
   )
 WHERE EXISTS (
       SELECT 1
         FROM tbl_legacy_key_map self_map
         JOIN ZTBLAIS_PROD.T_AUDITEE_ENTITIES s
           ON s.ENTITY_ID = TO_NUMBER(self_map.source_pk_value)
        WHERE self_map.target_table_name = 'TBL_ENTITY'
          AND self_map.target_pk_value = t.entity_id
          AND s.COMPLIANCE_UNIT IS NOT NULL
 );

-------------------------------------------------------------------------------
-- Migration issues for low-confidence master relationships
-------------------------------------------------------------------------------

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AUDITEE_ENTITIES',
    'ENTITY_ID',
    'MULTIPLE_ROWS',
    'PARENT/INSPECTEDBY/AUDITBY',
    'TBL_ENTITY',
    'MANUAL_REVIEW',
    'Legacy entity parent, inspected-by, and some audit-link relationships require separate business verification.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'T_AUDITEE_ENTITIES'
       AND source_column_name = 'PARENT/INSPECTEDBY/AUDITBY'
);
