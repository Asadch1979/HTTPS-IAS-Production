--Views
CREATE OR REPLACE VIEW V_EXEC_ENTITY_SCOPE AS
WITH entity_map AS
(
    SELECT
        entity_id,
        CASE WHEN COUNT(DISTINCT c_type_id) = 1 THEN MAX(c_type_id) END AS c_type_id
    FROM t_auditee_entities_maping
    WHERE NVL(status, 'Y') = 'Y'
    GROUP BY entity_id
),
hierarchy AS
(
    SELECT
        h.entity_id,
        h.reporting_1 AS parent_id,
        h.reporting_1,
        h.reporting_2,
        h.reporting_3,
        h.reporting_4,
        h.reporting_5,
        h.reporting_6
    FROM V_IAS_ENTITY_REPORTING_IDS h
    JOIN entity_map m
      ON m.entity_id = h.entity_id
),
hierarchy_levels AS
(
    SELECT
        h.entity_id,
        x.level_no,
        x.office_id,
        m.c_type_id
    FROM hierarchy h
    CROSS APPLY
    (
        SELECT 0 level_no, h.entity_id office_id FROM dual
        UNION ALL
        SELECT 1, h.reporting_1 FROM dual
        UNION ALL
        SELECT 2, h.reporting_2 FROM dual
        UNION ALL
        SELECT 3, h.reporting_3 FROM dual
        UNION ALL
        SELECT 4, h.reporting_4 FROM dual
        UNION ALL
        SELECT 5, h.reporting_5 FROM dual
        UNION ALL
        SELECT 6, h.reporting_6 FROM dual
    ) x
    LEFT JOIN entity_map m
      ON m.entity_id = x.office_id
    WHERE x.office_id IS NOT NULL
),
office_scope AS
(
    SELECT
        entity_id,

        MAX(CASE WHEN c_type_id = 21 THEN office_id END)
          KEEP (DENSE_RANK FIRST ORDER BY
                CASE WHEN c_type_id = 21
                     THEN level_no ELSE 99 END)
          AS gm_office_id,

        MAX(CASE WHEN c_type_id = 3 THEN office_id END)
          KEEP (DENSE_RANK FIRST ORDER BY
                CASE WHEN c_type_id = 3
                     THEN level_no ELSE 99 END)
          AS division_id,

        MAX(CASE WHEN c_type_id = 18 THEN office_id END)
          KEEP (DENSE_RANK FIRST ORDER BY
                CASE WHEN c_type_id = 18
                     THEN level_no ELSE 99 END)
          AS business_group_id

    FROM hierarchy_levels
    GROUP BY entity_id
)
SELECT
    h.entity_id,
    h.parent_id,

    h.reporting_1 AS reporting_office_id,

    s.gm_office_id,
    s.division_id,
    s.business_group_id,

    h.reporting_1,
    h.reporting_2,
    h.reporting_3,
    h.reporting_4,
    h.reporting_5,
    h.reporting_6

FROM hierarchy h
LEFT JOIN office_scope s
       ON s.entity_id = h.entity_id;
/
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

    CASE
        WHEN c.para_added_on IS NOT NULL
        THEN TRUNC(NVL(CASE WHEN c.para_status = 9 THEN c.setteled_on END, SYSDATE)) - TRUNC(c.para_added_on)
    END AS aging_days,

    CASE
        WHEN c.para_added_on IS NULL THEN 'UNKNOWN'
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

    CASE
        WHEN c.para_status = 8
         AND (
                c.risk = 1
             OR NVL(c.com_cycle,0) > 3
             OR (
                    c.para_added_on IS NOT NULL
                AND TRUNC(SYSDATE) - TRUNC(c.para_added_on) > 365
                )
             )
        THEN 1
        ELSE 0
    END AS is_critical,

    c.no_of_instances

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
       ON h.com_id = c.com_id;
