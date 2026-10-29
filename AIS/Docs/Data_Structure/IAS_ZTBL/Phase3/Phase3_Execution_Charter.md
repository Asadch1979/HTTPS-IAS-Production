# IAS_ZTBL Phase 3 Execution Charter

## A. Phase 3 Goal

Phase 3 converts the approved Phase 2 baseline into an execution-ready delivery set for controlled IAS_ZTBL implementation.

This phase is for:

- DDL gap closure
- executable future-state SQL preparation
- migration script preparation
- package replacement preparation
- controlled implementation sequencing

## B. In-Scope Work

Phase 3 includes the following deliverables and work areas:

- final target schema structure for approved modules in IAS_ZTBL
- closure of major `RequiredTable` gaps identified in the Phase 2 migration matrix
- closure of major `ExistingTableRequiredColumn` gaps for approved modules
- implementation of normalized lookup and status domains
- implementation of current-state, history, workflow-event, and migration-support tables
- executable package specs and package bodies aligned to approved module boundaries
- first-pass migration scripts for active business domains
- post-deploy validation and reconciliation query pack
- file execution order and dependency notes

## C. Out of Scope for This Phase

The following items are explicitly outside Phase 3:

- production cutover
- destructive cleanup of low-confidence legacy objects
- retirement of all legacy procedures/packages in production
- full automated migration validation against live Oracle data
- production deployment runbook and release approvals
- DBA-only production hardening tasks outside the IAS_ZTBL schema boundary

## D. Module-Wise Execution Order

The controlled implementation order for Phase 3 is:

1. Security and administration
2. Entity and reference backbone
3. Planning and engagement
4. Execution and observation
5. Compliance and para history
6. IID and inquiry child structures
7. Commercial audit
8. FRPT / report composition
9. Notifications, documents, migration-support, and low-confidence support areas

## E. DDL Gap Closure Summary

Phase 2 proved that the earlier IAS_ZTBL draft was not yet DDL-complete. Phase 3 closes the major approved gaps by adding:

- migration-support tables:
  - `tbl_migration_batch`
  - `tbl_legacy_key_map`
  - `tbl_migration_issue`
  - `tbl_document_migration_queue`
- para/compliance structures:
  - `tbl_para_case`
  - `tbl_para_case_text`
  - `tbl_para_assignment`
  - `tbl_para_status_history`
  - `tbl_para_settlement_history`
  - `tbl_compliance_evidence`
  - `tbl_compliance_case_history`
- observation and execution gaps:
  - `tbl_observation_evidence`
  - `tbl_observation_dsa`
  - `tbl_working_paper`
  - gap-closing columns on `tbl_observation_assignment`, `tbl_observation_response`, `tbl_observation_reference`, and `tbl_workflow_event`
- planning gaps:
  - `tbl_audit_plan`
  - `tbl_engagement_team`
  - `tbl_engagement_criteria_history`
- reference and checklist gaps:
  - `tbl_checklist_sub_item`
  - `tbl_checklist_annexure`
  - manual-index support columns on `tbl_reference_document_version`
- entity backbone gaps:
  - `tbl_entity_risk_profile`
  - `tbl_entity_size_profile`
- IID child/history gaps:
  - `tbl_iid_record`
  - `tbl_iid_analysis`
  - `tbl_iid_assessment`
  - `tbl_iid_plan_approval`
  - `tbl_iid_head_review`
  - `tbl_iid_case_study`
  - `tbl_iid_violation`
  - `tbl_iid_proceeding`
  - `tbl_iid_workflow_history`
  - gap-closing columns on `tbl_iid_subject`, `tbl_iid_statement`, `tbl_iid_investigation_plan`, and `tbl_iid_report`
- FRPT/report composition gaps:
  - gap-closing columns on `tbl_report_section` and `tbl_report_snapshot`
- commercial-audit gaps:
  - gap-closing columns on `tbl_commercial_arpse_resolution`
