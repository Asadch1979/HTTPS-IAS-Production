-- ========================================================================
-- CCO_FINAL_FIXATION.sql
-- IAS CCO Function Wise Position | single-file guarded Oracle 19c deployment
-- Case: 2026-10-31, HTTP 503, cco_database_unavailable
-- API: /ZTBLAIS/api/CcoCompliance/GetFunctionPosition
-- Production example: 2026-01-01 to 2026-10-30, maxRows=1000
-- ========================================================================
-- IMPORTANT: PKG_CCO_REPORTING IS NOT CALLED by GetFunctionPosition.
-- This script installs/validates only its actual scope + reporting dependencies.
-- No IAS user impersonation, manufactured scope token, wildcard role grants,
-- audit data correction, dashboard permission bypass, or unscoped fallback.
--
-- RUNNING:
-- 1) Export/back up live package specifications, package bodies, trusted view,
--    context ownership and existing grants before proceeding.
-- 2) Ask DBA to verify the *dedicated protected owner* of PKG_CCO_SCOPE,
--    IAS_CCO_CTX owner/package, required direct grants and GTT restrictions.
-- 3) Edit the 3 DEFINE values below (case-insensitive, names only; no passwords).
-- 4) Run the WHOLE script with PL/SQL Developer COMMAND Window (F5) or SQL*Plus/
--    SQLcl in the protected CCO owner account. NOT as a single SQL Window F8.
-- 5) The default APPLY_FIX = NO deliberately exits BEFORE DDL. Review read-only
--    diagnostics first; change to YES ONLY after authorized DBA approval.
-- 6) If preflight reports duplicate COM_ID, wrong owner, absent privileges,
--    incorrect GTT form or invalid objects, STOP: don't force the package.
-- 7) After deployment, log in as approved CCO, retry original URL and inspect
--    IIS logs for the new correlation + Oracle stage if error remains.
--
-- CRITICAL MULTI-OWNER CHECK: When the runtime/reporting schema owns
-- PKG_CCO_FUNCTION_POSITION, DO NOT execute a replacement in the security
-- owner. The C# unqualified call would still resolve its own original package.
-- A separate approved owner-specific deployment is then mandatory.
-- The script cannot prove that your live production Oracle objects match GitHub.
-- It cannot guarantee removal of a 503 without the actual logged Oracle error.
-- It installs the verified repository CCO scope transaction correction and
-- function position package, subject to the safety checks below.
-- ========================================================================

DEFINE CCO_SECURITY_OWNER = REPLACE_WITH_PROTECTED_CCO_OWNER
DEFINE CCO_RUNTIME_USER   = REPLACE_WITH_IAS_RUNTIME_DB_USER
DEFINE APPLY_FIX          = NO

SET SERVEROUTPUT ON SIZE UNLIMITED
SET VERIFY OFF
SET FEEDBACK ON
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK

PROMPT === CCO_FINAL_FIXATION: STEP 0 / VERIFY OWNER AND APPROVAL ===

DECLARE
  v_owner VARCHAR2(128) := UPPER(TRIM('&CCO_SECURITY_OWNER'));
  v_runtime VARCHAR2(128) := UPPER(TRIM('&CCO_RUNTIME_USER'));
BEGIN
  IF v_owner LIKE 'REPLACE%' OR v_runtime LIKE 'REPLACE%'
     OR v_owner IS NULL OR v_runtime IS NULL THEN
     RAISE_APPLICATION_ERROR(-20900,'Set the actual protected CCO owner and IAS runtime username first.');
  END IF;
  IF NOT REGEXP_LIKE(v_owner,'^[A-Z][A-Z0-9_$#]*$')
     OR NOT REGEXP_LIKE(v_runtime,'^[A-Z][A-Z0-9_$#]*$') THEN
     RAISE_APPLICATION_ERROR(-20901,'Only actual simple Oracle schema usernames are accepted.');
  END IF;
  IF USER <> v_owner THEN
     RAISE_APPLICATION_ERROR(-20902,'Connect directly as the approved CCO protected security owner.');
  END IF;
  IF v_owner=v_runtime THEN
     RAISE_APPLICATION_ERROR(-20903,'CCO security owner must not be the IAS application runtime owner.');
  END IF;
  -- Never create a shadow function package when the runtime already resolves
  -- its own copy. This requires an owner-specific DB deployment plan.
  DECLARE v_local_runtime_package NUMBER;
  BEGIN
    SELECT COUNT(*) INTO v_local_runtime_package
      FROM ALL_OBJECTS
     WHERE OWNER=v_runtime
       AND OBJECT_NAME='PKG_CCO_FUNCTION_POSITION'
       AND OBJECT_TYPE='PACKAGE';
    IF v_local_runtime_package>0 THEN
      RAISE_APPLICATION_ERROR(-20912,
        'The runtime account already owns PKG_CCO_FUNCTION_POSITION. This script must not install a shadow package in the security schema. Coordinate source comparison and deployment with its actual owner.');
    END IF;
  END;
  DBMS_OUTPUT.PUT_LINE('Protected schema: '||USER||' | Runtime schema: '||v_runtime);
