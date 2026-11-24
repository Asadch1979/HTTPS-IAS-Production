-- Read only; labels do not constitute approval.
SELECT SYS_CONTEXT('USERENV','CURRENT_SCHEMA') current_schema FROM dual;
SELECT table_name,column_name,data_type FROM user_tab_columns WHERE table_name IN ('T_AU_PLAN_ENG','T_AUDITEE_ENTITIES','T_AU_OBSERVATION','T_FAD_RISK_MAP_STAGE');
SELECT statusid,statusname FROM t_au_observation_status;
SELECT status,COUNT(*) FROM t_au_plan_eng GROUP BY status;
SELECT type_id,COUNT(*) FROM t_auditee_entities GROUP BY type_id;
SELECT version_id,state,calculator FROM T_FAD_RISK_VERSION;
-- Stop installation if approved numeric ANEX_ID is absent. Never infer aliases.
SELECT anex_id,COUNT(*) FROM T_FAD_RISK_MAP_STAGE GROUP BY anex_id HAVING COUNT(*)>1 OR anex_id IS NULL;

-- Period 5 is the initial target, never a hardcoded authorization override.
SELECT auditperiodid FROM T_AU_PERIOD WHERE auditperiodid=5;
SELECT g.auditby_id,b.type_id,g.status,COUNT(*) engagements FROM T_AU_PLAN_ENG g
 JOIN T_AUDITEE_ENTITIES b ON b.entity_id=g.entity_id WHERE g.period_id=5
 GROUP BY g.auditby_id,b.type_id,g.status;
SELECT s.excel_row,s.annexure_code,s.anex_id,a.code operational_code
 FROM T_FAD_RISK_MAP_STAGE s LEFT JOIN T_AUDIT_CHECKLIST_ANNEXURE a ON a.id=s.anex_id
 WHERE a.id IS NULL OR UPPER(TRIM(a.code))<>UPPER(TRIM(s.annexure_code));
-- Read after applying 11_review_upgrade.sql. Existing rows must not be auto-approved.
SELECT policy_id,entity_type_id,policy_version,approval_ref,approved_on,reference_rule FROM T_FAD_ENGINE_POLICY;
SELECT eng_id,policy_version,final_observation_count,approval_ref,approved_on FROM T_FAD_ENGINE_COMPLETENESS;
