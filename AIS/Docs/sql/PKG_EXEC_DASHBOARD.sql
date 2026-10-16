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
                              P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                              P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                              P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                              IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_RISK_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                               P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                               P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                               P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                               P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                               P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                               IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_AGING_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_CYCLE_SUMMARY(P_DASHBOARD_TYPE IN VARCHAR2,
                                P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_SETTLEMENT_TREND(P_DASHBOARD_TYPE IN VARCHAR2,
                                    P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                    P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                    P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                    P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                    P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                    IO_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_GET_CRITICAL_ITEMS(P_DASHBOARD_TYPE IN VARCHAR2,
                                 P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                 P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                 P_MAX_ROWS       IN NUMBER DEFAULT 100,
                                 P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                 P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                 P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                 IO_CURSOR        OUT SYS_REFCURSOR);

  -------------------------------------------------------------------
  -- Lifecycle KPIs: activity by event date; existing APIs unaffected.
  -- This procedure uses the approved V_EXEC_KPI_LIFECYCLE view.
  -------------------------------------------------------------------
  PROCEDURE P_GET_KPI_LIFECYCLE(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR);


  -- CIA issues: both dates optional, all eligible history when omitted.
  PROCEDURE P_GET_CIA_ISSUES(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE DEFAULT NULL,
      P_END_DATE IN DATE DEFAULT NULL,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR);


  -------------------------------------------------------------------
  -- CIA Audit Command Center (isolated; existing cursors unchanged).
  -- The selected audit period defines plan and issue-event dates.
  -- Pending response / delayed reports / active teams are CURRENT
  -- states of engagements belonging to that selected audit period.
  -------------------------------------------------------------------
  PROCEDURE P_GET_CIA_EXEC_KPIS(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR);

  -- Exactly ten categories, plus total/unmapped diagnostic columns.
  -- Requires T_IAS_CIA_ISSUE_NATURE_MAP (deploy its DDL first).
  PROCEDURE P_GET_CIA_ISSUE_NATURE(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR);

END PKG_EXEC_DASHBOARD;
/
CREATE OR REPLACE PACKAGE BODY PKG_EXEC_DASHBOARD AS

  FUNCTION VALIDATE_REQUEST(P_DASHBOARD_TYPE IN VARCHAR2,
                            P_ENTITY_ID IN NUMBER) RETURN VARCHAR2 AS
    V_TYPE VARCHAR2(20) := UPPER(TRIM(P_DASHBOARD_TYPE));
  BEGIN
    IF V_TYPE IS NULL OR V_TYPE NOT IN ('TOP', 'GROUP', 'CCO', 'CIA', 'RLM') THEN
      RAISE_APPLICATION_ERROR(-20001, 'Unsupported executive dashboard type.');
    END IF;
    IF P_ENTITY_ID IS NOT NULL AND
       (P_ENTITY_ID <= 0 OR P_ENTITY_ID <> TRUNC(P_ENTITY_ID)) THEN
      RAISE_APPLICATION_ERROR(-20002, 'Entity ID must be a positive integer.');
    END IF;
    IF V_TYPE IN ('GROUP','RLM') AND P_ENTITY_ID IS NULL THEN
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
                              P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                              P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                              P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
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
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
            
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
                               P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                               P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                               P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
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
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
              
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
                                P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
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
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
              
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
                                P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN
  
  
    ---------------------------------------------------------------
    -- CCO and CIA require finer cycle breakup
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
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
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
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
                
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


  -------------------------------------------------------------------
  -- 5. SETTLED PARAS: last 12 calendar months (including zero months)
  -------------------------------------------------------------------
  PROCEDURE P_GET_SETTLEMENT_TREND(P_DASHBOARD_TYPE IN VARCHAR2,
                                    P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                    P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                    P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                    P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                    P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                    IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN
    OPEN IO_CURSOR FOR
      WITH months AS
       (SELECT ADD_MONTHS(TRUNC(SYSDATE, 'MM'), 1 - LEVEL) month_start
          FROM dual
        CONNECT BY LEVEL <= 12),
      month_counts AS
       (SELECT TRUNC(p.setteled_on, 'MM') month_start,
               COUNT(*) AS settled_paras,
               SUM(CASE WHEN p.risk_id = 1 THEN 1 ELSE 0 END) AS high_risk_settled
          FROM V_EXEC_PARA_BASE p
         WHERE p.is_settled = 1
           AND p.setteled_on >= ADD_MONTHS(TRUNC(SYSDATE, 'MM'), -11)
           AND p.setteled_on < ADD_MONTHS(TRUNC(SYSDATE, 'MM'), 1)
           AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
           AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
             OR (P_ENTITY_ID IS NOT NULL AND
                 (p.entity_id = P_ENTITY_ID OR
                  p.reporting_1 = P_ENTITY_ID OR
                  p.reporting_2 = P_ENTITY_ID OR
                  p.reporting_3 = P_ENTITY_ID OR
                  p.reporting_4 = P_ENTITY_ID OR
                  p.reporting_5 = P_ENTITY_ID OR
                  p.reporting_6 = P_ENTITY_ID)))
         GROUP BY TRUNC(p.setteled_on, 'MM'))
      SELECT TO_CHAR(m.month_start, 'YYYY-MM') AS settlement_month,
             NVL(c.settled_paras, 0) AS settled_paras,
             NVL(c.high_risk_settled, 0) AS high_risk_settled
        FROM months m
        LEFT JOIN month_counts c ON c.month_start = m.month_start
       ORDER BY m.month_start;
  END P_GET_SETTLEMENT_TREND;


  -------------------------------------------------------------------
  -- 6. CRITICAL OPEN PARAS: bounded detail for executive drill-down
  -------------------------------------------------------------------
  PROCEDURE P_GET_CRITICAL_ITEMS(P_DASHBOARD_TYPE IN VARCHAR2,
                                 P_ENTITY_ID      IN NUMBER DEFAULT NULL,
                                 P_AUDIT_PERIOD   IN VARCHAR2 DEFAULT NULL,
                                 P_MAX_ROWS       IN NUMBER DEFAULT 100,
                                 P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                                 P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
                                 P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
                                 IO_CURSOR        OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
  BEGIN
    IF P_MAX_ROWS IS NULL OR P_MAX_ROWS < 1 OR P_MAX_ROWS > 500 OR
       P_MAX_ROWS <> TRUNC(P_MAX_ROWS) THEN
      RAISE_APPLICATION_ERROR(-20004, 'Maximum rows must be between 1 and 500.');
    END IF;

    OPEN IO_CURSOR FOR
      SELECT *
        FROM (
          SELECT p.com_id,
                 p.entity_id,
                 p.entity_name,
                 p.division_id,
                 p.division_name,
                 p.business_group_id,
                 p.business_group_name,
                 p.audit_period,
                 p.para_no,
                 p.gist_of_paras,
                 p.risk_id,
                 p.risk_name,
                 p.com_cycle,
                 p.para_added_on,
                 p.aging_days,
                 p.days_since_last_movement
            FROM V_EXEC_PARA_BASE p
           WHERE p.is_open = 1
             AND p.is_critical = 1
             AND (P_AUDIT_PERIOD IS NULL OR p.audit_period = P_AUDIT_PERIOD)
             AND (P_ENTITY_TYPE_ID IS NULL OR p.entity_type_id = P_ENTITY_TYPE_ID)
             AND (P_AUDITED_BY_ID IS NULL OR p.audited_by = P_AUDITED_BY_ID)
             AND (P_REPORTING_OFFICE_ID IS NULL OR p.entity_id = P_REPORTING_OFFICE_ID OR
                  P_REPORTING_OFFICE_ID IN (p.reporting_1,p.reporting_2,p.reporting_3,
                                            p.reporting_4,p.reporting_5,p.reporting_6))
             AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
               OR (P_ENTITY_ID IS NOT NULL AND
                   (p.entity_id = P_ENTITY_ID OR
                    p.reporting_1 = P_ENTITY_ID OR
                    p.reporting_2 = P_ENTITY_ID OR
                    p.reporting_3 = P_ENTITY_ID OR
                    p.reporting_4 = P_ENTITY_ID OR
                    p.reporting_5 = P_ENTITY_ID OR
                    p.reporting_6 = P_ENTITY_ID)))
           ORDER BY CASE WHEN p.risk_id = 1 THEN 0 ELSE 1 END,
                    p.aging_days DESC NULLS LAST,
                    p.com_id
        )
       WHERE ROWNUM <= P_MAX_ROWS;
  END P_GET_CRITICAL_ITEMS;

  -------------------------------------------------------------------
  -- Lifecycle / At-a-Glance KPI preview.
  --
  -- Four activity counts use inclusive Start/End calendar dates
  -- and the tested Source A/O/C lifecycle business rules.
  -- Issue Rectified excludes status 23 drops. ISSUE_DROPPED remains
  -- an independent diagnostic of drops before audit conclusion.
  --
  -- Historical opening/closing balances and associated end-date risk,
  -- aging and cycle counts are DATE-BASED ESTIMATES (E). A historical
  -- source status ledger is not complete enough to certify reopens,
  -- transfers, and historic changes in risk/COM_CYCLE.
  -- For End Date=today, closing snapshots use the current-state flags (Y).
  -- Do NOT mark older snapshots Y merely because they have a count.
  -- Estimate algorithm follows PKG_EXEC_REPORTS close-on convention,
  -- but uses approved lifecycle source A/O/C exclusions and scope.
  -- For modern source A, a para is not considered open before the
  -- known STATUS=14 conclusion event, even if PARA_ADDED_ON is copied
  -- from the much earlier observation ENTEREDDATE. When missing,
  -- the date-based estimate falls back to PARA_ADDED_ON.
  -------------------------------------------------------------------
  PROCEDURE P_GET_KPI_LIFECYCLE(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE,
      P_END_DATE IN DATE,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
    V_FROM DATE;
    V_TO_EXCLUSIVE DATE;
    V_TODAY_SNAPSHOT NUMBER(1);
  BEGIN
    IF P_START_DATE IS NULL OR P_END_DATE IS NULL
       OR TRUNC(P_START_DATE) > TRUNC(P_END_DATE) THEN
      RAISE_APPLICATION_ERROR(-20005,
        'Both KPI reporting dates are required; Start Date cannot exceed End Date.');
    END IF;

    V_FROM := TRUNC(P_START_DATE);
    V_TO_EXCLUSIVE := TRUNC(P_END_DATE) + 1;
    IF TRUNC(P_END_DATE) > TRUNC(SYSDATE) THEN
      RAISE_APPLICATION_ERROR(-20006, 'KPI End Date cannot be in the future.');
    END IF;

    V_TODAY_SNAPSHOT := CASE
      WHEN TRUNC(P_END_DATE) = TRUNC(SYSDATE) THEN 1 ELSE 0 END;

    OPEN IO_CURSOR FOR
      SELECT
        NVL(SUM(CASE
          WHEN k.record_type = 'ISSUE'
           AND k.issue_eligible = 1
           AND k.issue_identified_on >= V_FROM
           AND k.issue_identified_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS issue_identified,

        NVL(SUM(CASE
          WHEN k.record_type = 'ISSUE'
           AND k.observation_status IN (4, 9)
           AND k.issue_rectified_on >= V_FROM
           AND k.issue_rectified_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS issue_rectified,

        NVL(SUM(CASE
          WHEN k.record_type = 'ISSUE'
           AND k.observation_status = 23
           AND k.issue_rectified_on >= V_FROM
           AND k.issue_rectified_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS issue_dropped,

        NVL(SUM(CASE
          WHEN k.record_type = 'PARA'
           AND k.para_added_on >= V_FROM
           AND k.para_added_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS para_added,

        NVL(SUM(CASE
          WHEN k.record_type = 'PARA'
           AND k.para_settled_on >= V_FROM
           AND k.para_settled_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS paras_settled,

        NVL(SUM(CASE
          WHEN k.record_type = 'PARA'
           AND k.excluded_pre_finalization = 1
           AND k.raw_para_added_on >= V_FROM
           AND k.raw_para_added_on < V_TO_EXCLUSIVE
          THEN 1 ELSE 0 END), 0) AS excluded_para_additions,

        -- Opening: outstanding immediately before selected Start Date.
        -- Historical ESTIMATE based on current master status/latest
        -- SETTELED_ON; cannot reconstruct unlogged reopenings.
        NVL(SUM(CASE
          WHEN k.record_type = 'PARA'
           AND k.para_added_on < V_FROM
           AND (k.source_ind <> 'A'
                OR k.report_concluded_on IS NULL
                OR k.report_concluded_on < V_FROM)
           AND (k.para_status = 8
                OR (k.para_status = 9
                    AND k.para_settled_on >= V_FROM))
          THEN 1 ELSE 0 END), 0) AS opening_open_paras,

        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN
          NVL(SUM(CASE WHEN k.record_type = 'PARA'
                        THEN k.current_open ELSE 0 END), 0)
        ELSE
          NVL(SUM(CASE
            WHEN k.record_type = 'PARA'
             AND k.para_added_on < V_TO_EXCLUSIVE
             AND (k.source_ind <> 'A'
                  OR k.report_concluded_on IS NULL
                  OR k.report_concluded_on < V_TO_EXCLUSIVE)
             AND (k.para_status = 8
                  OR (k.para_status = 9
                      AND k.para_settled_on >= V_TO_EXCLUSIVE))
            THEN 1 ELSE 0 END), 0)
        END AS closing_open_paras,

        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN
          NVL(SUM(CASE WHEN k.record_type = 'PARA'
                        THEN k.current_high_risk_open ELSE 0 END), 0)
        ELSE
          NVL(SUM(CASE
            WHEN k.record_type = 'PARA'
             AND k.para_added_on < V_TO_EXCLUSIVE
             AND (k.source_ind <> 'A'
                  OR k.report_concluded_on IS NULL
                  OR k.report_concluded_on < V_TO_EXCLUSIVE)
             AND (k.para_status = 8
                  OR (k.para_status = 9
                      AND k.para_settled_on >= V_TO_EXCLUSIVE))
             AND k.risk_id = 1
            THEN 1 ELSE 0 END), 0)
        END AS high_risk_open_paras,

        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN
          NVL(SUM(CASE WHEN k.record_type = 'PARA'
                        THEN k.current_over_one_year ELSE 0 END), 0)
        ELSE
          NVL(SUM(CASE
            WHEN k.record_type = 'PARA'
             AND k.para_added_on < V_TO_EXCLUSIVE
             AND (k.source_ind <> 'A'
                  OR k.report_concluded_on IS NULL
                  OR k.report_concluded_on < V_TO_EXCLUSIVE)
             AND (k.para_status = 8
                  OR (k.para_status = 9
                      AND k.para_settled_on >= V_TO_EXCLUSIVE))
             AND TRUNC(P_END_DATE) - TRUNC(k.para_added_on) > 365
            THEN 1 ELSE 0 END), 0)
        END AS over_1_year_paras,

        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN
          NVL(SUM(CASE WHEN k.record_type = 'PARA'
                        THEN k.current_over_three_cycles ELSE 0 END), 0)
        ELSE
          NVL(SUM(CASE
            WHEN k.record_type = 'PARA'
             AND k.para_added_on < V_TO_EXCLUSIVE
             AND (k.source_ind <> 'A'
                  OR k.report_concluded_on IS NULL
                  OR k.report_concluded_on < V_TO_EXCLUSIVE)
             AND (k.para_status = 8
                  OR (k.para_status = 9
                      AND k.para_settled_on >= V_TO_EXCLUSIVE))
             AND NVL(k.com_cycle, 0) > 3
            THEN 1 ELSE 0 END), 0)
        END AS over_3_compliance_cycles,

        V_FROM AS start_date,
        V_TO_EXCLUSIVE - 1 AS end_date,
        -- E=Estimated (not verified); Y=verified current-state only.
        -- Keep existing column names for API compatibility.
        'E' AS historical_opening_verified,
        CASE WHEN V_TODAY_SNAPSHOT = 1 THEN 'Y' ELSE 'E' END
          AS closing_asof_verified

      FROM V_EXEC_KPI_LIFECYCLE k
      WHERE (P_ENTITY_TYPE_ID IS NULL OR k.entity_type_id = P_ENTITY_TYPE_ID)
        AND (P_AUDITED_BY_ID IS NULL OR k.audited_by_id = P_AUDITED_BY_ID)
        AND (P_REPORTING_OFFICE_ID IS NULL
             OR k.entity_id = P_REPORTING_OFFICE_ID
             OR P_REPORTING_OFFICE_ID IN
                (k.reporting_1, k.reporting_2, k.reporting_3,
                 k.reporting_4, k.reporting_5, k.reporting_6))
        AND ((V_TYPE IN ('TOP', 'CCO', 'CIA') AND P_ENTITY_ID IS NULL)
             OR (P_ENTITY_ID IS NOT NULL
                 AND (k.entity_id = P_ENTITY_ID
                      OR k.reporting_1 = P_ENTITY_ID
                      OR k.reporting_2 = P_ENTITY_ID
                      OR k.reporting_3 = P_ENTITY_ID
                      OR k.reporting_4 = P_ENTITY_ID
                      OR k.reporting_5 = P_ENTITY_ID
                      OR k.reporting_6 = P_ENTITY_ID)));
  END P_GET_KPI_LIFECYCLE;


  -------------------------------------------------------------------
  -- CIA-only ISSUE event counts. No dates = all eligible historical
  -- observations within the authenticated authorized IAS scope.
  -- Issue identified/rectified use their own event date and may differ.
  -- Status 23 (Drop by Team Lead) is excluded from rectifications.
  -------------------------------------------------------------------
  PROCEDURE P_GET_CIA_ISSUES(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_START_DATE IN DATE DEFAULT NULL,
      P_END_DATE IN DATE DEFAULT NULL,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR) AS
    V_TYPE VARCHAR2(20) := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
    V_FROM DATE;
    V_TO_EXCLUSIVE DATE;
  BEGIN
    IF V_TYPE <> 'CIA' THEN
      RAISE_APPLICATION_ERROR(-20007, 'Issue count procedure requires CIA perspective.');
    END IF;
    IF (P_START_DATE IS NULL AND P_END_DATE IS NOT NULL)
       OR (P_START_DATE IS NOT NULL AND P_END_DATE IS NULL)
       OR (P_START_DATE IS NOT NULL AND
           TRUNC(P_START_DATE) > TRUNC(P_END_DATE)) THEN
      RAISE_APPLICATION_ERROR(-20008,
        'CIA issue dates must both be omitted or form a valid Start/End Date range.');
    END IF;
    IF P_START_DATE IS NOT NULL THEN
      V_FROM := TRUNC(P_START_DATE);
      V_TO_EXCLUSIVE := TRUNC(P_END_DATE) + 1;
    END IF;

    OPEN IO_CURSOR FOR
      SELECT NVL(SUM(CASE
                 WHEN k.issue_eligible = 1
                  AND k.issue_identified_on IS NOT NULL
                  AND (V_FROM IS NULL OR
                      (k.issue_identified_on >= V_FROM AND
                       k.issue_identified_on < V_TO_EXCLUSIVE))
                 THEN 1 ELSE 0 END), 0) AS issue_identified,
             NVL(SUM(CASE
                 WHEN k.observation_status IN (4, 9)
                  AND k.issue_rectified_on IS NOT NULL
                  AND (V_FROM IS NULL OR
                      (k.issue_rectified_on >= V_FROM AND
                       k.issue_rectified_on < V_TO_EXCLUSIVE))
                 THEN 1 ELSE 0 END), 0) AS issue_rectified
        FROM V_EXEC_KPI_LIFECYCLE k
       WHERE k.record_type = 'ISSUE'
         AND (P_ENTITY_TYPE_ID IS NULL OR k.entity_type_id = P_ENTITY_TYPE_ID)
         AND (P_AUDITED_BY_ID IS NULL OR k.audited_by_id = P_AUDITED_BY_ID)
         AND (P_REPORTING_OFFICE_ID IS NULL
              OR k.entity_id = P_REPORTING_OFFICE_ID
              OR P_REPORTING_OFFICE_ID IN
                 (k.reporting_1, k.reporting_2, k.reporting_3,
                  k.reporting_4, k.reporting_5, k.reporting_6))
         AND (P_ENTITY_ID IS NULL OR
              (k.entity_id = P_ENTITY_ID OR P_ENTITY_ID IN
                 (k.reporting_1, k.reporting_2, k.reporting_3,
                  k.reporting_4, k.reporting_5, k.reporting_6)));
  END P_GET_CIA_ISSUES;


  -------------------------------------------------------------------
  -- CIA Audit Command Center: shared period & perspective validation.
  -- Authorization is also mandatory in the authenticated IAS API.
  -------------------------------------------------------------------
  PROCEDURE RESOLVE_CIA_COMMAND_PERIOD(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_FROM OUT DATE,
      P_TO_EXCLUSIVE OUT DATE) AS
    V_TYPE VARCHAR2(20);
  BEGIN
    V_TYPE := VALIDATE_REQUEST(P_DASHBOARD_TYPE, P_ENTITY_ID);
    IF V_TYPE <> 'CIA' THEN
      RAISE_APPLICATION_ERROR(-20041,
        'CIA Command Center procedures require the CIA perspective.');
    END IF;
    IF P_AUDIT_PERIOD_ID IS NULL OR P_AUDIT_PERIOD_ID <= 0
       OR P_AUDIT_PERIOD_ID <> TRUNC(P_AUDIT_PERIOD_ID) THEN
      RAISE_APPLICATION_ERROR(-20042,
        'Select a valid numeric audit period ID.');
    END IF;

    SELECT TRUNC(p.start_date), TRUNC(p.end_date) + 1
      INTO P_FROM, P_TO_EXCLUSIVE
      FROM t_au_period p
     WHERE p.auditperiodid = P_AUDIT_PERIOD_ID;

    IF P_FROM IS NULL OR P_TO_EXCLUSIVE IS NULL
       OR P_FROM >= P_TO_EXCLUSIVE THEN
      RAISE_APPLICATION_ERROR(-20043, 'Audit period dates are invalid.');
    END IF;
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      RAISE_APPLICATION_ERROR(-20044, 'Audit period was not found.');
  END RESOLVE_CIA_COMMAND_PERIOD;

  -------------------------------------------------------------------
  -- CIA EXECUTIVE KPIs
  --
  -- Planned/Audited: DISTINCT planned entities (statuses 1,2);
  --                  concluded by status-14 engagement log/current.
  -- Delayed reports: historical IAS report proxy: engagement stages
  --                  4..13, audit end +15 days, no status-14 log.
  -- Fraud A1:        final observation statuses 8,9 with ANNEX=1,
  --                  ENTEREDDATE inside selected audit period dates.
  -- Awaiting reply:  T_AU_OBSERVATION.STATUS=2, current state,
  --                  linked to engagements in the selected period.
  -- Active teams:    distinct assigned teams in active stages 4..12,
  --                  work commenced and audit not concluded.
  --
  -- NOTE: Delay SLA and active stage range must be reconciled with
  -- actual IAS workflow before front-end publication. This procedure
  -- never alters other executive KPI procedures or perspectives.
  -------------------------------------------------------------------
  PROCEDURE P_GET_CIA_EXEC_KPIS(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR) AS
    V_FROM DATE;
    V_TO_EXCLUSIVE DATE;
  BEGIN
    RESOLVE_CIA_COMMAND_PERIOD(
      P_DASHBOARD_TYPE, P_ENTITY_ID, P_AUDIT_PERIOD_ID,
      V_FROM, V_TO_EXCLUSIVE);

    OPEN IO_CURSOR FOR
      WITH plan_scope AS
      (
        SELECT p.id AS plan_id, p.entity_id
          FROM t_au_plan p
          LEFT JOIN v_exec_entity_scope s ON s.entity_id = p.entity_id
         WHERE p.auditperiodid = P_AUDIT_PERIOD_ID
           AND p.status IN (1, 2)
           AND (P_ENTITY_TYPE_ID IS NULL
                OR p.entity_typeid = P_ENTITY_TYPE_ID)
           AND (P_AUDITED_BY_ID IS NULL
                OR p.auditedby = P_AUDITED_BY_ID)
           AND (P_REPORTING_OFFICE_ID IS NULL
                OR p.entity_id = P_REPORTING_OFFICE_ID
                OR P_REPORTING_OFFICE_ID IN
                   (s.reporting_1, s.reporting_2, s.reporting_3,
                    s.reporting_4, s.reporting_5, s.reporting_6))
           AND (P_ENTITY_ID IS NULL OR p.entity_id = P_ENTITY_ID
                OR P_ENTITY_ID IN
                   (s.reporting_1, s.reporting_2, s.reporting_3,
                    s.reporting_4, s.reporting_5, s.reporting_6))
      ),
      engagement_scope AS
      (
        SELECT e.eng_id, e.plan_id, e.period_id, e.entity_id,
               e.auditby_id, e.status, e.team_id,
               e.audit_startdate, e.audit_enddate,
               e.operation_startdate
          FROM t_au_plan_eng e
          LEFT JOIN t_auditee_entities a ON a.entity_id = e.entity_id
          LEFT JOIN v_exec_entity_scope s ON s.entity_id = e.entity_id
         WHERE (P_ENTITY_TYPE_ID IS NULL
                OR a.type_id = P_ENTITY_TYPE_ID)
           AND (P_AUDITED_BY_ID IS NULL
                OR e.auditby_id = P_AUDITED_BY_ID)
           AND (P_REPORTING_OFFICE_ID IS NULL
                OR e.entity_id = P_REPORTING_OFFICE_ID
                OR P_REPORTING_OFFICE_ID IN
                   (s.reporting_1, s.reporting_2, s.reporting_3,
                    s.reporting_4, s.reporting_5, s.reporting_6))
           AND (P_ENTITY_ID IS NULL OR e.entity_id = P_ENTITY_ID
                OR P_ENTITY_ID IN
                   (s.reporting_1, s.reporting_2, s.reporting_3,
                    s.reporting_4, s.reporting_5, s.reporting_6))
      ),
      coverage AS
      (
        SELECT COUNT(DISTINCT p.entity_id) AS planned_entities,
               COUNT(DISTINCT CASE
                 WHEN EXISTS
                 (
                   SELECT 1 FROM engagement_scope e
                    WHERE e.plan_id = p.plan_id
                      AND e.period_id = P_AUDIT_PERIOD_ID
                      AND (e.status = 14 OR EXISTS
                          (SELECT 1 FROM t_au_plan_eng_log l
                            WHERE l.e_id = e.eng_id
                              AND l.status_id = 14))
                 )
                 THEN p.entity_id END) AS audited_entities
          FROM plan_scope p
      ),
      delayed AS
      (
        SELECT COUNT(DISTINCT e.eng_id) AS delayed_audit_reports
          FROM engagement_scope e
         WHERE e.period_id = P_AUDIT_PERIOD_ID
           AND e.status BETWEEN 4 AND 13
           AND e.audit_enddate + 15 < TRUNC(SYSDATE)
           AND NOT EXISTS
               (SELECT 1 FROM t_au_plan_eng_log l
                 WHERE l.e_id = e.eng_id AND l.status_id = 14)
      ),
      fraud AS
      (
        SELECT COUNT(DISTINCT o.id) AS fraud_a1_paras
          FROM t_au_observation o
          JOIN engagement_scope e ON e.eng_id = o.engplanid
         WHERE o.annex = 1
           AND o.status IN (8, 9)
           AND o.entereddate >= V_FROM
           AND o.entereddate < V_TO_EXCLUSIVE
      ),
      awaiting AS
      (
        SELECT COUNT(DISTINCT o.id) AS awaiting_auditee_response
          FROM t_au_observation o
          JOIN engagement_scope e ON e.eng_id = o.engplanid
         WHERE e.period_id = P_AUDIT_PERIOD_ID
           AND o.status = 2
      ),
      active_teams AS
      (
        SELECT COUNT(DISTINCT e.team_id) AS teams_conducting_audit
          FROM engagement_scope e
         WHERE e.period_id = P_AUDIT_PERIOD_ID
           AND e.team_id IS NOT NULL
           AND e.status BETWEEN 4 AND 12
           AND TRUNC(NVL(e.operation_startdate, e.audit_startdate))
               <= TRUNC(SYSDATE)
           AND NOT EXISTS
               (SELECT 1 FROM t_au_plan_eng_log l
                 WHERE l.e_id = e.eng_id AND l.status_id = 14)
      )
      SELECT c.planned_entities,
             c.audited_entities,
             ROUND(CASE WHEN c.planned_entities = 0 THEN NULL
                        ELSE c.audited_entities * 100
                             / c.planned_entities END, 2)
               AS plan_coverage_pct,
             d.delayed_audit_reports,
             f.fraud_a1_paras,
             a.awaiting_auditee_response,
             t.teams_conducting_audit,
             P_AUDIT_PERIOD_ID AS audit_period_id,
             V_FROM AS audit_period_from,
             V_TO_EXCLUSIVE - 1 AS audit_period_to,
             TRUNC(SYSDATE) AS current_state_as_of
        FROM coverage c
        CROSS JOIN delayed d
        CROSS JOIN fraud f
        CROSS JOIN awaiting a
        CROSS JOIN active_teams t;
  END P_GET_CIA_EXEC_KPIS;

  -------------------------------------------------------------------
  -- CIA Nature of Issue Identified (exactly ten rows).
  -- Eligibility, observation date and scope follow P_GET_CIA_ISSUES.
  -- Source categories are ID-based mappings: no free-text guessing.
  -- Mapping priority handles potentially overlapping match sources.
  -- An unmapped total is returned to prevent misleading zero charts.
  -------------------------------------------------------------------
  PROCEDURE P_GET_CIA_ISSUE_NATURE(
      P_DASHBOARD_TYPE IN VARCHAR2,
      P_ENTITY_ID IN NUMBER,
      P_AUDIT_PERIOD_ID IN NUMBER,
      P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
      P_AUDITED_BY_ID IN NUMBER DEFAULT NULL,
      P_REPORTING_OFFICE_ID IN NUMBER DEFAULT NULL,
      IO_CURSOR OUT SYS_REFCURSOR) AS
    V_FROM DATE;
    V_TO_EXCLUSIVE DATE;
  BEGIN
    RESOLVE_CIA_COMMAND_PERIOD(
      P_DASHBOARD_TYPE, P_ENTITY_ID, P_AUDIT_PERIOD_ID,
      V_FROM, V_TO_EXCLUSIVE);

    OPEN IO_CURSOR FOR
      WITH categories AS
      (
        SELECT 1 display_order, 'AML_CFT' category_code,
               'AML/CFT' category_name FROM dual
        UNION ALL SELECT 2, 'ACCOUNTS', 'Accounts' FROM dual
        UNION ALL SELECT 3, 'LOAN_ADVANCES', 'Loan and Advances' FROM dual
        UNION ALL SELECT 4, 'CASH_MANAGEMENT', 'Cash Management' FROM dual
        UNION ALL SELECT 5, 'RECORD_MANAGEMENT', 'Record Management' FROM dual
        UNION ALL SELECT 6, 'BANKING_OPERATIONS', 'Banking Operations' FROM dual
        UNION ALL SELECT 7, 'BRANCH_GENERAL_CONTROLS',
                         'Branch General Controls' FROM dual
        UNION ALL SELECT 8, 'ACCOUNTING_REPORTING_RECON',
                         'Accounting / Reporting / Reconciliation' FROM dual
        UNION ALL SELECT 9, 'MANAGEMENT_AUDIT', 'Management Audit' FROM dual
        UNION ALL SELECT 10, 'IS_AUDIT', 'IS Audit' FROM dual
      ),
      identified AS
      (
        SELECT k.observation_id, o.annex AS annex_id,
               o.v_cat_nature_id AS violation_sub_id,
               k.audited_by_id
          FROM v_exec_kpi_lifecycle k
          JOIN t_au_observation o ON o.id = k.observation_id
         WHERE k.record_type = 'ISSUE'
           AND k.issue_eligible = 1
           AND k.issue_identified_on >= V_FROM
           AND k.issue_identified_on < V_TO_EXCLUSIVE
           AND (P_ENTITY_TYPE_ID IS NULL
                OR k.entity_type_id = P_ENTITY_TYPE_ID)
           AND (P_AUDITED_BY_ID IS NULL
                OR k.audited_by_id = P_AUDITED_BY_ID)
           AND (P_REPORTING_OFFICE_ID IS NULL
                OR k.entity_id = P_REPORTING_OFFICE_ID
                OR P_REPORTING_OFFICE_ID IN
                   (k.reporting_1, k.reporting_2, k.reporting_3,
                    k.reporting_4, k.reporting_5, k.reporting_6))
           AND (P_ENTITY_ID IS NULL OR k.entity_id = P_ENTITY_ID
                OR P_ENTITY_ID IN
                   (k.reporting_1, k.reporting_2, k.reporting_3,
                    k.reporting_4, k.reporting_5, k.reporting_6))
      ),
      matches AS
      (
        SELECT i.observation_id, m.category_code,
               ROW_NUMBER() OVER
                 (PARTITION BY i.observation_id
                  ORDER BY m.match_priority,
                           CASE m.source_type
                             WHEN 'VIOLATION_SUB' THEN 1
                             WHEN 'ANNEX' THEN 2
                             ELSE 3 END,
                           m.source_id) AS match_rank
          FROM identified i
          JOIN t_ias_cia_issue_nature_map m
            ON m.is_active = 'Y'
           AND ((m.source_type = 'VIOLATION_SUB'
                 AND m.source_id = i.violation_sub_id)
                OR (m.source_type = 'ANNEX'
                    AND m.source_id = i.annex_id)
                OR (m.source_type = 'AUDIT_PORTFOLIO'
                    AND m.source_id = i.audited_by_id))
      ),
      classified AS
      (
        SELECT i.observation_id, m.category_code
          FROM identified i
          LEFT JOIN matches m
            ON m.observation_id = i.observation_id
           AND m.match_rank = 1
      ),
      grouped AS
      (
        SELECT category_code, COUNT(*) AS issue_count
          FROM classified
         WHERE category_code IS NOT NULL
         GROUP BY category_code
      ),
      totals AS
      (
        SELECT COUNT(*) AS total_issues,
               NVL(SUM(CASE WHEN category_code IS NULL
                            THEN 1 ELSE 0 END), 0) AS unmapped_issues
          FROM classified
      )
      SELECT c.category_code, c.category_name, c.display_order,
             NVL(g.issue_count, 0) AS issue_count,
             t.total_issues, t.unmapped_issues,
             CASE WHEN t.total_issues = 0 THEN 'NO_ISSUES'
                  WHEN t.unmapped_issues > 0 THEN 'PARTIAL'
                  ELSE 'COMPLETE' END AS mapping_status
        FROM categories c
        LEFT JOIN grouped g ON g.category_code = c.category_code
        CROSS JOIN totals t
       ORDER BY c.display_order;
  END P_GET_CIA_ISSUE_NATURE;


END PKG_EXEC_DASHBOARD;
