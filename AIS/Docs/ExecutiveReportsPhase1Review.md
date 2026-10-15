# Executive Reports A–E: phase 1 review (2026-10-15)

**Scope:** review and reconcile Report A/B/C/D/E data and classifications, then implement the Reports frontend; keep the operational CIA dashboard and nine TOP lifecycle KPI project isolated.

## Confirmed from current repository code

1. `PKG_EXEC_REPORTS.sql` defines source view, package spec, and five procedures. `ExecutiveReportsController` and `DBConnection.ExecutiveDashboardReports.cs` already implement guarded endpoints with typed Oracle parameters. The Reports Razor partial `_Reports.cshtml` **only displays placeholders**.
2. Report A–D population currently uses `T_AUDITEE_ENT_TYPES.AUDIT_TYPE IN ('B','D')`; Report E uses `AUDIT_TYPE='D'`. The populations overlap; verify intended FAD vs MAD split on live data before certifying or exposing figures.
3. Report source uses raw post-compliance `PARA_ADDED_ON`, current `PARA_STATUS` and `SETTELED_ON` estimates. It does **not** consume the approved Status-14 lifecycle exclusions from `V_EXEC_KPI_SRC_A`. Reconcile before activation.
4. `ExecutiveDashboardViews.ForPerspective("RLM")` exposes Report E but `PKG_EXEC_REPORTS.VALIDATE_REQUEST` permits TOP/GROUP/CCO/CIA, **not RLM**. Decide whether RLM should be supported and add its authorized scope validation, or hide Report E in RLM.
5. Backend gating `ExecutiveReports:Enabled` defaults to off according to repository documentation. API grants must be configured before switching on. Page must explicitly disclose estimated history and unknown settlement attribution.

## First action

Run `AIS/Docs/sql/exec_reports_phase1_diagnostics.sql` (each SELECT independently, F8), share the six result sets, and confirm the correct **Field Audit vs Management Audit** source attribution. Only then update `V_EXEC_REPORT_SOURCE`, `PKG_EXEC_REPORTS`, and UI in small focused commits.

**Current status:** analysis SQL only; no business logic, DB package, API authorization, or UI changed in this branch.
