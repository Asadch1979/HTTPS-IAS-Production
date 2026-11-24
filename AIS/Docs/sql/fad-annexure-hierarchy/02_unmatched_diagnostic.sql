-- READ ONLY after source adapter. Only UPPER(TRIM(code)) matches count as resolved here.
SET LINESIZE 250
SET PAGESIZE 50000
SET LONG 1000000
SPOOL fad-unmatched-annexures.txt
SELECT COUNT(*) staged_rows, COUNT(DISTINCT UPPER(TRIM(annexure_code))) staged_codes FROM T_FAD_RISK_MAP_STAGE;
-- Authoritative display status includes duplicate/null operational IDs as unresolved.
SELECT reconciliation_status,COUNT(*) source_rows FROM V_FAD_RISK_REGISTER
GROUP BY reconciliation_status;
SELECT source_annexure_code,procedure_code,procedure_name,violation_description,
       reconciliation_status,annexure_id FROM V_FAD_RISK_REGISTER
WHERE reconciliation_status<>'MATCHED' ORDER BY excel_row;
SELECT COUNT(*) matched_rows, COUNT(DISTINCT a.id) distinct_matched_ias_ids
FROM T_FAD_RISK_MAP_STAGE s JOIN T_AUDIT_CHECKLIST_ANNEXURE a ON UPPER(TRIM(a.code))=UPPER(TRIM(s.annexure_code));
SELECT COUNT(*) unmatched_codes FROM T_FAD_RISK_MAP_STAGE s
WHERE NOT EXISTS (SELECT 1 FROM T_AUDIT_CHECKLIST_ANNEXURE a WHERE UPPER(TRIM(a.code))=UPPER(TRIM(s.annexure_code)));
SELECT s.annexure_code staged_code, s.violation_description, s.procedure_code,
 s.procedure_name, a.code possible_ias_code, a.id existing_ias_annexure_id,
 CASE WHEN a.id IS NULL THEN 'NO FORMATTING CANDIDATE; MANUAL NAMING REVIEW'
 WHEN UPPER(TRIM(s.annexure_code))=UPPER(TRIM(a.code)) THEN 'CASE OR WHITESPACE ONLY'
 WHEN REGEXP_REPLACE(UPPER(s.annexure_code),'[^A-Z0-9]')=REGEXP_REPLACE(UPPER(a.code),'[^A-Z0-9]') THEN 'PUNCTUATION DIFFERENCE'
 ELSE 'LETTER O / DIGIT ZERO CANDIDATE; HUMAN REVIEW REQUIRED' END proposal_reason,
 'UNAPPROVED' approval_status
FROM V_FAD_RISK_APPROVED_SOURCE s
LEFT JOIN T_AUDIT_CHECKLIST_ANNEXURE a
 ON REPLACE(REGEXP_REPLACE(UPPER(s.annexure_code),'[^A-Z0-9]'),'O','0')
  = REPLACE(REGEXP_REPLACE(UPPER(a.code),'[^A-Z0-9]'),'O','0')
WHERE NOT EXISTS (SELECT 1 FROM T_AUDIT_CHECKLIST_ANNEXURE e WHERE UPPER(TRIM(e.code))=UPPER(TRIM(s.annexure_code)))
ORDER BY s.annexure_code,a.code,a.id;
SELECT s.annexure_code,COUNT(a.id) normalized_matches FROM T_FAD_RISK_MAP_STAGE s
LEFT JOIN T_AUDIT_CHECKLIST_ANNEXURE a ON UPPER(TRIM(a.code))=UPPER(TRIM(s.annexure_code))
GROUP BY s.annexure_code HAVING COUNT(a.id)<>1;
SPOOL OFF
