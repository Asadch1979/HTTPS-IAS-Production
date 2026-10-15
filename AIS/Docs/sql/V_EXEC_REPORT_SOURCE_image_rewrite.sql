-- Executive Reports A-E: approved Status-14-aligned source.
-- Oracle 18c. Execute as one CREATE OR REPLACE VIEW using F8.
-- Uses the verified V_EXEC_KPI_LIFECYCLE; does NOT count Source-A
-- observations disposed before conclusion. Preserve one row per COM_ID.
-- AUDIT_TYPE B/D family mapping remains provisional until DB owner validation.
CREATE OR REPLACE VIEW V_EXEC_REPORT_SOURCE AS
WITH hist AS (
    SELECT h.com_id,
           MAX(h.comment_on) AS last_movement_on,
           MAX(CASE WHEN h.com_status = 16 THEN h.comment_on END) AS history_close_on,
           MAX(CASE WHEN h.com_status = 16 THEN h.com_stage END)
             KEEP (DENSE_RANK LAST ORDER BY
                CASE WHEN h.com_status = 16 THEN h.comment_on
                     ELSE DATE '0001-01-01' END,
                CASE WHEN h.com_status = 16 THEN h.hist_id ELSE -1 END)
             AS history_close_stage
    FROM ais_t_au_post_compliance_history h
    GROUP BY h.com_id
), types AS (
    SELECT entity_id, MAX(c_type_id) AS c_type_id
    FROM t_auditee_entities_maping
    WHERE NVL(status, 'Y') = 'Y'
    GROUP BY entity_id
    HAVING COUNT(DISTINCT c_type_id) = 1
)
SELECT
    k.com_id,
    k.source_ind,
    k.entity_id,
    k.entity_type_id,
    k.audited_by_id AS audited_by,
    et.audit_type,
    k.para_added_on,
    k.report_concluded_on,
    CASE
      WHEN k.source_ind = 'A'
       AND k.report_concluded_on IS NOT NULL
       AND k.report_concluded_on > k.para_added_on
      THEN k.report_concluded_on
      ELSE k.para_added_on
    END AS effective_added_on,
    k.para_settled_on AS setteled_on,
    k.para_status,
    k.risk_id AS risk,
    k.com_cycle,
    CASE WHEN k.para_status = 9 THEN k.para_settled_on END AS close_on,
    h.history_close_on,
    h.history_close_stage,
    h.last_movement_on,
    s.reporting_office_id,
    s.gm_office_id,
    s.division_id,
    s.business_group_id,
    s.reporting_1,
    s.reporting_2,
    s.reporting_3,
    s.reporting_4,
    s.reporting_5,
    s.reporting_6,
    ro.name AS reporting_office_name,
    gm.name AS gm_name,
    dv.name AS division_name,
    bg.name AS group_name,
    au.name AS audited_by_name,
    CASE
      WHEN t0.c_type_id = 5 THEN s.entity_id
      WHEN t1.c_type_id = 5 THEN s.reporting_1
      WHEN t2.c_type_id = 5 THEN s.reporting_2
      WHEN t3.c_type_id = 5 THEN s.reporting_3
      WHEN t4.c_type_id = 5 THEN s.reporting_4
      WHEN t5.c_type_id = 5 THEN s.reporting_5
      WHEN t6.c_type_id = 5 THEN s.reporting_6
    END AS region_id,
    CASE WHEN NVL(k.audited_by_id, -1) = NVL(ae.auditby_id, -1)
         THEN 0 ELSE 1 END AS auditor_assignment_mismatch
FROM v_exec_kpi_lifecycle k
LEFT JOIN v_exec_entity_scope s ON s.entity_id = k.entity_id
LEFT JOIN t_auditee_ent_types et ON et.autid = k.entity_type_id
LEFT JOIN t_auditee_entities ae ON ae.entity_id = k.entity_id
LEFT JOIN t_auditee_entities ro ON ro.entity_id = s.reporting_office_id
LEFT JOIN t_auditee_entities gm ON gm.entity_id = s.gm_office_id
LEFT JOIN t_auditee_entities dv ON dv.entity_id = s.division_id
LEFT JOIN t_auditee_entities bg ON bg.entity_id = s.business_group_id
LEFT JOIN t_auditee_entities au ON au.entity_id = k.audited_by_id
LEFT JOIN hist h ON h.com_id = k.com_id
LEFT JOIN types t0 ON t0.entity_id = s.entity_id
LEFT JOIN types t1 ON t1.entity_id = s.reporting_1
LEFT JOIN types t2 ON t2.entity_id = s.reporting_2
LEFT JOIN types t3 ON t3.entity_id = s.reporting_3
LEFT JOIN types t4 ON t4.entity_id = s.reporting_4
LEFT JOIN types t5 ON t5.entity_id = s.reporting_5
LEFT JOIN types t6 ON t6.entity_id = s.reporting_6
WHERE k.record_type = 'PARA'
  AND k.para_status IN (8, 9)
  AND k.para_added_on IS NOT NULL;
