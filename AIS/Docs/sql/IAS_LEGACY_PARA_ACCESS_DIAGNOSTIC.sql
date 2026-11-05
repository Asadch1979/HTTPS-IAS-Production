-- READ ONLY: diagnose Legacy Para /ApiCalls/ API access.
-- Run with a DBA against the same schema used by the deployed application.
-- Do not change or activate grants based solely on this report.
--
-- For role 15, expected URLs are:
-- 2471 /ApiCalls/LegacyParaSubmit     (maker page 192)
-- 2472 /ApiCalls/LegacyParaLookup     (maker page 192)
-- 2473 /ApiCalls/LegacyParaList       (maker 192, checker 193)
-- 2474 /ApiCalls/LegacyParaDetail     (maker 192, checker 193)
-- 2475 /ApiCalls/LegacyParaDecide     (checker page 193)
-- API_ID values above describe the reported installation; do not assume these
-- identifiers exist or have those meanings in other installations.
--
-- The initial automatic queue request is POST /ApiCalls/LegacyParaList.
-- If the API path remains /api/LegacyParaRequests/List, the cached path
-- permission cannot match the new frontend even with an active role grant.

SELECT a.api_id,
       a.controller_name,
       a.action_name,
       a.api_path,
       a.page_id AS master_page_id,
       a.http_method,
       a.is_active AS api_active,
       rp.role_id,
       rp.is_active AS role_grant_active,
       d.page_id AS dependency_page_id,
       d.is_active AS dependency_active,
       CASE
         WHEN LOWER(TRIM(BOTH '/' FROM a.api_path)) IN
              ('apicalls/legacyparasubmit','apicalls/legacyparalookup',
               'apicalls/legacyparalist','apicalls/legacyparadetail',
               'apicalls/legacyparadecide') THEN 'NEW_ROUTE'
         ELSE 'LEGACY_OR_OTHER_ROUTE'
       END AS route_status
  FROM t_au_api_master a
  LEFT JOIN t_au_role_api_permission rp
    ON rp.api_id = a.api_id AND rp.role_id = 15
  LEFT JOIN t_ias_legacy_page_api d ON d.api_id = a.api_id
 WHERE LOWER(a.api_path) LIKE '%legacypara%'
 ORDER BY a.api_id, d.page_id;

-- Page grant for selected group (substitute approved selected group_id):
SELECT gm.group_id, gm.page_id, p.status AS page_status
  FROM t_menu_pages_groupmap gm
  JOIN t_menu_pages p ON p.id = gm.page_id
 WHERE gm.group_id = :selected_group_id
   AND gm.page_id IN (192,193)
 ORDER BY gm.page_id;

-- After an approved path/permission change, sign out and back in: the web
-- session caches API and page grants. Also verify the matching Oracle package
-- has been deployed. If the denied response has a code, distinguish:
-- API_FORBIDDEN (session API path), PAGE_FORBIDDEN (page grant),
-- DATABASE_FORBIDDEN (Oracle role/page/scope), CONTEXT_REQUIRED (login context).
