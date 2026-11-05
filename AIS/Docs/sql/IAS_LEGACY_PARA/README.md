# Legacy para addition source redesign

The two existing routes and page IDs 192/193 are retained. No menu or user assignments are added and no live permission state is changed. Source registration prepares only inactive new API grants. The application switch `LegacyParaAddition:Enabled` defaults to false, and the dedicated database control row defaults to `N`. This assignment does not activate either switch. The retired direct-to-live APIs, DB methods and four unsafe PKG_HD bodies remain disabled. Existing CAD/HO records are preserved and checked for duplicates; the new workflow always uses the FAD three-table contract.

## Source changes

- `Views/Execution/Add_Legacy_Para.cshtml` provides entity/year selection, full observation entry and My Requests with returned-request correction.
- `Views/Execution/Authorize_Adding_Legacy_Para.cshtml` provides a searchable, paginated queue, complete sandboxed observation preview, details, attachments, history and checker decisions.
- `_CauObservationEditor.cshtml` is extracted from the existing CAU view and used by both flows. CAU routing/reply controls remain CAU-only; legacy requests use the selected audit entity. The CAU view retains its existing script and save behavior. The legacy view loads only the dedicated request scripts and never requires an engagement.
- `_LegacyParaRequestQueue.cshtml`, `legacy-para-requests.js` and the two existing view scripts implement the consolidated entry and review flow with DOM text encoding and direct event binding.
- `LegacyParaRequest.cs`, `LegacyParaOperation.cs`, `ApiCallsController.LegacyPara.cs`, `DBConnection.LegacyParaRequests.cs` and `LegacyParaHtml.cs` provide typed validation, authenticated selected context, page/API permission enforcement, CSRF, parameter binding, safe complete HTML handling and audit events.
- `IAS_LEGACY_PARA_DDL.sql` defines requests, immutable revision/action snapshots, a permanent linked attributes record, historical recognition policy and a disabled control row. The request payload includes responsible-person details and attachment bytes; no files are written to public storage.
- `PKG_IAS_LEGACY_PARA.sql` implements submission, correction/resubmission, lists/details/lookups, return, rejection and authorization. `P_EXECUTE` receives identity/context only from the server session and re-derives active assignment, group permissions and organizational scope in the database.

Submission and correction write only the request and history. Authorization locks the request and new allocator-binding rows, repeats duplicate/master/scope checks, posts main/text/compliance plus supported responsibility mappings and permanent attributes, records resulting IDs, and appends history. There are no package commits. The application transaction commits only after the complete procedure and cursor read succeed; failures roll back. A successful repeated authorization returns the existing IDs. Returned/rejected requests are retained.

## Repository-derived mappings

`PKG_HD.P_GetAuditYear` supplies `T_AU_OLD_AUDIT_PERIOD.AUDIT_YEAR`; eligible years are limited to 1900–2025 and must exist in that master. `PKG_HD.P_GetAuditEntities` supplies the conservative direct Audit Cluster scope: `AUDITABLE='Y'` and `AUDITBY_ID` equal to the selected active context entity. Type, code and reporting auditor are re-derived from `T_AUDITEE_ENTITIES`.

`PKG_AR.P_update_legacy_Para_text` establishes `T_AU_OLD_PARAS_FAD_TEXT.REF_P`, compliance `OLD_PARA_ID`, old-para indicator `O`, status 8, cycle 0 and stages 13 (type 6) / 21 (other types). The new main row uses `B` plus the numeric ID for REF_P, preserving the convention in the retired FAD submission source. Main `PROCESS_DETAIL` holds checklist detail; annexure risk comes from `T_AUDIT_CHECKLIST_ANNEXURE.RISK`.

`PKG_AR.p_add_para_responsibility` establishes the existing responsibility table columns and numeric `OBS_ID` / `REF_P` association. The new package maps those fields there. Reference IDs are checked against `VW_REFERENCE_MASTER_DETAIL`. No engagement-bound reference row is fabricated. Structured ANNEX and NO_OF_INSTANCES are posted to the FAD row; ANNEX, AMOUNT and NO_OF_INSTANCES are posted to compliance. Historical ANNEXURE is left unset. Category/subchecklist IDs, modern annexure ID, reference selection, responsibility roles and attachment content remain in the permanent attributes JSON keyed by legacy numeric ID and REF_P, as well as the complete request/history snapshots. The shared circular/manual picker retains its current APIs and permission behavior.

## Unresolved schema and policy assumptions

