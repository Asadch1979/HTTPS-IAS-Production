-- IAS ENTITY MANAGEMENT | PHASE 1 | POST-DEPLOY VALIDATION (READ-ONLY)
SET SERVEROUTPUT ON SIZE UNLIMITED
SET LINESIZE 200
SET PAGESIZE 200
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
PROMPT 01. Package and new table status
SELECT object_name, object_type, status FROM user_objects
 WHERE object_name IN ('T_IAS_ENTITY_CHANGE_LOG','T_IAS_ENTITY_PARA_ARCHIVE',
   'S_IAS_ENTITY_CHANGE_LOG','S_IAS_ENTITY_PARA_ARCHIVE','PKG_IAS_ENTITY_MGMT')
 ORDER BY object_name,object_type;
PROMPT 02. Package compile diagnostics (must return no rows)
SELECT name,type,line,position,text FROM user_errors WHERE name='PKG_IAS_ENTITY_MGMT'
 ORDER BY sequence;
PROMPT 03. Archive source-column fidelity (must return no rows)
SELECT column_name, data_type, data_length, data_precision, data_scale
  FROM user_tab_columns WHERE table_name='AIS_T_AU_POST_COMPLIANCE'
MINUS
SELECT column_name, data_type, data_length, data_precision, data_scale
  FROM user_tab_columns WHERE table_name='T_IAS_ENTITY_PARA_ARCHIVE';
PROMPT 04. Archive/log immutability triggers: must both be ENABLED
SELECT trigger_name,table_name,status FROM user_triggers
 WHERE trigger_name IN ('TR_IAS_ENT_LOG_IMMUT','TR_IAS_ENT_ARC_IMMUT');
PROMPT 05. Archive populated before an approved closure? Should be zero now.
SELECT (SELECT COUNT(*) FROM T_IAS_ENTITY_PARA_ARCHIVE) archive_records,
       (SELECT COUNT(*) FROM T_IAS_ENTITY_CHANGE_LOG) change_events
  FROM dual;
PROMPT 06. Sample read-only procedure check: one real entity ID
VAR TEST_ENTITY_ID NUMBER
BEGIN
  SELECT MIN(entity_id) INTO :TEST_ENTITY_ID FROM T_AUDITEE_ENTITIES
   WHERE entity_id>0;
END;
/
VAR ENTITY_PROFILE REFCURSOR
EXEC PKG_IAS_ENTITY_MGMT.P_GET_ENTITY_PROFILE(:TEST_ENTITY_ID,:ENTITY_PROFILE);
PRINT ENTITY_PROFILE
VAR CLOSURE_PREVIEW REFCURSOR
EXEC PKG_IAS_ENTITY_MGMT.P_PREVIEW_ENTITY_CLOSURE(:TEST_ENTITY_ID,:CLOSURE_PREVIEW);
PRINT CLOSURE_PREVIEW
PROMPT ===== Validation complete. Compare source KPIs against prefight baseline. =====
