# IAS_ZTBL Database Redesign

This redesign was prepared from the legacy exports in:

- `AIS/Docs/Data_Structure/Table Structure.xlsx`
- `AIS/Docs/Data_Structure/Packages.xlsx`
- `AIS/Docs/Data_Structure/Table_Structure_for_Codex.txt`
- `AIS/Docs/Data_Structure/Packages_for_Codex.txt`

Revision note:

- A second-pass review based on actual application usage has now been completed from all DBConnection files, controller callsites, and mapped package usage.
- Before finalizing IAS_ZTBL scope, also use:
  - `AIS/Docs/Data_Structure/IAS_ZTBL/DBConnection_Business_Logic_Flow_Register.md`
  - `AIS/Docs/Data_Structure/IAS_ZTBL/Active_Usage_Register.md`
  - `AIS/Docs/Data_Structure/IAS_ZTBL/Unused_Object_Review_Register.md`
  - `AIS/Docs/Data_Structure/IAS_ZTBL/IAS_ZTBL_Redesign_Usage_Revision.md`
  - `AIS/Docs/Data_Structure/IAS_ZTBL/Phase2_Official_Baseline.md`
  - `AIS/Docs/Data_Structure/IAS_ZTBL/Procedure_Replacement_Matrix.md`
  - `AIS/Docs/Data_Structure/IAS_ZTBL/Table_Column_Migration_Matrix.md`
  - `AIS/Docs/Data_Structure/IAS_ZTBL/Status_Lookup_Normalization_Register.md`
  - `AIS/Docs/Data_Structure/IAS_ZTBL/History_Strategy_Register.md`
  - `AIS/Docs/Data_Structure/IAS_ZTBL/DB_Level_Dependency_Review.md`

The redesign intentionally treats `IAS_ZTBL` as a clean future-state schema. It does not preserve legacy naming noise, backup objects, temporary structures, or accidental module boundaries from `ZTBLAIS_PROD`.

## Part 1 - Existing Database Review

### 1.1 Legacy Inventory Snapshot

| Area | Observation |
| --- | --- |
| Legacy table inventory | 493 distinct table-like objects were exported in `Table Structure.xlsx`. |
| Legacy package inventory | 26 package families were exported in `Packages.xlsx`. |
| Constraint quality | The export shows 160 primary key occurrences versus only 61 foreign key occurrences, which is low for a workflow-heavy enterprise schema. |
| Temporary and backup noise | 57 `TEMP` objects, 8 `BACKUP` objects, 8 `HISTORY` objects, 34 `OLD` objects, 27 `LOG` objects, 1 `STG` object, Oracle Text `DR$` objects, and an `ERR$` table exist alongside live business tables. |
| Trigger usage | Trigger usage is narrow and inconsistent; the extracted table metadata only exposed `INSERT` triggers for selected tables such as `T_AU_API_MASTER`. |
| Naming | The current schema mixes `AIS_T_`, `T_`, `TEMP_`, `TBL_`, bare table names such as `EMAILTEMPLATES`, and system-generated names. |

### 1.2 Major Problems in the Current Design

1. Naming standards are not consistent.
   `T_USER`, `AIS_T_AU_POST_COMPLIANCE`, `EMAILTEMPLATES`, `ATTACHMENTSOURCES`, `TBL_PARA_REFERENCE_LINKS`, and `TEMP_*` all coexist in the same business domain.

2. Temporary, backup, abandoned, and reporting support objects are mixed into the operational schema.
   Examples include `TEMP_AIS_T_AU_POST_COMPLIANCE`, `T_AU_PLAN_ENG_BACKUP`, `T_GROUP_RIGHTS_BACKUP`, `T_FRPT_STAFF_SNAPSHOT_OLD`, `ERR$_T_AU_RESPONSIBILITY_ASSIGNED_FINAL`, and Oracle Text `DR$*` artifacts.

3. The schema has weak relational discipline.
   Many tables clearly hold relationships but do not expose strong named foreign keys. Workflow-heavy tables rely more on implicit IDs than on explicit relational rules.

4. Audit and lifecycle fields are inconsistent.
   The current model uses mixed patterns such as `ISACTIVE`, `ACTIVE`, `ACTIVE_FLAG`, `STATUS`, `STATUS_ID`, `CREATEDBY`, `ENTEREDBY`, `LASTUPDATEDBY`, `UPDATED_ON`, and module-specific variants.

