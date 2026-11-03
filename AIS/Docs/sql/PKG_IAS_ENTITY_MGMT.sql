CREATE OR REPLACE PACKAGE PKG_IAS_ENTITY_MGMT AUTHID DEFINER AS
  PROCEDURE P_SEARCH_ENTITIES(P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                              P_SEARCH_TEXT    IN VARCHAR2 DEFAULT NULL,
                              P_STATUS         IN VARCHAR2 DEFAULT 'ALL',
                              P_OFFSET         IN NUMBER DEFAULT 0,
                              P_PAGE_SIZE      IN NUMBER DEFAULT 50,
                              O_CURSOR         OUT SYS_REFCURSOR);
  PROCEDURE P_GET_ENTITY_PROFILE(P_ENTITY_ID IN NUMBER,
                                 O_CURSOR    OUT SYS_REFCURSOR);
  PROCEDURE P_PREVIEW_ENTITY_CLOSURE(P_ENTITY_ID IN NUMBER,
                                     O_CURSOR    OUT SYS_REFCURSOR);
  PROCEDURE P_GET_ENTITY_CHANGE_LOG(P_ENTITY_ID IN NUMBER,
                                    O_CURSOR    OUT SYS_REFCURSOR);
  PROCEDURE P_GET_ENTITY_PARA_ARCHIVE(P_ENTITY_ID IN NUMBER,
                                      O_CURSOR    OUT SYS_REFCURSOR);
