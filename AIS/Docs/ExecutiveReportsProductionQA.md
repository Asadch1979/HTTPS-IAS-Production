# Executive Reports A–E — production visual and PDF QA (2026-10-16)

## Reference materials

This update responds to exported Report A–E PDFs captured on 16-Oct-2026 and to the Excel-style GM/Region sample. The captures had browser-inserted localhost URLs, blank trailing pages, image-only text, overlong data cautions, and oversized reconciliation tables.

## What changed

- Server-generated PDF is available on the **existing, already-authorized GET** `/api/ExecutiveReports/GetReportA` through `GetReportE`: use the identical report filters and add `format=pdf`. No new controller route/API permission grant is required.
- Uses existing iText7 + pdfHTML dependencies to render selectable/searchable PDF text. Explicit A4 landscape page size, IAS/ZTBL page header (existing deployed `wwwroot/Images/ztbllogo.png` if found), generated date, and **Page X of Y** stamped on all pages.
- No browser print headers, footers or URLs. Page breaks are controlled by report table layout, repeating table headings and avoiding row breaks.
- PDF button now downloads a PDF from the server. CSV export remains unchanged.
- Web Reports A–E use unified dark-green/neutral styling, semantic good/poor color for regional rankings, compact neutral source-estimate note, and the collapsed **IAS Settlement Attribution and Reconciliation** table showing Compliance/Audit/Unknown only.
- Report A: grouped annual columns for Settled and Added with Outstanding as a separate-axis line; summary alongside on wide screens.
- Report B: label GM Field Offices **Field Audit Division**, Other Field Offices **Management Audit Division** for presentation only (underlying Oracle classifications unchanged).
- Report C: GM-summary table derived from returned SUBTOTAL rows; region detail retains GM group headers without repeating GM column and puts the subtotal in Region Name.
- Report D: green TOP / red BOTTOM; captions show outstanding and total. Reference-line benchmark is fetched from the authorized Report C `TOTAL` for identical filters. Do not hardcode screenshot 54.25%.
- Report E: settlement percentage appended.
- Hide all-zero Unrated risk column in B/C. Settlement percentage uses neutral in-cell scale, not invented performance targets.

## Acceptance checks (manual, authenticated IAS debugging session)

1. Compile application and open each report on TOP (and CCO/CIA/GROUP according to existing report authorization).
2. Check reports return HTTP 200 and selected dates/scope constrain the API; check PDF request requires the same session and API permission.
3. Report C subtotal matches the corresponding GM-summary row; no repeated General Manager column or N/A text in group subtotals.
4. Confirm Report E Settlement % equals `100 * Settled / Total` under the existing Oracle calculation.
5. Verify Report D average benchmark matches Report C grand-total `settlementPercentage` for the same scope and dates.
6. PDF: check desktop A4 landscape page size, no browser localhost URL, repeated IAS header, generated date, sequential **Page X of Y**, no blank trailing page, and no orphan group header.
7. PDF: use Ctrl+F for an office name and select/copy text to confirm text is searchable.
8. CSV: check the renamed group headings, subtotals and all numeric values, including the new Report E percentage.
9. Screen: check the tables at 1920px/1366px and a narrow viewport; make sure CSS is scoped to Reports and CIA/TOP At a Glance is unchanged.

## Limitations and deployment

- Rendering has not been compiled/tested in live IAS here. The new PDF route uses the same API action and permission; PDF creation exceptions are covered by the existing controller error handler.
- Server PDF favors clean, searchable tables; interactive web charts are not embedded in PDF as screenshots. Long GM tables may span several pages despite page-break-avoid rules; do not promise impossible one-page group fits.
- The short muted note preserves the factual qualification that historical status/mapping and settlement attribution are indicative, without reproducing the six-paragraph disclaimer.
- Field-vs-management B/D classification remains unchanged in Oracle, as do the previously known limits of historical master-state estimates. Division-label changes are presentational and need audit-owner signoff.
- This is a separate front-end/controller/PDF-service change. **Do not deploy to production before a successful .NET build and PDF smoke test.**
