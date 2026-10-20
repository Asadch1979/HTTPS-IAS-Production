# Dashboard enhancement checkpoint

Branch: `main`. Application changes and focused checks are complete for the work
listed below. Production deployment and the production IAS Para Position package
change are **not** complete.

## Implemented

- Executive dashboard pages and Audit Teams Monitoring have section PNG, PDF and
  XLSX exports. The shared client captures the current rendered section, expands
  scrolling containers in a detached copy, excludes export controls, and includes
  literal, usable table data and KPI sheets in Excel. No new data/export API or
  permission grant is introduced. Existing report export actions remain available.
- Dashboard titles share the CCO typography and display the authorized scope.
  Page colors and existing layout components remain in place.
- CCO dates are required on every analysis view and preserved in navigation.
  The existing scoped API actions use the lifecycle cursor for activity/balance
  KPIs and the existing dated distribution/critical cursors for charts and lists.
  Period settlement cards are labelled as period activity. Historical balances
  remain explicitly estimated; unknown historical cycles remain an unknown bucket.
- Snap supports the VARCHAR2 value `'1'` for All Divisions. Both parent audit
  divisions and their reporting descendants are combined through `EXISTS`, not
  fact-multiplying joins. All + Department is rejected server-side; the selector
  clears/disables Department for All. Existing CIA/Super User/page/API checks stay.
- Monitoring admits an active authenticated posting with explicit page allocation,
  rather than a hardcoded head-role list. Every database reader checks the actual
  allocated menu pages, including Super User. Oracle independently checks the active
  context and the user's entity in `V_EXEC_ENTITY_SCOPE`. Engagement membership
  follows the audit-performing entity's own/descendant hierarchy. Review/document
  ownership checks remain; out-of-scope documents return 403.
- IAS Para Position defaults use Pakistan dates, January 1 through yesterday (the
  prior full year on January 1). Client, API and database-access validation enforce
  2024-01-01 as the minimum, ordered dates and no future date.
- The existing lifecycle column order is preserved. All now displays the returned
  authorized table as well as summary cards. DataTables hides Reporting Office and
  Audit Entity ID, including exports, and restores them on a specific selection.
  Footer colspans track visibility.

## Manual Oracle deployment

Back up the target package/view definitions and compare them with the repository
before deployment. These sources must be reviewed and compiled in a non-production
database first; no Oracle connection or production DDL was executed by this task.
Deploy database changes before the corresponding application changes.

1. `sql/snap_audit_division_scope.sql` — All Divisions validation and empty child
   selector for All. Existing FAD/MAD hierarchy must already be valid.
2. `sql/PKG_EXEC_DASHBOARD.sql` — union-of-divisions predicates; allow the existing
   dated distribution/critical procedures for the already-authorized CCO perspective;
   return dated high-risk bucket counts and critical chronology metadata.
   Existing `V_EXEC_KPI_LIFECYCLE` and compliance-history dependencies must be present.
3. `sql/PKG_EXEC_REPORTS.sql` — apply the same division predicate to Reports A–E.
   Existing report-specific definitions and authorization remain intact.
4. `sql/PKG_HEAD_AUDIT_MONITOR.sql` — entity/descendant engagement scope and active
   assignment checks. `V_HEAD_AUDIT_ROLE_DEPT` remains for compatibility but is no
   longer the portfolio or permission source.
5. `sql/head_audit_dashboard_menu.sql` — moves the existing registered page 1000401
   under the existing Dashboard menu. Fails if registration is missing/ambiguous.
   It creates no page allocations, API permissions or role grants.

Check `USER_ERRORS` and `USER_OBJECTS.STATUS` after every compilation. These are
Oracle DDL scripts: rollback does not undo successful earlier DDL. Restore captured
definitions if deployment fails. Use the existing IAS administration workflow to
allocate page/API access; this change intentionally creates no grants.

## Exact remaining blocker: production IAS Para Position

The user identified `AIS/Docs/sql` as the source directory. A case-insensitive
search of that entire directory found `R_IAS_PARA_POSITION` only in the explicitly
marked **legacy standalone** `R_IAS_PARA_POSITION.sql`. The checked-in
`PKG_RPT.sql` does not contain the procedure. Neither file was substituted for the
production package, and no production KPI or authorization predicate was guessed.

Run the **read-only** `sql/PKG_RPT_IAS_PARA_POSITION_preflight.sql` in the application
schema to capture the actual production specification/body and argument signature.
It performs no replacement or grants. Resume from those captured files:

1. Identify the exact authorized entity set and All-selection grouping in the
   production member procedure. Preserve its user/role/entity predicates.
2. Update only that member's KPI expressions to consume the approved
   `V_EXEC_KPI_LIFECYCLE` semantics and preserve its output aliases:
   one eligible modern issue per observation; rectification recognized using
   `ISSUE_RECTIFIED_ON` (the original PARA_ADDED_ON cohort alias); additions include
   eligible status 8/9 paras, including same-period settlements; exclude modern
   pre-finalization rectifications from para balances. Reuse the dashboard's
   opening/as-of/settlement rules verbatim, including their historical limitations.
3. Ensure All returns entity-type grouped, scoped results for every eligible role.
   The client currently displays the authorized rows supplied by the deployed
   procedure; it cannot safely infer missing type/scope metadata.
4. Produce a reviewed package-body deployment patch against the captured version,
   retaining every other package member. Never deploy the standalone reference.
5. Compare selected-period results with the executive lifecycle cursor over the
   exact same authorized entity set, including additions settled in the same
   period, duplicate observation mappings, start/end boundaries and rectified issues.

Do not repeat the completed frontend work when that source becomes available.

## Verification performed

- `dotnet build AIS/AIS.csproj --no-restore`: 0 errors; existing warnings.
- `dotnet run --project tests/head-audit-monitoring/HeadAuditMonitoring.Checks.csproj`:
  role/posting prerequisites, authenticated GET surface, no-store, no client
  authority parameters, minimum/future/reversed dates, January boundary, Snap All
  validation and CCO date validation passed. Existing assembly-conflict warnings.
- `tests/dashboard-enhancements/export.browser.cjs`: isolated Chrome fixture,
  full scrolling section PNG, multi-page PDF, XLSX image and all 35 rows, hidden
  columns excluded, formula-like strings remain literal, live layout unchanged,
  export blocked while loading. PNG visually inspected.
- `tests/dashboard-enhancements/report.browser.cjs`: real bundled DataTables,
  All/specific column restoration, export columns, footer colspans, column sequence,
  and invalid-date requests blocked.
- `tests/dashboard-enhancements/compliance.browser.cjs`: all six analysis requests
  carry dates, undated responses are rejected, labels and navigation persist dates.
- Existing lifecycle, as-of-cycle and division-scope tests: 16 tests passed.

Browser checks use `playwright` on `NODE_PATH` and installed Chrome (or
`TEST_BROWSER=msedge`). Fixtures contain synthetic data only.

Still required in the target Oracle environment: compile scripts, reconcile
selected-period data, verify no duplicate combined division facts, and exercise
allocated/unallocated users, sibling entities, descendant engagements, mismatched
document IDs and expired assignments through actual HTTP sessions. These were not
represented as live integration tests.

## Credit/progress handling

The 5-hour balance was checked during execution. It reset during the task before
reaching the 2% stop threshold. No credit-triggered pause or resumption automation
was needed. The remaining dependency is the production package source, not credit.

Pre-existing local changes to `AIS/appsettings.json`, `AIS/obj/*` and `.vs/` were
left out of task commits.
