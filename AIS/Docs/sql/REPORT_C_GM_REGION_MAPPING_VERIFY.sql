-- REPORT C: GM -> REGION / HEAD OFFICE UNIT MAPPING, READ ONLY
-- Preflight on an authorized staging Oracle schema. Do not update mappings.
-- Use the same authorized reporting period/audited-by/office filters as Report C.
-- IMPORTANT: confirm that type IDs 6,21,22,28 represent the deployed
-- Branch, GM Office, Region and CAU (or applicable special office) taxonomy.
-- The SQL implementation deliberately does NOT guess a GM for ambiguous
-- AUDIT_BY assignments and does not alter report authorization filters.

-- 1. Verify entity-type metadata before compiling the modified package.
SELECT t.autid AS entity_type_id, t.entitytypedesc, t.audit_type
  FROM t_auditee_ent_types t
 WHERE t.autid IN (6, 21, 22, 28)
 ORDER BY t.autid;

-- 2. List each relevant source entity exactly once with its GM candidates.
-- Change these two dates and audited-by filter for the selected report scope.
VAR P_END_DATE VARCHAR2(10);
VAR P_AUDITED_BY_ID NUMBER;
EXEC :P_END_DATE := '2026-10-26';
EXEC :P_AUDITED_BY_ID := NULL;

WITH source_entities AS (
  SELECT DISTINCT s.entity_id, s.entity_type_id, s.audit_type,
         s.audited_by, s.region_id, s.gm_office_id, s.reporting_1
    FROM v_exec_report_source s
   WHERE s.effective_added_on < TO_DATE(:P_END_DATE,'YYYY-MM-DD') + 1
     AND s.audit_type IN ('B','D')
     AND (:P_AUDITED_BY_ID IS NULL OR s.audited_by = :P_AUDITED_BY_ID)
),
direct_map AS (
  SELECT m.entity_id,
         COUNT(DISTINCT m.gm_office) AS direct_candidates,
         CASE WHEN COUNT(DISTINCT m.gm_office) = 1
              THEN MAX(CASE WHEN gm.type_id=21 THEN m.gm_office END)
         END AS valid_mapped_gm
    FROM t_auditee_entities_maping m
    LEFT JOIN t_auditee_entities gm ON gm.entity_id=m.gm_office
   WHERE NVL(m.status,'Y')='Y'
   GROUP BY m.entity_id
),
gm_by_auditor AS (
  SELECT m.audit_by, COUNT(DISTINCT m.entity_id) AS gm_candidates,
         CASE WHEN COUNT(DISTINCT m.entity_id)=1
              THEN MAX(m.entity_id) END AS unique_gm
    FROM t_auditee_entities_maping m
    JOIN t_auditee_entities gm
      ON gm.entity_id=m.entity_id AND gm.type_id=21
   WHERE NVL(m.status,'Y')='Y'
     AND m.audit_by IS NOT NULL
   GROUP BY m.audit_by
),
gm_auditor AS (
  SELECT m.entity_id,
         CASE WHEN COUNT(DISTINCT m.audit_by)=1
              THEN MAX(m.audit_by) END AS audited_by
    FROM t_auditee_entities_maping m
    JOIN t_auditee_entities gm
      ON gm.entity_id=m.entity_id AND gm.type_id=21
   WHERE NVL(m.status,'Y')='Y'
   GROUP BY m.entity_id
),
resolution AS (
  SELECT s.*, e.name AS entity_name, t.entitytypedesc AS type_name,
         d.direct_candidates, d.valid_mapped_gm, g.gm_candidates,
         g.unique_gm,
         CASE WHEN s.entity_type_id=21 THEN s.entity_id
              WHEN s.audit_type='B'
                   AND s.entity_type_id NOT IN (6,21,22,28)
                THEN COALESCE(CASE WHEN ga.audited_by=s.audited_by
                                   THEN d.valid_mapped_gm END, g.unique_gm)
              ELSE COALESCE(d.valid_mapped_gm, s.gm_office_id)
         END AS selected_gm_id
    FROM source_entities s
    LEFT JOIN t_auditee_entities e ON e.entity_id=s.entity_id
    LEFT JOIN t_auditee_ent_types t ON t.autid=s.entity_type_id
    LEFT JOIN direct_map d ON d.entity_id=s.entity_id
    LEFT JOIN gm_by_auditor g ON g.audit_by=s.audited_by
    LEFT JOIN gm_auditor ga ON ga.entity_id=d.valid_mapped_gm
)
SELECT r.entity_id,r.entity_name,r.entity_type_id,r.type_name,
       r.audit_type,r.audited_by,r.reporting_1,r.region_id,
       r.gm_office_id AS source_gm_id, r.valid_mapped_gm,
       r.direct_candidates, r.gm_candidates, r.unique_gm,
       r.selected_gm_id, gm.name AS selected_gm_name,
       CASE WHEN r.audit_type='B'
                  AND r.entity_type_id NOT IN (6,21,22,28)
                  THEN 'HEAD OFFICE UNIT'
            WHEN r.entity_type_id=6 THEN 'BRANCH'
            WHEN r.entity_type_id=22 THEN 'REGION'
            WHEN r.entity_type_id=21 THEN 'GM OFFICE'
            WHEN r.entity_type_id=28 THEN 'SPECIAL / CAU TYPE: VERIFY'
            ELSE 'OTHER' END AS entity_class,
       CASE WHEN r.selected_gm_id IS NULL THEN 'MISSING / AMBIGUOUS GM'
            WHEN gm.entity_id IS NULL THEN 'GM ID INVALID'
            ELSE 'MATCHED' END AS mapping_quality
  FROM resolution r
  LEFT JOIN t_auditee_entities gm
    ON gm.entity_id=r.selected_gm_id AND gm.type_id=21
 ORDER BY mapping_quality DESC, r.audit_type, r.audited_by,
          r.entity_type_id, r.entity_name, r.entity_id;

-- 3. Verify that report totals stay constant after reclassification.
-- Compare Report C TOTAL opening, additions, settlements and outstanding
-- before/after change for identical dates and an identical audited scope.
-- Mappings affect dimensions only; no para row should be duplicated.