5. Current and historical states are mixed in a non-uniform way.
   Some modules have separate history tables, others use log tables, others keep backup copies, and others overwrite the current row. This is especially visible in observations, old paras, post-compliance, and commercial audit.

6. Several tables are denormalized for convenience rather than clarity.
   Examples include text-heavy staging of team data in `T_AU_IID_INV_PLAN`, `PAGE_IDS` stored in `T_USER_GROUP_MAP`, repeated memo and para text structures, and repeated attachment metadata patterns across modules.

7. Package responsibilities overlap heavily.
   `PKG_AD`, `PKG_AIS`, `PKG_AR`, and `PKG_DB` cross security, entity maintenance, planning, checklist, observation, and dashboard concerns. This makes regression risk high and testing difficult.

8. Business typos and legacy shortcuts are now part of the persistent contract.
   Examples include `RESPONIBILITY`, `COMPLAINCE`, `STELLED_ON`, `ENTITIEES`, and multiple para/compliance spellings.

### 1.3 Legacy Module Grouping

| Module | Legacy Tables / Objects | Legacy Packages |
| --- | --- | --- |
| Security and administration | `T_USER`, `T_GROUPS`, `T_MENU`, `T_MENU_PAGES`, `T_USER_MAPING`, `T_USER_GROUP_MAP`, `T_AU_API_MASTER`, `T_AU_PAGE_API_MAP`, `T_AU_ROLE_API_PERMISSION`, `T_USER_SESSION` | `PKG_AD`, `PKG_LG`, `PKG_SEC`, `PKG_SYS`, `PKG_SYS_IAS` |
| Entity and reference management | `T_AUDITEE_ENT_TYPES`, `T_AUDITEE_ENTITIES`, `T_AUDITEE_ENTITIES_*`, `T_REFERENCE_MASTER`, checklist annexure tables | `PKG_AD`, `PKG_AIS`, `PKG_DB`, `PKG_MASTER-like logic spread across packages` |
| Planning and engagement | `T_AU_PERIOD`, `T_AU_PLAN`, `T_AU_PLAN_ENG`, `T_AU_TEAM_MEMBERS`, `T_AU_AUDIT_TEAM_TASKLIST`, `T_AUDIT_CRITERIA`, `T_AUDIT_CHECKLIST*`, `T_RISK*`, `T_AU_SAMPLE*` | `PKG_PG`, `PKG_AR`, `PKG_SM`, `PKG_AIS`, `PKG_DB` |
| Observation and execution | `T_AU_OBSERVATION`, `T_AU_OBSERVATION_TEXT`, `T_AU_OBSERVATION_RESPONIBILITY_ASSIGNED`, `T_AU_OBSERVATIONS_AUDITEE_RESPONSE`, `T_AU_OBSERVATION_REFERENCE_MAP`, `T_AU_REMARKS` | `PKG_AR`, `PKG_AIS`, `PKG_AE`, `PKG_HD`, `PKG_FAD` |
| Compliance and post-compliance | `AIS_T_AU_POST_COMPLIANCE*`, `T_AU_POST_COMPLIANCE_*`, `T_AU_OLD_PARAS_*`, para status change logs | `PKG_HD`, `PKG_FAD`, `PKG_AE`, `PKG_SBP_COM` |
| Inquiry and IID | `T_AU_IID_COMPLAINT_HDR`, `T_AU_IID_COMPLAINANT`, `T_AU_IID_INV_PLAN`, `T_AU_IID_REPORT`, `T_AU_IID_INQ_*`, `T_AU_IID_EXC_*`, `T_INS_COMPLAINTS*` | `PKG_INQ`, `PKG_IID_EXC`, `PKG_AI` |
| Commercial audit | `T_CAU_*`, `T_COM_AUDIT_*`, legacy CAU response tables | `PKG_CM`, `PKG_COMMERCIAL_AUDIT` |
| Reporting | `T_FRPT_*`, `T_AUDIT_REPORTS`, reporting support views/tables | `PKG_RPT`, `PKG_FRPT` |
| Notifications and operational support | `T_AU_EMAIL_QUEUE`, `EMAILTEMPLATES`, `ATTACHMENTSOURCES`, `NOTIFICATIONEVENTS`, `NOTIFICATIONRULES`, `T_APP_MAINTENANCE`, log tables | `PKG_EMAIL`, `PKG_INQ`, `PKG_SYS` |

