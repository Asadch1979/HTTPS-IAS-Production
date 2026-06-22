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
    V_ROLE_NAME VARCHAR2(300);
    V_BUCKET    VARCHAR2(20) := UPPER(TRIM(P_CYCLE_BUCKET));
BEGIN
    SELECT MAX(UPPER(NVL(g.group_name, g.description)))
      INTO V_ROLE_NAME
      FROM T_GROUPS g
     WHERE g.group_id = P_ROLE_ID
       AND NVL(g.status, 'Y') = 'Y';

    IF P_ENT_ID IS NULL
       OR P_ENT_ID <= 0
       OR V_ROLE_NAME IS NULL
       OR (V_ROLE_NAME NOT LIKE '%DIVISIONAL HEAD%'
           AND V_ROLE_NAME NOT LIKE '%DIVISION HEAD%'
           AND V_ROLE_NAME NOT LIKE '%GROUP HEAD%')
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
