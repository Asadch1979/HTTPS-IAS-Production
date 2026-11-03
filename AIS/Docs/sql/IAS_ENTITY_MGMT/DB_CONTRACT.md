# Database handoff contract for application development (NOT an implementation instruction to change production)

## Grounded source design

- Master: `T_AUDITEE_ENTITIES` (`ENTITY_ID` currently lacks a declared PK/identity mechanism in the reviewed schema; legacy source uses MAX+1, pending a safe migration; basic editable identity, `TYPE_ID`, `AUDITBY_ID`, flags, risk/size, contact data).
- Current mapping: `T_AUDITEE_ENTITIES_MAPING` (parent, audit cluster `AUDITEDBY`, reporting, `GM_OFFICE`, `DIV_OFFICE`, `B_GROUP`, names/codes/types derived from referenced masters).
- Compliance: `AIS_T_AU_POST_COMPLIANCE` (COM_ID keyed). Source column `SETTELED_ON` is intentionally spelled with the extra `E`.
- New history: `T_IAS_ENTITY_CHANGE_LOG`, append-only events and before/after CLOB snapshots; `T_IAS_ENTITY_PARA_ARCHIVE`, immutable full source row snapshot plus archival metadata.

## Immediately supplied query contracts (read-only)

Oracle package: `PKG_IAS_ENTITY_MGMT`, initially restricted to read operations:

- `P_SEARCH_ENTITIES(P_ENTITY_TYPE_ID,P_SEARCH_TEXT,P_STATUS,P_OFFSET,P_PAGE_SIZE,O_CURSOR)`; status: ALL/ACTIVE/INACTIVE; limit <=200.
- `P_GET_ENTITY_PROFILE(P_ENTITY_ID,O_CURSOR)`; one-to-many current mapping is deliberately not collapsed.
- `P_PREVIEW_ENTITY_CLOSURE(P_ENTITY_ID,O_CURSOR)`; preliminary counts (including non-final engagement statuses and unusual para statuses) only, not authorization to close.
- `P_GET_ENTITY_CHANGE_LOG(P_ENTITY_ID,O_CURSOR)`.
- `P_GET_ENTITY_PARA_ARCHIVE(P_ENTITY_ID,O_CURSOR)`.

## Not yet approved for implementation

All database **write** APIs, including entity save, mapping edit, organizational movement, bulk edit, para archival, closure and reopening, MUST await production metadata confirmation, secure database user-context verification and the historical-reporting closure specification. Do not expose old direct-update procedures as substitutes.

Any future write package must require trusted and validated session/page/entity-scope authorization, be called only from secured application routes, use bound parameters, enforce maker-checker for closure, and manage one database transaction per approved operation. Avoid commits inside per-entity loops. The existing working Engagement Reversal and Organization Structure move logic are not to be modified by Phase 1.

The final closure semantics must reconcile `PARA_STATUS=0`, requested nulling of the two DATE fields, audit-status 28 used in shifting, and as-of KPI/report consumers. No update should occur before this is proven with sample historical periods and snapshots.

## Production metadata requirements added 3 November 2026

The production preflight shows zero indexes and zero constraints on both `T_AUDITEE_ENTITIES` and `T_AUDITEE_ENTITIES_MAPING`, though mapping IDs currently have no duplicates. Do not assume master IDs are unique until the targeted diagnostic is reviewed, and do not introduce a `MAX+1` writer. Before an `INSERT`/`UPDATE` capability is approved, design uniqueness enforcement and migrate existing writers to a sequence/identity strategy without conflict.
