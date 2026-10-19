# Executive Dashboard / Reports: Super User-only production evaluation

**Prepared:** 19 October 2026. **Status:** Source-only pending DBA review; neither
script has been executed against production by ChatGPT. These SQL files are
**write scripts**, whereas `verify_executive_superuser_permissions.sql` is
read-only.

## Reviewed source and rationale

The IAS code already has separate role-specific Razor parents (TOP, GROUP,
CCO, CIA, Audit Para Snap and RLM). The old dashboard script granted API
access to CIA/CCO and optional management roles. The old reports script
assigned Reports A-E to CIA/CCO/management and used
`WHEN MATCHED THEN UPDATE IS_ACTIVE=V_ACTIVE` with default N,
potentially deactivating already approved reporting APIs on repeat.

For the restricted pilot:

- The 5 *dedicated* `T_MENU_PAGES` IDs 1000300 through 1000304 are
  exclusively mapped to `T_MENU_PAGES_GROUPMAP.GROUP_ID=1`.
  All other **group-map rows for these five IDs** are removed.
  Existing page `STATUS`, `HIDE_MENU` and metadata are not rewritten.
- Exactly 12 `ExecutiveDashboard` GET and 8 `AuditParaSnap` GET
  registrations are registered once each. Matching
  `T_AU_ROLE_API_PERMISSION` grants for IAS role 1 are set Y; all
  other roles' grants to those URLs are set N.
- The 5 separate `ExecutiveReports/GetReportA-E` GET registrations
  receive role 1 only; all other role grants to those paths are set N.
  This does **not** affect Audit Para Snap's distinct /api/AuditParaSnap/
  GetReportA-E URLs or any existing IAS report endpoints.
- Existing API master `IS_ACTIVE` is preserved. Newly created masters
  are N by default. If the report history is independently approved,
  the operator can explicitly set
  `V_ACTIVATE_VERIFIED_REPORTS='Y'` in the report script.
  That switch changes the five report masters to Y **only**;
  it does not broaden access to other roles.
- Neither script changes application role mappings, IAS
  `PKG_EXEC_DASHBOARD` or other Oracle packages, and neither creates
  duplicate page IDs for shared GET endpoints.
- The scripts commit as one transaction each after fail-closed checks.
  Oracle DML can be rolled back before commit; **after commit, use a
  database snapshot or backed-up permission tables for restoration**.

## Important exception: RLM page shell

`/ExecutiveOverview/RLM` uses a `PageIdResolver` alias to the shared
`/Dashboard/entity_wise_obs` page. Non-Super Users may need that page
for existing IAS functions. **Neither permission script removes any
mapping for that shared page.** Consequently, a previously authorized
ordinary role could still load RLM's parent shell, even though the 12
ExecutiveDashboard data endpoints are denied by Super User-only API grants.

If the requirement is to block **even the dashboard shell**, add a
focused controller-level evaluation gate for all executive parents,
including RLM, under a dedicated configuration flag. This cannot be
reliably solved by editing these two SQL files alone without impacting
unrelated Organization Structure access. Do not treat removal of API
grants alone as complete route authorization.

The application's permission cache may contain stale allowed page/API
lists after SQL updates. For validation, force fresh sessions and cache
reload for ordinary and Super User accounts; ensure no old session can
keep using a cached grant.

## Before production execution (manual)

1. Export/back up the current rows in:
   `T_MENU_PAGES`, `T_MENU_PAGES_GROUPMAP` limited to 1000300–1000304,
   `T_AU_API_MASTER` and `T_AU_ROLE_API_PERMISSION` limited to the 25
   executive GET paths. Confirm they match the actual production
   fixed Super User role/group ID 1.
2. Run the nine read-only queries in
   `AIS/Docs/sql/verify_executive_superuser_permissions.sql`
   and save output for comparison. Confirm collisions and registration
   mismatches are absent; review inherited Report A-E activation states.
3. Run `executive_dashboard_permissions.sql` in **F5 / SQL*Plus
   script mode**, as the owning schema. It revokes other group and API
   grants for the new dashboard pages and their 20 APIs, creating missing
   records inactive. It does not touch the RLM org page.
4. Run `executive_reports_permissions.sql` in the same mode. Keep
   `V_ACTIVATE_VERIFIED_REPORTS='N'` until Report A-E data history and
   Oracle dependencies have been independently approved.
5. Rerun read-only verification. Expectations: exactly five dedicated
   page IDs granted to group 1, zero other group grants on those pages,
   20+5 distinct executive GET actions, zero **active** role grants
   outside role 1, role 1 active on all registered actions, no
   unexpected API master changes.
6. Invalidate cached permissions/re-authenticate; verify Super User
   loads the dashboards and *authorized, currently active* API data.
   Test a separate ordinary CIA, CCO, Group and RLM account:
   ordinary users must not retrieve any newly restricted executive
   API data. Review RLM-shell caveat above before declaring pilot
   completely Super User-only.
7. IAS feature flags remain independent of database grants, including
   `RoleDashboards:CiaCommandCenterEnabled`,
   `RoleDashboards:AuditParaSnapEnabled` and
   `RoleDashboards:OracleReadEnabled`.
   Unverified Oracle views/packages can still produce N/A or 503 even
   for Super User. **Do not activate unverified data merely to display it.**

## Source QA

All 25 GET actions and dedicated page IDs have been checked against the
committed C# controllers. A lightweight static source test is at
`tests/executive-dashboard/executive-superuser-permissions.test.cjs`.
No direct production Oracle connections, SQL execution, live authorization
testing or deployment occurred during this change.
