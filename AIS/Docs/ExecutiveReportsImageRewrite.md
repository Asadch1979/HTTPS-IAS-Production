# Executive Reports A–E — reference-image-aligned SQL rewrite

**Status: REVIEW / NOT DEPLOYED.** Oracle compilation and live data checks are required before merging into main or exposing figures to management.

Reference images: `AIS/wwwroot/Images/reference_images/` (main branch).
The repo contains six images defining **five report datasets**; images 01 and 06 are two visual presentations of Annual Field Audit progress.

| Image | SQL procedure | Required output layout |
|---|---|---|
| 01, 06 — Annual Field Audit Progress / Annual Summary | `P_ANNUAL_FIELD_PROGRESS` | One row per calendar year: total settled, settlement %, additions, outstanding |
| 02 — Field Office Paras Position | `P_FIELD_OFFICE_POSITION` | Office rows under **GM Field Offices / Other Field Offices**, subtotals, grand total; opening, added, total, settled, %, outstanding, outstanding by risk, settled by compliance/audit |
| 03 — GM/Region Paras Position | `P_GM_REGION_POSITION` | Region rows under GM group, GM subtotals, grand total, same numeric columns as image 02 |
| 04 — Top/Bottom Regions | `P_REGION_RANKINGS` | Five highest and five lowest regions by settled %, with total paras, outstanding and ranking group |
| 05 — Management Audit Division Settlement | `P_MANAGEMENT_SETTLEMENT` | Relevant function/Division rows and total, opening, added, total, settled, outstanding |

## SQL files and execution order (Oracle 18c)

1. Ensure `V_EXEC_ENTITY_SCOPE` and approved `V_EXEC_KPI_LIFECYCLE` (including `V_EXEC_KPI_SRC_A/O/C`) exist and are valid.
2. Compile **`V_EXEC_REPORT_SOURCE_image_rewrite.sql`** with F8 as one CREATE VIEW statement.
3. Compile **`PKG_EXEC_REPORTS_image_rewrite_spec.sql`**, then **`PKG_EXEC_REPORTS_image_rewrite_body.sql`** using F8. The canonical `PKG_EXEC_REPORTS.sql` combines spec and body with SQL*Plus slash separators for F5.
4. Execute each block from `verify_exec_reports_image_rewrite.sql` independently using F8.
5. Compare report totals and risk categories with the original IAS FAD monthly-review reports before enabling `ExecutiveReports:Enabled`. **No API/UI change is included.**

## Data semantics

- One reported para per `COM_ID` from `V_EXEC_KPI_LIFECYCLE`; avoid double-counting issue rows.
- Source A observations resolved/dropped **before Status 14 conclusion** are excluded.
- For source A, `EFFECTIVE_ADDED_ON` is the later of copied `PARA_ADDED_ON` and Status 14 `REPORT_CONCLUDED_ON` (if available). This prevents showing a pre-conclusion observation as an already-open post-compliance para. Legacy O/C use their existing added date.
- Opening = estimated open at **00:00 on Start Date**. End outstanding = estimated open at **00:00 the day after End Date**. Current master settlement date / status is used; historical reopening or transfers cannot be reconstructed perfectly.
- Additions / settlements measured in period by their respective dates; `RECONCILIATION_DIFFERENCE` indicates movement imbalances, not automatic adjustments.
- Settlement percentage = 100 × settled / (opening + added), NULL if denominator zero.
- Risk breakdown = current `RISK_ID` on estimated end-open paras. Existing `PKG_RPT.R_FAD_MONTHLY_REVIEW` has special annex-based risk logic for audit type B / entity types other than 28. **This has not been reproduced**, because the exact live `ANNEX_RISK` mapping is not established; expect possible risk-column differences until validated.
- Settlement attribution uses same-calendar-day history `COM_STATUS=16` and stages 41/45 as a **working** Compliance classification; other known stages Audit; missing/mismatched history Unknown. `SETTLED_UNKNOWN` remains in the cursor even if not depicted in the screenshot.
- Grouping: Report B B/D audit-type sections, Report C nearest category-5 region under current GM, Report E current Division with Business Group/Office fallback. Non-mapped locations remain as `Unassigned` rather than silently disappear.
- Reports A-D retain existing FAD `AUDIT_TYPE IN ('B','D')`; Report E retains `AUDIT_TYPE='D'`. **These populations overlap** and cannot yet be certified as distinct Field vs Management Audit families. This classification must be explicitly confirmed using `exec_reports_phase1_diagnostics.sql` and live organizational ownership before production deployment.
- The screenshot values (2023–2026 / September 2026) are **reference examples only**; never hardcode them or claim they match present-day IAS.
- Report E is currently offered in RLM UI, but `PKG_EXEC_REPORTS` authorization accepts TOP, GROUP, CCO, CIA only; fix this UI/API scope mismatch separately. Do not relax database access checks blindly.
- No change to report stored-procedure signatures or `ExecutiveReportRow` field names. Charts/formatting are later frontend work. Review `AIS/Views/ExecutiveOverview/_Reports.cshtml` before connecting APIs.

## Required evidence for owner sign-off

1. `USER_ERRORS` shows no errors in `V_EXEC_REPORT_SOURCE` or `PKG_EXEC_REPORTS`.
2. All five cursor smoke tests execute and return expected row hierarchies.
3. A/B/C sample period opening, added, settled and closing figures reconcile. Report D top/bottom percentages agree with regional source data and exclude unrankable regions from rank 1–5. Report E division source attribution is documented.
4. Audit type B/D overlap resolved, and per-para high/medium/low risk policy confirmed against annex logic before marking risk reports authoritative.

Work is kept on a dedicated branch; no production object has been changed in this repository-only task.
