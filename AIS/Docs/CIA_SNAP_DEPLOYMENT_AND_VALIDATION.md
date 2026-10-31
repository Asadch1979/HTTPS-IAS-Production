**F8 note:** Generate temporary PL/SQL Developer SQL Window units with `node scripts/generate-f8-sql.cjs`. The ignored output is in `generated/f8/`; canonical sources remain in `AIS/Docs/sql/`.

# CIA Command Center and Audit Para Snap

Source implementation only. **Oracle compilation, staging reconciliation and authenticated browser acceptance remain required. No production deployment was performed.**

## Manual deployment order

1. Use an explicitly approved staging IAS schema. Back up existing package specifications/bodies and permission rows. Do not point automated validation at production.
2. Verify existing dependencies are valid: `V_IAS_ENTITY_REPORTING_NAMES`, `V_EXEC_ENTITY_SCOPE`, `V_EXEC_PARA_BASE`, `V_EXEC_KPI_LIFECYCLE`, `V_EXEC_REPORT_SOURCE`, `V_IAS_CIA_ISSUE_CHECKLIST_ROOT`, `T_IAS_CIA_ISSUE_NATURE_MAP`, `V_EXEC_ISSUE_RECTIFIED_RECON`, and the existing `PKG_HD.P_GET_ALL_PARA_TEXT`. Their definitions are unchanged by this feature. Some dependency DDL is absent from this repository; obtain the approved installed versions, not invented substitutes.
3. If not already installed/valid, compile `sql/snap_audit_division_scope.sql`: specification, then body. Existing verified audit parents are FAD 112243 and MAD 113254; department IDs are read from IAS mapping, never new guessed IDs.
4. Compile the updated `PKG_EXEC_DASHBOARD` specification, then body. The full `sql/PKG_EXEC_DASHBOARD.sql` contains existing dependency views before the package. Review these before execution. To replace only this package, use **F8** `generated/f8/PKG_EXEC_DASHBOARD/03.sql` (specification) then `04.sql` (body), one unit at a time. Do not reapply unrelated view or Head Audit DDL.
5. Compile `PKG_EXEC_REPORTS`: F8 `generated/f8/PKG_EXEC_REPORTS/01.sql` (specification) then `02.sql` (body), or the reviewed full F5 script.
6. Check `USER_ERRORS` for both packages and validate their dependencies. Compile dependent stored callers if invalidated. Check old callers still execute with optional arguments omitted. Reporting procedures remain read-only and application commands remain bound by name.
7. Register the two exact new GETs through the reviewed `sql/executive_dashboard_permissions.sql`: `/api/AuditParaSnap/GetPerformance` and `/api/AuditParaSnap/GetParaText`, page **1000304**, class **SNAP**, roles **3 (CIA)** and **1 (verified Super User)** only. Existing API and role IDs are discovered/reused. New registrations/grants default to **inactive**; deliberately activate only these approved API/role rows using the established IAS permission administration process. Do not enable unrelated rows or replace disabled grants. No wildcard grant, new database user, broad table privilege or legacy HTTP permission is needed. Existing PKG_HD execute privilege is reused.
8. Existing page/API-manifest provisioning is a prerequisite if that installation uses it. The legacy script references `executive_dashboard_page_api_manifest.sql`, whose source is absent from this checkout; do not fabricate it or deploy an incomplete permission package. Verify effective exact grants with the deployed permission service and fresh sessions.
9. Deploy the application only after the revised packages compile and approved exact grants are effective. Application readers bind the new arguments, so app-first deployment will fail closed until Oracle is updated.

## Amended Oracle contracts

