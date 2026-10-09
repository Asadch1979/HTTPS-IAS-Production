/*
  IAS Organization Structure: read-only database API for the MVC organization chart.

  Prerequisite: V_IAS_ENTITY_REPORTING_NAMES must be deployed and verified.
  The application calls only PKG_ORG_STRUCTURE procedures; it does not require
  SELECT grants on the underlying hierarchy views.

  P_GET_NODES(NULL, cursor)      : all nodes, including detached roots/orphans.
  P_GET_NODES(entity_id, cursor) : that entity and all its descendants, any depth.
  P_GET_ENTITY_PATH(entity_id, cursor) : reporting chain, highest known parent first.
  P_GET_ORG_OPEN_PARA_COUNTS(root, cursor) : current own/descendant totals for scope.

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

    PROCEDURE P_GET_ORG_OPEN_PARA_COUNTS(
        P_ROOT_ENTITY_ID IN NUMBER,
        O_CURSOR        OUT SYS_REFCURSOR
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

    PROCEDURE P_GET_ORG_OPEN_PARA_COUNTS(
        P_ROOT_ENTITY_ID IN NUMBER,
        O_CURSOR        OUT SYS_REFCURSOR
    ) AS
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
            org_edges AS (
                SELECT DISTINCT h.entity_id,
                    CASE WHEN h.reporting_1 = h.entity_id THEN NULL
                         ELSE h.reporting_1 END AS parent_entity_id
                FROM V_IAS_ENTITY_REPORTING_NAMES h
                WHERE h.entity_id IS NOT NULL
            ),
            org_names AS (
                SELECT h.entity_id,
                    NVL(MAX(TRIM(h.entity_name)),
                        'Entity ' || TO_CHAR(h.entity_id)) AS entity_name
                FROM V_IAS_ENTITY_REPORTING_NAMES h
                WHERE h.entity_id IS NOT NULL
                GROUP BY h.entity_id
            ),
            /* NULL requests the entire directory.
               Otherwise include the specified root and its descendants. */
            scope_ids AS (
                SELECT DISTINCT entity_id
                FROM org_edges
                START WITH P_ROOT_ENTITY_ID IS NULL OR entity_id = P_ROOT_ENTITY_ID
                CONNECT BY NOCYCLE PRIOR entity_id = parent_entity_id
            ),
            scoped_org AS (
                SELECT e.entity_id, e.parent_entity_id
                FROM org_edges e
                WHERE EXISTS (SELECT 1 FROM scope_ids s WHERE s.entity_id = e.entity_id)
            ),
            /* Calculate each entity's own open paras once.
               Compliance history is deliberately not joined. */
            own_counts AS (
                SELECT p.entity_id, COUNT(DISTINCT p.com_id) AS own_open_paras
                FROM AIS_T_AU_POST_COMPLIANCE p
                WHERE p.para_status = 8
                  AND EXISTS (SELECT 1 FROM scope_ids s WHERE s.entity_id = p.entity_id)
                GROUP BY p.entity_id
            ),
            /* Every ancestor/descendant pair, including the entity itself.
               DISTINCT prevents repeated paths from inflating totals. */
            hierarchy_pairs AS (
                SELECT DISTINCT CONNECT_BY_ROOT entity_id AS ancestor_entity_id,
                    entity_id AS descendant_entity_id
                FROM scoped_org
                CONNECT BY NOCYCLE PRIOR entity_id = parent_entity_id
            ),
            rolled_counts AS (
                SELECT h.ancestor_entity_id AS entity_id,
                    SUM(NVL(c.own_open_paras, 0)) AS total_open_paras
                FROM hierarchy_pairs h
                LEFT JOIN own_counts c ON c.entity_id = h.descendant_entity_id
                GROUP BY h.ancestor_entity_id
            )
            SELECT s.entity_id,
                n.entity_name,
                NVL(c.own_open_paras, 0) AS own_open_paras,
                NVL(r.total_open_paras, 0) - NVL(c.own_open_paras, 0) AS subordinate_open_paras,
                NVL(r.total_open_paras, 0) AS total_open_paras
            FROM scope_ids s
            LEFT JOIN org_names n ON n.entity_id = s.entity_id
            LEFT JOIN own_counts c ON c.entity_id = s.entity_id
            LEFT JOIN rolled_counts r ON r.entity_id = s.entity_id
            ORDER BY s.entity_id;
    END P_GET_ORG_OPEN_PARA_COUNTS;

END PKG_ORG_STRUCTURE;
/
