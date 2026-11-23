-- READ ONLY; retain output for schema owner review before DDL.
SET LONG 1000000
SET LINESIZE 250
SET PAGESIZE 50000
SPOOL fad-hierarchy-schema-review.txt
SELECT USER, SYS_CONTEXT('USERENV','DB_NAME') db_name FROM dual;
SELECT table_name,column_name,data_type,data_length,char_length,nullable,data_default
FROM user_tab_columns WHERE table_name LIKE 'T_RISK_FAD_%'
 OR table_name IN ('T_FAD_RISK_MAP_STAGE','T_AUDIT_CHECKLIST_ANNEXURE') ORDER BY table_name,column_id;
SELECT c.table_name,c.constraint_name,c.constraint_type,c.status,c.validated,c.r_constraint_name,cc.column_name,cc.position
FROM user_constraints c JOIN user_cons_columns cc ON cc.constraint_name=c.constraint_name
WHERE c.table_name LIKE 'T_RISK_FAD_%' OR c.table_name='T_AUDIT_CHECKLIST_ANNEXURE'
ORDER BY c.table_name,c.constraint_name,cc.position;
SELECT table_name,trigger_name,status,trigger_body FROM user_triggers WHERE table_name LIKE 'T_RISK_FAD_%';
SELECT sequence_name,increment_by,last_number FROM user_sequences WHERE sequence_name LIKE 'SEQ_RISK_FAD_%';
SELECT name,type,referenced_name FROM user_dependencies WHERE referenced_name LIKE 'T_RISK_FAD_%';
SELECT name,type,line,text FROM user_source WHERE name='PKG_FAD_RISK' ORDER BY type,line;
SELECT * FROM T_FAD_RISK_MAP_STAGE;
SELECT * FROM T_RISK_FAD_GRAVITY;
SPOOL OFF
