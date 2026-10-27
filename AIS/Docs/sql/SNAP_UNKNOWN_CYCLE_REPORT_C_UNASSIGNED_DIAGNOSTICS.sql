-- IAS Audit Para Snap / Executive Reports: read-only diagnostics.
-- Run in an approved Oracle read-only session with the same date and authorized
-- audited-by/entity scope as the dashboard. NO updates or security changes.
--
-- CURRENT master COM_CYCLE is now the source for cycle distributions, even
-- when the selected End Date is historical. The historical open portfolio
-- remains an estimate and can contain currently settled PARA_STATUS=9 rows.
-- History is required only for last-movement fields, not for cycle values.
VAR P_START_DATE VARCHAR2(10);
VAR P_END_DATE VARCHAR2(10);
VAR P_AUDITED_BY_ID NUMBER;
EXEC :P_START_DATE := '2026-01-01';
EXEC :P_END_DATE := '2026-10-26';
EXEC :P_AUDITED_BY_ID := NULL;

-- 1. Every record that still qualifies for the Unknown cycle bucket.
--    A current COM_CYCLE of NULL or a negative value is an actual master
--    data issue. No history matching is used or required for this check.
--    Unlike a PARA_STATUS=8-only query, this includes status 9 records
--    that were open on the selected historical End Date.
WITH dates AS (
    SELECT TO_DATE(:P_START_DATE, 'YYYY-MM-DD') d_from,
           TO_DATE(:P_END_DATE, 'YYYY-MM-DD') + 1 d_to,
           CASE WHEN TO_DATE(:P_END_DATE, 'YYYY-MM-DD') = TRUNC(SYSDATE)
                THEN 1 ELSE 0 END is_today
      FROM dual
),
scoped AS (
    SELECT k.com_id, k.entity_id, k.audited_by_id, k.source_ind,
           k.para_status, k.com_cycle AS current_cycle, k.para_added_on,
           k.report_concluded_on, k.para_settled_on,
           CASE WHEN d.is_today = 1 THEN k.current_open
                WHEN k.para_added_on < d.d_to
                 AND (k.source_ind <> 'A' OR k.report_concluded_on IS NULL
                      OR k.report_concluded_on < d.d_to)
                 AND (k.para_status = 8 OR
                      (k.para_status = 9 AND k.para_settled_on >= d.d_to))
                THEN 1 ELSE 0 END AS snapshot_open
      FROM V_EXEC_KPI_LIFECYCLE k
      CROSS JOIN dates d
     WHERE k.record_type = 'PARA'
       AND (:P_AUDITED_BY_ID IS NULL OR k.audited_by_id = :P_AUDITED_BY_ID)
)
SELECT s.com_id, s.entity_id, e.name AS entity_name, s.audited_by_id,
       s.source_ind, s.para_status, s.current_cycle,
       s.para_added_on, s.report_concluded_on, s.para_settled_on,
       CASE WHEN s.current_cycle IS NULL THEN 'NULL MASTER COM_CYCLE'
            ELSE 'NEGATIVE MASTER COM_CYCLE' END AS diagnostic_reason
  FROM scoped s
  LEFT JOIN t_auditee_entities e ON e.entity_id = s.entity_id
 WHERE s.snapshot_open = 1
   AND (s.current_cycle IS NULL OR s.current_cycle < 0)
 ORDER BY s.audited_by_id, s.entity_id, s.com_id;

