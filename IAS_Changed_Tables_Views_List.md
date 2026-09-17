# IAS Changed Tables and Views

## Comparison boundary

- Baseline: `149146303c2e41c5be054d44ed7925df1fde954d` (`Entity Shifting & Email alerts`, 03 September 2026 15:32:25 +05:00).
- Latest production-ready commit: `bf650b33b58d87ad8688888211ce20d0a233eb39` (`Ready to production`, 17 September 2026 16:23:03 +05:00).
- Compared range: `149146303c2e41c5be054d44ed7925df1fde954d..bf650b33b58d87ad8688888211ce20d0a233eb39`. Changes already contained in the baseline commit are excluded.
- The previously generated root migration/review files were not treated as repository evidence.

## Definitive changed-object list

| Sr. | Object Name | Type | Change | Commit | Date | Evidence | Description |
| ---: | --- | --- | --- | --- | --- | --- | --- |
| 1 | `T_AU_SYSTEM_ERROR_MASTER` | TABLE | CREATED, then MODIFIED | `1b24d0d`, `73b7992` | 03 Sep 2026 | Explicit DDL | Central error fingerprint/current-state table. `EMAIL_SENT` was expanded from `Y/N` to `Y/N/P` for pending email claims. |
| 2 | `T_AU_SYSTEM_ERROR_HISTORY` | TABLE | CREATED | `1b24d0d` | 03 Sep 2026 | Explicit DDL | Stores each occurrence linked to the system-error master row. |
| 3 | `T_AU_SYSTEM_ERROR_RECIPIENTS` | TABLE | CREATED | `1b24d0d` | 03 Sep 2026 | Explicit DDL | Stores active `TO` and `CC` recipients for error notifications. |
| 4 | `T_AU_SYSTEM_ERROR_STATUS_HISTORY` | TABLE | CREATED | `9d06834` | 03 Sep 2026 | Explicit DDL | Stores error resolution/reopen transitions. |
| 5 | `T_AU_APPLICATION_AUDIT_LOG` | TABLE | CREATED | `4ee365b` | 04 Sep 2026 | Explicit DDL plus application inference | Stores application/business-action audit events. The generated DDL adds `FAILURE` to the committed status check because committed application code writes that value. |

There are **five proven changed tables and zero proven changed views** after the baseline.

## Direct dependencies

| Parent object | Dependency | Type | Originating commit |
| --- | --- | --- | --- |
| `T_AU_APPLICATION_AUDIT_LOG` | `SEQ_T_AU_APPLICATION_AUDIT_LOG` | SEQUENCE | `4ee365b` |
| `T_AU_APPLICATION_AUDIT_LOG` | `IX_APP_AUDIT_EVENT_TIME`, `IX_APP_AUDIT_PPNO`, `IX_APP_AUDIT_SESSION`, `IX_APP_AUDIT_ENTITY`, `IX_APP_AUDIT_ENGAGEMENT`, `IX_APP_AUDIT_PARA`, `IX_APP_AUDIT_COM`, `IX_APP_AUDIT_TRACE` | INDEXES | `4ee365b` |
| `T_AU_SYSTEM_ERROR_HISTORY` | `IX_AU_SYS_ERR_HIST_ERROR_ID` | INDEX | `1b24d0d` |
| `T_AU_SYSTEM_ERROR_STATUS_HISTORY` | `IX_AU_SYS_ERR_STATUS_ERROR_ID` | INDEX | `9d06834` |
| `T_AU_SYSTEM_ERROR_RECIPIENTS` | `UX_AU_SYS_ERR_RECIP_EMAIL` | UNIQUE INDEX | `1b24d0d` |

No grants for these five tables are present in their committed source DDL. Recipient seed rows and menu-page changes are DML and therefore are outside the requested table/view DDL deliverable.

## Explicit DDL versus inferred requirement

The five tables and their dependencies above have explicit post-baseline DDL. One correction is inferred from committed application behavior: `ApplicationAuditAttribute` sets `RESULT_STATUS` to `FAILURE` when a logged action fails, and `ApiCallsController` enables `LogFailures` on a committed action. The repository table script permits only `SUCCESS`, `VALIDATION`, `REJECTED`, and `INFORMATION`. The consolidated DDL therefore permits `FAILURE`; without it, those audit inserts can raise `ORA-02290`.

No other new column requirement was inferred from controllers, repository methods, or package code after comparing their table/column use with the baseline schema inventory and the explicit DDL above.

## Objects reviewed but excluded

Commit `a7a002b` added `AIS/Docs/Data_Structure/Complaince Summary.txt`, containing DDL for `AIS_T_AU_POST_COMPLIANCE` and these views:

- `V_RPT_DB_P_COMPAINCE_SUMMARY_REGION_TOTAL`
- `V_RPT_DB_P_COMPAINCE_DETAILS_BRANCH`
- `V_RPT_DB_P_COMPAINCE_DETAILS`

They are excluded from the changed-object list because all four objects already appear in the baseline schema inventory (`Tables.txt`), and the three views are already referenced by baseline package source. The repository has no pre-baseline view definitions with which to prove a definition change. The added file is therefore evidence of a DDL snapshot, not evidence that these production objects were created or modified after the baseline. Their live definitions should be compared with `DBMS_METADATA.GET_DDL` before any redeployment.

`T_AU_ENG_ENTITY_SHIFT_HIST` is also excluded: it was created by DDL already contained in the baseline commit itself. Later removal of that DDL from a package file did not modify the database object.

Commits `a86cd44` and `93f87ec` added/cleaned consolidated package text that happened to contain DDL fragments. Those edits did not establish new table or view changes and are not included.

