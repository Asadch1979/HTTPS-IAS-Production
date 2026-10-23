# Head Audit tabs and secure para display — review and debugging

Source-only implementation on `codex/head-audit-tabs-and-para-format`. No merge, application deployment or database deployment was performed.

## Behavior

- The actual IAS audit-period ID and existing period-selection rules remain in use. Position as on defaults to Pakistan yesterday for a current period, or the end of a completed period. Server and Oracle source reject dates outside the selected period or in the future. As in CIA, if yesterday precedes a period that starts today, an explicit valid cutoff is required; the source never silently reports outside that period.
- Four server-selected tabs share the period and cutoff. Register office/status/exception filters continue to narrow only the register; the other tables retain the existing authorized period portfolio. These filters survive tab links and Review Saved Work navigation.
- Completed stages remain 14, ongoing 4–13, pending reports 12/13, conducting teams 4–11, and delays use the recorded audit end date. Approved plan statuses remain 1/2/4. Conclusion, rather than report issuance, defines completion. Unknown history has a separate plan count; it is not asserted to be not-started activity.
- Plan totals group by `PlanId`, including approved plans with no engagement. Days count once per plan. Missing/conflicting saved days produce an unavailable total, rather than a fabricated zero. Distinct engagement count is separate.
- Each of the five dashboard tables exports the rendered authorized rows to XLSX with heading, period ID/description, cutoff, scope, applied register filters, historical limitations and any total row. ExcelJS is the existing local 4.4.0 asset. Numeric/date cells are typed; formula-like text remains inert strings. No export data endpoint or shared Super User export-policy change was introduced.
- Para Text uses the existing secured `GetParaText` API, returning display-sanitized `html` instead of raw `text`. The HM-style 90% dialog has a red heading, formatted document area, long-body scrolling and read-only controls. `ParaDisplayHtml` removes active content and URL attributes; only approved paragraphs, headings, emphasis, lists, tables, line breaks and a small set of presentation CSS properties survive. Table spans are bounded. Existing COM_ID eligibility checks before/after retrieval, date/division/department/entity scope, session revocation, aborts and clearing remain in place. The legacy HTTP endpoint is never used.

## Historical evidence and limits

History columns were checked against repository workflow writers, including `PKG_AIS` engagement updates and `PKG_HD` report/conclusion transitions. The source selects the latest log at or before the Pakistan cutoff end of day, ordered by `NVL(LAST_UPDATED_ON, CREATED_ON)` and then log ID. `LAST_UPDATED_ON` matters because some writers copy the engagement's original creation date into `CREATED_ON`.

Reversal code in `PKG_AD` writes log status 10 without recording the resulting stage, and can store arbitrary remarks. Therefore historical log status 10 is conservatively Unknown, as are reversal-labelled events. Status 14 requires the recorded `AUDIT CONCLUDED` evidence; no fallback to today's master status is used for missing cutoff history. NULL cutoff is retained only for compatibility with existing database callers and baseline comparison; the dashboard supplies a validated cutoff.

The log is not a complete temporal database. Deleted engagements, missing or inaccurate events, plan approval/membership history, office/team mappings, plan days and schedule revisions cannot be fully reconstructed. Observation reviews, task/workpaper and report summaries are current saved state. These limitations are displayed and exported. Current approved plans are classified by the available engagement evidence, and this must not be represented as a fully verified historical snapshot. Live workflow reconciliation has **not** been performed.

## Selective data access

The selected tab is validated in MVC and passed into `GetHeadAuditDashboard`; other sections are not rendered and their readers are not invoked.

| Tab | Oracle readers, including period validation |
| --- | --- |
| Audit Plan Progress | Periods, office summary, distinct active team IDs |
| Audit Plan Details | Periods, plan details |
| Pending Actions & Exceptions | Periods, engagements with `P_DETAILS=0`; no workpaper, member-list, task/observation-summary or report-summary reads |
| Engagement Register | Periods, scoped office options, engagements, workpaper counts restricted to the selected period |

A carried office filter additionally requires a cheap scoped-office validation where needed. It does not cause the summary/plan datasets to load. Previously every dashboard request loaded periods, office summary, engagements, all-period workpaper counts and plan details together.

Tabs load on demand using the existing authorized MVC page, so no new API grant is necessary. Visited DOM stays only in page memory. A cache revisit performs an authorization-only GET that skips menus and Oracle datasets. An opaque server context header is compared with the initial page context before cached/new content or exports are used; a session/role/entity/context change requires a reload. Filter submissions reload and reset memory. Full-page links remain available without JavaScript. Live Oracle execution timings/trace counts remain a UAT check, not a claimed benchmark.

## Database prerequisites and compilation order

Do not run the deployment files against production as part of this task. Use a controlled test database and the existing deployment review process.

