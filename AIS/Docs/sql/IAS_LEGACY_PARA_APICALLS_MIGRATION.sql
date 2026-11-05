-- Legacy Para route migration: /api/LegacyParaRequests/* -> /ApiCalls/LegacyPara*
-- REVIEWED SOURCE ONLY. Execute in a controlled database change window alongside
-- deployment of the matching application and PKG_IAS_LEGACY_PARA package source.
-- Does not add roles, activate grants, change module flags, or commit.
-- Existing API_IDs, PAGE_IDs, active states and role/page dependencies are retained.
-- On any unexpected mixed or missing route state, fail closed with no partial DML.
DECLARE
  v_old_count NUMBER;
  v_new_count NUMBER;
  v_updated NUMBER;
  PROCEDURE migrate_action(p_action VARCHAR2) IS
    v_old_path VARCHAR2(200) := 'api/legacypararequests/'||LOWER(p_action);
    v_new_path VARCHAR2(200) := 'apicalls/legacypara'||LOWER(p_action);
  BEGIN
    SELECT COUNT(*) INTO v_old_count FROM T_AU_API_MASTER
      WHERE LOWER(TRIM(BOTH '/' FROM api_path)) = v_old_path
        AND UPPER(TRIM(http_method)) = 'POST';
    SELECT COUNT(*) INTO v_new_count FROM T_AU_API_MASTER
      WHERE LOWER(TRIM(BOTH '/' FROM api_path)) = v_new_path
        AND UPPER(TRIM(http_method)) = 'POST';
    IF v_old_count > 0 AND v_new_count > 0 THEN
      RAISE_APPLICATION_ERROR(-20941, 'Mixed old/new Legacy Para API route registrations. Review manually: '||p_action);
    END IF;
    IF v_old_count = 0 AND v_new_count = 0 THEN
      RAISE_APPLICATION_ERROR(-20942, 'Legacy Para API registration missing: '||p_action);
    END IF;
    IF v_old_count > 0 THEN
      UPDATE T_AU_API_MASTER
         SET controller_name = 'ApiCalls',
             action_name = 'LegacyPara'||INITCAP(LOWER(p_action)),
             api_path = '/ApiCalls/LegacyPara'||INITCAP(LOWER(p_action))
       WHERE LOWER(TRIM(BOTH '/' FROM api_path)) = v_old_path
         AND UPPER(TRIM(http_method)) = 'POST';
      v_updated := SQL%ROWCOUNT;
      IF v_updated <> v_old_count THEN
        RAISE_APPLICATION_ERROR(-20943, 'Legacy Para registration count changed during migration: '||p_action);
      END IF;
    END IF;
  END;
BEGIN
  SAVEPOINT legacy_para_api_migration;
  migrate_action('Submit');
  migrate_action('Lookup');
  migrate_action('List');
  migrate_action('Detail');
  migrate_action('Decide');
EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK TO legacy_para_api_migration;
    RAISE;
END;
/

-- Read-only post-migration validation before DBA COMMIT:
SELECT a.api_id, a.controller_name, a.action_name, a.api_path,
       a.page_id, a.http_method, a.is_active AS api_active,
       p.role_id, p.is_active AS grant_active,
       d.page_id AS dependent_page, d.is_active AS dependency_active
  FROM T_AU_API_MASTER a
  LEFT JOIN T_AU_ROLE_API_PERMISSION p ON p.api_id = a.api_id
  LEFT JOIN T_IAS_LEGACY_PAGE_API d ON d.api_id = a.api_id
 WHERE LOWER(TRIM(BOTH '/' FROM a.api_path)) LIKE 'apicalls/legacypara%'
   AND UPPER(TRIM(a.http_method)) = 'POST'
 ORDER BY a.api_id, p.role_id, d.page_id;

-- Do not COMMIT until all five actions, selected role grants and page 192/193
-- dependencies have been reviewed against the change ticket and package deployment.
-- Have an approved rollback plan for the application/Oracle pair.
