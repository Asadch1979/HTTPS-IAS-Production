# IAS CCO — scoped authorization view and UI changes

**Date:** 28 October 2026 · **Repository:** `Asadch1979/HTTPS-IAS-Production` · **Target:** PR #350 follow-up · **Status:** design/code-ready handoff, NOT database-validated or activated.

## A. Exact deliverable for V_CCO_AUTHORIZED_ENTITIES

See `CCO_AUTHORIZED_ENTITIES.sql.review` (companion file). It defines:

1. `T_CCO_SCOPE_ENTITIES_GTT` — session-local authorization IDs, `ON COMMIT DELETE ROWS`.
2. `PKG_CCO_SCOPE` — begin/assert/end calls checking current active IAS user, role 38 or independently verified Super User 1, session, active context assignment, active CCO page 1000302 and active role-group mapping; generates scope token in Oracle using `SYS_GUID()`.
3. Trusted Oracle context `IAS_CCO_CTX` — must be created by DBA using `PKG_CCO_SCOPE` as its sole trusted package. **Cannot** be set from application SQL or HTTP parameters.
4. `V_CCO_AUTHORIZED_ENTITIES (SCOPE_TOKEN, ENTITY_ID)` — joins session-local scope rows to live, active IAS authorization records and exact trusted context.

**Deployment gate:** A separate locked-down database object owner must own GTT, context, package and view. Shared IAS app service login should have execute/select grants only and **must not** own or directly mutate the GTT. If an app login is the object owner, the DB authorization boundary is inadequate and this design must NOT be activated. Cross-schema privileges / synonym names must be reviewed by DBA. The repository confirms column and join conventions from `PKG_HEAD_AUDIT_MONITOR.P_VALIDATE_SCOPE`; the script has not been compiled against Oracle and may require DB-verified schema/role adjustments.

**Connection lifecycle:** one ASP.NET request → existing session/context/page/exact endpoint permission check → one ODP.NET connection → explicit transaction → `P_BEGIN_SCOPE` from **server-authenticated** values (not filters) → assert → bound `PKG_CCO_REPORTING` package read using DB-issued token → fully consume cursor, validate row bound + role/context again → `P_END_SCOPE` → commit/rollback → release connection. No auto-commit between begin and cursor. `ON COMMIT DELETE ROWS` and context cleanup must be proven with pooled concurrent connections. Maintain current 503 fail-closed state until passed.

**Scope qualification:** role38 legitimately has CCO bank-wide review; group/division/GM/region selections only narrow the verified set. Include auditees without valid paths in the authorized population to report mapping exceptions; do not discard them with an inner join to a hierarchy view. `AUDITED_BY` is not auditee ownership.

**Unresolved pre-activation:** `P_GET_ORG_FILTERS` secure selector reader; `PKG_CCO_REPORTING` path assertion after token validation; one-row-per-COM_ID mapping and movement/source parity; HO fallback and CAU exception decisions; `SOURCE_ROW_COUNT` overflow reject; cursor alias/data-type mapping; exceptions register details; current vs historical cycle quality; actual organization names; authorization grants. No package/view activation, phase-2 UI enabling or production deployment until staging and security sign-off.

## B. Replace organization dropdown with existing Organization Structure as a table

The existing `OrganizationStructureService`, `DBConnection.GetOrganizationNodes` and `PKG_ORG_STRUCTURE.P_GET_NODES` already return `OrganizationNode` (`EntityId`, `EntityName`, `EntityTypeId`, `ParentEntityId`, `Reporting1...6`, `HasTypeConflict`, `IsOrphan`). Reuse this model and entity/parent relationships for presentation, not the chart or move endpoint.

In CCO `org-position`, `performance`, `effectiveness` remove the disabled `fieldset` and Group/Division/GM dropdown selector UI. Replace it with a **read-only, accessible expandable hierarchical table**, whose row levels are validated Group → Division → GM Office → Region/Head Office unit/Branch as actual IAS structure supports. Columns: **Organization Level | Reporting Office | Entity | Opening Cases | Para Added | Total Cases | Settled | Outstanding | Settlement % | Mapping Status**; optional risk/aging/cycle columns via horizontal scrolling or detail panel. Show indent, expand/collapse, Search, expand all, and grand totals, with explicit `Unmapped/Conflict` rows. Clicking a row narrows the selected authorized organization path and refreshes table from the server; no browser-chosen root can expand scope.

**Important existing behavior:** `OrganizationStructureService.GetSnapshot()` narrows ordinary non-SuperUsers to the user's posting. A CCO role38 bank-wide page must NOT silently reuse the wrong posting-only snapshot or call the unrestricted `OrganizationStructure/Data` endpoint without its own page permission. Use a new CCO-only read method that obtains `PKG_ORG_STRUCTURE.P_GET_NODES` on the verified CCO scope and intersects every node with `V_CCO_AUTHORIZED_ENTITIES` within the same DB connection/transaction. Compare IDs with existing `/OrganizationStructure` for structure parity; no move/preview buttons.

