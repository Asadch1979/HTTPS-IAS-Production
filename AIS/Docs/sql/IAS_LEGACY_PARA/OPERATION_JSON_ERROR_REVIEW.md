# Legacy Para P_EXECUTE operation contract and ORA-40462 fix

Source reviewed on 2026-11-05, against the latest main package commit
`3b4b366752cee47a91a243f6b781501ccbb43efc`.

## What the one public procedure does

`P_EXECUTE` is a five-operation dispatch entry point:
`LOOKUP` (reference data), `LIST` (maker/checker queues),
`DETAIL` (request/history), `SUBMIT` (new/corrected draft),
and `DECIDE` (approve/return/reject and atomic historical posting).
The procedure also rechecks selected context/role/group/page, entity
scope, duplicate historical records, approved recognition policy,
shared ID allocation and write transaction state.

Do not DROP this package or remove `P_EXECUTE` until a reviewed
replacement implements **all** five operations and posting dependencies.

## Root cause of ORA-40462

The unpatched package always extracted `$.requestId` and `$.version`
with `ERROR ON ERROR` before dispatching. The C# `LegacyParaQuery`
model carries `RequestId`, `Queue`, `Search` and `Offset`, but
**not Version**; the `LegacyParaLookup` model carries neither request
ID nor version. Oracle applies ERROR ON ERROR to missing paths unless
NULL ON EMPTY is specified, raising `ORA-40462`. A LIST call cannot
reach its cursor query while that unconditional extraction is present.

The revised package reads requestId only for SUBMIT/DETAIL/DECIDE and
version only for SUBMIT/DECIDE. Missing required details are rejected
explicitly with existing `-20801` fail-closed validation; wrong types
still raise strict JSON conversion errors. LIST/LOOKUP never read the
unneeded fields. No API, role, page or scope checks are bypassed.

## Separate ORA-40477

`ORA-40477` indicates an internal Oracle JSON processing error, not
necessarily the same defect. If it persists after this fix, confirm
database version and patch level and capture the deployed package's
error stack and source, exact operation, safe metadata about JSON
shape (not payload text or attachments), and the trace reference.
Escalate an internal JSON processing error to the DBA/Oracle Support.

## Read-only source inspection of deployed package

```sql
SELECT line, text
FROM all_source
WHERE owner = 'ZTBLAIS_PROD'
  AND name = 'PKG_IAS_LEGACY_PARA'
  AND type = 'PACKAGE BODY'
  AND ((line BETWEEN 275 AND 325) OR (line BETWEEN 800 AND 830))
ORDER BY line;
```

```sql
SELECT object_name, object_type, status, last_ddl_time
FROM all_objects
WHERE owner = 'ZTBLAIS_PROD'
  AND object_name = 'PKG_IAS_LEGACY_PARA';
```

To reproduce the missing-version contract without invoking the
write-capable stored procedure:

```sql
SELECT JSON_VALUE('{"requestId":0,"queue":false,"search":"","offset":0}',
                  '$.version' RETURNING NUMBER ERROR ON ERROR NULL ON EMPTY)
       AS optional_version
FROM dual;
```

Expected: NULL, with no JSON exception. The original form without
`NULL ON EMPTY` raises ORA-40462 on supported Oracle versions.

## Security regression and PR #369

The latest manually reformatted main package had dropped the former
exact selected-role, API path/HTTP verb and page dependency check, and
weakened the original entity mapping controls. This branch restores
those **without activating any grants**. The independent PR #369 adds
the relationship/controlling office/place of posting hierarchy and
related `LOOKUP` kinds; its package source is based on an earlier
revision. **Reconcile both branches before release**. Do not merge a
package that overwrites either the JSON fix, relationship lookups or
the strict selected-role and mapping controls.

No live Oracle compile, SQL execution, backend build, deployment or
end-to-end tests performed by this source edit.