/
CREATE OR REPLACE PACKAGE PKG_EXEC_DASHBOARD AS

  /*
    Dashboard types:
      TOP   = Bank-wide Top Management
      GROUP = Business Group scope
      CCO   = Bank-wide Compliance oversight
      CIA   = Bank-wide Internal Audit oversight

    The IAS backend must derive dashboard type and root entity from the
    authenticated user. This package applies hierarchy-based filtering but
    cannot authenticate privileges from dashboard type alone.
  */

  PROCEDURE P_GET_KPI_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                              P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                              P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                              IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_RISK_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                               P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                               P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                               IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_AGING_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_CYCLE_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_SETTLEMENT_TREND(P_DASHBOARD_TYPE IN VARCHAR2,
                                    P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                    P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                    IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_CRITICAL_ITEMS(P_DASHBOARD_TYPE IN VARCHAR2,
                                 P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                 P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                 P_MAX_ROWS       IN NUMBER DEFAULT 100,
                                 IO_CURSOR        OUT SYS_REFCURSOR);

END PKG_EXEC_DASHBOARD;
/
CREATE OR REPLACE PACKAGE BODY PKG_EXEC_DASHBOARD AS

  FUNCTION VALIDATE_REQUEST(P_DASHBOARD_TYPE IN VARCHAR2,
                            P_ENTITY_ID IN NUMBER) RETURN VARCHAR2 AS
    V_TYPE VARCHAR2(20) := UPPER(TRIM(P_DASHBOARD_TYPE));
  BEGIN
    IF V_TYPE IS NULL OR V_TYPE NOT IN ('TOP', 'GROUP', 'CCO', 'CIA') THEN
      RAISE_APPLICATION_ERROR(-20001, 'Unsupported executive dashboard type.');
    END IF;
    IF P_ENTITY_ID IS NOT NULL AND
       (P_ENTITY_ID <= 0 OR P_ENTITY_ID <> TRUNC(P_ENTITY_ID)) THEN
      RAISE_APPLICATION_ERROR(-20002, 'Entity ID must be a positive integer.');
    END IF;
    IF V_TYPE = 'GROUP' AND P_ENTITY_ID IS NULL THEN
      RAISE_APPLICATION_ERROR(-20003, 'Group dashboard needs an authorized entity.');
    END IF;
    RETURN V_TYPE;
  END VALIDATE_REQUEST;

  -------------------------------------------------------------------
  -- 1. KPI SUMMARY
  -------------------------------------------------------------------
  PROCEDURE P_GET_KPI_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                              P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                              P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                              IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN
  
  
    OPEN IO_CURSOR FOR
      SELECT COUNT(*) AS total_cases,
             
             NVL(SUM(p.is_open), 0) AS open_paras,
             
             NVL(SUM(CASE
                       WHEN p.is_open = 1 AND p.risk_id = 1 THEN
                        1
                       ELSE
                        0
                     END),
                 0) AS high_risk_paras,
             
             NVL(SUM(p.is_zero_cycle), 0) AS zero_cycle_paras,
             
             NVL(SUM(p.is_over_3_cycle), 0) AS over_3_cycle_paras,
             
             NVL(SUM(p.is_over_1_year), 0) AS over_1_year_paras,
             
             NVL(SUM(p.is_critical), 0) AS critical_items,
             
             NVL(SUM(p.is_settled), 0) AS settled_total,
             
             NVL(SUM(CASE
                       WHEN p.is_settled = 1 AND
                            p.setteled_on >= TRUNC(SYSDATE, 'MM') AND
                            p.setteled_on < ADD_MONTHS(TRUNC(SYSDATE, 'MM'), 1) THEN
                        1
                       ELSE
                        0
                     END),
                 0) AS settled_mtd,
             
             ROUND(CASE
                     WHEN NVL(SUM(p.is_open), 0) + NVL(SUM(p.is_settled), 0) = 0 THEN
                      0
                     ELSE
                      NVL(SUM(p.is_settled), 0) /
                      (NVL(SUM(p.is_open), 0) + NVL(SUM(p.is_settled), 0)) * 100
                   END,
                   2) AS settlement_ratio
      
        FROM V_EXEC_PARA_BASE p
      
       WHERE (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
            
         AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)));
  
  END P_GET_KPI_SUMMARY;

  -------------------------------------------------------------------
  -- 2. OPEN PARAS BY RISK
  -------------------------------------------------------------------
  PROCEDURE P_GET_RISK_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                               P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                               P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                               IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN
  
  
    OPEN IO_CURSOR FOR
    
      WITH risk_categories AS
       (SELECT 1 AS risk_id, 'High' AS risk_name, 1 AS display_order
          FROM dual
        UNION ALL
        SELECT 2, 'Medium', 2
          FROM dual
        UNION ALL
        SELECT 3, 'Low', 3
          FROM dual
        UNION ALL
        SELECT 0, 'Unrated', 4
          FROM dual),
      risk_data AS
       (SELECT CASE
                 WHEN p.risk_id IN (1, 2, 3) THEN
                  p.risk_id
                 ELSE
                  0
               END AS risk_id,
               COUNT(*) AS para_count
        
          FROM V_EXEC_PARA_BASE p
        
         WHERE p.is_open = 1
              
           AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
              
           AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)))
        
         GROUP BY CASE
                    WHEN p.risk_id IN (1, 2, 3) THEN
                     p.risk_id
                    ELSE
                     0
                  END),
      result_data AS
       (SELECT r.risk_id,
               r.risk_name,
               NVL(d.para_count, 0) AS para_count,
               r.display_order
          FROM risk_categories r
          LEFT JOIN risk_data d
            ON d.risk_id = r.risk_id)
      
      SELECT risk_id,
             risk_name,
             para_count,
             
             ROUND(CASE
                     WHEN SUM(para_count) OVER() = 0 THEN
                      0
                     ELSE
                      para_count / SUM(para_count) OVER() * 100
                   END,
                   2) AS percentage
      
        FROM result_data
       ORDER BY display_order;
  
  END P_GET_RISK_SUMMARY;

  -------------------------------------------------------------------
  -- 3. OPEN PARAS BY AGING
  -------------------------------------------------------------------
  PROCEDURE P_GET_AGING_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN
  
  
    OPEN IO_CURSOR FOR
    
      WITH age_categories AS
       (SELECT '0_90' AS bucket_code,
               '0-90 DAYS' AS bucket_name,
               1 AS display_order
          FROM dual
        
        UNION ALL
        SELECT '91_180', '91-180 DAYS', 2
          FROM dual
        
        UNION ALL
        SELECT '181_365', '181-365 DAYS', 3
          FROM dual
        
        UNION ALL
        SELECT '1_3_YEARS', '1-3 YEARS', 4
          FROM dual
        
        UNION ALL
        SELECT 'OVER_3_YEARS', 'OVER 3 YEARS', 5
          FROM dual
        
        UNION ALL
        SELECT 'UNKNOWN', 'UNKNOWN', 6
          FROM dual),
      age_data AS
       (SELECT p.aging_bucket,
               
               COUNT(*) AS para_count,
               
               SUM(CASE
                     WHEN p.risk_id = 1 THEN
                      1
                     ELSE
                      0
                   END) AS high_risk_count
        
          FROM V_EXEC_PARA_BASE p
        
         WHERE p.is_open = 1
              
           AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
              
           AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)))
        
         GROUP BY p.aging_bucket)
      
      SELECT a.bucket_code,
             a.bucket_name,
             NVL(d.para_count, 0) AS para_count,
             NVL(d.high_risk_count, 0) AS high_risk_count
      
        FROM age_categories a
        LEFT JOIN age_data d
          ON d.aging_bucket = a.bucket_name
      
       ORDER BY a.display_order;
  
  END P_GET_AGING_SUMMARY;

  -------------------------------------------------------------------
  -- 4. OPEN PARAS BY COMPLIANCE CYCLE
  -------------------------------------------------------------------
  PROCEDURE P_GET_CYCLE_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN
  
  
    ---------------------------------------------------------------
    -- CCO requires finer cycle breakup
    ---------------------------------------------------------------
    IF V_TYPE IN ('CCO', 'CIA') THEN
    
      OPEN IO_CURSOR FOR
      
        WITH cycle_categories AS
         (SELECT 'ZERO' AS bucket_code,
                 'Cycle 0' AS bucket_name,
                 1 AS display_order
            FROM dual
          
          UNION ALL
          SELECT 'ONE', 'Cycle 1', 2
            FROM dual
          
          UNION ALL
          SELECT 'TWO', 'Cycle 2', 3
            FROM dual
          
          UNION ALL
          SELECT 'THREE', 'Cycle 3', 4
            FROM dual
          
          UNION ALL
          SELECT 'FOUR_FIVE', 'Cycle 4-5', 5
            FROM dual
          
          UNION ALL
          SELECT 'OVER_FIVE', 'Over 5 Cycles', 6
            FROM dual),
        cycle_data AS
         (SELECT CASE
                   WHEN p.com_cycle = 0 THEN
                    'ZERO'
                   WHEN p.com_cycle = 1 THEN
                    'ONE'
                   WHEN p.com_cycle = 2 THEN
                    'TWO'
                   WHEN p.com_cycle = 3 THEN
                    'THREE'
                   WHEN p.com_cycle BETWEEN 4 AND 5 THEN
                    'FOUR_FIVE'
                   ELSE
                    'OVER_FIVE'
                 END AS bucket_code,
                 
                 COUNT(*) AS para_count,
                 
                 SUM(CASE
                       WHEN p.risk_id = 1 THEN
                        1
                       ELSE
                        0
                     END) AS high_risk_count
          
            FROM V_EXEC_PARA_BASE p
          
           WHERE p.is_open = 1
                
             AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_ID IS NULL OR
                  p.entity_id = P_ENTITY_ID OR
                  p.reporting_1 = P_ENTITY_ID OR
                  p.reporting_2 = P_ENTITY_ID OR
                  p.reporting_3 = P_ENTITY_ID OR
                  p.reporting_4 = P_ENTITY_ID OR
                  p.reporting_5 = P_ENTITY_ID OR
                  p.reporting_6 = P_ENTITY_ID)
           GROUP BY CASE
                      WHEN p.com_cycle = 0 THEN
                       'ZERO'
                      WHEN p.com_cycle = 1 THEN
                       'ONE'
                      WHEN p.com_cycle = 2 THEN
                       'TWO'
                      WHEN p.com_cycle = 3 THEN
                       'THREE'
                      WHEN p.com_cycle BETWEEN 4 AND 5 THEN
                       'FOUR_FIVE'
                      ELSE
                       'OVER_FIVE'
                    END)
        
        SELECT c.bucket_code,
               c.bucket_name,
               NVL(d.para_count, 0) AS para_count,
               NVL(d.high_risk_count, 0) AS high_risk_count
        
          FROM cycle_categories c
          LEFT JOIN cycle_data d
            ON d.bucket_code = c.bucket_code
        
         ORDER BY c.display_order;
    
      ---------------------------------------------------------------
      -- TOP and GROUP use simpler management breakup
      ---------------------------------------------------------------
    ELSE
    
      OPEN IO_CURSOR FOR
      
        WITH cycle_categories AS
         (SELECT 'ZERO' AS bucket_code,
                 'No Compliance / Cycle 0' AS bucket_name,
                 1 AS display_order
            FROM dual
          
          UNION ALL
          SELECT 'ONE', 'Cycle 1', 2
            FROM dual
          
          UNION ALL
          SELECT 'TWO_THREE', 'Cycle 2-3', 3
            FROM dual
          
          UNION ALL
          SELECT 'OVER_THREE', 'Over 3 Cycles', 4
            FROM dual),
        cycle_data AS
         (SELECT CASE
                   WHEN p.com_cycle = 0 THEN
                    'ZERO'
                   WHEN p.com_cycle = 1 THEN
                    'ONE'
                   WHEN p.com_cycle BETWEEN 2 AND 3 THEN
                    'TWO_THREE'
                   ELSE
                    'OVER_THREE'
                 END AS bucket_code,
                 
                 COUNT(*) AS para_count,
                 
                 SUM(CASE
                       WHEN p.risk_id = 1 THEN
                        1
                       ELSE
                        0
                     END) AS high_risk_count
          
            FROM V_EXEC_PARA_BASE p
          
           WHERE p.is_open = 1
                
             AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
                
             AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)))
          
           GROUP BY CASE
                      WHEN p.com_cycle = 0 THEN
                       'ZERO'
                      WHEN p.com_cycle = 1 THEN
                       'ONE'
                      WHEN p.com_cycle BETWEEN 2 AND 3 THEN
                       'TWO_THREE'
                      ELSE
                       'OVER_THREE'
                    END)
        
        SELECT c.bucket_code,
               c.bucket_name,
               NVL(d.para_count, 0) AS para_count,
               NVL(d.high_risk_count, 0) AS high_risk_count
        
          FROM cycle_categories c
          LEFT JOIN cycle_data d
            ON d.bucket_code = c.bucket_code
        
         ORDER BY c.display_order;
    
    END IF;
  
  END P_GET_CYCLE_SUMMARY;

END PKG_EXEC_DASHBOARD;
