-- Hierarchy-only transition: READ ONLY, no inserts/updates to legacy weighted masters.
-- Retained filename for rollout compatibility; this script reports source prerequisites.
-- Dedicated session with no pending work. Read-only transaction gives a consistent snapshot.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
SET SERVEROUTPUT ON
SET TRANSACTION READ ONLY;
DECLARE
 expected_source_sha256 CONSTANT VARCHAR2(64) := '9e5f375ba0a8df2339edc3374307aa049ac2975580ea0f206d31bbb8090b2425';
 n NUMBER;
 PROCEDURE require_true(ok BOOLEAN, message VARCHAR2) IS
 BEGIN IF ok IS NULL OR NOT ok THEN RAISE_APPLICATION_ERROR(-20101,message); END IF; END;
BEGIN
 SELECT COUNT(*) INTO n FROM T_FAD_RISK_MAP_STAGE
 WHERE SOURCE_FILE_SHA256 IS NULL
    OR NOT REGEXP_LIKE(TRIM(SOURCE_FILE_SHA256), '^[0-9a-fA-F]{64}$')
    OR LOWER(TRIM(SOURCE_FILE_SHA256)) <> expected_source_sha256;
 require_true(n=0, 'Source CSV SHA-256 mismatch, missing hash or mixed source batch');
 -- Independently compare every source field with fingerprints authored from the CSV.
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_SOURCE_FINGERPRINT e
 FULL OUTER JOIN V_FAD_RISK_STAGE_FINGERPRINT s ON s.excel_row=e.excel_row
 WHERE e.excel_row IS NULL OR s.excel_row IS NULL OR s.row_sha256<>e.row_sha256 OR s.row_sha256 IS NULL;
 require_true(n=0,'Staged content differs from the approved CSV fingerprints');
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_APPROVED_SOURCE;
 require_true(n=121,'Expected 121 staged mappings');
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_APPROVED_SOURCE WHERE
 main_code IS NULL OR package_code IS NULL OR process_code IS NULL OR procedure_code IS NULL OR annexure_code IS NULL
 OR main_name IS NULL OR package_name IS NULL OR process_name IS NULL OR procedure_name IS NULL
 OR gravity_score IS NULL OR violation_description IS NULL OR risk_classification IS NULL
 OR LENGTHB(main_name)>4000 OR LENGTHB(package_name)>4000 OR LENGTHB(process_name)>4000 OR LENGTHB(procedure_name)>4000
 OR LENGTHB(risk_classification)>200 OR LENGTHB(main_code)>50 OR LENGTHB(package_code)>50
 OR LENGTHB(process_code)>50 OR LENGTHB(procedure_code)>50 OR LENGTHB(annexure_code)>50;
 require_true(n=0,'Missing source values or oversized text; no truncation permitted');
 SELECT COUNT(DISTINCT main_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=4,'Unexpected main_code cardinality');
 SELECT COUNT(DISTINCT package_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=9,'Unexpected package_code cardinality');
 SELECT COUNT(DISTINCT process_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=14,'Unexpected process_code cardinality');
 SELECT COUNT(DISTINCT procedure_code) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=37,'Unexpected procedure_code cardinality');
 SELECT COUNT(DISTINCT UPPER(TRIM(annexure_code))) INTO n FROM V_FAD_RISK_APPROVED_SOURCE; require_true(n=121,'Unexpected annexure_code cardinality');

 -- Detect contradictory source descriptions and parents rather than choosing one.
 SELECT COUNT(*) INTO n FROM (
 SELECT main_code FROM V_FAD_RISK_APPROVED_SOURCE GROUP BY main_code HAVING COUNT(DISTINCT main_name)<>1
 UNION ALL SELECT package_code FROM V_FAD_RISK_APPROVED_SOURCE GROUP BY package_code
 HAVING COUNT(DISTINCT main_code)<>1 OR COUNT(DISTINCT package_name)<>1
 UNION ALL SELECT process_code FROM V_FAD_RISK_APPROVED_SOURCE GROUP BY process_code
 HAVING COUNT(DISTINCT package_code)<>1 OR COUNT(DISTINCT process_name)<>1
 UNION ALL SELECT procedure_code FROM V_FAD_RISK_APPROVED_SOURCE GROUP BY procedure_code
 HAVING COUNT(DISTINCT process_code)<>1 OR COUNT(DISTINCT procedure_name)<>1);
 require_true(n=0,'Conflicting staged hierarchy parents or descriptions');
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_REGISTER;
 require_true(n=121,'Register must preserve all 121 source rows');
 FOR r IN (SELECT source_annexure_code,reconciliation_status FROM V_FAD_RISK_REGISTER
 WHERE reconciliation_status<>'MATCHED' ORDER BY excel_row) LOOP
 DBMS_OUTPUT.PUT_LINE(r.reconciliation_status||': '||r.source_annexure_code);
 END LOOP;
 SELECT COUNT(*) INTO n FROM V_FAD_RISK_REGISTER WHERE reconciliation_status<>'MATCHED';
 DBMS_OUTPUT.PUT_LINE('121 hierarchy rows retained; '||n||' Annexures pending reconciliation.');
 DBMS_OUTPUT.PUT_LINE('No weights or legacy scoring/master records were written.');
 ROLLBACK;
EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/
