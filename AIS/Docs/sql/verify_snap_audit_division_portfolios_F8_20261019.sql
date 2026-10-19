-- Audit Para Snap Division / Department-Cluster preflight.
-- READ ONLY, PL/SQL Developer: select ONE statement and press F8.
-- No guessing of live parent IDs, department membership or cluster labels.
-- Two proposed user labels: Field Audit Division / Management Audit Division.

-- A. Real audited-by departments and entity labels. These IDs are what
-- V_EXEC_KPI_LIFECYCLE.AUDITED_BY_ID identifies (not business reporting IDs).
SELECT d.entity_id AS auditby_id, d.deptname AS department_name,
       d.status AS department_status,
       e.name AS entity_directory_name
  FROM t_audit_departments d
  LEFT JOIN t_auditee_entities e ON e.entity_id = d.entity_id
 ORDER BY UPPER(d.deptname), d.entity_id;

-- B. Field Audit reporting mapping. Review candidate parent/child
-- relationships; do not assume all descendants are audit portfolios.
SELECT f.parent_id AS parent_id,
       parent.name AS parent_name,
       f.entity_id AS child_auditby_id,
       child.name AS child_entity_name,
       dep.deptname AS child_audit_department
  FROM t_auditee_entities_maping_fad f
  LEFT JOIN t_auditee_entities parent ON parent.entity_id=f.parent_id
  LEFT JOIN t_auditee_entities child ON child.entity_id=f.entity_id
  LEFT JOIN t_audit_departments dep ON dep.entity_id=f.entity_id
 ORDER BY f.parent_id, f.entity_id;

-- C. General IAS organizational mapping: check whether the actual
-- 'Management Audit Division' exists as a parent. Business hierarchy
-- alone cannot be assumed to confer an audit-portfolio assignment.
SELECT m.parent_id AS parent_id, parent.name AS parent_name,
       m.entity_id AS child_id, child.name AS child_name,
       d.deptname AS audit_department_name
  FROM t_auditee_entities_maping m
  LEFT JOIN t_auditee_entities parent ON parent.entity_id=m.parent_id
  LEFT JOIN t_auditee_entities child ON child.entity_id=m.entity_id
  LEFT JOIN t_audit_departments d ON d.entity_id=m.entity_id
 WHERE UPPER(NVL(parent.name,'?')) LIKE '%AUDIT%'
 ORDER BY m.parent_id, m.entity_id;

-- D. Actual audit portfolio data. Determine which departments/clusters
-- have audit engagements and whether proposed grouping covers all of them.
SELECT e.auditby_id,
       d.deptname AS audit_department_name,
       COUNT(DISTINCT e.eng_id) AS engagement_count,
       MIN(e.audit_startdate) AS first_start_date,
       MAX(e.audit_enddate) AS last_audit_end_date
  FROM t_au_plan_eng e
  LEFT JOIN t_audit_departments d ON d.entity_id=e.auditby_id
 GROUP BY e.auditby_id,d.deptname
 ORDER BY engagement_count DESC,e.auditby_id;

-- E. Historical KPI base audit-by values (distinct from engagement count).
SELECT k.audited_by_id,
       d.deptname AS audit_department_name,
       k.record_type, COUNT(*) AS lifecycle_rows
  FROM v_exec_kpi_lifecycle k
  LEFT JOIN t_audit_departments d ON d.entity_id=k.audited_by_id
 GROUP BY k.audited_by_id,d.deptname,k.record_type
 ORDER BY k.audited_by_id,k.record_type;

-- Confirm the exact membership of BOTH parent divisions with the DBA.
-- Do not deploy the new division ALL filter until every eligible
-- AUDITED_BY_ID has been assigned unambiguously and all reports A-E
-- use the same server-enforced filter. Filtering only the dropdown
-- UI would not restrict any actual Oracle results.
