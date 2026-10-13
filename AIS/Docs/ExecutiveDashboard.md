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

## Modular Executive Dashboard (phase 1)

The four authorized entry routes now default to **At a Glance**. The shared shell
selects a reusable view from backend definitions only after the existing session,
role and page authorization checks. Unknown view IDs return HTTP 400. View selection
is presentation state; the six existing API permission and hierarchy checks remain
unchanged, as do their Oracle bindings and calculations.

`ExecutiveDashboardViews.ForPerspective` defines KPI keys, ordered chart/table
sections and report catalogue entries for the resolved perspective. Glance emphasizes
bank-wide position/risk for TOP, local aging for GROUP, compliance cycles for CCO,
and risk/settlement effectiveness for CIA. Common chart and critical-table partials
and the existing JavaScript renderers are reused by Audit Para Position,
Settlement & Performance, Risk, and Aging & Compliance. All existing widgets remain
available across these views. Risk and Aging & Compliance are separate navigation
items per this phase's requested scope.

The Reports view lists relevant planned reports (TOP/CIA A–E, GROUP E, CCO B–D).
It makes no reporting calls and clearly marks reports as unavailable pending the
verified reporting phase. It supplies no invented data or new report permission grants.
The existing audit-period/entity filters stay in place; date filters, cascading filters,
formal report layouts, exports and new Oracle calculations are deferred.

Super User perspective links use the original authorized routes. GROUP still requires
a valid selected root before any dashboard API loads. Navigation within a perspective
preserves the root and selected entity; filter changes update view links. Ordinary
users retain their session-derived perspective and hierarchy restrictions.

Focused verification: AIS net8.0 build succeeded with 0 errors and 57 existing warnings;
JavaScript syntax passed. Live authenticated role/Oracle verification remains a deployment check.

## Shared dates and hierarchical filters (phase 2)

One `_Filters.cshtml` GET form is shared by Glance and all five additional views,
including Reports. Dates and all organizational selections are carried by view links;
parent changes clear lower selections before reloading server-generated options.
Switching the Super User GROUP root preserves dates and resets organizational choices.
An unselected GROUP root never loads directory choices or summary APIs.

`ExecutiveDashboardFilterRequest` is the common MVC/API contract:
`startDate` / `endDate` are optional together, strict ISO `yyyy-MM-dd` calendar dates;
`selectBy` is `entityType` or `auditedBy`; `dimensionId`, `reportingOfficeId` and
`entityId` are optional positive IDs. Dates must be ordered and the end must allow
an exclusive next-day bound. Future procedures should bind `P_START_DATE` and
`P_END_DATE` with `OracleDbType.Date`, and use `[StartDate, EndExclusive)` for event
activity. Opening/closing positions still require their own history-aware definitions.
No date is translated to `P_AUDIT_PERIOD` or passed to the existing procedures.
Legacy API callers may continue using the unchanged audit-period parameter.

The dedicated `DBConnection.ExecutiveDashboardFilters.cs` reuses
`PKG_AD.P_GET_AIS_ENTITIES` (its name-search branch with a bound wildcard) and
`PKG_ORG_STRUCTURE.P_GET_NODES`. It intersects the directory with the existing
server-authorized scope before constructing any options. Entity type is the
unambiguous organizational category; Audited By is the entity directory's current
`auditby_id`, a distinct dimension rather than the reporting parent. Reporting Office
is `REPORTING_1`, consistent with `V_EXEC_ENTITY_SCOPE`. GROUP options exclude offices
outside its hierarchy. Missing/ambiguous categories cannot elevate scope. Parent-child
compatibility is revalidated on the MVC page and summary APIs; forged IDs are rejected.
No bank-wide directory is sent to the browser for client-side filtering. No new SQL
package or permission exemption is introduced. The Oracle principal also needs EXECUTE
on the existing `PKG_AD` lookup package.

Compatibility boundary: organizational dimensions narrow the available Entity choices.
The six legacy summaries can apply only the selected Entity hierarchy (or authorized
perspective/root when no Entity is selected), not an exact type/auditor/office predicate
or the selected dates. The shared page note, loading status and API response metadata
explicitly disclose this. Current directory Audited By is not evidence of a historical
para's audit/settlement source. Exact dimensional reporting and date activity remain
for compatible future reporting procedures; this phase implements no Reports A–E.

