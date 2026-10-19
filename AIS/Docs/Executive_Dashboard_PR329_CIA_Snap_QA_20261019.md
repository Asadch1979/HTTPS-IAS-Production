# IAS dashboard QA and follow-up to merged PR #329

Date: 19 October 2026
Status: **SOURCE REVIEW / DEVELOPMENT PROPOSAL ONLY**. No Oracle/IIS production
deployment, compile, database access or interactive browser visual QA performed.

## 1. Merged PR #329: Audit Teams Monitoring

GitHub pull request: https://github.com/Asadch1979/HTTPS-IAS-Production/pull/329
Merge SHA: `67fc57c8b737f47602f8ad9750f1e2819f0026e6`.
17 files, ~942 additions and 208 deletions.

Confirmed implemented:
- TeamMate-inspired workbench is relocated to
  `/HeadAuditDepartment/AuditTeamsMonitoring` (page `1000401`), with
  `/HeadAuditDepartment/Review` scoped read-only details.
- `PKG_HEAD_AUDIT_MONITOR` is a separate read-only Oracle package; original
  `PKG_TEAM_LEAD_DASHBOARD` is retained; normal FAD/MAD team execution actions
  are left independent.
- Head audit role codes 5/6/7 and Super User 1 are checked alongside IAS
  department/user context and engagement portfolios.
- Dedicated GET APIs, session/page/exact-API authorization, evidence/report
  attachment download, server-side revalidation.
- Current saved work: plan, criteria, team, tasks, legacy and optional V2
  workpapers, observations, feedback/history, evidence and draft reports.

**Not yet verified** and worth DBA review before production:
- `V_HEAD_AUDIT_ROLE_DEPT` exact role/department matches and the production
  `V_HEAD_AUDIT_ENG_SCOPE` population for FAD/MAD/ISAD.
- The query `e.status BETWEEN 4 AND 13` includes status **13**: verify the
  live status catalogue; finalized work should not be counted as *teams
  currently conducting audit* unless deliberately included.
- `GetHeadAuditDocument` interprets response evidence and saved reports as
  base64 text, but legacy evidence as BLOB. Verify Oracle column types;
  a mismatch can cause download exceptions despite otherwise valid scope.
- The package's many read-only workpaper sections must be compiled/tested
  against real Oracle legacy and V2 table columns and object availability.
- `head_audit_monitoring_permissions.sql` explicitly grants roles 1,5,6,7.
  For a Super User-only preview **do not run** that grant script unchanged:
  approval of wider Head roles must precede enabling them.
- No GitHub commit status evidence was present at review time. The actual
  Oracle objects, deployed front end and live role tests were not available.

Read-only F8 checklist:
`AIS/Docs/sql/verify_head_monitor_postmerge_F8_20261019.sql`.

## 2. CIA Command Center: eight visible categories

Business request: omit **Accounts** and **IS Audit** from the screen.
The UI now displays the other eight nature cards; the Oracle
`P_GET_CIA_ISSUE_NATURE` continues returning all **ten**, including
Accounts and IS Audit, without changing mapping DDL. All ten are
validated in JavaScript. Hidden categories are counted in an explicitly
labelled **Other classifications (not shown)** subtotal in the summary.
`mapped + unmapped = total` is still required; no data is silently dropped.
Existing CIA API/page entitlement guards and other KPI cards are untouched.

## 3. Audit Para Snap Division + Department / Cluster

The two dropdowns are:
- Division: `FAD` = Field Audit Division,
  `MAD` = Management Audit Division. Default `FAD`.
- Department / Cluster: `All` plus actual authorized audit-by departments
  for the selected division. Changing division resets child to All and
  submits GET, refreshing its authorized children list.
- *All* means the sum of records for **all mapped AUDITBY_IDs** belonging
  to that one division; it is **not bank-wide**. A selected child uses exactly
  that department's `AUDITED_BY_ID`.
- If a parent department itself records audits, it must also have an
  explicit row in the map. Mapping must cover each true audit portfolio
  exactly once; unique AUDITBY_ID prevents double counting.
- Date filters, dashboard tabs, reports A–E and PDF/CSV export links carry
  both selectors. Their Oracle sources all use the same audit division
  map plus optional exact child restriction. Browser-provided child/parent
  combinations are revalidated server-side.

