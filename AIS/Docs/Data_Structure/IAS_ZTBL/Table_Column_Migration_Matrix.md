# Old-to-New Table and Column Migration Matrix

Machine-readable register:

- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\old_to_new_table_column_migration_matrix.csv`

Source inventories:

- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\legacy_table_columns.csv`
- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\target_table_columns.csv`

## Coverage

- active legacy base-table column rows mapped: `2411`

## Target Availability Results

| Availability | Count | Meaning |
| --- | ---: | --- |
| `ExistingTableExistingColumn` | 392 | A direct landing table and landing column already exist in current IAS_ZTBL SQL. |
| `ExistingTableRequiredColumn` | 542 | The target table exists, but the current IAS_ZTBL DDL still needs one or more columns added. |
| `RequiredTable` | 467 | The migration needs a target table that is not yet present in current IAS_ZTBL SQL. |
| `ReviewRequired` | 1010 | The legacy field still needs explicit mapping clarification before DDL freeze. |

## Largest Missing-Table Gaps

- `tbl_para_case`
- `tbl_para_status_history`
- `tbl_compliance_evidence`
- `tbl_para_settlement_history`
- `tbl_observation_dsa`
- `tbl_audit_plan`
- `tbl_iid_case_study`
- `tbl_iid_head_review`
- `tbl_iid_violation`
- `tbl_iid_proceeding`

## Main Findings

1. Current IAS_ZTBL SQL is not yet migration-complete for legacy para/compliance, IID child workflow, DSA, and working-paper areas.
2. A large share of legacy fields can already be staged into the right domain table, but not always into a fully defined target column.
3. The matrix should be treated as the authoritative punch list for Phase 3 DDL refinement.