### 1.4 Specific Redesign Opportunities

| Legacy Pattern | Redesign Opportunity |
| --- | --- |
| `T_USER`, `T_USER_MAPING`, `T_GROUPS`, `T_USER_GROUP_MAP`, `T_GROUP_RIGHTS` | Separate users, roles, permissions, pages, APIs, and scopes into explicit normalized security tables. |
| `T_AU_PLAN`, `T_AU_PLAN_ENG`, `T_AU_TEAM_MEMBERS`, `T_AU_AUDIT_TEAM_TASKLIST` | Use clear plan, engagement, engagement member, and engagement task tables with explicit status lookups. |
| `T_AU_OBSERVATION*` plus `AIS_T_AU_POST_COMPLIANCE*` | Separate observation lifecycle from compliance lifecycle while preserving a one-to-many observation-to-compliance relationship. |
| `T_AU_OLD_PARAS_*` | Treat old paras as migrated historical observations or compliance cases, not as a parallel permanent schema. |
| `T_AU_IID_INV_PLAN` storing team information as text | Normalize IID plans and plan activities; assign users by FK rather than comma-separated or text fields. |
| `T_AU_EMAIL_QUEUE` plus standalone email configuration tables | Introduce a full notification model with event, template, rule, queue, and generic attachment linkage. |
| `PKG_AD` and `PKG_AIS` mixing unrelated concerns | Replace with smaller, module-bound packages with consistent parameter and result patterns. |

## Part 2 - Proposed Module-wise Data Architecture

### 2.1 Core Design Rules for IAS_ZTBL

- All base tables use prefix `tbl_`.
- All views use prefix `vw_`.
- All sequences use prefix `seq_`.
- All triggers use prefix `trg_`.
- Each base table uses one numeric primary key named `<table_name>_id`.
- Status, severity, stage, and type controls use `tbl_lookup_type` and `tbl_lookup_value` unless the domain is a dedicated master table.
- Transaction tables use standard audit fields: `is_active`, `created_by`, `created_on`, `modified_by`, `modified_on`, `record_version`.
- Approval-driven tables additionally use `approved_by` and `approved_on`.
- Attachments and workflow events are centralized rather than re-modeled in every module.

### 2.2 Proposed Modules and Tables

#### Security and Administration

| Table | Category | Purpose |
| --- | --- | --- |
| `tbl_role` | Master | Role catalogue for enterprise and module roles. |
| `tbl_application_page` | Master | Pages / screens exposed to UI and page-level permission control. |
| `tbl_api_endpoint` | Master | API routes and HTTP methods. |
| `tbl_permission` | Master | Permission definitions linked to page, API, or action-level access. |
| `tbl_user` | Master | User account, PP number, authentication, and base organizational identity. |
| `tbl_user_role` | Bridge | User-to-role assignment with primary role support. |
| `tbl_role_permission` | Bridge | Role-to-permission assignment. |
| `tbl_user_scope` | Bridge | Entity-wise scope restrictions for data access. |
| `tbl_user_session` | Transaction | Optional DB-side session / token audit trail. |

#### Master and Reference Data

| Table | Category | Purpose |
| --- | --- | --- |
| `tbl_lookup_type` | Master | Master list categories. |
| `tbl_lookup_value` | Lookup | Controlled values for status, severity, stage, type, and flags. |
| `tbl_entity_type` | Master | Entity type hierarchy and audit ownership flags. |
| `tbl_entity` | Master | Auditable entities, branches, departments, units, and support entities. |
| `tbl_entity_relation` | Bridge | Reporting, controlling, and audit hierarchy relations with effective dates. |
| `tbl_reference_document` | Master | Manuals, circulars, SOPs, policies, and other reference documents. |
| `tbl_reference_document_version` | History-like master | Version history for reference documents. |
| `tbl_checklist_section` | Master | Checklist headings / sections. |
| `tbl_checklist_item` | Master | Checklist detail items used in fieldwork. |
| `tbl_audit_period` | Master | Controlled audit periods and year windows. |

#### Planning and Engagement Management

| Table | Category | Purpose |
| --- | --- | --- |
| `tbl_plan_criteria` | Master / approval | Risk, size, frequency, and duration rules for planning. |
| `tbl_engagement` | Transaction | Approved audit engagement against entity and period. |
| `tbl_engagement_member` | Detail | Team members assigned to an engagement. |
| `tbl_engagement_task` | Detail | Worklist and assignment tasks for field execution. |
| `tbl_engagement_checklist` | Detail | Checklist item usage and status inside an engagement. |

