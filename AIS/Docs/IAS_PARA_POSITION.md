# IAS Para Position

Production entry point: `pkg_rpt.R_IAS_PARA_POSITION`. DBConnection retains named bindings:
`P_NO NUMBER`, `ENT_ID NUMBER`, `R_ID NUMBER`, `R_T NUMBER`, `S_DATE DATE`, `E_DATE DATE`, `IO_CURSOR OUT REF CURSOR`.

The packaged IAS procedure is not present in the checked-in PKG_RPT snapshot. The old standalone `sql/R_IAS_PARA_POSITION.sql` is a reference, not a replacement for the working production package. Do not compile it as the deployment step. Verify ALL_ARGUMENTS (including owner/overload), live package source and returned cursor aliases with `sql/IAS_corrective_review_F8.sql` before deployment. The configured Oracle connection was unavailable during this corrective review; signature compatibility is not certified.

The existing page/API permissions, AJAX contract and numeric calculations are unchanged. For Entity Type -1 the page sums the same returned rows into a compact summary; individual types retain the detailed DataTable. Total remains Opening Balance + Para Added. Empty/error/loading states hide both result layouts. Switching filters destroys any old DataTable and cancels stale AJAX results.

Issue Rectified excludes dropped status 23. Its persisted flag records settlement during the active audit, is copied at successful conclusion and cleared on applicable observation reversal. ISSUE lifecycle dates use the latest current-cycle conclusion; modern PARA balances retain their existing first-conclusion recognition logic. No other executive KPI formulas were changed. Historic reopened/re-concluded or missing-log cases require manual reconciliation rather than an unconditional backfill from MAX(created_on).

Manual database work is described in `IAS_Corrective_Review_2026-10-17.md`. Do not run historical DML, replace packages/views or change grants automatically. Existing menu/API grants need no new registration for the summary UI. For an initial report installation, separately review the existing permission script and current production grants.

Live validation must reconcile the package result aliases P_NAME, CHILD_CODE, C_NAME, OPENING_BAL, ISSUE_IDENTIFIED, ISSUE_RECTIFIED, PARA_ADDED, SETTLED_COM, SETTLED_AUD, OUTSTANDING and R1/R2/R3 with the existing model and the selected entity/date scopes. Validate All Entities, individual types, zero rows, request failure, switching during a request, dropped/reversed observations, first conclusion and re-conclusion. Historical opening/closing balances retain their existing estimation limitations.
