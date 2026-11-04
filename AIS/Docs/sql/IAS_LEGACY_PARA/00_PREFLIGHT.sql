-- READ ONLY. Run in the intended Oracle owner and retain the spool for review.
-- No grants, menu changes, DML, sequence allocation or production posting.
SET PAGESIZE 50000
SET LONG 1000000
SET LINESIZE 240
SPOOL legacy_para_preflight.txt
SELECT USER AS schema_owner, systimestamp AS captured_on FROM dual;
SELECT table_name, column_id, column_name, data_type, data_length,
       data_precision, data_scale, nullable, data_default
  FROM user_tab_columns
 WHERE table_name IN ('T_AU_OLD_PARAS_FAD','T_AU_OLD_PARAS_FAD_TEXT',
       'AIS_T_AU_POST_COMPLIANCE','T_AU_OLD_AUDIT_PERIOD',
       'T_AU_OLD_AUDIT_NATURE','T_AUDITEE_ENTITIES')
 ORDER BY table_name, column_id;
SELECT table_name, column_name, generation_type, sequence_name, identity_options
  FROM user_tab_identity_cols
 WHERE table_name IN ('T_AU_OLD_PARAS_FAD','T_AU_OLD_PARAS_FAD_TEXT',
                      'AIS_T_AU_POST_COMPLIANCE');
SELECT c.table_name, c.constraint_name, c.constraint_type, c.status,
       c.validated, c.r_constraint_name, cc.column_name, cc.position
  FROM user_constraints c
  LEFT JOIN user_cons_columns cc ON cc.constraint_name=c.constraint_name
 WHERE c.table_name IN ('T_AU_OLD_PARAS_FAD','T_AU_OLD_PARAS_FAD_TEXT',
                        'AIS_T_AU_POST_COMPLIANCE')
 ORDER BY c.table_name,c.constraint_name,cc.position;
SELECT table_name, index_name, column_name, column_position
  FROM user_ind_columns
 WHERE table_name IN ('T_AU_OLD_PARAS_FAD','T_AU_OLD_PARAS_FAD_TEXT',
                      'AIS_T_AU_POST_COMPLIANCE')
 ORDER BY table_name,index_name,column_position;
SELECT table_name, trigger_name, status, trigger_type, triggering_event,
       trigger_body
  FROM user_triggers
 WHERE table_name IN ('T_AU_OLD_PARAS_FAD','T_AU_OLD_PARAS_FAD_TEXT',
                      'AIS_T_AU_POST_COMPLIANCE');
SELECT sequence_name, increment_by, cycle_flag, cache_size, last_number
  FROM user_sequences ORDER BY sequence_name;
-- Related structures: inspect their keys before selecting mappings.
SELECT table_name, column_name, data_type, nullable
  FROM user_tab_columns
 WHERE REGEXP_LIKE(table_name,'(RESPONS|ATTACH|REFERENCE|CIRCULAR|ANNEX|CHECKLIST)')
 ORDER BY table_name,column_id;
SELECT name,type,line,text FROM user_source
 WHERE type='PACKAGE BODY' AND name IN ('PKG_HD','PKG_FAD','PKG_AR','PKG_AD')
   AND (UPPER(text) LIKE '%PARA_ADDED_ON%'
     OR UPPER(text) LIKE '%OLD_PARA_ID%'
     OR UPPER(text) LIKE '%OLD_PARAS_FAD_TEXT%')
 ORDER BY name,line;
SELECT 'T_AU_OLD_PARAS_FAD' AS target, COUNT(*) AS before_count
  FROM T_AU_OLD_PARAS_FAD
UNION ALL SELECT 'T_AU_OLD_PARAS_FAD_TEXT',COUNT(*) FROM T_AU_OLD_PARAS_FAD_TEXT
UNION ALL SELECT 'AIS_T_AU_POST_COMPLIANCE',COUNT(*) FROM AIS_T_AU_POST_COMPLIANCE;
SPOOL OFF
