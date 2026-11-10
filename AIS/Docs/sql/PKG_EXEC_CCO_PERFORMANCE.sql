-- Separate CCO-only read contract. Deploy under the executive reporting owner.
-- Call only after the IAS application validates the live CCO session, page and exact API grant.
-- Group is a verified reporting selector. The application derives authorization independently.
CREATE OR REPLACE PACKAGE PKG_EXEC_CCO_PERFORMANCE AS
  PROCEDURE P_GET_PERFORMANCE(
    P_START_DATE IN DATE,
    P_END_DATE IN DATE,
    P_GROUP_ID IN NUMBER,
    P_ROW_LIMIT IN NUMBER,
    IO_CURSOR OUT SYS_REFCURSOR);
END PKG_EXEC_CCO_PERFORMANCE;
/

CREATE OR REPLACE PACKAGE BODY PKG_EXEC_CCO_PERFORMANCE AS
  PROCEDURE P_GET_PERFORMANCE(
    P_START_DATE IN DATE,
    P_END_DATE IN DATE,
    P_GROUP_ID IN NUMBER,
    P_ROW_LIMIT IN NUMBER,
    IO_CURSOR OUT SYS_REFCURSOR) IS
    V_END_EXCLUSIVE DATE;
  BEGIN
    IF P_START_DATE IS NULL OR P_END_DATE IS NULL
       OR P_START_DATE <> TRUNC(P_START_DATE) OR P_END_DATE <> TRUNC(P_END_DATE)
       OR P_START_DATE > P_END_DATE OR P_END_DATE > TRUNC(SYSDATE)
       OR MONTHS_BETWEEN(TRUNC(P_END_DATE,'MM'), TRUNC(P_START_DATE,'MM')) >= 1200
       OR (P_GROUP_ID IS NOT NULL AND (P_GROUP_ID <= 0 OR P_GROUP_ID <> TRUNC(P_GROUP_ID)))
       OR P_ROW_LIMIT IS NULL OR P_ROW_LIMIT NOT BETWEEN 2 AND 1001
       OR P_ROW_LIMIT <> TRUNC(P_ROW_LIMIT) THEN
      RAISE_APPLICATION_ERROR(-20810, 'Invalid CCO performance filters');
    END IF;
    V_END_EXCLUSIVE := P_END_DATE + 1;
    OPEN IO_CURSOR FOR
      -- Match approved Report C/E movement dates. Classify B and D independently.
      WITH source_facts AS (
          SELECT DISTINCT audit_type, com_id, entity_id, effective_added_on, close_on, risk
            FROM v_exec_report_source
           WHERE audit_type IN ('B','D') AND effective_added_on < V_END_EXCLUSIVE
      ),
      paras AS (
          SELECT audit_type, com_id,
                 CASE WHEN COUNT(DISTINCT NVL(entity_id,0)) = 1 THEN MIN(entity_id) END entity_id,
                 MIN(effective_added_on) added_on, MIN(close_on) close_on, MIN(risk) risk,
                 CASE WHEN COUNT(DISTINCT NVL(entity_id,0)) > 1 THEN 1 ELSE 0 END entity_conflict,
                 CASE WHEN COUNT(DISTINCT effective_added_on) > 1
                        OR COUNT(DISTINCT NVL(close_on, DATE '0001-01-01')) > 1
                        OR COUNT(DISTINCT NVL(risk,0)) > 1 THEN 1 ELSE 0 END fact_conflict
            FROM source_facts
           GROUP BY audit_type, com_id
      ),
      org_paths AS (
          SELECT entity_id,
                 CASE WHEN COUNT(DISTINCT NVL(business_group_id,0)) = 1 THEN MIN(business_group_id) END group_id,
                 CASE WHEN COUNT(DISTINCT NVL(division_id,0)) = 1 THEN MIN(division_id) END division_id,
                 CASE WHEN COUNT(DISTINCT NVL(gm_office_id,0)) = 1 THEN MIN(gm_office_id) END gm_id,
                 CASE WHEN COUNT(DISTINCT NVL(business_group_id,0)) > 1
                        OR COUNT(DISTINCT NVL(division_id,0)) > 1
                        OR COUNT(DISTINCT NVL(gm_office_id,0)) > 1 THEN 1 ELSE 0 END path_conflict
            FROM v_exec_entity_scope
           GROUP BY entity_id
      ),
      classified AS (
          SELECT p.*, g.entity_id group_id, d.entity_id division_id, gm.entity_id gm_id,
                 CASE WHEN p.entity_conflict = 1 OR o.path_conflict = 1 THEN 'Conflicting'
                      WHEN g.entity_id IS NULL OR d.entity_id IS NULL OR gm.entity_id IS NULL THEN 'Unmapped'
                      ELSE 'Mapped' END mapping_quality
            FROM paras p
            LEFT JOIN org_paths o ON o.entity_id = p.entity_id
            LEFT JOIN t_auditee_entities g ON g.entity_id = o.group_id AND g.type_id = 18
            LEFT JOIN t_auditee_entities d ON d.entity_id = o.division_id AND d.type_id = 3
            LEFT JOIN t_auditee_entities gm ON gm.entity_id = o.gm_id AND gm.type_id = 21
           WHERE (P_GROUP_ID IS NULL OR g.entity_id = P_GROUP_ID)
             AND (p.close_on IS NULL OR p.close_on >= P_START_DATE
                  OR p.added_on >= P_START_DATE OR p.fact_conflict = 1)
      ),
      totals AS (
          SELECT group_id, division_id, gm_id, audit_type, mapping_quality,
                 CASE WHEN GROUPING(group_id) = 1 THEN 'TOTAL'
                      WHEN GROUPING(division_id) = 1 THEN 'GROUP'
                      WHEN GROUPING(gm_id) = 1 THEN 'DIVISION' ELSE 'GM' END row_kind,
                 COUNT(CASE WHEN added_on < P_START_DATE
                              AND (close_on IS NULL OR close_on >= P_START_DATE) THEN 1 END) opening_balance,
                 COUNT(CASE WHEN added_on >= P_START_DATE THEN 1 END) paras_added,
                 COUNT(CASE WHEN close_on >= P_START_DATE AND close_on < V_END_EXCLUSIVE THEN 1 END) settled_paras,
                 COUNT(CASE WHEN close_on IS NULL OR close_on >= V_END_EXCLUSIVE THEN 1 END) outstanding_paras,
                 COUNT(CASE WHEN (close_on IS NULL OR close_on >= V_END_EXCLUSIVE) AND risk = 1 THEN 1 END) high_risk_open,
                 COUNT(CASE WHEN mapping_quality <> 'Mapped' THEN 1 END) mapping_exceptions,
                 NVL(MAX(fact_conflict),0) fact_conflict
            FROM classified
           GROUP BY GROUPING SETS ((), (group_id), (group_id,division_id),
                                  (group_id,division_id,gm_id,audit_type,mapping_quality))
      )
      SELECT t.*, g.name group_name, d.name division_name, gm.name gm_name
        FROM totals t
        LEFT JOIN t_auditee_entities g ON g.entity_id = t.group_id
        LEFT JOIN t_auditee_entities d ON d.entity_id = t.division_id
        LEFT JOIN t_auditee_entities gm ON gm.entity_id = t.gm_id
       WHERE ROWNUM <= P_ROW_LIMIT;
  END P_GET_PERFORMANCE;
END PKG_EXEC_CCO_PERFORMANCE;
/
