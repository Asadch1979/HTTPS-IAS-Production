-- Read-only preflight: execute only in an approved staging session.
SELECT object_name, object_type, status FROM user_objects
WHERE object_name IN ('V_IAS_ENTITY_REPORTING_IDS','V_IAS_ENTITY_REPORTING_NAMES',
 'V_EXEC_ENTITY_SCOPE','V_EXEC_PARA_BASE','V_EXEC_KPI_LIFECYCLE','PKG_EXEC_DASHBOARD');
-- Missing/conflicting immediate auditee reporting offices. Never auto-correct.
SELECT h.entity_id, COUNT(DISTINCT h.reporting_1) distinct_offices,
 COUNT(*) hierarchy_rows, COUNT(h.reporting_1) mapped_rows
FROM V_IAS_ENTITY_REPORTING_IDS h GROUP BY h.entity_id
HAVING COUNT(DISTINCT h.reporting_1) <> 1 OR COUNT(h.reporting_1) <> COUNT(*);
SELECT h.entity_id, h.reporting_1 FROM V_IAS_ENTITY_REPORTING_IDS h
WHERE h.reporting_1 IS NOT NULL AND NOT EXISTS
 (SELECT 1 FROM t_auditee_entities e WHERE e.entity_id=h.reporting_1);
SELECT entity_id, COUNT(DISTINCT name) names FROM t_auditee_entities
GROUP BY entity_id HAVING COUNT(DISTINCT name) <> 1;
-- Duplicated source keys block acceptance pending investigation.
SELECT com_id, COUNT(*) FROM V_EXEC_KPI_LIFECYCLE WHERE record_type='PARA'
GROUP BY com_id HAVING COUNT(*) > 1;
-- Current ordinary aging reconciliation (apply the same API filters for scoped acceptance).
SELECT COUNT(*) total_outstanding,
 COUNT(CASE WHEN aging_days > 365 THEN 1 END) over_365,
 COUNT(CASE WHEN aging_days > 730 THEN 1 END) over_730,
 COUNT(CASE WHEN aging_days IS NULL THEN 1 END) unknown_dates,
 COUNT(CASE WHEN aging_days IS NOT NULL THEN 1 END) known_dates
FROM V_EXEC_PARA_BASE WHERE is_open=1;
-- Run after deployment: must return no rows.
SELECT name,type,line,position,text FROM user_errors
WHERE name IN ('V_EXEC_ENTITY_SCOPE','V_EXEC_PARA_BASE','PKG_EXEC_DASHBOARD')
ORDER BY name,type,sequence;
