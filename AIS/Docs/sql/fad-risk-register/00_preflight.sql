-- READ ONLY. Run as the IAS schema owner; retain output for review.
SET PAGESIZE 50000
SET LINESIZE 250
SET LONG 1000000
SPOOL fad-risk-preflight.txt
SELECT SYS_CONTEXT('USERENV','DB_NAME') db_name, USER schema_name FROM dual;
SELECT table_name,column_id,column_name,data_type,data_length,data_precision,
       data_scale,nullable,data_default,identity_column
FROM user_tab_columns WHERE table_name LIKE 'T_RISK_FAD_%'
ORDER BY table_name,column_id;
SELECT c.table_name,c.constraint_name,c.constraint_type,c.status,c.r_constraint_name,
       cc.column_name,cc.position
FROM user_constraints c JOIN user_cons_columns cc ON cc.constraint_name=c.constraint_name
WHERE c.table_name LIKE 'T_RISK_FAD_%' ORDER BY c.table_name,c.constraint_name,cc.position;
SELECT trigger_name,table_name,status,trigger_body FROM user_triggers
WHERE table_name LIKE 'T_RISK_FAD_%';
SELECT sequence_name,last_number,increment_by FROM user_sequences WHERE sequence_name LIKE '%RISK%FAD%';
SELECT * FROM T_RISK_FAD_KPI_MAIN;
SELECT * FROM T_RISK_FAD_KPI_SUB;
SELECT * FROM T_RISK_FAD_PROCESS;
SELECT * FROM T_RISK_FAD_SUB_PROCESS;
SELECT * FROM T_RISK_FAD_ANNEXURE;
SELECT * FROM T_RISK_FAD_SUBPROC_ANNEX;
SELECT * FROM T_RISK_FAD_GRAVITY;
SELECT * FROM T_RISK_FAD_RATING_MATRIX;
SELECT 'ANNEXURE_COUNT' table_name,COUNT(*) row_count FROM T_RISK_FAD_ANNEXURE_COUNT
UNION ALL SELECT 'KPI_RESULT',COUNT(*) FROM T_RISK_FAD_KPI_RESULT
UNION ALL SELECT 'WORK_RESULT',COUNT(*) FROM T_RISK_FAD_WORK_RESULT;
SELECT name,type,line,text FROM user_source
WHERE name IN ('PKG_FAD_RISK','PKG_FRPT') ORDER BY name,type,line;
SELECT namespace,schema,package FROM all_context;
SPOOL OFF
