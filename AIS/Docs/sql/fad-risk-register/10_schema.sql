-- Source-only additive migration. Review 00_preflight output before execution.
-- Oracle 19c+; execute once as schema owner, with application stopped.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
CREATE TABLE T_FAD_RISK_VERSION (
 version_id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
 version_name VARCHAR2(100) NOT NULL UNIQUE, source_sha256 VARCHAR2(64) NOT NULL,
 calculator VARCHAR2(40) DEFAULT 'PKG_FAD_RISK_SCORE_V1' NOT NULL,
 state VARCHAR2(12) DEFAULT 'DRAFT' NOT NULL CHECK(state IN ('DRAFT','PUBLISHED')),
 rules CLOB NOT NULL, published_on TIMESTAMP, CONSTRAINT CK_FR_RULES CHECK(rules IS JSON));
CREATE TABLE T_FAD_RISK_PROCESS (
 version_id NUMBER REFERENCES T_FAD_RISK_VERSION NOT NULL, process_code VARCHAR2(20) NOT NULL,
 main_kpi VARCHAR2(200) NOT NULL, sub_kpi VARCHAR2(200) NOT NULL,
 description VARCHAR2(4000) NOT NULL, weight NUMBER NOT NULL CHECK(weight BETWEEN 0 AND 100),
 denominator NUMBER NOT NULL CHECK(denominator>=0),
 PRIMARY KEY(version_id,process_code));
CREATE TABLE T_FAD_RISK_SUBPROCESS (
 version_id NUMBER NOT NULL, sub_code VARCHAR2(20) NOT NULL, process_code VARCHAR2(20) NOT NULL,
 description VARCHAR2(4000) NOT NULL, gravity NUMBER CHECK(gravity BETWEEN 1 AND 5),
 criterion VARCHAR2(20) NOT NULL CHECK(criterion IN ('instances','movement','shortfall','profit','fraud_override')),
 PRIMARY KEY(version_id,sub_code),
 FOREIGN KEY(version_id,process_code) REFERENCES T_FAD_RISK_PROCESS);
CREATE TABLE T_FAD_RISK_ANNEX (
 version_id NUMBER NOT NULL, code VARCHAR2(50) NOT NULL, sub_code VARCHAR2(20) NOT NULL,
 description VARCHAR2(4000) NOT NULL, risk_level VARCHAR2(20) NOT NULL, source_cell VARCHAR2(100) NOT NULL,
 PRIMARY KEY(version_id,code), FOREIGN KEY(version_id,sub_code) REFERENCES T_FAD_RISK_SUBPROCESS);
-- Explicit, reviewed assignments; empty by default. No inferred role grants.
CREATE TABLE T_FAD_RISK_ACCESS (
 group_id NUMBER NOT NULL, audit_entity_id NUMBER NOT NULL,
 can_prepare CHAR(1) DEFAULT 'N' NOT NULL CHECK(can_prepare IN ('Y','N')),
 can_review CHAR(1) DEFAULT 'N' NOT NULL CHECK(can_review IN ('Y','N')),
 PRIMARY KEY(group_id,audit_entity_id));
CREATE TABLE T_FAD_RISK_ASSESSMENT (
 assessment_id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
 eng_id NUMBER NOT NULL REFERENCES T_AU_PLAN_ENG(ENG_ID),
 entity_id NUMBER NOT NULL, version_id NUMBER NOT NULL REFERENCES T_FAD_RISK_VERSION,
 branch_name VARCHAR2(500) NOT NULL, audit_period VARCHAR2(500) NOT NULL,
 branch_size VARCHAR2(10) NOT NULL CHECK(branch_size IN ('Small','Medium','Large')),
 portfolio NUMBER NOT NULL CHECK(portfolio>=0), size_reason VARCHAR2(2000) NOT NULL,
 status VARCHAR2(15) DEFAULT 'DRAFT' NOT NULL CHECK(status IN ('DRAFT','SUBMITTED','RETURNED','APPROVED')),
 revision NUMBER DEFAULT 1 NOT NULL, preparer_id NUMBER NOT NULL, reviewer_id NUMBER,
 score NUMBER, rating VARCHAR2(10), fraud_count NUMBER,
 created_on TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL, updated_on TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
 UNIQUE(eng_id,version_id), UNIQUE(assessment_id,version_id));
CREATE TABLE T_FAD_RISK_DETAIL (
 assessment_id NUMBER NOT NULL, version_id NUMBER NOT NULL, item_code VARCHAR2(50) NOT NULL,
 sub_code VARCHAR2(20) NOT NULL, instances NUMBER, base_value NUMBER, actual_value NUMBER,
 observation VARCHAR2(2000), evidence VARCHAR2(2000),
 PRIMARY KEY(assessment_id,item_code),
 FOREIGN KEY(assessment_id,version_id) REFERENCES T_FAD_RISK_ASSESSMENT(assessment_id,version_id),
 FOREIGN KEY(version_id,sub_code) REFERENCES T_FAD_RISK_SUBPROCESS);
CREATE TABLE T_FAD_RISK_RESULT (
 assessment_id NUMBER NOT NULL REFERENCES T_FAD_RISK_ASSESSMENT,
 process_code VARCHAR2(20) NOT NULL, raw_marks NUMBER NOT NULL, weight NUMBER NOT NULL,
 denominator NUMBER NOT NULL, capped_marks NUMBER NOT NULL,
 PRIMARY KEY(assessment_id,process_code));
CREATE TABLE T_FAD_RISK_HISTORY (
 history_id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
 assessment_id NUMBER NOT NULL REFERENCES T_FAD_RISK_ASSESSMENT,
 revision NUMBER NOT NULL, action VARCHAR2(20) NOT NULL, actor_id NUMBER NOT NULL,
 recorded_on TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL, remarks VARCHAR2(2000),
 input_snapshot CLOB NOT NULL, result_snapshot CLOB,
 CONSTRAINT CK_FR_HISTORY_INPUT CHECK(input_snapshot IS JSON));
CREATE INDEX IX_FR_ENG ON T_FAD_RISK_ASSESSMENT(eng_id);
CREATE INDEX IX_FR_HISTORY ON T_FAD_RISK_HISTORY(assessment_id,history_id);
-- Version snapshots and approved assessments cannot be rewritten by normal DML.
CREATE OR REPLACE TRIGGER TR_FR_VERSION_LOCK BEFORE UPDATE OR DELETE ON T_FAD_RISK_VERSION
FOR EACH ROW BEGIN
 IF :OLD.state='PUBLISHED' THEN RAISE_APPLICATION_ERROR(-20045,'Published configuration is immutable.'); END IF;
END;
/
CREATE OR REPLACE TRIGGER TR_FR_ASSESSMENT_LOCK BEFORE UPDATE OR DELETE ON T_FAD_RISK_ASSESSMENT
FOR EACH ROW BEGIN
 IF :OLD.status='APPROVED' THEN RAISE_APPLICATION_ERROR(-20045,'Approved assessment is immutable.'); END IF;
END;
/
CREATE OR REPLACE TRIGGER TR_FR_HISTORY_LOCK BEFORE UPDATE OR DELETE ON T_FAD_RISK_HISTORY
BEGIN RAISE_APPLICATION_ERROR(-20045,'Assessment history is append only.'); END;
/
