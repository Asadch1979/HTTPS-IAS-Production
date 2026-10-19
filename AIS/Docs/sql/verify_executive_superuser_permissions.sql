-- IAS executive production-evaluation permission AUDIT: read-only.
-- Execute each SELECT in SQL Developer F8 before and after the two
-- Super-User-only registration scripts. Expected verified role/group ID: 1.
-- Scope: 5 dedicated executive pages + 12 ExecutiveDashboard GETs +
-- 8 AuditParaSnap GETs + 5 ExecutiveReports GETs.
-- The shared Organization Structure RLM alias must NOT be revoked by SQL.

-- 1. All five dedicated parent page definitions + their status.
SELECT id,page_name,page_path,page_key,status,hide_menu
  FROM t_menu_pages
 WHERE id IN (1000300,1000301,1000302,1000303,1000304)
 ORDER BY id;

-- 2. After execution, MUST return ZERO rows.
-- A non-Super User still granted any of the five new parent pages.
SELECT p.id,p.page_path,g.group_id
  FROM t_menu_pages p
  JOIN t_menu_pages_groupmap g ON g.page_id=p.id
 WHERE p.id IN (1000300,1000301,1000302,1000303,1000304)
   AND (g.group_id IS NULL OR g.group_id<>1)
 ORDER BY p.id,g.group_id;

-- 3. After execution, exactly five distinct Super User page grants.
SELECT COUNT(DISTINCT g.page_id) super_user_pages,
       COUNT(*) super_user_grant_rows
  FROM t_menu_pages_groupmap g
 WHERE g.page_id IN (1000300,1000301,1000302,1000303,1000304)
   AND g.group_id=1;

-- 4. All 25 narrowly scoped executive GET registrations, activation
-- status and role-1 grant. Inactive master endpoints remain unavailable
-- even though Super User role-1 grants are active (Y).
SELECT a.controller_name,a.action_name,a.api_path,a.page_id,
       a.is_active master_active,
       MAX(CASE WHEN g.role_id=1 THEN g.is_active END) super_user_grant,
       SUM(CASE WHEN (g.role_id IS NULL OR g.role_id<>1)
                     AND g.is_active='Y' THEN 1 ELSE 0 END)
           non_super_active_grants
  FROM t_au_api_master a
  LEFT JOIN t_au_role_api_permission g ON g.api_id=a.api_id
 WHERE UPPER(a.controller_name) IN
       ('EXECUTIVEDASHBOARD','AUDITPARASNAP','EXECUTIVEREPORTS')
   AND UPPER(TRIM(a.http_method))='GET'
 GROUP BY a.controller_name,a.action_name,a.api_path,a.page_id,a.is_active
 ORDER BY a.controller_name,a.action_name;

-- 5. After execution, MUST return ZERO rows for these three
-- controllers' GET actions. Detect privilege exposure to non-Super Users.
SELECT a.api_id,a.controller_name,a.action_name,g.role_id,g.is_active
  FROM t_au_api_master a
  JOIN t_au_role_api_permission g ON g.api_id=a.api_id
 WHERE UPPER(a.controller_name) IN
       ('EXECUTIVEDASHBOARD','AUDITPARASNAP','EXECUTIVEREPORTS')
   AND UPPER(TRIM(a.http_method))='GET'
   AND (g.role_id IS NULL OR g.role_id<>1)
   AND g.is_active='Y'
 ORDER BY a.controller_name,a.action_name,g.role_id;

-- 6. No duplicated GET HTTP method/normalized path among these endpoints.
SELECT LOWER(TRIM(BOTH '/' FROM TRIM(api_path))) normalized_path,
       UPPER(TRIM(http_method)) http_method,COUNT(*) duplicate_rows
  FROM t_au_api_master
 WHERE LOWER(TRIM(BOTH '/' FROM TRIM(api_path))) LIKE
       'api/executivedashboard/%'
    OR LOWER(TRIM(BOTH '/' FROM TRIM(api_path))) LIKE
       'api/auditparasnap/%'
    OR LOWER(TRIM(BOTH '/' FROM TRIM(api_path))) LIKE
       'api/executivereports/%'
 GROUP BY LOWER(TRIM(BOTH '/' FROM TRIM(api_path))),
          UPPER(TRIM(http_method))
HAVING COUNT(*)>1;

-- 7. After execution, missing role-1 API grants should return ZERO rows.
SELECT a.api_id,a.controller_name,a.action_name,a.is_active,
       g.is_active super_user_grant
  FROM t_au_api_master a
  LEFT JOIN t_au_role_api_permission g
    ON g.api_id=a.api_id AND g.role_id=1
 WHERE UPPER(a.controller_name) IN
       ('EXECUTIVEDASHBOARD','AUDITPARASNAP','EXECUTIVEREPORTS')
   AND UPPER(TRIM(a.http_method))='GET'
   AND (g.role_id IS NULL OR NVL(g.is_active,'N')<>'Y');

-- 8. The existing shared organization page is NOT a dedicated RLM page.
-- If another role has this organization page grant, they can still reach
-- /ExecutiveOverview/RLM's shell without a separate server-side role guard,
-- although its new executive API reads should return 403.
SELECT p.id,p.page_path,g.group_id
  FROM t_menu_pages p
  JOIN t_menu_pages_groupmap g ON g.page_id=p.id
 WHERE LOWER(TRIM(BOTH '/' FROM TRIM(p.page_path))) =
       'dashboard/entity_wise_obs'
 ORDER BY g.group_id;

-- 9. Confirm report A-E master state; N must be reviewed separately
-- instead of blindly reactivated before report-history reconciliation.
SELECT a.api_id,a.action_name,a.is_active master_active,
       g.is_active super_user_grant
  FROM t_au_api_master a
  LEFT JOIN t_au_role_api_permission g
    ON g.api_id=a.api_id AND g.role_id=1
 WHERE UPPER(a.controller_name)='EXECUTIVEREPORTS'
   AND UPPER(TRIM(a.http_method))='GET'
 ORDER BY a.action_name;
