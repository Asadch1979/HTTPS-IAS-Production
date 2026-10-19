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

**Superseded design correction (19 October 2026):** IAS already maintains
the bank-wide hierarchy in `V_IAS_ENTITY_REPORTING_NAMES`. PR #330 originally
proposed `T_IAS_SNAP_AUDIT_SCOPE`, which would duplicate that hierarchy.
This new correction **removes that proposed table** and switches all Snap
classification and validation to the existing view. No new table, seed script,
data-copy or nightly synchronization is needed.

Verified by the owner's production hierarchy query:

| Division label | IAS hierarchy parent ID | Direct dropdown children |
| --- | --- | --- |
| Field Audit Division (FAD) | `112243` — FIELD AUDIT DEPARTMENT | 9 audit clusters |
| Management Audit Division (MAD) | `113254` — MANAGEMENT AUDIT DIVISION | 3 audit departments |

- `PKG_AUDIT_SNAP_SCOPE.P_GET_CHILDREN` uses the view's
  `REPORTING_1` to retrieve **direct** children, returning `ENTITY_ID`
  and `ENTITY_NAME`. The Razor dropdown retains an explicit **All**.
- `PKG_AUDIT_SNAP_SCOPE.REQUIRE_SELECTION` validates the division code
  and an individual child against the same **direct-child** hierarchy,
  rejecting all forged or cross-division selections.
- `All` uses the selected parent `AUDITED_BY_ID` itself **plus**
  all descendants whose `ENTITY_ID` has that parent in
  `REPORTING_1` through `REPORTING_6`. This includes direct records of
  a parent that also performs audits; it never means all ZTBL.
- An individual dropdown selection adds an exact `AUDITED_BY_ID =
  P_AUDITED_BY_ID` condition **in addition** to the selected division's
  ancestry predicate, preventing a browser from escaping the division.
- This exact source predicate is applied consistently to
  `PKG_EXEC_DASHBOARD`'s lifecycle, distribution and critical cursors,
  and `PKG_EXEC_REPORTS`'s two common data-source branches used by
  Reports A–E.
- The source view is used against **audit-performing IDs**
  `V_EXEC_KPI_LIFECYCLE.AUDITED_BY_ID` and
  `V_EXEC_REPORT_SOURCE.AUDITED_BY`; it is **not** applied to the
  hierarchy of the *audited entity*.

**Remaining production verification:** the user's query proves the
organizational hierarchy. It does not independently prove that each
reporting source uses the same audit-performing IAS `ENTITY_ID` values.
Before deploying, run the rewritten read-only F8 script:
`AIS/Docs/sql/verify_snap_audit_division_portfolios_F8_20261019.sql`.
It compares every parent/descendant against both Oracle source views,
finds overlaps, checks the 9/3 direct children and identifies any
leftover old table. If audit-by keys are different, **fail closed** and
resolve the relationship—do not weaken the scope or return bank-wide data.

**Required DBA install order (after backup and staging validation):**
1. Verify `V_IAS_ENTITY_REPORTING_NAMES` and the two source views.
2. Compile the revised `PKG_AUDIT_SNAP_SCOPE` **spec and body** from
   `AIS/Docs/sql/snap_audit_division_scope.sql`. Its exposed signatures
   remain unchanged.
3. Compile the revised `PKG_EXEC_DASHBOARD` and `PKG_EXEC_REPORTS`
   package **bodies only**: their signatures stay as in merged PR #330.
   No new scope table and **no new seed**.
4. Verify `USER_ERRORS` and `USER_OBJECTS`, counts for FAD All and
   MAD All versus direct and deeper descendants, selected child,
   invalid child rejection, risk/trend, critical items and Reports A–E
   including download/export.
5. Keep existing Super User-only pilot permissions; this correction
   changes **neither** roles/API activation nor application routing.
   Do not merge/deploy unless explicitly authorized.

If an operator installed `T_IAS_SNAP_AUDIT_SCOPE` during an earlier
deployment, this PR does **not** drop it automatically. The DBA must check
for external dependencies, take a backup, and plan its retirement
separately.

**Do not execute either canonical SQL file in full**: extract only the
approved package spec/body for the stage being deployed.

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
