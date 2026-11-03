-- IAS ENTITY MANAGEMENT | ROLLBACK NEW AND EMPTY FOUNDATION ONLY
-- This script removes new Phase 1 objects. It cannot rollback pre-existing Oracle DDL.
-- DO NOT RUN if there are real business events or archived para records.
-- Explicitly intended for staging after a failed/aborted pilot.
SET SERVEROUTPUT ON SIZE UNLIMITED
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
DECLARE
  v_archive_rows NUMBER;
  v_log_rows NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_archive_rows FROM T_IAS_ENTITY_PARA_ARCHIVE;
  SELECT COUNT(*) INTO v_log_rows FROM T_IAS_ENTITY_CHANGE_LOG;
  IF v_archive_rows<>0 OR v_log_rows<>0 THEN
    RAISE_APPLICATION_ERROR(-20730,'Rollback refused: entity history tables are NOT empty.');
  END IF;
  DBMS_OUTPUT.PUT_LINE('Both history tables empty; safe to drop only new Phase 1 objects.');
END;
/
DROP PACKAGE PKG_IAS_ENTITY_MGMT;
DROP TABLE T_IAS_ENTITY_PARA_ARCHIVE PURGE;
DROP TABLE T_IAS_ENTITY_CHANGE_LOG PURGE;
DROP SEQUENCE S_IAS_ENTITY_PARA_ARCHIVE;
DROP SEQUENCE S_IAS_ENTITY_CHANGE_LOG;
PROMPT New empty foundation removed. Existing master and para tables untouched.
