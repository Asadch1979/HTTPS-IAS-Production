-- CCO_SCOPE_RETIREMENT.sql
-- Oracle 19c: remove obsolete IAS CCO trusted-scope objects only AFTER the
-- application from PR #358 uses PKG_EXEC_REPORTS.P_MANAGEMENT_SETTLEMENT.
--
-- SQL*Plus / SQLcl / PL/SQL Developer COMMAND Window (not one F8 SQL Window)
-- DBA approval and complete DBMS_METADATA backups are mandatory.
-- DDL commits implicitly; no rollback is possible after a DROP.
-- This script neither changes IAS authorizations nor replaces report packages.
-- It does not read, modify or delete audit observations.
--
-- Set exact uppercase schema names discovered in DBA_OBJECTS:
DEFINE CCO_SCOPE_OWNER  = REPLACE_WITH_SCOPE_OWNER
DEFINE CCO_REPORT_OWNER = REPLACE_WITH_REPORT_OWNER
DEFINE APPLY_CCO_CLEANUP = NO

SET SERVEROUTPUT ON SIZE UNLIMITED
SET VERIFY OFF
SET FEEDBACK ON
WHENEVER SQLERROR EXIT SQL.SQLCODE

PROMPT 1. Check installed objects and trusted context
SELECT owner,object_name,object_type,status
  FROM dba_objects
 WHERE object_name IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
                       'PKG_CCO_REPORTING','V_CCO_AUTHORIZED_ENTITIES',
                       'T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT')
 ORDER BY owner,object_name,object_type;

SELECT namespace,schema,package,type
  FROM dba_context
 WHERE namespace='IAS_CCO_CTX';

PROMPT 2. Show all installed references to objects proposed for removal
SELECT owner,name,type,referenced_owner,referenced_name,referenced_type
  FROM dba_dependencies
 WHERE referenced_name IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
                           'PKG_CCO_REPORTING','V_CCO_AUTHORIZED_ENTITIES',
                           'T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT')
 ORDER BY owner,name,referenced_owner,referenced_name;

PROMPT 3. Show synonyms that may require separately approved cleanup
SELECT owner,synonym_name,table_owner,table_name
  FROM dba_synonyms
 WHERE table_name IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
                      'PKG_CCO_REPORTING','V_CCO_AUTHORIZED_ENTITIES',
                      'T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT')
 ORDER BY owner,synonym_name;

PROMPT 4. Guard against external dependencies and wrong schema ownership
DECLARE
  v_scope VARCHAR2(128) := UPPER(TRIM('&CCO_SCOPE_OWNER'));
  v_report VARCHAR2(128) := UPPER(TRIM('&CCO_REPORT_OWNER'));
  v_bad NUMBER;
  v_context NUMBER;
