# DBConnection Business Logic and Flow Register

This register replaces table-name-only assumptions with code-driven evidence from the IAS application layer.

Scope reviewed:

- `AIS/DBConnection*.cs`
- `AIS/DBConnection.archive.cs`
- controller callsites that invoke `DBConnection` methods
- package/procedure references extracted from `Packages.xlsx`

Working baseline:

- 29 DBConnection-related files reviewed
- 997 extracted DBConnection methods
- 810 `Confirmed Active`
- 54 `Likely Active`
- 47 `Low-Confidence / Needs Verification`
- 86 `Internal Helper`
- 817 distinct procedure/package calls from DBConnection methods
- 738 procedure calls mapped back to package-body object usage

The full method-level register is in:

- `AIS/Docs/Data_Structure/IAS_ZTBL/analysis/dbconnection_method_register.csv`
- `AIS/Docs/Data_Structure/IAS_ZTBL/analysis/dbconnection_method_register_active.csv`
- `AIS/Docs/Data_Structure/IAS_ZTBL/analysis/dbconnection_module_summary.csv`
- `AIS/Docs/Data_Structure/IAS_ZTBL/analysis/controller_dbconnection_usage.csv`

## Evidence Status

| Status | Meaning |
| --- | --- |
| Confirmed Active | Method is referenced by a non-archive controller or API path. |
| Likely Active | Method has weaker live evidence, such as helper/view usage, but should still be treated carefully. |
| Low-Confidence / Needs Verification | No solid live callsite was found, but indirect use through views, reports, triggers, package internals, or parsing gaps is still possible. |
| Internal Helper | Non-public support method used by active DBConnection logic. |

## Cross-System Findings

1. `ApiCallsController.cs` is the central orchestration layer. It touches 622 DBConnection entry points and crosses almost every business module.
2. `PlanningController.cs`, `ExecutionController.cs`, `EngagementController.cs`, `PostComplianceController.cs`, `IIDController.cs`, `FieldAuditReportController.cs`, `AdministrationPanelController.cs`, `ReportsController.cs`, and `SetupController.cs` all drive live workflows.
3. `DBConnection.archive.cs` is not a separate persistence layer. It is a thin archive wrapper over existing live `DBConnection` methods and should not be treated as proof of a separate future-state schema.
4. Notification behavior is mostly assembled in C# from existing observation, engagement, IID, and report methods. Current notification storage is thin and partly indirect.
5. Some objects with `ARCHIVE` or `HISTORY` in the name are still active through package logic and cannot be dropped only on naming grounds.

## Module-Wise Live Business Flow

### 1. Security and Administration

Primary DBConnection files:

- `DBConnection.AD.cs`
- `DBConnection.AdminCatalog.cs`
- `DBConnection.LG.cs`
- `DBConnection.Security.cs`
- `DBConnection.Permission.cs`
- parts of `DBConnection.cs`

Main controllers:

- `AdministrationPanelController.cs`
- `ApiMasterController.cs`
- `CatalogController.cs`
- `DashboardLayoutController.cs`
- `HomeController.cs`
- `LoginController.cs`
- `SecurityController.cs`

Main packages/procedures:

- `PKG_AD`
- `PKG_LG`

Core live tables:

- `T_USER`
- `T_USER_MAPING`
- `T_GROUPS`
- `T_MENU`
- `T_MENU_PAGES`
- `T_MENU_PAGES_GROUPMAP`
- `T_AU_API_MASTER`
- `T_AU_PAGE_API_MAP`
- `T_AU_ROLE_API_PERMISSION`
- `T_USER_SESSION`
- `T_AU_ACTIVITY_LOG`
- `T_IAS_VERSION_HISTORY`

Observed business flow:

- user login, session creation, session kill, logout, and password change are live
- menu/page/API permission management is live
- role and group maintenance is live
- dashboard quick links and role dashboard page assignments are live
- activity log writing is still present through `P_add_activity_log`
- version history is a live admin feature, not a dead artifact

### 2. Entity Management and Reference Setup

