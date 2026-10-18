# Executive dashboard permission alignment after PRs #321 and #322

**Status:** source-only feature branch. Neither the database permission
registration nor its read-only audit has been executed in IAS Development.
No production change, merge or API activation has occurred.

## Findings

- PR #321 merged first, and PR #322 was based on its merge result.
  PR #322 changes controllers, parent Razor views, C# filters, JavaScript,
  CSS and tests; it does **not** overwrite the corrected
  `PKG_EXEC_DASHBOARD.sql`, `V_EXEC_KPI_LIFECYCLE.sql`,
  `V_EXEC_REPORT_SOURCE_image_rewrite.sql`, or reconciliation view.
  No reapplication of #321 SQL is required.
- PR #322 creates distinct TOP, CCO, GROUP and RLM parent Razor files,
  retaining separate CIA and Audit Para Snap pages. Index is an authenticated
  redirect only, not a new executive permission page.
- The old permission registration was risky on rerun: it updated existing
  page status/menu visibility and inserted new shared APIs immediately
  active; it had no verified RLM-role grants, and shared URLs were anchored
  to TOP without documentation.
- The new script preserves **existing** STATUS/HIDE_MENU,
  T_AU_API_MASTER.IS_ACTIVE and T_AU_ROLE_API_PERMISSION.IS_ACTIVE values.
  New API records and new API-role grants start **N**, not Y. Existing active
  grants remain active, including currently working development dashboards.
- Caution: the separate `executive_reports_permissions.sql` still
  contains a `WHEN MATCHED THEN UPDATE SET IS_ACTIVE=V_ACTIVE` workflow
  with default `V_ACTIVE='N'`. Do not rerun it as part of this registration:
  it can disable approved Report A-E endpoints. Review it separately.

## Parent page IDs and API ownership

| Parent route | Page permission | Ordinary role grants |
| --- | --- | --- |
| /ExecutiveOverview/TOP | 1000300 | Super User 1; optional verified TOP role |
| /ExecutiveOverview/GROUP | 1000301 | Super User 1; optional verified GROUP role |
| /ExecutiveOverview/CCO | 1000302 | Super User 1; fixed CCO 38 |
| /ExecutiveOverview/CIA | 1000303 | Super User 1; fixed CIA 3 |
| /ExecutiveOverview/AuditParaSnap | 1000304 | Super User 1; fixed CIA 3 |
| /ExecutiveOverview/RLM | existing Dashboard/entity_wise_obs | Super User 1; verified RLM roles only |

The common 6 ExecutiveDashboard GET endpoints, 3 RLM analytical
endpoints and GetKpiLifecycle each have one legacy
`T_AU_API_MASTER` record pointing to page **1000300**. A URL cannot
be multiply registered under several parent IDs without breaking path-based
API authorization. `ExecutiveDashboardController` independently checks
the authorized perspective/page, the API grant, and the narrowed IAS
organization root; this is a registration anchor, **not TOP authorization**.

- CIA-only GetCiaCommandKpis/GetCiaIssueNature belong to 1000303, and
  active grants to roles outside 1/3 block registration.
- Eight independent AuditParaSnap GET endpoints belong to 1000304, with
  identical 1/3 allowlist. Feature flag remains a separate controller gate.
- CCO 38 is included on shared APIs, not CIA/Snap-only APIs.
- The `ExecutiveReports` A-E endpoints remain in their separate script;
  they are not modified here.
- Browser-level filters and date calculations are not changed. Several
  legacy current-state KPIs remain **not date-filtered** even though
  PR #322's parent UI shows Start Date / End Date with an explanatory note.
- CIA Nature of Issue source mappings and #321 Oracle compilation /
  reconciliation remain independent work.

## Optional roles: check and explicitly supply before DBA execution

At the top of `executive_dashboard_permissions.sql`, all optional roles
default to NULL / empty lists:

- V_TOP_ROLE, V_GROUP_ROLE: must match verified IAS ROLE_ID and
  T_MENU_PAGES_GROUPMAP.GROUP_ID for the corresponding role, and
  configured ExecutiveDashboard:RoleMappings.
- V_RLM_REGIONAL_ROLES: IAS roles allowed to see a verified GM/Regional
  scoped view, plus the existing RLM organization page permission.
- V_RLM_MANAGEMENT_ROLES: explicitly approved Group/Division scoped
  manager roles. These must also be allowed by
  RoleDashboards:ManagementRlmRoleIds for the management perspective.

Do not assume GROUP, Divisional Head, Regional Head or GM role IDs from
posting types (entity type 3/5/18/21). Ordinary GROUP scope is its
authenticated IAS UserEntityID. The Super User group/root selectors
remain constrained by C# and existing page IDs. Optional new grants are
inserted inactive pending manual review even if role IDs are supplied.

## Manual development workflow only

1. Check current `T_MENU_PAGES`, `T_MENU_PAGES_GROUPMAP`,
   `T_AU_API_MASTER` and `T_AU_ROLE_API_PERMISSION` metadata.
   Confirm reserved page IDs 1000300–1000304, existing RLM alias source,
   fixed roles 1/3/38, and any optional real IAS role IDs.
2. Review `AIS/Docs/sql/executive_dashboard_permissions.sql`
   before running it with F5/SQL*Plus as the development schema owner.
   The script is transactionally idempotent, locks registration tables
   during MAX+1 allocation, fails on collisions or unauthorized active
   CIA/Snap grants, and does **not** enable inactive endpoints.
3. Run separate read-only SELECTs in
   `AIS/Docs/sql/verify_executive_dashboard_permissions_20261018.sql`.
   Expect no duplicate paths, no mismapped CIA/Snap API pages, no
   sensitive extra active roles, and only one RLM alias source.
4. Activation of a new GET API (N -> Y) and a new role/API grant
   (N -> Y) is a separate, explicitly approved development action.
   The CIA debug-only activation script may be used separately after
   reviewing its effects; it intentionally activates only two CIA calls.
   Audit Para Snap activation requires compilation and Oracle verification.
5. Test with authenticated Super User, CIA, CCO, Group, ordinary
   RLM/GM and unauthorized-role sessions, checking 200 and 403 behavior
   plus selected root/query constraints. No live tests have been run here.

## Verification

The new 5 page registrations and 20 GET registrations were matched
against the actual main-branch C# controller action names. All 20
expected names were present, with no duplicate registrations.
Review static regression tests in
`tests/executive-dashboard/executive-role-permissions.test.cjs`.

The new SQL has **not** been compiled or run against Oracle; a successful
source review is not a substitute for development database verification.
