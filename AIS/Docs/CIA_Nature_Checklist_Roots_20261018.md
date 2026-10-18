# CIA Nature of Issue from verified IAS checklist roots

**Status:** development source only. No live Oracle statement was executed and
no permission, application page, API signature or production object changed.

## Business source

Approved IAS audit-period ID 5 showed:

| T_AUDIT_CHECKLIST.T_ID | Reported heading | CATEGORY_CODE | All observation count |
|--:|---|---|--:|
| 1 | KYC/AML/ CFT | AML_CFT | 507 |
| 5 | LOAN AND ADVANCES | LOAN_ADVANCES | 6059 |
| 6 | CASH MANAGEMENT | CASH_MANAGEMENT | 1951 |
| 8 | ACCOUNTING/REPORTING/RECONCILIATION | ACCOUNTING_REPORTING_RECON | 1539 |
| 9 | RECORD MANAGEMENT | RECORD_MANAGEMENT | 1147 |
| 14 | MANAGEMENT AUDIT | MANAGEMENT_AUDIT | 661 |
| 15 | BANKING OPERATIONS | BANKING_OPERATIONS | 2246 |
| 16 | BRANCH GENERAL CONTROLS | BRANCH_GENERAL_CONTROLS | 805 |

Total in supplied broad query: **14,915**. That query does not apply
`V_EXEC_KPI_LIFECYCLE.ISSUE_ELIGIBLE` or the audit-period Start/End dates.
It will not necessarily equal the earlier **14,604** CIA eligible issues.

`ACCOUNTS` and `IS_AUDIT` remain legitimate dashboard categories but
no `T_ID` for either was present in these verified rows. They are left
unmapped/zero until a distinct approved IAS source mapping is supplied.
**Do not relabel T_ID=14 as IS_AUDIT** or infer category from the annexure.

## Source and behavior changes

- `V_IAS_CIA_ISSUE_CHECKLIST_ROOT.sql`: one dimensional row per
  modern `T_AU_OBSERVATION.ID` via
  `CHECKLISTDETAIL_ID -> T_AUDIT_CHECKLIST_DETAILS.ID ->
  T_AUDIT_CHECKLIST_SUB.S_ID -> T_AUDIT_CHECKLIST.T_ID`.
  LEFT JOINs preserve missing-checklist rows for unmapped diagnostics.
- `T_IAS_CIA_ISSUE_NATURE_MAP.sql`: fresh-schema DDL now supports
  `SOURCE_TYPE='CHECKLIST_ROOT'`. It does **not** change allowed
  `CATEGORY_CODE` values or disable constraints.
- `cia_issue_nature_checklist_root_migration.sql`: development-only,
  rerunnable migration for schemas with the old source-type constraint.
  It first verifies *all eight actual master headings* and aborts if
  any approved existing mapping conflicts/is inactive. A replacement
  source check is added **before** dropping the old one. Then it inserts
  only missing approved mappings. It does not overwrite existing mappings.
  Oracle DDL commits implicitly; DBA review is required.
- `PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE`: the checklist-root
  mapping wins before legacy `VIOLATION_SUB`, `ANNEX` or
  `AUDIT_PORTFOLIO` fallbacks. Only that procedure body changes.
  No changes to procedure signature, ten cursor categories, selected
  `T_AU_PERIOD`, `ISSUE_ELIGIBLE`, authorized org filters, ten-row
  output, `UNMAPPED_ISSUES` or `MAPPING_STATUS`.
- `V_EXEC_KPI_LIFECYCLE`, `V_EXEC_PARA_BASE`,
  `V_EXEC_REPORT_SOURCE`, `PKG_HD`, Audit Para Snap and Reports
  A-E **remain unchanged**.

## Development compilation and verification order

1. Confirm IAS master T_ID/HEADING rows and backup DDL.
2. Only when the mapping table is **missing**, run
   `T_IAS_CIA_ISSUE_NATURE_MAP.sql` once. It contains CREATE TABLE.
   Do not rerun it if the table already exists.
3. For an existing table, run
   `cia_issue_nature_checklist_root_migration.sql` as a **script**
   (SQL Developer F5/SQL*Plus), after DBA review. It can also seed a
   fresh table that already permits CHECKLIST_ROOT. Do not use invalid
   display headings as CATEGORY_CODE. The existing check
   `CK_IAS_CIA_NATURE_CATEGORY` remains unchanged.
4. Compile `V_IAS_CIA_ISSUE_CHECKLIST_ROOT.sql` (F8). Verify
   `COUNT(*) = COUNT(DISTINCT observation_id)` to rule out multiplicity.
5. Compile the **package BODY** part of
   `PKG_EXEC_DASHBOARD.sql` independently with F8. Package specification
   need not change. Avoid replacing unrelated earlier views unnecessarily.
6. Run `verify_cia_issue_nature_checklist_roots.sql` in read-only
   steps, using valid IAS audit-period ID 5 (or another selected ID).
   Reconcile `SUM(ISSUE_COUNT)+UNMAPPED_ISSUES=TOTAL_ISSUES`.
   Unexpected unmapped issues are not dropped. Check 10 rows always,
   the two unsupported categories are still present, and package is VALID.
7. Refresh the authenticated CIA Command Center after a successful
   development compile and mapping. No .NET modifications are needed
   for the existing named cursor fields.

## Outstanding QA

No remote Oracle connection is available. Confirm actual
`CHECKLIST_DETAILS.ID`, `CHECKLIST_SUB.S_ID`, and
`CHECKLIST.T_ID` cardinality; the supplied query grounds these
relationships but does not itself prove uniqueness of every master.
Investigate incomplete `CHECKLISTDETAIL_ID` values and existing
non-checklist mappings if `UNMAPPED_ISSUES` remains nonzero.
The current procedure intentionally retains a legacy fallback when
an observation lacks an approved checklist-root mapping.
