-- CIA Audit Command Center: explicit Nature of Issue mappings.
-- Oracle 18c+. Run this CREATE TABLE ONCE, before compiling the new
-- PKG_EXEC_DASHBOARD specification/body. No existing IAS tables modified.
-- This CREATE TABLE is for a fresh schema only. Existing IAS schemas must use
-- cia_issue_nature_checklist_root_migration.sql to extend the source constraint.
-- Eight verified checklist-root IDs from IAS audit period 5 are seeded by that
-- separate, reviewed migration; no other category/source IDs are guessed.
--
-- SOURCE_TYPE:
--   CHECKLIST_ROOT -> T_AUDIT_CHECKLIST.T_ID, reached through
--                     T_AU_OBSERVATION.CHECKLISTDETAIL_ID ->
--                     T_AUDIT_CHECKLIST_DETAILS.ID -> T_AUDIT_CHECKLIST_SUB.S_ID
--                     -> T_AUDIT_CHECKLIST.T_ID.
--   VIOLATION_SUB  -> T_AU_OBSERVATION.V_CAT_NATURE_ID
--   ANNEX          -> T_AU_OBSERVATION.ANNEX
--   AUDIT_PORTFOLIO -> T_AU_PLAN_ENG.AUDITBY_ID
-- CHECKLIST_ROOT is the primary classifier when linked. Fallback matches
-- use MATCH_PRIORITY, then VIOLATION_SUB / ANNEX / AUDIT_PORTFOLIO order.
-- Do not classify a linked checklist issue by AUDIT_PORTFOLIO instead.
--
CREATE TABLE T_IAS_CIA_ISSUE_NATURE_MAP
(
    SOURCE_TYPE     VARCHAR2(24) NOT NULL,
    SOURCE_ID       NUMBER NOT NULL,
    CATEGORY_CODE   VARCHAR2(40) NOT NULL,
    MATCH_PRIORITY  NUMBER(5) DEFAULT 100 NOT NULL,
    IS_ACTIVE       CHAR(1) DEFAULT 'Y' NOT NULL,

    CONSTRAINT PK_IAS_CIA_NATURE_MAP
        PRIMARY KEY (SOURCE_TYPE, SOURCE_ID),

    CONSTRAINT CK_IAS_CIA_NATURE_SOURCE
        CHECK (SOURCE_TYPE IN
          ('CHECKLIST_ROOT', 'VIOLATION_SUB', 'ANNEX', 'AUDIT_PORTFOLIO')),

    CONSTRAINT CK_IAS_CIA_NATURE_CATEGORY
        CHECK (CATEGORY_CODE IN
          ('AML_CFT', 'ACCOUNTS', 'LOAN_ADVANCES',
           'CASH_MANAGEMENT', 'RECORD_MANAGEMENT',
           'BANKING_OPERATIONS', 'BRANCH_GENERAL_CONTROLS',
           'ACCOUNTING_REPORTING_RECON', 'MANAGEMENT_AUDIT',
           'IS_AUDIT')),

    CONSTRAINT CK_IAS_CIA_NATURE_ACTIVE
        CHECK (IS_ACTIVE IN ('Y', 'N')),

    CONSTRAINT CK_IAS_CIA_NATURE_PRIORITY
        CHECK (MATCH_PRIORITY BETWEEN 1 AND 99999),

    CONSTRAINT CK_IAS_CIA_NATURE_SOURCE_ID
        CHECK (SOURCE_ID > 0)
);

COMMENT ON TABLE T_IAS_CIA_ISSUE_NATURE_MAP IS
  'Approved source-ID mappings for CIA Nature of Issue Identified chart';
COMMENT ON COLUMN T_IAS_CIA_ISSUE_NATURE_MAP.MATCH_PRIORITY IS
  'Lowest number wins when an observation matches multiple configured sources';

-- Preflight queries (run individually in SQL Developer with F8):
-- SELECT t_id, heading FROM t_audit_checklist ORDER BY t_id;
-- SELECT id, code, heading FROM t_audit_checklist_annexure ORDER BY id;
-- SELECT id, sub_v_name FROM t_control_violation_sub ORDER BY id;
-- SELECT entity_id, name FROM t_auditee_entities
--  WHERE UPPER(name) LIKE '%AUDIT%' ORDER BY name;
--
-- Example ONLY after approving the actual IDs:
-- INSERT INTO T_IAS_CIA_ISSUE_NATURE_MAP
--   (SOURCE_TYPE, SOURCE_ID, CATEGORY_CODE, MATCH_PRIORITY)
-- VALUES ('VIOLATION_SUB', <VERIFIED_ID>, 'AML_CFT', 20);
-- COMMIT;
--
-- Checklist roots with verified T_IDs take precedence over all fallback types.
-- Suggested fallback: violation-sub 20, annex 30, audit-portfolio 40.
-- Accounts and IS Audit require their own verified source IDs before mapping.
-- Without mappings the chart returns zero in all ten categories with
-- MAPPING_STATUS='PARTIAL' and UNMAPPED_ISSUES > 0. Do not publish zeros.
