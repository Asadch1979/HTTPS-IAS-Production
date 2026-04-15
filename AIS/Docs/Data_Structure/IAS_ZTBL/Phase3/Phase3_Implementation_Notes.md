# IAS_ZTBL Phase 3 Implementation Notes

## Assumptions

- target schema is `IAS_ZTBL`
- legacy source schema is `ZTBLAIS_PROD`
- IAS_ZTBL has `SELECT` access to required legacy source objects before migration scripts run
- numbered DDL scripts are executed before numbered migration scripts
- the Phase 2 registers remain the authority for unresolved-field review

## One-Time vs Repeatable Behavior

- `00` through `08` are one-time environment/build scripts unless explicitly adjusted
- `05_seed_lookup_types_values.sql` is written to be rerunnable where practical
- `10_migration_precheck.sql` is safe to rerun for validation and batch registration
- `11` through `19` are controlled migration scripts and should run against a defined migration batch only
- `20_migration_reconciliation_checks.sql` is read-only and can be rerun freely

## Handling ReviewRequired Fields

Phase 2 identified a large set of fields that could not be mapped safely into final business columns without review.

Phase 3 handles those fields by:

- mapping deterministic fields directly into IAS_ZTBL target columns
- capturing unresolved source values into `tbl_migration_issue`
- capturing unresolved filesystem/document work into `tbl_document_migration_queue`

This avoids silent data loss while keeping the future-state schema clean.

## Migration Batch Convention

The delivered migration scripts use a named batch code:

- `PHASE3_BASELINE_01`

This can be changed per rehearsal, but all scripts in one execution cycle must use the same batch code.

## Source Inputs Used

The Phase 3 pack was built against the approved execution baseline and the legacy source exports documented in Phase 2, including the workbook exports and the Codex text exports.
