-- Executive dashboard production reconciliation: READ ONLY, PL/SQL Developer F8.
-- Select/run ONE SELECT statement at a time. No SET/EXEC/VARIABLE/PROMPT.
-- Compare the current installed Oracle objects, not only repository SQL.

-- 1. Distinguish stored dates from displayed age. The first two IDs are
-- seen in the screenshot reporting 29,146 days, but are not proof that
-- PARA_ADDED_ON is wrong. Do not UPDATE source dates by inference.
SELECT c.com_id, c.audit_period, c.para_no, c.ind,
       c.para_status, c.risk, c.com_cycle,
       TO_CHAR(c.para_added_on,'YYYY-MM-DD') AS stored_para_added_on,
       TO_CHAR(c.setteled_on,'YYYY-MM-DD') AS stored_settled_on,
       TRUNC(SYSDATE) - TRUNC(c.para_added_on) AS source_elapsed_days,
       p.aging_days AS displayed_age_days,
       p.aging_quality, p.aging_bucket, p.is_critical
  FROM ais_t_au_post_compliance c
  LEFT JOIN v_exec_para_base p ON p.com_id=c.com_id
 WHERE c.com_id IN (61581,61582,19030,3463)
 ORDER BY c.com_id;

-- 2. Find exceptional chronology among open paras. No arbitrary maximum
-- life is imposed on legacy paras; inspect source date + audit year.
SELECT p.com_id,p.audit_period,p.para_no,p.para_source,
       p.para_added_on,p.aging_days,p.aging_quality,p.risk_id
  FROM v_exec_para_base p
 WHERE p.is_open=1
   AND p.aging_days > 20000
   AND ROWNUM <= 100
 ORDER BY p.aging_days DESC;

-- 3. Before AND after definition change: current high risk MUST equal
-- critical count, because ZTBL uses only risk=1 for open critical paras.
SELECT COUNT(*) AS all_cases,
       SUM(CASE WHEN is_open=1 THEN 1 ELSE 0 END) open_paras,
       SUM(CASE WHEN is_open=1 AND risk_id=1 THEN 1 ELSE 0 END) high_risk_open,
       SUM(is_critical) critical_count,
       SUM(CASE WHEN is_critical=1 AND (is_open<>1 OR NVL(risk_id,0)<>1)
                THEN 1 ELSE 0 END) invalid_critical_classification
  FROM v_exec_para_base;

-- 4. Date filtering MUST be verified against lifecycle cursor for the
-- same bank-wide TOP scope and same selected start/end dates.
-- Historical opening / closing may be estimated, not a certified ledger.
-- Run separately in F8 with DBMS Output panel enabled if desired;
-- DBMS_SQL.RETURN_RESULT can be used where the client supports it.
DECLARE
  v_cur SYS_REFCURSOR;
BEGIN
  PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE(
    P_DASHBOARD_TYPE=>'TOP',
    P_ENTITY_ID=>NULL,
    P_START_DATE=>DATE '2026-01-01',
    P_END_DATE=>DATE '2026-10-18',
    IO_CURSOR=>v_cur);
  DBMS_SQL.RETURN_RESULT(v_cur);
END;

-- 5. Confirm the new Group hierarchy is data-backed, rather than
-- assuming the activation placeholder means there are no records.
-- Replace 112918 only if the DBA confirms another authorized Group root;
-- example 112918 was supplied for the RLM page, not verified as a Group.
SELECT entity_id,name
  FROM t_auditee_entities
 WHERE entity_id=112918;

-- 6. Verify installed objects VALID before any code-driven pilot test.
SELECT object_name,object_type,status
  FROM user_objects
 WHERE object_name IN ('PKG_EXEC_DASHBOARD','V_EXEC_PARA_BASE',
                       'V_EXEC_KPI_LIFECYCLE','PKG_EXEC_REPORTS')
 ORDER BY object_name,object_type;

-- 7. Check access grants for new dashboards before removing app-level
-- environment switches. Super User is IAS role/group 1 only for pilot.
SELECT a.controller_name,a.action_name,a.api_path,a.is_active api_active,
       g.role_id,g.is_active grant_active
  FROM t_au_api_master a
  LEFT JOIN t_au_role_api_permission g ON g.api_id=a.api_id
 WHERE UPPER(a.controller_name) IN
       ('EXECUTIVEDASHBOARD','AUDITPARASNAP')
   AND UPPER(a.http_method)='GET'
 ORDER BY a.controller_name,a.action_name,g.role_id;
