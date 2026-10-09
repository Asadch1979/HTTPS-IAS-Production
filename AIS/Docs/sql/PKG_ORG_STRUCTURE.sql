/*
  IAS Organization Structure: database API for the MVC organization chart.

  Prerequisite: V_IAS_ENTITY_REPORTING_NAMES must be deployed and verified.
  The application calls only PKG_ORG_STRUCTURE procedures; it does not require
  SELECT grants on the underlying hierarchy views.

  P_GET_NODES(NULL, cursor)      : all nodes, including detached roots/orphans.
  P_GET_NODES(entity_id, cursor) : that entity and all its descendants, any depth.
  P_GET_ENTITY_PATH(entity_id, cursor) : reporting chain, highest known parent first.
  P_GET_ORG_OPEN_PARA_COUNTS(root, cursor) : current own/descendant totals for scope.

  P_PREVIEW_ENTITY_MOVE validates without mutation. P_MOVE_ENTITY updates the
  reporting mapping and writes T_ORG_ENTITY_MOVE_LOG; the caller must commit
  or roll back on the same connection. The log table must already exist.
  These supplied definitions are deployed by the DBA, never at application startup.
  EXECUTE permission should be granted to the IAS application's database user
  by the DBA, after review. UI/controller must enforce IAS authorization.
*/

CREATE OR REPLACE PACKAGE PKG_ORG_STRUCTURE AUTHID DEFINER AS
  PROCEDURE P_GET_NODES(P_ROOT_ENTITY_ID IN NUMBER DEFAULT NULL,
                        O_CURSOR         OUT SYS_REFCURSOR);

  PROCEDURE P_GET_ENTITY_PATH(P_ENTITY_ID IN NUMBER,
                              O_CURSOR    OUT SYS_REFCURSOR);

  PROCEDURE P_GET_ORG_OPEN_PARA_COUNTS(P_ROOT_ENTITY_ID IN NUMBER,
                                       O_CURSOR         OUT SYS_REFCURSOR);
  PROCEDURE P_PREVIEW_ENTITY_MOVE(P_ENTITY_ID     IN NUMBER,
                                  P_NEW_PARENT_ID IN NUMBER,
                                  O_CURSOR        OUT SYS_REFCURSOR);

  PROCEDURE P_MOVE_ENTITY(P_ENTITY_ID          IN NUMBER,
                          P_EXPECTED_PARENT_ID IN NUMBER,
                          P_NEW_PARENT_ID      IN NUMBER,
                          P_CHANGED_BY         IN VARCHAR2,
                          P_REASON             IN VARCHAR2,
                          O_MOVE_ID            OUT VARCHAR2);

