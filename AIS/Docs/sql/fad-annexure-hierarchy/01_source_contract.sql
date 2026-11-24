-- DDL only: hierarchy display has no dependency on scoring weights or legacy masters.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
CREATE OR REPLACE VIEW V_FAD_RISK_APPROVED_SOURCE AS
SELECT s.IAS_MAIN_KPI_CODE main_code, s.IAS_SUB_KPI_CODE package_code,
 s.IAS_PROCESS_CODE process_code, s.IAS_SUB_PROCESS_CODE procedure_code, s.ANNEXURE_CODE,
 CASE s.IAS_MAIN_KPI_CODE
 WHEN 'RRM_PORTFOLIO' THEN q'~Portfolio Management
 (Lending/ Credit Administration/ Recovery/ Collection & Default Management)~'
 WHEN 'RRM_ACHIEVEMENT' THEN q'~Business Achievement~'
 WHEN 'RRM_OPERATIONS' THEN q'~General Banking Operations & Management (Security & Cash Operations, Account Opening, Cheque Book/ Debit Card Management, Receipt/ Payment Operations & General Controls)~'
 WHEN 'RRM_FRAUD' THEN q'~Fraud & Forgery, Embezzlement, Impersonation, Parallel Banking etc. (A-1, I-4)~'
 END main_name,
 s.RESOLVED_PACKAGE package_name,
 'Process #' || TO_CHAR(s.PROCESS_NUMBER,'FM999') process_name,
 s.RISK_RATING_PROCEDURE_EXCEL_TEXT procedure_name,
 s.VIOLATION_BANK_INSTRUCTIONS violation_description,
 s.SOURCE_RISK_LEVEL risk_classification,
 s.RATING_MATRIX_GRAVITY_SCORE gravity_score,
 s.SOURCE_FILE_SHA256 source_sha256,
 s.EXCEL_ROW, s.PROCEDURE_GROUP_EXCEL_ROW, s.RATING_MATRIX_ROW,
 s.SOURCE_PACKAGE_CELL, s.PROCESS_NUMBER
FROM T_FAD_RISK_MAP_STAGE s
WITH READ ONLY;

-- One output row per source row, even for missing or ambiguous operational matches.
-- Only the established UPPER(TRIM(code)) comparison resolves codes.
-- MIN is exposed only when both the code and the actual ID are unique.
CREATE OR REPLACE VIEW V_FAD_RISK_REGISTER AS
WITH code_matches AS (
 SELECT UPPER(TRIM(CODE)) normalized_code, COUNT(*) match_count,
        MIN(ID) candidate_id, MIN(CODE) candidate_code
 FROM T_AUDIT_CHECKLIST_ANNEXURE GROUP BY UPPER(TRIM(CODE))
), id_matches AS (
 SELECT ID, COUNT(*) id_count FROM T_AUDIT_CHECKLIST_ANNEXURE
 WHERE ID IS NOT NULL GROUP BY ID
)
SELECT s.main_code,s.main_name,s.package_code,s.package_name,
 s.process_code,s.process_name,s.procedure_code,s.procedure_name,
 s.annexure_code source_annexure_code,
 CASE WHEN c.match_count=1 AND i.id_count=1 THEN c.candidate_id END annexure_id,
 CASE WHEN c.match_count=1 AND i.id_count=1 THEN c.candidate_code END annexure_code,
 CASE WHEN c.match_count IS NULL THEN 'PENDING_RECONCILIATION'
      WHEN c.match_count<>1 THEN 'CONFLICTING_IAS_CODE'
      WHEN c.candidate_id IS NULL THEN 'INVALID_IAS_ID'
      WHEN i.id_count<>1 THEN 'CONFLICTING_IAS_ID'
      ELSE 'MATCHED' END reconciliation_status,
 NVL(c.match_count,0) ias_match_count,
 s.violation_description,s.risk_classification,s.gravity_score,
 s.source_sha256,s.excel_row,s.procedure_group_excel_row,s.process_number
FROM V_FAD_RISK_APPROVED_SOURCE s
LEFT JOIN code_matches c ON c.normalized_code=UPPER(TRIM(s.annexure_code))
LEFT JOIN id_matches i ON i.id=c.candidate_id
WITH READ ONLY;
