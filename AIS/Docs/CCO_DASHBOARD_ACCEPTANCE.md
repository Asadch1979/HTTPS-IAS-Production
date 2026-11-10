# CCO dashboard staging checklist

The performance report uses database-derived Group, Division, GM Office and overall totals. GM Office rows identify Field Audit (B) and Management Audit (D). A Group Head selection filters verified reporting classifications. Unmapped or conflicting assignments remain visible in the all-Group totals. A missing settlement denominator displays N/A. Historical balances use IAS effective addition and close dates.

## Deployment order

1. Verify the installed definitions and privileges for V_EXEC_REPORT_SOURCE and the corrected V_EXEC_ENTITY_SCOPE. Run the read-only checks in sql/cco_reporting_preflight.sql and inspect duplicate COM_ID values and mapping cardinality. Compare against the currently deployed package spec/body before applying any changes.
2. Apply only the PKG_EXEC_DASHBOARD package spec and body changes for P_GET_SNAP_CRITICAL from sql/PKG_EXEC_DASHBOARD.sql. Preserve the installed view definitions. Compile and check USER_ERRORS. Verify existing callers that omit the new optional arguments and run all five CCO risk cases before deploying the client.
3. Compile sql/PKG_EXEC_CCO_PERFORMANCE.sql under the executive reporting owner. Verify its cursor with the authorized staging DBA. If the IAS runtime is a different schema, review EXECUTE on this package for the existing IAS application principal only. The application requires no new direct source-table or view grants.
4. Verify the exact IAS registrations below. Register the new Performance GET and its role mappings inactive first. Activate for the staging test principals after approval and refresh permission caches through a new session.
5. Deploy the application with CcoReporting:PerformanceEnabled unset or false. Enable the flag in isolated staging for acceptance only after verifying the Oracle reader. Keep the production flag off until the authenticated staging acceptance checks pass.

| Exact method and path | Required page and role |
|---|---|
| GET /api/ExecutiveDashboard/GetCriticalItems | Retain its existing shared registration and role mappings. CCO risk paging additionally requires page 1000302 and role 38 or verified Super User 1. |
| GET /api/CcoCompliance/GetFunctionPosition | Retain existing exact grant for page 1000302 and role 38 or verified Super User 1. |
| GET /api/CcoCompliance/GetOrgPerformance | Register this exact new read against page 1000302, role 38 and independently verified Super User role 1. |

The procedure uses effective addition and close dates from the same report source as Reports C/E. Reconcile B with Report C and D with Report E for identical dates before checking the combined grand total. Compare Overview separately with identical eligible COM_IDs and document any source-clock or population differences. Do not compare unaligned grand totals. Review opening, additions, total, settlements, closing, high-risk closing, rate and residual. Check the sum of GM details against each Division and Group subtotal, with B/D identified and missing mappings included. Record discrepancies before activation.

In staging, test a multi-Group result, a single verified Group, an invalid Group, multiple Divisions/GM Offices, ambiguous/missing assignments, zero denominator and an empty period. Exercise all five observation risks, a later page and an empty page, and compare the High-Risk KPI separately. Revoke the session and repeat the API requests. Repeat with missing exact grants, no CCO page grant, an unauthorized role, unverified role 1, and invalid or duplicate filters. Review the CCO browser and CSV/PDF exports and use both desktop and mobile widths. No Oracle deployment or reconciliation was performed as part of local validation.
