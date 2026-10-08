/*
  IAS Organization Structure: read-only database API for the MVC organization chart.

  Prerequisite: V_IAS_ENTITY_REPORTING_NAMES must be deployed and verified.
  The application calls only PKG_ORG_STRUCTURE procedures; it does not require
  SELECT grants on the underlying hierarchy views.

  P_GET_NODES(NULL, cursor)      : all nodes, including detached roots/orphans.
  P_GET_NODES(entity_id, cursor) : that entity and all its descendants, any depth.
  P_GET_ENTITY_PATH(entity_id, cursor) : reporting chain, highest known parent first.

  Source relationships are never changed by this package.
  EXECUTE permission should be granted to the IAS application's database user
  by the DBA, after review. UI/controller must enforce IAS authorization.
*/

CREATE OR REPLACE PACKAGE PKG_ORG_STRUCTURE AUTHID DEFINER AS
    PROCEDURE P_GET_NODES(
        P_ROOT_ENTITY_ID IN NUMBER DEFAULT NULL,
        O_CURSOR        OUT SYS_REFCURSOR
    );

    PROCEDURE P_GET_ENTITY_PATH(
        P_ENTITY_ID IN NUMBER,
        O_CURSOR    OUT SYS_REFCURSOR
    );
END PKG_ORG_STRUCTURE;
/

CREATE OR REPLACE PACKAGE BODY PKG_ORG_STRUCTURE AS

    PROCEDURE P_GET_NODES(
        P_ROOT_ENTITY_ID IN NUMBER DEFAULT NULL,
        O_CURSOR        OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN O_CURSOR FOR
            WITH type_map AS (
                SELECT
                    entity_id,
                    MIN(c_type_id) AS type_id,
                    COUNT(DISTINCT c_type_id) AS type_count
                FROM T_AUDITEE_ENTITIES_MAPING
                GROUP BY entity_id
            ),
            org AS (
                SELECT
                    h.entity_id,
                    NVL(TRIM(h.entity_name),
                        'Unnamed Entity (' || TO_CHAR(h.entity_id) || ')') AS entity_name,
                    CASE WHEN tm.type_count = 1 THEN tm.type_id END AS entity_type_id,
                    CASE WHEN tm.type_count > 1 THEN 1 ELSE 0 END AS has_type_conflict,
                    h.reporting_1 AS parent_entity_id,
                    CASE
                        WHEN h.reporting_1 IS NULL THEN NULL
                        ELSE NVL(TRIM(h.reporting_1_name),
                                 'Entity ' || TO_CHAR(h.reporting_1))
                    END AS parent_entity_name,
                    CASE WHEN TRIM(h.entity_name) IS NULL THEN 1 ELSE 0 END
                        AS is_missing_name,
                    h.reporting_1,
                    h.reporting_2,
                    h.reporting_3,
                    h.reporting_4,
                    h.reporting_5,
                    h.reporting_6
                FROM V_IAS_ENTITY_REPORTING_NAMES h
                LEFT JOIN type_map tm ON tm.entity_id = h.entity_id
            ),
            child_counts AS (
                SELECT parent_entity_id, COUNT(*) AS direct_child_count
                FROM org
                WHERE parent_entity_id IS NOT NULL
                  AND parent_entity_id <> entity_id
                GROUP BY parent_entity_id
            ),
            selected_nodes AS (
                SELECT entity_id
                FROM org
                START WITH entity_id = P_ROOT_ENTITY_ID
                CONNECT BY NOCYCLE PRIOR entity_id = parent_entity_id
            )
            SELECT
                n.entity_id,
                n.entity_name,
                n.entity_type_id,
                CASE n.entity_type_id
                    WHEN 2  THEN 'Institution / Head Office'
                    WHEN 3  THEN 'Division'
                    WHEN 4  THEN 'Department'
                    WHEN 5  THEN 'Regional Office'
                    WHEN 6  THEN 'Branch'
                    WHEN 18 THEN 'Group'
                    WHEN 21 THEN 'GM Office'
                    ELSE 'Other Entity'
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
                CASE WHEN n.parent_entity_id IS NULL THEN 1 ELSE 0 END AS is_root,
                CASE
                    WHEN n.parent_entity_id IS NOT NULL AND p.entity_id IS NULL
                    THEN 1 ELSE 0
                END AS is_orphan,
                n.is_missing_name,
                n.has_type_conflict
            FROM org n
            LEFT JOIN org p ON p.entity_id = n.parent_entity_id
            LEFT JOIN child_counts cc ON cc.parent_entity_id = n.entity_id
            WHERE P_ROOT_ENTITY_ID IS NULL
               OR EXISTS (
                   SELECT 1
                   FROM selected_nodes s
                   WHERE s.entity_id = n.entity_id
               )
            ORDER BY
                CASE WHEN n.parent_entity_id IS NULL THEN 0 ELSE 1 END,
                n.parent_entity_id NULLS FIRST,
                UPPER(n.entity_name),
                n.entity_id;
    END P_GET_NODES;


    PROCEDURE P_GET_ENTITY_PATH(
        P_ENTITY_ID IN NUMBER,
        O_CURSOR    OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN O_CURSOR FOR
            SELECT
                LEVEL AS level_from_entity,
                h.entity_id,
                NVL(TRIM(h.entity_name),
                    'Unnamed Entity (' || TO_CHAR(h.entity_id) || ')') AS entity_name,
                h.reporting_1 AS parent_entity_id,
                CASE
                    WHEN h.reporting_1 IS NULL THEN NULL
                    ELSE NVL(TRIM(h.reporting_1_name),
                             'Entity ' || TO_CHAR(h.reporting_1))
                END AS parent_entity_name
            FROM V_IAS_ENTITY_REPORTING_NAMES h
            START WITH h.entity_id = P_ENTITY_ID
            CONNECT BY NOCYCLE PRIOR h.reporting_1 = h.entity_id
            ORDER BY level_from_entity DESC;
    END P_GET_ENTITY_PATH;

END PKG_ORG_STRUCTURE;
/