BEGIN
  IF v_scope LIKE 'REPLACE%' OR v_report LIKE 'REPLACE%'
    OR NOT REGEXP_LIKE(v_scope,'^[A-Z][A-Z0-9_$#]*$')
    OR NOT REGEXP_LIKE(v_report,'^[A-Z][A-Z0-9_$#]*$') THEN
    RAISE_APPLICATION_ERROR(-20920,'Enter actual verified Oracle object owners.');
  END IF;

  SELECT COUNT(*) INTO v_bad FROM dba_objects
   WHERE object_name IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
                         'PKG_CCO_REPORTING','V_CCO_AUTHORIZED_ENTITIES',
                         'T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT')
     AND NOT (
       (owner=v_scope AND object_name IN
        ('PKG_CCO_SCOPE','V_CCO_AUTHORIZED_ENTITIES',
         'T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT'))
       OR
       (owner=v_report AND object_name IN
        ('PKG_CCO_FUNCTION_POSITION','PKG_CCO_REPORTING'))
     );
  IF v_bad<>0 THEN
    RAISE_APPLICATION_ERROR(-20921,'Other owners have same-named objects. Review before removal.');
  END IF;

  SELECT COUNT(*) INTO v_bad
    FROM dba_dependencies d
   WHERE (
       (d.referenced_owner=v_scope AND d.referenced_name IN
        ('PKG_CCO_SCOPE','V_CCO_AUTHORIZED_ENTITIES',
         'T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT'))
       OR
       (d.referenced_owner=v_report AND d.referenced_name IN
        ('PKG_CCO_FUNCTION_POSITION','PKG_CCO_REPORTING'))
     )
     AND NOT (
       (d.owner=v_scope AND d.name IN ('PKG_CCO_SCOPE','V_CCO_AUTHORIZED_ENTITIES'))
       OR
       (d.owner=v_report AND d.name IN ('PKG_CCO_FUNCTION_POSITION','PKG_CCO_REPORTING'))
     );
  IF v_bad<>0 THEN
    RAISE_APPLICATION_ERROR(-20922,
      'Live Oracle objects still depend on retired CCO objects. STOP; inspect DBA_DEPENDENCIES.');
  END IF;

  SELECT COUNT(*) INTO v_bad FROM dba_source s
   WHERE (UPPER(s.text) LIKE '%PKG_CCO_SCOPE%'
      OR UPPER(s.text) LIKE '%PKG_CCO_FUNCTION_POSITION%'
      OR UPPER(s.text) LIKE '%PKG_CCO_REPORTING%'
      OR UPPER(s.text) LIKE '%V_CCO_AUTHORIZED_ENTITIES%'
      OR UPPER(s.text) LIKE '%T_CCO_SCOPE_ENTITIES_GTT%'
      OR UPPER(s.text) LIKE '%T_CCO_SCOPE_INIT_GTT%'
      OR UPPER(s.text) LIKE '%IAS_CCO_CTX%')
     AND NOT ((s.owner=v_scope AND s.name='PKG_CCO_SCOPE')
           OR (s.owner=v_report AND s.name IN
               ('PKG_CCO_FUNCTION_POSITION','PKG_CCO_REPORTING')));
  IF v_bad<>0 THEN
    RAISE_APPLICATION_ERROR(-20923,
      'Other database source references old CCO objects/context. STOP; inspect DBA_SOURCE.');
  END IF;

  SELECT COUNT(*) INTO v_bad FROM dba_synonyms s
    WHERE (s.table_owner=v_scope AND s.table_name IN
         ('PKG_CCO_SCOPE','V_CCO_AUTHORIZED_ENTITIES',
          'T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT'))
       OR (s.table_owner=v_report AND s.table_name IN
          ('PKG_CCO_FUNCTION_POSITION','PKG_CCO_REPORTING'));
  IF v_bad<>0 THEN
    RAISE_APPLICATION_ERROR(-20924,
      'Synonyms still point at retired objects. Review and remove separately by their owners.');
  END IF;

  SELECT COUNT(*) INTO v_context FROM dba_context
    WHERE namespace='IAS_CCO_CTX';
  IF v_context>0 THEN
    SELECT COUNT(*) INTO v_bad FROM dba_context
     WHERE namespace='IAS_CCO_CTX' AND schema=v_scope AND package='PKG_CCO_SCOPE';
    IF v_context<>1 OR v_bad<>1 THEN
      RAISE_APPLICATION_ERROR(-20925,'IAS_CCO_CTX is bound to a different trusted package.');
    END IF;
  END IF;
  SELECT COUNT(*) INTO v_bad FROM dba_context
   WHERE namespace<>'IAS_CCO_CTX' AND schema=v_scope AND package='PKG_CCO_SCOPE';
  IF v_bad<>0 THEN
    RAISE_APPLICATION_ERROR(-20926,'Other contexts depend on protected CCO package.');
  END IF;

  SELECT COUNT(*) INTO v_bad FROM dba_tables
   WHERE owner=v_scope AND table_name IN
     ('T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT')
     AND (temporary<>'Y' OR duration<>'SYS$TRANSACTION');
  IF v_bad<>0 THEN
    RAISE_APPLICATION_ERROR(-20927,'A similarly named table is not the expected transaction-private GTT.');
  END IF;

  DBMS_OUTPUT.PUT_LINE('Static dependency checks passed. Active sessions and dynamic calls are not provable here.');
  DBMS_OUTPUT.PUT_LINE('BEFORE APPLY=YES: verify no old app instances/jobs use these packages; back up DDL.');
