-- F8: execute this file alone in PL/SQL Developer SQL Window.
CREATE OR REPLACE VIEW V_EXEC_PARA_BASE AS
SELECT
    c.com_id,
    c.old_para_id,
    c.new_para_id,
    c.ind AS para_source,

    c.audit_period,
    c.para_no,
    c.gist_of_paras,

    c.entity_id,
    c.entity_code,
    e.name AS entity_name,
    c.entity_type_id,
    et.entitytypedesc AS entity_type_name,

    s.reporting_office_id,
    ro.name AS reporting_office_name,

    s.reporting_1,
    s.reporting_2,
    s.reporting_3,
    s.reporting_4,
    s.reporting_5,
    s.reporting_6,

    s.gm_office_id,
    gm.name AS gm_office_name,

    s.division_id,
    dv.name AS division_name,

    s.business_group_id,
    bg.name AS business_group_name,

    c.audited_by,
    ab.name AS audited_by_name,

    c.risk AS risk_id,
    CASE c.risk
        WHEN 1 THEN 'High'
        WHEN 2 THEN 'Medium'
        WHEN 3 THEN 'Low'
        ELSE 'Unrated'
    END AS risk_name,

    c.para_status,
    c.com_status,
    c.com_stage,
    NVL(c.com_cycle, 0) AS com_cycle,

    c.para_added_on,
    c.setteled_on,

    h.last_movement_date,

    -- Missing settlement dates and negative/future ages are UNKNOWN,
    -- never misleadingly classified as a valid 0-90-day para.
    CASE
        WHEN c.para_added_on IS NOT NULL
         AND (c.para_status = 8 OR
              (c.para_status = 9 AND c.setteled_on IS NOT NULL))
         AND TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE))
             >= TRUNC(c.para_added_on)
        THEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE))
             - TRUNC(c.para_added_on)
    END AS aging_days,

    CASE
        WHEN c.para_added_on IS NULL
          OR c.para_status NOT IN (8, 9) OR c.para_status IS NULL
          OR (c.para_status = 9 AND c.setteled_on IS NULL)
          OR TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE))
             < TRUNC(c.para_added_on)
        THEN 'UNKNOWN'
        WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE)) - TRUNC(c.para_added_on) <= 90
            THEN '0-90 DAYS'
        WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE)) - TRUNC(c.para_added_on) <= 180
            THEN '91-180 DAYS'
        WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE)) - TRUNC(c.para_added_on) <= 365
            THEN '181-365 DAYS'
        WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE)) - TRUNC(c.para_added_on) <= 1095
            THEN '1-3 YEARS'
        ELSE 'OVER 3 YEARS'
    END AS aging_bucket,

    CASE
        WHEN h.last_movement_date IS NOT NULL
        THEN TRUNC(SYSDATE) - TRUNC(h.last_movement_date)
    END AS days_since_last_movement,

    CASE
        WHEN c.para_status = 8 THEN 1
        ELSE 0
    END AS is_open,

    CASE
        WHEN c.para_status = 9 THEN 1
        ELSE 0
    END AS is_settled,

    CASE
        WHEN c.para_status = 8
         AND NVL(c.com_cycle,0) = 0
        THEN 1 ELSE 0
    END AS is_zero_cycle,

    CASE
        WHEN c.para_status = 8
         AND NVL(c.com_cycle,0) > 3
        THEN 1 ELSE 0
    END AS is_over_3_cycle,

    CASE
        WHEN c.para_status = 8
         AND c.para_added_on IS NOT NULL
         AND TRUNC(SYSDATE) - TRUNC(c.para_added_on) > 365
        THEN 1 ELSE 0
    END AS is_over_1_year,

    -- ZTBL definition: an OPEN high-risk para (RISK=1) is Critical.
    -- Aging / repeat compliance cycles remain independent indicators.
    CASE WHEN c.para_status = 8 AND c.risk = 1
         THEN 1 ELSE 0 END AS is_critical,

    c.no_of_instances,
    -- Copy made at audit conclusion, NOT a new lifecycle ISSUE event.
    CASE WHEN c.ind = 'A' THEN NVL(c.issue_rectified, 0)
    END AS issue_rectified_copy,
    CASE
      WHEN c.para_added_on IS NULL THEN 'MISSING_ADDED_ON'
      WHEN c.para_status NOT IN (8, 9) OR c.para_status IS NULL
        THEN 'UNKNOWN_STATUS'
      WHEN c.para_status = 9 AND c.setteled_on IS NULL THEN 'MISSING_SETTLEMENT_DATE'
      WHEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE))
           < TRUNC(c.para_added_on) THEN 'NEGATIVE_AGE'
      ELSE 'OK'
    END AS aging_quality

FROM ais_t_au_post_compliance c

LEFT JOIN t_auditee_entities e
       ON e.entity_id = c.entity_id

LEFT JOIN t_auditee_ent_types et
       ON et.autid = c.entity_type_id

LEFT JOIN V_EXEC_ENTITY_SCOPE s
       ON s.entity_id = c.entity_id

LEFT JOIN t_auditee_entities ro
       ON ro.entity_id = s.reporting_office_id

LEFT JOIN t_auditee_entities gm
       ON gm.entity_id = s.gm_office_id

LEFT JOIN t_auditee_entities dv
       ON dv.entity_id = s.division_id

LEFT JOIN t_auditee_entities bg
       ON bg.entity_id = s.business_group_id

LEFT JOIN t_auditee_entities ab
       ON ab.entity_id = c.audited_by

LEFT JOIN (
    SELECT com_id,
           MAX(comment_on) AS last_movement_date
    FROM ais_t_au_post_compliance_history
    GROUP BY com_id
) h
       ON h.com_id = c.com_id

-- The executive/role dashboards report POST-REPORT paras, not issues
-- rectified during audit finalization. The source flag is 0 by default,
-- not NULL. Keep status-9 paras settled after report issuance, including
-- additions and settlements within the same reporting period.
-- CASE preserves any legacy/unclassified IND values unchanged.
WHERE CASE WHEN c.ind = 'A' AND NVL(c.issue_rectified, 0) = 1
           THEN 0 ELSE 1 END = 1;
