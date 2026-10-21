-- F8: execute this file alone in PL/SQL Developer SQL Window.
--Views
CREATE OR REPLACE VIEW V_EXEC_ENTITY_SCOPE AS
WITH entity_map AS
(
    SELECT
        entity_id,
        CASE WHEN COUNT(DISTINCT c_type_id) = 1 THEN MAX(c_type_id) END AS c_type_id
    FROM t_auditee_entities_maping
    WHERE NVL(status, 'Y') = 'Y'
    GROUP BY entity_id
),
hierarchy AS
(
    SELECT
        h.entity_id,
        h.reporting_1 AS parent_id,
        h.reporting_1,
        h.reporting_2,
        h.reporting_3,
        h.reporting_4,
        h.reporting_5,
        h.reporting_6
    FROM V_IAS_ENTITY_REPORTING_IDS h
    JOIN entity_map m
      ON m.entity_id = h.entity_id
),
hierarchy_levels AS
(
    SELECT
        h.entity_id,
        x.level_no,
        x.office_id,
        m.c_type_id
    FROM hierarchy h
    CROSS APPLY
    (
        SELECT 0 level_no, h.entity_id office_id FROM dual
        UNION ALL
        SELECT 1, h.reporting_1 FROM dual
        UNION ALL
        SELECT 2, h.reporting_2 FROM dual
        UNION ALL
        SELECT 3, h.reporting_3 FROM dual
        UNION ALL
        SELECT 4, h.reporting_4 FROM dual
        UNION ALL
        SELECT 5, h.reporting_5 FROM dual
        UNION ALL
        SELECT 6, h.reporting_6 FROM dual
    ) x
    LEFT JOIN entity_map m
      ON m.entity_id = x.office_id
    WHERE x.office_id IS NOT NULL
),
office_scope AS
(
    SELECT
        entity_id,

        MAX(CASE WHEN c_type_id = 21 THEN office_id END)
          KEEP (DENSE_RANK FIRST ORDER BY
                CASE WHEN c_type_id = 21
                     THEN level_no ELSE 99 END)
          AS gm_office_id,

        MAX(CASE WHEN c_type_id = 3 THEN office_id END)
          KEEP (DENSE_RANK FIRST ORDER BY
                CASE WHEN c_type_id = 3
                     THEN level_no ELSE 99 END)
          AS division_id,

        MAX(CASE WHEN c_type_id = 18 THEN office_id END)
          KEEP (DENSE_RANK FIRST ORDER BY
                CASE WHEN c_type_id = 18
                     THEN level_no ELSE 99 END)
          AS business_group_id

    FROM hierarchy_levels
    GROUP BY entity_id
)
SELECT
    h.entity_id,
    h.parent_id,

    h.reporting_1 AS reporting_office_id,

    s.gm_office_id,
    s.division_id,
    s.business_group_id,

    h.reporting_1,
    h.reporting_2,
    h.reporting_3,
    h.reporting_4,
    h.reporting_5,
    h.reporting_6

FROM hierarchy h
LEFT JOIN office_scope s
       ON s.entity_id = h.entity_id;