| Object | Optional arguments appended after existing arguments | Behavior |
|---|---|---|
| `PKG_EXEC_DASHBOARD.P_GET_CIA_EXEC_KPIS` | `P_POSITION_AS_ON`, `P_SNAP_DIVISION_CODE` | Actual IAS period retained; inclusive cutoff on dated A1 activity; approved audit portfolio scope |
| `PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE` | Same | Same portfolio and exclusive next-day cutoff; ten-category/mapped/unmapped reconciliation retained |
| `PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE` | `P_SNAP_AUDITED_BY_ID` | Independent organizational Audited By intersection; existing portfolio department remains `P_AUDITED_BY_ID` |
| `PKG_EXEC_DASHBOARD.P_GET_SNAP_DISTRIBUTION` | `P_SNAP_AUDITED_BY_ID` | Adds monthly movement and critical exposure datasets; extra `PARAS_ADDED`, `PARAS_SETTLED` cursor columns |
| `PKG_EXEC_DASHBOARD.P_GET_SNAP_CRITICAL` | `P_SNAP_AUDITED_BY_ID`, `P_COM_ID` | Exact para eligibility before ranking/capping; no capped-list membership shortcut |
| All five `PKG_EXEC_REPORTS` public procedures | `P_SNAP_AUDITED_BY_ID` | Independent selected scope in Reports A–E and PDFs |

No new reporting procedure, underlying reporting calculation or lifecycle view was created. Existing named/positional callers may omit appended arguments. Oracle 18c/19c-compatible SQL is used; actual compilation is not established by source tests.

## Focused local evidence

- `.NET` build succeeds: 0 errors, 57 existing warnings.
- `pwsh -NoProfile -File tests/executive-dashboard/cia-snap-enhancement-policy.ps1`: 52 checks pass (9 CIA defaults/cutoffs, 9 Snap filters/forged IDs, 4 performance rates, 26 controller authorization/role/session/exact-route checks, 4 PDF HTML scope/columns/encoding). Both new endpoints reach date validation only with their exact grants; substituting GetKpis permission denies CIA and Super User.
- `node --test tests/executive-dashboard/cia-command-loading.test.cjs`: 13 actual-client checks pass, including section-local errors, abort/late-result behavior and selected IAS period/portfolio parameters.
- `node --test tests/executive-dashboard/cia-snap-enhancements.test.cjs`: seven checks pass. Covers unchanged unrelated Oracle procedure bodies/lifecycle calculations/models, spec/body signatures, COM_ID predicate before rank, unchanged nine glance KPIs/two charts, actual aging rendering, safe para modal rendering/denials/mismatched COM_IDs and Snap-only report table/CSV-source column removal.
- JavaScript syntax and `git diff --check` pass.
- Broad legacy dashboard suite has **13 pre-existing failures**, reproduced with reads from verified upstream main `8f18681`. They include stale historical signature/presentation checks, missing optional manifest/checklist dependency sources and unrelated Head Audit expectations. Feature-induced structural expectations were updated; no additional failures remain. These baseline failures were not hidden, skipped or repaired through unrelated dashboard changes.
- CSV reads the same rendered report tables as the browser; Snap removes the source columns before rendering. PDF HTML tests verify both headers and row cells are removed only for Snap. No standalone XLSX export exists in this report client; CSV remains Excel-readable.

## Staging acceptance still required

1. CIA: load actual period IDs; test current/historical defaults, Pakistan midnight, Apply preserving date, Reset, future/out-of-period/malformed dates and invalid period IDs. Reconcile FAD/MAD/departments and ten nature source categories including hidden mapped categories and unmapped records. Confirm current-state coverage/workflow labels are understood.
2. Snap: confirm master names/IDs for Group, Division, Department and GM Office; verify dropdown cascades and parent clearing; preserve full scope across all six tabs and Reports A–E. Verify no unauthorized entity/office is advertised or returned.
3. Reconcile each comparison row against the same scoped lifecycle source. Compare independent division/overall totals; retain displayed residuals for division-direct/other-descendant portfolios. Confirm query latency with the approved child counts. Multiple lifecycle reads are bounded but not a transaction-consistent snapshot; live changes can produce differences.
4. Sum monthly additions/settlements against selected-period KPIs, including status 9 and same-period additions/settlements. Verify rate/clearance/ranking N/A for zero/missing/inconsistent denominators. Estimates remain explicitly labeled. No proven inactivity is inferred from missing history.
5. Critical register: verify capped 100-record label; authorized COM_ID full text; wrong division/department/entity/date/view, non-critical/ineligible COM_ID, missing exact grant, revoked session and wrong role all deny without returning another para's text. Test stored HTML/script strings remain text in the read-only dialog.
6. Aging: percentages only when distribution equals closing outstanding; zero/unknown denominator or insufficient history remains N/A. Verify unknown/zero cycles and over-three-year buckets.
7. Reports: inspect A–E browser/CSV/PDF layouts with long office labels; verify the three settlement-source columns are absent for Snap. CIA cannot export; only verified Super User with exact page/report API grants can export. Unrelated report presentations must retain their existing columns.
8. Exercise direct forged API calls with CIA, Super User and unauthorized roles and all unchanged dashboard entry points. Refresh permission sessions after grant activation. Do not weaken security to obtain successful data.

