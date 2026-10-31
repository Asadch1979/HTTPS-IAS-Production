# IAS Oracle SQL canonical source

Use this folder's maintained `PKG_*.sql` files as the **repository source of record**. Verify every production database object against its installed source before deployment. Do not assume that a checked-in file has already been compiled in Oracle.

- **Executive dashboard:** `PKG_EXEC_DASHBOARD.sql`, `PKG_EXEC_REPORTS.sql`, `PKG_HEAD_AUDIT_MONITOR.sql`.
- **CCO Function Wise Position:** uses the existing `PKG_EXEC_REPORTS.P_MANAGEMENT_SETTLEMENT`. Retired `PKG_CCO_SCOPE`, `PKG_CCO_FUNCTION_POSITION`, and `PKG_CCO_REPORTING` are no longer maintained here. After authenticated application cutover, use `CCO_SCOPE_RETIREMENT.sql` for guarded DBA removal of unused Oracle objects.
- **Audit packages:** `PKG_AR.sql`, `PKG_HD.sql`, `PKG_FRPT.sql`, `PKG_LG.sql`, `PKG_RPT.sql`.
- **Scheduler:** `PKG_IAS_SCHEDULER.sql`, database worker, and the distinct deployment/verification scripts.

## SQL Developer F8 on demand

Run `node scripts/generate-f8-sql.cjs` from the repository root. This generates separate numbered SQL Window units in ignored `generated/f8/<package>/NN.sql`, outside `AIS/Docs`. Review and execute one unit at a time in an approved schema. The generator does **not** access Oracle. Source scripts remain authoritative.

The useful **read-only** extraction and validation scripts have been retained directly in this folder as `extract_pkg_rpt_*.sql`, `validate_errors.sql`, and `validate_status.sql`.

Old `- Prod` snapshots, dated notes and alternate body/review copies can be recovered through Git history. Avoid deploying any script based only on its filename.
