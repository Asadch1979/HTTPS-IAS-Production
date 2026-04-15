# Unused and Obsolete Object Review Register

This review is intentionally conservative.

Objects are not marked as unused only because a direct DBConnection call was not found. Classification also considered:

- package/procedure usage resolved from `Packages.xlsx`
- controller/API callsites
- direct SQL inside DBConnection methods
- obvious temporary, backup, and system-generated naming patterns

Full per-object reasoning is in:

- `AIS/Docs/Data_Structure/IAS_ZTBL/analysis/object_review_register.csv`
- `AIS/Docs/Data_Structure/IAS_ZTBL/analysis/object_review_register_exclusion_focus.csv`

## Classification Summary

| Status | Count | Treatment |
| --- | ---: | --- |
| Confirmed Active | 232 | Must be mapped into IAS_ZTBL core, history, or controlled archive. |
| Likely Active | 1 | Keep in scope until verified. |
| Low-Confidence / Needs Verification | 169 | Do not exclude yet. Requires view/trigger/report validation. |
| Likely Obsolete | 17 | Exclude from IAS_ZTBL core unless a hidden dependency is later proven. |
| Backup / Temp / Archive Only | 74 | Exclude from IAS_ZTBL operational schema. Preserve only outside the future core if legacy retention is needed. |

## Important Caution

Names are not enough.

Some `ARCHIVE` and `HISTORY` objects are still active:

- `AIS_T_AU_POST_COMPLIANCE_EVIDENCE_ARCHIVE`
- `AIS_T_AU_POST_COMPLIANCE_HISTORY`
- `T_AU_OLD_PARAS_POST_COMPLIANCE_HISTORY`
- `T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY`
- `T_IAS_VERSION_HISTORY`

These are not exclusion candidates.

## Confirmed Exclusion Candidates

### 1. Likely Obsolete

These objects have legacy/old naming and no active DBConnection or mapped procedure evidence.

| Object | Reason |
| --- | --- |
| `T_AU_OBSERVATION_OLD_PARAS_FAD` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_CAD_LOG` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_FAD_RESPONSIBILITY_ASSIGNED_IMP` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_FAD_UPDATE_LOG` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_FAD_UPDATE_STATUS` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_POST_COMPLIANCE` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_POST_COMPLIANCE_HEAD_REMARKS` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_RESPONSE` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_RESPONSE_EVIDENCES` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_RESPONSE_FAD` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_RESPONSE_FAD_AUDITOR_RESPONSE` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_RESPONSE_FAD_STATUS` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_RESPONSE_IMP_REMARKS` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_RESPONSE_SVP_AZ_REMARKS` | Legacy/old naming with no active code reference found. |
| `T_AU_OLD_PARAS_RESPONSE_TEXT` | Legacy/old naming with no active code reference found. |
| `T_FRPT_STAFF_SNAPSHOT_OLD` | Old snapshot table with no active code reference found. |
| `TEMPLATEPLACEHOLDERS` | Legacy notification/support table with no active code reference found. |

### 2. Backup, Temp, Archive-Only, and System Support Tables

These are exclusion candidates because naming and evidence both indicate non-operational support usage only.

Representative patterns:

- `TEMP_*`
- `*_BACKUP`
- `*_BAK`
- `*_BKP`
- `*_STG`
- `DR$*`
- `ERR$*`

Representative examples:

- `AIS_T_AU_POST_COMPLIANCE_BACKUP`
- `AIS_T_AU_POST_COMPLIANCE_BACKUP_23082024`
- `AIS_T_AU_POST_COMPLIANCE_HISTORY_BACKUP`
- `AIS_T_AU_POST_COMPLIANCE_TEXT_BACKUP`
- `T_AU_PLAN_ENG_BACKUP`
- `T_AU_SAMPLE_BKP`
- `T_AUDIT_EMP_BACKUP`
- `T_GROUP_RIGHTS_BACKUP`
- `T_MENU_PAGES_BKP`
- `T_USER_BAK`
- `T_AU_API_MASTER_STG`
- `ERR$_T_AU_RESPONSIBILITY_ASSIGNED_FINAL`
- `DR$IDX_TEXT$I`
- `DR$TEXT_INDEX$K`

Per-table reasons are in `object_review_register_exclusion_focus.csv`.

## Low-Confidence Objects That Must Not Be Dropped Yet

These are the main verification-risk objects. They have no confirmed active DBConnection/package evidence, but their names suggest they may still support reports, triggers, queues, attachments, or side flows:

- `AIS_T_AU_OBSERVATION`
- `AIS_T_AU_POST_COMPLIANCE_23_PARAS`
- `AIS_T_AU_POST_COMPLIANCE_FR`
- `AIS_T_AU_POST_COMPLIANCE_HISTORY_DIFF_AIS_COM`
- `AIS_T_AU_POST_COMPLIANCE_SETTELED_ON_DIFF`
- `ATTACHMENTSOURCES`
- `EMAILLOG`
- `EMAILTEMPLATES`
- `NOTIFICATIONEVENTS`
- `NOTIFICATIONRULES`
- `T_AU_EMAIL_QUEUE`
- `T_AU_EMAILS`
- `T_AU_EMAILS_INTERNAL`
- `T_APP_MAINTENANCE`
- `T_AU_FAD_REPORTS`
- `T_AU_IID_EXC_*` tables not directly proven through package-object mapping

Recommended treatment:

1. keep all `Low-Confidence` objects out of the exclusion list for now
2. validate them against database views, triggers, scheduler jobs, and report SQL before DDL freeze
3. only move them to exclusion after the hidden-dependency check is complete

## Exclusion Rules for IAS_ZTBL

Only these groups are currently safe to exclude from the future operational schema:

- `Likely Obsolete`
- `Backup / Temp / Archive Only`

These groups are not safe to exclude yet:

- `Confirmed Active`
- `Likely Active`
- `Low-Confidence / Needs Verification`

## What This Means for Redesign

1. The future-state schema should be cleaned aggressively for obvious backup/temp objects.
2. The redesign should not drop historical-sounding tables that are still written by live packages.
3. Notification, attachment, and reporting support tables need a second-pass dependency review before exclusion.