END;
/

PROMPT === STEP 1 / READ-ONLY OBJECT INVENTORY ===
SELECT OWNER, OBJECT_NAME, OBJECT_TYPE, STATUS
  FROM ALL_OBJECTS
 WHERE OBJECT_NAME IN
  ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
   'PKG_CCO_REPORTING','V_CCO_AUTHORIZED_ENTITIES',
   'T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT',
   'V_EXEC_REPORT_SOURCE','V_EXEC_ENTITY_SCOPE',
   'V_IAS_ENTITY_REPORTING_IDS_GROUP_REPORTS')
 ORDER BY OBJECT_NAME,OWNER,OBJECT_TYPE;

SELECT NAME, TYPE, LINE, POSITION, TEXT
  FROM USER_ERRORS
 WHERE NAME IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION',
                'V_CCO_AUTHORIZED_ENTITIES')
 ORDER BY NAME, TYPE, SEQUENCE;

PROMPT === STEP 2 / DATA CHECK: EXACT FAIL-CLOSED REPORT COHORT ===
PROMPT Any duplicate COM_ID in this cohort causes ORA-20803.
SELECT COM_ID, COUNT(*) AS SOURCE_ROWS
  FROM V_EXEC_REPORT_SOURCE
 WHERE AUDIT_TYPE = 'D'
   AND EFFECTIVE_ADDED_ON < DATE '2026-10-31'
 GROUP BY COM_ID
HAVING COUNT(*) <> 1 OR COM_ID IS NULL
 ORDER BY SOURCE_ROWS DESC
 FETCH FIRST 50 ROWS ONLY;

DECLARE
  v_bad NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_bad
    FROM (
     SELECT COM_ID FROM V_EXEC_REPORT_SOURCE
      WHERE AUDIT_TYPE = 'D'
        AND EFFECTIVE_ADDED_ON < DATE '2026-10-31'
      GROUP BY COM_ID
     HAVING COUNT(*) <> 1 OR COM_ID IS NULL
    );
  IF v_bad > 0 THEN
    RAISE_APPLICATION_ERROR(-20904,
      'STOP: Duplicate/null COM_ID source groups='||v_bad||
      '. Fix V_EXEC_REPORT_SOURCE fact duplication at source; never silently deduplicate amounts.');
  END IF;
  DBMS_OUTPUT.PUT_LINE('COM_ID uniqueness for 01-Jan through 30-Oct-2026: PASS');
END;
/

PROMPT === STEP 3 / ENFORCE READ-ONLY PREFLIGHT UNTIL DBA APPROVES ===
BEGIN
  IF UPPER(TRIM('&APPLY_FIX')) <> 'YES' THEN
    RAISE_APPLICATION_ERROR(-20905,
       'DIAGNOSTICS COMPLETE. DDL NOT APPLIED. After DBA owner/context/grant checks, explicitly set APPLY_FIX=YES.');
  END IF;
  DBMS_OUTPUT.PUT_LINE('Manual deployment authorization confirmed.');
END;
/