Primary DBConnection files:

- `DBConnection.AD.cs`
- `DBConnection.cs`
- `DBConnection.FAD.cs`

Main controllers:

- `AdministrationPanelController.cs`
- `SetupController.cs`
- `ApiCallsController.cs`
- `IAMSController.cs`

Main packages/procedures:

- `PKG_AD`
- `PKG_AIS`
- `PKG_RPT`

Core live tables:

- `T_AUDITEE_ENTITIES`
- `T_AUDITEE_ENT_TYPES`
- `T_AUDITEE_ENTITIES_MAPING`
- `T_AUDITEE_ENTITIES_SIZE_DISC`
- `T_AUDITEE_ENTITIES_UPDATE_AZ`
- `T_RISK`
- `T_AUDIT_DEPARTMENTS`
- `T_PUBLIC_HOLIDAY`
- `T_MANUAL_MASTER`
- `T_MANUAL_SECTIONS`
- `T_MANUAL_CHAPTERS`
- `TBL_PARA_REFERENCE_LINKS` and related reference tables through `PKG_FAD`

Observed business flow:

- auditee entities, sub-entities, entity mappings, sizes, risk tags, GM/reporting line relationships, and compliance flow configuration are actively maintained
- branch and HR reference data are used in live setup and planning screens
- reference/manual/circular lookup is live in FAD and para-reference screens
- entity shifting and branch-to-Islamic conversion logic are active and affect downstream observation and compliance data

### 3. Planning and Engagement

Primary DBConnection files:

- `DBConnection.cs`
- `DBConnection.PG.cs`
- `DBConnection.AD.cs`
- `DBConnection.AR.cs`

Main controllers:

- `PlanningController.cs`
- `EngagementController.cs`
- `ApiCallsController.cs`

Main packages/procedures:

- `PKG_PG`
- `PKG_AD`
- `PKG_AR`

Core live tables:

- `T_AU_PERIOD`
- `T_AU_PLAN`
- `T_AU_PLAN_ENG`
- `T_AU_PLAN_ENG_LOG`
- `T_AU_AUDIT_TEAMS`
- `T_AU_TEAM_MEMBERS`
- `T_AU_AUDIT_TEAM_TASKLIST`
- `T_AUDIT_CRITERIA`
- `T_AUDIT_CRITERIA_LOG`
- `T_AUDIT_BUDGET`
- `T_AUDIT_EMP`

Observed business flow:

- audit periods are maintained and queried for planning screens
- tentative plans and engagement plans are created, updated, re-recommended, approved, referred back, and reversed
- audit team creation and task assignment are live
- joining report/task list behavior is tied to engagement execution
- post-change team/engagement reversal logic is active, so future design needs explicit engagement lifecycle history

### 4. Sampling and Exception Monitoring

Primary DBConnection files:

- `DBConnection.SM.cs`
- `DBConnection.IID.ExceptionReports.cs`
- parts of `DBConnection.cs`

Main controllers:

- `SamplingController.cs`
- `ApiCallsController.cs`

Main packages/procedures:

- `PKG_SM`
- `PKG_ISM`
- `PKG_AI`

Core live tables:

- `T_AU_SAMPLE`
- `T_AU_EXCEPTION_REPORT`
- package-driven source tables behind account, loan, document, and transaction sampling

Observed business flow:

- sampling is a live transactional module, not just a reporting helper
- sample creation, sample refresh, exception report creation, format maintenance, and regeneration are active
- IID exception report screens reuse ISM package calls, which means exception-monitoring structures cross module boundaries

### 5. Field Audit Execution and Observation Management

Primary DBConnection files:

- `DBConnection.AR.cs`
- `DBConnection.AD.cs`
- `DBConnection.ObservationPdf.cs`

Main controllers:

- `ExecutionController.cs`
- `FieldAuditController.cs`
- `ManagementAuditController.cs`
- `ObservationPdfController.cs`
- `UploadFileController.cs`
- `ApiCallsController.cs`

Main packages/procedures:

