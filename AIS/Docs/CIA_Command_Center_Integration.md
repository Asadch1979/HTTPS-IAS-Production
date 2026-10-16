# CIA command dataset integration

The CIA At a Glance view now requests three dedicated, CIA-only GET actions:

- `/api/ExecutiveDashboard/GetCiaAuditYears`
- `/api/ExecutiveDashboard/GetCiaExecKpis?auditPeriodId=<numeric ID>`
- `/api/ExecutiveDashboard/GetCiaIssueNature?auditPeriodId=<numeric ID>`

These reuse the existing session, page/API permission and organizational scope
pipeline. Other perspectives are rejected before querying the new datasets.
The new procedure adapters are independent of `GetKpiSummary`, CIA issue events
and TOP lifecycle calculations. No existing Oracle procedure is modified.

## Changed files

| File | Change |
| --- | --- |
| `DBConnection.ExecutiveDashboard.cs` | Independent typed procedure adapters and the existing audit-period master lookup |
| `Controllers/ExecutiveDashboardController.cs` | Three dedicated GET actions using existing permission/scope checks and an additional CIA restriction |
| `Models/ExecutiveDashboardModels.cs` | Typed KPI, issue-category and audit-year responses |
| `Models/ExecutiveDashboardViews.cs` | Enable the new datasets only for CIA glance |
| `Views/ExecutiveOverview/_Sections.cshtml` | Five primary cards and issue-nature panel above retained sections |
| `Views/ExecutiveOverview/_Filters.cshtml` | Numeric audit-year selector and separate timeframe explanations |
| `wwwroot/js/executive-dashboard.js` | Year lookup, independent fetching, cards, chart and mapping diagnostics |
| `wwwroot/css/executive-dashboard.css` | Responsive CIA card/chart styles |
| `Docs/sql/cia_command_center_permissions.sql` | Targeted API registration/grants for DBA deployment |
| `Docs/CIA_Command_Center_Integration.md` | Integration, validation and deployment handoff |

## Years and timeframes

`PKG_AIS.P_GetAuditPeriods` supplies actual `T_AU_PERIOD.AUDITPERIODID` values,
descriptions and dates, including historical periods. Invalid date records are
excluded. The UI defaults to a current period, then the most recent started
period, then the first available period. An explicitly selected ID is retained
and validated against this lookup; an invalid ID is not silently substituted.
The existing Oracle procedures perform their own period validation too.

Audit Year affects planned/audited entities, coverage, A1 para detection and
issue nature. Delayed reports, awaiting response (observation status 2) and
conducting teams remain current-state across all audit years, with the database's
`CURRENT_STATE_AS_OF` date displayed. Null coverage remains N/A when no planned
entities exist. The A1 label describes reported paras and does not establish fraud.

Optional Start/End Dates continue to affect only the existing CIA issue-event
counts. They are not substituted for the numeric audit-period ID and are not sent
to the new endpoints. Existing para KPIs, risk/trend/critical sections and other
dashboard perspectives retain their contracts.

## Mapping and permissions

The ten bars display the procedure's category names and order. The UI checks
that totals agree and that `SUM(ISSUE_COUNT) + UNMAPPED_ISSUES = TOTAL_ISSUES`.
PARTIAL results display **Incomplete classification**, the unmapped count and
an explicit qualification that bars show mapped issues only. Empty or malformed
responses show an unavailable message; zero categories are not synthesized.
New dataset failures do not suppress the other new dataset or existing sections.

Deploy `sql/cia_command_center_permissions.sql` through the established IAS DBA
permission process. It registers only these three GET APIs against the existing
CIA page and grants only already page-authorized CIA role 3 / Super User role 1.
It fails on conflicting registrations and does not grant CIA page access.
Refresh permission caches / sign in again after deployment as required by IAS.
Missing registration or grants continue to return 403.

Remaining environment dependencies:

- Execute the API permission registration script in the target IAS schema.
- Confirm the application schema can execute the existing audit-period lookup
  `PKG_AIS.P_GetAuditPeriods`, in addition to `PKG_EXEC_DASHBOARD`.
- Populate and validate approved mappings in `T_IAS_CIA_ISSUE_NATURE_MAP`.
  Unmapped observations remain visible in the classification warning.
- Reconcile the existing procedure's business definitions and source counts
  (A1, status-14 coverage, report delay threshold and conducting-team stages)
  as described in `CIA_Executive_KPIs_Deployment.md`.
- Live Oracle/API authorization and browser checks require the target environment;
  no production permission changes or data reconciliation were executed locally.

The prior visual redesign branch is not merged by this integration. This change
adds the requested datasets to the current CIA At a Glance view using its existing
components. Review before merging into main.

## Focused verification

- `dotnet build AIS/AIS.csproj --no-restore --verbosity quiet --output AIS/bin/cia-integration-check`:
  passed with 0 errors. Separate output avoids a lock held by the running AIS
  executable; the initial build reached compilation but could not replace it.
- `node --check AIS/wwwroot/js/executive-dashboard.js`: passed.
- In-memory frontend smoke checks passed for five cards, null coverage, ten bars,
  complete/partial/no-issue/empty responses, inconsistent totals, numeric audit
  year and organizational filter transport, and independent KPI API failure.
- Source diff confirms no edits to the canonical package or existing procedure
  adapters, and only CIA glance enables the three new datasets.