PROMPT === STEP 4 / CHECK GTT STRUCTURES AND CREATE ONLY WHEN MISSING ===
DECLARE
  v_count NUMBER;
  v_ok NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM USER_TABLES
   WHERE TABLE_NAME='T_CCO_SCOPE_ENTITIES_GTT';
  IF v_count=0 THEN
    EXECUTE IMMEDIATE q'~CREATE GLOBAL TEMPORARY TABLE T_CCO_SCOPE_ENTITIES_GTT
     (SCOPE_TOKEN VARCHAR2(32) NOT NULL,ENTITY_ID NUMBER NOT NULL,
      CONSTRAINT PK_T_CCO_SCOPE_ENTITIES_GTT PRIMARY KEY(SCOPE_TOKEN,ENTITY_ID))
      ON COMMIT DELETE ROWS~';
    DBMS_OUTPUT.PUT_LINE('Created protected scope entity GTT.');
  END IF;
  SELECT COUNT(*) INTO v_ok FROM USER_TABLES
   WHERE TABLE_NAME='T_CCO_SCOPE_ENTITIES_GTT'
     AND TEMPORARY='Y' AND DURATION='SYS$TRANSACTION';
  IF v_ok<>1 THEN RAISE_APPLICATION_ERROR(-20906,'Scope entity GTT is not ON COMMIT DELETE ROWS'); END IF;

  SELECT COUNT(*) INTO v_count FROM USER_TABLES
   WHERE TABLE_NAME='T_CCO_SCOPE_INIT_GTT';
  IF v_count=0 THEN
    EXECUTE IMMEDIATE q'~CREATE GLOBAL TEMPORARY TABLE T_CCO_SCOPE_INIT_GTT
     (SCOPE_TOKEN VARCHAR2(32) NOT NULL PRIMARY KEY)
      ON COMMIT DELETE ROWS~';
    DBMS_OUTPUT.PUT_LINE('Created transaction marker GTT.');
  END IF;
  SELECT COUNT(*) INTO v_ok FROM USER_TABLES
   WHERE TABLE_NAME='T_CCO_SCOPE_INIT_GTT'
     AND TEMPORARY='Y' AND DURATION='SYS$TRANSACTION';
  IF v_ok<>1 THEN RAISE_APPLICATION_ERROR(-20907,'Scope marker GTT is not ON COMMIT DELETE ROWS'); END IF;

  SELECT COUNT(*) INTO v_ok FROM USER_TAB_COLUMNS WHERE TABLE_NAME='T_CCO_SCOPE_ENTITIES_GTT'
     AND ((COLUMN_NAME='SCOPE_TOKEN' AND DATA_TYPE='VARCHAR2' AND DATA_LENGTH=32)
       OR (COLUMN_NAME='ENTITY_ID' AND DATA_TYPE='NUMBER'));
  IF v_ok<>2 THEN RAISE_APPLICATION_ERROR(-20908,'Scope entity GTT column contract differs'); END IF;
  SELECT COUNT(*) INTO v_ok FROM USER_TAB_COLUMNS WHERE TABLE_NAME='T_CCO_SCOPE_INIT_GTT'
     AND COLUMN_NAME='SCOPE_TOKEN' AND DATA_TYPE='VARCHAR2' AND DATA_LENGTH=32;
  IF v_ok<>1 THEN RAISE_APPLICATION_ERROR(-20909,'Scope marker GTT column contract differs'); END IF;
END;
/

PROMPT === STEP 5 / TRUSTED PKG_CCO_SCOPE SPECIFICATION ===
PROMPT DBA prerequisite: IAS_CCO_CTX must already be defined USING <OWNER>.PKG_CCO_SCOPE.
PROMPT If it is missing or points elsewhere STOP and have DBA correct context ownership.

CREATE OR REPLACE PACKAGE PKG_CCO_SCOPE AUTHID DEFINER AS
    PROCEDURE P_BEGIN_SCOPE(
        P_USER_ID      IN NUMBER,
        P_ROLE_ID      IN NUMBER,
        P_GROUP_ID     IN NUMBER,
        P_ENTITY_ID    IN NUMBER,
        P_CONTEXT_ID   IN NUMBER,
        P_SESSION_ID   IN VARCHAR2,
        O_SCOPE_TOKEN  OUT VARCHAR2);
    PROCEDURE P_ASSERT_SCOPE(P_SCOPE_TOKEN IN VARCHAR2);
    PROCEDURE P_END_SCOPE;
END PKG_CCO_SCOPE;
/

