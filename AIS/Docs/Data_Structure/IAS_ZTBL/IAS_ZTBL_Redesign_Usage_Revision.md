# IAS_ZTBL Revised Redesign Recommendation Based on Actual Code Usage

This document refines the first IAS_ZTBL redesign using the DBConnection/business-flow review.

Reference inputs:

- `AIS/Docs/Data_Structure/IAS_ZTBL/DBConnection_Business_Logic_Flow_Register.md`
- `AIS/Docs/Data_Structure/IAS_ZTBL/Active_Usage_Register.md`
- `AIS/Docs/Data_Structure/IAS_ZTBL/Unused_Object_Review_Register.md`
- `AIS/Docs/Data_Structure/Table_Structure_for_Codex.txt`
- `AIS/Docs/Data_Structure/Packages_for_Codex.txt`

## 1. What Changed From the First Redesign

The original redesign was driven mainly by exported table and package metadata.

The code-driven review changes the redesign in four important ways:

1. `Legacy para` and `post compliance` are confirmed live operational modules, not just historical baggage.
2. `Field audit reporting` is an active write-path with snapshots, text blocks, statistics, and finalization state. It needs dedicated IAS_ZTBL tables.
3. `IID` and `Commercial Audit` are not peripheral modules. Both are active and deserve clean first-class module boundaries.
4. `ArchiveController` does not justify carrying archive tables into the future core. It is mostly a presentation wrapper over live methods.

## 2. Refined IAS_ZTBL Module Boundaries

### Security and Administration

Future package/module direction:

- `pkg_sec`
- `pkg_admin`

Future table groups:

- `tbl_user`
- `tbl_role`
- `tbl_permission`
- `tbl_user_role`
- `tbl_role_permission`
- `tbl_application_page`
- `tbl_api_endpoint`
- `tbl_user_session`
- `tbl_activity_log`
- `tbl_release_version_history`

### Entity and Reference Management

Future package/module direction:

- `pkg_master`
- `pkg_entity`
- `pkg_reference`

Future table groups:

- `tbl_entity_type`
- `tbl_entity`
- `tbl_entity_relation`
- `tbl_entity_mapping`
- `tbl_entity_risk_profile`
- `tbl_entity_size_profile`
- `tbl_reference_document`
- `tbl_reference_document_version`
- `tbl_reference_link`

### Planning and Engagement

Future package/module direction:

- `pkg_planning`

Future table groups:

- `tbl_audit_period`
- `tbl_audit_plan`
- `tbl_engagement`
- `tbl_engagement_status_history`
- `tbl_engagement_team`
- `tbl_engagement_team_member`
- `tbl_engagement_task`
- `tbl_engagement_criteria`
- `tbl_engagement_criteria_history`
- `tbl_engagement_budget`

### Execution and Observation

Future package/module direction:

- `pkg_execution`
- `pkg_observation`

Future table groups:

- `tbl_checklist`
- `tbl_checklist_item`
- `tbl_checklist_sub_item`
- `tbl_checklist_annexure`
- `tbl_observation`
- `tbl_observation_text`
- `tbl_observation_status_history`
- `tbl_observation_assignment`
- `tbl_observation_response`
- `tbl_observation_evidence`
- `tbl_observation_reply`
- `tbl_working_paper`

### Compliance and Legacy Paras

Future package/module direction:

- `pkg_compliance`

Future table groups:

- `tbl_compliance_case`
- `tbl_compliance_case_history`
- `tbl_compliance_action`
- `tbl_compliance_evidence`
- `tbl_compliance_review`
- `tbl_para_case`
- `tbl_para_case_text`
- `tbl_para_assignment`
- `tbl_para_status_change_request`
- `tbl_para_status_history`
- `tbl_para_settlement_history`

Key revision:

- do not bury legacy paras inside archive strategy
- redesign them as a normalized para/compliance domain with explicit current-state and history tables

### IID and Inquiry

Future package/module direction:

- `pkg_iid`

Future table groups:

- `tbl_iid_complaint`
- `tbl_iid_complaint_party`
- `tbl_iid_assessment`
- `tbl_iid_analysis`
- `tbl_iid_investigation_plan`
- `tbl_iid_head_review`
- `tbl_iid_evidence_file`
- `tbl_iid_statement`
- `tbl_iid_proceeding`
- `tbl_iid_record`
- `tbl_iid_violation`
- `tbl_iid_finding`
- `tbl_iid_recommendation`
- `tbl_iid_report`
- `tbl_iid_workflow_history`

### Commercial Audit

Future package/module direction:

- `pkg_commercial_audit`

Future table groups:

- `tbl_commercial_audit_om`
- `tbl_commercial_audit_pdp`
- `tbl_commercial_audit_pdp_link`
- `tbl_commercial_audit_arpse`
- `tbl_commercial_audit_arpse_dac`
- `tbl_commercial_audit_arpse_pac`
- `tbl_commercial_audit_workflow_history`

### Reporting and Report Composition

Future package/module direction:

- `pkg_report`

Future table groups:

