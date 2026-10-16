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

END PKG_EXEC_DASHBOARD;
