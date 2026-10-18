# Executive dashboard parent views (development review)

Baseline: main at `5b64770` (PR #320), verified against GitHub on 18 October 2026.

`ExecutiveOverview/Index` remains an authenticated route resolver. TOP, CCO,
GROUP and RLM now render their own Razor parents alongside the independent CIA
and Audit Para Snap parents. The shared Index header/navigation/filter shell is
removed. Role parents own titles, navigation, date forms and script selection;
existing KPI/chart and report presentation remains shared. The compact Super User
switcher is separate from dashboard content, including on CIA and Snap.

Only CIA Command Center offers organizational filters, its IAS audit-period
selector and the Organization Structure link. Non-CIA parent routes reject
organizational filter parameters instead of carrying hidden controls. API
comparison drilldowns retain their existing server-validated narrowing contract;
they cannot replace the authenticated Group/RLM root. Ordinary GROUP rejects any
client-supplied groupRootId and resolves from IAS UserEntityID. Super User GROUP
and RLM retain separate verified root selectors, with no data load before a root
is selected. Page/API permissions and fixed/configured role mappings still apply.

Group shows the Management Audit banner once, then six analysis tabs: Management
Audit, Audit Para Position, Settlement & Performance, Risk, Aging & Compliance,
and Reports. Legacy Group glance bookmarks open Management Audit. The banner is
owned by the parent, including on Group analysis tabs; the content partial no
longer duplicates it. RLM owns its regional/management banner and retains the
approved dark Regional Audit Pulse styling.

## Date-filter dependencies

| Data | Current behavior and required backend work |
| --- | --- |
| TOP At a Glance lifecycle KPIs | Existing `PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE` accepts start/end dates. Unchanged; no duplicate procedure. Historical availability/estimate labels remain. |
| Current KPI summary, risk, aging, cycles, settlement trend, critical observations | `P_GET_KPI_SUMMARY`, `P_GET_RISK_SUMMARY`, `P_GET_AGING_SUMMARY`, `P_GET_CYCLE_SUMMARY`, `P_GET_SETTLEMENT_TREND`, `P_GET_CRITICAL_ITEMS` have no start/end parameters in `ReadExecutiveSummary`. Dates are retained in parent navigation but are explicitly not claimed to filter these datasets. Range support needs separately approved date-aware Oracle contracts, event/snapshot definitions, development compilation and reconciliation before API/UI activation. Existing signatures are untouched. |
| Group/Division/GM/Region comparisons and follow-ups | `P_GET_ROLE_ENTITY_HEATMAP`, `P_GET_ROLE_UNIT_COMPARISON`, `P_GET_ROLE_FOLLOWUPS` accept a root/audit-period contract, not start/end dates. Current-state only; separate date-aware Oracle work and verification required. |
| Reports A–E | Existing report reader already binds `P_START_DATE` and `P_END_DATE`. Permissions, calculations, exports and SQL remain unchanged. Historical balances/settlement attribution remain indicative and use current organization mapping. |
| Audit Para Snap | Date-only parent with independent date-range policy, nine lifecycle KPIs and existing `P_GET_KPI_LIFECYCLE` are preserved. Its disabled Oracle/API features still need separate development database compilation, verification and approved activation. N/A remains N/A. |
| CIA Command Center | IAS audit-period IDs remain independent of calendar date ranges. Operational KPIs remain current-state. Ten approved nature labels and unmapped reconciliation are unchanged. The user-reported 14,604 unmapped records still require approved source-ID classification in `T_IAS_CIA_ISSUE_NATURE_MAP`; no mappings are inferred here. |

## Validation

- `dotnet build AIS/AIS.csproj --no-restore --configuration Debug --verbosity quiet`:
  passed, zero errors (57 existing compiler warnings).
- `node --test tests/executive-dashboard/*.test.cjs`: passed, 30 checks including
  real client execution without organizational inputs and preservation of Oracle
  signatures, Report A–E, CIA labels/reconciliation and independent Snap wiring.
- `tests/executive-dashboard/role-parent-policy.ps1`: 45 compiled filter checks.
- `tests/executive-dashboard/cia-command-policy.ps1`: 23 Snap date/default checks,
  18 Snap controller access checks, 13 CIA period/configuration checks and 22 CIA
  controller access/gate checks. No Oracle connection is opened by these checks.

No authenticated browser or development Oracle database validation was performed.
Review Group/Super User root selection and regional appearance with real IAS
development sessions before acceptance. No production configuration, SQL source,
deployment or database data was changed. Local appsettings and generated-file
edits present before this work are excluded from the branch commits.
