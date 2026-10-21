-- F8: execute this file alone in PL/SQL Developer SQL Window.
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
