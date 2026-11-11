-- Deploy under the existing IAS reporting schema and execute through the existing IAS Oracle connection.
-- The application enforces CCO/Super User role, page and exact API grants; this package
-- independently checks the same active IAS session and assigned context before reading.
CREATE OR REPLACE PACKAGE PKG_EXEC_CCO_PERFORMANCE AUTHID DEFINER AS
  PROCEDURE P_GET_PERFORMANCE(
    P_USER_ID IN NUMBER,
    P_CONTEXT_ID IN NUMBER,
    P_ROLE_ID IN NUMBER,
    P_POSTING_ID IN NUMBER,
    P_SESSION_ID IN VARCHAR2,
    P_START_DATE IN DATE,
    P_END_DATE IN DATE,
    P_GROUP_ID IN NUMBER,
    P_ROW_LIMIT IN NUMBER,
    IO_CURSOR OUT SYS_REFCURSOR);
END PKG_EXEC_CCO_PERFORMANCE;
/

CREATE OR REPLACE PACKAGE BODY PKG_EXEC_CCO_PERFORMANCE AS
  PROCEDURE P_GET_PERFORMANCE(
    P_USER_ID IN NUMBER,
    P_CONTEXT_ID IN NUMBER,
    P_ROLE_ID IN NUMBER,
    P_POSTING_ID IN NUMBER,
    P_SESSION_ID IN VARCHAR2,
    P_START_DATE IN DATE,
    P_END_DATE IN DATE,
    P_GROUP_ID IN NUMBER,
    P_ROW_LIMIT IN NUMBER,
    IO_CURSOR OUT SYS_REFCURSOR) IS
    V_END_EXCLUSIVE DATE;
    V_MATCHES NUMBER;
  BEGIN
    -- Identity and selectors come from the authenticated IAS server context, not HTTP input.
    -- Do not trust the role/context arguments alone: require an active matching IAS DB session.
    IF P_USER_ID IS NULL OR P_CONTEXT_ID IS NULL OR P_SESSION_ID IS NULL
       OR TRIM(P_SESSION_ID) IS NULL
       OR P_ROLE_ID IS NULL OR P_ROLE_ID NOT IN (1, 38) THEN
      RAISE_APPLICATION_ERROR(-20811, 'CCO performance context is not authorized');
    END IF;

    SELECT COUNT(*) INTO V_MATCHES
      FROM T_USER_CONTEXT_ASSIGNMENT a
     WHERE a.USER_ID = P_USER_ID
       AND a.USER_CONTEXT_ID = P_CONTEXT_ID
       AND a.ROLE_ID = P_ROLE_ID
       AND (a.ENTITY_ID = P_POSTING_ID OR (a.ENTITY_ID IS NULL AND P_POSTING_ID IS NULL))
       AND a.IS_ACTIVE = 'Y'
       AND (a.EFFECTIVE_FROM IS NULL OR a.EFFECTIVE_FROM <= TRUNC(SYSDATE))
       AND (a.EFFECTIVE_TO IS NULL OR a.EFFECTIVE_TO >= TRUNC(SYSDATE))
       AND EXISTS (
         SELECT 1 FROM T_USER_SESSION s
         JOIN T_USER u ON u.PPNO = s.USER_PP_NUMBER AND u.USERID = a.USER_ID
          WHERE s.SESSION_ID = P_SESSION_ID
            AND s.SESSION_ACTIVE = 'Y' AND s.LOGGED_OUT_DATE IS NULL
            AND s.ROLE_ID = P_ROLE_ID AND s.USER_CONTEXT_ID = P_CONTEXT_ID
            AND (s.ENTITY_ID = P_POSTING_ID OR (s.ENTITY_ID IS NULL AND P_POSTING_ID IS NULL))
            AND u.ISACTIVE = 'Y'
       );

    IF V_MATCHES <> 1 THEN
      RAISE_APPLICATION_ERROR(-20811, 'CCO performance context is not authorized');
    END IF;

    IF P_START_DATE IS NULL OR P_END_DATE IS NULL
       OR P_START_DATE <> TRUNC(P_START_DATE) OR P_END_DATE <> TRUNC(P_END_DATE)
       OR P_START_DATE > P_END_DATE OR P_END_DATE > TRUNC(SYSDATE)
       OR MONTHS_BETWEEN(TRUNC(P_END_DATE,'MM'), TRUNC(P_START_DATE,'MM')) >= 1200
       OR (P_GROUP_ID IS NOT NULL AND (P_GROUP_ID <= 0 OR P_GROUP_ID <> TRUNC(P_GROUP_ID)))
       OR P_ROW_LIMIT IS NULL OR P_ROW_LIMIT NOT BETWEEN 2 AND 1001
       OR P_ROW_LIMIT <> TRUNC(P_ROW_LIMIT) THEN
      RAISE_APPLICATION_ERROR(-20810, 'Invalid CCO performance filters');
    END IF;

    IF P_GROUP_ID IS NOT NULL THEN
      SELECT COUNT(*) INTO V_MATCHES
        FROM t_auditee_entities e
       WHERE e.entity_id = P_GROUP_ID AND e.type_id = 18
         AND EXISTS (
           SELECT MIN(m.c_type_id) FROM t_auditee_entities_maping m
            WHERE m.entity_id = e.entity_id AND NVL(m.status, 'Y') = 'Y'
            HAVING COUNT(DISTINCT NVL(m.c_type_id, -1)) = 1 AND MIN(m.c_type_id) = 18
         );
      IF V_MATCHES <> 1 THEN
        RAISE_APPLICATION_ERROR(-20810, 'Invalid CCO performance Group');
      END IF;
    END IF;

    V_END_EXCLUSIVE := P_END_DATE + 1;
    OPEN IO_CURSOR FOR
      WITH source_facts AS (
          -- Same populations and effective date as Report C (B) and Report E (D).
          SELECT audit_type, com_id, entity_id, entity_type_id, effective_added_on, close_on,
                 risk, business_group_id, division_id, gm_office_id, region_id, reporting_1
            FROM v_exec_report_source
           WHERE audit_type IN ('B','D') AND effective_added_on < V_END_EXCLUSIVE
      ),
      movement_versions AS (
          SELECT DISTINCT audit_type, com_id, effective_added_on, close_on,
                          CASE WHEN risk = 1 THEN 1 ELSE 0 END high_risk
            FROM source_facts
      ),
      movements AS (
          SELECT com_id, MIN(audit_type) audit_type, MIN(effective_added_on) added_on,
                 MIN(close_on) close_on, MIN(high_risk) high_risk,
                 MAX(CASE WHEN close_on IS NULL OR close_on >= P_START_DATE
                               OR effective_added_on >= P_START_DATE THEN 1 ELSE 0 END) in_period,
                 CASE WHEN com_id IS NULL OR COUNT(*) <> 1 THEN 1 ELSE 0 END fact_conflict
            FROM movement_versions
           GROUP BY com_id
      ),
      integrity AS (
          -- A COM_ID in both populations, or with conflicting movement evidence,
          -- makes the report unverifiable. Reject even if a Group would hide it.
          SELECT NVL(MAX(fact_conflict), 0) fact_conflict FROM movements WHERE in_period = 1
      ),
      e_candidates AS (
          SELECT DISTINCT r.entity_id division_id, r.parent_id group_id,
                          r.p_name group_name, r.c_name division_name
            FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r
           WHERE EXISTS (
               SELECT 1 FROM t_auditee_entities_maping m
                WHERE m.entity_id = r.entity_id AND m.parent_id = r.parent_id
                  AND m.p_name = r.p_name AND m.c_name = r.c_name AND NVL(m.status,'Y') = 'Y'
           )
      ),
      e_map AS (
          SELECT division_id, COUNT(*) path_count, MAX(group_id) group_id,
                 MAX(group_name) group_name, MAX(division_name) division_name
            FROM e_candidates
           GROUP BY division_id
      ),
      e_group_names AS (
          SELECT group_id, CASE WHEN COUNT(DISTINCT group_name) = 1 THEN MIN(group_name) END group_name
            FROM e_map WHERE path_count = 1
           GROUP BY group_id
      ),
      candidate_paths AS (
          SELECT DISTINCT s.audit_type, s.com_id,
                 CASE WHEN s.audit_type = 'B' THEN g.entity_id
                      WHEN em.path_count = 1 AND em.group_id IS NOT NULL
                           AND (s.business_group_id IS NULL OR s.business_group_id = em.group_id)
                      THEN em.group_id END group_id,
                 CASE WHEN s.audit_type = 'B' THEN d.entity_id
                      WHEN em.path_count = 1 AND em.group_id IS NOT NULL
                           AND (s.business_group_id IS NULL OR s.business_group_id = em.group_id)
                      THEN em.division_id END division_id,
                 gm.entity_id gm_id,
                 CASE WHEN s.audit_type = 'D' THEN s.entity_id
                      WHEN s.entity_type_id = 6 THEN COALESCE(s.region_id, parent_region.entity_id)
                      WHEN s.entity_type_id IS NOT NULL THEN s.entity_id
                      ELSE s.region_id END item_id,
                 CASE WHEN s.audit_type = 'D' AND
                           (em.path_count > 1 OR (em.path_count = 1 AND s.business_group_id <> em.group_id))
                      THEN 1 ELSE 0 END path_conflict
            FROM source_facts s
            LEFT JOIN t_auditee_entities g ON s.audit_type = 'B' AND g.entity_id = s.business_group_id AND g.type_id = 18
            LEFT JOIN t_auditee_entities d ON s.audit_type = 'B' AND d.entity_id = s.division_id AND d.type_id = 3
            LEFT JOIN t_auditee_entities gm ON s.audit_type = 'B' AND gm.type_id = 21
                 AND gm.entity_id = CASE WHEN s.entity_type_id = 21 THEN s.entity_id ELSE s.gm_office_id END
            LEFT JOIN t_auditee_entities parent_region ON s.audit_type = 'B' AND s.entity_type_id = 6
                 AND parent_region.entity_id = s.reporting_1 AND parent_region.type_id = 21
            LEFT JOIN e_map em ON s.audit_type = 'D'
                 AND em.division_id = COALESCE(s.division_id,
                     CASE WHEN s.entity_type_id IN (3,4) THEN s.entity_id END)
      ),
      paths AS (
          SELECT audit_type, com_id,
                 CASE WHEN COUNT(*) = 1 THEN MIN(group_id) END group_id,
                 CASE WHEN COUNT(*) = 1 THEN MIN(division_id) END division_id,
                 CASE WHEN COUNT(*) = 1 THEN MIN(gm_id) END gm_id,
                 CASE WHEN COUNT(DISTINCT NVL(item_id,-1)) = 1 THEN MIN(item_id) END item_id,
                 CASE WHEN COUNT(*) <> 1 OR MAX(path_conflict) = 1 THEN 1 ELSE 0 END path_conflict
            FROM candidate_paths
           GROUP BY audit_type, com_id
      ),
      entity_names AS (
          SELECT entity_id, CASE WHEN COUNT(DISTINCT name) = 1 THEN MIN(name) END entity_name
            FROM t_auditee_entities
           GROUP BY entity_id
      ),
      classified AS (
          SELECT m.*, p.group_id, p.division_id, p.gm_id, p.item_id,
                 CASE WHEN p.path_conflict = 1 THEN 'Conflicting'
                      WHEN p.group_id IS NULL OR p.division_id IS NULL OR n.entity_name IS NULL
                           OR (m.audit_type = 'B' AND p.gm_id IS NULL) THEN 'Unmapped'
                      ELSE 'Mapped' END mapping_quality
            FROM movements m
            JOIN paths p ON p.com_id = m.com_id AND p.audit_type = m.audit_type
            LEFT JOIN entity_names n ON n.entity_id = p.item_id
           WHERE m.fact_conflict = 0
             AND (P_GROUP_ID IS NULL OR p.group_id = P_GROUP_ID)
             AND m.in_period = 1
      ),
      totals AS (
          SELECT group_id, division_id, gm_id, item_id, audit_type, mapping_quality,
                 CASE WHEN GROUPING(group_id) = 1 AND GROUPING(audit_type) = 1 THEN 'TOTAL'
                      WHEN GROUPING(group_id) = 1 THEN 'POPULATION'
                      WHEN GROUPING(division_id) = 1 THEN 'GROUP'
                      WHEN GROUPING(gm_id) = 1 THEN 'DIVISION'
                      WHEN GROUPING(item_id) = 1 THEN 'GM' ELSE 'DETAIL' END row_kind,
                 COUNT(CASE WHEN added_on < P_START_DATE
                              AND (close_on IS NULL OR close_on >= P_START_DATE) THEN 1 END) opening_balance,
                 COUNT(CASE WHEN added_on >= P_START_DATE THEN 1 END) paras_added,
                 COUNT(CASE WHEN close_on >= P_START_DATE AND close_on < V_END_EXCLUSIVE THEN 1 END) settled_paras,
                 COUNT(CASE WHEN close_on IS NULL OR close_on >= V_END_EXCLUSIVE THEN 1 END) outstanding_paras,
                 COUNT(CASE WHEN (close_on IS NULL OR close_on >= V_END_EXCLUSIVE) AND high_risk = 1 THEN 1 END) high_risk_open,
                 COUNT(CASE WHEN mapping_quality <> 'Mapped' THEN 1 END) mapping_exceptions
            FROM classified
           GROUP BY GROUPING SETS ((), (audit_type), (group_id), (group_id,division_id),
                                  (group_id,division_id,audit_type,gm_id),
                                  (group_id,division_id,audit_type,gm_id,item_id,mapping_quality))
      )
      SELECT t.*, i.fact_conflict,
             COALESCE(g.entity_name, eg.group_name) group_name,
             COALESCE(d.entity_name, ed.division_name) division_name,
             gm.entity_name gm_name,
             CASE WHEN t.audit_type = 'D' AND t.item_id = t.division_id AND item.entity_name IS NOT NULL
                  THEN item.entity_name || ' (division level)' ELSE item.entity_name END item_name
        FROM totals t
        CROSS JOIN integrity i
        LEFT JOIN entity_names g ON g.entity_id = t.group_id
        LEFT JOIN entity_names d ON d.entity_id = t.division_id
        LEFT JOIN entity_names gm ON gm.entity_id = t.gm_id
        LEFT JOIN entity_names item ON item.entity_id = t.item_id
        LEFT JOIN e_group_names eg ON eg.group_id = t.group_id
        LEFT JOIN e_map ed ON ed.division_id = t.division_id AND ed.group_id = t.group_id AND ed.path_count = 1
       WHERE (t.row_kind <> 'GM' OR t.audit_type = 'B')
         AND ROWNUM <= P_ROW_LIMIT;
  END P_GET_PERFORMANCE;
END PKG_EXEC_CCO_PERFORMANCE;
/