- operational support gaps:
  - `tbl_activity_log`
  - `tbl_release_version_history`

## F. Package Implementation Strategy

Phase 3 package delivery follows the approved Phase 2 replacement matrix.

Legacy package action by family:

- `PKG_AD`: split into `pkg_sec`, `pkg_entity`, `pkg_reference`, `pkg_master`
- `PKG_AIS`: split and redesign across `pkg_master`, `pkg_planning`, `pkg_observation`, `pkg_reference`
- `PKG_AR`: split and redesign across `pkg_planning`, `pkg_execution`, `pkg_observation`
- `PKG_AE`: redesign into `pkg_execution`, `pkg_observation`, `pkg_compliance`
- `PKG_HD`: split and redesign into `pkg_execution`, `pkg_observation`, `pkg_compliance`
- `PKG_FAD`: redesign into `pkg_execution`, `pkg_observation`, `pkg_compliance`, `pkg_report`
- `PKG_PG`: redesign into `pkg_planning`
- `PKG_INQ`: redesign into `pkg_inquiry`
- `PKG_IID_EXC`: wrap temporarily, then redesign into `pkg_inquiry`
- `PKG_COMMERCIAL_AUDIT`: redesign into `pkg_commercial_audit`
- `PKG_CM`: retire later after migration cutover and wrapper period
- `PKG_RPT`: keep temporarily for compatibility reporting extracts; redesign target is `pkg_report`
- `PKG_FRPT`: wrap read-side temporarily, redesign write-side into `pkg_report`
- `PKG_DB`, `PKG_BAC`: keep temporarily where used only for read support
- `PKG_EMAIL` and `PKG_AIS_EMAIL`: split into `pkg_notify` and `pkg_document`; retire duplicated flows later

Future-state package set delivered in Phase 3:

- `pkg_sec`
- `pkg_master`
- `pkg_entity`
- `pkg_reference`
- `pkg_planning`
- `pkg_execution`
- `pkg_observation`
- `pkg_compliance`
- `pkg_inquiry`
- `pkg_commercial_audit`
- `pkg_report`
- `pkg_notify`
- `pkg_document`

## G. Migration Strategy Summary

Migration is side-by-side only. Legacy production tables are read-only inputs.

Migration approach by data class:

- master tables: migrate first and preserve source/target linkage in `tbl_legacy_key_map`
- transactional tables: migrate after parent masters and use generated IAS_ZTBL surrogate keys
- history tables: migrate separately from current-state rows and preserve actor/timeline fields
- attachments and documents: migrate metadata to `tbl_attachment` and queue file copies in `tbl_document_migration_queue`
- notifications and email queues: migrate templates, rules, and queue evidence separately
- low-confidence support objects: do not drop; route unresolved values into `tbl_migration_issue`

## H. Risks and Blockers

Phase 3 keeps the following risks visible:

- direct SQL outside package calls still exists in the live codebase
- file-system based documents and evidences are not fully governed by database tables
- trigger-driven behavior exists in legacy modules and may hide sequencing logic
- active legacy views are still part of read-side behavior
- scheduler/job usage could not be fully confirmed without live DBA metadata
- several field mappings remain `ReviewRequired` and therefore route to migration issue capture
- some legacy commercial and IID support structures remain broad or partially duplicated

## I. Acceptance Criteria

Phase 3 can be treated as complete when all of the following are true:

- all required target tables for approved modules are defined in executable SQL
- major required target columns identified in Phase 2 are added for approved modules
- package specs and bodies compile in dependency order under IAS_ZTBL
- migration scripts are separated from DDL scripts and sequenced clearly
- validation and reconciliation queries are available
- no approved-module `RequiredTable` gap remains open
- no low-confidence object is dropped or retired in this phase without being preserved, wrapped, or routed to review
- unresolved mappings are explicitly captured in `tbl_migration_issue` or `tbl_document_migration_queue`
