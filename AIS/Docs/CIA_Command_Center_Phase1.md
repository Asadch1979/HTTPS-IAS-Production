# CIA Audit Command Center — Phase 1

Implemented against `IAS_Four_Role_Dashboard_Designs.md` from design commit
`af086f7c` (`design/ias-four-role-dashboards-20261016`), already included by
merge `9bb01cf2`. The local document matches that design commit. A remote fetch
was unavailable because GitHub could not be reached.

## Scope

- `/ExecutiveOverview/CIA?view=glance` uses a dedicated Razor layout nested in
  the existing IAS shell, a CIA partial and CSS scoped to `.cia-dashboard`.
- Navy/teal executive priorities, existing IAS KPIs, audit performance,
  risk/remediation and leadership-attention sections replace the CIA glance view.
- Existing summary, risk, settlement trend and critical-item endpoints are reused;
  the CIA glance definition also enables the existing aging endpoint.
- Session validation, role resolution, page/API permissions, organizational
  hierarchy validation and filter requests are unchanged. No controllers, database
  methods, SQL, role mappings or permission grants changed.
- TOP, CCO, GROUP, RLM, the other CIA views and Reports A–E retain their existing
  layouts and contracts. Navigation preserves the existing filter route values.

## Data and date semantics

| Display | Existing source |
|---|---|
| Issue Identified / Issue Rectified | Authorized `GetKpiSummary`, using `P_GET_CIA_ISSUES`; optional paired dates apply only to these issue events |
| Open Paras / High Risk / Settled Month-to-Date / Settlement Ratio / Over 1 Year / Critical Items | Authorized `GetKpiSummary` current-state fields |
| Monthly settlements, including high-risk settlements | `GetSettlementTrend` |
| Risk mix / Aging | `GetRiskSummary` / `GetAgingSummary` |
| Critical observations | `GetCriticalItems`, existing risk filter and expandable observation text |

Both issue dates blank means all eligible issue history. The existing backend
rectification contract excludes status 23 drops. Current-state KPIs, aging, risk,
trend and observations do not claim to follow the optional issue date range.
The header time records successful retrieval in the browser's local timezone;
it is not a database snapshot timestamp.

## Implementation gaps

- Audit plan coverage and execution by portfolio: **N/A** until scoped planned
  audits, completion states and an approved coverage denominator are verified.
- Draft reports awaiting review: **N/A** until a scoped approval queue and
  workflow state/assignment contract are verified.
- Overdue remediation: **N/A** until remediation deadlines and status rules are
  verified. Aging and days since movement are not used to infer overdue counts.
- Leadership actions, assigned owners and due dates remain unavailable. The
  existing observation table identifies the entity and priority/risk; it does
  not invent an owner, approval action or deadline.
- Heads of Audit require their own verified role/page mapping. This phase does
  not grant CIA access to another role.
- Team Leader, Group/Division and GM/Region designs remain separate phases.

## Focused verification and review gate

- `dotnet build AIS/AIS.csproj --no-restore --verbosity quiet`: passed,
  0 errors and 57 warnings in existing code.
- `node --check AIS/wwwroot/js/executive-dashboard.js`: passed.
- Diff review: CIA-only presentation selection; no changes to security/SQL or
  Reports A–E; new stylesheet selectors scoped to the CIA command center.
- Live Oracle reconciliation, authenticated allowed/denied-role smoke checks
  and browser review at desktop/mobile sizes remain deployment/review checks;
  they were not performed in this environment.

Do not merge into Main until the redesigned CIA dashboard is reviewed and
approved. Existing Oracle package deployment and CIA count verification remain
required before release, as documented in `ExecutiveDashboardRoleStylesAndCIAIssues.md`.
