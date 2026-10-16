-- Run after R_IAS_PARA_POSITION.sql. Reuses the original report's access grants.
-- Page ID 1000400 must match Services/PageIdResolver.cs. Fails on collisions.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
DECLARE
    V_SOURCE_ID NUMBER;
    V_API_ID NUMBER;
    V_COLLISIONS NUMBER;
BEGIN
    SELECT id INTO V_SOURCE_ID FROM t_menu_pages
     WHERE LOWER(LTRIM(page_path, '/')) = 'reports/fad_monthly_review';

    SELECT COUNT(*) INTO V_COLLISIONS FROM t_menu_pages
     WHERE (id = 1000400 AND NVL(page_key, '#') <> 'IAS_PARA_POSITION')
        OR (LOWER(LTRIM(page_path, '/')) = 'reports/ias_para_position' AND id <> 1000400);
    IF V_COLLISIONS > 0 THEN
        RAISE_APPLICATION_ERROR(-20020, 'IAS Para Position page ID/path collision; resolve before deployment.');
    END IF;

    INSERT INTO t_menu_pages
        (id, menu_id, page_name, page_path, page_order, status, hide_menu, sub_menu, page_key, page_url)
    SELECT 1000400, menu_id, 'IAS Para Position', 'Reports/IAS_PARA_POSITION',
           page_order + 1, status, hide_menu, sub_menu, 'IAS_PARA_POSITION', '/Reports/IAS_PARA_POSITION'
      FROM t_menu_pages WHERE id = V_SOURCE_ID
       AND NOT EXISTS (SELECT 1 FROM t_menu_pages WHERE id = 1000400);

    FOR g IN (SELECT DISTINCT group_id FROM t_menu_pages_groupmap WHERE page_id = V_SOURCE_ID) LOOP
        INSERT INTO t_menu_pages_groupmap (groupmap_id, group_id, page_id)
        SELECT (SELECT NVL(MAX(groupmap_id), 0) + 1 FROM t_menu_pages_groupmap), g.group_id, 1000400
          FROM dual WHERE NOT EXISTS (
              SELECT 1 FROM t_menu_pages_groupmap WHERE group_id = g.group_id AND page_id = 1000400);
    END LOOP;

    INSERT INTO t_au_api_master
        (action_name, page_id, controller_name, api_path, http_method, is_active, created_on)
    SELECT 'get_ias_para_position_for_entity_type_id', 1000400, 'ApiCalls',
           '/ApiCalls/get_ias_para_position_for_entity_type_id', 'POST', 'Y', SYSDATE
      FROM dual WHERE NOT EXISTS (
          SELECT 1 FROM t_au_api_master
           WHERE LOWER(api_path) = '/apicalls/get_ias_para_position_for_entity_type_id'
             AND UPPER(http_method) = 'POST');
    SELECT api_id INTO V_API_ID FROM t_au_api_master
     WHERE LOWER(api_path) = '/apicalls/get_ias_para_position_for_entity_type_id'
       AND UPPER(http_method) = 'POST';
    FOR r IN (
        SELECT DISTINCT p.role_id FROM t_au_role_api_permission p
          JOIN t_au_api_master a ON a.api_id = p.api_id
         WHERE LOWER(a.api_path) = '/apicalls/get_fad_monthly_review_paras_for_entity_type_id'
           AND UPPER(a.http_method) = 'POST' AND a.is_active = 'Y' AND p.is_active = 'Y'
    ) LOOP
        INSERT INTO t_au_role_api_permission (role_id, api_id, is_active, created_by)
        SELECT r.role_id, V_API_ID, 'Y', NULL FROM dual WHERE NOT EXISTS (
            SELECT 1 FROM t_au_role_api_permission WHERE role_id = r.role_id AND api_id = V_API_ID);
    END LOOP;
    COMMIT;
EXCEPTION WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/