Backend view definitions list required datasets. The browser requests only those
APIs; Reports requests none. Requests carrying `view` are checked against the resolved
perspective's dataset definition after the existing page and API permission gates.
Direct legacy calls without a view retain their previous behavior. Session-derived
ordinary perspectives, selected Super User perspective, GROUP root and entity hierarchy
validation remain intact.

Focused verification: one AIS net8.0 build succeeded. JavaScript syntax and temporary
basic checks covered ISO/invalid/reversed dates, the exclusive end bound, both dimensions,
outside/incompatible IDs, per-view dataset lists, cascade resets, navigation persistence
and Reports making no data calls. No live Oracle or extensive test suite was run.

When a type/auditor or office is selected without an Entity, the UI withholds legacy
figures and asks for an Entity rather than displaying wider perspective totals under
an unsupported dimensional filter. The API continues disclosing its legacy scope mode.

## Reports backend (phase 3)

See [ExecutiveReports.md](ExecutiveReports.md) for the staged Reports A–E package,
certified history dependencies, source evidence, deployment order and verification.
New APIs are disabled by default. The original Oracle dashboard package and layouts
are unchanged. Legacy summary APIs now reject DimensionId/ReportingOfficeId instead
of returning unsupported broader figures, and no longer perform the full directory
lookup merely because an unfiltered Select By mode was supplied. This supersedes the
phase-2 compatibility behavior described above.


## All matching entities and Reporting Line Management

The 6 `PKG_EXEC_DASHBOARD` procedures now accept optional bind parameters:
`P_ENTITY_TYPE_ID`, `P_AUDITED_BY_ID`, `P_REPORTING_OFFICE_ID`.
`P_ENTITY_ID` remains optional for bank-wide authorized perspectives and represents
an authorized subtree when set. With Entity = All matching entities, the dimensions
select the intersecting portfolio across all matching entities. Reporting Office
includes the selected office **and its descendants** via `REPORTING_1..6` from
`V_EXEC_ENTITY_SCOPE`. Audited By uses observation-level `AUDITED_BY`;
Entity Type uses observation `ENTITY_TYPE_ID`. No client-supplied ID can
increase the authenticated scope. Dates remain displayed but are **not** applied
to these six current-state/legacy summary APIs.

The in-dashboard TOP/GROUP/CCO/CIA buttons were removed. Existing perspectives
and separately permissioned routes remain available through existing navigation.

`/ExecutiveOverview/RLM` is Reporting Line Management. It shares all modular
dashboard views, but removes Select By/Entity Type/Audited By controls. It offers
dates, Reporting Office and Entity. The server always derives the parent/root from
`SessionUser.UserEntityID`; no query parameter can override this root.
Descendant selection is checked against `PKG_ORG_STRUCTURE.P_GET_NODES` via
`GetOrganizationNodes(root)`. An empty Entity selection means **all children and
descendants of the logged-in entity**, including its own observations.
`RLM` uses the existing `/Dashboard/entity_wise_obs` page permission and
session-validated `/api/ExecutiveDashboard` endpoints.

**Deployment order** (Oracle 18c): (1) compile the updated
`AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql` view/package definition before
deploying the C# application; (2) inspect USER_ERRORS in the target schema;
(3) deploy the updated application; (4) review
`AIS/Docs/sql/executive_reporting_line_permissions.sql` with DBA: its
`V_ENABLE` defaults to `N` and it must be intentionally changed to `Y`
for permitted roles to call the six read-only APIs; (5) re-login to refresh
role/API permissions; (6) verify TOP/GROUP/CCO/CIA filters and RLM login-root
scope using real organizational hierarchies. Test that forged
`groupRootId`, out-of-line `entityId` and invalid office IDs fail.

SQL Developer clients differ in keyboard shortcuts. In the user's environment,
F8 executes statements and F5 displays Explain Plan: compile each `CREATE`
statement separately if the client cannot execute multi-statement scripts.
Do not execute raw SQL*Plus-only directives through the statement executor.
No live Oracle compilation or role/API integration test was performed during
this GitHub edit.

**Reporting limitation:** The Reports catalogue remains staged; the five
`PKG_EXEC_REPORTS` endpoints do not yet accept RLM as a formal reporting
perspective. RLM summary widgets work after deployment/grants, but A–E remain
placeholders pending separate controlled report integration.