#### Field Audit Execution and Observations

| Table | Category | Purpose |
| --- | --- | --- |
| `tbl_observation` | Transaction | Core observation / para record. |
| `tbl_observation_detail` | Detail | Gist, narrative, memo headings, and rich text fragments. |
| `tbl_observation_assignment` | Detail | Responsibility assignments for observation handling. |
| `tbl_observation_response` | Detail | Audittee / reviewer responses across stages. |
| `tbl_observation_recommendation` | Detail | Auditor recommendations and final recommendation text. |
| `tbl_observation_reference` | Bridge | Links observations to manuals, circulars, and annexures. |

#### Compliance and Post-Compliance

| Table | Category | Purpose |
| --- | --- | --- |
| `tbl_compliance_case` | Transaction | Post-compliance case opened against an observation. |
| `tbl_compliance_action` | Detail | Action items and compliance commitments. |
| `tbl_compliance_review` | Detail | Review decisions and settlement assessments. |

#### Inquiry / IID Workflow

| Table | Category | Purpose |
| --- | --- | --- |
| `tbl_iid_case` | Transaction | Complaint or inquiry case header. |
| `tbl_iid_complainant` | Detail | Complainant party records. |
| `tbl_iid_subject` | Detail | Accused / subject persons or entities. |
| `tbl_iid_investigation_plan` | Transaction | Inquiry plan, scope, risk, and approval lifecycle. |
| `tbl_iid_plan_activity` | Detail | Planned activity steps for IID execution. |
| `tbl_iid_evidence` | Detail | Evidence records; files linked through centralized attachment tables. |
| `tbl_iid_statement` | Detail | Witness / accused / complainant statements. |
| `tbl_iid_finding` | Detail | Findings, allegations, and recommendations. |
| `tbl_iid_report` | Transaction | Submitted IID report versions. |
| `tbl_iid_exception_report` | Master / transaction | IID exception report definition used for sourced account / loan data. |
| `tbl_iid_exception_column` | Detail | Metadata for exception report columns. |
| `tbl_iid_exception_item` | Detail | Exception account / loan item rows. |
| `tbl_iid_exception_item_txn` | Detail | Transactions linked to an exception item. |

#### Commercial Audit Workflow

| Table | Category | Purpose |
| --- | --- | --- |
| `tbl_commercial_om` | Transaction | OM records. |
| `tbl_commercial_pdp` | Transaction | PDP records. |
| `tbl_commercial_pdp_observation` | Bridge | PDP-to-OM linkage. |
| `tbl_commercial_arpse` | Transaction | ARPSE paras. |
| `tbl_commercial_arpse_resolution` | Detail | DAC / PAC / management resolution entries. |

#### Reporting, Notifications, Attachments, and Audit Trail

| Table | Category | Purpose |
| --- | --- | --- |
| `tbl_report` | Transaction | Final report header and approval record. |
| `tbl_report_section` | Detail | Report narrative sections. |
| `tbl_report_snapshot` | Detail | KPI, NPL, staffing, and other snapshot values. |
| `tbl_notification_event` | Master | Event definitions for queueing notifications. |
| `tbl_notification_template` | Master | Event templates by culture. |
| `tbl_notification_rule` | Master | Recipient and attachment rules. |
| `tbl_notification_queue` | Transaction | Queued outbound notifications. |
| `tbl_attachment` | Transactional master | File metadata store. |
| `tbl_attachment_link` | Bridge | Generic links from attachments to source entities. |
| `tbl_workflow_event` | History / audit | Central workflow and status change event log. |

## Part 3 - Old-to-New Mapping

### 3.1 Security and Administration Mapping

