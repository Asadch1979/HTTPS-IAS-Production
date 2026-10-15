-- Executive Reports A-E: initial base-data diagnostics (Oracle 18c)
-- READ ONLY. Execute each SELECT separately using F8.
-- This analysis does NOT change production objects or grant API permissions.
-- Goal: reconcile report family populations with approved A/O/C lifecycle
-- views before the Reports page is activated.

-- Q1: Audit-type attribution and sources (identifies whether B/D overlap
-- with the legacy Report E classification = D).
SELECT
    NVL(et.audit_type, 'UNMAPPED') AS audit_type,
    NVL(p.ind, 'NULL') AS source_ind,
    COUNT(*) AS total_paras,
    SUM(CASE WHEN p.para_status = 8 THEN 1 ELSE 0 END) AS open_paras,
    SUM(CASE WHEN p.para_status = 9 THEN 1 ELSE 0 END) AS settled_paras,
    SUM(CASE WHEN p.para_added_on IS NULL THEN 1 ELSE 0 END) AS missing_added_date
FROM ais_t_au_post_compliance p
LEFT JOIN t_auditee_ent_types et ON et.autid = p.entity_type_id
GROUP BY NVL(et.audit_type, 'UNMAPPED'), NVL(p.ind, 'NULL')
ORDER BY audit_type, source_ind;

-- Q2: Quantify structurally overlapping family populations.
-- Current implementation: A-D includes B,D; E includes D.
SELECT
    COUNT(*) AS all_master_paras,
    SUM(CASE WHEN et.audit_type IN ('B','D') THEN 1 ELSE 0 END)
      AS report_a_to_d_eligible,
    SUM(CASE WHEN et.audit_type = 'D' THEN 1 ELSE 0 END)
      AS report_e_eligible,
    SUM(CASE WHEN et.audit_type = 'D' THEN 1 ELSE 0 END)
      AS overlap_a_to_d_and_e,
    SUM(CASE WHEN et.audit_type NOT IN ('B','D') OR et.audit_type IS NULL
             THEN 1 ELSE 0 END) AS unclassified_by_current_reports
FROM ais_t_au_post_compliance p
LEFT JOIN t_auditee_ent_types et ON et.autid = p.entity_type_id;

-- Q3: Report-source membership versus lifecycle status-14 exclusions.
-- This is deliberately not filtered to any date window; it diagnoses
-- records present in the legacy report source that the approved lifecycle
-- source excludes from PARA_ADDED_ON.
SELECT
    k.source_ind,
    COUNT(*) AS lifecycle_para_rows,
    SUM(CASE WHEN k.excluded_pre_finalization = 1
             THEN 1 ELSE 0 END) AS excluded_before_conclusion,
    SUM(CASE WHEN k.excluded_pre_finalization = 1
               AND r.com_id IS NOT NULL
             THEN 1 ELSE 0 END) AS excluded_still_in_report_source,
    SUM(CASE WHEN k.para_added_on IS NOT NULL
             THEN 1 ELSE 0 END) AS eligible_lifecycle_additions,
    SUM(CASE WHEN r.com_id IS NOT NULL
             THEN 1 ELSE 0 END) AS rows_in_report_source
FROM v_exec_kpi_lifecycle k
LEFT JOIN v_exec_report_source r ON r.com_id = k.com_id
WHERE k.record_type = 'PARA'
GROUP BY k.source_ind
ORDER BY k.source_ind;

-- Q4: Compare each reporting population's count and current open baseline
-- before date-specific reconciliation. Report source excludes some records
-- through its own current-status and added-date filters.
SELECT
    NVL(et.audit_type, 'UNMAPPED') AS audit_type,
    COUNT(*) AS report_source_rows,
    COUNT(DISTINCT r.com_id) AS distinct_com_ids,
    SUM(CASE WHEN r.para_status = 8 THEN 1 ELSE 0 END) AS current_open,
    SUM(CASE WHEN r.para_status = 9 THEN 1 ELSE 0 END) AS current_settled,
    SUM(CASE WHEN r.close_on IS NULL AND r.para_status = 9
             THEN 1 ELSE 0 END) AS settled_without_effective_date
FROM v_exec_report_source r
LEFT JOIN t_auditee_ent_types et ON et.autid = r.entity_type_id
GROUP BY NVL(et.audit_type, 'UNMAPPED')
ORDER BY audit_type;

-- Q5: Check how many report source rows have no organizational mapping
-- and how many have no regional category 5 ancestor; do not silently
-- exclude these from the region ranking.
SELECT
    COUNT(*) AS report_source_rows,
    SUM(CASE WHEN r.reporting_office_id IS NULL
             THEN 1 ELSE 0 END) AS missing_reporting_office,
    SUM(CASE WHEN r.region_id IS NULL
             THEN 1 ELSE 0 END) AS missing_region,
    SUM(CASE WHEN r.gm_office_id IS NULL
             THEN 1 ELSE 0 END) AS missing_gm_office,
    SUM(CASE WHEN r.division_id IS NULL
             THEN 1 ELSE 0 END) AS missing_division,
    SUM(CASE WHEN r.auditor_assignment_mismatch = 1
             THEN 1 ELSE 0 END) AS auditor_assignment_mismatch
FROM v_exec_report_source r;

-- Q6: Confirm package/view compilation status (return no USER_ERRORS).
SELECT name, type, line, position, text
FROM user_errors
WHERE name IN ('PKG_EXEC_REPORTS', 'V_EXEC_REPORT_SOURCE')
ORDER BY name, type, sequence;
