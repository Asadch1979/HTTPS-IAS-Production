/*
  Verification only: central PKG_LG entity selectors for Manage Users.
  Run in SQL Developer (Run Script/F5) or SQL*Plus after the DBA has
  reviewed and deployed AIS/Docs/sql/PKG_LG.sql.

  No inserts, updates, deletes, commits or entity moves are performed.
  Enter the CURRENT authenticated IAS context when prompted.
  PAGE_ID 43 is the Manage Users ID in the repository page/API inventory.
*/
SET SERVEROUTPUT ON
SET VERIFY OFF
SET LINESIZE 220
SET PAGESIZE 100

PROMPT === Check Manage Users page ID in the target database ===
SELECT ID, PAGE_PATH, STATUS
  FROM T_MENU_PAGES
 WHERE ID = 43
    OR LOWER(PAGE_PATH) LIKE '%manage_user%'
 ORDER BY ID;

PROMPT === Verify package specification and body compiled ===
SELECT OBJECT_NAME, OBJECT_TYPE, STATUS
  FROM USER_OBJECTS
 WHERE OBJECT_NAME = 'PKG_LG'
   AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
 ORDER BY OBJECT_TYPE;

SELECT TYPE, LINE, POSITION, TEXT
  FROM USER_ERRORS
 WHERE NAME = 'PKG_LG'
 ORDER BY TYPE, SEQUENCE;

PROMPT === Provide the current IAS user context ===
ACCEPT TEST_PPNO NUMBER PROMPT 'Authenticated PP number: '
ACCEPT TEST_ROLE NUMBER PROMPT 'Current role/group ID (R_ID): '
ACCEPT TEST_ENTITY NUMBER PROMPT 'Current posting entity ID (ENT_ID): '

PROMPT === Entity types (DISPLAY_ORDER NULLS LAST) ===
VARIABLE ENTITY_TYPES REFCURSOR
EXEC PKG_LG.P_Get_Entity_type(&TEST_PPNO, &TEST_ROLE, &TEST_ENTITY, 43, :ENTITY_TYPES);
PRINT ENTITY_TYPES

PROMPT === Provide a returned Entity Type AUTID ===
ACCEPT TEST_TYPE NUMBER PROMPT 'Entity type AUTID (e.g. 6 for Branch): '

PROMPT === Distinct reporting offices for that child type ===
VARIABLE REPORTING_OFFICES REFCURSOR
EXEC PKG_LG.P_Get_Reporting_Office(&TEST_TYPE, &TEST_PPNO, &TEST_ROLE, &TEST_ENTITY, 43, :REPORTING_OFFICES);
PRINT REPORTING_OFFICES

PROMPT === Provide a reporting office ENTITY_ID from that result ===
ACCEPT TEST_PARENT NUMBER PROMPT 'Reporting office ENTITY_ID: '

PROMPT === Postings matching both selected parent and entity type ===
VARIABLE POSTINGS REFCURSOR
EXEC PKG_LG.P_Get_Posting(&TEST_PARENT, &TEST_TYPE, &TEST_PPNO, &TEST_ROLE, &TEST_ENTITY, 43, :POSTINGS);
PRINT POSTINGS

PROMPT === Negative check: unconfigured PAGE_ID must be rejected (-20052) ===
DECLARE
  L_CURSOR PKG_LG.t_cursor;
BEGIN
  BEGIN
    PKG_LG.P_Get_Entity_type(&TEST_PPNO, &TEST_ROLE, &TEST_ENTITY, 999999, L_CURSOR);
    RAISE_APPLICATION_ERROR(-20991,
      'Verification failed: unsupported PAGE_ID was accepted.');
  EXCEPTION
    WHEN OTHERS THEN
      IF SQLCODE = -20052 THEN
        DBMS_OUTPUT.PUT_LINE('PASS: unsupported PAGE_ID rejected.');
      ELSE
        RAISE;
      END IF;
  END;
END;
/

PROMPT === Verification finished: no database records changed ===
