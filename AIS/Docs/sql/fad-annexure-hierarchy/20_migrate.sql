-- DATA ONLY. Dedicated session; no DDL or internally committing package calls.
-- Existing sequences below are evidenced in PKG_FAD_RISK.sql; confirm live ownership,
-- triggers and sequence positions in 00 before execution. No sequence resets.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
SET SERVEROUTPUT ON
DECLARE
 expected_source_sha256 CONSTANT VARCHAR2(64) := '9e5f375ba0a8df2339edc3374307aa049ac2975580ea0f206d31bbb8090b2425';
 n NUMBER; m NUMBER; k NUMBER; p NUMBER; sp NUMBER; g NUMBER; a NUMBER;
 PROCEDURE require_true(ok BOOLEAN, message VARCHAR2) IS
 BEGIN IF ok IS NULL OR NOT ok THEN RAISE_APPLICATION_ERROR(-20101,message); END IF; END;
BEGIN
 LOCK TABLE T_FAD_RISK_MAP_STAGE IN SHARE MODE;
 LOCK TABLE T_RISK_FAD_CODE_ALIAS IN SHARE MODE;
 LOCK TABLE T_FAD_RISK_WEIGHT_APPROVAL IN SHARE MODE;
 SELECT COUNT(*) INTO n FROM T_FAD_RISK_MAP_STAGE WHERE SOURCE_FILE_SHA256 IS NULL
 OR NOT REGEXP_LIKE(TRIM(SOURCE_FILE_SHA256),'^[0-9a-fA-F]{64}
 LOCK TABLE T_RISK_FAD_KPI_MAIN, T_RISK_FAD_KPI_SUB, T_RISK_FAD_PACKAGE,
 T_RISK_FAD_PROCESS, T_RISK_FAD_SUB_PROCESS, T_RISK_FAD_ANNEXURE_MAP IN EXCLUSIVE MODE;
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_APPROVED_SOURCE;
 require_true(n=121,'Expected 121 staged mappings');
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_APPROVED_SOURCE WHERE
 main_code IS NULL OR package_code IS NULL OR process_code IS NULL OR procedure_code IS NULL OR annexure_code IS NULL
 OR main_name IS NULL OR package_name IS NULL OR process_name IS NULL OR procedure_name IS NULL
 OR gravity_score IS NULL OR violation_description IS NULL OR risk_classification IS NULL OR package_weight IS NULL OR process_weight IS NULL
 OR LENGTHB(main_name)>4000 OR LENGTHB(package_name)>4000 OR LENGTHB(process_name)>4000 OR LENGTHB(procedure_name)>4000
 OR LENGTHB(risk_classification)>200 OR LENGTHB(main_code)>50 OR LENGTHB(package_code)>50
 OR LENGTHB(process_code)>50 OR LENGTHB(procedure_code)>50 OR LENGTHB(annexure_code)>50;
 require_true(n=0,'Missing source values or oversized text; no truncation permitted');
 SELECT COUNT(DISTINCT main_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=4,'Unexpected main_code cardinality');
 SELECT COUNT(DISTINCT package_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=9,'Unexpected package_code cardinality');
 SELECT COUNT(DISTINCT process_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=14,'Unexpected process_code cardinality');
 SELECT COUNT(DISTINCT procedure_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=37,'Unexpected procedure_code cardinality');
 SELECT COUNT(DISTINCT UPPER(TRIM(annexure_code))) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=121,'Unexpected annexure_code cardinality');
 -- Print every unresolved or ambiguous row before rejecting the entire import.
 n:=0;
 FOR r IN (SELECT s.annexure_code FROM V_FAD_RISK_APPROVED_SOURCE s WHERE
 (SELECT COUNT(*) FROM T_AUDIT_CHECKLIST_ANNEXURE x WHERE UPPER(TRIM(x.code))=UPPER(TRIM(s.annexure_code))) +
 (SELECT COUNT(*) FROM T_RISK_FAD_CODE_ALIAS al WHERE UPPER(TRIM(al.staged_code))=UPPER(TRIM(s.annexure_code))) <> 1) LOOP
 DBMS_OUTPUT.PUT_LINE('UNRESOLVED / CONFLICT: '||r.annexure_code); n:=n+1; END LOOP;
 require_true(n=0,'Resolve all Annexures via diagnostic and separately approved aliases before migration');
 FOR r IN (SELECT * FROM V_FAD_RISK_APPROVED_SOURCE ORDER BY main_code,package_code,process_code,procedure_code,annexure_code) LOOP
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_GRAVITY WHERE gravity_score=r.gravity_score;
 require_true(n=1,'Unresolved risk classification: '||r.risk_classification);
 SELECT gravity_id INTO g FROM T_RISK_FAD_GRAVITY WHERE gravity_score=r.gravity_score;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_KPI_MAIN WHERE KPI_CODE=r.main_code;
 require_true(n<=1,'Ambiguous main code: '||r.main_code);
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_KPI_MAIN (KPI_MAIN_ID,KPI_CODE,KPI_NAME,IS_ACTIVE)
 VALUES (SEQ_RISK_FAD_KPI_MAIN.NEXTVAL,r.main_code,r.main_name,'Y') RETURNING KPI_MAIN_ID INTO m;
 ELSE
 SELECT KPI_MAIN_ID INTO m FROM T_RISK_FAD_KPI_MAIN WHERE KPI_CODE=r.main_code;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_KPI_MAIN WHERE KPI_MAIN_ID=m AND KPI_NAME=r.main_name AND IS_ACTIVE='Y';
 require_true(n=1,'Historical main conflicts with source: '||r.main_code);
 END IF;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_KPI_SUB WHERE KPI_SUB_CODE=r.package_code;
 require_true(n<=1,'Ambiguous package code: '||r.package_code);
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_KPI_SUB (KPI_SUB_ID,KPI_SUB_CODE,KPI_SUB_NAME,IS_ACTIVE,KPI_MAIN_ID,WEIGHTAGE)
 VALUES (SEQ_RISK_FAD_KPI_SUB.NEXTVAL,r.package_code,r.package_name,'Y',m,r.package_weight) RETURNING KPI_SUB_ID INTO k;
 ELSE
 SELECT KPI_SUB_ID INTO k FROM T_RISK_FAD_KPI_SUB WHERE KPI_SUB_CODE=r.package_code;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_KPI_SUB WHERE KPI_SUB_ID=k AND KPI_SUB_NAME=r.package_name AND IS_ACTIVE='Y' AND KPI_MAIN_ID=m AND WEIGHTAGE=r.package_weight;
 require_true(n=1,'Historical package conflicts with source: '||r.package_code);
 END IF;
 INSERT INTO T_RISK_FAD_PACKAGE(PACKAGE_ID) SELECT k FROM dual WHERE NOT EXISTS (SELECT 1 FROM T_RISK_FAD_PACKAGE WHERE PACKAGE_ID=k);
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_PROCESS WHERE PROCESS_CODE=r.process_code;
 require_true(n<=1,'Ambiguous process code: '||r.process_code);
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_PROCESS (PROCESS_ID,PROCESS_CODE,PROCESS_NAME,IS_ACTIVE,KPI_SUB_ID,WEIGHTAGE,PACKAGE_ID)
 VALUES (SEQ_RISK_FAD_PROCESS.NEXTVAL,r.process_code,r.process_name,'Y',k,r.process_weight,k) RETURNING PROCESS_ID INTO p;
 ELSE
 SELECT PROCESS_ID INTO p FROM T_RISK_FAD_PROCESS WHERE PROCESS_CODE=r.process_code;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_PROCESS WHERE PROCESS_ID=p AND PROCESS_NAME=r.process_name AND IS_ACTIVE='Y' AND KPI_SUB_ID=k AND WEIGHTAGE=r.process_weight AND (PACKAGE_ID IS NULL OR PACKAGE_ID=k);
 require_true(n=1,'Historical process conflicts with source: '||r.process_code);
 END IF;
 UPDATE T_RISK_FAD_PROCESS SET PACKAGE_ID=k WHERE PROCESS_ID=p AND PACKAGE_ID IS NULL;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_SUB_PROCESS WHERE SUB_PROCESS_CODE=r.procedure_code;
 require_true(n<=1,'Ambiguous procedure code: '||r.procedure_code);
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_SUB_PROCESS (SUB_PROCESS_ID,SUB_PROCESS_CODE,SUB_PROCESS_NAME,IS_ACTIVE,PROCESS_ID,GRAVITY_ID)
 VALUES (SEQ_RISK_FAD_SUB_PROCESS.NEXTVAL,r.procedure_code,r.procedure_name,'Y',p,g) RETURNING SUB_PROCESS_ID INTO sp;
 ELSE
 SELECT SUB_PROCESS_ID INTO sp FROM T_RISK_FAD_SUB_PROCESS WHERE SUB_PROCESS_CODE=r.procedure_code;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_SUB_PROCESS WHERE SUB_PROCESS_ID=sp AND SUB_PROCESS_NAME=r.procedure_name AND IS_ACTIVE='Y' AND PROCESS_ID=p AND GRAVITY_ID=g;
 require_true(n=1,'Historical procedure conflicts with source: '||r.procedure_code);
 END IF;
 SELECT id INTO a FROM (
 SELECT x.id FROM T_AUDIT_CHECKLIST_ANNEXURE x WHERE UPPER(TRIM(x.code))=UPPER(TRIM(r.annexure_code))
 UNION ALL SELECT x.id FROM T_RISK_FAD_CODE_ALIAS al JOIN T_AUDIT_CHECKLIST_ANNEXURE x ON x.id=al.annexure_id WHERE UPPER(TRIM(al.staged_code))=UPPER(TRIM(r.annexure_code)));
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_ANNEXURE_MAP WHERE UPPER(TRIM(source_annexure_code))=UPPER(TRIM(r.annexure_code)) OR annexure_id=a;
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_ANNEXURE_MAP(sub_process_id,annexure_id,source_annexure_code,violation_description,risk_classification,source_sha256)
 VALUES(sp,a,r.annexure_code,r.violation_description,r.risk_classification,expected_source_sha256);
 ELSE
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_ANNEXURE_MAP WHERE UPPER(TRIM(source_annexure_code))=UPPER(TRIM(r.annexure_code))
 AND source_sha256=expected_source_sha256 AND annexure_id=a AND sub_process_id=sp AND risk_classification=r.risk_classification
 AND DBMS_LOB.COMPARE(violation_description,TO_CLOB(r.violation_description))=0;
 require_true(n=1,'Existing Annexure mapping conflicts: '||r.annexure_code);
 END IF;
 END LOOP;
 -- One atomic commit, including reruns; failures roll back every data change.
 COMMIT;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
)
 OR LOWER(TRIM(SOURCE_FILE_SHA256))<>expected_source_sha256;
 require_true(n=0,'Source CSV SHA-256 mismatch, missing hash or mixed source batch');
 -- Independently compare every source field with fingerprints authored from the CSV.
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_SOURCE_FINGERPRINT e
 FULL OUTER JOIN V_FAD_RISK_STAGE_FINGERPRINT s ON s.excel_row=e.excel_row
 WHERE e.excel_row IS NULL OR s.excel_row IS NULL OR s.row_sha256<>e.row_sha256 OR s.row_sha256 IS NULL;
 require_true(n=0,'Staged content differs from the approved CSV fingerprints');
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_APPROVED_SOURCE WHERE package_weight IS NULL OR process_weight IS NULL;
 require_true(n=0,'Explicit approved package/process weights and approval references are required');
 LOCK TABLE T_RISK_FAD_KPI_MAIN, T_RISK_FAD_KPI_SUB, T_RISK_FAD_PACKAGE,
 T_RISK_FAD_PROCESS, T_RISK_FAD_SUB_PROCESS, T_RISK_FAD_ANNEXURE_MAP IN EXCLUSIVE MODE;
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_APPROVED_SOURCE;
 require_true(n=121,'Expected 121 staged mappings');
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_APPROVED_SOURCE WHERE
 main_code IS NULL OR package_code IS NULL OR process_code IS NULL OR procedure_code IS NULL OR annexure_code IS NULL
 OR main_name IS NULL OR package_name IS NULL OR process_name IS NULL OR procedure_name IS NULL
 OR violation_description IS NULL OR risk_classification IS NULL OR package_weight IS NULL OR process_weight IS NULL
 OR LENGTHB(main_name)>4000 OR LENGTHB(package_name)>4000 OR LENGTHB(process_name)>4000 OR LENGTHB(procedure_name)>4000
 OR LENGTHB(risk_classification)>200 OR LENGTHB(main_code)>50 OR LENGTHB(package_code)>50
 OR LENGTHB(process_code)>50 OR LENGTHB(procedure_code)>50 OR LENGTHB(annexure_code)>50;
 require_true(n=0,'Missing source values or oversized text; no truncation permitted');
 SELECT COUNT(DISTINCT main_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=4,'Unexpected main_code cardinality');
 SELECT COUNT(DISTINCT package_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=9,'Unexpected package_code cardinality');
 SELECT COUNT(DISTINCT process_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=14,'Unexpected process_code cardinality');
 SELECT COUNT(DISTINCT procedure_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=37,'Unexpected procedure_code cardinality');
 SELECT COUNT(DISTINCT annexure_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=121,'Unexpected annexure_code cardinality');
 -- Print every unresolved or ambiguous row before rejecting the entire import.
 n:=0;
 FOR r IN (SELECT s.annexure_code FROM V_FAD_RISK_APPROVED_SOURCE s WHERE
 (SELECT COUNT(*) FROM T_AUDIT_CHECKLIST_ANNEXURE x WHERE UPPER(TRIM(x.code))=UPPER(TRIM(s.annexure_code))) +
 (SELECT COUNT(*) FROM T_RISK_FAD_CODE_ALIAS al WHERE UPPER(TRIM(al.staged_code))=UPPER(TRIM(s.annexure_code))) <> 1) LOOP
 DBMS_OUTPUT.PUT_LINE('UNRESOLVED / CONFLICT: '||r.annexure_code); n:=n+1; END LOOP;
 require_true(n=0,'Resolve all Annexures via diagnostic and separately approved aliases before migration');
 FOR r IN (SELECT * FROM V_FAD_RISK_APPROVED_SOURCE ORDER BY main_code,package_code,process_code,procedure_code,annexure_code) LOOP
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_GRAVITY WHERE gravity_score=r.gravity_score;
 require_true(n=1,'Unresolved risk classification: '||r.risk_classification);
 SELECT gravity_id INTO g FROM T_RISK_FAD_GRAVITY WHERE gravity_score=r.gravity_score;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_KPI_MAIN WHERE KPI_CODE=r.main_code;
 require_true(n<=1,'Ambiguous main code: '||r.main_code);
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_KPI_MAIN (KPI_MAIN_ID,KPI_CODE,KPI_NAME,IS_ACTIVE)
 VALUES (SEQ_RISK_FAD_KPI_MAIN.NEXTVAL,r.main_code,r.main_name,'Y') RETURNING KPI_MAIN_ID INTO m;
 ELSE
 SELECT KPI_MAIN_ID INTO m FROM T_RISK_FAD_KPI_MAIN WHERE KPI_CODE=r.main_code;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_KPI_MAIN WHERE KPI_MAIN_ID=m AND KPI_NAME=r.main_name AND IS_ACTIVE='Y';
 require_true(n=1,'Historical main conflicts with source: '||r.main_code);
 END IF;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_KPI_SUB WHERE KPI_SUB_CODE=r.package_code;
 require_true(n<=1,'Ambiguous package code: '||r.package_code);
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_KPI_SUB (KPI_SUB_ID,KPI_SUB_CODE,KPI_SUB_NAME,IS_ACTIVE,KPI_MAIN_ID,WEIGHTAGE)
 VALUES (SEQ_RISK_FAD_KPI_SUB.NEXTVAL,r.package_code,r.package_name,'Y',m,r.package_weight) RETURNING KPI_SUB_ID INTO k;
 ELSE
 SELECT KPI_SUB_ID INTO k FROM T_RISK_FAD_KPI_SUB WHERE KPI_SUB_CODE=r.package_code;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_KPI_SUB WHERE KPI_SUB_ID=k AND KPI_SUB_NAME=r.package_name AND IS_ACTIVE='Y' AND KPI_MAIN_ID=m AND WEIGHTAGE=r.package_weight;
 require_true(n=1,'Historical package conflicts with source: '||r.package_code);
 END IF;
 INSERT INTO T_RISK_FAD_PACKAGE(PACKAGE_ID) SELECT k FROM dual WHERE NOT EXISTS (SELECT 1 FROM T_RISK_FAD_PACKAGE WHERE PACKAGE_ID=k);
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_PROCESS WHERE PROCESS_CODE=r.process_code;
 require_true(n<=1,'Ambiguous process code: '||r.process_code);
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_PROCESS (PROCESS_ID,PROCESS_CODE,PROCESS_NAME,IS_ACTIVE,KPI_SUB_ID,WEIGHTAGE,PACKAGE_ID)
 VALUES (SEQ_RISK_FAD_PROCESS.NEXTVAL,r.process_code,r.process_name,'Y',k,r.process_weight,k) RETURNING PROCESS_ID INTO p;
 ELSE
 SELECT PROCESS_ID INTO p FROM T_RISK_FAD_PROCESS WHERE PROCESS_CODE=r.process_code;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_PROCESS WHERE PROCESS_ID=p AND PROCESS_NAME=r.process_name AND IS_ACTIVE='Y' AND KPI_SUB_ID=k AND WEIGHTAGE=r.process_weight AND (PACKAGE_ID IS NULL OR PACKAGE_ID=k);
 require_true(n=1,'Historical process conflicts with source: '||r.process_code);
 END IF;
 UPDATE T_RISK_FAD_PROCESS SET PACKAGE_ID=k WHERE PROCESS_ID=p AND PACKAGE_ID IS NULL;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_SUB_PROCESS WHERE SUB_PROCESS_CODE=r.procedure_code;
 require_true(n<=1,'Ambiguous procedure code: '||r.procedure_code);
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_SUB_PROCESS (SUB_PROCESS_ID,SUB_PROCESS_CODE,SUB_PROCESS_NAME,IS_ACTIVE,PROCESS_ID,GRAVITY_ID)
 VALUES (SEQ_RISK_FAD_SUB_PROCESS.NEXTVAL,r.procedure_code,r.procedure_name,'Y',p,g) RETURNING SUB_PROCESS_ID INTO sp;
 ELSE
 SELECT SUB_PROCESS_ID INTO sp FROM T_RISK_FAD_SUB_PROCESS WHERE SUB_PROCESS_CODE=r.procedure_code;
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_SUB_PROCESS WHERE SUB_PROCESS_ID=sp AND SUB_PROCESS_NAME=r.procedure_name AND IS_ACTIVE='Y' AND PROCESS_ID=p AND GRAVITY_ID=g;
 require_true(n=1,'Historical procedure conflicts with source: '||r.procedure_code);
 END IF;
 SELECT id INTO a FROM (
 SELECT x.id FROM T_AUDIT_CHECKLIST_ANNEXURE x WHERE UPPER(TRIM(x.code))=UPPER(TRIM(r.annexure_code))
 UNION ALL SELECT x.id FROM T_RISK_FAD_CODE_ALIAS al JOIN T_AUDIT_CHECKLIST_ANNEXURE x ON x.id=al.annexure_id WHERE UPPER(TRIM(al.staged_code))=UPPER(TRIM(r.annexure_code)));
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_ANNEXURE_MAP WHERE UPPER(TRIM(source_annexure_code))=UPPER(TRIM(r.annexure_code)) OR annexure_id=a;
 IF n=0 THEN
 INSERT INTO T_RISK_FAD_ANNEXURE_MAP(sub_process_id,annexure_id,source_annexure_code,violation_description,risk_classification)
 VALUES(sp,a,r.annexure_code,r.violation_description,r.risk_classification);
 ELSE
 SELECT COUNT(*) INTO n FROM T_RISK_FAD_ANNEXURE_MAP WHERE UPPER(TRIM(source_annexure_code))=UPPER(TRIM(r.annexure_code))
 AND annexure_id=a AND sub_process_id=sp AND risk_classification=r.risk_classification
 AND DBMS_LOB.COMPARE(violation_description,TO_CLOB(r.violation_description))=0;
 require_true(n=1,'Existing Annexure mapping conflicts: '||r.annexure_code);
 END IF;
 END LOOP;
 -- One atomic commit, including reruns; failures roll back every data change.
 COMMIT;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
