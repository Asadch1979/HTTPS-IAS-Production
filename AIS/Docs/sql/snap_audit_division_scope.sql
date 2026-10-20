-- IAS Audit Para Snap: Audit Division / Department-Cluster hierarchy.
-- This uses the EXISTING, DBA-maintained V_IAS_ENTITY_REPORTING_NAMES.
-- No new table, no seed data, no DML and no change to the IAS hierarchy.
--
-- User-verified audit-performing parents:
-- FAD = 112243 FIELD AUDIT DEPARTMENT       (nine direct audit clusters)
-- MAD = 113254 MANAGEMENT AUDIT DIVISION    (three direct audit departments)
--
-- This package validates the child selector. The reporting packages below
-- apply the same parent/descendant predicate to AUDITED_BY_ID / AUDITED_BY.
-- This is deliberately NOT based on the *audited entity's* reporting line.
--
-- Oracle deployment: review and compile this SPEC and BODY once, before
-- compiling the revised PKG_EXEC_DASHBOARD and PKG_EXEC_REPORTS.
-- Slash separators are for F5/SQL*Plus; do NOT run the whole file as F8.

CREATE OR REPLACE PACKAGE PKG_AUDIT_SNAP_SCOPE AS
  PROCEDURE REQUIRE_SELECTION(
    P_DIVISION_CODE IN VARCHAR2, P_AUDITBY_ID IN NUMBER DEFAULT NULL);
  PROCEDURE P_GET_CHILDREN(
    P_DIVISION_CODE IN VARCHAR2, IO_CURSOR OUT SYS_REFCURSOR);
END PKG_AUDIT_SNAP_SCOPE;
/

CREATE OR REPLACE PACKAGE BODY PKG_AUDIT_SNAP_SCOPE AS

  FUNCTION DIVISION_PARENT(P_DIVISION_CODE IN VARCHAR2)
    RETURN NUMBER IS
  BEGIN
    CASE P_DIVISION_CODE
      WHEN 'FAD' THEN RETURN 112243;
      WHEN 'MAD' THEN RETURN 113254;
      ELSE RAISE_APPLICATION_ERROR(
        -20871,'Select Field Audit Division or Management Audit Division.');
    END CASE;
  END DIVISION_PARENT;

  PROCEDURE REQUIRE_SELECTION(
    P_DIVISION_CODE IN VARCHAR2, P_AUDITBY_ID IN NUMBER DEFAULT NULL) AS
    V_PARENT_ID NUMBER;
    V_COUNT NUMBER;
  BEGIN
    IF P_DIVISION_CODE = '1' THEN
      IF P_AUDITBY_ID IS NOT NULL THEN
        RAISE_APPLICATION_ERROR(-20872,'All Divisions does not accept a Department / Cluster.');
      END IF;
      REQUIRE_SELECTION('FAD',NULL);
      REQUIRE_SELECTION('MAD',NULL);
      RETURN;
    END IF;
    V_PARENT_ID := DIVISION_PARENT(P_DIVISION_CODE);

    IF P_AUDITBY_ID IS NOT NULL
       AND (P_AUDITBY_ID <= 0 OR
            P_AUDITBY_ID <> TRUNC(P_AUDITBY_ID)) THEN
      RAISE_APPLICATION_ERROR(-20872,'Invalid Department / Cluster ID.');
    END IF;

    -- The UI lists DIRECT children (REPORTING_1). A manually supplied child
    -- cannot escape its chosen division. NULL child means 'All', not a
    -- request to bypass the division predicate in reporting SQL.
    SELECT COUNT(*) INTO V_COUNT
      FROM V_IAS_ENTITY_REPORTING_NAMES h
     WHERE h.REPORTING_1 = V_PARENT_ID
       AND h.ENTITY_ID <> V_PARENT_ID
       AND (P_AUDITBY_ID IS NULL OR h.ENTITY_ID = P_AUDITBY_ID);

    IF V_COUNT = 0 THEN
      RAISE_APPLICATION_ERROR(
        -20872,'Department / Cluster is not a direct child of the selected Division.');
    END IF;
  END REQUIRE_SELECTION;

  PROCEDURE P_GET_CHILDREN(
    P_DIVISION_CODE IN VARCHAR2, IO_CURSOR OUT SYS_REFCURSOR) AS
    V_PARENT_ID NUMBER;
  BEGIN
    IF P_DIVISION_CODE = '1' THEN
      REQUIRE_SELECTION(P_DIVISION_CODE,NULL);
      OPEN IO_CURSOR FOR SELECT CAST(NULL AS NUMBER) ID, CAST(NULL AS VARCHAR2(500)) NAME FROM dual WHERE 1=0;
      RETURN;
    END IF;
    V_PARENT_ID := DIVISION_PARENT(P_DIVISION_CODE);
    REQUIRE_SELECTION(P_DIVISION_CODE,NULL);

    OPEN IO_CURSOR FOR
      SELECT DISTINCT h.ENTITY_ID AS ID,
                      h.ENTITY_NAME AS NAME
        FROM V_IAS_ENTITY_REPORTING_NAMES h
       WHERE h.REPORTING_1 = V_PARENT_ID
         AND h.ENTITY_ID <> V_PARENT_ID
       ORDER BY NAME, ID;
  END P_GET_CHILDREN;
END PKG_AUDIT_SNAP_SCOPE;
/

-- For ALL in each reporting cursor, include:
-- (1) the division parent itself, if it directly performs an audit, and
-- (2) audit-performing descendants whose entity IDs appear in the
-- existing REPORTING_1..REPORTING_6 hierarchy.
-- For one child, require the exact AUDITED_BY_ID match IN ADDITION to this
-- division-wide predicate. All report readers and PDFs must apply it.
--
-- Deployment evidence: validate that AUDITED_BY_ID/AUDITED_BY values in
-- real lifecycle and reports actually use these IAS ENTITY_ID values.
-- If data uses another key, DO NOT fall back to bank-wide filtering.