| Old Object | Proposed New Object | Purpose | Target Type | Recommendation |
| --- | --- | --- | --- | --- |
| `T_USER` | `tbl_user` | User account and identity | Master | Keep and redesign |
| `T_GROUPS` | `tbl_role` | Role definition | Master | Keep and rename |
| `T_USER_MAPING` | `tbl_user_role` | User-role assignment | Bridge | Keep and normalize |
| `T_GROUP_RIGHTS` | `tbl_role_permission` | Role permission map | Bridge | Keep and normalize |
| `T_MENU`, `T_MENU_SUB`, `T_MENU_PAGES` | `tbl_application_page` | UI navigation and page registry | Master | Merge |
| `T_AU_API_MASTER` | `tbl_api_endpoint` | API registry | Master | Keep and rename |
| `T_AU_PAGE_API_MAP` | `tbl_permission` plus `tbl_role_permission` | Page/API permission model | Master + Bridge | Split |
| `T_AU_ROLE_API_PERMISSION` | `tbl_role_permission` | Role-to-API permission | Bridge | Merge |
| `T_USER_GROUP_MAP` | `tbl_role_permission` and `tbl_permission` | Mixed menu/page grant table | Bridge | Split |
| `T_USER_SESSION` | `tbl_user_session` | Session audit | Transaction | Keep and rename |
| `T_GROUP_RIGHTS_BACKUP`, `T_USER_BAK` | None | Backups | Archive only | Drop from redesign |

### 3.2 Entity, Reference, and Planning Mapping

| Old Object | Proposed New Object | Purpose | Target Type | Recommendation |
| --- | --- | --- | --- | --- |
| `T_AUDITEE_ENT_TYPES` | `tbl_entity_type` | Entity type master | Master | Keep and rename |
| `T_AUDITEE_ENTITIES` | `tbl_entity` | Entity master | Master | Keep and redesign |
| `T_AUDITEE_ENTITIES_MAPING*` | `tbl_entity_relation` | Reporting and controlling hierarchy | Bridge | Merge |
| `T_AUDITEE_ENTITIES_ADDRESS` | `tbl_entity` | Address attributes | Master | Merge |
| `T_AUDITEE_ENTITIES_RISK`, `T_RISK*` | `tbl_lookup_value`, `tbl_plan_criteria`, `tbl_entity` | Risk classification | Lookup + Master | Merge / rationalize |
| `T_AUDITEE_ENTITIES_SIZE*` | `tbl_lookup_value`, `tbl_plan_criteria`, `tbl_entity` | Size classification | Lookup + Master | Merge / rationalize |
| `T_AU_PERIOD` | `tbl_audit_period` | Audit period | Master | Keep and rename |
| `T_AUDIT_CRITERIA`, `T_AUDIT_CRITERIA_APPROVED`, `T_AUDIT_CRITERIA_LOG` | `tbl_plan_criteria` plus `tbl_workflow_event` | Planning criteria and approval trail | Master / History | Merge |
| `T_AU_PLAN` | `tbl_plan_criteria` and `tbl_engagement` | Plan header to engagement pipeline | Master / Transaction | Split |
| `T_AU_PLAN_ENG` | `tbl_engagement` | Engagement header | Transaction | Keep and redesign |
| `T_AU_TEAM_MEMBERS` | `tbl_engagement_member` | Engagement members | Detail | Keep and rename |
| `T_AU_AUDIT_TEAM_TASKLIST` | `tbl_engagement_task` | Member task list | Detail | Keep and rename |
| `T_AUDIT_CHECKLIST*` | `tbl_checklist_section`, `tbl_checklist_item`, `tbl_engagement_checklist` | Checklist library and usage | Master + Detail | Split and normalize |
| `T_AU_SAMPLE*`, `T_SAMPLE_*` | `tbl_engagement_task`, `tbl_attachment_link`, optional future sampling submodule | Sampling support | Detail | Rationalize, not core redesign v1 |
| `TEMP_*` planning tables | None | Temporary support | Archive only | Drop |

### 3.3 Observation and Compliance Mapping

