-- Read-only preflight for Phase 1 branch risk rating.
-- The Excel configuration must be installed and PUBLISHED before execution.

SELECT p.auditperiodid,p.description,p.start_date,p.end_date,
       p.status_id,s.status
  FROM T_AU_PERIOD p
  JOIN T_AU_PERIOD_STATUS s ON s.id=p.status_id
 ORDER BY p.auditperiodid;

SELECT g.period_id,COUNT(*) engagements,COUNT(DISTINCT g.entity_id) branches
  FROM T_AU_PLAN_ENG g
 WHERE g.entity_type=6
 GROUP BY g.period_id
 ORDER BY g.period_id;

SELECT v.version_id,v.version_name,v.state,v.calculator,
       (SELECT COUNT(*) FROM T_FAD_RISK_PROCESS p
         WHERE p.version_id=v.version_id) process_count,
       (SELECT NVL(SUM(p.weight),0) FROM T_FAD_RISK_PROCESS p
         WHERE p.version_id=v.version_id) total_weight,
       (SELECT COUNT(*) FROM T_FAD_RISK_ANNEX a
         WHERE a.version_id=v.version_id) annexure_count
  FROM T_FAD_RISK_VERSION v
 ORDER BY v.version_id;

SELECT COUNT(*) stage_rows,COUNT(DISTINCT s.anex_id) distinct_ids,
       COUNT(CASE WHEN a.id IS NOT NULL THEN 1 END) existing_ids
  FROM T_FAD_RISK_MAP_STAGE s
  LEFT JOIN T_AUDIT_CHECKLIST_ANNEXURE a ON a.id=s.anex_id;

-- This is not an automatic source for financial/branch-size figures.
-- It shows availability of approved manual IAS inputs for the new model.
SELECT g.period_id,COUNT(*) engagements,
       COUNT(a.assessment_id) approved_input_assessments
  FROM T_AU_PLAN_ENG g
  LEFT JOIN T_FAD_RISK_ASSESSMENT a ON a.eng_id=g.eng_id
     AND a.status='APPROVED'
 WHERE g.entity_type=6
 GROUP BY g.period_id
 ORDER BY g.period_id;

SELECT object_name,object_type,status FROM USER_OBJECTS
 WHERE object_name IN ('PKG_FAD_RISK','PKG_FAD_RISK_SCORE_V1',
   'PKG_FAD_RISK_ENGINE','V_FAD_RISK_ENGINE_CURRENT')
 ORDER BY object_name,object_type;
