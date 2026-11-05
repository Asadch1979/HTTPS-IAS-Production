# Legacy Para: capture-only maker and database modularization

## Architecture decision (5 November 2026)

The maker page `/Execution/Add_Legacy_Para` is **data capturing only**.
It must never call `/ApiCalls/LegacyParaList` automatically, render a
`My Requests` table, or call `Detail` for already submitted requests.
Request lists belong only to the separate checker authorization screen
`/Execution/Authorize_Adding_Legacy_Para`.
Submitted/returned records are preserved in the database; no history,
request or compliance row is deleted. A separate future maker correction
screen is needed if returned-request correction is still required.

### Completed in the source branch

- Maker Razor view no longer contains `_LegacyParaRequestQueue`.
- Shared JavaScript initializes the pending queue **only** on checker page.
- Maker JavaScript no longer calls `app.refresh()` after submit or
  binds its old returned-request correction behavior.
- `LegacyParaLookup` is routed to dedicated
  `PKG_IAS_LEGACY_PARA_LOOKUP.P_GET_LOOKUP`, with typed parameters,
  selected active session context, page 192, exact selected-role API
  grant, and bounded/mapping-validated lookups.
- `LegacyParaList` is now checker-only (page 193), routed to
  `PKG_IAS_LEGACY_PARA_PENDING.P_GET_PENDING`. This is a read-only,
  small, independent queue procedure.
- General-purpose `DBConnection.LegacyParaRequests.cs` is retired
  and replaced with `DBConnection.LegacyParaWorkflow.cs`, which rejects
  List/Lookup calls. The existing `PKG_IAS_LEGACY_PARA.P_EXECUTE`
  remains for Submit, Detail and Decide until fully replaced.
- In `P_EXECUTE`, requestId/version are parsed only for operations
  needing them, using `ERROR ON ERROR NULL ON EMPTY`. Missing fields
  no longer cause ORA-40462 on unrelated operations. Required
  identity/version checks are not disabled.

### Required next phase: remove the original P_EXECUTE entirely

Do not DROP the original package while any live application call or
audit workflow still relies on it. Instead, prepare independently
compiled and tested replacements:

| Operation | Proposed procedure | Access |
|---|---|---|
| Submit new or corrected request | `PKG_IAS_LEGACY_PARA_CAPTURE.P_SUBMIT` | page 192, existing exact role API grant |
| Display a pending checker queue | `PKG_IAS_LEGACY_PARA_PENDING.P_GET_PENDING` | page 193, already provided |
| Load protected request details/history | `PKG_IAS_LEGACY_PARA_REVIEW.P_GET_DETAIL` | page 193 or separately approved maker correction flow |
| Decide/authorize and post | `PKG_IAS_LEGACY_PARA_REVIEW.P_DECIDE` | page 193, maker-checker segregation |
| Load maker reference data | `PKG_IAS_LEGACY_PARA_LOOKUP.P_GET_LOOKUP` | page 192, already provided |

**No string dispatch procedure** should be left in the final contract.
Each public procedure needs explicitly typed inputs and its own tests.
Keep shared security and entity-scope validation in a small **private**
package/helper taking all context inputs as parameters, not writable
package-global state, with exact selected role/group/API/page/HTTP method,
audit-office authority and relationship/parent/posting mapping.
All entry points fail closed, including Super User. No permission exemptions.

Submission must only write request and immutable history tables.
Authorization must repeat scope/duplicate/version checks and atomically
write the FAD master/text, post-compliance, responsibilities, attributes
and request history, using approved shared allocator and recognition
policy. Never truncate submitted text or invent recognition dates.
Preserve the existing transaction owner: no package COMMIT. The caller
commits only after successful complete cursor read; failures roll back.
Do not issue DROP PACKAGE until old consumers have been migrated and
the replacements passed Oracle compilation, integration and VAPT.

## Root cause of ORA-40462

The old P_EXECUTE attempted JSON_VALUE('$.version' ERROR ON ERROR) even
for LIST, whose C# DTO does not have a Version field. This is a missing
JSON key, not a nullable database table column. The new maker page
never triggers a List request; the new query procedures avoid generic
JSON dispatch entirely. Submission still uses the original package and
must be tested before claiming complete resolution.

## Deployment safety

This is a **staged source refactor, not a deploy-ready replacement**.
The new application must be deployed with both
`PKG_IAS_LEGACY_PARA_LOOKUP.sql` and
`PKG_IAS_LEGACY_PARA_PENDING.sql` compiled and valid. Compile the
updated `PKG_IAS_LEGACY_PARA.sql` as part of the same planned cutover.
Do not deploy web code first; missing procedures will cause HTTP 500.
Do not revoke old grants or drop the original package during this phase.
Explicit rollback includes the prior application, SQL packages, and
release-specific permission config; no data-loss cleanup is allowed.

Test maker form load with **zero List requests**, relationship →
controlling office → posting selection, lookup access, submit and CSRF.
Test checker page pending queue, detail and decisions, unauthorized
maker attempts to access the checker queue, concurrent requests, and
the role 15 API/page grants. Verify no ORA-40462 and no silent data
loss. The maker correction feature is deliberately deferred and should
not be treated as restored.

No production SQL, Oracle compilation, .NET build, runtime/transaction
test, activation or deployment performed in this branch.