- `tbl_report_run`
- `tbl_field_audit_report`
- `tbl_field_audit_report_text`
- `tbl_field_audit_report_statistic`
- `tbl_field_audit_report_kpi_snapshot`
- `tbl_field_audit_report_npl_snapshot`
- `tbl_field_audit_report_staff_snapshot`
- `tbl_management_audit_report_text`

### Notifications and Attachments

Future package/module direction:

- `pkg_notify`
- `pkg_document`

Future table groups:

- `tbl_notification_event`
- `tbl_notification_template`
- `tbl_notification_rule`
- `tbl_notification_queue`
- `tbl_document`
- `tbl_document_link`

Key revision:

- current notification logic is partly C# orchestration and partly package queue logic
- IAS_ZTBL should centralize this instead of leaving it spread across ad hoc tables and package internals

## 3. Business-Critical Legacy Tables That Must Be Migrated

These are confirmed live and should be mapped into IAS_ZTBL even if the names change:

- `T_USER`, `T_USER_MAPING`, `T_GROUPS`, `T_MENU`, `T_MENU_PAGES`, `T_AU_API_MASTER`
- `T_AUDITEE_ENTITIES`, `T_AUDITEE_ENT_TYPES`, `T_AUDITEE_ENTITIES_MAPING`, `T_RISK`
- `T_AU_PERIOD`, `T_AU_PLAN`, `T_AU_PLAN_ENG`, `T_AU_AUDIT_TEAMS`, `T_AU_TEAM_MEMBERS`, `T_AU_AUDIT_TEAM_TASKLIST`, `T_AUDIT_CRITERIA`
- `T_AUDIT_CHECKLIST`, `T_AUDIT_CHECKLIST_DETAILS`, `T_AUDIT_CHECKLIST_SUB`, `T_AUDIT_CHECKLIST_ANNEXURE`
- `T_AU_OBSERVATION`, `T_AU_OBSERVATION_TEXT`, `T_AU_OBSERVATIONS_AUDITEE_RESPONSE`
- `AIS_T_AU_POST_COMPLIANCE`, `AIS_T_AU_POST_COMPLIANCE_HISTORY`, `AIS_T_AU_POST_COMPLIANCE_EVIDENCE`, `AIS_T_AU_POST_COMPLIANCE_EVIDENCE_CAU`
- `T_AU_OLD_PARAS_FAD`, `T_AU_OLD_PARAS_FAD_TEXT`, `T_AU_OLD_PARAS_POST_COMPLIANCE_HISTORY`, `T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY`
- `T_AU_IID_COMPLAINT_HDR` and active `T_AU_IID_*` children
- `T_CAU_OM`, `T_CAU_PDP`, `T_CAU_ARPSE` and active commercial-audit children
- `T_FRPT_*` tables that support report composition/finalization

## 4. Objects That Should Not Be Carried Forward Into IAS_ZTBL Core

Do not carry these categories into the future operational schema:

- `TEMP_*`
- `*_BACKUP`
- `*_BAK`
- `*_BKP`
- `*_STG`
- `DR$*`
- `ERR$*`
- tables in the `Likely Obsolete` list from `Unused_Object_Review_Register.md`

Important exception:

- some `ARCHIVE` and `HISTORY` tables are still active, so only the reviewed exclusion list should be dropped

## 5. Package Redesign Changes Required

The package redesign should now follow actual live boundaries instead of legacy package sprawl:

- move security/login/session behavior out of `PKG_AD` and `PKG_LG` overlap into `pkg_sec`
- move entity/reference maintenance out of mixed `PKG_AD` procedures into `pkg_entity` and `pkg_reference`
- keep planning/engagement approval and reversal history together in `pkg_planning`
- split execution from observation/compliance so that evidence, replies, and submission history are explicit
- redesign `PKG_INQ` into a clean complaint-to-report workflow package family instead of many flat procedures
- merge old `PKG_CM` and new `PKG_COMMERCIAL_AUDIT` responsibilities into one future-state commercial-audit module
- retain `pkg_report` as a write-capable reporting-composition package, not only a read/report utility

## 6. Migration and Scope Freeze Guidance

### Safe to freeze as in-scope now

- security/admin
- entity management
- planning/engagement
- execution/observation
- compliance/post-compliance
- IID/inquiry
- commercial audit
- field audit report composition

### Not safe to freeze as out-of-scope yet

- low-confidence notification tables
- low-confidence attachment linkage tables
- low-confidence reporting support tables
- low-confidence IID exception-support tables

### Recommended migration sequence

1. migrate security, entity, and engagement backbone keys first
2. migrate observations and checklist structures
3. migrate compliance and legacy-para chains with full history preservation
4. migrate IID complaint structures
5. migrate commercial-audit structures
6. migrate FRPT/report-composition data
7. backfill notifications, attachments, and low-confidence support objects after dependency verification

## 7. Bottom-Line Recommendation

IAS_ZTBL should proceed as a clean redesign, but the redesign boundary must now be based on confirmed live usage:

- keep all live operational domains
- collapse duplicate legacy structures into normalized future-state modules
- remove obvious backup/temp noise
- do not drop low-confidence support objects until database-level dependency review is finished
