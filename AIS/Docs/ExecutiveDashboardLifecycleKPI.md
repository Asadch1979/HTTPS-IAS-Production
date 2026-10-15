# Executive dashboard: lifecycle KPI package (Status 14)

Scope: add a safe, separately testable KPI procedure to `PKG_EXEC_DASHBOARD`. The existing six dashboard procedures, C# access checks, API payloads, and frontend remain unchanged in this PR.

## Deployment order (Oracle 18c)

1. Ensure `V_EXEC_ENTITY_SCOPE` exists; existing `PKG_EXEC_DASHBOARD.sql` already defines this view.
2. Compile `V_EXEC_KPI_SRC_A.sql`, `V_EXEC_KPI_SRC_O.sql`, and `V_EXEC_KPI_SRC_C.sql`. Production may already have these; the scripts version the verified definitions.
3. Compile `V_EXEC_KPI_LIFECYCLE.sql`.
4. Compile the PACKAGE specification and PACKAGE BODY sections of `PKG_EXEC_DASHBOARD.sql` (separately if running SQL Developer F8). Its existing `V_EXEC_PARA_BASE` and `V_EXEC_ENTITY_SCOPE` definitions are unchanged.
5. Execute each check from `PKG_EXEC_DASHBOARD_lifecycle_verify.sql` using F8, including its anonymous cursor-return block. Check `USER_ERRORS` first.
6. Only then consider updating the C# API and frontend to use the new procedure. Do **not** switch the existing GetKpiSummary API before results are verified.

## Scope and numeric definitions

- Period: `TRUNC(P_START_DATE)` to `TRUNC(P_END_DATE)+1`, end-exclusive.
- Issue Identified: eligible modern issues by `ISSUE_IDENTIFIED_ON` (status 4/8/9/23 on currently concluded engagements).
- Issue Rectified: `ISSUE_RECTIFIED_ON` from verified Status 14 conclusion logic. Includes dropped status 23, returned separately in `ISSUE_DROPPED`.
- Para Added: `PARA_ADDED_ON` across A/O/C, omitting source-A dispositions before Status 14.
- Paras Settled: `PARA_SETTLED_ON` across A/O/C; current settled status and date, omitting excluded early settlements.
- Organization: original TOP/CIA/CCO/GROUP/RLM scope validation, selected entity descendants, audited-by, entity-type and reporting-office filters.
- Current Closing, High-Risk, Over 1 Year and Over 3 Cycles are populated only when End Date is **today**; this is an as-of-now snapshot, not a full end-of-day future claim.
- Historical opening/closing/risk/aging/cycles: **NULL**, not zero. These need validated reopens/settlements from the explicit para status-change logs; `AIS_T_AU_POST_COMPLIANCE_HISTORY.COM_STATUS` alone is insufficient.

**Important:** The four independent date-based activity counts do not necessarily add up to the same date's Issue Identified figure. The confirmed equality `14681 = 6789 + 7892` is a fixed cohort of identified observations on currently concluded engagements, not a universal date-event accounting equation.

## TOP At a Glance integration

`GET /api/ExecutiveDashboard/GetKpiLifecycle` calls the deployed procedure with typed Oracle parameters. Under the application's `/ZTBLAIS` path base, the URL is `/ZTBLAIS/api/ExecutiveDashboard/GetKpiLifecycle`. It accepts `startDate` and `endDate` (required ISO dates), `entityId`, `selectBy`, `dimensionId`, `reportingOfficeId`, and the existing `dashboardType` perspective parameter. `selectBy` maps the dimension to Entity Type or Audited By. The authenticated scope must resolve to TOP, including for Super User requests; existing session, page/API grants and organizational scope checks apply. When `view` is supplied, only TOP's `glance` view enables this dataset.

Only TOP At a Glance requests this endpoint. Its initial period defaults to January 1 through today on the application server; selected dates persist through navigation and filter submissions. All nine cards refresh together. Counts remain nullable, and unverified opening/closing snapshot values are suppressed on both server and client. Existing summaries still supply charts and the critical-items count, and do not receive the selected date range. CIA, CCO, GROUP, RLM and the other TOP views retain their existing data sources.

Deployment prerequisite: register GET `/api/ExecutiveDashboard/GetKpiLifecycle` in the existing API permission administration and grant it only to the authorized TOP role(s), including Super User if applicable, against the TOP permission page. An absent registration/grant returns 403; there is no permission bypass or automatic grant. This integration does not modify any database tables, views or packages.


## Historical snapshot blank-value correction (2026-10-15)

`P_GET_KPI_LIFECYCLE` previously always returned NULL for Opening and only
returned closing/risk/aging/cycle when `TRUNC(P_END_DATE)=TRUNC(SYSDATE)`.
For a historical End Date (including yesterday), this led to five blank cards.

The updated procedure returns numeric **date-based estimates** for historical
opening, closing, high-risk, over-one-year, and over-three-cycle counts, using
the same latest-master-date convention as `PKG_EXEC_REPORTS`:
- A para is included only when `V_EXEC_KPI_LIFECYCLE.PARA_ADDED_ON` is before
  the boundary (so source-A pre-conclusion exclusions are respected).
- A currently open para is provisionally considered open after its addition.
- A currently settled para is provisionally considered open until its current
  `PARA_SETTLED_ON`.
- Opening boundary: midnight of Start Date. Closing boundary: midnight
  immediately after End Date (End Date inclusive).
- For historical risk/cycle metrics, today's risk and COM_CYCLE are used.
  Historical cycle changes are not derivable from latest current master alone.
- Future End Dates are rejected; the four activity KPIs are unchanged.
- `HISTORICAL_OPENING_VERIFIED = 'E'` means **ESTIMATED**, not verified.
- `CLOSING_ASOF_VERIFIED = 'E'` for past End Dates, `'Y'` only when End Date
  equals today and current-state flags are used.

These are **not authoritative historical counts**. Reopenings after the
selected date, transfers, and changes of risk or compliance cycle can change
historical figures; full precision requires reconstructing dated status events
and past organizational assignments from verified logs. The API now returns
estimated values with explicit `OpeningEstimated`/`ClosingEstimated`
booleans while preserving the original `...Verified` booleans. TOP shows
`≈` and an estimate qualifier. CIA/CCO/GROUP/RLM keep existing reporting.

Deployment sequence:
1. Compile only the revised **PACKAGE BODY** section of
   `AIS/Docs/sql/PKG_EXEC_DASHBOARD.sql` in Oracle (existing spec unchanged).
2. Run individual queries in
   `AIS/Docs/sql/PKG_EXEC_DASHBOARD_snapshot_verify.sql`.
3. Deploy the updated TOP C# model/DBConnection/JS from the application
   branch only after Oracle compilation; then refresh TOP and verify the
   previously blank cards show numerical estimates, not verified data.
