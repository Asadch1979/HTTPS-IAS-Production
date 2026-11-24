-- Read only; labels do not constitute approval.
SELECT SYS_CONTEXT('USERENV','CURRENT_SCHEMA') current_schema FROM dual;
SELECT table_name,column_name,data_type FROM user_tab_columns WHERE table_name IN ('T_AU_PLAN_ENG','T_AUDITEE_ENTITIES','T_AU_OBSERVATION','T_FAD_RISK_MAP_STAGE');
SELECT statusid,statusname FROM t_au_observation_status;
SELECT status,COUNT(*) FROM t_au_plan_eng GROUP BY status;
SELECT type_id,COUNT(*) FROM t_auditee_entities GROUP BY type_id;
SELECT version_id,state,calculator FROM T_FAD_RISK_VERSION;
-- Stop installation if approved numeric ANEX_ID is absent. Never infer aliases.
SELECT anex_id,COUNT(*) FROM T_FAD_RISK_MAP_STAGE GROUP BY anex_id HAVING COUNT(*)>1 OR anex_id IS NULL;
