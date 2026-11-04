-- Source-only allocator adapter. Does not create operational-table sequences,
-- infer a starting value, reserve ranges, lock operational tables or enable posting.
CREATE TABLE T_IAS_LEGACY_ID_BINDING (
  TARGET_CODE VARCHAR2(20) PRIMARY KEY
    CHECK (TARGET_CODE IN ('FAD','TEXT','COMPLIANCE','RESPONSIBILITY')),
  SEQUENCE_NAME VARCHAR2(128),
  ALL_WRITERS_COORDINATED CHAR(1) DEFAULT 'N' NOT NULL
    CHECK (ALL_WRITERS_COORDINATED IN ('N','Y')),
  APPROVED_BY NUMBER,
  APPROVED_ON DATE,
  APPROVAL_REFERENCE VARCHAR2(2000)
);
INSERT INTO T_IAS_LEGACY_ID_BINDING(TARGET_CODE) VALUES ('FAD');
INSERT INTO T_IAS_LEGACY_ID_BINDING(TARGET_CODE) VALUES ('TEXT');
INSERT INTO T_IAS_LEGACY_ID_BINDING(TARGET_CODE) VALUES ('COMPLIANCE');
INSERT INTO T_IAS_LEGACY_ID_BINDING(TARGET_CODE) VALUES ('RESPONSIBILITY');

CREATE OR REPLACE PACKAGE PKG_IAS_LEGACY_IDS AS
  FUNCTION NEXT_ID(p_target IN VARCHAR2) RETURN NUMBER;
END PKG_IAS_LEGACY_IDS;
/
CREATE OR REPLACE PACKAGE BODY PKG_IAS_LEGACY_IDS AS
  FUNCTION NEXT_ID(p_target IN VARCHAR2) RETURN NUMBER IS
    v_binding T_IAS_LEGACY_ID_BINDING%ROWTYPE;
    v_id NUMBER;
  BEGIN
    -- Serializes binding changes only, never existing IAS operational tables.
    SELECT * INTO v_binding FROM T_IAS_LEGACY_ID_BINDING
     WHERE target_code=p_target FOR UPDATE;
    IF v_binding.sequence_name IS NULL OR v_binding.all_writers_coordinated<>'Y'
      OR v_binding.approved_by IS NULL OR v_binding.approved_on IS NULL
      OR v_binding.approval_reference IS NULL THEN
      RAISE_APPLICATION_ERROR(-20804,'Approved shared identifier allocation is not configured.');
    END IF;
    -- Owner-managed binding, not request input. SIMPLE_SQL_NAME excludes schema
    -- switches, links and SQL fragments. This must be the allocator shared by ALL
    -- writers for the target, not a new legacy-workflow-only sequence.
    EXECUTE IMMEDIATE 'SELECT '||DBMS_ASSERT.SIMPLE_SQL_NAME(v_binding.sequence_name)||'.NEXTVAL FROM dual' INTO v_id;
    IF v_id IS NULL OR v_id<=0 OR v_id<>TRUNC(v_id) THEN
      RAISE_APPLICATION_ERROR(-20804,'Invalid identifier from the approved allocator.');
    END IF;
    RETURN v_id;
  END;
END PKG_IAS_LEGACY_IDS;
/
