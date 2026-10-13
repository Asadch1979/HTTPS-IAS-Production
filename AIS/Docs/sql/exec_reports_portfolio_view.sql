-- Current-portfolio dimensions only. No historical status comes from this view.
-- Historical ownership/organization snapshots are unavailable in repository evidence.
CREATE OR REPLACE VIEW V_EXEC_REPORT_PORTFOLIO AS
WITH entity_types AS (
  SELECT entity_id, MAX(c_type_id) entity_type_id
  FROM T_AUDITEE_ENTITIES_MAPING WHERE NVL(status,'Y') = 'Y'
  GROUP BY entity_id HAVING COUNT(DISTINCT c_type_id) = 1
), regions AS (
  SELECT s.entity_id,
         MAX(x.office_id) KEEP (DENSE_RANK FIRST ORDER BY x.level_no) region_id
  FROM V_EXEC_ENTITY_SCOPE s
  CROSS APPLY (
    SELECT 0 level_no, s.entity_id office_id FROM dual UNION ALL
    SELECT 1, s.reporting_1 FROM dual UNION ALL SELECT 2, s.reporting_2 FROM dual UNION ALL
    SELECT 3, s.reporting_3 FROM dual UNION ALL SELECT 4, s.reporting_4 FROM dual UNION ALL
    SELECT 5, s.reporting_5 FROM dual UNION ALL SELECT 6, s.reporting_6 FROM dual
  ) x
  JOIN entity_types t ON t.entity_id = x.office_id AND t.entity_type_id = 5
  GROUP BY s.entity_id
)
SELECT p.com_id, p.para_source, p.old_para_id, p.new_para_id,
       p.para_added_on, p.entity_id, p.entity_type_id, p.audited_by,
       p.reporting_office_id, p.reporting_1, p.reporting_2, p.reporting_3,
       p.reporting_4, p.reporting_5, p.reporting_6,
       ot.entity_type_id office_group_id, et.entitytypedesc office_group_name,
       p.audited_by office_id, p.audited_by_name office_name,
       p.gm_office_id gm_id, p.gm_office_name gm_name,
       r.region_id, TO_CHAR(re.code) region_code, re.name region_name,
       COALESCE(p.division_id,p.business_group_id,p.entity_id) function_id,
       COALESCE(p.division_name,p.business_group_name,p.entity_name) function_name,
       CASE WHEN DECODE(p.audited_by,e.auditby_id,0,1) = 1 THEN 1 ELSE 0 END auditor_assignment_mismatch
FROM V_EXEC_PARA_BASE p
LEFT JOIN regions r ON r.entity_id = p.entity_id
LEFT JOIN T_AUDITEE_ENTITIES re ON re.entity_id = r.region_id
LEFT JOIN T_AUDITEE_ENTITIES e ON e.entity_id = p.entity_id
LEFT JOIN entity_types ot ON ot.entity_id = p.audited_by
LEFT JOIN T_AUDITEE_ENT_TYPES et ON et.autid = ot.entity_type_id;
/
