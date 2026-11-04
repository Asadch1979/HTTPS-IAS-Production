# Legacy para addition — safety stage, incomplete redesign

Status: NOT READY FOR MERGE, DEPLOYMENT OR MENU ACTIVATION.

This change retires the unsafe implementation. It does **not** implement the requested replacement workflow. There is no request table, replacement editor, request API or posting package yet. The two existing routes remain and display an unavailable notice. No page IDs or grants are changed. No scripts have been executed against Oracle.

## Findings

- `PKG_HD.P_ADD_LEGACY_PARA` committed directly to live FAD or CAD tables, using `MAX(ID)+1` / `MAX(PARA_ID)+1`.
- Its CAD branch selected organizational entities 112242/112248. Authorization separately selected hard-coded PP numbers 113092/111564. These cannot establish scope for a new workflow.
- FAD authorization had its update commented out yet returned a success message. The delete operation physically deleted CAD records.
- The API wrapped these results with `Status:true`. The retired endpoints now return HTTP 503 with `Status:false`; their DB methods throw before opening a connection. The four package bodies now raise -20870. Other package procedures remain byte-equivalent after newline normalization.
- The existing menu restoration script activated pages 192/193. It now raises before executing any update. Do not remove this block as part of ordinary deployment.

## Required evidence and remaining implementation

Run `00_PREFLIGHT.sql` read-only in the intended Oracle owner and review its spool. Repository SQL is not evidence of production identity, trigger, constraint or sequence configuration. Obtain the approved historical recognition rule for `PARA_ADDED_ON` and a non-production Oracle environment before writing or validating posting logic. Do not substitute SYSDATE.

Then implement and review:

1. A dedicated request aggregate with full observation CLOB, normalized responsible persons, attachments and reference relationships; immutable revision/action history, requestor/context timestamps, statuses and resulting IDs. Enforce eligible audit years <=2025 against the actual period source in both layers.
2. Shared CAU editor components and cascades, without engagement dependencies or calls to `save_observations_cau`; maker submission review and correction and a searchable checker preview/queue.
3. A dedicated Oracle package with trusted context/scope and page permission verification, maker/checker separation, server validation and duplicate checks on submit and approval. Lock requests and serialize approval of the same natural key; enforce uniqueness even for simultaneous requests. Retain returned/rejected history.
4. Atomic three-table posting using verified key allocation, legacy ID/REF_P and compliance OLD_PARA_ID relationships, IND='O', approved outstanding lifecycle and historical recognition date. Resolve supported mappings for every meaningful editor field before implementing inserts. Preserve old CAD/HO records; new requests must follow the three-table contract.
5. New authenticated APIs with selected context, page permissions, organizational scope, explicit CSRF validation (the current API controller has a class-level IgnoreAntiforgeryToken), rich-text sanitization that preserves supported tables, encoded preview metadata, structured errors and audit events that accurately reflect transaction outcomes.

## Review and deployment order

Nothing in this directory authorizes deployment. Review the SQL and application security changes first. After the replacement implementation and tests are complete: back up deployed package source and schema metadata; run preflight; apply reviewed request DDL and indexes; compile and verify the dedicated package and retirement changes; deploy the application with activation blocked; execute the integration matrix below; obtain a separate menu activation decision. Never bulk-enable permissions or add users.

For this safety stage, compile the modified `PKG_HD.sql` only in an approved review environment, checking USER_ERRORS and dependency validity. It retains public signatures, but callers will now receive -20870. No historical records are migrated or deleted.

## Rollback

Keep the feature unavailable. Roll back unrelated application changes independently. If reverting this safety stage, retain the 503 guard and menu activation block; restoring the old SQL bodies or DB methods would restore unsafe posting. Preserve database backups and any future request/history data. Do not delete authorized live records as a rollback strategy; reconcile through an approved data repair. No data rollback is needed for the current local-only changes.

## Test report (2026-11-04)

| Check | Result |
|---|---|
| `dotnet build AIS/AIS.csproj --no-restore -v quiet` | Passed; 0 errors, 57 warnings |
| `node --test tests/legacy-para-addition/retirement.test.cjs` | 3 passed |
| `git diff --check` | Passed |
| Before/after live table counts | Not measured: no Oracle execution |
| Submission/correction/return/reject | Not implemented or tested |
| Authorization, parallel approval, duplicate contention, rollback injection | Not implemented or tested |
| Rich-text formatting/tables, attachments/responsibilities | Not implemented or tested |
| Legacy screens, compliance, Para Snap, executive reporting | Not tested |

The source regression checks verify that only the four intended SQL bodies changed, retired DB calls are absent, retired APIs return an unavailable result, and the activation script raises before DML. They do not substitute for HTTP/security or Oracle integration tests.

For the completed workflow, capture target counts before submission, after submission and after each transition in an isolated test dataset. Pending/returned/rejected transitions must have delta 0/0/0; first authorization must have delta 1/1/1 with matching keys; replay and parallel approvals must add zero further rows. Inject a failure at each insert and before status update and verify full rollback. Test existing-paras and pending-request duplicates, self-approval, unauthorized/cross-scope access, missing session/context, CSRF failure, malicious preview content and >2025 years. Verify request/action history and created identifiers as well as table counts.
