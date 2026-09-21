-- Deploy from this directory with SQL*Plus or SQLcl.
-- This script recompiles the complete checked-in PKG_LG specification and body
-- so all existing package members are preserved while adding the atomic email claim.

WHENEVER OSERROR EXIT FAILURE ROLLBACK
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

SET DEFINE OFF
SET SERVEROUTPUT ON

PROMPT Verifying that T_AU_SYSTEM_ERROR_MASTER accepts the pending state...

DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*)
    INTO v_count
    FROM USER_CONSTRAINTS
   WHERE TABLE_NAME = 'T_AU_SYSTEM_ERROR_MASTER'
     AND CONSTRAINT_TYPE = 'C'
     AND UPPER(SEARCH_CONDITION_VC) LIKE '%EMAIL_SENT%'
     AND UPPER(SEARCH_CONDITION_VC) LIKE '%''P''%';

  IF v_count = 0 THEN
    RAISE_APPLICATION_ERROR(
      -20001,
      'T_AU_SYSTEM_ERROR_MASTER.EMAIL_SENT must allow N, P and Y before PKG_LG is deployed. Run system_error_monitoring_delta_20260903.sql first.');
  END IF;
END;
/

PROMPT Recompiling PKG_LG with CLAIM_SYSTEM_ERROR_EMAIL...
@@PKG_LG.sql

DECLARE
  v_count NUMBER;
BEGIN
  SELECT COUNT(*)
    INTO v_count
    FROM USER_PROCEDURES
   WHERE OBJECT_NAME = 'PKG_LG'
     AND PROCEDURE_NAME = 'CLAIM_SYSTEM_ERROR_EMAIL';

  IF v_count <> 1 THEN
    RAISE_APPLICATION_ERROR(-20002, 'PKG_LG.CLAIM_SYSTEM_ERROR_EMAIL was not deployed.');
  END IF;

  SELECT COUNT(*)
    INTO v_count
    FROM USER_OBJECTS
   WHERE OBJECT_NAME = 'PKG_LG'
     AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
     AND STATUS = 'VALID';

  IF v_count <> 2 THEN
    RAISE_APPLICATION_ERROR(-20003, 'PKG_LG specification or body is invalid after deployment.');
  END IF;

  DBMS_OUTPUT.PUT_LINE('PKG_LG.CLAIM_SYSTEM_ERROR_EMAIL is deployed and valid.');
END;
/

EXIT SUCCESS