| Old Object | Proposed New Object | Purpose | Target Type | Recommendation |
| --- | --- | --- | --- | --- |
| `T_AU_OBSERVATION` | `tbl_observation` | Core observation record | Transaction | Keep and redesign |
| `T_AU_OBSERVATION_TEXT` | `tbl_observation_detail` | Observation narrative | Detail | Merge into structured detail model |
| `T_AU_OBSERVATION_GIST` | `tbl_observation_detail` | Observation gist | Detail | Merge |
| `T_AU_OBSERVATION_RESPONIBILITY_ASSIGNED`, `T_AU_OBSERVATION_ASSIGNEDTO` | `tbl_observation_assignment` | Responsibility ownership | Detail | Merge |
| `T_AU_OBSERVATIONS_AUDITEE_RESPONSE`, `T_AU_OBSERVATIONS_AUDITOR_REPLY`, `T_AU_OBSERVATIONS_AUDITOR_RESPONSE` | `tbl_observation_response` | Response lifecycle | Detail | Merge |
| `T_AU_OBSERVATION_FINAL_RECCOMENDATION`, `T_AU_OBSERVATIONS_AUDITOR_RECOMMENDATION` | `tbl_observation_recommendation` | Recommendation lifecycle | Detail | Merge |
| `T_AU_OBSERVATION_REFERENCE_MAP`, `T_OBSERVATION_REFERENCE_LINK`, `TBL_PARA_REFERENCE_LINKS` | `tbl_observation_reference` | Reference mapping | Bridge | Merge |
| `T_AU_OBSERVATION_EVIDENCES`, `T_AU_OBS_AUDITEE_REPLY_EVIDENCE`, `T_AU_OBSERVATIONS_AUDITEE_EVIDENCES*` | `tbl_attachment`, `tbl_attachment_link` | Evidence storage | Bridge | Centralize |
| `AIS_T_AU_POST_COMPLIANCE` | `tbl_compliance_case` | Post-compliance header | Transaction | Keep and redesign |
| `AIS_T_AU_POST_COMPLIANCE_HISTORY`, `T_AU_POST_COMPLIANCE_FLOW`, `T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY` | `tbl_compliance_review` plus `tbl_workflow_event` | Compliance stage and settlement history | Detail + History | Merge |
| `AIS_T_AU_POST_COMPLIANCE_TEXT*`, `T_AU_POST_COMPLIANCE_FAD_PARA_TEXT*` | `tbl_compliance_action` and `tbl_attachment_link` | Compliance commentary and detail | Detail | Merge |
| `AIS_T_AU_POST_COMPLIANCE_EVIDENCE*` | `tbl_attachment`, `tbl_attachment_link` | Compliance evidence | Bridge | Centralize |
| `T_AU_OLD_PARAS_*`, `T_AU_OBSERVATION_OLD_*` | `tbl_observation` / `tbl_compliance_case` migrated with source-type metadata | Historical paras | Transaction / History | Migrate and retire parallel tables |
| `AIS_T_*_BACKUP`, `TEMP_AIS_*`, old log copies | None | Backup / temp noise | Archive only | Drop from redesign |

### 3.4 IID / Inquiry Mapping

| Old Object | Proposed New Object | Purpose | Target Type | Recommendation |
| --- | --- | --- | --- | --- |
| `T_AU_IID_COMPLAINT_HDR` | `tbl_iid_case` | Complaint / case header | Transaction | Keep and redesign |
| `T_AU_IID_COMPLAINANT` | `tbl_iid_complainant` | Complainants | Detail | Keep and rename |
| `T_AU_IID_COMPLAINT_IAID`, `T_INS_COMPLAINTANT_INFO` | `tbl_iid_case`, `tbl_iid_complainant`, `tbl_iid_subject` | Complaint details | Transaction + Detail | Split |
| `T_AU_IID_INV_PLAN` | `tbl_iid_investigation_plan` | Investigation plan | Transaction | Keep and redesign |
| `T_AU_IID_INV_PLAN_ACT` | `tbl_iid_plan_activity` | Plan activity detail | Detail | Keep and rename |
| `T_AU_IID_INQ_STATEMENTS` | `tbl_iid_statement` | Statements | Detail | Keep and rename |
| `T_AU_IID_INQ_PROCEEDINGS` | `tbl_iid_report` / `tbl_workflow_event` | Proceedings narrative | Detail / History | Merge |
| `T_AU_IID_INQ_ACCUSATIONS`, `T_AU_IID_INQ_ACCUSED_LIST` | `tbl_iid_subject`, `tbl_iid_finding` | Subjects and allegations | Detail | Split |
| `T_AU_IID_INQ_VIOLATIONS`, `T_AU_IID_INQ_FIND_RECOMM` | `tbl_iid_finding` | Findings / violations / recommendations | Detail | Merge |
| `T_AU_IID_INQ_EVIDENCE_FILES`, `T_AU_IID_INQ_EVIDENCE_STEP` | `tbl_iid_evidence`, `tbl_attachment_link` | Evidence chain | Detail + Bridge | Merge |
| `T_AU_IID_REPORT` | `tbl_iid_report` | Report header and text | Transaction | Keep and redesign |
| `T_AU_IID_STATUS_MST` | `tbl_lookup_value` | IID status list | Lookup | Merge |
| `T_AU_IID_EXC_REPORT_MST` | `tbl_iid_exception_report` | Exception report header | Master / Transaction | Keep and rename |
| `T_AU_IID_EXC_REPORT_COL_MST` | `tbl_iid_exception_column` | Exception report columns | Detail | Keep and rename |
| `T_AU_IID_EXC_ACCOUNT`, `T_AU_IID_EXC_LOAN` | `tbl_iid_exception_item` | Exception items | Detail | Merge |
| `T_AU_IID_EXC_ACCOUNT_TXN`, `T_AU_IID_EXC_LOAN_TXN` | `tbl_iid_exception_item_txn` | Exception transactions | Detail | Merge |
| `T_AU_IID_EXC_*_DOC*` | `tbl_attachment`, `tbl_attachment_link` | IID documents | Bridge | Centralize |

