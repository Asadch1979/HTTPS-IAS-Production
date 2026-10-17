# IAS approved executive dashboard design-parity delivery

Branch: `feat/approved-dashboard-design-parity-20261017`. Source implementation only; no SQL objects/permissions, production release or feature flags were changed.

## Approved design surfaces

### 1. TeamMate-inspired Team Leader workbench
`AIS/Views/Shared/EngagementWorkbench.cshtml` and `AIS/wwwroot/css/engagement-workbench.css` now render the blue IAS/TeamMate navigation strip, a four-card scorecard, four-stage ribbon and four-column engagement board, engagement register, authorized quick links, and two read-only workpaper/observation detail panels when the existing approved Team Leader Oracle integration is enabled.

- Tasks are **only** from `Model.AvailableEngagements` / `Model.WorkbenchTasks` already intersected with authenticated Team Leader assignments.
- Planning = verified IAS task status 1; Fieldwork = task status 2. Review and Reporting are visible in the approved design but intentionally have `N/A`/mapping pending. Do not treat missing mapping as zero.
- V2 workpaper counts use the existing `TeamLeadSnapshot`; when `RoleDashboards:TeamLeadOracleEnabled` is disabled, show explanation rather than generated counts.
- Buttons link to existing authorized `FieldAudit/AR_Dashboard` or `ManagementAudit/MA_Dashboard` engagement/step actions. There is no new permission or bypass.

### 2. ZTBL Management Audit: Group and Divisional Heads
`AIS/Views/ExecutiveOverview/_ManagementAudit.cshtml`, `AIS/wwwroot/css/management-role-dashboard.css` and `AIS/wwwroot/js/executive-role-visuals.js` now have a green scoped management command banner, four top KPIs, risk and trend chart cards, three donut-style scorecards for settlement ratio, high-risk share and over-365-day share, departmental comparison bars, and a follow-up register.

- KPI rings subscribe to `ias:executive-data`, emitted from the successful **existing authorized** `GetKpiSummary` / `GetRiskSummary` / `GetSettlementTrend` fetch. No duplicate Oracle KPI request.
- Settlement ratio = Oracle source `settlementRatio`; high-risk share = `highRiskParas/openParas`; over-365 share = `over1YearParas/openParas`. Zero denominators display **N/A**. Ring scale 0–100% is visual, not a distinct bank metric.
- Department/Division bar view uses `GetRoleUnitComparison` returning server-scoped entities, showing up to eight top units on a common open-count scale; the detailed table remains accessible.
- If `RoleDashboards:OracleReadEnabled` is off, it displays neutral skeleton bars clearly marked as pending, not made-up departmental performance.

### 3. Dark Design 3: GM and Regional Heads
`AIS/Views/ExecutiveOverview/_RegionalPulse.cshtml`, `AIS/wwwroot/css/regional-audit-pulse.css`, `AIS/Views/ExecutiveOverview/_RoleReporting.cshtml` and `AIS/wwwroot/js/executive-role-reporting.js` now form a dense midnight-navy layout with compact risk KPI cards, trend/risk/aging/cycle graphics, risk-classified branch heatmap, follow-up register and leadership monitoring strip.

- The heatmap visualizes `PKG_EXEC_DASHBOARD.P_GET_ROLE_ENTITY_HEATMAP` in verified authorized Region/GM scope, with source counts for open/high/aged/cycles and **branch-specific high-risk/open ratios**.
- Thresholds 20%+ red, 10–19.9% amber, below 10% teal, zero-open neutral are clearly labelled **presentation bands**, not changes to recorded IAS severity.
- Heatmap displays up to 36 exposure-ranked branches from the returned (up to 250) cursor; underlying accessible comparison table preserves all returned rows. No capped list is summed as a regional portfolio total.
- While bulk Oracle integration remains off, existing six-branch comparisons are risk-color-coded without increasing the existing number of calls.

### 4. CIA Command Center: agreed SECOND block visible
`AIS/Views/ExecutiveOverview/_CiaCommandCenter.cshtml`, `AIS/wwwroot/css/executive-themes.css` and `AIS/wwwroot/js/cia-command-center.js` implement a permanently rendered `Nature of Issue Identified` panel **immediately below the CIA five-KPI block**:

1. AML/CFT
2. Accounts
3. Loan and Advances
4. Cash Management
5. Record Management
6. Banking Operations
7. Branch General Controls
8. Accounting / Reporting / Reconciliation
9. Management Audit
10. IS Audit

- Ten labels are always visible even when `RoleDashboards:CiaCommandCenterEnabled` is false or Oracle is unavailable. Display **N/A**, never example data.
- The authorized `P_GET_CIA_ISSUE_NATURE` dataset populates each card, proportion bar and classified-vs-unmapped donut.
- The rendering checks that exactly the ten known category codes are returned; each count is a nonnegative integer; all rows agree on `TOTAL_ISSUES` and `UNMAPPED_ISSUES`; and `mapped + unmapped = total`. Mapping gaps are explicitly flagged. No unclassified issue is silently dropped.
- `Index.cshtml` shows the Command Center partial immediately without waiting for JavaScript; server still enforces CIA/Super User page permissions.

## Data and access protection
No C# controller endpoints, Oracle package bodies, role mappings, API permissions, authorization filters, TOP/CCO report KPIs, or Report A-E procedures were changed. The new charts only consume *existing scoped* IAS APIs and Model properties. No synthetic values, unnamed branch locations or hidden user impersonation are introduced.

## Focused QA and release prerequisites
Source-level QA checked JavaScript syntax, Razor section/div/article tag balance, ten category codes, role-reporting container hooks and unchanged feature-gated package usage. Portable checks: `node --test tests/executive-dashboard/approved-design-parity.test.cjs`.

Before merging/deploying:
1. Perform a .NET build and Razor view compilation with the project toolchain; validate the rebuilt TeamMate workbench on both Field and Management Audits.
2. Check desktop 1440, laptop 1024, tablet 768 and mobile 390 widths with authenticated user screenshots against the approved concepts.
3. Verify `TeamLeadOracleEnabled`, `OracleReadEnabled` and `CiaCommandCenterEnabled` only after DBA compilation and data-reconciliation approval; flags remain unchanged in source.
4. Reconcile CIA classified/unmapped with `PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE`, and branch high-risk/high-risk-share with the existing authorized queries.
5. Negative tests for unauthenticated roles, revoked session, foreign Team Leader engagement, altered RLM root and foreign Group/Division. UI style is **not** authority.
6. Confirm TOP/CCO, formal reports A-E, responsive filters and PDF exports are not affected.

This is a design-parity implementation, **not** proof that Oracle packages have been deployed or workflow stage mappings have been approved. Audit KPI renaming and revised calculation semantics are intentionally deferred.