END;
/

PROMPT 5. Default stop: do not execute DDL without approved deployment
BEGIN
  IF UPPER(TRIM('&APPLY_CCO_CLEANUP')) <> 'YES' THEN
    RAISE_APPLICATION_ERROR(-20928,
      'Read-only preflight complete. No objects dropped. Set APPLY_CCO_CLEANUP=YES ONLY after approved cutover.');
  END IF;
END;
/

PROMPT 6. Retire approved objects in dependency order (irreversible DDL)
DECLARE
  v_scope VARCHAR2(128) := DBMS_ASSERT.SIMPLE_SQL_NAME(UPPER(TRIM('&CCO_SCOPE_OWNER')));
  v_report VARCHAR2(128) := DBMS_ASSERT.SIMPLE_SQL_NAME(UPPER(TRIM('&CCO_REPORT_OWNER')));
  v_count NUMBER;
  PROCEDURE DROP_IF_EXISTS(p_owner VARCHAR2,p_name VARCHAR2,p_type VARCHAR2,p_ddl VARCHAR2) IS
  BEGIN
    SELECT COUNT(*) INTO v_count FROM dba_objects
     WHERE owner=p_owner AND object_name=p_name AND object_type=p_type;
    IF v_count=1 THEN
      EXECUTE IMMEDIATE p_ddl;
      DBMS_OUTPUT.PUT_LINE('REMOVED: '||p_owner||'.'||p_name||' ('||p_type||')');
    ELSE
      DBMS_OUTPUT.PUT_LINE('Already absent: '||p_owner||'.'||p_name);
    END IF;
  END;
BEGIN
  DROP_IF_EXISTS(v_report,'PKG_CCO_REPORTING','PACKAGE',
                 'DROP PACKAGE '||v_report||'.PKG_CCO_REPORTING');
  DROP_IF_EXISTS(v_report,'PKG_CCO_FUNCTION_POSITION','PACKAGE',
                 'DROP PACKAGE '||v_report||'.PKG_CCO_FUNCTION_POSITION');
  DROP_IF_EXISTS(v_scope,'V_CCO_AUTHORIZED_ENTITIES','VIEW',
                 'DROP VIEW '||v_scope||'.V_CCO_AUTHORIZED_ENTITIES');

  SELECT COUNT(*) INTO v_count FROM dba_context WHERE namespace='IAS_CCO_CTX';
  IF v_count=1 THEN
    EXECUTE IMMEDIATE 'DROP CONTEXT IAS_CCO_CTX';
    DBMS_OUTPUT.PUT_LINE('REMOVED: IAS_CCO_CTX');
  END IF;

  DROP_IF_EXISTS(v_scope,'PKG_CCO_SCOPE','PACKAGE',
                 'DROP PACKAGE '||v_scope||'.PKG_CCO_SCOPE');
  DROP_IF_EXISTS(v_scope,'T_CCO_SCOPE_INIT_GTT','TABLE',
                 'DROP TABLE '||v_scope||'.T_CCO_SCOPE_INIT_GTT');
  DROP_IF_EXISTS(v_scope,'T_CCO_SCOPE_ENTITIES_GTT','TABLE',
                 'DROP TABLE '||v_scope||'.T_CCO_SCOPE_ENTITIES_GTT');
END;
/

PROMPT 7. Final verification: there should be zero rows
SELECT owner,object_name,object_type,status
  FROM dba_objects
 WHERE object_name IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
                       'PKG_CCO_REPORTING','V_CCO_AUTHORIZED_ENTITIES',
                       'T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT')
 ORDER BY owner,object_name,object_type;
SELECT namespace,schema,package
  FROM dba_context
 WHERE namespace='IAS_CCO_CTX';

PROMPT Verify PKG_EXEC_REPORTS remains valid and the updated CCO API works
SELECT owner,object_name,object_type,status
  FROM dba_objects
 WHERE object_name='PKG_EXEC_REPORTS'
 ORDER BY owner,object_type;

PROMPT End: restart/recycle IIS to clear stale assemblies AFTER successful application deployment