### 3.5 Commercial Audit Mapping

| Old Object | Proposed New Object | Purpose | Target Type | Recommendation |
| --- | --- | --- | --- | --- |
| `T_COM_AUDIT_OM`, `T_CAU_OM` | `tbl_commercial_om` | OM register | Transaction | Merge, keep cleaner `T_COM_*` semantics |
| `T_COM_AUDIT_PDP`, `T_CAU_PDP` | `tbl_commercial_pdp` | PDP register | Transaction | Merge |
| `T_COM_AUDIT_PDP_OM_MAP`, `T_CAU_PARA_MAPPING` | `tbl_commercial_pdp_observation` | PDP to OM mapping | Bridge | Merge |
| `T_COM_AUDIT_ARPSE`, `T_CAU_ARPSE` | `tbl_commercial_arpse` | ARPSE register | Transaction | Merge |
| `T_COM_AUDIT_ARPSE_DAC`, `T_COM_AUDIT_ARPSE_PAC`, `T_CAU_DAC_RECOMMENDATION`, `T_CAU_PAC_RECOMMENDATION` | `tbl_commercial_arpse_resolution` | Resolution entries | Detail | Merge |
| `*_HIST` tables | `tbl_workflow_event` and optional dedicated archive tables | History | History | Replace with consistent lifecycle log |

### 3.6 Reporting, Notification, and Support Mapping

| Old Object | Proposed New Object | Purpose | Target Type | Recommendation |
| --- | --- | --- | --- | --- |
| `T_FRPT_REPORT_META` | `tbl_report` | Final report header | Transaction | Keep and rename |
| `T_FRPT_PARA_NARRATIVE`, `T_FRPT_TEXT_BLOCKS` | `tbl_report_section` | Narrative sections | Detail | Merge |
| `T_FRPT_KPI_SNAPSHOT`, `T_FRPT_NPL_SNAPSHOT`, `T_FRPT_STAFF_SNAPSHOT` | `tbl_report_snapshot` | Snapshot metrics | Detail | Merge |
| `T_AU_EMAIL_QUEUE` | `tbl_notification_queue` | Outbound queue | Transaction | Keep and rename |
| `NOTIFICATIONEVENTS` | `tbl_notification_event` | Event master | Master | Keep and rename |
| `EMAILTEMPLATES` | `tbl_notification_template` | Template master | Master | Keep and rename |
| `NOTIFICATIONRULES` | `tbl_notification_rule` | Recipient rules | Master | Keep and rename |
| `ATTACHMENTSOURCES` | `tbl_notification_rule` plus `tbl_attachment_link` | Attachment sourcing | Master / Bridge | Split |
| `T_AU_ACTIVITY_LOG`, `T_SYS_LOG`, module log tables | `tbl_workflow_event` | Central event log | History | Merge |
| `T_APP_MAINTENANCE` | Separate app-operational schema or retained support table outside IAS_ZTBL core | Runtime maintenance flag | Support | Keep outside core redesign |

## Part 4 - DDL Script

The physical DDL baseline is provided in:

- `AIS/Docs/Data_Structure/IAS_ZTBL/sql/ias_ztbl_schema.sql`

Key implementation decisions in that script:

1. The script assumes the `IAS_ZTBL` schema already exists or will be created by a DBA.
2. Controlled statuses, stages, severities, and type codes are modeled through `tbl_lookup_type` and `tbl_lookup_value`.
3. Sequences and before-insert triggers are generated consistently for every `tbl_%` primary key through a metadata-driven block.
4. Attachments and workflow history are centralized through `tbl_attachment`, `tbl_attachment_link`, and `tbl_workflow_event`.
5. Reporting support views are included with the required `vw_` naming convention.