- No live schema was inspected. Target ID defaults/triggers/capacities and a shared operational allocator are unestablished. PKG_IAS_LEGACY_IDS replaces operational table locks/MAX allocation with approved owner-managed shared-sequence bindings. Bindings start unconfigured and require approval and explicit all-writer coordination; posting fails closed otherwise. A new workflow-only sequence cannot safely coexist with uncoordinated MAX(ID)+1 writers. This source does not claim that the existing writers have been coordinated or modify unrelated procedures.
- The approved historical audit recognition formula is absent from the supplied SQL sources. There is no fallback to submission date, authorization date or SYSDATE. Authorization requires an explicit approved per-year policy record with recognition date, approving employee, approval date and rule description. No policy date is seeded. Actual submission and decision timestamps remain separate.
- Oracle SQL/JSON object, table and aggregate support (including CLOB return values) is assumed. Package/DDL compatibility with any running database is unestablished.
- Existing final-table reporting conventions are taken from PKG_AR. No reporting package is modified. Reference/attachment and other fields without an established engagement-free final-table mapping use the new attributes record. No existing reporting consumer is assumed to read that extension automatically.
- Scope deliberately uses the current direct Audit Cluster relationship. Broader cross-cluster or inherited authorization is not inferred from hard-coded users or historical CAD/HO exceptions. Database checks additionally require an active group with the selected role, matching selected server-session group/role and the active selected T_USER_CONTEXT_ASSIGNMENT. The selected assignment is the membership authority. The default T_USER_MAPING row is not an additional membership gate.
- The current CAU observation-template provider returns an empty list in `DBConnection.GetAuditObservationTemplates`. Changing checklist selection therefore preserves entered legacy text; no unsupported template table is invented.
- Attachments are limited to PDF, PNG and JPEG, 5 MB per file / 10 MB total, delivered as downloads after authorized request retrieval. Extension and file signatures are checked; this does not imply a malware-scanning service exists.

Only repository source was modified. No database connection, SQL execution, package compilation, application execution, testing, activation or deployment was performed. The pre-existing preflight source and security regression artifact are preserved without execution.

## Correction source files

`PKG_IAS_LEGACY_IDS.sql` adds disabled/unconfigured shared allocator bindings without operational table locks. `IAS_LEGACY_PARA_API_PERMISSIONS.sql` registers new endpoints inactive, maps pages 192/193 and the six read-only observation picker dependencies, and prepares inactive API-role rows only for existing page assignments. Existing API anchors, grants and activation states are preserved; no page assignment or activation is supplied. The mapping is an explicit source manifest, not a permission bypass. Canonical POST routes are /ApiCalls/LegacyParaSubmit, /ApiCalls/LegacyParaLookup, /ApiCalls/LegacyParaList, /ApiCalls/LegacyParaDetail and /ApiCalls/LegacyParaDecide. They do not contain PathBase. PKG_AD uses these dependencies for page assignment and preserves shared dependencies on revocation.

`IAS_LEGACY_RECOGNITION_POLICY.sql` provides an empty reviewed-policy JSON manifest and an insert-only importer. Explicit dates, approving employee and approval reference/rule must be present in repository policy; no rule/date can be inferred from the available reporting SQL. Existing differing policy is rejected. No date is invented or seeded.

The legacy editor now reacquires the plugin-cloned textarea, rebuilds the editor for clear/restore to reset code view and undo history, reads the active editing surface, and prevents submission during asynchronous restoration. No fallback can resurrect cleared observation HTML. Existing controllers, DBConnection and package files retain their original encoding/newline conventions.

## ApiCalls consolidation (5 November 2026)

The separate LegacyParaRequestsController and /api/LegacyParaRequests/* endpoints are retired.
The existing ApiCallsController exposes five protected POST actions from
ApiCallsController.LegacyPara.cs, preserving the page views and their maker/checker flow.
The action-level [Authorize, ValidateAntiForgeryToken] attributes are essential
because the main ApiCallsController has an [IgnoreAntiforgeryToken] controller default.
The normal ApiCalls rate limit still applies.

The operation is a server-selected LegacyParaOperation enum. The maker/checker page
is the server-selected LegacyParaPage enum (192/193); the client submits only the
queue indicator and request details, never the operation or page number.
The Oracle P_EXECUTE contract still accepts the exact operation and page values
and strictly validates them. Its permission query now checks the new ApiCalls
route prefix. No database permission, session, selected role, scope, activity
logging, or posting validation is bypassed. Full API and page permission checks
remain mandatory even for Super Users.

**Coordinated DBA and application release is required.** For an existing database,
review and execute IAS_LEGACY_PARA_APICALLS_MIGRATION.sql to rename the five
previously approved API registrations *in place*, preserving API_ID and active
role/page decisions. Check all maker/checker dependencies and selected-role
grants before commit. Deploy the matching PKG_IAS_LEGACY_PARA.sql and application
during the same controlled cutover; there is no old-route alias or automatic
activation. The registration manifest handles fresh installations and still
creates any missing rows inactive. Users must sign out and sign in to refresh
cached permissions after approved changes. Roll back the coordinated release
if either the package, API routes or registrations are out of sync.

ORA-20801 remains a fail-closed unsupported-operation/page error. The new
server code binds strongly typed operations and page values to reduce
mismatches, and reports a fixed OPERATION_CONTRACT_MISMATCH reference on 20801
without revealing Oracle errors to browsers. A persistent 20801 requires an
examination of the server log and deployed code/package versions, not disabling
the database validation.

Source-level tests: tests/legacy-para/apicalls-routing.test.cjs.
No application compilation, Oracle deployment or end-to-end checks have been
performed by this repository edit.
