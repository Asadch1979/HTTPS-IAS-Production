# Executive dashboard permissions, CIA Command Center, and report palettes

Source branch: `feat/integrate-role-oracle-dashboards-20261017` / draft PR #314.
These are source changes only: no Oracle script executed and no production deployment.

## 1. Super User registration

`AIS/Docs/sql/executive_dashboard_permissions.sql` is a **DBA-run** script that:
- keeps known Super User IAS role `1` and preserves verified CIA `3` and CCO `38`;
- registers four perspective pages (`TOP=1000300`, `GROUP=1000301`, `CCO=1000302`, `CIA=1000303`) for Super User, plus optional separately verified additional TOP/GROUP role IDs;
- links Super User to existing `Dashboard/entity_wise_obs` (RLM alias), `FieldAudit/AR_Dashboard` and `ManagementAudit/MA_Dashboard` page records **if those exact page paths exist**, reporting absent pages for DBA attention. No duplicate RLM page ID is created;
- adds GET endpoints `GetCiaCommandKpis`, `GetCiaIssueNature`, `GetRoleEntityHeatmap`, `GetRoleUnitComparison`, `GetRoleFollowups`, plus the preexisting core summary APIs and lifecycle;
- grants CIA-only endpoints to verified CIA and Super User; generic role endpoints to Super User and verified GROUP where relevant. Ordinary RLM role API grants must be independently approved after confirming role IDs. It does **not** indiscriminately grant all roles access;
- keeps report A-E API permissions under the independent `executive_reports_permissions.sql` activation control.

**Before DBA execution:** ensure the IAM table `T_MENU_PAGES_GROUPMAP.GROUP_ID=1` actually denotes the Super User menu group in the target environment (the existing script assumed this). Verify unique reserved page IDs, Oracle schema `T_AU_API_MASTER` columns, existing inactive API records, and role/page grants. The SQL script contains COMMIT and is **not** a read-only preflight. Back up affected registration rows before execution.

Super User interface includes navigation across TOP / CIA / CCO / GROUP / RLM. On RLM, an explicit organizational-root selector requires exactly one verified Group, Division, GM, or Region with entity types **18, 3, 21 or 5**. Without a root, no scoped API query runs. Ordinary RLM users cannot supply a root and stay bound to their own authenticated IAS posting. Super User cannot impersonate a Team Leader: the TeamMate Workbench still requires active leader/engagement assignments.

## 2. CIA Command Center restored separately from TOP

The CIA `/ExecutiveOverview/CIA` route now defaults to `view=command`; explicit `view=glance` remains available. TOP's **nine lifecycle KPIs** are unchanged. CIA has a dedicated navy/teal HighBond-inspired screen with:
1. Audit Plan Coverage (planned entities, audited entities, percent) for the selected IAS period.
2. Delayed Audit Reports (approved audit-end + 15 calendar days operational criterion).
3. A1 observations detected in the selected IAS period. Label intentionally says **A1 observations** rather than criminally established fraud.
4. Observations awaiting auditee response (status 2), current state.
5. Teams conducting audit, current state.

The second row displays all ten approved Nature of Issue Identified categories. It reveals the unmapped count and flags incomplete category mapping rather than silently presenting incomplete chart totals.

Oracle endpoints are `GetCiaCommandKpis` and `GetCiaIssueNature`, with typed DTOs in `AIS/Models/CiaCommandCenterData.cs` and package readers `AIS/DBConnection.CiaCommandCenter.cs`. The audit period selector uses actual `T_AU_PERIOD.AUDITPERIODID` values from existing `DBConnection.GetAuditPeriods()`, not a calendar year guessed to equal an Oracle key. Authenticated CIA/Super User and existing page/API controls apply.

Set `RoleDashboards:CiaCommandCenterEnabled=true` **only after** verifying:
- `PKG_EXEC_DASHBOARD.P_GET_CIA_EXEC_KPIS` and `P_GET_CIA_ISSUE_NATURE` compile;
- `T_IAS_CIA_ISSUE_NATURE_MAP` exists, approved IDs are populated and `SUM(ISSUE_COUNT)+UNMAPPED_ISSUES=TOTAL_ISSUES`;
- chosen period, A1 annexure, 15-day SLA and active audit stages reconcile with live IAS records;
- approved GET API permissions and denied-role tests.

While flag is false (default), the CIA's dedicated Command Center layout is visible with N/A and a clear activation note, not fake metrics. Its original CIA At a Glance data remain accessible as a separate tab.

## 3. Distinct visual identities

| Area | Distinct scheme |
|---|---|
| TOP executive lifecycle dashboard | Steel-blue and slate |
| CIA Audit Command Center | Navy and teal, separate five-KPI layout |
| CCO dashboard | Warm gold and sand |
| GROUP/Divisional management | ZTBL bank green |
| Generic RLM reporting line | Indigo-slate |
| GM/Regional Audit Pulse | Midnight dark navy, mint, amber/red semantic risk |
| Team Leader Workbench | Existing TeamMate-style royal blue |
| Executive Report A | Indigo, annual audit progress |
| Executive Report B | Burnt orange, field office position |
| Executive Report C | Teal, GM/Region position |
| Executive Report D | Violet, Top/Bottom Regions |
| Executive Report E | Forest green, division settlement |

`AIS/wwwroot/css/executive-themes.css` scopes dashboard colors by `data-perspective` and report colors by already-set `#exec-reports[data-active-report="A"..."E"]`. Risk priority colors retain their common meaning. The server-generated searchable PDFs use corresponding report palette colors in headings, table headers, totals and running headers; calculations, pagination and report export/permissions are unchanged.

## 4. Mandatory verification

- Focused .NET build of `AIS.sln`, Razor partial resolution and JS syntax checks.
- DBA-run `verify_exec_cia_command_F8.sql`, `verify_role_dashboards_F8.sql` and review of existing API/page registrations before running `executive_dashboard_permissions.sql`.
- Super User: TOP/CCO/CIA/GROUP/RLM navigation; confirmed single-root-only selection for Divisional/GM; no null-root or foreign-root API escalation.
- Ordinary roles: session-bound RLM and Team Leader; 401/403 for unauthorized perspective or manipulated root/engagement; exact API path grants.
- Report A-E tabs and downloaded PDFs with distinguishable palette and unchanged figures.
- CIA: selected-period ID and category mapping diagnostics, explicitly selected At a Glance stays distinct from Command Center.
- Compare Oracle and UI figures before enabling role-dashboard feature flags.

No .NET SDK, authenticated IAS runtime or live Oracle connection has been available for this GitHub-only implementation. Do not merge PR #314 into main without review and deployment approval.
