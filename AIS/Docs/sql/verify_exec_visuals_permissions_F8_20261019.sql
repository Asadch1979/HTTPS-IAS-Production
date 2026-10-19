-- Read-only Production/Development reconciliation, PL/SQL Developer F8.
-- Run ONE SELECT at a time. No SQL*Plus commands, DML or COMMIT.
-- 1. Bank-wide MTD denominator check. Compare with Settlement & Performance.
--    MTD rate is a reconstructed month-opening workload, not lifetime ratio.
WITH x AS
(
  SELECT NVL(SUM(CASE WHEN p.is_settled=1
                        AND p.setteled_on>=TRUNC(SYSDATE,'MM')
                        AND p.setteled_on<ADD_MONTHS(TRUNC(SYSDATE,'MM'),1)
                       THEN 1 ELSE 0 END),0) settled_mtd,
         NVL(SUM(CASE WHEN p.para_added_on < TRUNC(SYSDATE,'MM')
                        AND (p.is_open=1 OR
                          (p.is_settled=1 AND p.setteled_on>=TRUNC(SYSDATE,'MM')))
                       THEN 1 ELSE 0 END),0) opening_workload,
         NVL(SUM(CASE WHEN p.para_added_on >= TRUNC(SYSDATE,'MM')
                        AND p.para_added_on < ADD_MONTHS(TRUNC(SYSDATE,'MM'),1)
                        AND (p.is_open=1 OR p.is_settled=1)
                       THEN 1 ELSE 0 END),0) added_mtd,
         NVL(SUM(p.is_settled),0) lifetime_settled,
         NVL(SUM(p.is_open),0) current_open
    FROM V_EXEC_PARA_BASE p
)
SELECT settled_mtd,opening_workload,added_mtd,
       opening_workload+added_mtd AS mtd_workload,
       ROUND(100*settled_mtd/NULLIF(opening_workload+added_mtd,0),2) AS mtd_settlement_rate,
       ROUND(100*lifetime_settled/NULLIF(current_open+lifetime_settled,0),2) AS lifetime_ratio
  FROM x;

-- 2. Selected period sample: Nov-2025 to Oct-2026. Compare with
--    Snap settlement trend "All settled" and "High-risk settled" series.
SELECT TO_CHAR(TRUNC(k.para_settled_on,'MM'),'YYYY-MM') AS settlement_month,
       COUNT(*) AS settled_paras,
       SUM(CASE WHEN k.risk_id=1 THEN 1 ELSE 0 END) AS high_risk_settled
  FROM V_EXEC_KPI_LIFECYCLE k
 WHERE k.record_type='PARA'
   AND k.para_settled_on>=DATE '2025-11-01'
   AND k.para_settled_on<DATE '2026-11-01'
 GROUP BY TRUNC(k.para_settled_on,'MM')
 ORDER BY settlement_month;

-- 3. Check risks excluded from display, NOT silently deleted from IAS.
SELECT NVL(TO_CHAR(p.risk_id),'UNRATED') AS risk_id,
       COUNT(*) AS open_paras
  FROM V_EXEC_PARA_BASE p
 WHERE p.is_open=1
 GROUP BY NVL(TO_CHAR(p.risk_id),'UNRATED')
 ORDER BY risk_id;

-- 4. The dedicated executive RLM parent must have its own menu page.
SELECT id,page_name,page_path,status,hide_menu
  FROM T_MENU_PAGES
 WHERE id IN (1000300,1000301,1000302,1000303,1000304,1000305)
 ORDER BY id;

-- 5. After manifest migration: exactly the assigned API dependencies
--    for each executive page. Run only once T_IAS_EXEC_PAGE_API exists.
SELECT p.page_id,m.controller_name,m.action_name,
       m.api_path,m.is_active AS api_master_active
  FROM T_IAS_EXEC_PAGE_API p
  JOIN T_AU_API_MASTER m ON m.api_id=p.api_id
 ORDER BY p.page_id,m.controller_name,m.action_name;

-- 6. Before wider rollout check non-Super User active API grants.
--    The Super User preview must not be relaxed by an SQL migration.
SELECT m.controller_name,m.action_name,g.role_id,g.is_active
  FROM T_AU_API_MASTER m
  JOIN T_AU_ROLE_API_PERMISSION g ON g.api_id=m.api_id
 WHERE UPPER(m.controller_name) IN
      ('EXECUTIVEDASHBOARD','AUDITPARASNAP','EXECUTIVEREPORTS')
   AND g.is_active='Y'
 ORDER BY m.controller_name,m.action_name,g.role_id;
