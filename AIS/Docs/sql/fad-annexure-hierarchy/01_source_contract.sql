-- DDL adapter: after schema review, supply actual staging COLUMN NAMES.
-- No defaults: descriptions, risk and weights must come from approved staging.
-- Supplied columns must be scalar VARCHAR2/NUMBER, not expressions or CLOBs.
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK
ACCEPT main_name_column CHAR PROMPT 'Staging Main KPI description column: '
ACCEPT package_name_column CHAR PROMPT 'Staging Package description column: '
ACCEPT process_name_column CHAR PROMPT 'Staging Process description column: '
ACCEPT procedure_name_column CHAR PROMPT 'Staging full Procedure description column: '
ACCEPT violation_column CHAR PROMPT 'Staging full violation description column: '
ACCEPT risk_column CHAR PROMPT 'Staging risk classification column (exact gravity name): '
ACCEPT package_weight_column CHAR PROMPT 'Staging approved package weight column: '
ACCEPT process_weight_column CHAR PROMPT 'Staging approved process weight column: '
CREATE OR REPLACE VIEW V_FAD_RISK_APPROVED_SOURCE AS
SELECT IAS_MAIN_KPI_CODE main_code, IAS_SUB_KPI_CODE package_code,
 IAS_PROCESS_CODE process_code, IAS_SUB_PROCESS_CODE procedure_code, ANNEXURE_CODE,
 &main_name_column main_name, &package_name_column package_name,
 &process_name_column process_name, &procedure_name_column procedure_name,
 &violation_column violation_description, &risk_column risk_classification,
 &package_weight_column package_weight, &process_weight_column process_weight
FROM T_FAD_RISK_MAP_STAGE;
