# Executive Reports A–E — existing IAS sources (revised after 77eeeb0)

## Source and scope

The reporting package now reads these existing objects only:

- `V_EXEC_ENTITY_SCOPE`: hierarchy and reporting levels (1–6), Group, Division, GM and Reporting Office
- `AIS_T_AU_POST_COMPLIANCE`: one case per `COM_ID`, creation date `PARA_ADDED_ON`, current `PARA_STATUS`, `SETTELED_ON`, risk, entity type and `AUDITED_BY`
- `AIS_T_AU_POST_COMPLIANCE_HISTORY`: dated movement and settlement-related `COM_STATUS=16` / `COM_STAGE` evidence, deduplicated per `COM_ID`
- Existing IAS entity directory/type mapping for names, audit-type and regional grouping

`PKG_EXEC_REPORTS.sql` creates `V_EXEC_REPORT_SOURCE`, package spec and package body.
The five existing procedure signatures and C# report models remain unchanged.
The earlier `T_EXEC_REPORT_CASE_HISTORY`, `T_EXEC_REPORT_HISTORY_EVENT`,
`EXEC_REPORT_FACT`, `EXEC_REPORT_FACTS`, `V_EXEC_REPORT_PORTFOLIO`
and pipelined `F_FACTS` are **no longer prerequisites**. Do not run the old
history-contract or portfolio-view scripts for this replacement.

## Important calculation limits

Historical balances use the date recorded in `PARA_ADDED_ON` and the **current master
settlement date only when `PARA_STATUS=9`**. A case with an effective recorded close
before the selected date is excluded from the estimated outstanding position; a case
still open is treated as open. Consequently these are **date-based estimates**, not a
certified reconstruction of earlier settle/reopen cycles or prior entity transfers.
The current mapping, current risk and current recorded auditor apply to all periods.
Reopening and resettlement metrics are reported as zero (not proof no events occurred).
`RECONCILIATION_DIFFERENCE` is exposed and should be investigated if nonzero.

The latest relevant `COM_STATUS=16` history record can suggest the original
settlement workflow: stages 41/45 are classified as Compliance; other non-null
stages as Audit only when the history entry and the master settlement date match
on the calendar day. Without a matching record the classification is UNKNOWN.
This is a **working classification using existing `PKG_RPT.R_FAD_MONTHLY_REVIEW`
stage logic**, not a certified source-of-settlement event. Unknown is never counted
as audit/compliance or silently dropped.

Family mapping also requires report-owner confirmation: Reports A–D follow FAD's
existing `AUDIT_TYPE IN ('B','D')` source rule; Report E currently uses
`AUDIT_TYPE='D'` as a proxy for desk/HO management functions and groups by
`V_EXEC_ENTITY_SCOPE.DIVISION_ID` with business-group fallback. Because D is also
included in FAD's legacy definition, the populations are **not guaranteed to be
mutually exclusive**. Do not certify Field-versus-Management totals without
confirming the correct audit-office/observation classification on the live database.

`V_EXEC_REPORT_SOURCE` avoids multiplying cases by joining a *single* aggregated
history row per `COM_ID`; the reporting group hierarchy uses the verified current
`V_EXEC_ENTITY_SCOPE`. Regions are identified by the nearest mapped entity of
category 5; validate the regional category code in your deployment.

## Deploy and verify (DB owner)

1. In the same Oracle schema used by IAS, verify source objects, dates,
   `T_AUDITEE_ENT_TYPES.AUDIT_TYPE`, regional category and history rows.
2. Run **only** `AIS/Docs/sql/PKG_EXEC_REPORTS.sql` (F5 / Run Script).
   This creates `V_EXEC_REPORT_SOURCE`, `PKG_EXEC_REPORTS` and package body.
3. Inspect `USER_ERRORS` for `PKG_EXEC_REPORTS` and `V_EXEC_REPORT_SOURCE`.
4. Run `AIS/Docs/sql/verify_exec_reports.sql` for an actual period, first TOP
   then GROUP with a genuine Group Head root, and compare Report B/C totals
   and Report E to the existing IAS reports.
5. Run `executive_reports_permissions.sql` with default inactive registrations.
   Verify actual TOP/GROUP role IDs before activating; CCO/CIA permissions are
   based on existing verified role IDs. Reload IAS permissions / sign in again.
6. To activate, set `ExecutiveReports:Enabled = true` in deployment configuration
   only **after** compile, permission and figures verification. This defaults false.
   `ExecutiveReports:HistoryVerified` is retired and no longer read.
7. Keep the reports disabled if missing/ambiguous dates, report-family attribution,
   settlement classification or as-of estimate limitations would mislead management.

The API response explicitly indicates `historicalBalancesEstimated=true`,
`currentOrganizationMapping=true` and `settlementAttributionEstimated=true`.
Task 4 must display these reporting-methodology cautions in the Reports UI, including
an 'Unknown' settlement-source count when applicable.

**No live Oracle compilation or data comparison was possible in this repository edit.**
The application code uses Oracle 18c bind parameters and maintains the authenticated
GROUP scope; the package is designed for use only through authorized IAS endpoints.

## How to enable the Executive Reports tab (IIS / ASP.NET Core)

The Reports page and `ExecutiveReportsController` independently read
`IConfiguration.GetValue<bool>("ExecutiveReports:Enabled")`. If absent, the
value is **false**; the Reports tab displays "Executive reports are disabled"
and the API returns HTTP **503**. No front-end change can override this flag.

After the database owner verifies `V_EXEC_REPORT_SOURCE`, `PKG_EXEC_REPORTS`,
all five report cursors, and the Field/Management source attribution, enable
**one** of the following in the *deployed IIS application*:

- Recommended deployment setting: environment variable
  `ExecutiveReports__Enabled=true` (double underscore is the .NET configuration
  section separator); use the environment settings for the IAS application or an
  authorized IIS `web.config` environmentVariables entry.
- Alternative: in the production application's deployed
  `appsettings.Production.json` (when Production is the active ASP.NET Core
  environment), add `"ExecutiveReports": { "Enabled": true }`.

Avoid overwriting production settings with a development `appsettings.json`.
Restart/recycle the IAS application pool after changing deployment settings,
then refresh the Reports page. Do not enable via source-controlled defaults
unless the production approval process explicitly calls for it.

**Second gate — IAS API permissions:**

1. Check registration of GET `/api/ExecutiveReports/GetReportA` through
   `GetReportE` in `T_AU_API_MASTER` and active role grants in
   `T_AU_ROLE_API_PERMISSION`.
2. The repository's `AIS/Docs/sql/executive_reports_permissions.sql`
   defaults `V_ACTIVE='N'` and TOP/GROUP role IDs to `NULL`; simply running it
   **will not enable** the endpoints. Review deployment-specific role IDs and
   desired perspective-by-report grants before using or adapting it, and
   do not guess role IDs or grant unauthorized scopes.
3. Refresh IAS permissions and sign in again. The API also checks page
   authorization, report authorization and the selected organizational scope.

**Troubleshooting:** HTTP **503** (`reports_not_deployed`) typically means
`ExecutiveReports:Enabled` is false/missing; **403** means a missing/denied
page or API permission or scope; **401** means no active IAS session; **500**
requires inspection of server/Oracle logs. A compiled package does not by itself
activate the application feature flag or grant API permissions.
