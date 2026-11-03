-- IAS ENTITY MANAGEMENT | PHASE 1 | READ-ONLY DATABASE PREFLIGHT
-- Run as the OWNER of IAS master tables, using SQLcl/SQL*Plus/SQL Developer Run Script.
-- No DML or DDL in this script. Save output for comparison before any deployment.
SET SERVEROUTPUT ON SIZE UNLIMITED
SET LINESIZE 220
SET PAGESIZE 200
SET LONG 10000
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
PROMPT ===== IAS ENTITY MANAGEMENT: SCHEMA AND DATA PREFLIGHT =====
SELECT USER AS SCHEMA_OWNER, SYSDATE AS DB_TIME FROM DUAL;
PROMPT 01. Verify required tables exist
SELECT table_name FROM user_tables
 WHERE table_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING',
   'AIS_T_AU_POST_COMPLIANCE','T_AU_PLAN_ENG','T_USER_SESSION',
   'T_AUDITEE_ENTITIES_UPDATE_LOG','T_AU_ENTITY_SHIFTING')
 ORDER BY table_name;
PROMPT 02. Source structures, including fields omitted from historic repository DDL
SELECT table_name, column_id, column_name, data_type, data_length,
       data_precision, data_scale, nullable
  FROM user_tab_columns
 WHERE table_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING',
                     'AIS_T_AU_POST_COMPLIANCE')
 ORDER BY table_name, column_id;
PROMPT 03. Source objects absent / critical columns absent: MUST return NO rows
SELECT 'TABLE_MISSING' AS issue, 'T_AUDITEE_ENTITIES' AS item FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tables WHERE table_name='T_AUDITEE_ENTITIES')
UNION ALL SELECT 'TABLE_MISSING','T_AUDITEE_ENTITIES_MAPING' FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tables WHERE table_name='T_AUDITEE_ENTITIES_MAPING')
UNION ALL SELECT 'TABLE_MISSING','AIS_T_AU_POST_COMPLIANCE' FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tables WHERE table_name='AIS_T_AU_POST_COMPLIANCE')
UNION ALL SELECT 'COLUMN_MISSING','POST_COMPLIANCE.COM_ID' FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tab_columns WHERE table_name='AIS_T_AU_POST_COMPLIANCE' AND column_name='COM_ID')
UNION ALL SELECT 'COLUMN_MISSING','POST_COMPLIANCE.PARA_STATUS' FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tab_columns WHERE table_name='AIS_T_AU_POST_COMPLIANCE' AND column_name='PARA_STATUS')
UNION ALL SELECT 'COLUMN_MISSING','POST_COMPLIANCE.PARA_ADDED_ON' FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tab_columns WHERE table_name='AIS_T_AU_POST_COMPLIANCE' AND column_name='PARA_ADDED_ON')
UNION ALL SELECT 'COLUMN_MISSING','POST_COMPLIANCE.SETTELED_ON' FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tab_columns WHERE table_name='AIS_T_AU_POST_COMPLIANCE' AND column_name='SETTELED_ON')
UNION ALL SELECT 'COLUMN_MISSING','MAPPING.GM_OFFICE' FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tab_columns WHERE table_name='T_AUDITEE_ENTITIES_MAPING' AND column_name='GM_OFFICE')
UNION ALL SELECT 'COLUMN_MISSING','MAPPING.AUDITEDBY' FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tab_columns WHERE table_name='T_AUDITEE_ENTITIES_MAPING' AND column_name='AUDITEDBY')
UNION ALL SELECT 'COLUMN_MISSING','MASTER.AUDITBY_ID' FROM dual
 WHERE NOT EXISTS (SELECT 1 FROM user_tab_columns WHERE table_name='T_AUDITEE_ENTITIES' AND column_name='AUDITBY_ID');
PROMPT 04. Known repository vs supplied-DDL discrepancies. Nonzero values require investigation.
SELECT
 (SELECT COUNT(*) FROM user_tab_columns WHERE table_name='T_AUDITEE_ENTITIES_MAPING' AND column_name='RELATION_TYPE_ID') AS MAPPING_HAS_LEGACY_REL_TYPE,
 (SELECT COUNT(*) FROM user_tab_columns WHERE table_name='T_AUDITEE_ENTITIES' AND column_name='COMPLICE_BY') AS MASTER_HAS_COMPLICE_BY,
 (SELECT COUNT(*) FROM user_tab_columns WHERE table_name='T_AUDITEE_ENTITIES' AND column_name='COMPLIANCE_UNIT') AS MASTER_HAS_COMPLIANCE_UNIT,
 (SELECT COUNT(*) FROM user_tab_columns WHERE table_name='AIS_T_AU_POST_COMPLIANCE' AND column_name='ISSUE_RECTIFIED') AS PARA_HAS_ISSUE_RECTIFIED
 FROM dual;
PROMPT 05. Existing PKs, constraints, indexes and DML triggers
SELECT c.table_name, c.constraint_name, c.constraint_type, c.status, c.validated
  FROM user_constraints c
 WHERE c.table_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING','AIS_T_AU_POST_COMPLIANCE')
 ORDER BY c.table_name,c.constraint_type,c.constraint_name;
SELECT table_name,index_name,uniqueness,status FROM user_indexes
 WHERE table_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING','AIS_T_AU_POST_COMPLIANCE')
 ORDER BY table_name,index_name;
