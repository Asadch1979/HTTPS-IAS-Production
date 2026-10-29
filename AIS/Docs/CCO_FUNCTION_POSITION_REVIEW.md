# Function Wise Position — source-only implementation

Baseline c7e6e7c8311e6f1208bd8c7aab164671fe1b72da verified as GitHub main through connector. New branch codex/cco-function-wise-position preserves prior work.

Population is explicitly Management Audit D, matching Report E. This is not the wider B/D CCO portfolio. No Report E procedure, endpoint permission, or CCO B/C/D allowlist changed. Wider population requires separate approval. Historical amounts remain estimates. Outstanding is calculated independently, never forced to Total minus Settled.

org-position now renders eight business columns only; no hierarchy controls or organizational lookups. Other tabs retain their existing partials. Valid zeros remain zero, missing rates show N/A, unresolved mapping labels remain visible, and empty data differs from unavailable data.

New GetFunctionPosition reuses existing CCO authentication/session/context/page/exact API checks and audit logging, accepting only dates and bounded maxRows. Its production adapter remains hard-disabled. The private ODP.NET candidate uses one connection/transaction, trusted begin/assert/end, bound parameters, source-count overflow checks and pool clearing on failed cleanup. It is not wired for activation. No API grant or configuration switch is added.

Review-only PKG_CCO_FUNCTION_POSITION uses the supplied P_NAME/C_NAME relationship, active mapping convention, approved exclusions 112215/112221 and comparison to V_EXEC_ENTITY_SCOPE. Mapping tuples are deduplicated and reduced before facts; conflicting paths remain unresolved. Duplicate COM_ID facts reject the request. Group subtotals and grand totals use GROUPING SETS over the same fact population. Missing base/view DDL remains recorded in CCO_DATABASE_SOURCE_MATRIX.md; SQL has not been compiled or executed.

Validation: application build passed (57 warnings, 0 errors); existing six source checks and two new function contract checks passed; Chrome function fixture passed. Compiled authorization checks include the new API, retaining 503 for authorized contexts and 403 for excluded roles. Mocked/static checks do not prove Oracle execution, arithmetic reconciliation, installed permissions or pool behavior. Independent review must validate those before activation.
