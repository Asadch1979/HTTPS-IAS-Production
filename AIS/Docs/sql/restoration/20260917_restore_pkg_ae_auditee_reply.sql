-- Restoration script for an environment where fb7ffc25's PKG_AE body was deployed.
-- Review the canonical PKG_AE.sql diff before execution: this recompiles the full package.
-- Run from this script's directory with SQL*Plus or SQLcl.

WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
SET DEFINE OFF

PROMPT Restoring canonical PKG_AE package specification and body...
@@../PKG_AE.sql

DECLARE
  v_error_count NUMBER;
BEGIN
  SELECT COUNT(*)
    INTO v_error_count
    FROM USER_ERRORS
   WHERE NAME = 'PKG_AE'
     AND TYPE IN ('PACKAGE', 'PACKAGE BODY');

  IF v_error_count > 0 THEN
    RAISE_APPLICATION_ERROR(-20001, 'PKG_AE restoration compiled with errors. Review USER_ERRORS.');
  END IF;
END;
/

PROMPT PKG_AE restoration completed without compilation errors.
