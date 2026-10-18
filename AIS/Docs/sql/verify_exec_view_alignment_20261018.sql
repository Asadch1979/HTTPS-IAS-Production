-- Executive view/package alignment: READ-ONLY diagnostics for IAS Development.
-- Run each SELECT separately (SQL Developer F8), after approval and compilation.
-- The scripts and GitHub changes themselves do not update a database.
-- Required order: V_EXEC_ENTITY_SCOPE -> V_EXEC_KPI_SRC_A/O/C ->
-- V_EXEC_KPI_LIFECYCLE -> V_EXEC_ISSUE_RECTIFIED_RECON ->
-- V_EXEC_PARA_BASE (within PKG_EXEC_DASHBOARD.sql) ->
-- V_EXEC_REPORT_SOURCE -> package spec/body; recompile PKG_EXEC_REPORTS if invalid.
-- Existing, unchanged views need not be replaced unless their dependencies
-- were invalidated; check user_errors after every compile.

-- 1. All dependencies must be VALID; zero USER_ERRORS after compile.
SELECT object_name, object_type, status
  FROM user_objects
 WHERE object_name IN
  ('V_EXEC_ENTITY_SCOPE','V_EXEC_PARA_BASE','V_EXEC_KPI_SRC_A',
   'V_EXEC_KPI_SRC_O','V_EXEC_KPI_SRC_C','V_EXEC_KPI_LIFECYCLE',
   'V_EXEC_REPORT_SOURCE','V_EXEC_ISSUE_RECTIFIED_RECON',
   'PKG_EXEC_DASHBOARD','PKG_EXEC_REPORTS')
 ORDER BY object_name,object_type;

SELECT name,type,line,position,text
  FROM user_errors
 WHERE name IN
  ('V_EXEC_PARA_BASE','V_EXEC_KPI_LIFECYCLE','V_EXEC_REPORT_SOURCE',
   'V_EXEC_ISSUE_RECTIFIED_RECON','PKG_EXEC_DASHBOARD','PKG_EXEC_REPORTS')
 ORDER BY name,sequence;

-- 2. One row per COM_ID in modern/legacy PARA views. Nonzero differences
-- signal upstream duplicated source keys or organizational scope multiplication.
SELECT 'PARA_BASE' source_name,COUNT(*) view_rows,
       COUNT(DISTINCT com_id) distinct_com_ids
  FROM V_EXEC_PARA_BASE
UNION ALL
SELECT 'LIFECYCLE_PARA',COUNT(*),COUNT(DISTINCT com_id)
  FROM V_EXEC_KPI_LIFECYCLE WHERE record_type = 'PARA'
UNION ALL
SELECT 'REPORT_SOURCE',COUNT(*),COUNT(DISTINCT com_id)
  FROM V_EXEC_REPORT_SOURCE;

SELECT 'ISSUE' source_name,COUNT(*) view_rows,
       COUNT(DISTINCT observation_id) distinct_observation_ids
  FROM V_EXEC_KPI_LIFECYCLE WHERE record_type = 'ISSUE';

SELECT entity_id,COUNT(*) scope_rows
  FROM V_EXEC_ENTITY_SCOPE
 GROUP BY entity_id HAVING COUNT(*) > 1
 ORDER BY scope_rows DESC,entity_id;

-- 3. Invalid age data must not masquerade as a valid 0-90 day bucket.
SELECT aging_quality,aging_bucket,COUNT(*) paras
  FROM V_EXEC_PARA_BASE
 GROUP BY aging_quality,aging_bucket
 ORDER BY aging_quality,aging_bucket;

SELECT com_id,para_status,para_added_on,setteled_on,
       aging_days,aging_bucket,aging_quality
  FROM V_EXEC_PARA_BASE
 WHERE aging_quality <> 'OK' AND ROWNUM <= 50;

