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
 reassigned to a different perspective. Client parameters never select the type/root.

## Database permissions and navigation

Run `sql/executive_dashboard_permissions.sql` after verifying role IDs and page-ID
availability. It creates dedicated MVC menu pages 1000300..1000303 and six GET API
master registrations. It grants only verified CIA/CCO roles by default. TOP/GROUP
variables default to NULL (no grants). It does not copy grants from operational pages.
The Page ID archive supplies matching route IDs. IAS menu/page grants use
T_MENU_PAGES_GROUPMAP; API grants use T_AU_ROLE_API_PERMISSION and T_AU_API_MASTER,
loaded by PKG_LG.p_GetApiPermissions. Re-login after changing permissions.
Ensure IAS role/group mapping resolves each intended role to its separately granted page.
No page or API permission exemption is added. Executive actions also check actual
cached page/API grants, including for Super User. Shared API paths require a separate
role-specific page grant before returning data.

The application Oracle principal needs EXECUTE on PKG_EXEC_DASHBOARD,
PKG_ORG_STRUCTURE (P_GET_NODES), and the existing PKG_PG audit-period lookup.
Deploy the latest supplied package and its dependencies as database owner; frontend
uses only IAS HTTP endpoints. Audit period is the package's exact text value.
Critical maxRows defaults to 100 and accepts 1..500; entity IDs must be positive and
present in the permitted server-derived directory. Unknown or out-of-scope IDs deny.

## Frontend and focused verification

One reusable Razor page is served at /ExecutiveOverview/TOP, /GROUP, /CCO and /CIA.
Each route must match the session-derived type and its dedicated page grant.
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
