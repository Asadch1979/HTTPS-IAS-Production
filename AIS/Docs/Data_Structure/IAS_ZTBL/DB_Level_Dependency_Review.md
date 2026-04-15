# DB-Level Dependency Review

Machine-readable register:

- `AIS\Docs\Data_Structure\IAS_ZTBL\analysis\db_level_dependency_review_register.csv`

## Dependency Coverage

| Dependency Type | Count |
| --- | ---: |
| Trigger | 15 |
| View | 93 |
| PackageInternal | 21 |
| ScheduledJob | 1 |
| DirectSQL | 3 |
| FilePath | 4 |

## Main Findings

1. Trigger metadata proves that some legacy sequencing/business-key behavior still exists outside DBConnection method names.
2. Active package procedures still depend on many legacy views, so those views must be validated before object exclusion.
3. No `DBMS_SCHEDULER` or `DBMS_JOB` references were found in the available package workbook source, but that is not the same as proving there are no live scheduler jobs in the database.
4. Direct SQL still exists in live code for manual/reference tables, FRPT row-existence checks, and IID finalization checks.
5. File/document handling is not purely database-driven. The running system still reads and deletes files directly from `wwwroot` subfolders for audit reports and evidence.

## Required Next-Step Use

Use this register before excluding any low-confidence notification, attachment, reporting, or support object from IAS_ZTBL.
