-- Read-only verification for CIA checklist-root Nature of Issue mapping.
-- Run each SELECT separately in IAS Development (F8). Requires compiled
-- V_IAS_CIA_ISSUE_CHECKLIST_ROOT and the seeded CHECKLIST_ROOT mapping rows.

-- 1. Table constraints + newly configured source IDs
SELECT constraint_name, status, search_condition_vc
  FROM user_constraints
 WHERE table_name = 'T_IAS_CIA_ISSUE_NATURE_MAP'
   AND constraint_name IN ('CK_IAS_CIA_NATURE_SOURCE',
                            'CK_IAS_CIA_NATURE_CATEGORY');

SELECT m.source_type,m.source_id,c.heading,m.category_code,
       m.match_priority,m.is_active
  FROM t_ias_cia_issue_nature_map m
  LEFT JOIN t_audit_checklist c ON c.t_id=m.source_id
 WHERE m.source_type='CHECKLIST_ROOT'
 ORDER BY m.source_id;

-- 2. No duplicated source observations from checklist hierarchy.
SELECT COUNT(*) observation_rows,COUNT(DISTINCT observation_id) unique_observations
  FROM v_ias_cia_issue_checklist_root;

SELECT observation_id,COUNT(*) duplicates
  FROM v_ias_cia_issue_checklist_root
 GROUP BY observation_id HAVING COUNT(*)>1;

-- 3. User-supplied period 5 all-status raw counts: expected sum 14,915
-- from eight observed master roots, independent of conclusion/eligibility.
SELECT c.checklist_root_id,t.heading,
       COUNT(*) all_observations,
       COUNT(DISTINCT c.observation_id) unique_observations
  FROM v_ias_cia_issue_checklist_root c
  JOIN t_au_observation o ON o.id=c.observation_id
  JOIN t_au_plan_eng e ON e.eng_id=o.engplanid
  LEFT JOIN t_audit_checklist t ON t.t_id=c.checklist_root_id
 WHERE e.period_id=5
 GROUP BY c.checklist_root_id,t.heading
 ORDER BY c.checklist_root_id;

-- 4. Exact CIA ISSUE eligibility, IAS-period boundaries and root population.
-- Replace :AUDITPERIODID with a real T_AU_PERIOD.AUDITPERIODID.
SELECT c.checklist_root_id,t.heading,
       COUNT(DISTINCT k.observation_id) eligible_issues
  FROM v_exec_kpi_lifecycle k
  LEFT JOIN v_ias_cia_issue_checklist_root c
    ON c.observation_id=k.observation_id
  LEFT JOIN t_audit_checklist t ON t.t_id=c.checklist_root_id
  JOIN t_au_period p ON p.auditperiodid=k.period_id
 WHERE k.record_type='ISSUE'
   AND k.issue_eligible=1
   AND k.period_id=:AUDITPERIODID
   AND k.issue_identified_on>=TRUNC(p.start_date)
   AND k.issue_identified_on<TRUNC(p.end_date)+1
 GROUP BY c.checklist_root_id,t.heading
 ORDER BY c.checklist_root_id;

-- 5. Raw eligible count MUST match sum from P_GET_CIA_ISSUE_NATURE
-- for the same period and identical optional scope filters (NULL here).
SELECT COUNT(DISTINCT k.observation_id) total_eligible_issues
  FROM v_exec_kpi_lifecycle k
  JOIN t_au_period p ON p.auditperiodid=k.period_id
 WHERE k.record_type='ISSUE'
   AND k.issue_eligible=1
   AND k.period_id=:AUDITPERIODID
   AND k.issue_identified_on>=TRUNC(p.start_date)
   AND k.issue_identified_on<TRUNC(p.end_date)+1;

-- 6. Call the authoritative existing package cursor; must give ten rows.
VAR nature_rc REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE(
    P_DASHBOARD_TYPE=>'CIA',
    P_ENTITY_ID=>NULL,
    P_AUDIT_PERIOD_ID=>:AUDITPERIODID,
    IO_CURSOR=>:nature_rc);
END;
/
PRINT nature_rc;

-- 7. All objects valid; package body has no compilation errors.
SELECT object_name,object_type,status
  FROM user_objects
 WHERE object_name IN
  ('V_IAS_CIA_ISSUE_CHECKLIST_ROOT','V_EXEC_KPI_LIFECYCLE',
   'V_EXEC_PARA_BASE','V_EXEC_REPORT_SOURCE','PKG_EXEC_DASHBOARD')
 ORDER BY object_name,object_type;

SELECT name,type,line,position,text
  FROM user_errors
 WHERE name IN ('V_IAS_CIA_ISSUE_CHECKLIST_ROOT','PKG_EXEC_DASHBOARD')
 ORDER BY name,sequence;