PROMPT === STEP 6 / CORRECTED SCOPE BODY WITH TRANSACTION MARKER ===
CREATE OR REPLACE PACKAGE BODY PKG_CCO_SCOPE AS

    PROCEDURE P_END_SCOPE IS
    BEGIN
        DBMS_SESSION.CLEAR_CONTEXT('IAS_CCO_CTX', NULL);
        DELETE FROM T_CCO_SCOPE_INIT_GTT;
        DELETE FROM T_CCO_SCOPE_ENTITIES_GTT;
        DBMS_SESSION.CLEAR_CONTEXT('IAS_CCO_CTX', NULL);
    END P_END_SCOPE;

    PROCEDURE P_REQUIRE_ACTIVE(
        P_USER_ID IN NUMBER, P_ROLE_ID IN NUMBER, P_GROUP_ID IN NUMBER,
        P_ENTITY_ID IN NUMBER, P_CONTEXT_ID IN NUMBER, P_SESSION_ID IN VARCHAR2) IS
        V_COUNT NUMBER;
    BEGIN
        IF P_USER_ID IS NULL OR P_ROLE_ID IS NULL OR P_ROLE_ID NOT IN (1,38)
           OR P_GROUP_ID IS NULL OR P_ENTITY_ID IS NULL
           OR P_CONTEXT_ID IS NULL OR P_SESSION_ID IS NULL THEN
            RAISE_APPLICATION_ERROR(-20810, 'Missing CCO IAS context');
        END IF;
        SELECT COUNT(DISTINCT U.USERID) INTO V_COUNT
          FROM T_USER U
          JOIN T_USER_SESSION S ON S.USER_PP_NUMBER = U.PPNO
          JOIN T_GROUPS G ON G.GROUP_ID = P_GROUP_ID
          JOIN T_USER_CONTEXT_ASSIGNMENT C ON C.USER_CONTEXT_ID = P_CONTEXT_ID
          JOIN T_MENU_PAGES MP ON MP.ID = 1000302
          JOIN T_MENU_PAGES_GROUPMAP MPG ON MPG.PAGE_ID = MP.ID
         WHERE U.USERID = P_USER_ID AND U.ISACTIVE = 'Y'
           AND S.SESSION_ID = P_SESSION_ID AND S.SESSION_ACTIVE = 'Y'
           AND S.LOGGED_OUT_DATE IS NULL
           AND S.ROLE_ID = P_ROLE_ID AND S.ENTITY_ID = P_ENTITY_ID
           AND S.GROUP_ID = P_GROUP_ID
           AND G.ROLE_ID = P_ROLE_ID AND G.STATUS = 'Y'
           AND C.USER_ID = U.USERID AND C.ROLE_ID = P_ROLE_ID
           AND C.ENTITY_ID = P_ENTITY_ID AND C.IS_ACTIVE = 'Y'
           AND NVL(C.GROUP_ID,C.ROLE_ID) = P_GROUP_ID
           AND (C.EFFECTIVE_FROM IS NULL OR C.EFFECTIVE_FROM <= SYSDATE)
           AND (C.EFFECTIVE_TO IS NULL OR C.EFFECTIVE_TO >= TRUNC(SYSDATE))
           AND MP.STATUS = 'A' AND MPG.GROUP_ID = P_GROUP_ID;
        IF V_COUNT <> 1 THEN
            RAISE_APPLICATION_ERROR(-20811, 'CCO session/context/page is not currently authorized');
        END IF;
    END P_REQUIRE_ACTIVE;

    PROCEDURE P_BEGIN_SCOPE(
        P_USER_ID IN NUMBER, P_ROLE_ID IN NUMBER, P_GROUP_ID IN NUMBER,
        P_ENTITY_ID IN NUMBER, P_CONTEXT_ID IN NUMBER, P_SESSION_ID IN VARCHAR2,
        O_SCOPE_TOKEN OUT VARCHAR2) IS
    BEGIN
        -- Clear residue before validating, including reused pooled connections.
        P_END_SCOPE;
        P_REQUIRE_ACTIVE(P_USER_ID,P_ROLE_ID,P_GROUP_ID,P_ENTITY_ID,P_CONTEXT_ID,P_SESSION_ID);
        O_SCOPE_TOKEN := RAWTOHEX(SYS_GUID());
        INSERT INTO T_CCO_SCOPE_INIT_GTT(SCOPE_TOKEN) VALUES (O_SCOPE_TOKEN);
        DBMS_SESSION.SET_CONTEXT('IAS_CCO_CTX','SCOPE_TOKEN', O_SCOPE_TOKEN);
        DBMS_SESSION.SET_CONTEXT('IAS_CCO_CTX','USER_ID', TO_CHAR(P_USER_ID));
        DBMS_SESSION.SET_CONTEXT('IAS_CCO_CTX','ROLE_ID', TO_CHAR(P_ROLE_ID));
        DBMS_SESSION.SET_CONTEXT('IAS_CCO_CTX','GROUP_ID', TO_CHAR(P_GROUP_ID));
        DBMS_SESSION.SET_CONTEXT('IAS_CCO_CTX','ENTITY_ID', TO_CHAR(P_ENTITY_ID));
        DBMS_SESSION.SET_CONTEXT('IAS_CCO_CTX','CONTEXT_ID', TO_CHAR(P_CONTEXT_ID));
        DBMS_SESSION.SET_CONTEXT('IAS_CCO_CTX','SESSION_ID', P_SESSION_ID);
        -- CCO bank-wide source population. Unlike a hierarchy join, this
        -- retains otherwise unmapped auditee entities for explicit exceptions.
        INSERT INTO T_CCO_SCOPE_ENTITIES_GTT(SCOPE_TOKEN,ENTITY_ID)
        SELECT O_SCOPE_TOKEN, E.ENTITY_ID
          FROM T_AUDITEE_ENTITIES E
         WHERE E.ENTITY_ID IS NOT NULL
         GROUP BY E.ENTITY_ID;
        P_ASSERT_SCOPE(O_SCOPE_TOKEN);
    EXCEPTION WHEN OTHERS THEN
        P_END_SCOPE;
        RAISE;
    END P_BEGIN_SCOPE;

    PROCEDURE P_ASSERT_SCOPE(P_SCOPE_TOKEN IN VARCHAR2) IS
        V_MARKERS NUMBER;
        V_USER_ID NUMBER;
        V_ROLE_ID NUMBER;
        V_GROUP_ID NUMBER;
        V_ENTITY_ID NUMBER;
        V_CONTEXT_ID NUMBER;
        V_SESSION_ID VARCHAR2(200);
    BEGIN
        IF SYS_CONTEXT('IAS_CCO_CTX','SCOPE_TOKEN') IS NULL OR P_SCOPE_TOKEN IS NULL OR LENGTH(P_SCOPE_TOKEN) <> 32
           OR NOT REGEXP_LIKE(P_SCOPE_TOKEN, '^[0-9A-F]{32}$')
           OR P_SCOPE_TOKEN <> SYS_CONTEXT('IAS_CCO_CTX','SCOPE_TOKEN') THEN
            RAISE_APPLICATION_ERROR(-20812, 'CCO scope-token mismatch');
        END IF;
        SELECT COUNT(*) INTO V_MARKERS FROM T_CCO_SCOPE_INIT_GTT WHERE SCOPE_TOKEN = P_SCOPE_TOKEN;
        IF V_MARKERS <> 1 THEN RAISE_APPLICATION_ERROR(-20813, 'CCO transaction scope expired'); END IF;
        V_USER_ID := TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','USER_ID'));
        V_ROLE_ID := TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','ROLE_ID'));
        V_GROUP_ID := TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','GROUP_ID'));
        V_ENTITY_ID := TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','ENTITY_ID'));
        V_CONTEXT_ID := TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','CONTEXT_ID'));
        V_SESSION_ID := SYS_CONTEXT('IAS_CCO_CTX','SESSION_ID');
        P_REQUIRE_ACTIVE(V_USER_ID,V_ROLE_ID,V_GROUP_ID,V_ENTITY_ID,V_CONTEXT_ID,V_SESSION_ID);
        -- Must be inside one active transaction; COMMIT or ROLLBACK empties GTT.
        -- If the organization is legitimately empty, the application must
        -- distinguish that from a lost transaction before reading aggregates.
    END P_ASSERT_SCOPE;
