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
