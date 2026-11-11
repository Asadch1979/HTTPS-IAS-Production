# Executive dashboards: page-bound GET API permissions (production deployment)

**Change scope:** Six approved IAS pages (menu 16), plus the existing RLM page that is already assigned in IAS. **No role-to-API activation is required.** All non-dashboard IAS API authorization remains unchanged.

| Page ID | Validated page path | GET API dependencies |
|---|---|---|
| 1000401 | HeadAuditDepartment/AuditTeamsMonitoring | HeadAuditDepartment Engagements, Work, Evidence, Report, ResponseEvidence |
| 1000304 | ExecutiveOverview/AuditParaSnap | AuditParaSnap GetKpis, GetDistribution, GetCriticalItems, GetPerformance, GetParaText, GetReportA–E |
| 1000303 | ExecutiveOverview/CIA | ExecutiveDashboard shared summaries, GetCiaCommandKpis, GetCiaIssueNature; ExecutiveReports A–E |
| 1000302 | ExecutiveOverview/CCO | ExecutiveDashboard shared summaries; CcoCompliance FunctionPosition/Org*; ExecutiveReports B–D |
| 1000301 | ExecutiveOverview/GROUP | ExecutiveDashboard shared summaries, GetRoleUnitComparison, GetRoleFollowups; ExecutiveReports E |
| 1000300 | ExecutiveOverview/TOP | ExecutiveDashboard shared summaries and TOP-only GetKpiLifecycle |
| **1000305 (existing, preserve)** | **ExecutiveOverview/RLM** | ExecutiveDashboard shared summaries, GetRoleEntityHeatmap and GetRoleFollowups |

**RLM note:** 1000305 is not in the supplied six-row list, but is already the independent authenticated RLM page in source. Reusing 1000301 for its distinct route would mismatch page resolution and break access. Verify the existing row and its actual group assignments; do not guess a new page ID.

## Semantics

- API eligibility = live authenticated IAS session + an **active assigned page** (`T_MENU_PAGES_GROUPMAP`, `T_MENU_PAGES`) + active exact GET dependency (`T_IAS_EXEC_PAGE_API`, `T_AU_API_MASTER`) + existing dashboard-specific perspective, scope, page, filter and dataset checks.
- `T_AU_API_MASTER.PAGE_ID` remains the single legacy registration anchor for shared URLs. **Do not duplicate API_MASTER routes** to force different `PAGE_ID` values. `T_IAS_EXEC_PAGE_API` is the many-to-many page/API dependency.
- `PKG_LG.P_GetApiPermissions` loads these dashboard families by the authenticated user's assigned page, **not** `T_AU_ROLE_API_PERMISSION`. It retains the old role-based rules for every *other* IAS API family, including Field Audit.
- `PKG_AD.P_AddGroupMenuItemsAssignment` and `P_RemoveGroupMenuItemsAssignment` stop activating/revoking role API grants for those seven pages. Page assignment is the entitlement; other IAS page assignment behavior is unchanged.
- Each HTTP controller additionally verifies the exact `PAGE_ID`, session group assignment and registered active path/verb directly from Oracle. The browser cannot supply or override the page ID. A cached grant cannot replace a revoked page assignment.
- Existing active role/API rows are **not deleted or modified**. They no longer independently authorize these seven dashboard API families.

## DBA preflight (read-only)