END PKG_CCO_SCOPE;
/

PROMPT === STEP 7 / VERIFIED SCOPE VIEW ===
CREATE OR REPLACE VIEW V_CCO_AUTHORIZED_ENTITIES AS
SELECT X.SCOPE_TOKEN, X.ENTITY_ID
  FROM T_CCO_SCOPE_ENTITIES_GTT X
 WHERE X.SCOPE_TOKEN = SYS_CONTEXT('IAS_CCO_CTX','SCOPE_TOKEN')
   AND EXISTS (SELECT 1 FROM T_CCO_SCOPE_INIT_GTT I WHERE I.SCOPE_TOKEN = X.SCOPE_TOKEN)
   AND EXISTS
   (
       SELECT 1 FROM T_USER U
       JOIN T_USER_SESSION S ON S.USER_PP_NUMBER = U.PPNO
       JOIN T_GROUPS G ON G.GROUP_ID = S.GROUP_ID
       JOIN T_USER_CONTEXT_ASSIGNMENT C ON C.USER_CONTEXT_ID =
            TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','CONTEXT_ID'))
       JOIN T_MENU_PAGES MP ON MP.ID = 1000302
       JOIN T_MENU_PAGES_GROUPMAP MPG ON MPG.PAGE_ID = MP.ID
      WHERE U.USERID = TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','USER_ID'))
        AND U.ISACTIVE = 'Y'
        AND S.SESSION_ID = SYS_CONTEXT('IAS_CCO_CTX','SESSION_ID')
        AND S.SESSION_ACTIVE = 'Y' AND S.LOGGED_OUT_DATE IS NULL
        AND S.ROLE_ID = TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','ROLE_ID'))
        AND S.ENTITY_ID = TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','ENTITY_ID'))
        AND S.GROUP_ID = TO_NUMBER(SYS_CONTEXT('IAS_CCO_CTX','GROUP_ID'))
        AND S.ROLE_ID IN (1,38) AND G.ROLE_ID = S.ROLE_ID AND G.STATUS='Y'
        AND C.USER_ID = U.USERID AND C.ROLE_ID = S.ROLE_ID
        AND C.ENTITY_ID = S.ENTITY_ID AND C.IS_ACTIVE = 'Y'
        AND NVL(C.GROUP_ID,C.ROLE_ID)=S.GROUP_ID
        AND (C.EFFECTIVE_FROM IS NULL OR C.EFFECTIVE_FROM<=SYSDATE)
        AND (C.EFFECTIVE_TO IS NULL OR C.EFFECTIVE_TO>=TRUNC(SYSDATE))
        AND MP.STATUS='A' AND MPG.GROUP_ID=S.GROUP_ID
   );
