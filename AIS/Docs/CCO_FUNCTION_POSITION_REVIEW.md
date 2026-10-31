# CCO Function Wise Position: established IAS Report E reader

Updated: 31 October 2026.

## Decision

The CCO Function Wise Position API now reuses the existing authorized IAS Management Audit Settlement Report E reader through `DBConnection.GetExecutiveReportE`, which calls `PKG_EXEC_REPORTS.P_MANAGEMENT_SETTLEMENT`. It **does not** call `PKG_CCO_SCOPE`, `PKG_CCO_FUNCTION_POSITION`, use a scope token, or read `V_CCO_AUTHORIZED_ENTITIES` on this endpoint.

This is **not** an unscoped reporting fallback. The IAS application retains its existing live-session, fixed CCO role 38 or verified Super User 1, CCO page 1000302, and exact `GET /api/CcoCompliance/GetFunctionPosition` API permissions. The database report reader uses the established CCO report perspective and existing Oracle parameter binding. Other CCO organizational APIs that are not implemented remain fail-closed.

## Response contract

The controller continues returning the same eight columns: Group, Division, Opening balance, Para added, Total, Settled, Para outstanding, and Settlement %. The adapter translates Report E's `ITEM_ID` and `ITEM_NAME` into Division ID and name. `ROW_KIND`, balances, movement and percentage are retained from the already maintained Report E source.

The adapter rejects a result with an unexpected report perspective, missing rows, more than the requested maximum, invalid row type or inconsistent total. The existing `CcoReportVisibility.FunctionRows` hides unmapped/conflicting details for CCO users who are not verified Super Users, while preserving the grand total. It also omits Group subtotal rows.

## Deployment

1. Deploy the new **application C# assembly** containing `AIS/DBConnection.CcoFunctionPosition.cs` and the revised client script. A database package-only redeployment will **not** activate this change.
2. Verify that the existing `PKG_EXEC_REPORTS.P_MANAGEMENT_SETTLEMENT` in production is valid and matches the approved, deployed source.
3. Retain the existing CCO page and exact API permission. **Do not** grant Report E's separate API, and do not weaken IAS session or role validation.
4. Sign in with CCO role 38; open Function Wise Position for 2026-01-01 through 2026-10-30, maxRows=1000.
5. Reconcile Group, Division, movement and grand totals with the existing Report E for the same date period. Test a verified Super User, a revoked session, an unrelated role, forged query fields and an expired IAS context.
6. If it still returns HTTP 503, search the corresponding correlation ID in IIS logs. New stages are `execute_authorized_management_report_e`, `validate_management_report_e_rows`, and `revalidate_ias_context`. Debug that stage instead of reinstalling the unused scope package.

## Oracle objects and limitations

The previously created `PKG_CCO_SCOPE`, entity GTTs, trusted `IAS_CCO_CTX`, and `PKG_CCO_FUNCTION_POSITION` may remain installed for historical use or separately approved future features. **Do not drop them** as part of this CCO page fix. Unrelated dependencies are unchanged.

The SQL in `PKG_CCO_FUNCTION_POSITION.sql` is no longer the active Function Wise Position reader; it remains repository reference only. No SQL DDL or DML is required by this application change.

Source review and static tests do not prove that the existing Report E package is installed or that production IAS sessions are valid. Complete CI and authorized UAT before merging.
