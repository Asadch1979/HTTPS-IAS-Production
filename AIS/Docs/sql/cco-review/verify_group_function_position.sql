-- READ-ONLY ORACLE F8 CHECKS. Illustrative dates: 01-Jan-2026 to 28-Oct-2026.
-- Run only under an explicitly approved IAS test/staging database identity.
-- No privileges, rows, triggers or objects are changed by these statements.
-- 1. Confirm deployed dependencies and status.
SELECT object_name,object_type,status
  FROM user_objects
 WHERE object_name IN ('PKG_EXEC_REPORTS','PKG_CCO_FUNCTION_POSITION',
                       'V_IAS_ENTITY_REPORTING_IDS_GROUP_REPORTS',
                       'V_EXEC_REPORT_SOURCE','V_EXEC_ENTITY_SCOPE')
 ORDER BY object_name,object_type;
-- If the scope security objects reside under a protected owner, have the DBA
-- verify them in ALL_OBJECTS and their direct grants separately.

-- 2. Report-view rows and supplied exclusions.
SELECT parent_id,p_name,entity_id,c_name
  FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports
 ORDER BY parent_id,entity_id;

SELECT COUNT(*) invalid_exclusions
  FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports
 WHERE parent_id=112215 OR entity_id=112221;

-- 3. Multiple distinct Group/Division paths must NOT be chosen arbitrarily.
SELECT r.entity_id,COUNT(DISTINCT r.parent_id) group_count,
       COUNT(DISTINCT NVL(r.p_name,'<NULL>')) group_name_count,
       COUNT(DISTINCT NVL(r.c_name,'<NULL>')) division_name_count
  FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r
 GROUP BY r.entity_id
HAVING COUNT(DISTINCT r.parent_id)>1
    OR COUNT(DISTINCT NVL(r.p_name,'<NULL>'))>1
    OR COUNT(DISTINCT NVL(r.c_name,'<NULL>'))>1
 ORDER BY r.entity_id;

-- 4. Identify disagreement with auditee ownership hierarchy.
SELECT DISTINCT h.entity_id,h.business_group_id,h.division_id,
       r.parent_id view_group_id,r.entity_id view_division_id
  FROM V_EXEC_ENTITY_SCOPE h
  JOIN V_IAS_ENTITY_REPORTING_IDS_Group_reports r ON r.entity_id=h.division_id
 WHERE h.business_group_id IS NOT NULL
   AND h.business_group_id<>r.parent_id
 ORDER BY h.entity_id;

-- 5. A single COM_ID must not multiply in the management source.
SELECT s.com_id,COUNT(*) source_rows
  FROM V_EXEC_REPORT_SOURCE s
 WHERE s.audit_type='D'
   AND s.effective_added_on<DATE '2026-10-29'
 GROUP BY s.com_id
HAVING s.com_id IS NULL OR COUNT(*)<>1
 ORDER BY source_rows DESC;

-- 6. Report E / Function Wise Position gross control totals.
-- Both paths must match on the same dates and authorized entity population.
SELECT SUM(CASE WHEN s.effective_added_on<DATE '2026-01-01'
                  AND (s.close_on IS NULL OR s.close_on>=DATE '2026-01-01')
                THEN 1 ELSE 0 END) opening_balance,
       SUM(CASE WHEN s.effective_added_on>=DATE '2026-01-01'
                  AND s.effective_added_on<DATE '2026-10-29'
                THEN 1 ELSE 0 END) paras_added,
       SUM(CASE WHEN s.close_on>=DATE '2026-01-01'
                  AND s.close_on<DATE '2026-10-29'
                  AND s.effective_added_on<DATE '2026-10-29'
                THEN 1 ELSE 0 END) settled_paras,
       SUM(CASE WHEN s.effective_added_on<DATE '2026-10-29'
                  AND (s.close_on IS NULL OR s.close_on>=DATE '2026-10-29')
                THEN 1 ELSE 0 END) outstanding_paras
  FROM V_EXEC_REPORT_SOURCE s
 WHERE s.audit_type='D'
   AND s.effective_added_on<DATE '2026-10-29';

-- 7. After DBA-APPROVED package compilation: no invalid sources/bodies.
SELECT name,type,line,position,text
  FROM user_errors
 WHERE name IN ('PKG_EXEC_REPORTS','PKG_CCO_FUNCTION_POSITION')
 ORDER BY name,sequence;

-- MANUAL / SECURITY CHECKS: require an independently trusted scope owner.
-- Validate the corrected PKG_CCO_SCOPE marker's post-COMMIT/ROLLBACK failure,
-- empty-portfolio behavior, revoked sessions, CCO role 38 / Super User role 1,
-- page 1000302 + exact GET /api/CcoCompliance/GetFunctionPosition permission,
-- unrelated-user denial, pooling isolation and EXPLICIT database owner grants.
-- Verify both per-division rows and grand totals against Report E and
-- investigate every unmapped/conflicting row. No automatic grants or activation.