1. Existing IAS scope/security objects must already be valid: `V_EXEC_ENTITY_SCOPE`, user/context/role tables, auditee entities, audit periods, plans/engagements/statuses, observation and report sources used by the existing package. Workpaper V2 tables remain optional as before.
2. Verify `T_AU_PLAN_ENG_LOG` has `ID`, `E_ID`, `STATUS_ID`, `CREATED_ON`, `LAST_UPDATED_ON`, `REMARKS`. Verify the DATE values written using `SYSDATE` represent Pakistan wall-clock time before treating them as Pakistan reporting evidence. Confirm Oracle supports the existing `LISTAGG ... ON OVERFLOW TRUNCATE` and scalar `FETCH FIRST 1 ROW ONLY` syntax (19c is the intended review target).
3. Review `AIS/Docs/sql/PKG_HEAD_AUDIT_MONITOR.sql`, or execute its synchronized F8 units **01, 02, 03, 04, 05, 06 in order**. They are respectively the legacy role view, office scope view, engagement scope view, package specification, package body and compilation-error check. Execute each F8 file as one unit in PL/SQL Developer. Do not paste the combined script into one F8 selection. No other dashboard package changes are required.
4. Inspect `USER_ERRORS` and verify all package/views are VALID. The new application expects the revised package signatures, new lightweight readers and `UNKNOWN_PLANS` result column. Install/verify the test package before running the revised application there. Existing execute grants on the package remain sufficient; do not add broad grants or weaken scope checks.
5. Review execution plans/statistics and the log lookup indexes with the DBA. An index supporting `E_ID`, the effective event date and ID may help, but no automatic index DDL is included. Measure the actual Oracle reads on each tab before accepting a performance claim.

## Local verification performed

- `dotnet build AIS/AIS.csproj --no-restore -v minimal`: succeeded, zero errors and 57 existing warnings (same count as the initial build).
- Build `tests/head-audit-monitoring/HeadAuditMonitoring.Checks.csproj`, then run `dotnet --roll-forward LatestMajor tests/head-audit-monitoring/bin/Debug/net8.0/HeadAuditMonitoring.Checks.dll`: passed policy/date/total/sanitizer tests plus 10 compiled controller gates and Oracle-free authorization probes. This host has .NET 9.0.16; the existing application's dependency graph references System.Text.Json 9, so running the test executable under only .NET 8 fails at SessionHandler construction. No application framework/dependency migration was made.
- `node --test tests/head-audit-monitoring/*.test.cjs tests/executive-dashboard/cia-snap-enhancements.test.cjs`: 16 passed. This includes five real XLSX serialization/reload checks, formula injection, SQL spec/body/F8 alignment, selected-query source contracts and unchanged unrelated dashboard/para eligibility/export-gate sources.
- `node tests/head-audit-monitoring/dashboard.browser.cjs` with Playwright on `NODE_PATH`: passed in headless Edge. Synthetic scoped fixtures exercised all four tab loads, five downloaded/read-back workbooks, displayed rows/metadata/filters, cache reuse, assignment changes, access-denial clearing and period changes. Desktop (1440px) and mobile (390px) para screenshots were inspected and compared with the HM reference modal. These are browser fixture checks, **not authenticated live IAS UAT**.
- `pwsh -NoProfile -File tests/executive-dashboard/cia-snap-enhancement-policy.ps1`: passed 9 CIA date/scope checks, 9 Snap cascade/forged-scope checks, 4 reconciliation checks, 26 compiled Snap access checks and 4 PDF checks, without Oracle.
- A broad `tests/executive-dashboard/*.test.cjs` run has **14 pre-existing failures and 3 skips**. The same 14 failed on an isolated unchanged `fa64939f` checkout. Failures include obsolete PR317/329 byte/source assertions, missing legacy SQL fixtures and an old disabled-Snap fake DOM. The branch's additional focused tests introduced no extra failures. These unrelated historical tests were not rewritten to hide baseline failures.

## Required live validation before acceptance/deployment

- Compile the package in the controlled Oracle environment and inspect `USER_ERRORS`.
- Reconcile all eight KPIs against the original current-position readers under equivalent conditions; NULL `P_AS_OF` preserves the current-master comparison path. Then reconcile dated samples against actual workflow logs, including reversals, tied/missing events and recorded conclusions.
- Trace procedure executions for each tab and cache revisit; confirm the counts described above and actual performance with representative data.
- Compare each downloaded table with its authorized live rows, filters and plan totals. Test an office with no engagements as well as multi-engagement plans.
- Test unauthorized users, revoked/reassigned sessions, unrelated offices, engagement IDs and exact COM_ID/date/division/department/entity combinations against real Oracle scope. Locally compiled gate tests and unchanged SQL eligibility predicates do not replace this validation.
- Verify Review Saved Work return links preserve period/cutoff/active tab/register filters. It intentionally shows current saved work.
- Run CIA, Snap's other views, CCO, Group/RLM and unrelated dashboard smoke tests in Visual Studio. Their source/security/calculation files were unchanged except for the requested Snap para presentation.

Start Visual Studio from the feature branch with the user's existing local settings. Local `AIS/appsettings.json`, NuGet `obj` changes and `.vs` files predated this task and are excluded from its commits.

## Continuation shutdown

Native heartbeat `resume-head-audit-tabs-and-para-formatting` was paused through supported scheduler controls after source work and focused validation completed. A subsequent view and the saved scheduler inventory confirmed `PAUSED`; there are no other local continuation schedules. The old `resume-ias-cia-and-snap-implementation` was absent. No scheduled continuation will initiate another session. Final official five-hour usage check: 80% remaining.

Remote push was attempted and failed on github.com:443 connectivity. The branch is local; retry git push -u origin codex/head-audit-tabs-and-para-format from a connected shell. No PR, merge or deployment was created.
