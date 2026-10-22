# Dashboard follow-up checkpoint — 21 October 2026

**Closed locally on 22 October 2026.** The remaining source review and canonical workbook registration are complete. See `DashboardFollowup-2026-10-22-verification.md` for results and manual Oracle/UAT requirements. The historical checkpoint below is retained for traceability; its resume steps are superseded.

Branch main. Latest request implemented; final source/security review and earlier registration verification remain. No Oracle connection or deployment occurred. Last credit check 91% used; stop before 97%. Reset advertised: 2026-10-21 12:44:20 UTC / 17:44:20 Pakistan.

Completed this checkpoint:
- Shared dashboard export assets rendered only for authenticated, non-revoked Super User (role 1). Report export buttons gated; server PDF formats denied to other users. Report JavaScript tolerates absent buttons.
- Excel contains context, table and KPI cells only, with no image/canvas capture. PNG/PDF preserved. Executive pages suppress top header like Head Audit.
- GROUP/RLM fixed January 1 through yesterday in Pakistan, MVC/API/readers and Oracle validation. Generic dated metrics reuse lifecycle/distribution sources; role comparisons/follow-ups use yesterday's historical-open predicate and YTD settlements. No root/permission fallback. January 1 returns a clear no-completed-day error rather than last year's data.
- Plan/Criteria/Team review links consolidate into job assignments. Review tables show employee names, dates, observation memo/draft/final numbers, risk and status. History has only event/description, date, performer. Saved rich text parsed into plain Razor-encoded text. Latest observation text selected once per observation.
- P_GET_PLAN_DETAILS independently validates context and period, scopes approved plans (statuses1/2/4) by performing office and scoped engagements. Includes plans without engagements; missing details N/A. Multiple engagement detail rows do not change KPI plan counts.
- SQLPlus sources retained. Changed F8 units regenerated; restored generator normalizes line endings.

Verification completed:
- Application build: 0 errors, 57 existing warnings. Subsequent console test runs compile the app and passed.
- Head audit console: authentication/GET contracts, date policies, fixed management period/forged dates, leap/Jan1 boundary, plan denominator, concluded/missing-date delays, safe narrative text.
- Browser synthetic fixtures: full-scroll PNG, PDF, data-only Excel, literal formula-like strings, hidden columns, unchanged layout and busy guard; all six dated CCO/GROUP/RLM requests; ordinary-user report loads without export controls. GROUP fixture initially lacked a root; corrected fixture and rerun passed.
- Changed SQL/F8 source equality and no SQLPlus directives passed. NOT live Oracle compilation/security/UAT.

Exact next steps:
1. Review this checkpoint commit diff for SQL correctness, especially management historical stock versus YTD activity, hierarchy de-duplication, employee-name lookup and plan detail joins. Check latest changes to partial views and any stale current-state labels. Do not rerun broad suites.
2. Verify page1000401 canonical registration in Page ID.xlsx (older checkpoint pending; workbook was not changed in this turn). Preserve all other registrations and PageIdResolver aliases. Follow spreadsheet skill if editing.
3. Add any focused missing checks, document final results and commit. No schema changes, grants or production deployment authorized.
4. Manual DBA: existing dependencies must be valid. For this delta execute f8/PKG_EXEC_DASHBOARD/04.sql (body), f8/PKG_EXEC_REPORTS/02.sql (body), then f8/PKG_HEAD_AUDIT_MONITOR/04.sql (spec),05.sql(body),06.sql(validation). After EACH unit run validate_errors.sql and validate_status.sql. Review package source diff before deployment; preserve compile errors. Deploy app together with these package changes.
5. UAT with explicitly allocated cluster/FAD/ISAD/CAD/QAD users: own plus performing-office descendants only; sibling engagement/document IDs and unallocated page must return403. Compare period plans/KPIs, missing engagement details, summary/office counts, all tabs and exports. Live Oracle/UAT remains external dependency.

Older PKG_RPT work: main has newer source/deployment analysis commits. Read AIS/Docs SQL notes before acting; never deploy standalone R_IAS_PARA_POSITION as PKG_RPT.

Unrelated work preserved: AIS/obj three NuGet files and .vs/. No application settings or workbook edits in this checkpoint.
Resume: read this file, inspect git log -1 and git status, check five-hour credits, then continue step1. Do not repeat completed implementation.
