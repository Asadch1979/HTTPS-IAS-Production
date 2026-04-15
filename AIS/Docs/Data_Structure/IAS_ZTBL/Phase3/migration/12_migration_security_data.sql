/*
  IAS_ZTBL Phase 3 security-data migration

  Scope
  - roles
  - application pages
  - API endpoints
  - permissions and role permissions
  - users, user-role assignments, and legacy session evidence
*/

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Roles, pages, APIs, permissions, and users
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
            TO_CHAR(g.ROLE_ID) AS source_id,
            'ROLE_' || TO_CHAR(g.ROLE_ID) AS role_code,
            NVL(TRIM(g.GROUP_NAME), 'Legacy Role ' || TO_CHAR(g.ROLE_ID)) AS role_name,
            NVL(g.DESCRIPTION, 'Legacy GROUP_ID=' || TO_CHAR(g.GROUP_ID)) AS role_description,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'ROLE_TYPE'
                   AND lv.lookup_code = 'BUSINESS'
            ) AS role_type_id,
            CASE WHEN NVL(g.STATUS, 'Y') IN ('Y', '1', 'ACTIVE') THEN 'Y' ELSE 'N' END AS is_active
        FROM ZTBLAIS_PROD.T_GROUPS g
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_GROUPS'
               AND m.source_pk_name = 'ROLE_ID'
               AND m.source_pk_value = TO_CHAR(g.ROLE_ID)
               AND m.target_table_name = 'TBL_ROLE'
        )
    ) LOOP
        l_new_id := seq_role.NEXTVAL;

        INSERT INTO tbl_role (
            role_id, role_code, role_name, role_description, role_type_id,
            is_system_role, is_active, created_by
        )
        VALUES (
            l_new_id, rec.role_code, rec.role_name, rec.role_description, rec.role_type_id,
            'N', rec.is_active, 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_GROUPS',
            'ROLE_ID', rec.source_id, 'TBL_ROLE', 'ROLE_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Role migrated.', 0
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(p.ID) AS source_id,
            'PG_' || TO_CHAR(p.ID) AS page_code,
            NVL(TRIM(p.PAGE_NAME), 'Legacy Page ' || TO_CHAR(p.ID)) AS page_name,
            NVL(p.PAGE_PATH, NVL(p.PAGE_URL, '/legacy/page/' || TO_CHAR(p.ID))) AS route_path,
            m.MENU_NAME AS menu_group,
            p.PAGE_KEY AS page_key,
            NVL(p.PAGE_ORDER, 0) AS sort_order,
            CASE WHEN NVL(p.HIDE_MENU, 0) = 1 THEN 'N' ELSE 'Y' END AS is_menu_visible,
            CASE WHEN NVL(p.STATUS, 'Y') IN ('Y', '1', 'ACTIVE') THEN 'Y' ELSE 'N' END AS is_active,
            m.MENU_DESCRIPTION AS description,
            m.MENU_IMAGE_PATH AS menu_image_path,
            p.PAGE_URL AS page_url,
            CASE WHEN NVL(p.HIDE_MENU, 0) = 1 THEN 'Y' ELSE 'N' END AS hide_menu_flag,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'SECURITY_STATUS'
                   AND lv.lookup_code = CASE WHEN NVL(p.STATUS, 'Y') IN ('Y', '1', 'ACTIVE') THEN 'ACTIVE' ELSE 'INACTIVE' END
            ) AS page_status_id
        FROM ZTBLAIS_PROD.T_MENU_PAGES p
        LEFT JOIN ZTBLAIS_PROD.T_MENU m
          ON m.MENU_ID = p.MENU_ID
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_MENU_PAGES'
               AND m.source_pk_value = TO_CHAR(p.ID)
               AND m.target_table_name = 'TBL_APPLICATION_PAGE'
        )
    ) LOOP
        l_new_id := seq_application_page.NEXTVAL;

        INSERT INTO tbl_application_page (
            application_page_id, page_code, page_name, route_path, menu_group, page_key, sort_order,
            is_menu_visible, is_active, description, menu_image_path, page_url, hide_menu_flag, page_status_id, created_by
        )
        VALUES (
            l_new_id, rec.page_code, rec.page_name, rec.route_path, rec.menu_group, rec.page_key, rec.sort_order,
            rec.is_menu_visible, rec.is_active, rec.description, rec.menu_image_path, rec.page_url, rec.hide_menu_flag, rec.page_status_id, 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_MENU_PAGES',
            'ID', rec.source_id, 'TBL_APPLICATION_PAGE', 'APPLICATION_PAGE_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Application page migrated.', 0
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(a.API_ID) AS source_id,
            'API_' || TO_CHAR(a.API_ID) AS api_code,
            a.CONTROLLER_NAME AS controller_name,
            a.ACTION_NAME AS action_name,
            a.HTTP_METHOD AS http_method,
            a.API_PATH AS api_path,
            a.VIEW_NAME AS endpoint_description,
            CASE WHEN NVL(a.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            a.CREATED_BY AS created_by,
            a.CREATED_ON AS created_on,
            a.UPDATE_BY AS updated_by,
            a.UPDATED_ON AS updated_on
        FROM ZTBLAIS_PROD.T_AU_API_MASTER a
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_API_MASTER'
               AND m.source_pk_value = TO_CHAR(a.API_ID)
               AND m.target_table_name = 'TBL_API_ENDPOINT'
        )
    ) LOOP
        l_new_id := seq_api_endpoint.NEXTVAL;

        INSERT INTO tbl_api_endpoint (
            api_endpoint_id, api_code, controller_name, action_name, http_method,
            api_path, endpoint_description, is_active, created_by, created_on, modified_by, modified_on
        )
        VALUES (
            l_new_id, rec.api_code, rec.controller_name, rec.action_name, rec.http_method,
            rec.api_path, rec.endpoint_description, rec.is_active, NVL(rec.created_by, 0), NVL(rec.created_on, SYSDATE), rec.updated_by, rec.updated_on
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_API_MASTER',
            'API_ID', rec.source_id, 'TBL_API_ENDPOINT', 'API_ENDPOINT_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', NVL(rec.created_by, 0), 'API endpoint migrated.', NVL(rec.created_by, 0)
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(a.API_ID) AS source_id,
            'PERM_API_' || TO_CHAR(a.API_ID) AS permission_code,
            NVL(a.VIEW_NAME, a.CONTROLLER_NAME || '.' || a.ACTION_NAME) AS permission_name,
            NVL(a.API_PATH, a.VIEW_NAME) AS permission_description,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'PERMISSION_TYPE'
                   AND lv.lookup_code = 'API'
            ) AS permission_type_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_MENU_PAGES'
                   AND m.source_pk_value = TO_CHAR(a.PAGE_ID)
                   AND m.target_table_name = 'TBL_APPLICATION_PAGE'
            ) AS application_page_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AU_API_MASTER'
                   AND m.source_pk_value = TO_CHAR(a.API_ID)
                   AND m.target_table_name = 'TBL_API_ENDPOINT'
            ) AS api_endpoint_id,
            a.ACTION_NAME AS action_code,
            CASE WHEN NVL(a.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active
        FROM ZTBLAIS_PROD.T_AU_API_MASTER a
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_AU_API_MASTER'
               AND m.source_pk_value = TO_CHAR(a.API_ID)
               AND m.target_table_name = 'TBL_PERMISSION'
        )
    ) LOOP
        l_new_id := seq_permission.NEXTVAL;

        INSERT INTO tbl_permission (
            permission_id, permission_code, permission_name, permission_description,
            permission_type_id, application_page_id, api_endpoint_id, action_code, is_active, created_by
        )
        VALUES (
            l_new_id, rec.permission_code, rec.permission_name, rec.permission_description,
            rec.permission_type_id, rec.application_page_id, rec.api_endpoint_id, rec.action_code, rec.is_active, 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_AU_API_MASTER',
            'API_ID', rec.source_id, 'TBL_PERMISSION', 'PERMISSION_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'Permission migrated from API master.', 0
        );
    END LOOP;

    FOR rec IN (
        SELECT
            TO_CHAR(u.USERID) AS source_id,
            LOWER(u.LOGIN_NAME) AS login_name,
            u.PPNO AS pp_no,
            NVL(u.LOGIN_NAME, 'PPNO-' || TO_CHAR(u.PPNO)) AS display_name,
            NULL AS email_address,
            u.PASSWORD AS password_hash,
            u.DESIGNATION AS designation_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(u.ENTITY_ID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS home_entity_id,
            (
                SELECT lv.lookup_value_id
                  FROM tbl_lookup_value lv
                  JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
                 WHERE lt.lookup_type_code = 'SECURITY_STATUS'
                   AND lv.lookup_code = CASE WHEN NVL(u.ISACTIVE, 'Y') = 'Y' THEN 'ACTIVE' ELSE 'INACTIVE' END
            ) AS user_status_id,
            NVL(u.USERFAILEDLOGINHITS, 0) AS failed_login_count,
            u.LASTLOGINDATETIME AS last_login_on,
            CASE WHEN NVL(u.ISACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END AS is_active,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(u.BRANCHID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS branch_entity_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(u.DEPARTMENTID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS department_entity_id,
            (
                SELECT m.target_pk_value
                  FROM tbl_legacy_key_map m
                 WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
                   AND m.source_pk_value = TO_CHAR(u.AUDIT_ZONEID)
                   AND m.target_table_name = 'TBL_ENTITY'
            ) AS audit_zone_entity_id
        FROM ZTBLAIS_PROD.T_USER u
        WHERE NOT EXISTS (
            SELECT 1
              FROM tbl_legacy_key_map m
             WHERE m.source_table_name = 'T_USER'
               AND m.source_pk_value = TO_CHAR(u.USERID)
               AND m.target_table_name = 'TBL_USER'
        )
    ) LOOP
        l_new_id := seq_user.NEXTVAL;

        INSERT INTO tbl_user (
            user_id, login_name, pp_no, display_name, email_address, password_hash, designation_id,
            home_entity_id, user_status_id, failed_login_count, last_login_on, is_locked, is_active,
            branch_entity_id, department_entity_id, audit_zone_entity_id, created_by
        )
        VALUES (
            l_new_id, rec.login_name, rec.pp_no, rec.display_name, rec.email_address, rec.password_hash, rec.designation_id,
            rec.home_entity_id, rec.user_status_id, rec.failed_login_count, rec.last_login_on, 'N', rec.is_active,
            rec.branch_entity_id, rec.department_entity_id, rec.audit_zone_entity_id, 0
        );

        INSERT INTO tbl_legacy_key_map (
            legacy_key_map_id, migration_batch_id, source_schema_name, source_table_name,
            source_pk_name, source_pk_value, target_table_name, target_pk_name,
            target_pk_value, source_system_code, mapping_type_code, migrated_by, remarks, created_by
        )
        VALUES (
            NULL, l_batch_id, 'ZTBLAIS_PROD', 'T_USER',
            'USERID', rec.source_id, 'TBL_USER', 'USER_ID',
            l_new_id, 'IAS_LEGACY', 'DIRECT', 0, 'User migrated.', 0
        );
    END LOOP;
END;
/

INSERT INTO tbl_role_permission (
    role_permission_id, role_id, permission_id, is_active, created_by
)
SELECT
    NULL,
    role_map.target_pk_value,
    perm_map.target_pk_value,
    CASE WHEN NVL(rp.IS_ACTIVE, 'Y') = 'Y' THEN 'Y' ELSE 'N' END,
    NVL(rp.CREATED_BY, 0)
FROM ZTBLAIS_PROD.T_AU_ROLE_API_PERMISSION rp
JOIN tbl_legacy_key_map role_map
  ON role_map.source_table_name = 'T_GROUPS'
 AND role_map.source_pk_name = 'ROLE_ID'
 AND role_map.source_pk_value = TO_CHAR(rp.ROLE_ID)
 AND role_map.target_table_name = 'TBL_ROLE'
JOIN tbl_legacy_key_map perm_map
  ON perm_map.source_table_name = 'T_AU_API_MASTER'
 AND perm_map.source_pk_value = TO_CHAR(rp.API_ID)
 AND perm_map.target_table_name = 'TBL_PERMISSION'
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_role_permission x
     WHERE x.role_id = role_map.target_pk_value
       AND x.permission_id = perm_map.target_pk_value
);

INSERT INTO tbl_user_role (
    user_role_id, user_id, role_id, is_primary_role, is_active, created_by
)
SELECT
    NULL,
    user_map.target_pk_value,
    role_map.target_pk_value,
    'Y',
    'Y',
    0
FROM ZTBLAIS_PROD.T_USER_MAPING um
JOIN tbl_legacy_key_map user_map
  ON user_map.source_table_name = 'T_USER'
 AND user_map.source_pk_value = TO_CHAR(um.USERID)
 AND user_map.target_table_name = 'TBL_USER'
JOIN tbl_legacy_key_map role_map
  ON role_map.source_table_name = 'T_GROUPS'
 AND role_map.source_pk_name = 'ROLE_ID'
 AND role_map.source_pk_value = TO_CHAR(um.ROLE_ID)
 AND role_map.target_table_name = 'TBL_ROLE'
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_user_role x
     WHERE x.user_id = user_map.target_pk_value
       AND x.role_id = role_map.target_pk_value
);

INSERT INTO tbl_user_session (
    user_session_id, user_id, session_token, login_ip_address, login_user_agent,
    login_on, session_status_id, entity_id, role_id, session_device_name, is_active, created_by
)
SELECT
    NULL,
    user_map.target_pk_value,
    'LEGACY-' || TO_CHAR(u.USERID) || '-' || TO_CHAR(ROWNUM),
    u.LOGIN_TERMINAL,
    'LEGACY_IMPORT',
    u.LASTLOGINDATETIME,
    (
        SELECT lv.lookup_value_id
          FROM tbl_lookup_value lv
          JOIN tbl_lookup_type lt ON lt.lookup_type_id = lv.lookup_type_id
         WHERE lt.lookup_type_code = 'SESSION_STATUS'
           AND lv.lookup_code = 'CLOSED'
    ) AS session_status_id,
    (
        SELECT m.target_pk_value
          FROM tbl_legacy_key_map m
         WHERE m.source_table_name = 'T_AUDITEE_ENTITIES'
           AND m.source_pk_value = TO_CHAR(u.ENTITY_ID)
           AND m.target_table_name = 'TBL_ENTITY'
    ) AS entity_id,
    role_map.target_pk_value,
    u.USER_LOCATION_TYPE,
    'Y',
    0
FROM ZTBLAIS_PROD.T_USER u
JOIN tbl_legacy_key_map user_map
  ON user_map.source_table_name = 'T_USER'
 AND user_map.source_pk_value = TO_CHAR(u.USERID)
 AND user_map.target_table_name = 'TBL_USER'
LEFT JOIN ZTBLAIS_PROD.T_USER_MAPING um
  ON um.USERID = u.USERID
LEFT JOIN tbl_legacy_key_map role_map
  ON role_map.source_table_name = 'T_GROUPS'
 AND role_map.source_pk_name = 'ROLE_ID'
 AND role_map.source_pk_value = TO_CHAR(um.ROLE_ID)
 AND role_map.target_table_name = 'TBL_ROLE'
WHERE u.LASTLOGINDATETIME IS NOT NULL;

-------------------------------------------------------------------------------
-- Migration issues for page/api metadata that is intentionally not modeled
-------------------------------------------------------------------------------

INSERT INTO tbl_migration_issue (
    migration_issue_id, migration_batch_id, source_table_name, source_pk_name, source_pk_value,
    source_column_name, target_table_name, issue_type_code, issue_note, created_by
)
SELECT
    NULL,
    (SELECT migration_batch_id FROM tbl_migration_batch WHERE batch_code = 'PHASE3_BASELINE_01'),
    'T_AU_PAGE_API_MAP',
    'MAP_ID',
    'MULTIPLE_ROWS',
    'CALL_TYPE/SOURCE_TYPE/CONFIDENCE_LEVEL/APICALLS',
    'TBL_PERMISSION',
    'MANUAL_REVIEW',
    'Legacy page-api confidence and call-type metadata is not modeled one-for-one in IAS_ZTBL and should be reviewed during wrapper design.',
    0
FROM dual
WHERE NOT EXISTS (
    SELECT 1
      FROM tbl_migration_issue
     WHERE source_table_name = 'T_AU_PAGE_API_MAP'
       AND source_column_name = 'CALL_TYPE/SOURCE_TYPE/CONFIDENCE_LEVEL/APICALLS'
);
