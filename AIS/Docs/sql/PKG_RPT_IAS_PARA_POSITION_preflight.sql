-- READ ONLY. Run in the application schema with SQL*Plus/SQL Developer (F5).
-- This captures the actual production package, not the legacy standalone proc.
-- It deliberately performs NO CREATE/REPLACE and grants no access.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
SET LONG 10000000 LONGCHUNKSIZE 1000000 PAGESIZE 0 LINESIZE 32767
SET FEEDBACK OFF HEADING OFF TRIMSPOOL ON
SPOOL PKG_RPT.production.spec.sql
SELECT DBMS_METADATA.GET_DDL('PACKAGE','PKG_RPT',USER) FROM dual;
SPOOL OFF
SPOOL PKG_RPT.production.body.sql
SELECT DBMS_METADATA.GET_DDL('PACKAGE_BODY','PKG_RPT',USER) FROM dual;
SPOOL OFF
SET PAGESIZE 100 FEEDBACK ON HEADING ON
SELECT package_name,object_name,argument_name,position,data_type,in_out
FROM user_arguments
WHERE package_name='PKG_RPT' AND object_name='R_IAS_PARA_POSITION'
ORDER BY overload,sequence;
SELECT object_name,object_type,status FROM user_objects
WHERE object_name IN ('PKG_RPT','V_EXEC_KPI_LIFECYCLE','V_EXEC_ENTITY_SCOPE');
DECLARE
  n NUMBER;
BEGIN
  SELECT COUNT(*) INTO n FROM user_arguments
  WHERE package_name='PKG_RPT' AND object_name='R_IAS_PARA_POSITION'
    AND argument_name IN ('P_NO','ENT_ID','R_ID','R_T','S_DATE','E_DATE','IO_CURSOR');
  IF n<>7 THEN
    RAISE_APPLICATION_ERROR(-20160,'Production report signature missing, overloaded or different. Review captured source before any deployment.');
  END IF;
END;
/
