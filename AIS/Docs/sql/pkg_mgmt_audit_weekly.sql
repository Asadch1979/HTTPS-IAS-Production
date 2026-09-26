CREATE OR REPLACE PACKAGE PKG_MGMT_AUDIT_WEEKLY AS

  PROCEDURE P_GET_MGMT_WEEKLY_DIVISIONS(P_FROM_DATE IN DATE,
                                        P_TO_DATE   IN DATE,
                                        IO_CURSOR   OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MGMT_WEEKLY_DIVISION_DATA(P_DIVISION_ID IN NUMBER,
                                            P_FROM_DATE   IN DATE,
                                            P_TO_DATE     IN DATE,
                                            IO_CURSOR     OUT SYS_REFCURSOR);

END PKG_MGMT_AUDIT_WEEKLY;
/
CREATE OR REPLACE PACKAGE BODY PKG_MGMT_AUDIT_WEEKLY AS

  ------------------------------------------------------------------
  -- Validate reporting period
  ------------------------------------------------------------------
  PROCEDURE VALIDATE_PERIOD(P_FROM_DATE IN DATE, P_TO_DATE IN DATE) IS
  BEGIN
    IF P_FROM_DATE IS NULL OR P_TO_DATE IS NULL THEN
      RAISE_APPLICATION_ERROR(-20120,
                              'Management Audit weekly reporting dates cannot be null.');
    END IF;
  
    IF TRUNC(P_TO_DATE) <= TRUNC(P_FROM_DATE) THEN
      RAISE_APPLICATION_ERROR(-20121,
                              'Management Audit weekly To Date must be greater than From Date.');
    END IF;
  END VALIDATE_PERIOD;

  ------------------------------------------------------------------
  -- 1. Return only those Divisions for which an email is required.
  --
  -- P_FROM_DATE = inclusive
  -- P_TO_DATE   = exclusive
  --
  -- Example:
  -- 21-Sep-2026 to 28-Sep-2026 means:
  -- >= 21-Sep-2026 00:00
  -- <  28-Sep-2026 00:00
  --
  -- If only 12 out of 17 Divisions have qualifying decisions,
  -- exactly 12 rows are returned.
  ------------------------------------------------------------------
  PROCEDURE P_GET_MGMT_WEEKLY_DIVISIONS(P_FROM_DATE IN DATE,
                                        P_TO_DATE   IN DATE,
                                        IO_CURSOR   OUT SYS_REFCURSOR) IS
  BEGIN
  
    VALIDATE_PERIOD(P_FROM_DATE, P_TO_DATE);
  
    OPEN IO_CURSOR FOR
    
      WITH DECISION_EVENTS AS
       (SELECT W.HIST_ID AS DECISION_HISTORY_ID,
               H.COM_ID,
               H.COM_CYCLE,
               PC.ENTITY_ID,
               PC.AUDITED_BY,
               
               W.DIVISION_ID,
               W.DIVISION_NAME,
               TRIM(W.DIVISION_EMAIL) AS TO_EMAIL,
               TRIM(W.REPORTING_EMAIL) AS CC_EMAIL,
               
               W.COM_STATUS,
               W.DECISION_ON,
               
               ROW_NUMBER() OVER(PARTITION BY H.COM_ID ORDER BY W.DECISION_ON DESC, W.HIST_ID DESC) AS RN
        
          FROM V_IAS_MGMT_WEEKLY_DATA W
        
         INNER JOIN AIS_T_AU_POST_COMPLIANCE_HISTORY H
            ON H.HIST_ID = W.HIST_ID
        
         INNER JOIN AIS_T_AU_POST_COMPLIANCE PC
            ON PC.COM_ID = H.COM_ID
        
         WHERE W.DECISION_ON >= TRUNC(P_FROM_DATE)
           AND W.DECISION_ON < TRUNC(P_TO_DATE)
              
           AND W.COM_STATUS IN (16, 12, 15, 18)
              
           AND PC.AUDITED_BY IN (112242, 112248)
              
           AND W.DIVISION_ID IS NOT NULL
              
           --AND TRIM(W.DIVISION_EMAIL) IS NOT NULL
           ),
      
      LATEST_DECISION AS
       (SELECT * FROM DECISION_EVENTS WHERE RN = 1),
      
      DIVISION_SUMMARY AS
       (SELECT DIVISION_ID,
               
               MAX(DIVISION_NAME) AS DIVISION_NAME,
               
               MAX(TO_EMAIL) AS TO_EMAIL,
               
               SUM(CASE
                     WHEN COM_STATUS = 16 THEN
                      1
                     ELSE
                      0
                   END) AS SETTLED_COUNT,
               
               SUM(CASE
                     WHEN COM_STATUS IN (12, 15, 18) THEN
                      1
                     ELSE
                      0
                   END) AS REJECTED_COUNT,
               
               COUNT(*) AS TOTAL_COUNT
        
          FROM LATEST_DECISION
        
         GROUP BY DIVISION_ID),
      
      DIVISION_CC AS
       (SELECT DIVISION_ID,
               
               LISTAGG(CC_EMAIL, ';') WITHIN GROUP(ORDER BY CC_EMAIL) AS CC_EMAIL
        
          FROM (SELECT DISTINCT DIVISION_ID, CC_EMAIL
                
                  FROM LATEST_DECISION
                
                 WHERE CC_EMAIL IS NOT NULL)
        
         GROUP BY DIVISION_ID)
      
      SELECT D.DIVISION_ID,
             D.DIVISION_NAME,
             
             D.TO_EMAIL,
             
             C.CC_EMAIL,
             
             D.SETTLED_COUNT,
             D.REJECTED_COUNT,
             D.TOTAL_COUNT
      
        FROM DIVISION_SUMMARY D
      
        LEFT JOIN DIVISION_CC C
          ON C.DIVISION_ID = D.DIVISION_ID
      
       ORDER BY D.DIVISION_NAME;
  
  END P_GET_MGMT_WEEKLY_DIVISIONS;

  ------------------------------------------------------------------
  -- 2. Return the actual latest Settled / Rejected decisions
  --    for one selected Division.
  --
  -- The same para is returned once only.
  --
  -- Example:
  -- Tuesday = Rejected
  -- Friday  = Settled
  --
  -- Only Friday / Settled will be returned.
  ------------------------------------------------------------------
  PROCEDURE P_GET_MGMT_WEEKLY_DIVISION_DATA(P_DIVISION_ID IN NUMBER,
                                            P_FROM_DATE   IN DATE,
                                            P_TO_DATE     IN DATE,
                                            IO_CURSOR     OUT SYS_REFCURSOR) IS
  BEGIN
  
    IF P_DIVISION_ID IS NULL THEN
      RAISE_APPLICATION_ERROR(-20122,
                              'Management Audit Division ID cannot be null.');
    END IF;
  
    VALIDATE_PERIOD(P_FROM_DATE, P_TO_DATE);
  
    OPEN IO_CURSOR FOR
    
      WITH DECISION_EVENTS AS
       (SELECT W.HIST_ID AS DECISION_HISTORY_ID,
               
               H.COM_ID,
               H.COM_CYCLE,
               
               PC.ENTITY_ID,
               PC.AUDITED_BY,
               
               W.DIVISION_ID,
               W.DIVISION_NAME,
               
               TRIM(W.DIVISION_EMAIL) AS TO_EMAIL,
               TRIM(W.REPORTING_EMAIL) AS CC_EMAIL,
               
               W.ENTITY_NAME,
               W.AUDIT_PERIOD,
               W.PARA_NO,
               W.TITLE,
               
               W.SUBMITTED_ON,
               W.DECISION_ON,
               
               W.COM_STATUS,
               
               CASE
                 WHEN W.COM_STATUS = 16 THEN
                  'SETTLED'
               
                 WHEN W.COM_STATUS IN (12, 15, 18) THEN
                  'REJECTED'
               END AS DECISION_STATUS,
               
               CASE
                 WHEN W.COM_STATUS IN (12, 15, 18) THEN
                  W.REASON
                 ELSE
                  NULL
               END AS REASON,
               
               ROW_NUMBER() OVER(PARTITION BY H.COM_ID ORDER BY W.DECISION_ON DESC, W.HIST_ID DESC) AS RN
        
          FROM V_IAS_MGMT_WEEKLY_DATA W
        
         INNER JOIN AIS_T_AU_POST_COMPLIANCE_HISTORY H
            ON H.HIST_ID = W.HIST_ID
        
         INNER JOIN AIS_T_AU_POST_COMPLIANCE PC
            ON PC.COM_ID = H.COM_ID
        
         WHERE W.DECISION_ON >= TRUNC(P_FROM_DATE)
           AND W.DECISION_ON < TRUNC(P_TO_DATE)
              
           AND W.COM_STATUS IN (16, 12, 15, 18)
              
           AND PC.AUDITED_BY IN (112242, 112248)
              
           AND W.DIVISION_ID IS NOT NULL
              
           AND TRIM(W.DIVISION_EMAIL) IS NOT NULL)
      
      SELECT DECISION_HISTORY_ID,
             
             COM_ID,
             COM_CYCLE,
             
             ENTITY_ID,
             AUDITED_BY,
             
             DIVISION_ID,
             DIVISION_NAME,
             
             TO_EMAIL,
             CC_EMAIL,
             
             ENTITY_NAME,
             AUDIT_PERIOD,
             PARA_NO,
             TITLE,
             
             SUBMITTED_ON,
             DECISION_ON,
             
             DECISION_STATUS,
             COM_STATUS,
             
             REASON
      
        FROM DECISION_EVENTS
      
       WHERE RN = 1
            
         AND DIVISION_ID = P_DIVISION_ID
      
       ORDER BY CASE
                  WHEN COM_STATUS = 16 THEN
                   1
                  ELSE
                   2
                END,
                
                DECISION_ON,
                ENTITY_NAME,
                PARA_NO;
  
  END P_GET_MGMT_WEEKLY_DIVISION_DATA;

END PKG_MGMT_AUDIT_WEEKLY;