END PKG_IAS_ENTITY_MGMT;
/
CREATE OR REPLACE PACKAGE BODY PKG_IAS_ENTITY_MGMT AS
  PROCEDURE P_SEARCH_ENTITIES(P_ENTITY_TYPE_ID IN NUMBER DEFAULT NULL,
                              P_SEARCH_TEXT    IN VARCHAR2 DEFAULT NULL,
                              P_STATUS         IN VARCHAR2 DEFAULT 'ALL',
                              P_OFFSET         IN NUMBER DEFAULT 0,
                              P_PAGE_SIZE      IN NUMBER DEFAULT 50,
                              O_CURSOR         OUT SYS_REFCURSOR) IS
    V_OFFSET    NUMBER := NVL(P_OFFSET, 0);
    V_PAGE_SIZE NUMBER := NVL(P_PAGE_SIZE, 50);
    V_STATUS    VARCHAR2(10) := UPPER(TRIM(NVL(P_STATUS, 'ALL')));
  BEGIN
    IF V_OFFSET < 0 OR V_PAGE_SIZE < 1 OR V_PAGE_SIZE > 200 OR
       V_STATUS NOT IN ('ALL', 'ACTIVE', 'INACTIVE') THEN
      RAISE_APPLICATION_ERROR(-20710,
                              'Invalid paging or status parameters.');
    END IF;
    OPEN O_CURSOR FOR
      SELECT *
        FROM (SELECT e.entity_id,
                     e.code,
                     e.name,
                     e.type_id,
                     e.auditby_id,
                     e.active,
                     e.auditable,
                     ROW_NUMBER() OVER (ORDER BY e.name, e.entity_id) AS RN,
                     COUNT(*) OVER () AS TOTAL_ROWS
                FROM T_AUDITEE_ENTITIES e
               WHERE (P_ENTITY_TYPE_ID IS NULL OR
                     e.type_id = P_ENTITY_TYPE_ID)
                 AND (V_STATUS = 'ALL' OR
                     (V_STATUS = 'ACTIVE' AND e.active = 'Y') OR
                     (V_STATUS = 'INACTIVE' AND NVL(e.active, 'N') <> 'Y'))
                 AND (P_SEARCH_TEXT IS NULL OR
                     INSTR(UPPER(e.name), UPPER(TRIM(P_SEARCH_TEXT))) > 0 OR
                     INSTR(TO_CHAR(e.code), TRIM(P_SEARCH_TEXT)) > 0 OR
                     TO_CHAR(e.entity_id) = TRIM(P_SEARCH_TEXT)))
       WHERE RN > V_OFFSET
         AND RN <= V_OFFSET + V_PAGE_SIZE
       ORDER BY RN;
  END P_SEARCH_ENTITIES;

  PROCEDURE P_GET_ENTITY_PROFILE(P_ENTITY_ID IN NUMBER,
                                 O_CURSOR    OUT SYS_REFCURSOR) IS
  BEGIN
    IF NVL(P_ENTITY_ID, 0) <= 0 THEN
      RAISE_APPLICATION_ERROR(-20711, 'A valid entity ID is required.');
    END IF;
    -- May return multiple rows if multiple mapping records exist: intentionally
    -- surface this data-quality issue rather than silently choosing one.
    OPEN O_CURSOR FOR
      SELECT e.entity_id,
             e.code,
             e.name,
             e.description,
             e.type_id,
             e.auditby_id,
             e.active,
             e.auditable,
             e.auditor,
             e.iad,
             e.address,
             e.telephone,
             e.email_address,
             e.risk_id,
             e.size_id,
             m.parent_id,
             m.parent_code,
             m.child_code,
             m.auditedby     AS mapping_auditedby,
             m.status        AS mapping_status,
             m.p_name,
             m.c_name,
             m.p_type_id,
             m.c_type_id,
             m.r_key,
             m.reporting,
             m.gm_office,
             m.div_office,
             m.b_group
        FROM T_AUDITEE_ENTITIES e
        LEFT JOIN T_AUDITEE_ENTITIES_MAPING m
          ON m.entity_id = e.entity_id
       WHERE e.entity_id = P_ENTITY_ID;
  END P_GET_ENTITY_PROFILE;

  PROCEDURE P_PREVIEW_ENTITY_CLOSURE(P_ENTITY_ID IN NUMBER,
                                     O_CURSOR    OUT SYS_REFCURSOR) IS
  BEGIN
    IF NVL(P_ENTITY_ID, 0) <= 0 THEN
      RAISE_APPLICATION_ERROR(-20712, 'A valid entity ID is required.');
    END IF;
    -- This is an informational first-pass. Additional authorization, user
    -- assignments, scheduled jobs, child/parent hierarchy and engagement
    -- blockers must be verified before any closure implementation is enabled.
    OPEN O_CURSOR FOR
      SELECT e.entity_id,
             e.name,
             e.active,
             e.auditable,
             (SELECT COUNT(*)
                FROM AIS_T_AU_POST_COMPLIANCE p
               WHERE p.entity_id = e.entity_id) AS TOTAL_POST_COMPLIANCE,
             (SELECT COUNT(*)
                FROM AIS_T_AU_POST_COMPLIANCE p
               WHERE p.entity_id = e.entity_id
                 AND p.para_status = 8) AS OPEN_PARAS,
             (SELECT COUNT(*)
                FROM AIS_T_AU_POST_COMPLIANCE p
               WHERE p.entity_id = e.entity_id
                 AND p.para_status = 9) AS SETTLED_PARAS,
             (SELECT COUNT(*)
                FROM AIS_T_AU_POST_COMPLIANCE p
               WHERE p.entity_id = e.entity_id
                 AND p.para_status = 0) AS ALREADY_ZERO_PARAS,
             (SELECT COUNT(*)
                FROM AIS_T_AU_POST_COMPLIANCE p
               WHERE p.entity_id = e.entity_id
                 AND p.para_status = 6) AS STATUS_6_PARAS,
             (SELECT COUNT(*)
                FROM AIS_T_AU_POST_COMPLIANCE p
               WHERE p.entity_id = e.entity_id
                 AND p.para_status = 28) AS STATUS_28_PARAS,
             (SELECT COUNT(*)
                FROM AIS_T_AU_POST_COMPLIANCE p
               WHERE p.entity_id = e.entity_id
                 AND (p.para_status IS NULL OR
                     p.para_status NOT IN (0, 6, 8, 9, 28))) AS OTHER_PARA_STATUSES,
             (SELECT COUNT(*)
                FROM T_AU_PLAN_ENG a
               WHERE a.entity_id = e.entity_id) AS ALL_ENGAGEMENTS,
             (SELECT COUNT(*)
                FROM T_AU_PLAN_ENG a
               WHERE a.entity_id = e.entity_id
                 AND a.status IN (10, 11, 12)) AS IN_PROGRESS_ENGAGEMENTS,
             -- Production also contains statuses 1,4,5,13. Do not regard
             -- 10/11/12-only checks as proof of closure eligibility.
             (SELECT COUNT(*)
                FROM T_AU_PLAN_ENG a
               WHERE a.entity_id = e.entity_id
                 AND (a.status IS NULL OR a.status <> 14)) AS ENGAGEMENTS_NOT_FINAL_14,
             (SELECT COUNT(*)
                FROM T_AU_PLAN_ENG a
               WHERE a.entity_id = e.entity_id
                 AND a.status IN (1, 4, 5, 12, 13)) AS OTHER_ENGAGEMENT_STATES_FOR_REVIEW,
             (SELECT COUNT(*)
                FROM T_AUDITEE_ENTITIES_MAPING m
               WHERE m.parent_id = e.entity_id
                 AND m.entity_id <> e.entity_id
                 AND NVL(m.status, 'Y') = 'Y') AS ACTIVE_CHILD_MAPPINGS,
             (SELECT COUNT(*)
                FROM T_AUDITEE_ENTITIES_MAPING m
               WHERE m.entity_id = e.entity_id) AS MAPPING_ROWS,
             (SELECT COUNT(*)
                FROM T_IAS_ENTITY_PARA_ARCHIVE h
               WHERE h.entity_id = e.entity_id) AS PREVIOUS_ARCHIVE_ROWS
        FROM T_AUDITEE_ENTITIES e
       WHERE e.entity_id = P_ENTITY_ID;
  END P_PREVIEW_ENTITY_CLOSURE;

  PROCEDURE P_GET_ENTITY_CHANGE_LOG(P_ENTITY_ID IN NUMBER,
                                    O_CURSOR    OUT SYS_REFCURSOR) IS
  BEGIN
    IF NVL(P_ENTITY_ID, 0) <= 0 THEN
      RAISE_APPLICATION_ERROR(-20713, 'A valid entity ID is required.');
    END IF;
    OPEN O_CURSOR FOR
      SELECT log_id,
             request_id,
             batch_id,
             entity_id,
             target_entity_id,
             action_code,
             event_status,
             change_reason,
             reference_no,
             effective_on,
             actor_ppno,
             actor_role_id,
             actor_entity_id,
             authorized_by,
             authorized_on,
             executed_by,
             executed_on,
             recorded_on,
             source_module,
             correlation_id,
             result_message,
             old_values,
             new_values
        FROM T_IAS_ENTITY_CHANGE_LOG
       WHERE entity_id = P_ENTITY_ID
       ORDER BY recorded_on DESC, log_id DESC;
  END P_GET_ENTITY_CHANGE_LOG;

  PROCEDURE P_GET_ENTITY_PARA_ARCHIVE(P_ENTITY_ID IN NUMBER,
                                      O_CURSOR    OUT SYS_REFCURSOR) IS
  BEGIN
    IF NVL(P_ENTITY_ID, 0) <= 0 THEN
      RAISE_APPLICATION_ERROR(-20714, 'A valid entity ID is required.');
    END IF;
    OPEN O_CURSOR FOR
      SELECT archive_id,
             entity_log_id,
             archived_on,
             archived_by,
             closure_reason,
             com_id,
             entity_id,
             entity_code,
             old_para_id,
             new_para_id,
             para_no,
             para_status,
             para_added_on,
             setteled_on,
             setteled_by,
             com_stage,
             com_status,
             com_cycle,
             ind
        FROM T_IAS_ENTITY_PARA_ARCHIVE
       WHERE entity_id = P_ENTITY_ID
       ORDER BY archived_on DESC, archive_id DESC;
  END P_GET_ENTITY_PARA_ARCHIVE;
END PKG_IAS_ENTITY_MGMT;
