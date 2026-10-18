-- Read-only IAS executive dashboard permission audit.
-- Run SELECTs separately in SQL Developer (F8) after reviewing/compiling source.
-- The registration script makes NO changes until explicitly executed by DBA.

-- 1. Distinct parent page IDs. RLM uses existing Dashboard/entity_wise_obs.
SELECT id,page_key,page_path,page_url,status,hide_menu,menu_id
  FROM t_menu_pages
 WHERE id IN (1000300,1000301,1000302,1000303,1000304)
    OR LOWER(TRIM(BOTH '/' FROM TRIM(page_path))) =
       'dashboard/entity_wise_obs'
 ORDER BY id;

-- 2. Duplicate route or key indicates potential permission-map ambiguity.
SELECT LOWER(TRIM(BOTH '/' FROM TRIM(page_path))) normalized_path,
       COUNT(*) pages
  FROM t_menu_pages
 WHERE LOWER(TRIM(BOTH '/' FROM TRIM(page_path))) IN
       ('executiveoverview/top','executiveoverview/group',
        'executiveoverview/cco','executiveoverview/cia',
        'executiveoverview/auditparasnap','dashboard/entity_wise_obs')
 GROUP BY LOWER(TRIM(BOTH '/' FROM TRIM(page_path)))
HAVING COUNT(*)>1;

-- 3. All expected ExecutiveDashboard/AuditParaSnap GET APIs, registration,
-- page IDs, and active/inactive states. Existing shared APIs use legacy
-- TOP metadata PAGE_ID=1000300 but page checks are independently scoped.
SELECT a.api_id,a.controller_name,a.action_name,a.api_path,a.http_method,
       a.page_id,a.is_active,
       COUNT(p.role_id) group_grants,
       SUM(CASE WHEN p.is_active='Y' THEN 1 ELSE 0 END) active_grants
  FROM t_au_api_master a
  LEFT JOIN t_au_role_api_permission p ON p.api_id=a.api_id
 WHERE UPPER(a.controller_name) IN ('EXECUTIVEDASHBOARD','AUDITPARASNAP')
   AND UPPER(a.http_method)='GET'
 GROUP BY a.api_id,a.controller_name,a.action_name,a.api_path,
          a.http_method,a.page_id,a.is_active
 ORDER BY a.controller_name,a.action_name;

-- 4. No duplicate normalized HTTP method+API path.
SELECT LOWER(TRIM(BOTH '/' FROM TRIM(api_path))) normalized_path,
       UPPER(TRIM(http_method)) http_method,
       COUNT(*) registrations
  FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM TRIM(api_path))) LIKE 'api/executivedashboard/%'
    OR LOWER(TRIM(BOTH '/' FROM TRIM(api_path))) LIKE 'api/auditparasnap/%'
 GROUP BY LOWER(TRIM(BOTH '/' FROM TRIM(api_path))),UPPER(TRIM(http_method))
HAVING COUNT(*)>1;

-- 5. Sensitive GET registrations MUST use CIA page or Snap page.
SELECT api_id,controller_name,action_name,page_id,is_active
  FROM t_au_api_master
 WHERE (UPPER(controller_name)='EXECUTIVEDASHBOARD'
        AND UPPER(action_name) IN ('GETCIACOMMANDKPIS','GETCIAISSUENATURE')
        AND NVL(page_id,-1)<>1000303)
    OR (UPPER(controller_name)='AUDITPARASNAP'
        AND NVL(page_id,-1)<>1000304);

-- 6. No role other than verified IAS Super User=1 / CIA=3 may hold
-- an ACTIVE grant for bank-wide CIA-only or Snap-only GET endpoints.
SELECT a.api_id,a.action_name,p.role_id,p.is_active
  FROM t_au_api_master a
  JOIN t_au_role_api_permission p ON p.api_id=a.api_id
 WHERE p.role_id NOT IN (1,3) AND NVL(p.is_active,'N')='Y'
   AND ((UPPER(a.controller_name)='AUDITPARASNAP')
     OR (UPPER(a.controller_name)='EXECUTIVEDASHBOARD'
         AND UPPER(a.action_name) IN
             ('GETCIACOMMANDKPIS','GETCIAISSUENATURE')));

-- 7. Inspect inactive/active grants across fixed and verified optional
-- roles before opting in: registration does not flip Y/N values.
SELECT a.controller_name,a.action_name,a.is_active api_active,
       p.role_id,p.is_active grant_active
  FROM t_au_api_master a
  JOIN t_au_role_api_permission p ON p.api_id=a.api_id
 WHERE UPPER(a.controller_name) IN ('EXECUTIVEDASHBOARD','AUDITPARASNAP')
   AND UPPER(a.http_method)='GET'
 ORDER BY a.controller_name,a.action_name,p.role_id;

-- 8. Confirm no new independent ExecutiveOverview/RLM page was created.
-- Its role/page authorization is via existing Dashboard/entity_wise_obs.
SELECT id,page_path,page_key
  FROM t_menu_pages
 WHERE LOWER(TRIM(BOTH '/' FROM TRIM(page_path))) =
       'executiveoverview/rlm';

-- 9. Optional role-to-page mappings: inspect with verified IAS role IDs,
-- *not* guessed role numbers.
SELECT p.page_id,m.page_path,g.group_id
  FROM t_menu_pages_groupmap g
  JOIN t_menu_pages m ON m.id=g.page_id
  JOIN (SELECT 1000300 page_id FROM dual
        UNION ALL SELECT 1000301 FROM dual
        UNION ALL SELECT 1000302 FROM dual
        UNION ALL SELECT 1000303 FROM dual
        UNION ALL SELECT 1000304 FROM dual) p
    ON p.page_id=m.id
 ORDER BY p.page_id,g.group_id;
