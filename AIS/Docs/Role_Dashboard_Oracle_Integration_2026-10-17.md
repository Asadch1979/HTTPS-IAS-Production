# IAS role-dashboard integration — controlled deployment

Branch: `feat/integrate-role-oracle-dashboards-20261017` (source implementation only).
Baseline: main after PR #313. **No live Oracle verification or production deployment was performed by this branch.**

## Scope and procedure wiring

| Procedure | C# consumer | Screen / source |
|---|---|---|
| `PKG_EXEC_DASHBOARD.P_GET_ROLE_ENTITY_HEATMAP` | `GetRoleEntityHeatmap` | Regional dark dashboard, separately scoped entity types 6 and 28 (two bulk queries, not N per-branch requests) |
| `P_GET_ROLE_UNIT_COMPARISON` | `GetRoleUnitComparison` | Group/Divisional management subordinate-unit table |
| `P_GET_ROLE_FOLLOWUPS` | `GetRoleFollowups` | Group/Division/Region priority follow-up table, top 100 open paras |
| `PKG_TEAM_LEAD_DASHBOARD.P_GET_SUMMARY` | `GetTeamLeadDashboardSnapshot` | Team Leader workbench counters |
| `P_GET_ENGAGEMENTS` | same | Workbench per-engagement status and dates |
| `P_GET_WORKPAPERS` | same | Latest-version Working Papers V2; must be installed |
| `P_GET_OBSERVATIONS` | same | Status 2/3 observation follow-ups, bounded 100 rows |

Three new GET APIs on the existing authorized `ExecutiveDashboardController`:
`GetRoleEntityHeatmap`, `GetRoleUnitComparison`, `GetRoleFollowups`.
They use the existing role resolver, page ID grant, exact API permission, session revocation, filtering, and descendant scope checks. A selected reporting office is converted to an authorized narrowed root because the new SQL cursors do not have a separate reporting-office parameter. Only GROUP (type 18) and explicitly allowed RLM division/group (type 3/18) may request management data. Regional bulk heatmap requires RLM GM/Region root (type 21/5). User-provided PPNO and root are never accepted by the Team Lead reader.

Team Lead uses server-side read-only cursors under the existing approved `AR_Dashboard?workbench=true` and `MA_Dashboard?workbench=true` routes. Authorization is checked before workflow selection, then independently within Oracle. The page filters detailed engagement/paper/observation rows to its authorized assignment list; summary counts reflect all the Team Lead's eligible IAS assignments.

## Safe activation — both switches default false

- `RoleDashboards:OracleReadEnabled=true` enables new management/region Oracle panels and related GET API queries.
- `RoleDashboards:ManagementRlmRoleIds=[...]` is a **required IAS-approved role-ID allowlist for Divisional/Group users on RLM** (entity type 3/18); default empty denies the new management view. Obtain exact IDs from the live role catalog and authorization approval, never guess or grant all roles. The existing verified GROUP perspective does not use this allowlist.
- `RoleDashboards:TeamLeadOracleEnabled=true` enables the four Team Lead cursors on workbench pages.
- Do not enable either until its package and dependencies compile in production. Existing GROUP/RLM KPI charts and original workbench remain available while switches are false.
- The `WorkingPapersV2:Enabled` rollout and installed `T_WP_HEADER` / `T_WP_ITEM` objects must be assessed before enabling TeamLeadOracle. V2 may be installed but disabled for actions; the new reader is read-only.
- Treat IAS role/page/API mappings as configuration-sensitive; enable only for specifically verified authorized personnel. Do not grant RLM to a generic role simply to show the management layout.

## DBA/manual checks before enabling

1. Run `AIS/Docs/sql/verify_role_dashboards_F8.sql` in Oracle as the IAS schema owner. Check both package specs/bodies, `USER_ERRORS`, and actual `USER_ARGUMENTS`.
2. Reconcile direct `V_EXEC_PARA_BASE` counts for a known Group (type 18), Division (type 3), GM/Region (type 21/5) and one descendant branch (6/28). Cross-check current open/settled balances, risk, ages and compliance cycles. A capped follow-up or heatmap list must never be summed as a portfolio total.
3. Register and approve **GET** API permissions for all three new `/api/ExecutiveDashboard/GetRole...` routes using the existing IAS API-master administration workflow. Do not guess registration IDs or grant universal access. Re-sign in to refresh cached permissions.
4. After configuring the verified `ManagementRlmRoleIds` allowlist, verify explicit `view=glance` still works; verify default Group and authorized Divisional landing page shows management; Regional uses dark view; TOP, CIA, CCO and Reports A–E remain unchanged.
5. Denied roles, revoked sessions, sibling/foreign entities, manipulated `groupRootId`/`entityId`, and foreign Team Lead `engId` must fail with 401/403. Verify API access independently of button visibility.
6. Compare Team Lead cursor counts/statuses with actual assignments and enabled V2 objects. Audit-status codes are **not** guessed into Review/Reporting kanban lanes; source task status is displayed as-is.
7. Complete one focused ASP.NET solution build and browser smoke check in the actual runtime. No .NET SDK or Oracle connection is available to this GitHub-only implementation environment.

## Known limitations / next approved design work

- The management follow-up cursor returns status/stage codes; it cannot truthfully identify an auditee decision or verified overdue-remediation deadline. It is an **open-para follow-up list**, not a management decision queue.
- Oracle `P_GET_ROLE_UNIT_COMPARISON` has no auditee-type or audited-by filter; the management view does not expose the dimension selector. Regional branch heatmap supports types 6/28, combined and capped at 250. Follow-ups are limited to 100; no claim of exhaustive rows.
- Team Lead workpaper `PASSED_ITEMS / TOTAL_ITEMS` is not a certified workflow-completion percentage. Review/Reporting lane placement awaits authoritative stage mapping. Working Papers V2 may be turned off separately.
- `P_GET_ROLE_FOLLOWUPS` may contain statuses that require catalog mapping before human-readable management decision categories can be displayed.
- The Group dashboard requires a valid authorized Group root; a Division Head uses existing RLM route with posting entity type 3. Do **not** create a new bank-wide Division perspective.
- The existing stored procedures still trust the session-authorized `P_ROOT_ENTITY_ID` supplied by application code; do not expose direct package EXECUTE to end users.

Deployment order: existing roles and Oracle objects -> verify SQL cursors -> API grant registration -> deploy code with feature flags false -> authenticated negative checks -> enable flags one at a time -> reconcile records -> approve rollout. No automatic main merge.
