# Hierarchy-only FAD Risk Register

The register displays the approved staging hierarchy independently of scoring weights. No database scripts or testing activities were executed for this correction. Deployment readiness is not asserted.

## Cause and correction

The previous adapter joined T_FAD_RISK_WEIGHT_APPROVAL and the migration required 9 package and 14 process weight approvals before importing into legacy mandatory-weight tables. That made a hierarchy-only display depend on scoring configuration and caused ORA-20101 when the 23 approvals were absent.

The replacement uses read-only views over T_FAD_RISK_MAP_STAGE. It does not create, join, populate or require the weight-approval table. It does not insert zero, NULL or inferred weights into any master. Existing weights, approvals, master records, package/process foreign keys, scoring formulas and assessment workflows are preserved.

## Source and application contract

01_source_contract.sql defines V_FAD_RISK_APPROVED_SOURCE using the actual staging columns. The four IAS code fields identify Main KPI, Package, Process and Procedure. Package descriptions come from RESOLVED_PACKAGE; procedure and violation descriptions are returned completely from RISK_RATING_PROCEDURE_EXCEL_TEXT and VIOLATION_BANK_INSTRUCTIONS. Process labels use PROCESS_NUMBER. The four Main KPI descriptions retain the original mapping pack's Rating Matrix headings. SOURCE_RISK_LEVEL and RATING_MATRIX_GRAVITY_SCORE are displayed as source attributes; no gravity master lookup or scoring inference is required.

V_FAD_RISK_REGISTER retains one row for every staged Annexure. Aggregated operational-code matches use UPPER(TRIM(code)). An IAS ID is exposed only if the normalized code has exactly one operational row and its non-null ID occurs exactly once in the operational table. Missing codes, duplicate codes, null IDs and duplicate IDs are explicitly unresolved. No candidate ID is assigned to an ambiguous match. This is a read-only join, not a physical foreign-key relationship, so an operational ID constraint is not a prerequisite for display. The operational table is not altered.

PKG_FAD_RISK.P_GET_REGISTER reads that view, after rejecting source SHA/content drift against the existing approved fingerprints. The expected CSV SHA-256 remains 9e5f375ba0a8df2339edc3374307aa049ac2975580ea0f206d31bbb8090b2425. The existing source fingerprints include all 16 CSV fields; they are not replaced with the workbook digest. The catalog must retain 4 Main KPIs, 9 Packages, 14 Processes, 37 Procedures and 121 Annexures.

The API now identifies hierarchy nodes by source codes, not numeric legacy master IDs. DBConnection preserves nullable operational IDs and reconciliation status. The independent hierarchy JavaScript uses those codes for cascade selections and shows all pending Annexures in a separate list plus per-row status. Complete procedure/violation text remains visible. The expected current baseline is 114 matches and seven pending rows; counts are calculated from the operational data, not hardcoded or asserted as locally observed.

The existing five configuration tabs, their JavaScript/save routes, Risk Assessment link, controller authorization/session/API-permission checks and scoring logic are unchanged by this correction. Only the register read procedure and independent hierarchy display change. A pending Annexure remains blocked from operational use but no longer blocks browsing the complete hierarchy.

## Controlled application of prepared scripts

1. Review the live staging schema and package against the repository using 00_schema_review.sql. Preserve live-only package members and existing permissions. This work used local source evidence, not live database access.
2. Apply the revised 01_source_contract.sql to replace the source adapter and create the read-only register view. It has no weighted-master prerequisites. CREATE OR REPLACE VIEW is DDL and commits implicitly; schedule the package/view update together. Existing external consumers of removed package_weight/process_weight view columns need separate review.
3. Retain or apply 03_source_fingerprints.sql for the approved CSV contract. No source data reload or change is performed by these scripts.
4. The former 10_schema.sql is retired and now contains comments only. Do not rerun the old master-extension DDL for hierarchy display. Already-created extension/approval tables remain untouched; no cleanup or rollback of historical configuration is attempted.
5. 20_migrate.sql now performs a read-only hierarchy transition report in a dedicated transaction. It checks SHA-256 provenance and row fingerprints, required source values, cardinality and consistent parents/descriptions. It reports every unresolved Annexure while retaining all 121 rows. It performs no inserts, updates, deletes, sequence allocation or weight substitution. Its final ROLLBACK ends the dedicated read-only snapshot, not a data migration. Do not use a session with pending unrelated work.
6. Apply the reviewed package change and application files together. All package members other than P_GET_REGISTER remain unchanged, including their existing legacy schema dependencies. This correction does not provision or remove those legacy dependencies. Existing RegisterHierarchy permission registration remains in force; use 30_api_registration.sql only if that API has never been registered, following normal IAS administration.
7. Use 02_unmatched_diagnostic.sql and RECONCILIATION.md for the seven pending mappings. No alias or operational-table change is automatically applied. 04_annexure_id_dependency.sql is an optional historical physical-FK report, not a requirement for the read-only hierarchy.

Weights remain a separate scoring configuration responsibility. Their absence has no effect on hierarchy visibility. All scripts are prepared for subsequent controlled execution only; no production changes, observation/Post-Compliance changes or testing activities are included.
