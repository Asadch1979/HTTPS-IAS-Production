# Executive Dashboard deployment

Reviewed commit 64c0901 and PKG_EXEC_DASHBOARD.sql on main (14379e21).
The package supports all four types, optional hierarchy filters and six procedures.
No Oracle package changes are included.

## Configuration template (merge into environment configuration)

```json
{
  "ExecutiveDashboard": {
    "RoleMappings": [
      { "RoleId": 0, "DashboardType": "TOP", "PagePath": "/ExecutiveOverview/TOP", "AllowBankWide": false },
      { "RoleId": 0, "DashboardType": "GROUP", "PagePath": "/ExecutiveOverview/GROUP", "AllowBankWide": false },
      { "RoleId": 38, "DashboardType": "CCO", "PagePath": "/ExecutiveOverview/CCO", "AllowBankWide": true },
      { "RoleId": 3, "DashboardType": "CIA", "PagePath": "/ExecutiveOverview/CIA", "AllowBankWide": true }
    ]
  }
}
```

CIA=3 and CCO=38 are verified by the repository's
`Codex/IAS_Comprehensive_Review_2026-08-28/Role_Permission_Matrix.csv`.
TOP and GROUP IDs are NOT verified in repository evidence: 0 disables those entries.
Verify against the deployment's IAS role catalog before replacing them; never substitute
Super User, general Compliance Officer (9), Regional Head (23), or GM (39).
Enable TOP AllowBankWide only after confirming the role's bank-wide entitlement.
GROUP must keep AllowBankWide=false and have a positive session entity with business
 group category 18. Missing/duplicate role mappings, shared permission paths, invalid
 types, or missing explicit bank-wide approval deny access. Roles 3/38 cannot be
 reassigned to a different perspective. Ordinary users cannot change the session-derived
 type/root. Role 1 uses the authenticated Super User exception described below and
 must not be added to RoleMappings.

## Database permissions and navigation

Run `sql/executive_dashboard_permissions.sql` after verifying role IDs and page-ID
availability. It creates dedicated MVC menu pages 1000300..1000303 and six GET API
master registrations. It grants Super User (1) and verified CIA/CCO roles by default. TOP/GROUP
variables default to NULL (no grants). It does not copy grants from operational pages.
The Page ID archive supplies matching route IDs. IAS menu/page grants use
T_MENU_PAGES_GROUPMAP; API grants use T_AU_ROLE_API_PERMISSION and T_AU_API_MASTER,
loaded by PKG_LG.p_GetApiPermissions. Re-login after changing permissions.
Ensure IAS role/group mapping resolves each intended role to its separately granted page.
No anonymous page or API permission exemption is added. Ordinary executive accounts
continue checking actual cached page/API grants. Role 1 uses the existing IAS
PermissionService Super User policy after session and perspective validation.
Shared API paths require an ordinary user's separate role-specific page grant.

The application Oracle principal needs EXECUTE on PKG_EXEC_DASHBOARD,
PKG_ORG_STRUCTURE (P_GET_NODES), and the existing PKG_PG audit-period lookup.
Deploy the latest supplied package and its dependencies as database owner; frontend
uses only IAS HTTP endpoints. Audit period is the package's exact text value.
Critical maxRows defaults to 100 and accepts 1..500; entity IDs must be positive and
present in the permitted server-derived directory. Unknown or out-of-scope IDs deny.

## Frontend and focused verification

One reusable Razor page is served at /ExecutiveOverview/TOP, /GROUP, /CCO and /CIA.
For ordinary users, each route must match the session-derived type and its dedicated page grant.
The menu registration supplies IAS navigation; authorized Organization Structure
users also get dashboard links for the page and selected entity. The dashboard link
back to Organization Structure is shown only with its existing page permission.
Entity lists are loaded server-side, restricted for GROUP, and API requests revalidate
selected IDs. Period options use existing PKG_PG.P_GetAuditPeriods DESCRIPTION values
(the exact text is passed as P_AUDIT_PERIOD, not a numeric period ID).

