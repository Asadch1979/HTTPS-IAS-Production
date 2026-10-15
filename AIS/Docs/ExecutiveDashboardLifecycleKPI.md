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

No production DB changes have been performed by this pull request. The new procedure is not yet connected to MVC/JavaScript.
