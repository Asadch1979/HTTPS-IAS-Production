-- CIA rectification reconciliation, development/source only.
-- Exactly one row per modern observation. All modern IND='A' post-compliance
-- rows are aggregated first so shifted/duplicate copies cannot multiply issues.
-- The source observation flag remains authoritative for Issue Rectified KPIs.
-- A post-compliance copy is a carried-forward audit conclusion flag, not a
-- separate rectification event (do not sum it into lifecycle ISSUE counts).
-- Prerequisite: ISSUE_RECTIFIED exists on T_AU_OBSERVATION and
-- AIS_T_AU_POST_COMPLIANCE (see ISSUE_RECTIFIED_prod_migration.sql).
CREATE OR REPLACE VIEW V_EXEC_ISSUE_RECTIFIED_RECON AS
WITH active_conclusion AS
(
    -- Use the same latest unreversed STATUS=14 event as the lifecycle view.
    SELECT l.e_id AS eng_id,
           MAX(l.created_on) KEEP
             (DENSE_RANK LAST ORDER BY l.id) AS concluded_on
      FROM t_au_plan_eng_log l
     WHERE l.status_id = 14
       AND NOT EXISTS
           (SELECT 1 FROM t_au_plan_eng_log r
             WHERE r.e_id = l.e_id AND r.status_id = 10 AND r.id > l.id)
     GROUP BY l.e_id
),
post_copies AS
(
    SELECT c.new_para_id AS observation_id,
           COUNT(*) AS post_copy_count,
           MIN(NVL(c.issue_rectified, 0)) AS min_post_rectified,
           MAX(NVL(c.issue_rectified, 0)) AS max_post_rectified
      FROM ais_t_au_post_compliance c
     WHERE c.ind = 'A'
       AND c.new_para_id IS NOT NULL
     GROUP BY c.new_para_id
)
SELECT o.id AS observation_id,
       e.eng_id,
       e.period_id AS audit_period_id,
       e.entity_id,
       ae.type_id AS entity_type_id,
       e.auditby_id AS audited_by_id,
       e.status AS engagement_status,
       o.status AS observation_status,
       o.entereddate AS issue_identified_on,
       o.stelled_on AS issue_rectified_on,
       a.concluded_on,
       NVL(o.issue_rectified, 0) AS source_rectified,
       NVL(p.post_copy_count, 0) AS post_copy_count,
       p.min_post_rectified,
       p.max_post_rectified,
       CASE WHEN o.status = 9 THEN NVL(o.issue_rectified, 0) ELSE 0
       END AS expected_post_rectified,
       CASE
         WHEN e.status = 14
          AND o.status IN (4, 9)
          AND NVL(o.issue_rectified, 0) = 1
          AND o.stelled_on IS NOT NULL
          AND a.concluded_on IS NOT NULL
          AND o.stelled_on < a.concluded_on
         THEN 1 ELSE 0
       END AS rectified_before_conclusion,
       CASE
         WHEN p.post_copy_count IS NULL THEN 'NOT_TRANSFERRED'
         WHEN p.min_post_rectified <> p.max_post_rectified THEN 'INCONSISTENT_COPIES'
         WHEN p.min_post_rectified <>
              CASE WHEN o.status = 9 THEN NVL(o.issue_rectified, 0) ELSE 0 END
           THEN 'MISMATCH'
         ELSE 'MATCH'
       END AS post_copy_status
  FROM t_au_observation o
  JOIN t_au_plan_eng e ON e.eng_id = o.engplanid
  LEFT JOIN t_auditee_entities ae ON ae.entity_id = e.entity_id
  LEFT JOIN active_conclusion a ON a.eng_id = e.eng_id
  LEFT JOIN post_copies p ON p.observation_id = o.id;
