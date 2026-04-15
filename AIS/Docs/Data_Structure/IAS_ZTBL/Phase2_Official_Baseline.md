# IAS_ZTBL Phase 2 Official Baseline

This is the official Phase 2 baseline derived from:

- DBConnection usage analysis
- controller/API callsites
- active package/procedure usage
- legacy table/column metadata from `AIS\Docs\Data_Structure\Table Structure.xlsx`
- legacy text export from `AIS\Docs\Data_Structure\Table_Structure_for_Codex.txt`
- legacy package text export from `AIS\Docs\Data_Structure\Packages_for_Codex.txt`
- IAS_ZTBL target DDL from `AIS\Docs\Data_Structure\IAS_ZTBL\sql\ias_ztbl_schema.sql`

Companion text file:

- `AIS\Docs\Data_Structure\IAS_ZTBL\Phase2_Official_Baseline.txt`

## Core Deliverables

- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\procedure_replacement_matrix.csv`
- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\old_to_new_table_column_migration_matrix.csv`
- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\status_lookup_normalization_register.csv`
- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\history_strategy_register.csv`
- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\db_level_dependency_review_register.csv`

Supporting inventories:

- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\legacy_table_columns.csv`
- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\target_table_columns.csv`
- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\active_procedure_package_register.csv`
- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\active_object_register.csv`
- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\dbconnection_method_register.csv`

## Output Size

| Register | Rows |
| --- | ---: |
| Procedure replacement matrix | 748 |
| Old-to-new table/column migration matrix | 2411 |
| Status / lookup normalization register | 39 |
| History strategy register | 10 |
| DB-level dependency review register | 137 |

## What Phase 2 Proves

1. The redesign can no longer be treated as a naming cleanup only. The live system depends on a deep mix of package logic, controller orchestration, direct SQL, and file-system document paths.
2. The current IAS_ZTBL DDL is a valid starting point, but it is not yet migration-complete. The field matrix identifies:
   - `392` legacy fields with a direct landing column already present
   - `542` fields that fit an existing IAS_ZTBL table but still need target-column additions
   - `467` fields that require new target tables not yet present in `ias_ztbl_schema.sql`
   - `1010` fields that still need explicit human review before DDL freeze
3. The largest required-table gaps are concentrated in:
   - `tbl_para_case`
   - `tbl_para_status_history`
   - `tbl_compliance_evidence`
   - `tbl_para_settlement_history`
   - `tbl_observation_dsa`
   - `tbl_audit_plan`
   - IID child/history tables such as `tbl_iid_case_study`, `tbl_iid_head_review`, `tbl_iid_violation`, `tbl_iid_proceeding`
4. Legacy commercial-audit procedures in `PKG_CM` are active but should be retired after migration to the consolidated commercial-audit module.

## Immediate Implication for the Next Phase

The next phase should not begin with more narrative redesign. It should begin with:

1. DDL extension planning for the required missing target tables and columns identified by the migration matrix.
2. High-risk replacement planning for `split` and `redesign` procedures.
3. Explicit migration sequencing for para/compliance, IID child entities, FRPT report composition, and attachment/file storage.