## Part 5 - Package Design

The package draft set is provided in:

- `AIS/Docs/Data_Structure/IAS_ZTBL/sql/ias_ztbl_packages.sql`

### 5.1 Proposed Package Catalogue

| Package | Responsibility |
| --- | --- |
| `pkg_sec` | Users, roles, permissions, page / API access, scoped security grants |
| `pkg_master` | Lookup values, entity types, entities, references, and reusable master data maintenance |
| `pkg_planning` | Plan criteria, engagements, engagement members |
| `pkg_execution` | Task list and engagement checklist operations |
| `pkg_observation` | Observation header, text detail, assignment, response, and recommendation lifecycle |
| `pkg_compliance` | Post-compliance case, action, and review workflow |
| `pkg_inquiry` | IID case intake, complainants, plans, evidence, findings, and report submission |
| `pkg_commercial_audit` | OM, PDP, ARPSE, and resolution maintenance |
| `pkg_report` | Final report header, sections, and snapshots |
| `pkg_notify` | Notification event, template, rule, and queue operations |

### 5.2 Package Standards

- All DML procedures use explicit verb-based names such as `save_*`, `assign_*`, `queue_*`, `close_*`.
- All list operations use `SYS_REFCURSOR`.
- All write procedures return `o_result_code` and `o_result_message`.
- Each module writes meaningful rows to `tbl_workflow_event` so history is queryable without scraping many log tables.
- Packages do not mix unrelated domains. For example, entity maintenance is not embedded in the observation package, and IID email queuing is not mixed with complaint header maintenance.

## Part 6 - Notes for Data Migration

### 6.1 Migration Strategy

1. Create `IAS_ZTBL` side by side with the current production schema.
2. Seed lookup types and controlled values first.
3. Migrate entity and security masters.
4. Migrate planning and checklist masters.
5. Migrate engagement records.
6. Migrate active observations and then post-compliance cases.
7. Migrate IID / inquiry data.
8. Migrate commercial audit data.
9. Migrate reporting support data only after transactional modules reconcile.
10. Run dual-run verification until the application shifts to the new schema or API layer.

### 6.2 Transformation Hotspots

| Legacy Issue | Migration Rule Needed |
| --- | --- |
| `T_USER`, `T_GROUPS`, `T_USER_MAPING`, `T_GROUP_RIGHTS`, `T_USER_GROUP_MAP` | Consolidate into users, roles, permissions, user-role, and role-permission sets. |
| `T_MENU`, `T_MENU_SUB`, `T_MENU_PAGES` | Normalize into one page registry with sort order and optional menu grouping. |
| `AIS_T_AU_POST_COMPLIANCE` containing both old and new para references | Split by `observation_id` linkage and assign `compliance_cycle_no`. |
| `T_AU_OLD_PARAS_*` parallel structures | Migrate into observation / compliance tables with a `source_system_code` or `legacy_type_id`. |
| `TEAM_LEAD`, `TEAM_MEMBERS` text in IID plan | Resolve to `tbl_user` references where possible; otherwise stage unresolved identifiers for manual mapping. |
| Repeated evidence tables | Migrate files to `tbl_attachment`, then create `tbl_attachment_link` rows by module and source key. |
| Mixed status columns (`STATUS`, `STATUS_ID`, `COM_STAGE`, `COM_STATUS`, `ACTIVE_FLAG`) | Map into controlled lookup IDs and explicit workflow events. |
| Package-generated IDs and trigger assumptions | Repoint calling code to the new `seq_` and `trg_` naming pattern or use package return IDs consistently. |
| Temporary / backup tables | Exclude from migration except when they contain data explicitly approved for recovery. |

### 6.3 Recommended Migration Controls

- Prepare table-by-table reconciliation counts for every major module.
- Preserve legacy IDs in staging or cross-reference tables during transition.
- Validate every workflow status map before cutover.
- Reconcile attachments separately because file systems and blob pointers are usually inconsistent in legacy environments.
- Freeze package changes in the legacy schema during final migration rehearsal.

## Summary

`IAS_ZTBL` should become a stable enterprise schema with:

- clear table naming,
- explicit lifecycle design,
- centralized attachments and workflow history,
- normalized security and permission handling,
- cleaner IID and commercial audit workflows,
- and module-bound package ownership.

That gives the IAS platform a realistic path away from legacy object sprawl without losing business meaning.