- `PKG_AR`
- `PKG_AD`

Core live tables:

- `T_AUDIT_CHECKLIST`
- `T_AUDIT_CHECKLIST_DETAILS`
- `T_AUDIT_CHECKLIST_SUB`
- `T_AUDIT_CHECKLIST_ANNEXURE`
- `T_AU_OBSERVATION`
- `T_AU_OBSERVATION_TEXT`
- `T_AU_OBSERVATION_STATUS`
- `T_AU_OBSERVATIONS_AUDITEE_RESPONSE`
- `T_AU_OBSERVATIONS_AUDITEE_EVIDENCES`
- `T_AU_REMARKS`
- responsibility and response tables reached through `PKG_AR`

Observed business flow:

- checklist, sub-checklist, annexure, and risk-linked execution are live
- observations are drafted, updated, submitted to auditee, responded to, replied to by auditors, and printed to PDF
- responsibility assignment is live and appears both for current observations and older paras
- DSA workflow is live and spans draft, submit, refer-back, and escalation steps

### 6. Compliance, Post Compliance, and Legacy Paras

Primary DBConnection files:

- `DBConnection.AE.cs`
- `DBConnection.FAD.cs`
- `DBConnection.HD.cs`
- parts of `DBConnection.AR.cs`

Main controllers:

- `PostComplianceController.cs`
- `FADController.cs`
- `HMController.cs`
- `ManagementAuditController.cs`
- `ApiCallsController.cs`

Main packages/procedures:

- `PKG_AE`
- `PKG_FAD`
- `PKG_HD`
- `PKG_AIS`

Core live tables:

- `AIS_T_AU_POST_COMPLIANCE`
- `AIS_T_AU_POST_COMPLIANCE_HISTORY`
- `AIS_T_AU_POST_COMPLIANCE_EVIDENCE`
- `AIS_T_AU_POST_COMPLIANCE_EVIDENCE_CAU`
- `AIS_T_AU_POST_COMPLIANCE_EVIDENCE_ARCHIVE`
- `T_AU_OLD_PARAS_FAD`
- `T_AU_OLD_PARAS_FAD_TEXT`
- `T_AU_OLD_PARAS_POST_COMPLIANCE_HISTORY`
- `T_AU_POST_COMPLIANCE_SETTLEMETMENT_HISTORY`
- `T_AU_OBSERVATION_OLD_CAD_PARAS`
- `T_AU_OBSERVATION_OLD_CAD_PARAS_TEXT`

Observed business flow:

- post-compliance submission, evidence upload, reviewer flow, and history tracking are live
- legacy paras are still part of the operational system, not just a historical import
- status-change and settlement workflows are active and need dedicated future-state history tables
- FAD monitoring, HM review, and settled/unsettled transitions remain business-critical

### 7. Reporting, Dashboards, and Final Report Composition

Primary DBConnection files:

- `DBConnection.DB.cs`
- `DBConnection.RPT.cs`
- `DBConnection.FRPT.cs`
- `DBConnection.FRPT.Pdf.cs`
- `DBConnection.MANRPT.cs`
- `DBConnection.BAC.cs`

Main controllers:

- `ReportsController.cs`
- `DashboardController.cs`
- `FieldAuditReportController.cs`
- `FieldAuditReportPdfController.cs`
- `MANReportController.cs`
- `MANReportPdfController.cs`
- `BACController.cs`

Main packages/procedures:

- `PKG_RPT`
- `PKG_DB`
- `PKG_FRPT`
- `PKG_BAC`

Core live tables:

- operational reads from `T_AU_PLAN_ENG`, `T_AU_OBSERVATION`, `T_AU_OLD_PARAS_FAD`, `AIS_T_AU_POST_COMPLIANCE`
- report composition/snapshot tables in the `T_FRPT_*` family
- `T_AU_ACTIVITY_LOG` for activity reporting

Observed business flow:

