# CIA executive enhancements deployment and acceptance

Do not run against production without separate approval. No SQL was executed during development.

## Artifacts and order

Changed SQL: `sql/PKG_EXEC_DASHBOARD.sql`. It creates, in order, V_EXEC_ENTITY_SCOPE (unchanged definition), V_EXEC_PARA_BASE (aging bucket split), PKG_EXEC_DASHBOARD specification (unchanged signatures), and package body (new lifecycle risk columns, split dated aging buckets and reporting-office cursor fields).

Prerequisites: existing V_IAS_ENTITY_REPORTING_IDS, V_IAS_ENTITY_REPORTING_NAMES, T_AUDITEE_ENTITIES, V_EXEC_KPI_LIFECYCLE and all existing package dependencies must be valid. This change does not replace hierarchy views, change grants, or introduce API entitlements. Do not redeploy historical dependency scripts blindly. On a fresh staging installation follow the existing lifecycle installation order before this package.

1. Save existing view/package DDL using DBMS_METADATA and record the application version and grants.
2. Run `sql/cia_executive_acceptance.sql` preflight queries. Investigate missing/conflicting parent mappings and duplicate COM_ID source rows. No arbitrary parent is selected among conflicting IDs or names.
3. Run the complete package script in an approved staging maintenance window. DDL commits implicitly. Check USER_ERRORS and USER_OBJECTS: every relevant object must be VALID and errors empty.
4. Deploy the matching application only after Oracle compilation passes. New readers require MEDIUM_RISK_OPEN_PARAS, LOW_RISK_OPEN_PARAS and reporting-office cursor fields. Old applications can ignore additive cursor fields, but aging bucket changes require coordinated application deployment.
5. Repeat acceptance queries and scoped API checks below. Production rollout remains a separate approval.

## Staging acceptance

Use real authorized TOP, GROUP, RLM, CIA/Snap and CCO sessions. Record USER_CONTEXT_ID, assigned entity, filter values, start/end dates and source quality for every comparison. Never substitute a bank-wide SQL result for a scoped API result.

- Confirm ten ordered first-tab indicators; CIA bookmark redirects to Snap. Command Center remains unchanged. CCO supplemental cycle/rate information remains; duplicate critical/high-risk card is removed.
- Opening + Added = Net; Opening + Added - Settled = Closing. Record nonzero differences with source COM_ID evidence; historical reconstruction cannot resolve unlogged reopenings or missing settlement dates. Unknown values must remain unavailable.
- Compare risk IDs 1/2/3 to the same closing portfolio at today and a historical cutoff. Unrated records explain any difference between closing and the sum of rated risks.
- Compare aging total (including unknown) with closing for the identical dated distribution filters. Ordinary views use their existing current-state portfolio; do not compare them to an unrelated historical cutoff. Known + unknown must equal total.
- Exercise ages 365, 366, 730, 731 and 1095/1096; only 731 and above count toward over-two-years. Future/invalid dates belong to unknown, never to the youngest bucket.
- Validate missing and conflicting reporting_1 IDs, missing office entities and conflicting names: no fabricated office; correction required indication. Check IDs against the authorized auditee hierarchy, not auditby. Query results identify actual exceptions; none were claimed verified locally.
- For CCO HIGH/MEDIUM/LOW, check offsets 0/100/200, empty page and exact boundary: stable MATCH_COUNT, no repeated COM_ID across pages, and unchanged authorized filtering. Office joins are grouped to one row per entity.
- Negative security cases: unauthenticated session, changed USER_CONTEXT_ID, wrong role, unassigned entity, foreign office selector and revoked page/API grant must deny access before data disclosure. Confirm TOP lifecycle endpoint still rejects non-TOP perspectives.
- Gist: missing/whitespace, long Unicode, and `<img onerror=...>` must render as text; COM_ID detail authorization stays intact.

## Rollback

Restore the previous application and saved Oracle DDL together in the approved window. Restore views before package spec/body in their original dependency order; recompile invalid dependents and check USER_ERRORS. No table data or grants are changed by this patch. Preserve diagnostic evidence and do not repair hierarchy data as part of rollback.

## Local validation limitations

Application builds with 0 errors (57 existing warnings). Focused regression checks cover lifecycle keys, authorization contracts, pagination, actual aging rendering, risk SQL predicates, mapping exceptions and safe gist. The broad repository suite already had 30 failing tests before this task (139 tests, 106 passed, 3 skipped); it contains old PR snapshot assertions. Oracle compilation, live mapping exceptions, scoped reconciliation and role-session acceptance require an approved staging database and credentials; they have not been executed here.

## Requirements checklist

- [x] Ten ordered cards for TOP, GROUP, RLM, CCO and CIA At a Glance via Snap; nullable Net arithmetic; same-cutoff medium/low risk; quality flags preserved.
- [x] Aging captions, exact two-year split, hidden unknown category, inclusive totals and data-quality note; existing cycles preserved.
- [x] Reporting-office cursor/model/reader/UI path with explicit missing/conflicting exceptions and unchanged paging/risk filters.
- [x] Safe gist and Para Heading retained from d3e25e745246467fd5a1577146e20dbcb9358bee.
- [x] Targeted regression tests updated/added; application build passes.
- [x] Deployment order, preflight SQL, USER_ERRORS, reconciliation and rollback documented.
- [ ] Oracle compilation and live staging acceptance: blocked on approved database/session access.
- [ ] Existing broad-suite failures: 144 tests, 112 passed, 29 failed, 3 skipped. Remaining failing test names also occurred in the pre-change run; this is not an all-green suite.

Two-year indicators are displayed alongside Aging Analysis after the one-year figure. Ordinary and dated buckets are split at day 730, so the count never approximates an unsplit 1–3-year bucket. Ordinary views retain their current-state/audit-period semantics; dated views retain their reporting cutoff.

## Complete changed-file list (including retained gist commit)

- `AIS/DBConnection.AuditParaSnap.cs`
- `AIS/DBConnection.ComplianceDashboard.cs`
- `AIS/DBConnection.ExecutiveDashboard.cs`
- `AIS/Docs/CIA_EXECUTIVE_DEPLOYMENT.md`
- `AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql`
- `AIS/Docs/sql/cia_executive_acceptance.sql`
- `AIS/Models/AuditParaSnapData.cs`
- `AIS/Models/CcoDashboardPresentation.cs`
- `AIS/Models/ExecutiveDashboardModels.cs`
- `AIS/Models/ExecutiveDashboardViews.cs`
- `AIS/Views/ExecutiveOverview/_AuditParaSnapAnalysis.cshtml`
- `AIS/Views/ExecutiveOverview/_Critical.cshtml`
- `AIS/Views/ExecutiveOverview/_Sections.cshtml`
- `AIS/wwwroot/js/audit-para-snap.js`
- `AIS/wwwroot/js/cco-compliance.js`
- `AIS/wwwroot/js/executive-dashboard.js`
- `tests/executive-dashboard/cia-approved-enhancements.test.cjs`
- `tests/executive-dashboard/cia-snap-enhancements.test.cjs`
- `tests/executive-dashboard/group-rlm-nine-lifecycle.test.cjs`
- `tests/executive-dashboard/rlm-audit-paras-harmonization.test.cjs`
