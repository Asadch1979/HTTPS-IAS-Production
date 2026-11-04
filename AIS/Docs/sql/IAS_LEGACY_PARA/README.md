# Legacy para addition source redesign

The two existing routes and page IDs 192/193 are retained. No menu, user, role or API grants are added. The application switch `LegacyParaAddition:Enabled` defaults to false, and the dedicated database control row defaults to `N`. This assignment does not activate either switch. The retired direct-to-live APIs, DB methods and four unsafe PKG_HD bodies remain disabled. Existing CAD/HO records are preserved and checked for duplicates; the new workflow always uses the FAD three-table contract.

## Source changes

- `Views/Execution/Add_Legacy_Para.cshtml` provides entity/year selection, full observation entry and My Requests with returned-request correction.
- `Views/Execution/Authorize_Adding_Legacy_Para.cshtml` provides a searchable, paginated queue, complete sandboxed observation preview, details, attachments, history and checker decisions.
- `_CauObservationEditor.cshtml` is extracted from the existing CAU view and used by both flows. CAU routing/reply controls remain CAU-only; legacy requests use the selected audit entity. The CAU view retains its existing script and save behavior. The legacy view loads only the dedicated request scripts and never requires an engagement.
- `_LegacyParaRequestQueue.cshtml`, `legacy-para-requests.js` and the two existing view scripts implement the consolidated entry and review flow with DOM text encoding and direct event binding.
- `LegacyParaRequest.cs`, `LegacyParaRequestsController.cs`, `DBConnection.LegacyParaRequests.cs` and `LegacyParaHtml.cs` provide typed validation, authenticated selected context, page/API permission enforcement, CSRF, parameter binding, safe complete HTML handling and audit events.
- `IAS_LEGACY_PARA_DDL.sql` defines requests, immutable revision/action snapshots, a permanent linked attributes record, historical recognition policy and a disabled control row. The request payload includes responsible-person details and attachment bytes; no files are written to public storage.
- `PKG_IAS_LEGACY_PARA.sql` implements submission, correction/resubmission, lists/details/lookups, return, rejection and authorization. `P_EXECUTE` receives identity/context only from the server session and re-derives active assignment, group permissions and organizational scope in the database.

Submission and correction write only the request and history. Authorization locks the request and target tables, repeats duplicate/master/scope checks, posts main/text/compliance plus supported responsibility mappings and permanent attributes, records resulting IDs, and appends history. There are no package commits. The application transaction commits only after the complete procedure and cursor read succeed; failures roll back. A successful repeated authorization returns the existing IDs. Returned/rejected requests are retained.

## Repository-derived mappings

`PKG_HD.P_GetAuditYear` supplies `T_AU_OLD_AUDIT_PERIOD.AUDIT_YEAR`; eligible years are limited to 1900–2025 and must exist in that master. `PKG_HD.P_GetAuditEntities` supplies the conservative direct Audit Cluster scope: `AUDITABLE='Y'` and `AUDITBY_ID` equal to the selected active context entity. Type, code and reporting auditor are re-derived from `T_AUDITEE_ENTITIES`.

`PKG_AR.P_update_legacy_Para_text` establishes `T_AU_OLD_PARAS_FAD_TEXT.REF_P`, compliance `OLD_PARA_ID`, old-para indicator `O`, status 8, cycle 0 and stages 13 (type 6) / 21 (other types). The new main row uses `B` plus the numeric ID for REF_P, preserving the convention in the retired FAD submission source. Main `PROCESS_DETAIL` holds checklist detail; annexure risk comes from `T_AUDIT_CHECKLIST_ANNEXURE.RISK`.

`PKG_AR.p_add_para_responsibility` establishes the existing responsibility table columns and numeric `OBS_ID` / `REF_P` association. The new package maps those fields there. Reference IDs are checked against `VW_REFERENCE_MASTER_DETAIL`. No engagement-bound reference row is fabricated. Instance count, category/subchecklist IDs, modern annexure ID, reference selection, responsibility roles and attachment content remain in the permanent attributes JSON keyed by legacy numeric ID and REF_P, as well as the complete request/history snapshots. The shared circular/manual picker retains its current APIs and permission behavior.

## Unresolved schema and policy assumptions

- No live schema was inspected. The source assumes the existing target columns and explicit-ID inserts used by PKG_HD/PKG_AR remain applicable. The repository does not establish target identity/default triggers, complete column capacities, constraints or a common allocator. No identity or sequence is invented for existing tables. The package uses table locks followed by maximum-based allocation, consistent with explicit IDs in the existing source. Lock contention causes a failed transaction. Older external writers that precompute IDs outside these locks remain outside this package's concurrency contract; unique-key collisions fail rather than being reported as success.
- The approved historical audit recognition formula is absent from the supplied SQL sources. There is no fallback to submission date, authorization date or SYSDATE. Authorization requires an explicit approved per-year policy record with recognition date, approving employee, approval date and rule description. No policy date is seeded. Actual submission and decision timestamps remain separate.
- Oracle SQL/JSON object, table and aggregate support (including CLOB return values) is assumed. Package/DDL compatibility with any running database is unestablished.
- Existing final-table reporting conventions are taken from PKG_AR. No reporting package is modified. Instance count/reference/attachment fields without an established engagement-free final-table mapping use the new attributes record. No existing reporting consumer is assumed to read that extension automatically.
- Scope deliberately uses the current direct Audit Cluster relationship. Broader cross-cluster or inherited authorization is not inferred from hard-coded users or historical CAD/HO exceptions. No additional permission mappings are supplied.
- The current CAU observation-template provider returns an empty list in `DBConnection.GetAuditObservationTemplates`. Changing checklist selection therefore preserves entered legacy text; no unsupported template table is invented.
- Attachments are limited to PDF, PNG and JPEG, 5 MB per file / 10 MB total, delivered as downloads after authorized request retrieval. Extension and file signatures are checked; this does not imply a malware-scanning service exists.

Only repository source was modified. No database connection, SQL execution, package compilation, application execution, testing, activation or deployment was performed. Prior preflight and retirement-check artifacts from the safety stage are removed from this deliverable.
