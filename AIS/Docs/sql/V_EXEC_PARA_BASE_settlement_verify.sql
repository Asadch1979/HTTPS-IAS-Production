-- IAS executive dashboards: read-only verification after recompiling
-- V_EXEC_PARA_BASE from PKG_EXEC_DASHBOARD.sql.
-- Compare the raw post-compliance register to the view consumed by
-- P_GET_KPI_SUMMARY, P_GET_SETTLEMENT_TREND, P_GET_ROLE_UNIT_COMPARISON,
-- P_GET_ROLE_ENTITY_HEATMAP, and related dashboards.
--
-- Verified source baseline supplied for AUDITED_BY 112242 + 112248:
-- 01-Jan-2026..20-Oct-2026:
-- Para Added 38; genuinely Settled 461; added+settled 11;
-- pre-conclusion rectified rows whose settlement date is in range 86.
-- These are NOT independent observations for all possible FAD departments.
-- Adjust the two DATE literals when checking a different range.
--
WITH date_params AS
(
  SELECT DATE '2026-01-01' AS date_from,
         DATE '2026-10-21' AS date_to
    FROM dual
),
raw_totals AS
(
  SELECT c.audited_by,
         COUNT(CASE WHEN c.para_status IN (8,9)
                      AND c.para_added_on >= d.date_from
                      AND c.para_added_on < d.date_to
                      AND CASE WHEN c.ind = 'A' AND NVL(c.issue_rectified,0)=1
                               THEN 0 ELSE 1 END = 1
                    THEN c.com_id END) expected_added,
         COUNT(CASE WHEN c.para_status = 9
                      AND c.setteled_on >= d.date_from
                      AND c.setteled_on < d.date_to
                      AND CASE WHEN c.ind = 'A' AND NVL(c.issue_rectified,0)=1
                               THEN 0 ELSE 1 END = 1
                    THEN c.com_id END) expected_settled,
         COUNT(CASE WHEN c.para_status = 9
                      AND c.para_added_on >= d.date_from
                      AND c.para_added_on < d.date_to
                      AND c.setteled_on >= d.date_from
                      AND c.setteled_on < d.date_to
                      AND CASE WHEN c.ind = 'A' AND NVL(c.issue_rectified,0)=1
                               THEN 0 ELSE 1 END = 1
                    THEN c.com_id END) expected_same_period,
         COUNT(CASE WHEN c.ind = 'A'
                      AND NVL(c.issue_rectified,0) = 1
                      AND c.para_status = 9
                      AND c.setteled_on >= d.date_from
                      AND c.setteled_on < d.date_to
                    THEN c.com_id END) excluded_preconclusion_settled
    FROM ais_t_au_post_compliance c CROSS JOIN date_params d
   WHERE c.audited_by IN (112242,112248)
   GROUP BY c.audited_by
),
view_totals AS
(
  SELECT v.audited_by,
         COUNT(CASE WHEN v.para_status IN (8,9)
                      AND v.para_added_on >= d.date_from
                      AND v.para_added_on < d.date_to
                    THEN v.com_id END) view_added,
         COUNT(CASE WHEN v.is_settled = 1
                      AND v.setteled_on >= d.date_from
                      AND v.setteled_on < d.date_to
                    THEN v.com_id END) view_settled,
         COUNT(CASE WHEN v.is_settled = 1
                      AND v.para_added_on >= d.date_from
                      AND v.para_added_on < d.date_to
                      AND v.setteled_on >= d.date_from
                      AND v.setteled_on < d.date_to
                    THEN v.com_id END) view_same_period
    FROM v_exec_para_base v CROSS JOIN date_params d
   WHERE v.audited_by IN (112242,112248)
   GROUP BY v.audited_by
),
results AS
(
  SELECT r.audited_by,
         r.expected_added, NVL(v.view_added,0) view_added,
         r.expected_settled, NVL(v.view_settled,0) view_settled,
         r.expected_same_period, NVL(v.view_same_period,0) view_same_period,
         r.excluded_preconclusion_settled
    FROM raw_totals r
    LEFT JOIN view_totals v ON v.audited_by = r.audited_by
)
SELECT CASE WHEN GROUPING(audited_by)=1
            THEN 'SELECTED FAD DEPARTMENTS'
            ELSE TO_CHAR(audited_by) END audit_department,
       SUM(expected_added) source_added,
       SUM(view_added) base_added,
       SUM(expected_settled) source_settled,
       SUM(view_settled) base_settled,
       SUM(expected_same_period) source_same_period,
       SUM(view_same_period) base_same_period,
       SUM(excluded_preconclusion_settled) excluded_rectification_rows,
       CASE WHEN SUM(expected_added)=SUM(view_added)
                  AND SUM(expected_settled)=SUM(view_settled)
                  AND SUM(expected_same_period)=SUM(view_same_period)
            THEN 'MATCH' ELSE 'MISMATCH' END verification_status
  FROM results
 GROUP BY ROLLUP(audited_by)
 ORDER BY GROUPING(audited_by) DESC, audited_by;
 
-- Report A-E source is intentionally not modified in this change:
-- its view V_EXEC_REPORT_SOURCE is not defined in the repository.
-- Ask DBA to check the live DDL via DBMS_METADATA.GET_DDL before
-- adjusting its CLOSE_ON/EFFECTIVE_ADDED_ON interpretation.
