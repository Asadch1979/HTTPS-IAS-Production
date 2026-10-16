# IAS Para Position

Open `/Reports/IAS_PARA_POSITION` after deployment. This is a separate copy of
the FAD Monthly Review report; the original report and package are unchanged.

## Deployment

1. Compile the approved `V_EXEC_KPI_SRC_A.sql`, `V_EXEC_KPI_SRC_O.sql`,
   `V_EXEC_KPI_SRC_C.sql`, then `V_EXEC_KPI_LIFECYCLE.sql`. Their existing
   `V_EXEC_ENTITY_SCOPE` dependency must already be installed.
2. Compile `sql/R_IAS_PARA_POSITION.sql` in the application's Oracle schema.
3. Run `sql/ias_para_position_permissions.sql`. It registers page ID 1000400
   and copies the original report's menu groups and active POST API role grants.
   It rejects conflicting page registrations. Run it without concurrent menu edits.
4. Deploy the app and sign in again to refresh cached menu/API permissions.

## Counting rules

- Issue Identified: one eligible ISSUE row whose identification date falls in
  the selected period (`issue_eligible = 1` from the lifecycle view).
- Issue Rectified: ISSUE rows in status 4 or 9 whose lifecycle rectification
  date falls in the period. The lifecycle view requires settlement before
  status-14 audit conclusion. Dropped issues (status 23) are excluded, matching
  `P_GET_KPI_LIFECYCLE`.
- Para Added: PARA rows whose lifecycle `para_added_on` falls in the period,
  including modern A and legacy O/C sources as defined by the supplied views. Modern
  pre-conclusion dispositions have no effective addition date and do not count.
- Total remains Opening Balance + Para Added. Issues are separate activity
  measures and are not added to para balances.
- Opening and outstanding use the lifecycle addition/settlement dates and
  current para status (8 open, 9 settled). End dates include the entire day.
  Historical balances are estimates because current status/latest settlement
  cannot reconstruct unlogged reopenings.
- Settlement follows the lifecycle settled date. Compliance stages 41 and 45
  count under compliance; other stages, including missing stages, count under audit.
  Risk buckets use lifecycle risk 1/2/0-or-3 for outstanding paras.

Auditor/reviewer restrictions follow the original monthly-review procedure.
Roles 1/3/5 selecting -1 receive entity-type summary rows; roles 6/7 see their
audited entities; roles 43/44 see their assigned compliance entities; roles
15/16 are restricted to their audit office. Other selections follow active
entity category mappings. Reporting-office visibility matches the copied page.
Entities with issues alone are included so they are not lost from the totals.

## Database verification

Verify compilation in `USER_ERRORS` for `R_IAS_PARA_POSITION`, then run the
report for an authorized auditor and compliance reviewer. Reconcile eligible
issues by observation ID and additions by COM_ID against the lifecycle view,
including a status-23 dropped issue, pre-conclusion dispositions, legacy O/C
paras, end-day events, and an entity with issues but no compliance paras.
Live Oracle execution is required to validate environment-specific schema
dependencies and grants.
