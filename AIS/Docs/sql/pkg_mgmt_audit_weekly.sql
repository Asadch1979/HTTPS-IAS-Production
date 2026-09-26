CREATE OR REPLACE PACKAGE PKG_MGMT_AUDIT_WEEKLY AS

  PROCEDURE P_GET_MGMT_WEEKLY_DIVISIONS(P_FROM_DATE IN DATE,
                                        P_TO_DATE   IN DATE,
                                        IO_CURSOR   OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MGMT_WEEKLY_DIVISION_DATA(P_DIVISION_ID IN NUMBER,
                                            P_FROM_DATE   IN DATE,
                                            P_TO_DATE     IN DATE,
                                            IO_CURSOR     OUT SYS_REFCURSOR);

  PROCEDURE P_GET_MGMT_WEEKLY_NO_COMPLIANCE(P_DIVISION_ID IN NUMBER,
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
       (
        ----------------------------------------------------------
        -- Settled decisions falling within the reporting period
        ----------------------------------------------------------
        SELECT S.HIST_ID,
                S.COM_ID,
                S.COM_CYCLE,
                S.ENTITY_ID,
                S.AUDITED_BY,
                S.DECISION_ON,
                S.COM_STATUS,
                'SETTLED' AS CATEGORY
          FROM V_IAS_MGMT_SETTLED_EVENTS S
         WHERE S.DECISION_ON >= TRUNC(P_FROM_DATE)
           AND S.DECISION_ON < TRUNC(P_TO_DATE)
        
        UNION ALL
        
        ----------------------------------------------------------
        -- Referred Back decisions within the reporting period
        ----------------------------------------------------------
        SELECT R.HIST_ID,
                R.COM_ID,
                R.COM_CYCLE,
                R.ENTITY_ID,
                R.AUDITED_BY,
                R.DECISION_ON,
                R.COM_STATUS,
                'REFERRED_BACK' AS CATEGORY
          FROM V_IAS_MGMT_REFERRED_EVENTS R
         WHERE R.DECISION_ON >= TRUNC(P_FROM_DATE)
           AND R.DECISION_ON < TRUNC(P_TO_DATE)),
      
      --------------------------------------------------------------
      -- Rank Settled and Referred Back events together.
      --
      -- Example:
      -- Tuesday  = Referred Back
      -- Friday   = Settled
      --
      -- Only Friday / Settled will survive.
      --------------------------------------------------------------
      RANKED_DECISIONS AS
       (SELECT D.HIST_ID,
               D.COM_ID,
               D.COM_CYCLE,
               D.ENTITY_ID,
               D.AUDITED_BY,
               D.DECISION_ON,
               D.COM_STATUS,
               D.CATEGORY,
               
               ROW_NUMBER() OVER(PARTITION BY D.COM_ID ORDER BY D.DECISION_ON DESC, D.HIST_ID DESC) AS RN
        
          FROM DECISION_EVENTS D),
      
      --------------------------------------------------------------
      -- One latest decision per para during the reporting period
      --------------------------------------------------------------
      LATEST_DECISIONS AS
       (SELECT COM_ID, ENTITY_ID, CATEGORY
          FROM RANKED_DECISIONS
         WHERE RN = 1),
      
      --------------------------------------------------------------
      -- Open paras for which NO compliance submission was recorded
      -- during the reporting period.
      --
      -- COM_STATUS = 10 represents submission of compliance.
      --------------------------------------------------------------
      NO_COMPLIANCE AS
       (SELECT P.COM_ID, P.ENTITY_ID, 'NO_COMPLIANCE' AS CATEGORY
          FROM V_IAS_MGMT_OPEN_PARAS P
         WHERE
        ------------------------------------------------------
        -- Para must have existed before end of reporting
        -- period
        ------------------------------------------------------
         (P.PARA_ADDED_ON IS NULL OR P.PARA_ADDED_ON < TRUNC(P_TO_DATE))
        
        ------------------------------------------------------
        -- No compliance submitted during this period
        ------------------------------------------------------
      AND NOT EXISTS (SELECT 1
            FROM AIS_T_AU_POST_COMPLIANCE_HISTORY H
           WHERE H.COM_ID = P.COM_ID
             AND H.COM_STATUS = 10
             AND H.COMMENT_ON >= TRUNC(P_FROM_DATE)
             AND H.COMMENT_ON < TRUNC(P_TO_DATE))
        
        ------------------------------------------------------
        -- Do not repeat a para in No Compliance when it has
        -- already appeared as Settled / Referred Back during
        -- this reporting period.
        ------------------------------------------------------
      AND NOT EXISTS (SELECT 1
            FROM AIS_T_AU_POST_COMPLIANCE_HISTORY H
           WHERE H.COM_ID = P.COM_ID
             AND H.COM_STATUS IN (16, 12, 15, 18)
             AND H.COMMENT_ON >= TRUNC(P_FROM_DATE)
             AND H.COMMENT_ON < TRUNC(P_TO_DATE))),
      
      --------------------------------------------------------------
      -- Final population for the weekly email
      --------------------------------------------------------------
      QUALIFYING_PARAS AS
       (SELECT COM_ID, ENTITY_ID, CATEGORY
          FROM LATEST_DECISIONS
        
        UNION ALL
        
        SELECT COM_ID, ENTITY_ID, CATEGORY
          FROM NO_COMPLIANCE),
      
      --------------------------------------------------------------
      -- Convert ENTITY_ID into Division / recipient information
      --------------------------------------------------------------
      MAPPED_PARAS AS
       (SELECT Q.COM_ID,
               Q.ENTITY_ID,
               Q.CATEGORY,
               
               M.DIVISION_ID,
               M.DIVISION_NAME,
               
               TRIM(M.DIVISION_EMAIL) AS TO_EMAIL,
               TRIM(M.REPORTING_EMAIL) AS CC_EMAIL
        
          FROM QUALIFYING_PARAS Q
        
         INNER JOIN V_IAS_MGMT_AUDIT_NOTIFY_MAP M
            ON M.ENTITY_ID = Q.ENTITY_ID
        
         WHERE M.DIVISION_ID IS NOT NULL
           AND TRIM(M.DIVISION_EMAIL) IS NOT NULL),
      
      --------------------------------------------------------------
      -- One summary record per Division
      --------------------------------------------------------------
      DIVISION_SUMMARY AS
       (SELECT DIVISION_ID,
               
               MAX(DIVISION_NAME) AS DIVISION_NAME,
               
               MAX(TO_EMAIL) AS TO_EMAIL,
               
               SUM(CASE
                     WHEN CATEGORY = 'SETTLED' THEN
                      1
                     ELSE
                      0
                   END) AS SETTLED_COUNT,
               
               SUM(CASE
                     WHEN CATEGORY = 'REFERRED_BACK' THEN
                      1
                     ELSE
                      0
                   END) AS REJECTED_COUNT,
               
               SUM(CASE
                     WHEN CATEGORY = 'NO_COMPLIANCE' THEN
                      1
                     ELSE
                      0
                   END) AS NO_COMPLIANCE_COUNT,
               
               COUNT(*) AS TOTAL_COUNT
        
          FROM MAPPED_PARAS
        
         GROUP BY DIVISION_ID),
      
      --------------------------------------------------------------
      -- Oracle 18c compatible distinct CC aggregation
      --------------------------------------------------------------
      DIVISION_CC AS
       (SELECT DIVISION_ID,
               
               LISTAGG(CC_EMAIL, ';') WITHIN GROUP(ORDER BY CC_EMAIL) AS CC_EMAIL
        
          FROM (SELECT DISTINCT DIVISION_ID, CC_EMAIL
                  FROM MAPPED_PARAS
                 WHERE CC_EMAIL IS NOT NULL)
        
         GROUP BY DIVISION_ID)
      
      --------------------------------------------------------------
      -- Final Division queue
      --------------------------------------------------------------
      SELECT D.DIVISION_ID,
             D.DIVISION_NAME,
             
             D.TO_EMAIL,
             C.CC_EMAIL,
             
             D.SETTLED_COUNT,
             D.REJECTED_COUNT,
             D.NO_COMPLIANCE_COUNT,
             
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

  PROCEDURE P_GET_MGMT_WEEKLY_NO_COMPLIANCE(P_DIVISION_ID IN NUMBER,
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
    
      SELECT PC.COM_ID,
             PC.COM_CYCLE,
             
             PC.ENTITY_ID,
             PC.AUDITED_BY,
             
             M.DIVISION_ID,
             M.DIVISION_NAME,
             
             M.ENTITY_NAME,
             
             PC.AUDIT_PERIOD,
             PC.PARA_NO,
             PC.GIST_OF_PARAS AS TITLE,
             PC.RISK,
             
             (SELECT MAX(H.COMMENT_ON)
                FROM AIS_T_AU_POST_COMPLIANCE_HISTORY H
               WHERE H.COM_ID = PC.COM_ID
                 AND H.COM_STATUS = 10
                 AND H.COMMENT_ON < TRUNC(P_TO_DATE)) AS LAST_COMPLIANCE_SUBMITTED_ON,
             
             PC.PARA_STATUS,
             PC.COM_STATUS,
             PC.COM_STAGE
      
        FROM AIS_T_AU_POST_COMPLIANCE PC
      
       INNER JOIN V_IAS_MGMT_AUDIT_NOTIFY_MAP M
          ON M.ENTITY_ID = PC.ENTITY_ID
      
       WHERE PC.AUDITED_BY IN (112242, 112248)
            
            /* Para must presently remain outstanding */
         AND PC.PARA_STATUS = 8
            
            /* Selected Division only */
         AND M.DIVISION_ID = P_DIVISION_ID
            
            /* Valid Division notification mapping */
         AND M.DIVISION_ID IS NOT NULL
         AND TRIM(M.DIVISION_EMAIL) IS NOT NULL
            
            /* Para must have existed by the end of the reporting period */
         AND (PC.PARA_ADDED_ON IS NULL OR
             PC.PARA_ADDED_ON < TRUNC(P_TO_DATE))
            
            /* No compliance was submitted during this reporting period */
         AND NOT EXISTS
       (SELECT 1
                FROM AIS_T_AU_POST_COMPLIANCE_HISTORY H
               WHERE H.COM_ID = PC.COM_ID
                 AND H.COM_STATUS = 10
                 AND H.COMMENT_ON >= TRUNC(P_FROM_DATE)
                 AND H.COMMENT_ON < TRUNC(P_TO_DATE))
      
       ORDER BY M.ENTITY_NAME, PC.AUDIT_PERIOD, PC.PARA_NO;
  
  END P_GET_MGMT_WEEKLY_NO_COMPLIANCE;

END PKG_MGMT_AUDIT_WEEKLY;