END PKG_ORG_STRUCTURE;
/
CREATE OR REPLACE PACKAGE BODY PKG_ORG_STRUCTURE AS

  PROCEDURE P_GET_NODES(P_ROOT_ENTITY_ID IN NUMBER DEFAULT NULL,
                        O_CURSOR         OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      WITH type_map AS
       (SELECT entity_id,
               MIN(c_type_id) AS type_id,
               COUNT(DISTINCT c_type_id) AS type_count
          FROM T_AUDITEE_ENTITIES_MAPING
         GROUP BY entity_id),
      org AS
       (SELECT h.entity_id,
               NVL(TRIM(h.entity_name),
                   'Unnamed Entity (' || TO_CHAR(h.entity_id) || ')') AS entity_name,
               CASE
                 WHEN tm.type_count = 1 THEN
                  tm.type_id
               END AS entity_type_id,
               CASE
                 WHEN tm.type_count > 1 THEN
                  1
                 ELSE
                  0
               END AS has_type_conflict,
               h.reporting_1 AS parent_entity_id,
               CASE
                 WHEN h.reporting_1 IS NULL THEN
                  NULL
                 ELSE
                  NVL(TRIM(h.reporting_1_name),
                      'Entity ' || TO_CHAR(h.reporting_1))
               END AS parent_entity_name,
               CASE
                 WHEN TRIM(h.entity_name) IS NULL THEN
                  1
                 ELSE
                  0
               END AS is_missing_name,
               h.reporting_1,
               h.reporting_2,
               h.reporting_3,
               h.reporting_4,
               h.reporting_5,
               h.reporting_6
          FROM V_IAS_ENTITY_REPORTING_NAMES h
          LEFT JOIN type_map tm
            ON tm.entity_id = h.entity_id),
      child_counts AS
       (SELECT parent_entity_id, COUNT(*) AS direct_child_count
          FROM org
         WHERE parent_entity_id IS NOT NULL
           AND parent_entity_id <> entity_id
         GROUP BY parent_entity_id),
      selected_nodes AS
       (SELECT entity_id
          FROM org
         START WITH entity_id = P_ROOT_ENTITY_ID
        CONNECT BY NOCYCLE PRIOR entity_id = parent_entity_id)
      SELECT n.entity_id,
             n.entity_name,
             n.entity_type_id,
             CASE n.entity_type_id
               WHEN 2 THEN
                'Institution / Head Office'
               WHEN 3 THEN
                'Division'
               WHEN 4 THEN
                'Department'
               WHEN 5 THEN
                'Regional Office'
               WHEN 6 THEN
                'Branch'
               WHEN 18 THEN
                'Group'
               WHEN 21 THEN
                'GM Office'
               ELSE
                'Other Entity'
             END AS entity_category,
             n.parent_entity_id,
             n.parent_entity_name,
             n.reporting_1,
             n.reporting_2,
             n.reporting_3,
             n.reporting_4,
             n.reporting_5,
             n.reporting_6,
             NVL(cc.direct_child_count, 0) AS direct_child_count,
             CASE
               WHEN n.parent_entity_id IS NULL THEN
                1
               ELSE
                0
             END AS is_root,
             CASE
               WHEN n.parent_entity_id IS NOT NULL AND p.entity_id IS NULL THEN
                1
               ELSE
                0
             END AS is_orphan,
             n.is_missing_name,
             n.has_type_conflict
        FROM org n
        LEFT JOIN org p
          ON p.entity_id = n.parent_entity_id
        LEFT JOIN child_counts cc
          ON cc.parent_entity_id = n.entity_id
       WHERE P_ROOT_ENTITY_ID IS NULL
          OR EXISTS
       (SELECT 1 FROM selected_nodes s WHERE s.entity_id = n.entity_id)
       ORDER BY CASE
                  WHEN n.parent_entity_id IS NULL THEN
                   0
                  ELSE
                   1
                END,
                n.parent_entity_id NULLS FIRST,
                UPPER(n.entity_name),
                n.entity_id;
  END P_GET_NODES;

  PROCEDURE P_GET_ENTITY_PATH(P_ENTITY_ID IN NUMBER,
                              O_CURSOR    OUT SYS_REFCURSOR) IS
  BEGIN
    OPEN O_CURSOR FOR
      SELECT LEVEL AS level_from_entity,
             h.entity_id,
             NVL(TRIM(h.entity_name),
                 'Unnamed Entity (' || TO_CHAR(h.entity_id) || ')') AS entity_name,
             h.reporting_1 AS parent_entity_id,
             CASE
               WHEN h.reporting_1 IS NULL THEN
                NULL
               ELSE
                NVL(TRIM(h.reporting_1_name),
                    'Entity ' || TO_CHAR(h.reporting_1))
             END AS parent_entity_name
        FROM V_IAS_ENTITY_REPORTING_NAMES h
       START WITH h.entity_id = P_ENTITY_ID
      CONNECT BY NOCYCLE PRIOR h.reporting_1 = h.entity_id
       ORDER BY level_from_entity DESC;
  END P_GET_ENTITY_PATH;

  PROCEDURE P_GET_ORG_OPEN_PARA_COUNTS(P_ROOT_ENTITY_ID IN NUMBER,
                                       O_CURSOR         OUT SYS_REFCURSOR)

   AS
  BEGIN
    IF P_ROOT_ENTITY_ID IS NOT NULL AND
       (P_ROOT_ENTITY_ID <= 0 OR
       P_ROOT_ENTITY_ID <> TRUNC(P_ROOT_ENTITY_ID)) THEN
      RAISE_APPLICATION_ERROR(-20001,
                              'Root entity ID must be a positive integer.');
    END IF;

    OPEN O_CURSOR FOR
      WITH
      /* Use the chart's reporting relationships.
                                         Treat self-reporting entries as roots. */
      org_edges AS
       (SELECT DISTINCT h.entity_id,
                        CASE
                          WHEN h.reporting_1 = h.entity_id THEN
                           NULL
                          ELSE
                           h.reporting_1
                        END AS parent_entity_id
          FROM V_IAS_ENTITY_REPORTING_NAMES h
         WHERE h.entity_id IS NOT NULL),

      org_names AS
       (SELECT h.entity_id,
               NVL(MAX(TRIM(h.entity_name)),
                   'Entity ' || TO_CHAR(h.entity_id)) AS entity_name
          FROM V_IAS_ENTITY_REPORTING_NAMES h
         WHERE h.entity_id IS NOT NULL
         GROUP BY h.entity_id),

      /* NULL requests the entire directory.
                                         Otherwise include the specified root and its descendants. */
      scope_ids AS
       (SELECT DISTINCT entity_id
          FROM org_edges
         START WITH P_ROOT_ENTITY_ID IS NULL
                 OR entity_id = P_ROOT_ENTITY_ID
        CONNECT BY NOCYCLE PRIOR entity_id = parent_entity_id),

      scoped_org AS
       (SELECT e.entity_id, e.parent_entity_id
          FROM org_edges e
         WHERE EXISTS
         (SELECT 1 FROM scope_ids s WHERE s.entity_id = e.entity_id)),

      /* Calculate each entity's own open paras once.
                                         Compliance history is deliberately not joined. */
      own_counts AS
       (SELECT p.entity_id, COUNT(DISTINCT p.com_id) AS own_open_paras
          FROM AIS_T_AU_POST_COMPLIANCE p
         WHERE p.para_status = 8
           AND EXISTS
         (SELECT 1 FROM scope_ids s WHERE s.entity_id = p.entity_id)
         GROUP BY p.entity_id),

      /* Every ancestor/descendant pair, including the entity itself.
                                         DISTINCT prevents repeated paths from inflating totals. */
      hierarchy_pairs AS
       (SELECT DISTINCT CONNECT_BY_ROOT entity_id AS ancestor_entity_id,
                        entity_id       AS descendant_entity_id
          FROM scoped_org
        CONNECT BY NOCYCLE PRIOR entity_id = parent_entity_id),

      rolled_counts AS
       (SELECT h.ancestor_entity_id AS entity_id,
               SUM(NVL(c.own_open_paras, 0)) AS total_open_paras
          FROM hierarchy_pairs h
          LEFT JOIN own_counts c
            ON c.entity_id = h.descendant_entity_id
         GROUP BY h.ancestor_entity_id)

      SELECT s.entity_id,
             n.entity_name,
             NVL(c.own_open_paras, 0) AS own_open_paras,
             NVL(r.total_open_paras, 0) - NVL(c.own_open_paras, 0) AS subordinate_open_paras,
             NVL(r.total_open_paras, 0) AS total_open_paras
        FROM scope_ids s
        LEFT JOIN org_names n
          ON n.entity_id = s.entity_id
        LEFT JOIN own_counts c
          ON c.entity_id = s.entity_id
        LEFT JOIN rolled_counts r
          ON r.entity_id = s.entity_id
       ORDER BY s.entity_id;

  END P_GET_ORG_OPEN_PARA_COUNTS;

  PROCEDURE VALIDATE_ENTITY_MOVE(P_ENTITY_ID     IN NUMBER,
                                 P_NEW_PARENT_ID IN NUMBER,
                                 O_ENTITY        OUT T_AUDITEE_ENTITIES_MAPING%ROWTYPE,
                                 O_PARENT        OUT T_AUDITEE_ENTITIES_MAPING%ROWTYPE,
                                 O_RELATION_ID   OUT NUMBER) AS
    V_COUNT NUMBER;
  BEGIN
    IF P_ENTITY_ID IS NULL OR P_NEW_PARENT_ID IS NULL OR P_ENTITY_ID <= 0 OR
       P_NEW_PARENT_ID <= 0 OR P_ENTITY_ID <> TRUNC(P_ENTITY_ID) OR
       P_NEW_PARENT_ID <> TRUNC(P_NEW_PARENT_ID) THEN
      RAISE_APPLICATION_ERROR(-20001,
                              'Valid entity and new parent IDs are required.');
    END IF;

    IF P_ENTITY_ID = 112201 THEN
      RAISE_APPLICATION_ERROR(-20002, 'The ZTBL root cannot be moved.');
    END IF;

    IF P_ENTITY_ID = P_NEW_PARENT_ID THEN
      RAISE_APPLICATION_ERROR(-20003, 'An entity cannot report to itself.');
    END IF;

    /* The hierarchy view reads all mapping rows.
    Reject duplicate IDs instead of choosing an arbitrary parent. */
    SELECT COUNT(*)
      INTO V_COUNT
      FROM (SELECT ENTITY_ID
              FROM T_AUDITEE_ENTITIES_MAPING
             WHERE ENTITY_ID IS NOT NULL
             GROUP BY ENTITY_ID
            HAVING COUNT(*) > 1);

    IF V_COUNT > 0 THEN
      RAISE_APPLICATION_ERROR(-20004,
                              'Duplicate entity mappings exist. Resolve them before moving.');
    END IF;

    BEGIN
      SELECT *
        INTO O_ENTITY
        FROM T_AUDITEE_ENTITIES_MAPING
       WHERE ENTITY_ID = P_ENTITY_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20005,
                                'The entity mapping does not exist.');
    END;

    BEGIN
      SELECT *
        INTO O_PARENT
        FROM T_AUDITEE_ENTITIES_MAPING
       WHERE ENTITY_ID = P_NEW_PARENT_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20006,
                                'The proposed parent mapping does not exist.');
    END;

    IF NVL(UPPER(TRIM(O_ENTITY.STATUS)), 'N') <> 'Y' OR
       NVL(UPPER(TRIM(O_PARENT.STATUS)), 'N') <> 'Y' THEN
      RAISE_APPLICATION_ERROR(-20007,
                              'Both entity mappings must be active.');
    END IF;

    IF O_ENTITY.PARENT_ID IS NULL OR
       O_ENTITY.PARENT_ID = O_ENTITY.ENTITY_ID THEN
      RAISE_APPLICATION_ERROR(-20008,
                              'A root mapping cannot be moved through this procedure.');
    END IF;

    IF O_ENTITY.PARENT_ID = P_NEW_PARENT_ID THEN
      RAISE_APPLICATION_ERROR(-20009,
                              'The entity already reports to this parent.');
    END IF;

    IF O_ENTITY.C_TYPE_ID IS NULL OR O_PARENT.C_TYPE_ID IS NULL OR
       O_PARENT.CHILD_CODE IS NULL THEN
      RAISE_APPLICATION_ERROR(-20010,
                              'Required entity type or parent code is missing.');
    END IF;

    /* Use the same entity names displayed by the chart. */
    BEGIN
      SELECT NAME
        INTO O_ENTITY.C_NAME
        FROM T_AUDITEE_ENTITIES
       WHERE ENTITY_ID = P_ENTITY_ID;

      SELECT NAME
        INTO O_PARENT.C_NAME
        FROM T_AUDITEE_ENTITIES
       WHERE ENTITY_ID = P_NEW_PARENT_ID;
    EXCEPTION
      WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20011,
                                'An entity master record is missing.');
      WHEN TOO_MANY_ROWS THEN
        RAISE_APPLICATION_ERROR(-20012,
                                'Duplicate entity master records exist.');
    END;

    IF TRIM(O_ENTITY.C_NAME) IS NULL OR TRIM(O_PARENT.C_NAME) IS NULL THEN
      RAISE_APPLICATION_ERROR(-20013,
                              'Entity and parent names are required.');
    END IF;

    /* Check both upward paths for existing circular relationships.
    The established root's self-reference is excluded. */
    SELECT COUNT(*)
      INTO V_COUNT
      FROM (SELECT CONNECT_BY_ISCYCLE AS IS_CYCLE
              FROM T_AUDITEE_ENTITIES_MAPING
             START WITH ENTITY_ID IN (P_ENTITY_ID, P_NEW_PARENT_ID)
            CONNECT BY NOCYCLE PRIOR PARENT_ID = ENTITY_ID
                   AND PRIOR ENTITY_ID <> PRIOR PARENT_ID)
     WHERE IS_CYCLE = 1;

    IF V_COUNT > 0 THEN
      RAISE_APPLICATION_ERROR(-20014,
                              'An existing circular reporting relationship must be corrected.');
    END IF;

    /* Reject a proposed parent anywhere below the moved entity. */
    SELECT COUNT(*)
      INTO V_COUNT
      FROM (SELECT ENTITY_ID
              FROM T_AUDITEE_ENTITIES_MAPING
             START WITH ENTITY_ID = P_ENTITY_ID
            CONNECT BY NOCYCLE PRIOR ENTITY_ID = PARENT_ID
                   AND ENTITY_ID <> PARENT_ID)
     WHERE ENTITY_ID = P_NEW_PARENT_ID;

    IF V_COUNT > 0 THEN
      RAISE_APPLICATION_ERROR(-20015,
                              'The new parent cannot be a descendant of this entity.');
    END IF;

    /* The destination must belong to the ZTBL hierarchy. */
    SELECT COUNT(*)
      INTO V_COUNT
      FROM (SELECT ENTITY_ID
              FROM T_AUDITEE_ENTITIES_MAPING
             START WITH ENTITY_ID = P_NEW_PARENT_ID
            CONNECT BY NOCYCLE PRIOR PARENT_ID = ENTITY_ID
                   AND PRIOR ENTITY_ID <> PRIOR PARENT_ID)
     WHERE ENTITY_ID = 112201;

    IF V_COUNT = 0 THEN
      RAISE_APPLICATION_ERROR(-20016,
                              'The proposed parent is not connected to the ZTBL root.');
    END IF;

    /* Infer the relation only from an existing, unambiguous
    active parent-type / child-type combination. */
    SELECT COUNT(DISTINCT RELATION_TYPE_ID), MIN(RELATION_TYPE_ID)
      INTO V_COUNT, O_RELATION_ID
      FROM T_AUDITEE_ENTITIES_MAPING
     WHERE P_TYPE_ID = O_PARENT.C_TYPE_ID
       AND C_TYPE_ID = O_ENTITY.C_TYPE_ID
       AND UPPER(TRIM(STATUS)) = 'Y'
       AND RELATION_TYPE_ID IS NOT NULL;

    IF V_COUNT <> 1 THEN
      RAISE_APPLICATION_ERROR(-20017,
                              'No unambiguous relation type exists for this parent/child type combination.');
    END IF;
  END VALIDATE_ENTITY_MOVE;

  PROCEDURE P_PREVIEW_ENTITY_MOVE(P_ENTITY_ID     IN NUMBER,
                                  P_NEW_PARENT_ID IN NUMBER,
                                  O_CURSOR        OUT SYS_REFCURSOR) AS
    V_ENTITY      T_AUDITEE_ENTITIES_MAPING%ROWTYPE;
    V_PARENT      T_AUDITEE_ENTITIES_MAPING%ROWTYPE;
    V_RELATION_ID NUMBER;
    V_DESCENDANTS NUMBER;
  BEGIN
    VALIDATE_ENTITY_MOVE(P_ENTITY_ID,
                         P_NEW_PARENT_ID,
                         V_ENTITY,
                         V_PARENT,
                         V_RELATION_ID);

    SELECT COUNT(DISTINCT ENTITY_ID) - 1
      INTO V_DESCENDANTS
      FROM T_AUDITEE_ENTITIES_MAPING
     START WITH ENTITY_ID = P_ENTITY_ID
    CONNECT BY NOCYCLE PRIOR ENTITY_ID = PARENT_ID
           AND ENTITY_ID <> PARENT_ID;

    OPEN O_CURSOR FOR
      SELECT V_ENTITY.ENTITY_ID  AS ENTITY_ID,
             V_ENTITY.C_NAME     AS ENTITY_NAME,
             V_ENTITY.PARENT_ID  AS CURRENT_PARENT_ID,
             V_ENTITY.P_NAME     AS CURRENT_PARENT_NAME,
             V_PARENT.ENTITY_ID  AS NEW_PARENT_ID,
             V_PARENT.C_NAME     AS NEW_PARENT_NAME,
             V_PARENT.CHILD_CODE AS NEW_PARENT_CODE,
             V_PARENT.C_TYPE_ID  AS NEW_PARENT_TYPE_ID,
             V_RELATION_ID       AS NEW_RELATION_TYPE_ID,
             V_DESCENDANTS       AS DESCENDANT_ENTITIES
        FROM DUAL;
  END P_PREVIEW_ENTITY_MOVE;

  PROCEDURE P_MOVE_ENTITY(P_ENTITY_ID          IN NUMBER,
                          P_EXPECTED_PARENT_ID IN NUMBER,
                          P_NEW_PARENT_ID      IN NUMBER,
                          P_CHANGED_BY         IN VARCHAR2,
                          P_REASON             IN VARCHAR2,
                          O_MOVE_ID            OUT VARCHAR2) AS
    V_ENTITY      T_AUDITEE_ENTITIES_MAPING%ROWTYPE;
    V_PARENT      T_AUDITEE_ENTITIES_MAPING%ROWTYPE;
    V_RELATION_ID NUMBER;
    V_MOVE_ID     RAW(16);
    V_NEW_R_KEY   VARCHAR2(200);
  BEGIN
    O_MOVE_ID := NULL;

    IF P_EXPECTED_PARENT_ID IS NULL THEN
      RAISE_APPLICATION_ERROR(-20018,
                              'The expected current parent is required.');
    END IF;

    IF TRIM(P_CHANGED_BY) IS NULL OR LENGTHB(TRIM(P_CHANGED_BY)) > 100 THEN
      RAISE_APPLICATION_ERROR(-20019,
                              'A valid acting-user identifier is required.');
    END IF;

    IF TRIM(P_REASON) IS NULL OR LENGTHB(TRIM(P_REASON)) > 1000 THEN
      RAISE_APPLICATION_ERROR(-20020,
                              'A change reason of up to 1000 bytes is required.');
    END IF;

    SAVEPOINT ORG_MOVE_START;

    BEGIN
      LOCK TABLE T_AUDITEE_ENTITIES_MAPING IN SHARE ROW EXCLUSIVE MODE NOWAIT;

      VALIDATE_ENTITY_MOVE(P_ENTITY_ID,
                           P_NEW_PARENT_ID,
                           V_ENTITY,
                           V_PARENT,
                           V_RELATION_ID);

      IF V_ENTITY.PARENT_ID <> P_EXPECTED_PARENT_ID THEN
        RAISE_APPLICATION_ERROR(-20021,
                                'The reporting parent has changed. Refresh the preview.');
      END IF;

      V_MOVE_ID := SYS_GUID();

      V_NEW_R_KEY := TO_CHAR(P_NEW_PARENT_ID, 'TM9') || '|' ||
                     TO_CHAR(P_ENTITY_ID, 'TM9');

      UPDATE T_AUDITEE_ENTITIES_MAPING
         SET PARENT_ID        = P_NEW_PARENT_ID,
             PARENT_CODE      = V_PARENT.CHILD_CODE,
             P_NAME           = V_PARENT.C_NAME,
             P_TYPE_ID        = V_PARENT.C_TYPE_ID,
             RELATION_TYPE_ID = V_RELATION_ID,
             R_KEY            = V_NEW_R_KEY
       WHERE ENTITY_ID = P_ENTITY_ID
         AND PARENT_ID = P_EXPECTED_PARENT_ID;

      IF SQL%ROWCOUNT <> 1 THEN
        RAISE_APPLICATION_ERROR(-20022,
                                'Exactly one entity mapping must be updated.');
      END IF;

      INSERT INTO T_ORG_ENTITY_MOVE_LOG
        (MOVE_ID,
         ENTITY_ID,
         ENTITY_NAME,
         OLD_PARENT_ID,
         NEW_PARENT_ID,
         OLD_PARENT_NAME,
         NEW_PARENT_NAME,
         OLD_PARENT_CODE,
         NEW_PARENT_CODE,
         OLD_PARENT_TYPE_ID,
         NEW_PARENT_TYPE_ID,
         OLD_RELATION_TYPE_ID,
         NEW_RELATION_TYPE_ID,
         OLD_R_KEY,
         NEW_R_KEY,
         CHANGED_BY,
         CHANGE_REASON)
      VALUES
        (V_MOVE_ID,
         V_ENTITY.ENTITY_ID,
         V_ENTITY.C_NAME,
         V_ENTITY.PARENT_ID,
         P_NEW_PARENT_ID,
         V_ENTITY.P_NAME,
         V_PARENT.C_NAME,
         V_ENTITY.PARENT_CODE,
         V_PARENT.CHILD_CODE,
         V_ENTITY.P_TYPE_ID,
         V_PARENT.C_TYPE_ID,
         V_ENTITY.RELATION_TYPE_ID,
         V_RELATION_ID,
         V_ENTITY.R_KEY,
         V_NEW_R_KEY,
         TRIM(P_CHANGED_BY),
         TRIM(P_REASON));

      O_MOVE_ID := RAWTOHEX(V_MOVE_ID);

    EXCEPTION
      WHEN OTHERS THEN
        ROLLBACK TO ORG_MOVE_START;
        O_MOVE_ID := NULL;
        RAISE;
    END;
  END P_MOVE_ENTITY;

END PKG_ORG_STRUCTURE;
/