/

PROMPT === STEP 8 / FUNCTION WISE POSITION PACKAGE ===
CREATE OR REPLACE PACKAGE PKG_CCO_FUNCTION_POSITION AUTHID DEFINER AS
 PROCEDURE P_GET_POSITION(P_SCOPE_TOKEN VARCHAR2,P_START_DATE DATE,P_END_DATE DATE,P_MAX_ROWS NUMBER,IO_CURSOR OUT SYS_REFCURSOR);
END;
/
CREATE OR REPLACE PACKAGE BODY PKG_CCO_FUNCTION_POSITION AS
 PROCEDURE P_GET_POSITION(P_SCOPE_TOKEN VARCHAR2,P_START_DATE DATE,P_END_DATE DATE,P_MAX_ROWS NUMBER,IO_CURSOR OUT SYS_REFCURSOR) IS
 n NUMBER;
 BEGIN
 PKG_CCO_SCOPE.P_ASSERT_SCOPE(P_SCOPE_TOKEN);
 IF P_START_DATE IS NULL OR P_END_DATE IS NULL OR P_START_DATE<>TRUNC(P_START_DATE) OR P_END_DATE<>TRUNC(P_END_DATE)
 OR P_START_DATE>P_END_DATE OR P_END_DATE>=DATE '9999-12-31'
 OR P_END_DATE>TRUNC(CAST(SYSTIMESTAMP AT TIME ZONE 'Asia/Karachi' AS DATE))
 OR MONTHS_BETWEEN(P_END_DATE,P_START_DATE)>=1200
 OR P_MAX_ROWS IS NULL OR P_MAX_ROWS<1 OR P_MAX_ROWS>1000 OR P_MAX_ROWS<>TRUNC(P_MAX_ROWS) THEN
 RAISE_APPLICATION_ERROR(-20801,'Invalid function position request'); END IF;
 -- Reject duplicate facts rather than pick arbitrary amounts or silently double-count.
 SELECT COUNT(*) INTO n FROM (SELECT s.com_id FROM V_EXEC_REPORT_SOURCE s
 WHERE s.audit_type='D' AND s.effective_added_on<P_END_DATE+1
 AND EXISTS(SELECT 1 FROM V_CCO_AUTHORIZED_ENTITIES a WHERE a.scope_token=P_SCOPE_TOKEN AND a.entity_id=s.entity_id)
 GROUP BY s.com_id HAVING COUNT(*)<>1 OR s.com_id IS NULL);
 IF n>0 THEN RAISE_APPLICATION_ERROR(-20803,'Duplicate authorized facts'); END IF;
 OPEN IO_CURSOR FOR
 WITH mapping_candidates AS (
 SELECT DISTINCT r.parent_id group_id,r.p_name group_name,
                 r.entity_id division_id,r.c_name division_name
 FROM V_IAS_ENTITY_REPORTING_IDS_Group_reports r
 WHERE EXISTS (SELECT 1 FROM T_AUDITEE_ENTITIES_MAPING m
               WHERE m.entity_id=r.entity_id AND m.parent_id=r.parent_id
                 AND m.c_type_id=3 AND NVL(m.status,'Y')='Y'
                 AND m.p_name=r.p_name AND m.c_name=r.c_name)
 ), mappings AS (
 SELECT division_id,COUNT(*) candidates,MAX(group_id) group_id,MAX(group_name) group_name,MAX(division_name) division_name
 FROM mapping_candidates GROUP BY division_id
 ), hierarchy AS (
 SELECT entity_id,COUNT(*) paths,MAX(business_group_id) group_id,MAX(division_id) division_id
 FROM V_EXEC_ENTITY_SCOPE GROUP BY entity_id
 ), facts AS (
 SELECT s.com_id,
 CASE WHEN h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id AND h.division_id=m.division_id THEN m.group_id END group_id,
 CASE WHEN h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id AND h.division_id=m.division_id THEN m.group_name ELSE 'Unmapped / conflicting Group' END group_name,
 CASE WHEN h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id AND h.division_id=m.division_id THEN m.division_id END division_id,
 CASE WHEN h.paths=1 AND m.candidates=1 AND h.group_id=m.group_id AND h.division_id=m.division_id THEN m.division_name ELSE 'Unmapped / conflicting Division' END division_name,
 CASE WHEN s.effective_added_on<P_START_DATE AND (s.close_on IS NULL OR s.close_on>=P_START_DATE) THEN 1 ELSE 0 END opening_balance,
 CASE WHEN s.effective_added_on>=P_START_DATE AND s.effective_added_on<P_END_DATE+1 THEN 1 ELSE 0 END paras_added,
 CASE WHEN s.close_on>=P_START_DATE AND s.close_on<P_END_DATE+1 AND s.effective_added_on<P_END_DATE+1 THEN 1 ELSE 0 END settled_paras,
 CASE WHEN s.effective_added_on<P_END_DATE+1 AND (s.close_on IS NULL OR s.close_on>=P_END_DATE+1) THEN 1 ELSE 0 END outstanding_paras
 FROM V_EXEC_REPORT_SOURCE s LEFT JOIN hierarchy h ON h.entity_id=s.entity_id
 LEFT JOIN mappings m ON m.division_id=h.division_id
 WHERE s.audit_type='D' AND s.effective_added_on<P_END_DATE+1
 AND EXISTS(SELECT 1 FROM V_CCO_AUTHORIZED_ENTITIES a WHERE a.scope_token=P_SCOPE_TOKEN AND a.entity_id=s.entity_id)
 ), aggregates AS (
 SELECT CASE WHEN GROUPING(group_id)=1 THEN 'TOTAL' WHEN GROUPING(division_id)=1 THEN 'SUBTOTAL' ELSE 'DETAIL' END row_kind,
 group_id,CASE WHEN GROUPING(group_id)=1 THEN 'Grand total' ELSE group_name END group_name,
 division_id,CASE WHEN GROUPING(group_id)=1 THEN 'All authorized functions' WHEN GROUPING(division_id)=1 THEN 'Group subtotal' ELSE division_name END division_name,
 NVL(SUM(opening_balance),0) opening_balance,NVL(SUM(paras_added),0) paras_added,
 NVL(SUM(opening_balance+paras_added),0) total_paras,NVL(SUM(settled_paras),0) settled_paras,
 NVL(SUM(outstanding_paras),0) outstanding_paras,
 ROUND(100*SUM(settled_paras)/NULLIF(SUM(opening_balance+paras_added),0),2) settlement_percentage
 FROM facts GROUP BY GROUPING SETS((group_id,group_name,division_id,division_name),(group_id,group_name),())
 ), meaningful AS (
 SELECT a.* FROM aggregates a WHERE opening_balance<>0 OR paras_added<>0 OR settled_paras<>0 OR outstanding_paras<>0
 ), counted AS (SELECT a.*,COUNT(*) OVER() source_row_count FROM meaningful a)
 SELECT * FROM counted ORDER BY CASE WHEN row_kind='TOTAL' THEN 1 ELSE 0 END,group_name,group_id,
 CASE WHEN row_kind='SUBTOTAL' THEN 1 ELSE 0 END,division_name,division_id
 FETCH FIRST P_MAX_ROWS ROWS ONLY;
 -- Adapter rejects SOURCE_ROW_COUNT overflow, consumes all rows, asserts again.
 PKG_CCO_SCOPE.P_ASSERT_SCOPE(P_SCOPE_TOKEN);
 END;
