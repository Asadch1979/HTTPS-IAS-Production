-- Executive Reports A-E: image-reference checks; Oracle 18c.
-- F8 each statement separately. No SQL*Plus PROMPT or slash.
-- Deploy V_EXEC_REPORT_SOURCE_image_rewrite.sql first;
-- then PKG_EXEC_REPORTS_image_rewrite_spec.sql and _body.sql.
-- This script is READ-ONLY.

-- 1: Compilation errors (expected none).
SELECT name, type, line, position, text
FROM user_errors
WHERE name IN ('V_EXEC_REPORT_SOURCE','PKG_EXEC_REPORTS')
ORDER BY name, type, sequence;

-- 2: View COM_ID uniqueness (expected no rows).
SELECT com_id, COUNT(*) AS count_rows
FROM v_exec_report_source
GROUP BY com_id
HAVING COUNT(*) <> 1;

-- 3: Modern dispositions before Status 14 must not appear in report source.
-- Expected EXCLUDED_VISIBLE = 0; source A rows may have non-null
-- EFFECTIVE_ADDED_ON later than copied PARA_ADDED_ON.
SELECT
  COUNT(*) AS source_a_paras_in_lifecycle,
  SUM(CASE WHEN k.excluded_pre_finalization = 1 THEN 1 ELSE 0 END)
    AS total_excluded_before_conclusion,
  SUM(CASE WHEN k.excluded_pre_finalization = 1 AND s.com_id IS NOT NULL
           THEN 1 ELSE 0 END) AS excluded_visible_in_report,
  SUM(CASE WHEN k.excluded_pre_finalization = 0 AND s.com_id IS NOT NULL
           AND s.report_concluded_on > s.para_added_on
           AND s.effective_added_on = s.report_concluded_on
           THEN 1 ELSE 0 END) AS shifted_to_conclusion
FROM v_exec_kpi_lifecycle k
LEFT JOIN v_exec_report_source s ON s.com_id = k.com_id
WHERE k.record_type = 'PARA' AND k.source_ind = 'A';

-- 4: AUDIT_TYPE B/D source attribution used by original FAD package.
-- Report A-D currently include B+D, Report E currently D;
-- these are potentially overlapping populations, DO NOT certify
-- Field/Management totals before report-owner confirms mapping.
SELECT
  NVL(audit_type,'UNMAPPED') AS audit_type,
  NVL(source_ind,'UNMAPPED') AS source_ind,
  COUNT(*) AS total_cases,
  SUM(CASE WHEN para_status=8 THEN 1 ELSE 0 END) AS open_cases,
  SUM(CASE WHEN para_status=9 THEN 1 ELSE 0 END) AS settled_cases
FROM v_exec_report_source
GROUP BY NVL(audit_type,'UNMAPPED'),NVL(source_ind,'UNMAPPED')
ORDER BY audit_type,source_ind;

-- 5: Settlement-source classification diagnostic.
-- COM_STATUS=16 history alone is NOT a certified status transition.
SELECT
  COUNT(*) AS settled_with_date,
  SUM(CASE WHEN TRUNC(history_close_on)=TRUNC(close_on)
                 AND history_close_stage IN (41,45) THEN 1 ELSE 0 END)
     AS settled_by_compliance_estimate,
  SUM(CASE WHEN TRUNC(history_close_on)=TRUNC(close_on)
                 AND history_close_stage IS NOT NULL
                 AND history_close_stage NOT IN (41,45) THEN 1 ELSE 0 END)
     AS settled_by_audit_estimate,
  SUM(CASE WHEN history_close_on IS NULL
                 OR TRUNC(history_close_on)<>TRUNC(close_on)
                 OR history_close_stage IS NULL THEN 1 ELSE 0 END)
     AS settled_unknown
FROM v_exec_report_source
WHERE close_on IS NOT NULL;

-- 6: Report A - annual field progress (images 01 and 06).
DECLARE c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_REPORTS.P_ANNUAL_FIELD_PROGRESS(
    'TOP',NULL,DATE '2023-01-01',DATE '2026-09-30',
    NULL,NULL,NULL,NULL,c);
  DBMS_SQL.RETURN_RESULT(c);
END;

-- 7: Report B - offices, sub totals and grand total (image 02).
DECLARE c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_REPORTS.P_FIELD_OFFICE_POSITION(
    'TOP',NULL,DATE '2026-01-01',DATE '2026-09-30',
    NULL,NULL,NULL,NULL,c);
  DBMS_SQL.RETURN_RESULT(c);
END;

-- 8: Report C - GM/Region totals (image 03).
DECLARE c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_REPORTS.P_GM_REGION_POSITION(
    'TOP',NULL,DATE '2026-01-01',DATE '2026-09-30',
    NULL,NULL,NULL,NULL,c);
  DBMS_SQL.RETURN_RESULT(c);
END;

-- 9: Report D - TOP and BOTTOM regional rankings (image 04).
DECLARE c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_REPORTS.P_REGION_RANKINGS(
    'TOP',NULL,DATE '2026-01-01',DATE '2026-09-21',
    NULL,NULL,NULL,NULL,c);
  DBMS_SQL.RETURN_RESULT(c);
END;

-- 10: Report E - division/group management functions (image 05).
DECLARE c SYS_REFCURSOR;
BEGIN
  PKG_EXEC_REPORTS.P_MANAGEMENT_SETTLEMENT(
    'TOP',NULL,DATE '2026-01-01',DATE '2026-09-21',
    NULL,NULL,NULL,NULL,c);
  DBMS_SQL.RETURN_RESULT(c);
END;
