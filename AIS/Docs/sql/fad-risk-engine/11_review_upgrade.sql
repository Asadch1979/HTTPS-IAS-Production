-- Apply once AFTER 10_schema.sql, or to an existing 4afa772 installation.
-- Never infer approval for existing rows. DBA must populate reviewed metadata.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
ALTER TABLE T_FAD_ENGINE_POLICY ADD (
 policy_version VARCHAR2(100), approved_on TIMESTAMP,
 reference_rule VARCHAR2(30) CHECK(reference_rule IN ('REQUIRE_UNIQUE','DISTINCT_OBSERVATIONS')));
ALTER TABLE T_FAD_RISK_ENGINE_RUN ADD (policy_snapshot CLOB, policy_version VARCHAR2(100),
 snapshot_on TIMESTAMP, calculated NUMBER DEFAULT 0 NOT NULL);
CREATE TABLE T_FAD_ENGINE_COMPLETENESS (
 eng_id NUMBER PRIMARY KEY REFERENCES T_AU_PLAN_ENG(ENG_ID),
 policy_version VARCHAR2(100) NOT NULL, final_observation_count NUMBER NOT NULL CHECK(final_observation_count>=0 AND final_observation_count=TRUNC(final_observation_count)),
 approval_ref VARCHAR2(500) NOT NULL, approved_on TIMESTAMP NOT NULL);
ALTER TABLE T_FAD_RISK_ENGINE_RESULT ADD (completeness_snapshot CLOB);
CREATE OR REPLACE VIEW V_FAD_RISK_ENGINE_CURRENT AS
 SELECT v.* FROM V_FAD_RISK_ENGINE_RESULTS v
 WHERE v.result_id=(SELECT MAX(x.result_id) KEEP (DENSE_RANK LAST ORDER BY u.started_on,u.run_id)
 FROM T_FAD_RISK_ENGINE_RESULT x JOIN T_FAD_RISK_ENGINE_RUN u ON u.run_id=x.run_id
 WHERE u.period_id=v.period_id AND x.eng_id=v.eng_id AND u.status='COMPLETED' AND u.completed_on IS NOT NULL)
 WITH READ ONLY;
-- Session-private validation snapshot, never an Annexure master or mapping fallback.
CREATE GLOBAL TEMPORARY TABLE T_FAD_ENGINE_MAP_CHECK (
 source_row NUMBER PRIMARY KEY,map_ok NUMBER NOT NULL) ON COMMIT DELETE ROWS;