END;
/

PROMPT === STEP 9 / GRANT ONLY THE THREE REQUIRED OBJECT PRIVILEGES ===
DECLARE
  v_runtime VARCHAR2(128) := DBMS_ASSERT.SIMPLE_SQL_NAME(UPPER(TRIM('&CCO_RUNTIME_USER')));
  v_bad NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_bad FROM USER_TAB_PRIVS_MADE
   WHERE TABLE_NAME IN ('T_CCO_SCOPE_ENTITIES_GTT','T_CCO_SCOPE_INIT_GTT')
     AND GRANTEE IN (v_runtime,'PUBLIC')
     AND PRIVILEGE IN ('INSERT','UPDATE','DELETE','ALTER','SELECT');
  IF v_bad > 0 THEN
    RAISE_APPLICATION_ERROR(-20910,'Unauthorized direct runtime/PUBLIC scope-GTT privileges detected. Revoke through approved DBA process.');
  END IF;
  EXECUTE IMMEDIATE 'GRANT EXECUTE ON PKG_CCO_SCOPE TO '||v_runtime;
  EXECUTE IMMEDIATE 'GRANT EXECUTE ON PKG_CCO_FUNCTION_POSITION TO '||v_runtime;
  EXECUTE IMMEDIATE 'GRANT SELECT ON V_CCO_AUTHORIZED_ENTITIES TO '||v_runtime;
  DBMS_OUTPUT.PUT_LINE('Narrow grants verified for '||v_runtime||'. No PUBLIC grants.');