1. Back up `PKG_LG` and `PKG_AD` **both specs and bodies**, and export current `T_MENU_PAGES`, `T_MENU_PAGES_GROUPMAP`, `T_AU_API_MASTER`, `T_AU_ROLE_API_PERMISSION` and existing `T_IAS_EXEC_PAGE_API` (if present).
2. Confirm exact page IDs, paths, `MENU_ID=16` for the six supplied rows, active status, and **RLM 1000305** separately.
3. Verify every endpoint in `AIS/Docs/sql/executive_dashboard_page_api_manifest.sql` already exists as **one ACTIVE GET row** in `T_AU_API_MASTER`. Do not run historical registration SQL if it would create unexpected roles or alter page IDs. Do not activate an unknown endpoint or bypass a disabled dependency.
4. Verify `T_IAS_EXEC_PAGE_API` structure (PAGE_ID, API_ID, IS_ACTIVE, unique PAGE_ID/API_ID), Oracle ownership/grants, and current disabled dependencies. The manifest aborts if an expected dependency already exists but is disabled.
5. Confirm existing exact API routes under the IAS PathBase (e.g. `/ZTBLAIS`), while storing application-relative `/api/...` routes in Oracle.

## Deployment order (must not be reversed)

1. Run the **reviewed** `AIS/Docs/sql/executive_dashboard_page_api_manifest.sql` as the established IAS schema owner. It creates the dependency table if absent; registers only known GET endpoints; preserves existing disabled dependencies; never mutates role grants. It aborts and rolls back data changes on missing, duplicate, inactive, or wrong-page registrations.
2. Compile **`AIS/Docs/sql/PKG_LG.sql`** and check `USER_ERRORS`, first. Confirm the existing `p_GetApiPermissions` signature is unchanged. Its new page lookup requires the dependency manifest.
3. Compile **`AIS/Docs/sql/PKG_AD.sql`**, then check `USER_ERRORS`. Confirm add/remove page assignments work without changing role/API grants.
4. Build and deploy the application with `DBConnection.ExecutivePageApiPermissions.cs` and all five updated controllers. **Do not deploy the application before the manifest and package compilation.**
5. Restart IIS; require existing dashboard users to log out and log back in to refresh stored API permissions.
6. Verify exact action/API and page access using real authenticated Super User, CIA, CCO, Group Head, RLM GM/Region, and Head Audit sessions. Confirm authorized data values, dashboards, exports, and compliance records match predeployment positions.

## Required fail-closed tests

- A role with no assigned executive page must **not** receive or invoke any dashboard API, even if `T_AU_ROLE_API_PERMISSION` still contains an ACTIVE grant for that role.
- A user with only the CCO page cannot invoke TOP-only `GetKpiLifecycle`, CIA Command APIs, Head Audit APIs or Group Report E.
- A user with only Audit Para Snap cannot inherit CIA, TOP or CCO APIs. Shared API paths must resolve only through the explicitly assigned page mapping.
- Revoking a page assignment denies its APIs **without waiting for the next login**; an API linked to a *different remaining assigned page* continues to work.
- An API whose `T_IAS_EXEC_PAGE_API.IS_ACTIVE='N'` or `T_AU_API_MASTER.IS_ACTIVE='N'` must return 403. Unknown/duplicate route-verb registrations must fail the manifest preflight.
- Super User must still hold the requested page and pass live IAS scope validation; it does not bypass a missing page-to-API dependency.
- Recheck CCO Function Wise Position, Audit Para’s, CCM Report E, GM/Region Pulse, CIA Command Center, Head Audit monitoring, Audit Para Snap report/PDF export, and existing Group/RLM totals.
- Test a wrong HTTP verb, query-supplied fake page ID, stale session, wrong reporting root and attempted cross-dashboard calls. All must deny.
- Verify unrelated ordinary IAS/Field Audit API grants are unchanged.

## Rollback

Revert the application code and restore the backed-up `PKG_LG`/`PKG_AD` bodies together if acceptance fails; retain the dependency table as inert metadata or reconcile it through a separate reviewed DBA change. **Never DROP the table or delete role grants as an automatic rollback.** Preserve the exported baseline to compare all page/API assignments and role decisions. Recheck that `PKG_EXEC_DASHBOARD.sql` and report packages remain unchanged.

**Current status:** source-only PR. Oracle compile, test-suite execution, IIS deployment and live IAS UAT were not performed by ChatGPT. Production activation requires those checks.