**Critical prerequisite: DATA MAPPING NOT YET SEEDED.**

The actual audit department IDs and FAD/MAD child membership are NOT
verified in connected GitHub. DBA must inspect:
`AIS/Docs/sql/verify_snap_audit_division_portfolios_F8_20261019.sql`.
Then review/create and seed:
`AIS/Docs/sql/snap_audit_division_scope.sql`.

The mapping table `T_IAS_SNAP_AUDIT_SCOPE` deliberately contains **zero
invented seed IDs**. If unseeded or incorrectly selected, the dashboard
returns an explicit error (fail closed), rather than silently showing the
entire bank. Do not deploy the ASP.NET dropdown before seeding both
division mappings and verifying the old totals by aggregation.

**Oracle dependency order for staging only:**
1. Verify live mapping and snapshot/export baselines.
2. DBA create `T_IAS_SNAP_AUDIT_SCOPE`, install
   `PKG_AUDIT_SNAP_SCOPE` spec/body; seed actual FAD/MAD department IDs
   through a separately approved DML script; verify membership.
3. Compile revised `PKG_EXEC_DASHBOARD` spec **and** body. Three relevant
   cursors have trailing optional `P_SNAP_DIVISION_CODE`; non-Snap
   calls pass none and retain existing behavior.
4. Compile revised `PKG_EXEC_REPORTS` spec **and** body; the five report
   entry points pass the same optional division parameter into two shared
   report data-source branches.
5. Confirm `USER_ERRORS` empty, getters valid, same baseline data counts,
   cross-division exclusion, malicious child rejection, report A–E and PDF.
6. Deploy app in a separate approved window; test the actual endpoints
   under an authorized CIA/Super User session and a forbidden user (403).

**Do not run either full canonical SQL file casually**. Extract and compile
reviewed package spec/body separately after backups, preserving unrelated
objects and implementing the approved deployment sequence. Deploy no code
against an incomplete DB signature/mapping.

## 4. Audit Para Snap Risk Distribution visual fix

Observed screenshot: valid High/Medium/Low labels/counts, empty grey bars
and a very large two-column data table with apparently missing right values.
Confirmed CSS cause: the JS-generated `exec-fill` is an inline `span`;
`style.width='NN%'` on an inline element does not draw the width.
`display:block; height:100%` now makes actual fills visible.

The bar and numeric columns use scoped, responsive layout, and the
complete data table is under `View chart data` disclosure rather than
taking over the whole card. The monthly two-series line trend gets
responsive width too. This is a presentation-only change: IAS counts
6,291 / 8,853 / 2,341 from the screenshot were not modified in SQL.

## 5. Dashboard look-and-feel QA

**Static source review completed (not a live browser visual test):**
- CIA: retains teal/navy Commander styles; eight visible nature
  cards, ten-category data-quality reconciliation and five top KPIs.
- Audit Para Snap: preserved top header alignment, date tools,
  period navigation, four analysis panels, monthly trend, responsive
  Risk/Aging/Cycle graphic cards, “View chart data” disclosure.
- TOP: dedicated life-cycle perspective and graph themes retained.
- CCO: warm compliance theme, charts and approved report navigation intact.
- Group Management: green management KPI/scorecard and comparison surfaces.
- RLM: dark regional palette remains bound across all RLM tabs.
- TeamMate Head: blue supervisory workbench retained, standalone Head
  review page remains read-only.

**Live UI QA still required:** test 1440px, 1024px, 768px and 390px,
table numeric alignment, no horizontal overflow, no clipped labels,
visible empty/error/loading states, correct contrast and keyboard operation.
Check authorization/guest 401/403, page assignments, Snap All-vs-child
counts, Reports export matching, CIA hidden counts and query latency.

Tests changed or added:
`tests/executive-dashboard/approved-design-parity.test.cjs`,
`cia-command-loading.test.cjs`,
`review-329-snap-presentation.test.cjs`,
`audit-snap-division-scope.test.cjs`.
Browser JS syntax/source checks passed; full automated .NET/Oracle
compilation and authenticated screenshot checks **not performed**.

## Integration

PR #325 (permission hardening) and #326 (CIA command loading) remain open
at this review. Their unmerged changes can conflict with this branch.
Keep source changes in a separate PR, no production merge/deploy without
review. Do not inadvertently restore environment-based loading blocks.
