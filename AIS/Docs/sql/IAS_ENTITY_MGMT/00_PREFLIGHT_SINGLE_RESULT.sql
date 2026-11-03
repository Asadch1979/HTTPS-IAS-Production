-- IAS ENTITY MANAGEMENT | PRODUCTION PREFLIGHT | SINGLE RESULT GRID
-- Read-only. No DDL, DML, COMMIT, DBMS_OUTPUT or SQL*Plus directives.
-- Execute this SINGLE SELECT using your SQL client's Run Statement / Execute Statement
-- action (SQL Developer: Ctrl+Enter). Export the entire result grid to XLSX.
-- Do not execute only the first SELECT inside the UNION ALL.
-- Results have five columns: SECTION, OBJECT_NAME, CHECK_NAME, RESULT_VALUE, DETAILS.
-- This script is deliberately defensive: structural checks do not require the
-- presence of optional legacy columns or the ability to access DBA_* views.
SELECT x.section, x.object_name, x.check_name, x.result_value, x.details
  FROM (
  SELECT 10 ord, '01 ENVIRONMENT' section, USER object_name,
         'CONNECTED_SCHEMA' check_name, USER result_value,
         'Database local time: '||TO_CHAR(SYSDATE,'YYYY-MM-DD HH24:MI:SS') details
    FROM dual
  UNION ALL
  SELECT 20, '02 SOURCE OBJECTS', r.object_name, 'REQUIRED_OBJECT',
         CASE WHEN o.object_name IS NULL THEN 'MISSING'
              ELSE o.status END,
         CASE WHEN o.object_name IS NULL THEN 'Expected TABLE is not owned by connected schema'
              ELSE 'Owned object: '||o.object_type END
    FROM (SELECT 'T_AUDITEE_ENTITIES' object_name FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE' FROM dual
          UNION ALL SELECT 'T_AU_PLAN_ENG' FROM dual) r
    LEFT JOIN user_objects o ON o.object_name=r.object_name AND o.object_type='TABLE'
  UNION ALL
  SELECT 30, '03 TABLE COLUMNS', c.table_name, c.column_name,
         c.data_type,
         'POSITION='||c.column_id||'; LENGTH='||c.data_length||
         '; PRECISION='||NVL(TO_CHAR(c.data_precision),'-')||
         '; SCALE='||NVL(TO_CHAR(c.data_scale),'-')||
         '; NULLABLE='||c.nullable
    FROM user_tab_columns c
   WHERE c.table_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING',
                          'AIS_T_AU_POST_COMPLIANCE','T_AU_PLAN_ENG')
  UNION ALL
  SELECT 40, '04 REQUIRED COLUMNS', r.table_name, r.column_name,
         CASE WHEN c.column_name IS NULL THEN 'MISSING' ELSE 'PRESENT' END,
         CASE WHEN c.column_name IS NULL THEN 'STOP: reconcile schema before DDL'
              ELSE c.data_type||'; NULLABLE='||c.nullable END
    FROM (SELECT 'T_AUDITEE_ENTITIES' table_name, 'ENTITY_ID' column_name FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES','CODE' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES','NAME' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES','TYPE_ID' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES','AUDITBY_ID' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES','ACTIVE' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING','ENTITY_ID' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING','PARENT_ID' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING','AUDITEDBY' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING','GM_OFFICE' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING','REPORTING' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING','C_TYPE_ID' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','COM_ID' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','ENTITY_ID' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','PARA_STATUS' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','PARA_ADDED_ON' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','SETTELED_ON' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','COM_STATUS' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','COM_STAGE' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','COM_CYCLE' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','SETTELED_BY' FROM dual
          UNION ALL SELECT 'T_AU_PLAN_ENG','ENTITY_ID' FROM dual
          UNION ALL SELECT 'T_AU_PLAN_ENG','STATUS' FROM dual) r
    LEFT JOIN user_tab_columns c
      ON c.table_name=r.table_name AND c.column_name=r.column_name
  UNION ALL
  SELECT 50, '05 LEGACY/REPORT COLUMNS', q.table_name, q.column_name,
         CASE WHEN c.column_name IS NULL THEN 'ABSENT' ELSE 'PRESENT' END,
         CASE WHEN c.column_name IS NULL THEN 'Determine whether referenced SQL must change'
              ELSE c.data_type END
    FROM (SELECT 'T_AUDITEE_ENTITIES_MAPING' table_name, 'RELATION_TYPE_ID' column_name FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING','STATUS' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING','B_GROUP' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING','DIV_OFFICE' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES','COMPLICE_BY' FROM dual
          UNION ALL SELECT 'T_AUDITEE_ENTITIES','COMPLIANCE_UNIT' FROM dual
          UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE','ISSUE_RECTIFIED' FROM dual) q
    LEFT JOIN user_tab_columns c
      ON c.table_name=q.table_name AND c.column_name=q.column_name
  UNION ALL
  SELECT 60, '06 CONSTRAINTS', c.table_name, c.constraint_name,
         c.constraint_type||' / '||c.status,
         'VALIDATED='||c.validated||'; DELETE_RULE='||NVL(c.delete_rule,'-')
    FROM user_constraints c
   WHERE c.table_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING',
                          'AIS_T_AU_POST_COMPLIANCE','T_AU_PLAN_ENG')
  UNION ALL
  SELECT 70, '07 INDEXES', i.table_name, i.index_name,
         i.uniqueness||' / '||i.status,
         'TABLESPACE='||NVL(i.tablespace_name,'-')||'; TYPE='||i.index_type
    FROM user_indexes i
   WHERE i.table_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING',
                          'AIS_T_AU_POST_COMPLIANCE','T_AU_PLAN_ENG')
  UNION ALL
  SELECT 80, '08 TRIGGERS', t.table_name, t.trigger_name,
         t.status, t.triggering_event
    FROM user_triggers t
   WHERE t.table_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING',
                          'AIS_T_AU_POST_COMPLIANCE','T_AU_PLAN_ENG')
  UNION ALL
  SELECT 90, '09 DIRECT DEPENDENCIES', d.referenced_name, d.name,
         d.type, 'DEPENDS ON '||d.referenced_type
    FROM user_dependencies d
   WHERE d.referenced_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING',
                               'AIS_T_AU_POST_COMPLIANCE')
  UNION ALL
  SELECT 100, '10 TABLE COUNTS', 'T_AUDITEE_ENTITIES', 'ROWS',
         TO_CHAR(COUNT(*)), 'Entity master records'
    FROM T_AUDITEE_ENTITIES
  UNION ALL
  SELECT 100, '10 TABLE COUNTS', 'T_AUDITEE_ENTITIES_MAPING', 'ROWS',
         TO_CHAR(COUNT(*)), 'Entity mapping records'
    FROM T_AUDITEE_ENTITIES_MAPING
  UNION ALL
  SELECT 100, '10 TABLE COUNTS', 'AIS_T_AU_POST_COMPLIANCE', 'ROWS',
         TO_CHAR(COUNT(*)), 'Post-compliance records'
    FROM AIS_T_AU_POST_COMPLIANCE
  UNION ALL
  SELECT 110, '11 MAPPING QUALITY', 'T_AUDITEE_ENTITIES_MAPING', 'DUPLICATE_ENTITY_IDS',
         TO_CHAR(COUNT(*)), 'Number of distinct entity IDs with more than one mapping row'
    FROM (SELECT m.entity_id FROM T_AUDITEE_ENTITIES_MAPING m
           GROUP BY m.entity_id HAVING COUNT(*)>1)
  UNION ALL
  SELECT 111, '11 MAPPING QUALITY', 'T_AUDITEE_ENTITIES_MAPING', 'MAPPING_WITHOUT_MASTER',
         TO_CHAR(COUNT(*)), 'Mapping entity_id not found in master'
    FROM T_AUDITEE_ENTITIES_MAPING m
    LEFT JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.entity_id
   WHERE e.entity_id IS NULL
  UNION ALL
  SELECT 112, '11 MAPPING QUALITY', 'T_AUDITEE_ENTITIES_MAPING', 'PARENT_WITHOUT_MASTER',
         TO_CHAR(COUNT(*)), 'Non-null mapping parent_id not found in master'
    FROM T_AUDITEE_ENTITIES_MAPING m
    LEFT JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.parent_id
   WHERE m.parent_id IS NOT NULL AND e.entity_id IS NULL
  UNION ALL
  SELECT 113, '11 MAPPING QUALITY', 'T_AUDITEE_ENTITIES_MAPING', 'SELF_PARENT',
         TO_CHAR(COUNT(*)), 'Mapping entity_id is same as parent_id'
    FROM T_AUDITEE_ENTITIES_MAPING m
   WHERE m.entity_id=m.parent_id
  UNION ALL
  SELECT 114, '11 MAPPING QUALITY', 'T_AUDITEE_ENTITIES_MAPING', 'INACTIVE_WITH_MAPPING',
         TO_CHAR(COUNT(*)), 'Mapping rows whose entity is marked inactive; review, not necessarily an error'
    FROM T_AUDITEE_ENTITIES_MAPING m
    JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.entity_id
   WHERE NVL(e.active,'N')<>'Y'
  UNION ALL
  SELECT 115, '11 MAPPING QUALITY', 'T_AUDITEE_ENTITIES_MAPING', 'MAPPING_NULL_PARENT',
         TO_CHAR(COUNT(*)), 'Review legitimately unmapped root entities separately'
    FROM T_AUDITEE_ENTITIES_MAPING m
   WHERE m.parent_id IS NULL
  UNION ALL
  SELECT 120, '12 PARA STATUS', 'AIS_T_AU_POST_COMPLIANCE',
         'STATUS='||NVL(TO_CHAR(p.para_status),'NULL'),
         TO_CHAR(COUNT(*)),
         'MISSING_ADDED_DATE='||SUM(CASE WHEN p.para_added_on IS NULL THEN 1 ELSE 0 END)||
         '; MISSING_SETTLED_DATE='||SUM(CASE WHEN p.setteled_on IS NULL THEN 1 ELSE 0 END)
    FROM AIS_T_AU_POST_COMPLIANCE p
   GROUP BY p.para_status
  UNION ALL
  SELECT 130, '13 PARA SUMMARY', 'AIS_T_AU_POST_COMPLIANCE', 'OPEN_STATUS_8',
         TO_CHAR(COUNT(*)), 'Open status count at extraction time'
    FROM AIS_T_AU_POST_COMPLIANCE p WHERE p.para_status=8
  UNION ALL
  SELECT 130, '13 PARA SUMMARY', 'AIS_T_AU_POST_COMPLIANCE', 'SETTLED_STATUS_9',
         TO_CHAR(COUNT(*)), 'Settled status count at extraction time'
    FROM AIS_T_AU_POST_COMPLIANCE p WHERE p.para_status=9
  UNION ALL
  SELECT 130, '13 PARA SUMMARY', 'AIS_T_AU_POST_COMPLIANCE', 'STATUS_0',
         TO_CHAR(COUNT(*)), 'Investigate existing para status zero records'
    FROM AIS_T_AU_POST_COMPLIANCE p WHERE p.para_status=0
  UNION ALL
  SELECT 130, '13 PARA SUMMARY', 'AIS_T_AU_POST_COMPLIANCE', 'NULL_PARA_ADDED_ON',
         TO_CHAR(COUNT(*)), 'Rows without issue/para added date'
    FROM AIS_T_AU_POST_COMPLIANCE p WHERE p.para_added_on IS NULL
  UNION ALL
  SELECT 130, '13 PARA SUMMARY', 'AIS_T_AU_POST_COMPLIANCE', 'NULL_SETTLED_ON_STATUS_9',
         TO_CHAR(COUNT(*)), 'Settled status rows without settlement date'
    FROM AIS_T_AU_POST_COMPLIANCE p
   WHERE p.para_status=9 AND p.setteled_on IS NULL
  UNION ALL
  SELECT 140, '14 ENGAGEMENT STATUS', 'T_AU_PLAN_ENG',
         'STATUS='||NVL(TO_CHAR(a.status),'NULL'),
         TO_CHAR(COUNT(*)), 'Engagements by current status'
    FROM T_AU_PLAN_ENG a GROUP BY a.status
  UNION ALL
  SELECT 150, '15 DATABASE OBJECTS', o.object_name, o.object_type,
         o.status, 'Existing IAS procedure/view compilation status'
    FROM user_objects o
   WHERE o.object_name IN ('PKG_AD','PKG_ORG_STRUCTURE','PKG_LG','PKG_USER_CONTEXT',
      'V_EXEC_KPI_LIFECYCLE','V_EXEC_REPORT_SOURCE','V_EXEC_ENTITY_SCOPE',
      'PKG_IAS_ENTITY_MGMT','T_IAS_ENTITY_CHANGE_LOG','T_IAS_ENTITY_PARA_ARCHIVE')
  UNION ALL
  SELECT 160, '16 ENTITY ID SEQUENCES', s.sequence_name, 'SEQUENCE',
         'PRESENT', 'LAST_NUMBER='||s.last_number||'; CACHE='||s.cache_size
    FROM user_sequences s WHERE s.sequence_name LIKE '%ENTIT%'
  UNION ALL
  SELECT 170, '17 FOUNDATION NAME COLLISION', o.object_name, o.object_type,
         o.status, 'Stop if any proposed Phase-1 foundation name already exists'
    FROM user_objects o
   WHERE o.object_name IN ('T_IAS_ENTITY_CHANGE_LOG','T_IAS_ENTITY_PARA_ARCHIVE',
      'S_IAS_ENTITY_CHANGE_LOG','S_IAS_ENTITY_PARA_ARCHIVE',
      'IX_IAS_ENTITY_LOG_ENTITY_ON','IX_IAS_ENTITY_LOG_BATCH',
      'IX_IAS_ENTITY_LOG_REQUEST','IX_IAS_ENTITY_ARCHIVE_ENT_COM',
      'IX_IAS_ENTITY_ARCHIVE_DATE','TR_IAS_ENT_LOG_IMMUT','TR_IAS_ENT_ARC_IMMUT')
  UNION ALL
  SELECT 180, '18 QUOTA', q.tablespace_name, 'TABLESPACE_QUOTA',
         CASE WHEN q.max_bytes=-1 THEN 'UNLIMITED'
              ELSE TO_CHAR(q.max_bytes) END,
         'BYTES='||q.bytes||'; MISC is used by proposed Phase-1 DDL'
    FROM user_ts_quotas q
   WHERE q.tablespace_name='MISC'
  ) x
 ORDER BY x.ord, x.object_name, x.check_name;
