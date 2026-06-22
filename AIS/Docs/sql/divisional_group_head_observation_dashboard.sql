CREATE OR REPLACE VIEW V_DASH_HEAD_OBS_RISK_BASE AS
SELECT c.com_id,
       c.entity_id AS department_id,
       e.name AS department_name,
       NVL(c.com_cycle, 0) AS com_cycle,
       c.risk
  FROM AIS_T_AU_POST_COMPLIANCE c
 INNER JOIN T_AUDITEE_ENTITIES e
    ON e.entity_id = c.entity_id;
/

CREATE OR REPLACE PROCEDURE P_GET_HEAD_OBS_RISK_SUMMARY
(
    P_ROLE_ID      IN NUMBER,
    P_ENT_ID       IN NUMBER,
    P_CYCLE_BUCKET IN VARCHAR2,
    IO_CURSOR      OUT SYS_REFCURSOR
)
AS
    V_BUCKET    VARCHAR2(20) := UPPER(TRIM(P_CYCLE_BUCKET));
BEGIN
    IF P_ENT_ID IS NULL
       OR P_ENT_ID <= 0
       OR P_ROLE_ID NOT IN (1, 3, 14)
       OR V_BUCKET IS NULL
       OR V_BUCKET NOT IN ('OVER_THREE', 'ZERO')
    THEN
        OPEN IO_CURSOR FOR
            SELECT CAST(NULL AS NUMBER) AS department_id,
                   CAST(NULL AS VARCHAR2(100)) AS department_name,
                   0 AS total_observations,
                   0 AS high_risk,
                   0 AS medium_risk,
                   0 AS low_risk,
                   0 AS unrated_risk,
                   CAST(NULL AS VARCHAR2(10)) AS risk_status
              FROM dual
             WHERE 1 = 0;
        RETURN;
    END IF;

    OPEN IO_CURSOR FOR
        SELECT department_id,
               department_name,
               COUNT(*) AS total_observations,
               SUM(CASE WHEN risk = 1 THEN 1 ELSE 0 END) AS high_risk,
               SUM(CASE WHEN risk = 2 THEN 1 ELSE 0 END) AS medium_risk,
               SUM(CASE WHEN risk = 3 THEN 1 ELSE 0 END) AS low_risk,
               SUM(CASE WHEN risk IS NULL OR risk NOT IN (1, 2, 3) THEN 1 ELSE 0 END) AS unrated_risk,
               CASE
                   WHEN SUM(CASE WHEN risk = 1 THEN 1 ELSE 0 END) > 0 THEN 'High'
                   WHEN SUM(CASE WHEN risk = 2 THEN 1 ELSE 0 END) > 0 THEN 'Medium'
                   WHEN SUM(CASE WHEN risk = 3 THEN 1 ELSE 0 END) > 0 THEN 'Low'
                   ELSE 'Unrated'
               END AS risk_status
          FROM V_DASH_HEAD_OBS_RISK_BASE b
         WHERE ((V_BUCKET = 'OVER_THREE' AND com_cycle > 3)
             OR (V_BUCKET = 'ZERO' AND com_cycle = 0))
           AND (department_id = P_ENT_ID
             OR EXISTS (
                    SELECT 1
                      FROM T_AUDITEE_ENTITIES_MAPING m
                     WHERE m.entity_id = b.department_id
                       AND NVL(m.status, 'Y') = 'Y'
                       AND (m.parent_id = P_ENT_ID
                         OR m.reporting = P_ENT_ID
                         OR m.gm_office = P_ENT_ID
                         OR m.div_office = P_ENT_ID
                         OR m.b_group = P_ENT_ID)
                ))
         GROUP BY department_id, department_name
         ORDER BY department_name;
END P_GET_HEAD_OBS_RISK_SUMMARY;
/

CREATE OR REPLACE PROCEDURE P_GET_HEAD_OBS_RISK_DETAILS
(
    P_ROLE_ID       IN NUMBER,
    P_ENT_ID        IN NUMBER,
    P_DEPARTMENT_ID IN NUMBER,
    P_CYCLE_BUCKET  IN VARCHAR2,
    IO_CURSOR       OUT SYS_REFCURSOR
)
AS
    V_BUCKET VARCHAR2(20) := UPPER(TRIM(P_CYCLE_BUCKET));
BEGIN
    IF P_ENT_ID IS NULL
       OR P_ENT_ID <= 0
       OR P_DEPARTMENT_ID IS NULL
       OR P_DEPARTMENT_ID <= 0
       OR P_ROLE_ID NOT IN (1, 3, 14)
       OR V_BUCKET NOT IN ('OVER_THREE', 'ZERO')
    THEN
        OPEN IO_CURSOR FOR
            SELECT CAST(NULL AS NUMBER) AS com_id,
                   CAST(NULL AS VARCHAR2(50)) AS audit_period,
                   CAST(NULL AS VARCHAR2(100)) AS para_no,
                   CAST(NULL AS VARCHAR2(4000)) AS gist_of_paras,
                   CAST(NULL AS VARCHAR2(20)) AS risk
              FROM dual
             WHERE 1 = 0;
        RETURN;
    END IF;

    OPEN IO_CURSOR FOR
        SELECT c.com_id,
               c.audit_period,
               c.para_no,
               c.gist_of_paras,
               CASE c.risk
                   WHEN 1 THEN 'High'
                   WHEN 2 THEN 'Medium'
                   WHEN 3 THEN 'Low'
                   ELSE 'Unrated'
               END AS risk
          FROM AIS_T_AU_POST_COMPLIANCE c
         WHERE c.entity_id = P_DEPARTMENT_ID
           AND ((V_BUCKET = 'OVER_THREE' AND NVL(c.com_cycle, 0) > 3)
             OR (V_BUCKET = 'ZERO' AND NVL(c.com_cycle, 0) = 0))
           AND (P_DEPARTMENT_ID = P_ENT_ID
             OR EXISTS (
                    SELECT 1
                      FROM T_AUDITEE_ENTITIES_MAPING m
                     WHERE m.entity_id = P_DEPARTMENT_ID
                       AND NVL(m.status, 'Y') = 'Y'
                       AND (m.parent_id = P_ENT_ID
                         OR m.reporting = P_ENT_ID
                         OR m.gm_office = P_ENT_ID
                         OR m.div_office = P_ENT_ID
                         OR m.b_group = P_ENT_ID)
                ))
         ORDER BY c.audit_period DESC, c.para_no, c.com_id;
END P_GET_HEAD_OBS_RISK_DETAILS;
/