SELECT table_name,trigger_name,status,triggering_event FROM user_triggers
 WHERE table_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING','AIS_T_AU_POST_COMPLIANCE')
 ORDER BY table_name,trigger_name;
PROMPT 06. Objects depending directly on the operational master / compliance tables
SELECT name,type,referenced_name,referenced_type FROM user_dependencies
 WHERE referenced_name IN ('T_AUDITEE_ENTITIES','T_AUDITEE_ENTITIES_MAPING','AIS_T_AU_POST_COMPLIANCE')
 ORDER BY referenced_name,type,name;
PROMPT 07. Master and mapping cardinality
SELECT 'T_AUDITEE_ENTITIES' AS source_name, COUNT(*) AS row_count FROM T_AUDITEE_ENTITIES
UNION ALL SELECT 'T_AUDITEE_ENTITIES_MAPING', COUNT(*) FROM T_AUDITEE_ENTITIES_MAPING
UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE', COUNT(*) FROM AIS_T_AU_POST_COMPLIANCE;
PROMPT 08. Duplicate entity identifiers in mapping (inspect before assuming 1:1)
SELECT entity_id, COUNT(*) AS mappings,
       COUNT(DISTINCT parent_id) AS distinct_parent_ids,
       COUNT(DISTINCT c_type_id) AS distinct_child_types
  FROM T_AUDITEE_ENTITIES_MAPING
 GROUP BY entity_id HAVING COUNT(*)>1
 ORDER BY mappings DESC,entity_id FETCH FIRST 100 ROWS ONLY;
PROMPT 09. Missing, inactive and self-referential mapping errors
SELECT 'MAPPING_NO_MASTER' issue, COUNT(*) cnt
 FROM T_AUDITEE_ENTITIES_MAPING m LEFT JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.entity_id
 WHERE e.entity_id IS NULL
UNION ALL SELECT 'PARENT_NO_MASTER', COUNT(*)
 FROM T_AUDITEE_ENTITIES_MAPING m LEFT JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.parent_id
 WHERE m.parent_id IS NOT NULL AND e.entity_id IS NULL
UNION ALL SELECT 'SELF_PARENT', COUNT(*) FROM T_AUDITEE_ENTITIES_MAPING m WHERE m.parent_id=m.entity_id
UNION ALL SELECT 'INACTIVE_ENTITY_ACTIVE_MAPPING', COUNT(*)
 FROM T_AUDITEE_ENTITIES_MAPING m JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.entity_id
 WHERE NVL(e.active,'N')<>'Y' AND NVL(m.status,'Y')='Y'
UNION ALL SELECT 'AUDIT_CLUSTER_MISMATCH', COUNT(*)
 FROM T_AUDITEE_ENTITIES_MAPING m JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.entity_id
 WHERE NVL(m.auditedby,-1)<>NVL(e.auditby_id,-1)
UNION ALL SELECT 'ENTITY_TYPE_MISMATCH', COUNT(*)
 FROM T_AUDITEE_ENTITIES_MAPING m JOIN T_AUDITEE_ENTITIES e ON e.entity_id=m.entity_id
 WHERE NVL(m.c_type_id,-1)<>NVL(e.type_id,-1)
UNION ALL SELECT 'ACTIVE_MAPPING_WITHOUT_GM', COUNT(*)
 FROM T_AUDITEE_ENTITIES_MAPING m
 WHERE NVL(m.status,'Y')='Y' AND m.gm_office IS NULL;
PROMPT 10. Post-compliance status and date baseline (do not alter until reports are reconciled)
SELECT para_status, COUNT(*) AS para_rows,
       SUM(CASE WHEN para_added_on IS NULL THEN 1 ELSE 0 END) AS missing_added_date,
       SUM(CASE WHEN setteled_on IS NULL THEN 1 ELSE 0 END) AS missing_settled_date
  FROM AIS_T_AU_POST_COMPLIANCE GROUP BY para_status ORDER BY para_status;
SELECT COUNT(*) AS total_paras,
       SUM(CASE WHEN para_status=8 THEN 1 ELSE 0 END) AS open_paras,
       SUM(CASE WHEN para_status=9 THEN 1 ELSE 0 END) AS settled_paras,
       SUM(CASE WHEN para_status=0 THEN 1 ELSE 0 END) AS status_zero,
       SUM(CASE WHEN para_added_on IS NULL THEN 1 ELSE 0 END) AS missing_added_date
  FROM AIS_T_AU_POST_COMPLIANCE;
PROMPT 11. Current engagement statuses: check live codes before implementing closure blockers
SELECT status, COUNT(*) AS engagements FROM T_AU_PLAN_ENG GROUP BY status ORDER BY status;
PROMPT 12. Existing IAS entity administration package compilation status
SELECT object_name,object_type,status FROM user_objects
 WHERE object_name IN ('PKG_AD','PKG_ORG_STRUCTURE','PKG_LG','PKG_USER_CONTEXT',
                       'V_EXEC_KPI_LIFECYCLE','V_EXEC_REPORT_SOURCE','V_EXEC_ENTITY_SCOPE')
 ORDER BY object_name,object_type;
PROMPT ===== PREFLIGHT COMPLETE. ALL FINDINGS REQUIRE REVIEW; NO DATA CHANGED =====
