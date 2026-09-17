# IAS DB Change Review - 03 Sep 2026 Onward

## Reviewed Range

- Range reviewed: `149146303c2e41c5be054d44ed7925df1fde954d^..HEAD`
- Included commits dated 03 Sep 2026 through 17 Sep 2026.
- Current local package files were also checked where they represent the latest `ALL_PACKAGES.sql`/`PKG_LG.sql` state.

## Required Execution Order

1. Run `IAS_DB_Migration_03Sep2026.sql`.
2. Run the latest package deployment scripts, especially `PKG_LG.sql`, `PKG_AD.sql`, `PKG_AE.sql`, `PKG_AR.sql`, `PKG_HD.sql`, `PKG_PG.sql`, `PKG_DB.sql`, `PKG_BAC.sql`, and `PKG_FAD.sql`.
3. Run `IAS_ALL_PACKAGES_Alignment.sql` if deploying from `ALL_PACKAGES.sql` or any package source that does not already implement `PKG_LG.CLAIM_SYSTEM_ERROR_EMAIL`.
4. Compile invalid objects and inspect `USER_ERRORS`.

## Commit-Wise Findings

| Date | Commit | DB-relevant finding |
| --- | --- | --- |
| 03 Sep 2026 | `1491463` Entity Shifting & Email alerts | `PKG_AD.P_SHIFT_ENGAGEMENT_ENTITY` depends on `T_AU_ENG_ENTITY_SHIFT_HIST` and `SEQ_AU_ENG_ENTITY_SHIFT_HIST`. The DDL was later removed from `PKG_AD.sql`; it is now included in the migration script. |
| 03 Sep 2026 | `1b24d0d`, `9d06834`, `73b7992`, `b0c374c`, `e3965d9` system error/login monitoring | Introduced application-side centralized error logging and IP/context capture. Requires `T_AU_SYSTEM_ERROR_MASTER`, `T_AU_SYSTEM_ERROR_HISTORY`, `T_AU_SYSTEM_ERROR_STATUS_HISTORY`, `T_AU_SYSTEM_ERROR_RECIPIENTS`, indexes, and `EMAIL_SENT` support for `P` pending state. |
| 04 Sep 2026 | `a7a002b`, `a86cd44`, `60dbc01`, `b5a322d`, `4ee365b`, `49fdecf` package consolidation/phase work | Added `ALL_PACKAGES.sql` and package updates. The latest `ALL_PACKAGES.sql` contains system error/audit package routines but is missing `CLAIM_SYSTEM_ERROR_EMAIL`; the alignment script patches that contract. |
| 05-07 Sep 2026 | `606a98c`, `e541d82`, `270a79d`, `a3119c1`, `fae0752`, `6197ab5` UAT and phase fixes | Package/query changes were reviewed. No additional new table/view DDL was found beyond the application audit, system error, and engagement-shift prerequisites. |
| 08-10 Sep 2026 | `7bf98b5`, `29ab92c`, `a7576d1`, `708b3c7` controller/package/security hardening | Adds audit attributes and stored-procedure contract hardening. Requires `T_AU_APPLICATION_AUDIT_LOG` and `PKG_LG.P_LOG_APPLICATION_ACTIVITY`. |
| 16-17 Sep 2026 | `93f87ec`, `4c80cf6`, `a509a78`, `d130eb4`, `fb7ffc2`, `fbcfa74`, `3fa977d`, `6717628` dashboards/draft observations/auditee workflow | Adds and repairs package calls for auditee response, post-compliance snapshot, finalized draft observations, and shiftable engagements. Current package files and `ALL_PACKAGES.sql` contain those routines; no new table/view DDL was identified. |

## Consolidated Structural Changes

- `T_AU_APPLICATION_AUDIT_LOG`
  - Required by `DBConnection.ApplicationAudit.cs` and `PKG_LG.P_LOG_APPLICATION_ACTIVITY`.
  - Includes sequence `SEQ_T_AU_APPLICATION_AUDIT_LOG` and audit indexes.
  - `CK_APP_AUDIT_RESULT` is aligned to allow `FAILURE`, because `ApplicationAuditAttribute` can emit that status when `LogFailures = true`.

- `T_AU_SYSTEM_ERROR_MASTER`
  - Required by `DBConnection.SystemError.cs` and `PKG_LG.REGISTER_SYSTEM_ERROR`, `MARK_SYSTEM_ERROR_EMAIL`, `CLAIM_SYSTEM_ERROR_EMAIL`, `GET_SYSTEM_ERRORS`, `GET_SYSTEM_ERROR_DETAIL`, and `RESOLVE_SYSTEM_ERROR`.
  - `EMAIL_SENT` must allow `Y`, `N`, and `P`; `P` is used as the email-claim/pending state.

- `T_AU_SYSTEM_ERROR_HISTORY`
  - Required by `REGISTER_SYSTEM_ERROR` and `GET_SYSTEM_ERROR_DETAIL`.

- `T_AU_SYSTEM_ERROR_STATUS_HISTORY`
  - Required by `REGISTER_SYSTEM_ERROR`, `RESOLVE_SYSTEM_ERROR`, and detail retrieval.

- `T_AU_SYSTEM_ERROR_RECIPIENTS`
  - Required by `GET_SYSTEM_ERROR_RECIPIENTS`.

- `T_AU_ENG_ENTITY_SHIFT_HIST`
  - Required by `PKG_AD.P_SHIFT_ENGAGEMENT_ENTITY`.
  - The package now inserts only the core row-count columns, but the migration preserves the fuller original table definition to remain compatible with older scripts.

## ALL_PACKAGES Alignment

- `PKG_LG.CLAIM_SYSTEM_ERROR_EMAIL`
  - Application call: `DBConnection.SystemError.cs` calls `PKG_LG.CLAIM_SYSTEM_ERROR_EMAIL(P_ERROR_ID, O_CLAIMED)`.
  - `PKG_LG.sql` declares the procedure but does not include the body implementation.
  - Latest `ALL_PACKAGES.sql` does not declare or implement it.
  - `IAS_ALL_PACKAGES_Alignment.sql` inserts the missing declaration/body into deployed `PKG_LG` source through `USER_SOURCE`, preserving unrelated package logic.

## Existing Objects Checked But Not Recreated

- `T_AU_API_MASTER`, `T_AU_ROLE_API_PERMISSION`, `T_ROLE_DASHBOARD_PAGES`, `T_IAS_VERSION_HISTORY`, and `T_APP_MAINTENANCE` appear in the existing data-structure exports and were not duplicated.
- `T_AU_FAD_ANNEXURE_CONFIG` and entity-shifting menu registration scripts predate 03 Sep 2026; they remain prerequisites only if the target database has not received those earlier migrations.
- No new view definitions introduced in the reviewed commit range required inclusion in the consolidated migration script.

## Deployment Notes

- Both SQL scripts are designed to be re-runnable and avoid data-destructive operations.
- Constraint replacement is limited to check constraints that block current application values.
- The migration script intentionally does not assume that existing repository SQL files have already been deployed.