Keep the UI **read-only/pending** until secure paths and Oracle data are verified. Do not switch `data-enabled` to true prematurely. Remove Group/Division/GM dropdown elements, including client `options()` cascading logic, and replace with table row expansion and validated server requests. Dates persist.

## C. Rename Field Office Position and remove report letters, CCO only

Rename visible tab `field-office` to **Audit Position**. Do **not** change its ID or the internal `ReportForView("field-office") => "B"`. For all three report tabs remove `Report B ·`, `Report C ·`, `Report D ·` from onscreen headings, selector text and document titles for CCO only. Use:

- `field-office` → **Audit Position** (internally Report B)
- `gm-region` → **GM / Region Position** (internally Report C)
- `regional-rankings` → **Regional Rankings** (internally Report D)

Changes belong in `CcoDashboardPresentation.cs`, `_Reports.cshtml` and `executive-reports.js`, guarded by `root.dataset.perspective === 'CCO'`. Preserve canonical `view=reports`, the B/C/D API/report allowlist, dates, PDF/CSV security and Report C benchmark. CIA/GROUP/SNAP labels and behavior must remain untouched.

## D. Critical Audit Observations — new CCO-only tab, reporting office and risk filter

Add a distinct **Critical Audit Observations** CCO tab (eight top-level tabs overall). Remove the `critical` section from `overview` **only for CCO**; preserve its existing KPI counts, risk charts, aging and cycles. Define its own tab/view allowlist and ensure `/api/ExecutiveDashboard/GetCriticalItems?view=...` remains restricted appropriately. Do NOT grant a generic CCO text or para endpoint.

For the new tab, table columns should start: **Reporting Office | Entity | Audit Period / Para | Risk | Cycle | Age | Days Since Movement | Observation**. Reporting Office means the auditee's verified reporting parent (`V_EXEC_ENTITY_SCOPE.REPORTING_OFFICE_ID` or `REPORTING_1`) and name resolved from verified entity directory — NEVER the audit-performing `AUDITED_BY` value. Null/ambiguous is displayed `Unmapped reporting office`; do not guess.

**Ordering:** SQL before `ROW_NUMBER`/pagination: normalized Reporting Office name ASC NULLS LAST, Entity name ASC NULLS LAST, Age in days DESC NULLS LAST, COM_ID ASC. Verify the paging limit (<=250/500 as approved) and row counts; sorting only the first 100 browser rows is incorrect.

**Risk selector:** `All Risks`, `High (1)`, `Medium (2)`, `Low (3)`, `Unrated`. Default **High** to preserve legacy critical observations until the new API is approved. Risk filter must be enforced in Oracle before paging, not only on downloaded rows. Verify correct risk display for all observations, NOT a false fixed label `Critical = High` when filters change. Keep observation text subject to strict scoped per-COM_ID authorization, size limits and safe escaping. If the business insists this tab is only *critical* observations, show High only; otherwise use the clearer heading **Audit Observations** while keeping the requested CCO navigation name.

**Do not modify** shared `PKG_EXEC_DASHBOARD.P_GET_SNAP_CRITICAL` to remove `AND k.risk_id = 1`: it serves CIA/Snap/other perspectives. Develop a dedicated CCO-only read in an authorized package using `V_EXEC_KPI_LIFECYCLE` eligibility, verified CCO scope view, selected dates, verified reporting parent, risk code and limit; register exact API permissions inactive until signed approval. Existing CCO high-risk observations can remain accessible via the legacy endpoint while the additional-risk facility remains pending.

## E. Test and release gates

- Existing seven-tab tests must be updated for the new eight-tab CCO navigation and explicit non-CCO isolation.
- Verify KPI values (opening + additions = total), label order, as-of caveats.
- Verify organization hierarchy table against current `/OrganizationStructure` for real scoped entities, including directly attached/ambiguous entities and entity shifts; no DML controls.
- Verify report tab/headings label only on CCO; B/C/D endpoint/allowlist and original other dashboards identical.
- Verify critical table sorting **before paging**, count/limit, all-risk eligibility, reporting office != audit-performing department, XSS protection, text scope, denial on revoked session/context and risk injection; preserve original high-risk query.
- Security tests for Oracle context/token and GTT: no init => no rows; revoked session/context/page => fail; wrong role => fail; connection pool isolation; commit/rollback => no rows; direct GTT DML denied; missing/ambiguous hierarchy stays visible as exception.
- Validate `V_EXEC_REPORT_SOURCE` vs `V_EXEC_KPI_LIFECYCLE` unique `COM_ID`, dates, status9, same-period added/settled, rectified and reopened; do not force totals to reconcile.
- Keep `PKG_CCO_REPORTING.sql.review` and new scope SQL **REVIEW ONLY**; no production deployment, automatic grants, or UI activation. Execute/compile only after an explicitly approved non-production database identity is provided.
