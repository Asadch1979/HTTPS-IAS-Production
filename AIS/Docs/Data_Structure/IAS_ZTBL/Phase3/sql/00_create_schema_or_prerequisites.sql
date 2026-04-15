/*
  IAS_ZTBL Phase 3 prerequisite script

  Purpose
  - establish execution assumptions for the IAS_ZTBL schema
  - document DBA-level prerequisites for side-by-side migration

  Notes
  - this script does not drop or alter the legacy production schema
  - DBA-owned schema creation and legacy grants remain controlled activities
*/

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;

ALTER SESSION SET CURRENT_SCHEMA = IAS_ZTBL;

-------------------------------------------------------------------------------
-- Optional DBA-only bootstrap
-------------------------------------------------------------------------------
-- Run the following as a DBA only if IAS_ZTBL does not already exist.
--
-- CREATE USER IAS_ZTBL IDENTIFIED BY "<password>";
-- GRANT CREATE SESSION, CREATE TABLE, CREATE VIEW, CREATE SEQUENCE,
--       CREATE TRIGGER, CREATE PROCEDURE, CREATE SYNONYM TO IAS_ZTBL;
--
-- Grant source read access from the legacy schema for migration rehearsal:
-- GRANT SELECT ON ZTBLAIS_PROD.T_USER TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.T_GROUPS TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.T_AUDITEE_ENTITIES TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.T_AUDITEE_ENT_TYPES TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.T_AU_PLAN TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.T_AU_PLAN_ENG TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.AIS_T_AU_OBSERVATION TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.AIS_T_AU_POST_COMPLIANCE TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.T_AU_IID_COMPLAINT_HDR TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.T_COM_AUDIT_OM TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.T_FRPT_TEXT_BLOCKS TO IAS_ZTBL;
-- GRANT SELECT ON ZTBLAIS_PROD.EMAILTEMPLATES TO IAS_ZTBL;
--
-- Optional synonym pattern if migration is executed from IAS_ZTBL:
-- CREATE OR REPLACE SYNONYM IAS_ZTBL.LEGACY_T_USER FOR ZTBLAIS_PROD.T_USER;

-------------------------------------------------------------------------------
-- Execution assumptions
-------------------------------------------------------------------------------
PROMPT IAS_ZTBL Phase 3 prerequisite assumptions loaded.
PROMPT Run core DDL scripts 01-08 before running migration scripts 10-20.
