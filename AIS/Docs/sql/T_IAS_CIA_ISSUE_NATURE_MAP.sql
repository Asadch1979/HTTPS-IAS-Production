-- CIA Audit Command Center: explicit Nature of Issue mappings.
-- Oracle 18c+. Run this CREATE TABLE ONCE, before compiling the new
-- PKG_EXEC_DASHBOARD specification/body. No existing IAS tables modified.
-- No guessed source IDs are seeded. Load approved mappings only after
-- inspecting live annex, violation subtype, and audit portfolio masters.
--
-- SOURCE_TYPE:
--   VIOLATION_SUB  -> T_AU_OBSERVATION.V_CAT_NATURE_ID
--   ANNEX          -> T_AU_OBSERVATION.ANNEX
--   AUDIT_PORTFOLIO -> T_AU_PLAN_ENG.AUDITBY_ID
-- Lower MATCH_PRIORITY wins. Ties: VIOLATION_SUB, ANNEX, AUDIT_PORTFOLIO.
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
          ('VIOLATION_SUB', 'ANNEX', 'AUDIT_PORTFOLIO')),

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
-- Suggested precedence: audit-portfolio 10 for management/IS portfolio,
-- violation-sub 20, annex 30. Configure explicit overlaps consciously.
-- Without mappings the chart returns zero in all ten categories with
-- MAPPING_STATUS='PARTIAL' and UNMAPPED_ISSUES > 0. Do not publish zeros.