-- 2. REPORT C UNASSIGNED GM / REGION: list the underlying auditee entities,
--    not merely the grouping labels. Uses the same report source, B/D families,
--    dates and movements as PKG_EXEC_REPORTS OPEN_REPORT for Report C.
--    Restrict further to the exact authorized hierarchy if the dashboard uses
--    an entity/office/root scope; these queries do not bypass app authorization.
WITH dates AS (
    SELECT TO_DATE(:P_START_DATE, 'YYYY-MM-DD') d_from,
           TO_DATE(:P_END_DATE, 'YYYY-MM-DD') + 1 d_to
      FROM dual
),
report_c_source AS (
    SELECT s.entity_id, e.name AS entity_name, s.audit_type,
           s.audited_by, s.gm_office_id, s.gm_name, s.region_id,
           reg.name AS region_name, s.effective_added_on, s.close_on,
           CASE WHEN s.effective_added_on < d.d_from
                     AND (s.close_on IS NULL OR s.close_on >= d.d_from)
                THEN 1 ELSE 0 END AS opening_balance,
           CASE WHEN s.effective_added_on >= d.d_from
                     AND s.effective_added_on < d.d_to
                THEN 1 ELSE 0 END AS paras_added,
           CASE WHEN s.close_on >= d.d_from AND s.close_on < d.d_to
                     AND s.effective_added_on < d.d_to
                THEN 1 ELSE 0 END AS settled_paras,
           CASE WHEN s.effective_added_on < d.d_to
                     AND (s.close_on IS NULL OR s.close_on >= d.d_to)
                THEN 1 ELSE 0 END AS outstanding_paras
      FROM v_exec_report_source s
      CROSS JOIN dates d
      LEFT JOIN t_auditee_entities e ON e.entity_id = s.entity_id
      LEFT JOIN t_auditee_entities reg ON reg.entity_id = s.region_id
     WHERE s.effective_added_on < d.d_to
       AND s.audit_type IN ('B','D')
       AND (:P_AUDITED_BY_ID IS NULL OR s.audited_by = :P_AUDITED_BY_ID)
)
SELECT c.entity_id, c.entity_name, c.audit_type, c.audited_by,
       c.gm_office_id, c.gm_name, c.region_id, c.region_name,
       CASE WHEN c.gm_office_id IS NULL OR c.gm_name IS NULL
            THEN 'UNASSIGNED GM' END AS gm_issue,
       CASE WHEN c.region_id IS NULL OR c.region_name IS NULL
            THEN 'UNASSIGNED REGION' END AS region_issue,
       SUM(c.opening_balance) AS opening_balance,
       SUM(c.paras_added) AS paras_added,
       SUM(c.settled_paras) AS paras_settled,
       SUM(c.outstanding_paras) AS paras_outstanding,
       COUNT(*) AS source_para_rows
  FROM report_c_source c
 WHERE c.gm_office_id IS NULL OR c.gm_name IS NULL
    OR c.region_id IS NULL OR c.region_name IS NULL
 GROUP BY c.entity_id, c.entity_name, c.audit_type, c.audited_by,
          c.gm_office_id, c.gm_name, c.region_id, c.region_name
 ORDER BY c.audit_type, c.gm_name NULLS FIRST, c.region_name NULLS FIRST,
          c.entity_name, c.entity_id;

-- 3. For a specific ENTITY_ID returned above, inspect reporting levels and
--    active type-map cardinality. C_TYPE_ID=21 is how V_EXEC_ENTITY_SCOPE
--    currently identifies GM offices. Do not assume region code/type.
--    Replace the example ID with an actual returned ID before executing.
-- SELECT h.entity_id, h.entity_name,
--        h.reporting_1, h.reporting_2, h.reporting_3,
--        h.reporting_4, h.reporting_5, h.reporting_6
--   FROM V_IAS_ENTITY_REPORTING_NAMES h
--  WHERE h.entity_id = :P_ENTITY_ID;
--
-- SELECT m.entity_id, COUNT(DISTINCT m.c_type_id) AS distinct_active_types,
--        LISTAGG(DISTINCT TO_CHAR(m.c_type_id), ',')
--          WITHIN GROUP (ORDER BY TO_CHAR(m.c_type_id)) AS active_type_ids
--   FROM t_auditee_entities_maping m
--  WHERE NVL(m.status,'Y') = 'Y' AND m.entity_id = :P_ENTITY_ID
--  GROUP BY m.entity_id;
--
-- 4. Reconcile output to Report C and Snap using identical dates/scope. When
--    history is incomplete, retain UNKNOWN instead of manufacturing a cycle.
--    When GM/Region is legitimately out-of-hierarchy, choose an explicit
--    business classification with management approval before altering mappings.
