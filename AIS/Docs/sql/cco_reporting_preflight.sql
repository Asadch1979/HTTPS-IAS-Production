-- READ ONLY. DBA review required; execute only on explicitly approved staging.
-- Bind :start_date and :end_date as DATE; use same dates as authenticated CCO baseline.
-- Diagnostics operate under the approved DBA identity, not dashboard authorization.
SELECT autid, entitycode, entitytypedesc FROM t_auditee_ent_types ORDER BY autid;
SELECT entity_id, COUNT(*) rows_per_entity FROM v_exec_entity_scope
 GROUP BY entity_id HAVING COUNT(*) > 1;
SELECT entity_id, COUNT(DISTINCT c_type_id) type_count,
 COUNT(DISTINCT parent_id) parent_count, COUNT(DISTINCT gm_office) gm_count,
 COUNT(DISTINCT audit_by) audit_by_count
 FROM t_auditee_entities_maping WHERE NVL(status,'Y')='Y'
 GROUP BY entity_id HAVING COUNT(DISTINCT c_type_id)>1
 OR COUNT(DISTINCT parent_id)>1 OR COUNT(DISTINCT gm_office)>1
 OR COUNT(DISTINCT audit_by)>1;
SELECT m.entity_id, m.gm_office, g.type_id
 FROM t_auditee_entities_maping m LEFT JOIN t_auditee_entities g ON g.entity_id=m.gm_office
 WHERE NVL(m.status,'Y')='Y' AND m.gm_office IS NOT NULL
 AND (g.entity_id IS NULL OR g.type_id<>21);
SELECT m.audit_by, COUNT(DISTINCT m.entity_id) gm_candidates
 FROM t_auditee_entities_maping m JOIN t_auditee_entities g
 ON g.entity_id=m.entity_id AND g.type_id=21
 WHERE NVL(m.status,'Y')='Y' AND m.audit_by IS NOT NULL
 GROUP BY m.audit_by HAVING COUNT(DISTINCT m.entity_id)>1;
SELECT h.entity_id, h.business_group_id, h.division_id, h.gm_office_id,
 g.business_group_id gm_group, g.division_id gm_division
 FROM v_exec_entity_scope h LEFT JOIN v_exec_entity_scope g ON g.entity_id=h.gm_office_id
 WHERE h.business_group_id IS NULL OR h.division_id IS NULL OR h.gm_office_id IS NULL
 OR DECODE(h.business_group_id,g.business_group_id,0,1)=1
 OR DECODE(h.division_id,g.division_id,0,1)=1;
-- Raw parent self-loops and cycles; no forced path resolution.
SELECT entity_id, parent_id, CONNECT_BY_ISCYCLE cycle_flag
 FROM (SELECT DISTINCT entity_id,parent_id FROM t_auditee_entities_maping WHERE NVL(status,'Y')='Y')
 WHERE CONNECT_BY_ISCYCLE=1 OR entity_id=parent_id
 CONNECT BY NOCYCLE PRIOR parent_id=entity_id;
SELECT 'REPORT' source, COUNT(*) source_rows, COUNT(DISTINCT com_id) distinct_paras FROM v_exec_report_source
 UNION ALL SELECT 'LIFECYCLE',COUNT(*),COUNT(DISTINCT com_id) FROM v_exec_kpi_lifecycle;
SELECT com_id FROM v_exec_report_source MINUS SELECT com_id FROM v_exec_kpi_lifecycle;
SELECT com_id FROM v_exec_kpi_lifecycle MINUS SELECT com_id FROM v_exec_report_source;
SELECT COUNT(DISTINCT com_id) qualifying_paras,
 SUM(CASE WHEN effective_added_on<:start_date AND (close_on IS NULL OR close_on>=:start_date) THEN 1 ELSE 0 END) opening,
 SUM(CASE WHEN effective_added_on>=:start_date AND effective_added_on<:end_date+1 THEN 1 ELSE 0 END) added,
 SUM(CASE WHEN close_on>=:start_date AND close_on<:end_date+1 THEN 1 ELSE 0 END) settled,
 SUM(CASE WHEN close_on IS NULL OR close_on>=:end_date+1 THEN 1 ELSE 0 END) outstanding
 FROM v_exec_report_source WHERE effective_added_on<:end_date+1;
-- Compare authenticated B/C/D TOTAL rows and lifecycle separately; investigate
-- source/date/exclusion differences rather than treating these diagnostics as parity.
