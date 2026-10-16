-- CIA five executive KPIs and Nature of Issue Identified.
-- Oracle 18c. Run each SQL SELECT/PLSQL block independently using F8.
-- READ ONLY: no DML or DDL in this script.
-- Deploy spec/body first in an authorized non-production IAS schema.

-- 1. Compilation must return NO ROWS.
SELECT name, type, line, position, text
FROM user_errors
WHERE name = 'PKG_EXEC_DASHBOARD'
ORDER BY type, sequence;

-- 2. Five executive KPI values, selected audit year (change 2026 as needed).
-- Delayed Audit Reports / Awaiting Auditee Response return NULL until their
-- evidence-based statuses and deadlines are agreed. Never interpret NULL as 0.
DECLARE
  c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_COMMAND_KPIS(
    P_DASHBOARD_TYPE => 'CIA',
    P_ENTITY_ID => NULL,
    P_AUDIT_YEAR => 2026,
    IO_CURSOR => c
  );
  DBMS_SQL.RETURN_RESULT(c);
END;

-- 3. Nature: selected 2026 reporting period. Category 11 captures OTHER.
DECLARE
  c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE(
    P_DASHBOARD_TYPE => 'CIA',
    P_ENTITY_ID => NULL,
    P_START_DATE => DATE '2026-01-01',
    P_END_DATE => DATE '2026-10-15',
    IO_CURSOR => c
  );
  DBMS_SQL.RETURN_RESULT(c);
END;

-- 4. Independently check existing CIA issue total for SAME period/scope.
DECLARE
  c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUES(
    P_DASHBOARD_TYPE => 'CIA',
    P_ENTITY_ID => NULL,
    P_START_DATE => DATE '2026-01-01',
    P_END_DATE => DATE '2026-10-15',
    IO_CURSOR => c
  );
  DBMS_SQL.RETURN_RESULT(c);
END;

-- 5. Direct reconciled issue count. Must equal sum(categories 1..11),
-- and P_GET_CIA_ISSUES.ISSUE_IDENTIFIED (same date/scope).
SELECT COUNT(DISTINCT observation_id) AS source_eligible_issues
FROM v_exec_kpi_lifecycle
WHERE record_type = 'ISSUE'
  AND issue_eligible = 1
  AND issue_identified_on >= DATE '2026-01-01'
  AND issue_identified_on < DATE '2026-10-16';

-- 6. Review real checklist headings BEFORE approving 10 category mappings.
-- KYC/AML/ CFT and ACCOUNTING headings often differ from proposed labels.
SELECT ch.t_id, ch.heading, COUNT(DISTINCT o.id) AS observations
FROM t_au_observation o
LEFT JOIN t_audit_checklist_details d ON d.id = o.checklistdetail_id
LEFT JOIN t_audit_checklist_sub sub ON sub.s_id = d.s_id
LEFT JOIN t_audit_checklist ch ON ch.t_id = sub.t_id
JOIN t_au_plan_eng e ON e.eng_id = o.engplanid
WHERE e.status = 14
GROUP BY ch.t_id, ch.heading
ORDER BY observations DESC, ch.heading;

-- 7. Verify A1 annexure ID/code mapping (not risk severity ID = 1).
SELECT a.id, a.code, a.heading
FROM t_audit_checklist_annexure a
WHERE UPPER(TRIM(a.code)) = 'A1';

-- 8. Verify status list before certifying the plan coverage denominator.
SELECT EXTRACT(YEAR FROM per.start_date) AS audit_year, p.status,
       COUNT(*) AS planned_records,
       COUNT(DISTINCT p.entity_id) AS distinct_planned_entities
FROM t_au_plan p
JOIN t_au_period per ON per.auditperiodid = p.auditperiodid
WHERE EXTRACT(YEAR FROM per.start_date) = 2026
GROUP BY EXTRACT(YEAR FROM per.start_date), p.status
ORDER BY p.status;

-- 9. Status/ongoing-team evidence for the PROVISIONAL team KPI.
SELECT e.status AS engagement_status, t.status AS team_status,
       COUNT(DISTINCT e.eng_id) AS engagements,
       COUNT(DISTINCT t.team_id) AS teams
FROM t_au_plan_eng e
JOIN t_au_audit_teams t ON t.eng_id = e.eng_id
GROUP BY e.status, t.status
ORDER BY e.status, t.status;

-- 10. Pending-response stage distribution for defining a later REAL metric.
-- Current COM_STAGE alone must not be assumed to mean auditee owes response.
SELECT p.com_stage, p.com_status,
       COUNT(*) AS open_post_compliance_paras
FROM ais_t_au_post_compliance p
WHERE p.para_status = 8
GROUP BY p.com_stage, p.com_status
ORDER BY p.com_stage, p.com_status;

-- 11. Delayed-report verification depends on approved issuance SLA and date.
-- Find the actual report issuance milestone/approval workflow first; do not
-- use AUDIT_ENDDATE or STATUS 14 as an invented report issuance deadline.

-- 12. Access restriction: this must raise ORA-20101 (CIA-only).
DECLARE
  c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_CIA_COMMAND_KPIS(
    P_DASHBOARD_TYPE => 'TOP',
    P_ENTITY_ID => NULL,
    P_AUDIT_YEAR => 2026,
    IO_CURSOR => c
  );
END;
