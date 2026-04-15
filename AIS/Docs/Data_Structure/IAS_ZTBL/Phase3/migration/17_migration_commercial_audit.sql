/*
  IAS_ZTBL Phase 3 commercial-audit migration

  Scope
  - OM
  - PDP
  - PDP to OM map
  - ARPSE
  - DAC/PAC resolutions
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

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
            TO_CHAR(o.OM_ID) AS source_id,
            o.OM_NO AS om_no,
            o.AUDIT_YEAR_ID AS audit_year,
            o.GIST_OF_OM AS gist_text,
            o.BODY_OF_OM AS body_text,
            o.MANAGEMENT_RESPONSE AS management_response,
            (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'COMMERCIAL_AUDIT_STATUS' AND lv.lookup_code = 'OM_DRAFT') AS commercial_status_id,
            CASE WHEN NVL(o.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            NVL(o.CREATED_BY, 0) AS created_by,
            NVL(o.CREATED_ON, SYSDATE) AS created_on,
            o.UPDATED_BY AS modified_by,
            o.UPDATED_ON AS modified_on
        FROM ZTBLAIS_PROD.T_COM_AUDIT_OM o
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_COM_AUDIT_OM'
               AND m.source_pk_value = TO_CHAR(o.OM_ID)
               AND m.target_table_name = 'TBL_COMMERCIAL_OM'
        )
    ) LOOP
        l_new_id := seq_commercial_om.NEXTVAL;

        INSERT INTO tbl_commercial_om (
            commercial_om_id, om_no, audit_year, gist_text, body_text, management_response,
            commercial_status_id, is_active, created_by, created_on, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.om_no, rec.audit_year, rec.gist_text, rec.body_text, rec.management_response,
            rec.commercial_status_id, rec.is_active, rec.created_by, rec.created_on, rec.modified_by, rec.modified_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_COM_AUDIT_OM',
            'OM_ID', rec.source_id, 'TBL_COMMERCIAL_OM', 'COMMERCIAL_OM_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Commercial OM migrated.', rec.created_by
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(p.PDP_ID) AS source_id,
            p.PDP_NO AS pdp_no,
            p.AUDIT_YEAR_ID AS audit_year,
            p.GIST_OF_PDP AS gist_text,
            p.BODY_OF_PDP AS body_text,
            p.MANAGEMENT_RESPONSE AS management_response,
            p.DAC_RECOMMENDATIONS AS dac_recommendation,
            (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'COMMERCIAL_AUDIT_STATUS' AND lv.lookup_code = 'PDP_DRAFTED') AS commercial_status_id,
            CASE WHEN NVL(p.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            NVL(p.CREATED_BY, 0) AS created_by,
            NVL(p.CREATED_ON, SYSDATE) AS created_on,
            p.UPDATED_BY AS modified_by,
            p.UPDATED_ON AS modified_on
        FROM ZTBLAIS_PROD.T_COM_AUDIT_PDP p
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_COM_AUDIT_PDP'
               AND m.source_pk_value = TO_CHAR(p.PDP_ID)
               AND m.target_table_name = 'TBL_COMMERCIAL_PDP'
        )
    ) LOOP
        l_new_id := seq_commercial_pdp.NEXTVAL;

        INSERT INTO tbl_commercial_pdp (
            commercial_pdp_id, pdp_no, audit_year, gist_text, body_text, management_response,
            dac_recommendation, commercial_status_id, is_active, created_by, created_on, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.pdp_no, rec.audit_year, rec.gist_text, rec.body_text, rec.management_response,
            rec.dac_recommendation, rec.commercial_status_id, rec.is_active, rec.created_by, rec.created_on, rec.modified_by, rec.modified_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_COM_AUDIT_PDP',
            'PDP_ID', rec.source_id, 'TBL_COMMERCIAL_PDP', 'COMMERCIAL_PDP_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Commercial PDP migrated.', rec.created_by
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(a.ARPSE_ID) AS source_id,
            a.ARPSE_YEAR_ID AS audit_year,
            a.PARA_NO AS para_no,
            a.GIST_OF_PARA AS gist_text,
            a.MANAGEMENT_RESPONSE AS management_response,
            (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'COMMERCIAL_AUDIT_STATUS' AND lv.lookup_code = 'ARPSE_DRAFTED') AS commercial_status_id,
            CASE WHEN NVL(a.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            NVL(a.CREATED_BY, 0) AS created_by,
            NVL(a.CREATED_ON, SYSDATE) AS created_on,
            a.UPDATED_BY AS modified_by,
            a.UPDATED_ON AS modified_on
        FROM ZTBLAIS_PROD.T_COM_AUDIT_ARPSE a
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_COM_AUDIT_ARPSE'
               AND m.source_pk_value = TO_CHAR(a.ARPSE_ID)
               AND m.target_table_name = 'TBL_COMMERCIAL_ARPSE'
        )
    ) LOOP
        l_new_id := seq_commercial_arpse.NEXTVAL;

        INSERT INTO tbl_commercial_arpse (
            commercial_arpse_id, audit_year, para_no, gist_text, management_response,
            commercial_status_id, is_active, created_by, created_on, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.audit_year, rec.para_no, rec.gist_text, rec.management_response,
            rec.commercial_status_id, rec.is_active, rec.created_by, rec.created_on, rec.modified_by, rec.modified_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_COM_AUDIT_ARPSE',
            'ARPSE_ID', rec.source_id, 'TBL_COMMERCIAL_ARPSE', 'COMMERCIAL_ARPSE_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', rec.created_by, 'Commercial ARPSE migrated.', rec.created_by
        );
    END LOOP;
END;
/

INSERT INTO tbl_commercial_pdp_observation (
    commercial_pdp_observation_id, commercial_pdp_id, commercial_om_id, is_active, created_by
)
SELECT
    NULL,
    pdp_map.target_pk_value,
    om_map.target_pk_value,
    CASE WHEN NVL(m.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END,
    NVL(m.CREATED_BY, 0)
FROM ZTBLAIS_PROD.T_COM_AUDIT_PDP_OM_MAP m
JOIN tbl_legacy_key_map pdp_map
  ON pdp_map.source_table_name = 'T_COM_AUDIT_PDP'
 AND pdp_map.source_pk_value = TO_CHAR(m.PDP_ID)
 AND pdp_map.target_table_name = 'TBL_COMMERCIAL_PDP'
JOIN tbl_legacy_key_map om_map
  ON om_map.source_table_name = 'T_COM_AUDIT_OM'
 AND om_map.source_pk_value = TO_CHAR(m.OM_ID)
 AND om_map.target_table_name = 'TBL_COMMERCIAL_OM'
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_commercial_pdp_observation x
     WHERE x.commercial_pdp_id = pdp_map.target_pk_value
       AND x.commercial_om_id = om_map.target_pk_value
);

INSERT INTO tbl_commercial_arpse_resolution (
    commercial_arpse_resolution_id, commercial_arpse_id, resolution_type_id, resolution_text,
    resolution_status_id, resolution_date, meeting_no, directive_date, directive_source_code,
    is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    arpse_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'RESOLUTION_TYPE' AND lv.lookup_code = 'DAC'),
    d.DAC_RECOMMENDATION,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'COMMERCIAL_AUDIT_STATUS' AND lv.lookup_code = 'DAC_REVIEW'),
    d.DAC_DATE,
    NULL,
    d.DAC_DATE,
    'DAC',
    CASE WHEN NVL(d.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END,
    NVL(d.CREATED_BY, 0),
    NVL(d.CREATED_ON, SYSDATE),
    d.UPDATED_BY,
    d.UPDATED_ON
FROM ZTBLAIS_PROD.T_COM_AUDIT_ARPSE_DAC d
JOIN tbl_legacy_key_map arpse_map
  ON arpse_map.source_table_name = 'T_COM_AUDIT_ARPSE'
 AND arpse_map.source_pk_value = TO_CHAR(d.ARPSE_ID)
 AND arpse_map.target_table_name = 'TBL_COMMERCIAL_ARPSE';

INSERT INTO tbl_commercial_arpse_resolution (
    commercial_arpse_resolution_id, commercial_arpse_id, resolution_type_id, resolution_text,
    resolution_status_id, resolution_date, meeting_no, directive_date, directive_source_code,
    is_active, created_by, created_on, modified_by, modified_on
)
SELECT
    NULL,
    arpse_map.target_pk_value,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'RESOLUTION_TYPE' AND lv.lookup_code = 'PAC'),
    p.PAC_DIRECTIVE,
    (SELECT lv.lookup_value_id FROM tbl_lookup_value lv JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id WHERE lt.lookup_type_code = 'COMMERCIAL_AUDIT_STATUS' AND lv.lookup_code = 'PAC_REVIEW'),
    p.PAC_DATE,
    NULL,
    p.PAC_DATE,
    'PAC',
    CASE WHEN NVL(p.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END,
    NVL(p.CREATED_BY, 0),
    NVL(p.CREATED_ON, SYSDATE),
    p.UPDATED_BY,
    p.UPDATED_ON
FROM ZTBLAIS_PROD.T_COM_AUDIT_ARPSE_PAC p
JOIN tbl_legacy_key_map arpse_map
  ON arpse_map.source_table_name = 'T_COM_AUDIT_ARPSE'
 AND arpse_map.source_pk_value = TO_CHAR(p.ARPSE_ID)
 AND arpse_map.target_table_name = 'TBL_COMMERCIAL_ARPSE';

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_CAU_*',
    'N/A',
    'N/A',
    'ALL',
    'TBL_COMMERCIAL_*',
    'MANUAL_REVIEW',
    'Older PKG_CM/T_CAU commercial structures remain available as legacy evidence but are not migrated one-for-one into the future-state execution model.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'T_CAU_*'
);