END;
/

PROMPT === STEP 10 / VERIFY COMPILED OBJECTS AND ANY ERRORS ===
SELECT OBJECT_NAME,OBJECT_TYPE,STATUS,LAST_DDL_TIME
  FROM USER_OBJECTS
 WHERE OBJECT_NAME IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION','V_CCO_AUTHORIZED_ENTITIES')
 ORDER BY OBJECT_NAME,OBJECT_TYPE;

SELECT NAME,TYPE,LINE,POSITION,TEXT
  FROM USER_ERRORS
 WHERE NAME IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION','V_CCO_AUTHORIZED_ENTITIES')
 ORDER BY NAME,SEQUENCE;

DECLARE
  v_bad NUMBER;
  v_missing NUMBER;
BEGIN
  SELECT COUNT(*) INTO v_bad FROM USER_OBJECTS
   WHERE OBJECT_NAME IN ('PKG_CCO_SCOPE','PKG_CCO_FUNCTION_POSITION','V_CCO_AUTHORIZED_ENTITIES')
     AND STATUS <> 'VALID';
  SELECT 5-COUNT(*) INTO v_missing FROM USER_OBJECTS
   WHERE (OBJECT_NAME='PKG_CCO_SCOPE' AND OBJECT_TYPE IN ('PACKAGE','PACKAGE BODY'))
      OR (OBJECT_NAME='PKG_CCO_FUNCTION_POSITION' AND OBJECT_TYPE IN ('PACKAGE','PACKAGE BODY'))
      OR (OBJECT_NAME='V_CCO_AUTHORIZED_ENTITIES' AND OBJECT_TYPE='VIEW');
  IF v_bad>0 OR v_missing<>0 THEN
    RAISE_APPLICATION_ERROR(-20911,'CCO objects INVALID or missing; review USER_ERRORS before API retest.');
  END IF;
  DBMS_OUTPUT.PUT_LINE('Five required compiled objects VALID. This alone does NOT prove live trusted-session validity.');
END;
/

PROMPT === STEP 11 / MANDATORY PRODUCTION RETEST ===
PROMPT 1) DBA: SELECT NAMESPACE,SCHEMA,PACKAGE FROM DBA_CONTEXT WHERE NAMESPACE='IAS_CCO_CTX';
PROMPT 2) Confirm IAS_CCO_CTX is USING the protected owner PKG_CCO_SCOPE.
PROMPT 3) Test CCO role 38 / valid context / page 1000302 / exact GET API permission.
PROMPT 4) Test revoked/other-role denied; ensure unscoped view returns no rows.
PROMPT 5) Retry URL with startDate=2026-01-01 endDate=2026-10-30 maxRows=1000.
PROMPT 6) On any 503, retrieve server log: Stage + OracleNumber + correlationId.
PROMPT 7) If ORA-20811: inspect live IAS session, context and mapping, DO NOT bypass.
PROMPT 8) If ORA-20813: inspect GTT marker and transaction, DO NOT use COMMIT before P_END.
PROMPT 9) If ORA-20803: investigate duplicate COM_ID / source rows, DO NOT remove validation.
PROMPT 10) If read_and_validate_function_rows: check SOURCE_ROW_COUNT and cursor aliases.
PROMPT 11) Reconcile authorized Group/Division totals with Report E.
PROMPT === CCO_FINAL_FIXATION completed without deliberate IAS user-data modification ===

WHENEVER SQLERROR CONTINUE