## Limitations

- Coverage and workflow KPIs retain their current-state definitions because reliable historical workflow reconstruction is not established. A1 and nature activity dates use the selected cutoff.
- Historical balances/risk/organization remain estimates using approved source definitions. Reopenings and incomplete history can alter past stock.
- Department rankings are indicative, exclude inconsistent denominators and arithmetic differences, and are not certified performance rankings.
- The para modal intentionally renders stored markup literally as plain text. It never injects the stored HTML.
- Live Oracle, browser/session and PDF visual acceptance are not proven by local unit/source tests. No database changes should be described as compiled or production-ready until the staging steps above pass.

## Modified files

- `AIS/Controllers/AuditParaSnapController.cs`
- `AIS/Controllers/ExecutiveOverviewController.cs`
- `AIS/DBConnection.AuditParaSnap.cs`
- `AIS/DBConnection.AuditParaSnapPerformance.cs`
- `AIS/DBConnection.CiaCommandCenter.cs`
- `AIS/DBConnection.ExecutiveDashboard.cs`
- `AIS/DBConnection.ExecutiveDashboardFilters.cs`
- `AIS/DBConnection.ExecutiveDashboardReports.cs`
- `AIS/Docs/CIA_SNAP_DEPLOYMENT_AND_VALIDATION.md`
- `AIS/Docs/CODEX_DASHBOARD_PROGRESS.md`
- `AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql`
- `AIS/Docs/sql/PKG_EXEC_REPORTS.sql`
- `AIS/Docs/sql/executive_dashboard_permissions.sql`
- `generated/f8/PKG_EXEC_DASHBOARD/03.sql`
- `generated/f8/PKG_EXEC_DASHBOARD/04.sql`
- `generated/f8/PKG_EXEC_REPORTS/01.sql`
- `generated/f8/PKG_EXEC_REPORTS/02.sql`
- `AIS/Models/AuditParaSnapData.cs`
- `AIS/Models/AuditParaSnapPerformance.cs`
- `AIS/Models/CiaCommandCenterData.cs`
- `AIS/Models/ExecutiveDashboardFilters.cs`
- `AIS/Services/ExecutiveReportsPdfGenerator.cs`
- `AIS/Views/ExecutiveOverview/CIA.cshtml`
- `AIS/Views/ExecutiveOverview/_AuditParaSnapAnalysis.cshtml`
- `AIS/Views/ExecutiveOverview/_AuditParaSnapFilters.cshtml`
- `AIS/Views/ExecutiveOverview/_CiaCommandFilters.cshtml`
- `AIS/wwwroot/css/audit-para-snap.css`
- `AIS/wwwroot/js/audit-para-snap.js`
- `AIS/wwwroot/js/cia-command-center.js`
- `AIS/wwwroot/js/executive-reports.js`
- `tests/executive-dashboard/AuditParaSnapAccessChecks.cs`
- `tests/executive-dashboard/audit-snap-division-scope.test.cjs`
- `tests/executive-dashboard/cia-checklist-nature.test.cjs`
- `tests/executive-dashboard/cia-command-loading.test.cjs`
- `tests/executive-dashboard/cia-snap-enhancement-policy.ps1`
- `tests/executive-dashboard/cia-snap-enhancements.test.cjs`
- `tests/executive-dashboard/executive-role-permissions.test.cjs`
- `tests/executive-dashboard/executive-source-alignment.test.cjs`
- `tests/executive-dashboard/role-parent-client.test.cjs`
