# IAS DDL and Package Validation

## Result

The five proven post-baseline tables have the columns and Oracle data-type families required by the latest committed `AIS/Docs/sql/ALL_PACKAGES.sql`. No proven post-baseline view change was found.

| Object | Latest `ALL_PACKAGES.sql` use | DDL alignment | Result |
| --- | --- | --- | --- |
| `T_AU_SYSTEM_ERROR_MASTER` | Inserts, updates, filters, resolution state, email state | All referenced columns exist; text, numeric, CLOB, and timestamp uses match | Aligned |
| `T_AU_SYSTEM_ERROR_HISTORY` | Inserts occurrence details; returns occurrence history | All referenced columns exist and match package use | Aligned |
| `T_AU_SYSTEM_ERROR_STATUS_HISTORY` | Inserts reopen/resolve events; returns status history | All referenced columns exist and match package use | Aligned |
| `T_AU_SYSTEM_ERROR_RECIPIENTS` | Returns active recipients ordered by `SORT_ORDER`, `RECIPIENT_ID` | All referenced columns exist and match package use | Aligned |
| `T_AU_APPLICATION_AUDIT_LOG` | `P_LOG_APPLICATION_ACTIVITY` inserts all audit fields using the sequence | Every inserted column exists and the sequence is included | Aligned with correction noted below |

## Discrepancies

### 1. Audit result constraint omits `FAILURE`

The committed source DDL for `T_AU_APPLICATION_AUDIT_LOG` restricts `RESULT_STATUS` to `SUCCESS`, `VALIDATION`, `REJECTED`, and `INFORMATION`. The committed `ApplicationAuditAttribute` emits `FAILURE`, and at least one committed controller action enables failure logging. `ALL_PACKAGES.sql` passes `P_RESULT_STATUS` directly into the table and does not translate it.

The generated DDL includes `FAILURE` in `CK_APP_AUDIT_RESULT`. This is an application-inferred correction, clearly distinguished from the original explicit DDL.

### 2. Missing `PKG_LG.CLAIM_SYSTEM_ERROR_EMAIL` in `ALL_PACKAGES.sql`

`DBConnection.SystemError.cs` calls `PKG_LG.CLAIM_SYSTEM_ERROR_EMAIL`, and the standalone `PKG_LG.sql` declares that procedure. The latest `ALL_PACKAGES.sql` contains neither its declaration nor its body. The standalone `PKG_LG.sql` also has no body implementation in the compared commit.

The table DDL supports the intended claim state through `EMAIL_SENT = 'P'`, but package deployment from `ALL_PACKAGES.sql` will still fail at runtime with an identifier/procedure error when the application calls it. This is a package defect, not a missing table/view DDL item, so it is reported here and not patched in `IAS_Tables_Views_DDL.sql`.

### 3. Compliance view definitions cannot be proven changed

The compliance DDL snapshot added in `a7a002b` defines three views that already existed at baseline. `ALL_PACKAGES.sql` expects the columns shown below, and the snapshot exposes them:

| View | Package-required columns | Snapshot result |
| --- | --- | --- |
| `V_RPT_DB_P_COMPAINCE_SUMMARY_REGION_TOTAL` | `REGION_ID`, `REGION`, `GM_ID`, `TOTAL_PARA`, `TOTAL_COMP`, `AT_REPORTING`, `UNDER_CONSIDERATION`, `REJECTED`, `SETTLED` | Matches |
| `V_RPT_DB_P_COMPAINCE_DETAILS_BRANCH` | `REGION_ID`, `REGION`, `GM_ID`, `AUTID`, `TOTAL_PARA`, `TOTAL_COMP`, `AT_REPORTING`, `UNDER_CONSIDERATION`, `REJECTED`, `SETTLED` | Matches |
| `V_RPT_DB_P_COMPAINCE_DETAILS` | `REGION_ID`, `REGION`, `GM_ID`, `AUTID`, `TOTAL_PARA`, `TOTAL_COMP`, `AT_REPORTING`, `UNDER_CONSIDERATION`, `REJECTED`, `SETTLED` | Matches |

This confirms package compatibility with the snapshot but does not prove a post-baseline view modification. Obtain the baseline/live view text before treating these `CREATE OR REPLACE VIEW` statements as migration DDL.

## Execution and verification

For a database known to be at baseline `1491463`, execute `IAS_Tables_Views_DDL.sql` before compiling the latest packages. The script intentionally uses direct DDL and will stop on an existing-object error; that behavior prevents an unknown partially deployed database from being silently treated as aligned.

Before production use, compare `USER_TABLES`, `USER_TAB_COLUMNS`, `USER_CONSTRAINTS`, `USER_INDEXES`, and `USER_VIEWS` with this report, then resolve the `CLAIM_SYSTEM_ERROR_EMAIL` package discrepancy. No SQL was executed against production as part of this review.