- dashboard packages still depend heavily on transactional tables, not just marts
- field audit report composition is a live subsystem with KPI, NPL, staff, narrative, statistics, and finalization states
- PDF access is gated through report-final and allowed-engagement checks
- management audit reporting is not dead; it reuses `PKG_FRPT`

### 8. IID and Inquiry

Primary DBConnection files:

- `DBConnection.IID.cs`
- `DBConnection.IID.Pdf.cs`
- `DBConnection.IID.ExceptionReports.cs`

Main controllers:

- `IIDController.cs`
- `IidInquiryReportPdfController.cs`
- `ApiCallsController.cs`

Main packages/procedures:

- `PKG_INQ`
- `PKG_ISM`

Core live tables:

- `T_AU_IID_COMPLAINT_HDR`
- complaint child tables under the `T_AU_IID_*` family
- findings/recommendation and FFR storage under `PKG_INQ`
- `T_AU_EMAIL_QUEUE` or package-owned email queue structures used by `PKG_INQ`

Observed business flow:

- complaint header creation, IAID save, FFR save, analysis, assessment, investigation plan, head review, evidence, statement, proceeding, accusation, accused, record, violation, report, and finalization paths are all live
- IID workflow mixes package calls with at least one direct SQL check on `T_AU_IID_COMPLAINT_HDR.is_finalized`
- inquiry email queue behavior exists through `PKG_INQ.P_ENQUEUE_EMAIL`, `P_MARK_EMAIL_SENT`, and `P_MARK_EMAIL_FAILED`

### 9. Commercial Audit and CAD

Primary DBConnection files:

- `DBConnection.CAU.cs`
- `DBConnection.CAD.cs`
- parts of `DBConnection.cs`

Main controllers:

- `CAUController.cs`
- `ApiCallsController.cs`
- `HMController.cs`

Main packages/procedures:

- `PKG_COMMERCIAL_AUDIT`
- `PKG_CM`
- `PKG_HD` for CAD request/reversal screens

Core live tables:

- `T_CAU_OM`
- `T_CAU_PDP`
- `T_CAU_ARPSE`
- DAC/PAC detail tables under commercial-audit package ownership
- CAU compliance/evidence structures reused from post-compliance module

Observed business flow:

- OM, PDP, ARPSE header, DAC entry, PAC entry, and OM-PDP mapping are active in the running system
- both old `PKG_CM` calls and new `PKG_COMMERCIAL_AUDIT` calls exist, so redesign must consolidate without losing any active branch
- CAD request/delete/reverse utilities are live and must be mapped into controlled workflow history in IAS_ZTBL

### 10. Notifications and Operational Support

Primary DBConnection files:

- `DBConnection.Notification.cs`
- `DBConnection.Logging.cs`

Main controllers:

- `ApiCallsController.cs`
- `FieldAuditReportController.cs`

Observed business flow:

- notification payloads are currently assembled in C# from active engagement, observation, IID, and FRPT data
- this is orchestration logic, not a full notification persistence model
- `Notification.cs` proves which live events matter today: task assignment, observation submission, inquiry assignment, and final report issuance
- `Logging.cs` shows DB-side activity logging is still expected by the application

## Mandatory Design Implications

1. `Legacy para` and `post compliance` data cannot be treated as archive-only. They are live workflows and must be redesigned as first-class modules.
2. `Field audit report` requires dedicated future-state tables, not only reporting views, because users actively save snapshots and narrative sections.
3. `IID` is a real transactional workflow with many child entities; future-state redesign must preserve complaint-stage granularity.
4. `Commercial audit` is active under both old and new package shapes; schema redesign must absorb both without carrying duplicate legacy names.
5. `Notification` should be centralized in IAS_ZTBL, but migration must preserve currently emitted events and any queue dependencies still owned by `PKG_INQ`.

## Review Output

Use the CSV appendices for method-by-method tracing. They already include:

- method name
- module/business area
- procedure calls
- input parameters
- output parameters
- cursor columns
- direct SQL objects
- package-derived read/write tables
- active controller evidence
- activity status

That appendix is the detailed method register requested for all reviewed DBConnection methods.