-- 4. Report A-E source must exclude ISSUE events and retain stable COM_ID keys.
-- Movement-quality flags are diagnostics; no settlement count is invented.
SELECT report_movement_quality,COUNT(*) para_rows
  FROM V_EXEC_REPORT_SOURCE
 GROUP BY report_movement_quality ORDER BY para_rows DESC;

SELECT source_ind,para_status,COUNT(*) report_rows,
       COUNT(issue_rectified_copy) modern_copy_records,
       SUM(CASE WHEN issue_rectified_copy=1 THEN 1 ELSE 0 END) copied_true
  FROM V_EXEC_REPORT_SOURCE
 GROUP BY source_ind,para_status
 ORDER BY source_ind,para_status;

-- 5. Check CIA plan coverage by ENTITY versus older plan-ID join.
-- Entity match is the approved distinct-entity numerator.
-- Do not treat a result as certified until statuses and plan population agree.
SELECT p.auditperiodid,
       COUNT(DISTINCT CASE WHEN p.status IN (1,2) THEN p.entity_id END)
           planned_entities,
       COUNT(DISTINCT CASE
          WHEN p.status IN (1,2) AND e.eng_id IS NOT NULL
          THEN p.entity_id END) audited_by_entity,
       COUNT(DISTINCT CASE
          WHEN p.status IN (1,2) AND e.eng_id IS NOT NULL
               AND e.plan_id=p.id THEN p.entity_id END) audited_by_planid
  FROM t_au_plan p
  LEFT JOIN t_au_plan_eng e
    ON e.period_id = p.auditperiodid
   AND e.entity_id = p.entity_id
   AND (e.status=14 OR EXISTS (
         SELECT 1 FROM t_au_plan_eng_log l
          WHERE l.e_id=e.eng_id AND l.status_id=14))
 GROUP BY p.auditperiodid
 ORDER BY p.auditperiodid DESC;

-- 6. Dropped issues: never infer from ISSUE_RECTIFIED_ON, which excludes
-- observation STATUS=23. Only dated dispositions before conclusion count.
SELECT TRUNC(o.stelled_on,'YYYY') dropped_year,
       COUNT(DISTINCT k.observation_id) eligible_dropped_issues
  FROM V_EXEC_KPI_LIFECYCLE k
  JOIN t_au_observation o ON o.id=k.observation_id
 WHERE k.record_type='ISSUE'
   AND k.issue_eligible=1
   AND k.observation_status=23
   AND o.stelled_on IS NOT NULL
   AND k.report_concluded_on IS NOT NULL
   AND o.stelled_on<k.report_concluded_on
 GROUP BY TRUNC(o.stelled_on,'YYYY')
 ORDER BY dropped_year;

-- 7. ISSUE_RECTIFIED is a source event, and the post-compliance value is a
-- carried copy. Do NOT add the below two populations together.
SELECT post_copy_status,COUNT(*) observations
  FROM V_EXEC_ISSUE_RECTIFIED_RECON
 GROUP BY post_copy_status ORDER BY post_copy_status;

-- 8. Check source mapping before changing CIA issue classification.
-- An empty table explains unmapped categories; do not guess SOURCE_IDs.
SELECT source_type,category_code,is_active,COUNT(*) approved_ids
  FROM t_ias_cia_issue_nature_map
 GROUP BY source_type,category_code,is_active
 ORDER BY source_type,category_code,is_active;

-- 9. Review recorded conclusion chronology only where multiple status14
-- events occurred; log ID sequence, not MAX(CREATED_ON), picks the latest.
SELECT e_id,COUNT(*) conclusion_events,
       MAX(created_on) latest_timestamp,
       MAX(created_on) KEEP (DENSE_RANK LAST ORDER BY id) last_log_timestamp
  FROM t_au_plan_eng_log
 WHERE status_id=14
 GROUP BY e_id
HAVING COUNT(*)>1 AND MAX(created_on) <>
       MAX(created_on) KEEP (DENSE_RANK LAST ORDER BY id);