The page renders eight KPIs, risk/aging/cycle/trend charts with accessible data tables,
and critical observation details. CCO/CIA automatically receive the package's finer
cycle categories. Data comes only from the six APIs; no sample metrics are supplied.
All observation/entity text is rendered through Razor encoding or DOM textContent.
Refresh hides stale figures, cancels previous requests and reports session/access,
loading, empty and failure states. Critical Items is the only actionable KPI because
only critical details have a verified package endpoint. Other KPIs/chart categories
remain informational. Critical details are capped at 100 rows and explicitly labeled;
no unsupported general observation drill-down or Audit Coverage is implemented.

Deployment gaps: confirm TOP/GROUP role IDs, apply menu/API grants, configure mappings,
and verify live authenticated Oracle responses after deployment. Local checks cover
compilation, JavaScript syntax and route/response/authorization consistency only.

Verification completed: AIS net8.0 build succeeded with 0 errors (57 existing warnings); both JavaScript files passed node --check. A temporary controller/route check confirmed unauthenticated rejection for six API and four MVC routes and four distinct page mappings. Source consistency checks confirmed all six procedure/API registrations, P_MAX_ROWS, strict page/API gates and no package modification. No live Oracle calls or extensive test suite were run.

## Super User access correction

Reviewed GitHub main commits 1f4f899 and 368b47e. They required a unique role mapping
and direct cached page/API grants even for Role 1, overriding IAS Super User policy.
Authenticated Role 1 now selects TOP/GROUP/CCO/CIA by the MVC route without changing
the user's IAS role or posting. The Razor page passes its validated `dashboardType`
to all six existing GET APIs. Direct Super User API requests must supply that value.
Only the four known perspectives are accepted. Ordinary users may omit it, or supply
their own mapped type; impersonating another type is denied.

`/ExecutiveOverview/GROUP` opens a Group Head selector for Super User. No dashboard
API request runs before selection. Load Group submits `groupRootId` and the server
verifies category 18 and rejects ambiguous/nonexistent entities. All six GROUP APIs
require this root; `entityId` may narrow it only to its permitted descendants.
Missing roots never request bank-wide data. Ordinary GROUP accounts retain their
session entity as root and cannot override it. Scope snapshots retain the actual
session role, posting, assignment and session ID separately from the selected root.

The IAS sidebar supplies missing Super User dashboard links immediately, showing
them only for an active authenticated Role 1 session. No duplicate links are added
when the database menu already supplies them. The existing registration script now
also grants Role 1 the four page registrations and six APIs for database-backed menu
consistency. Deploy it after checking page IDs; it does not add Role 1 role mappings
or widen any ordinary user's grants. Re-login after applying database grants.
All four routes are present in the current Page ID.xlsx with IDs 1000300..1000303;
the user's existing workbook/archive edits are not included in this fix.

Controller denial logs identify `SessionValidation`, `RoleMapping`, `PagePermission`,
`ApiPermission` or `EntityScope`, plus user ID, actual role, perspective and route.
The common IAS permission guard continues logging permission failures before actions.
Charts, KPIs, filters, response envelopes and all six Oracle procedure bindings remain
unchanged. PKG_EXEC_DASHBOARD.sql is not modified.

Focused verification: AIS net8.0 compilation succeeded with 0 errors and the same
57 existing warnings; dashboard JavaScript syntax passed. A temporary harness ran
the actual dashboard controllers and scope resolution with isolated session/menu/
Oracle dependencies. All four Super User pages and all six APIs per perspective
were authorized; GROUP required a valid selected root and preserved it when narrowed.
The harness rejected ordinary-user perspective/root overrides, missing page/API
grants, invalid/revoked sessions, unknown/ambiguous roots and out-of-hierarchy entities,
and checked all five denial diagnostic categories. No live Oracle verification or
extensive application test suite was run. GitHub main was verified at 368b47e.
