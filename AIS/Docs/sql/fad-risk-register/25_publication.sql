-- Administrative deployment operation only. Do not grant to application roles.
-- Explicit dedicated deployment principals only. Empty by default, fail closed.
-- Never populate with the application's database account; no application DML grants.
CREATE TABLE T_FAD_RISK_PUBLISH_ADMIN (database_user VARCHAR2(128) PRIMARY KEY);
CREATE OR REPLACE PROCEDURE P_PUBLISH_FAD_RISK(p_version NUMBER) AUTHID DEFINER IS
 s VARCHAR2(12); n NUMBER; w NUMBER;
BEGIN
 SELECT COUNT(*) INTO n FROM T_FAD_RISK_PUBLISH_ADMIN
  WHERE database_user=SYS_CONTEXT('USERENV','SESSION_USER');
 IF n<>1 THEN RAISE_APPLICATION_ERROR(-20043,'Dedicated publication administrator required.'); END IF;
 SELECT state INTO s FROM t_fad_risk_version WHERE version_id=p_version FOR UPDATE;
 P_CHECK_FAD_RISK_V1(p_version);
 IF s<>'DRAFT' THEN RAISE_APPLICATION_ERROR(-20045,'Only draft configurations may be published.'); END IF;
 SELECT COUNT(*) INTO n FROM t_fad_risk_annex WHERE version_id=p_version;
 IF n<>121 THEN RAISE_APPLICATION_ERROR(-20040,'Exactly 121 Annexures required.'); END IF;
 SELECT COUNT(*),SUM(weight) INTO n,w FROM t_fad_risk_process WHERE version_id=p_version;
 IF n<>20 OR w<>100 THEN RAISE_APPLICATION_ERROR(-20040,'Exactly 20 processes totaling 100 required.'); END IF;
 SELECT COUNT(*) INTO n FROM t_fad_risk_subprocess s LEFT JOIN t_fad_risk_process p
 ON p.version_id=s.version_id AND p.process_code=s.process_code WHERE s.version_id=p_version
 AND (p.process_code IS NULL OR (s.criterion IN ('instances','fraud_override') AND s.gravity IS NULL)
 OR (s.criterion='fraud_override' AND p.weight<>0));
 IF n<>0 THEN RAISE_APPLICATION_ERROR(-20040,'Invalid sub-process mapping or gravity.'); END IF;
 SELECT COUNT(*) INTO n FROM t_fad_risk_annex a LEFT JOIN t_fad_risk_subprocess s
 ON s.version_id=a.version_id AND s.sub_code=a.sub_code WHERE a.version_id=p_version AND s.sub_code IS NULL;
 IF n<>0 THEN RAISE_APPLICATION_ERROR(-20040,'Invalid Annexure parent.'); END IF;
 SELECT COUNT(*) INTO n FROM t_fad_risk_process p WHERE p.version_id=p_version
 AND ((p.weight>0 AND (p.denominator<=0 OR p.weight>p.denominator)) OR NOT EXISTS
 (SELECT 1 FROM t_fad_risk_subprocess s WHERE s.version_id=p.version_id AND s.process_code=p.process_code));
 IF n<>0 THEN RAISE_APPLICATION_ERROR(-20040,'Invalid process or denominator.'); END IF;
 SELECT COUNT(*) INTO n FROM t_fad_risk_version WHERE version_id=p_version AND calculator='PKG_FAD_RISK_SCORE_V1';
 IF n<>1 THEN RAISE_APPLICATION_ERROR(-20040,'Unsupported calculator.'); END IF;
 UPDATE t_fad_risk_version SET state='PUBLISHED',published_on=SYSTIMESTAMP WHERE version_id=p_version;
 -- Caller explicitly commits after inspection; no hidden commit.
END;
/
