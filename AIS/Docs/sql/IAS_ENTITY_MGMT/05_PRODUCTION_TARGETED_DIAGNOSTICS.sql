-- IAS ENTITY MANAGEMENT | PROD TARGETED DATA QUALITY REVIEW
-- 03 NOV 2026 production preflight follow-up.
-- ONE SELECT / ONE RESULT GRID. READ ONLY. Use SQL Developer Ctrl+Enter.
-- No changes to entity or para records.
-- Findings are review candidates, NOT automatic indications of corruption.
SELECT r.finding, r.entity_id, r.reference_id, r.measure_value,
       r.status_value, r.details
  FROM (
    SELECT 'MASTER_ID_NULL_COUNT' finding,
           CAST(NULL AS NUMBER) entity_id, CAST(NULL AS NUMBER) reference_id,
           COUNT(*) measure_value, CAST(NULL AS VARCHAR2(80)) status_value,
           'Master ENTITY_ID should be not null, but has no declared PK in production' details
      FROM T_AUDITEE_ENTITIES e WHERE e.entity_id IS NULL
    UNION ALL
    SELECT 'MAPPING_ID_NULL_COUNT', NULL, NULL, COUNT(*), NULL,
           'Mapping ENTITY_ID should be not null, but has no declared PK in production'
      FROM T_AUDITEE_ENTITIES_MAPING m WHERE m.entity_id IS NULL
    UNION ALL
    SELECT 'MASTER_MAX_ENTITY_ID', NULL, NULL, MAX(e.entity_id), NULL,
           'Diagnostic only; do not seed a concurrent-safe sequence from MAX without writer migration'
      FROM T_AUDITEE_ENTITIES e
    UNION ALL
    SELECT 'MASTER_DUPLICATE_ENTITY_ID', e.entity_id, NULL, COUNT(*), NULL,
           'Duplicate ENTITY_ID prevents adding a unique key without remediation'
      FROM T_AUDITEE_ENTITIES e
     WHERE e.entity_id IS NOT NULL
     GROUP BY e.entity_id HAVING COUNT(*)>1
    UNION ALL
    SELECT 'ACTIVE_AUDITABLE_NO_MAPPING_COUNT', NULL, NULL, COUNT(*), NULL,
           'Active auditable master entities without any hierarchy mapping; roots/exemptions require review'
      FROM T_AUDITEE_ENTITIES e
     WHERE e.active='Y' AND e.auditable='Y'
       AND NOT EXISTS (SELECT 1 FROM T_AUDITEE_ENTITIES_MAPING m
                        WHERE m.entity_id=e.entity_id)
    UNION ALL
    SELECT 'ACTIVE_AUDITABLE_NO_MAPPING', e.entity_id, e.auditby_id,
           CAST(NULL AS NUMBER), e.active,
           SUBSTR(e.name,1,300)
      FROM T_AUDITEE_ENTITIES e
     WHERE e.active='Y' AND e.auditable='Y'
       AND NOT EXISTS (SELECT 1 FROM T_AUDITEE_ENTITIES_MAPING m
                        WHERE m.entity_id=e.entity_id)
    UNION ALL
    SELECT 'MAPPING_SELF_PARENT', m.entity_id, m.parent_id,
           CAST(NULL AS NUMBER), m.status,
           'Child name='||SUBSTR(m.c_name,1,180)||'; parent name='||SUBSTR(m.p_name,1,180)
      FROM T_AUDITEE_ENTITIES_MAPING m
     WHERE m.entity_id=m.parent_id
    UNION ALL
    SELECT 'MAPPING_INACTIVE_MASTER', m.entity_id, m.parent_id,
           CAST(NULL AS NUMBER), e.active,
           'Entity name='||SUBSTR(e.name,1,180)||'; mapping status='||NVL(m.status,'NULL')
      FROM T_AUDITEE_ENTITIES_MAPING m
      JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.entity_id
     WHERE NVL(e.active,'N')<>'Y'
    UNION ALL
    SELECT 'MAPPING_NULL_GM_COUNT', NULL, NULL, COUNT(*), NULL,
           'Active auditable entity mappings with GM_OFFICE IS NULL: do not auto-populate'
      FROM T_AUDITEE_ENTITIES_MAPING m
      JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.entity_id
     WHERE e.active='Y' AND e.auditable='Y' AND m.gm_office IS NULL
    UNION ALL
    SELECT 'MAPPING_AUDITBY_DIFFERENCE_COUNT', NULL, NULL, COUNT(*), NULL,
           'Mapping AUDITEDBY differs from master AUDITBY_ID; not necessarily wrong'
      FROM T_AUDITEE_ENTITIES_MAPING m
      JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.entity_id
     WHERE (m.auditedby<>e.auditby_id OR (m.auditedby IS NULL AND e.auditby_id IS NOT NULL)
            OR (m.auditedby IS NOT NULL AND e.auditby_id IS NULL))
    UNION ALL
    SELECT 'PARA_UNEXPECTED_STATUS', p.entity_id, p.com_id,
           CAST(NULL AS NUMBER), TO_CHAR(p.para_status),
           'OLD_PARA_ID='||NVL(TO_CHAR(p.old_para_id),'NULL')||
           '; NEW_PARA_ID='||NVL(TO_CHAR(p.new_para_id),'NULL')||
           '; IND='||NVL(SUBSTR(p.ind,1,30),'NULL')||
           '; COM_STAGE='||NVL(TO_CHAR(p.com_stage),'NULL')||
           '; COM_STATUS='||NVL(TO_CHAR(p.com_status),'NULL')||
           '; ADDED='||NVL(TO_CHAR(p.para_added_on,'YYYY-MM-DD'),'NULL')
      FROM AIS_T_AU_POST_COMPLIANCE p
     WHERE p.para_status IS NULL OR p.para_status NOT IN (0,6,8,9,28)
    UNION ALL
    SELECT 'PARA_STATUS_ZERO', p.entity_id, p.com_id,
           CAST(NULL AS NUMBER), TO_CHAR(p.para_status),
           'NEW_PARA_ID='||NVL(TO_CHAR(p.new_para_id),'NULL')||
           '; IND='||NVL(SUBSTR(p.ind,1,30),'NULL')||
           '; ADDED='||NVL(TO_CHAR(p.para_added_on,'YYYY-MM-DD'),'NULL')
      FROM AIS_T_AU_POST_COMPLIANCE p
     WHERE p.para_status=0
    UNION ALL
    SELECT 'PARA_SETTLED_WITHOUT_DATE', p.entity_id, p.com_id,
           CAST(NULL AS NUMBER), TO_CHAR(p.para_status),
           'OLD_PARA_ID='||NVL(TO_CHAR(p.old_para_id),'NULL')||
           '; NEW_PARA_ID='||NVL(TO_CHAR(p.new_para_id),'NULL')||
           '; SETTELED_BY='||NVL(TO_CHAR(p.setteled_by),'NULL')||
           '; ADDED='||NVL(TO_CHAR(p.para_added_on,'YYYY-MM-DD'),'NULL')
      FROM AIS_T_AU_POST_COMPLIANCE p
     WHERE p.para_status=9 AND p.setteled_on IS NULL
    UNION ALL
    SELECT 'ENGAGEMENT_STATUS_TOTAL', NULL, NULL, COUNT(*),
           NVL(TO_CHAR(a.status),'NULL'), 'Current engagement status distribution'
      FROM T_AU_PLAN_ENG a GROUP BY a.status
    UNION ALL
    SELECT 'ENGAGEMENT_NOT_FINAL_14_COUNT', NULL, NULL, COUNT(*), NULL,
           'Potential closure blockers (all engagement statuses other than final report issued 14)'
      FROM T_AU_PLAN_ENG a
     WHERE a.status IS NULL OR a.status<>14
  ) r
 ORDER BY r.finding, r.entity_id NULLS FIRST, r.reference_id NULLS FIRST;
