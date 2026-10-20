-- READ-ONLY, run each SELECT separately in SQL Developer (F8).
-- Run only AFTER deploying V_EXEC_KPI_LIFECYCLE (canonical post-compliance
-- source), the compatibility views, and the PKG_EXEC_DASHBOARD body.
-- The 20-Oct-2026 observed baseline was:
-- ISSUE/A=83553, PARA/A=83553, PARA/C=1034, PARA/O=19681.
-- Counts may change with live audit activity; compare to same-time source.

-- 1: Compile status - no INVALID views/package errors expected.
SELECT object_name, object_type, status
  FROM user_objects
 WHERE object_name IN (
       'V_EXEC_KPI_LIFECYCLE','V_EXEC_KPI_SRC_A',
       'V_EXEC_KPI_SRC_O','V_EXEC_KPI_SRC_C','PKG_EXEC_DASHBOARD')
 ORDER BY object_name, object_type;

SELECT name, type, line, position, text
  FROM user_errors
 WHERE name IN (
       'V_EXEC_KPI_LIFECYCLE','V_EXEC_KPI_SRC_A',
       'V_EXEC_KPI_SRC_O','V_EXEC_KPI_SRC_C','PKG_EXEC_DASHBOARD')
 ORDER BY name, sequence;

-- 2: Source parity (four rows, full recognized fact population).
SELECT record_type, source_ind, COUNT(*) rows_returned
  FROM v_exec_kpi_lifecycle
 GROUP BY record_type, source_ind
 ORDER BY record_type, source_ind;

-- 3: Any duplicate ISSUE identity is an error; diagnostic only.
SELECT record_id, COUNT(*) duplicate_issue_count
  FROM v_exec_kpi_lifecycle
 WHERE record_type = 'ISSUE'
 GROUP BY record_id HAVING COUNT(*) > 1;

-- 4: Match PARA counts against the exact source register.
WITH src AS
(
    SELECT ind source_ind, COUNT(*) n
      FROM ais_t_au_post_compliance
     WHERE ind IN ('A','O','C')
     GROUP BY ind
),
view_counts AS
(
    SELECT source_ind, COUNT(*) n
      FROM v_exec_kpi_lifecycle
     WHERE record_type = 'PARA'
     GROUP BY source_ind
)
SELECT src.source_ind, src.n source_rows, NVL(v.n,0) view_rows,
       src.n - NVL(v.n,0) difference
  FROM src LEFT JOIN view_counts v ON v.source_ind = src.source_ind
 ORDER BY src.source_ind;

-- 5: Multiple post-compliance copies under one modern observation.
-- Reconciliation required if any row is returned.
SELECT new_para_id, COUNT(*) copies,
       MIN(NVL(issue_rectified,0)) min_flag,
       MAX(NVL(issue_rectified,0)) max_flag
  FROM ais_t_au_post_compliance
 WHERE ind = 'A' AND new_para_id IS NOT NULL
 GROUP BY new_para_id
HAVING COUNT(*) > 1
    OR MIN(NVL(issue_rectified,0)) <> MAX(NVL(issue_rectified,0));

-- 6: Incomplete/ambiguous AUDIT_PERIOD description resolution.
-- This is important for CIA Nature of Issue, which filters by PERIOD_ID.
SELECT p.audit_period, COUNT(*) modern_rows,
       COUNT(CASE WHEN l.period_id IS NULL THEN 1 END)
         AS unmapped_issue_rows
  FROM ais_t_au_post_compliance p
  JOIN v_exec_kpi_lifecycle l
    ON l.record_type='ISSUE'
   AND l.record_id=p.new_para_id
   AND l.source_ind='A'
 WHERE p.ind='A'
 GROUP BY p.audit_period
HAVING COUNT(CASE WHEN l.period_id IS NULL THEN 1 END) > 0
 ORDER BY modern_rows DESC;

SELECT UPPER(TRIM(description)) audit_period_label,
       COUNT(DISTINCT auditperiodid) period_ids
  FROM t_au_period
 WHERE description IS NOT NULL
 GROUP BY UPPER(TRIM(description))
HAVING COUNT(DISTINCT auditperiodid) > 1;

-- 7: All flagged rectifications should carry a usable event date.
SELECT COUNT(*) flagged_missing_rectification_date
  FROM ais_t_au_post_compliance
 WHERE ind='A' AND NVL(issue_rectified,0)=1
   AND setteled_on IS NULL;

-- 8: Date/activity classification without double-counting ISSUES as PARAS.
-- Bind the requested inclusive date range in SQL Developer.
SELECT
    COUNT(CASE WHEN record_type='ISSUE' AND issue_eligible=1
                AND issue_identified_on >= TRUNC(:P_FROM)
                AND issue_identified_on < TRUNC(:P_TO)+1
               THEN 1 END) AS issue_identified,
    COUNT(CASE WHEN record_type='ISSUE'
                AND issue_rectified_on >= TRUNC(:P_FROM)
                AND issue_rectified_on < TRUNC(:P_TO)+1
               THEN 1 END) AS issue_rectified,
    COUNT(CASE WHEN record_type='PARA'
                AND para_added_on >= TRUNC(:P_FROM)
                AND para_added_on < TRUNC(:P_TO)+1
               THEN 1 END) AS para_added,
    COUNT(CASE WHEN record_type='PARA'
                AND para_settled_on >= TRUNC(:P_FROM)
                AND para_settled_on < TRUNC(:P_TO)+1
               THEN 1 END) AS paras_settled,
    COUNT(CASE WHEN record_type='PARA'
                AND excluded_pre_finalization=1
               THEN 1 END) AS excluded_modern_paras
  FROM v_exec_kpi_lifecycle;

-- 9: Compare legacy SRC compatibility view counts with canonical PARA rows.
SELECT 'A' source_ind, COUNT(*) row_count FROM v_exec_kpi_src_a
UNION ALL SELECT 'O', COUNT(*) FROM v_exec_kpi_src_o
UNION ALL SELECT 'C', COUNT(*) FROM v_exec_kpi_src_c;

-- LIMITATIONS: 4/23 pre-transfer observations and historical conclusion
-- timestamps remain unknowable from this post-compliance-only fact source.
-- ISSUE_DROPPED deliberately returns NULL, not an invented zero.
-- Historical opening/closing and risk positions remain estimates.
-- Never use this script to update source data or relax authorized scope.
