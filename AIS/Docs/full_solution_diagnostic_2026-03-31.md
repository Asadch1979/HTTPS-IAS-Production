# IAS Full Solution Diagnostic Review

Date: 2026-03-31

## Scope

This review covered the complete MVC solution structure with emphasis on:

- full-page MVC views
- dashboard-hosted partials
- views loaded through AJAX
- view to controller/action mappings
- local JS/CSS/image dependencies
- layout/shared partial loading
- page-id and permission route mapping
- startup/runtime validation
- pages recently affected by archive/dashboard/readonly refactors

## Methodology

The following checks were completed across the solution:

1. Inventoried all Razor views under `AIS/Views`.
2. Scanned controllers for conventional `return View();` actions whose expected view no longer exists.
3. Scanned explicit `View(...)` and `PartialView(...)` targets for missing files.
4. Scanned local `src`/`href` asset references in all views against `AIS/wwwroot`.
5. Traced issue candidates from view to controller to JS/API/DB call surface before changing code.
6. Built the solution after fixes.
7. Started the application locally and reviewed startup logs for route/page-id validation errors.

## Important Testing Limitation

This review reached a reliable source-level and startup/runtime validation point, but it did **not** complete authenticated end-to-end business flow execution for every page because no test credentials, seeded business scenarios, or approved test data set were provided in this workspace.

That means the following were verified for the full solution:

- view file exists
- controller render path exists
- local static dependencies resolve
- startup-time page-id routing validation is clean
- compile/build is clean

The following still require manual business validation in a real test session:

- authenticated page render under user-specific permissions
- dropdown population from live data
- save/update/delete transactions
- AJAX round-trips against live DB procedures
- report exports/PDF content correctness under real data
- role-specific navigation behavior

## Summary

- Total Razor views checked: `470`
- Total issue groups found: `15`
- Total issue groups fixed in code: `9`
- Total pending issue groups: `6`
- Final build status: `dotnet build 'AIS\\AIS.csproj' -c Debug` passed with `0 errors`
- Runtime status: application started successfully on localhost after fixes; startup page-id warnings for the new FAD readonly pages were cleared

### High-Risk Modules

- `AuditeePortal`
- `PostCompliance`
- `Setup`
- `Execution`
- `Engagement`
- `Reports`
- `IID`
- `AdministrationPanel`

### Pages Still Needing Manual Business Validation

- all authenticated create/update/delete workflows
- all DB-driven dashboards and filter screens
- all report/pdf generation screens with live data
- all permission-sensitive menu-driven pages
- all archive routes still intentionally served through `ArchiveController`

### Pages/Flows Dependent on DB/Procedure Validation Outside Codex Scope

- `IID` investigation/inquiry save flows
- `Execution` observation/draft/closing flows
- `Planning` and `Engagement` approval/generation flows
- `AdministrationPanel` setup/configuration save screens
- `PostCompliance` and `AuditeePortal` response/compliance flows
- `Reports` screens backed by live reporting procedures

## Issue Log

### 1. Administration Panel / Entity Relationship Dashboard Partial

1. Module / Page Name  
   `AdministrationPanel / DashboardPartials / _EntityRelationship`
2. File(s) affected  
   `AIS/Views/AdministrationPanel/DashboardPartials/_EntityRelationship.cshtml`  
   `AIS/wwwroot/js/csp/Views_AdministrationPanel_entity_relationship.js`
3. Symptoms observed  
   The partial referenced an active JS file that no longer existed under `wwwroot/js/csp`, so page handlers would not load.
4. Root cause  
   The archive segregation moved the JS implementation out of the active `csp` path, but the active partial still referenced the old active location.
5. Fix applied  
   Restored an active script at `wwwroot/js/csp/Views_AdministrationPanel_entity_relationship.js` so the partial’s existing script reference resolves again.
6. Risk / impact  
   Without the script, entity relationship setup behavior would silently fail in the browser.
7. Testing performed  
   Local asset scan across all views, file existence validation, build verification.
8. Status  
   `Fixed`

### 2. Management Audit / Closing Partial

1. Module / Page Name  
   `ManagementAudit / MA_Partials / _Closing`
2. File(s) affected  
   `AIS/Views/ManagementAudit/MA_Partials/_Closing.cshtml`  
   `AIS/wwwroot/js/csp/Views_Execution_Audit_Execution.js`
3. Symptoms observed  
   The partial referenced `~/js/csp/Views_Execution_Audit_Execution.js`, but that active file was missing.
4. Root cause  
   The script had effectively been left only in the archive area after the refactor while an active partial still depended on it.
5. Fix applied  
   Restored an active JS file at the expected `csp` path.
6. Risk / impact  
   The Management Audit observation step would render but its client-side observation logic would be broken.
7. Testing performed  
   Local asset scan, dependency trace from partial to script, build verification.
8. Status  
   `Fixed`

### 3. Sampling / Account Document

1. Module / Page Name  
   `Sampling / account_document`
2. File(s) affected  
   `AIS/Views/Sampling/account_document.cshtml`
3. Symptoms observed  
   The view referenced `~/css/account_document.css`, which does not exist in `wwwroot/css`.
4. Root cause  
   A stale stylesheet reference remained in the view after styles were consolidated under `sampling.css`.
5. Fix applied  
   Removed the dead stylesheet reference and retained the valid `sampling.css` include already used by the page.
6. Risk / impact  
   Broken network requests and misleading dependency footprint on page load.
7. Testing performed  
   Asset scan, style reference trace, build verification.
8. Status  
   `Fixed`

### 4. Reports / significant_finding

1. Module / Page Name  
   `Reports / significant_finding`
2. File(s) affected  
   `AIS/Controllers/ReportsController.cs`
3. Symptoms observed  
   The controller action returned `View()` conventionally, but `Views/Reports/significant_finding.cshtml` no longer exists.
4. Root cause  
   The route survived after the view was renamed/replaced by `significant_observations_management_audit.cshtml`.
5. Fix applied  
   Wired the action to `~/Views/Reports/significant_observations_management_audit.cshtml`.
6. Risk / impact  
   Direct navigation to the report action would throw a Razor view-not-found error.
7. Testing performed  
   Conventional route scan, surviving view trace, build verification.
8. Status  
   `Fixed`

### 5. Reports / audit_plan_report_old

1. Module / Page Name  
   `Reports / audit_plan_report_old`
2. File(s) affected  
   `AIS/Controllers/ReportsController.cs`
3. Symptoms observed  
   The action returned a conventional view that no longer exists.
4. Root cause  
   The old report route remained, but the actual surviving screen is `audit_plan_report.cshtml`.
5. Fix applied  
   Wired the action to `~/Views/Reports/audit_plan_report.cshtml`.
6. Risk / impact  
   View-not-found failure when the route is used.
7. Testing performed  
   Controller/view scan, surviving file validation, build verification.
8. Status  
   `Fixed`

### 6. Reports / FAD_Legacy_User_Wise_Performance

1. Module / Page Name  
   `Reports / FAD_Legacy_User_Wise_Performance`
2. File(s) affected  
   `AIS/Controllers/ReportsController.cs`
3. Symptoms observed  
   The controller action targeted a conventional view that no longer exists.
4. Root cause  
   The remaining physical Razor file is `FAD_Legacy_User_Wise_Performance - Copy.cshtml`; the controller was never updated.
5. Fix applied  
   Wired the action to `~/Views/Reports/FAD_Legacy_User_Wise_Performance - Copy.cshtml`.
6. Risk / impact  
   Users reaching the route would hit a render failure.
7. Testing performed  
   Controller/view scan, file validation, build verification.
8. Status  
   `Fixed`

### 7. Reports / master_cdms

1. Module / Page Name  
   `Reports / master_cdms`
2. File(s) affected  
   `AIS/Controllers/ReportsController.cs`
3. Symptoms observed  
   The action returned a missing conventional view.
4. Root cause  
   The surviving report file is `master_cdms_trns.cshtml`, but the route still expected `master_cdms.cshtml`.
5. Fix applied  
   Wired the action to `~/Views/Reports/master_cdms_trns.cshtml`.
6. Risk / impact  
   Direct route failure for the report page.
7. Testing performed  
   Controller/view scan, surviving file validation, build verification.
8. Status  
   `Fixed`

### 8. Engagement / audit_criteria

1. Module / Page Name  
   `Engagement / audit_criteria`
2. File(s) affected  
   `AIS/Controllers/EngagementController.cs`
3. Symptoms observed  
   The action returned a missing conventional view `Views/Engagement/audit_criteria.cshtml`.
4. Root cause  
   The render target was removed from `Views/Engagement`, but the surviving `Views/Planning/audit_criteria.cshtml` still matches the action’s ViewData contract and its JS posts to `/Engagement/add_audit_criteria`.
5. Fix applied  
   Wired the action to `~/Views/Planning/audit_criteria.cshtml`.
6. Risk / impact  
   Engagement audit criteria page was unreachable despite having a working downstream action/API path.
7. Testing performed  
   Controller/view trace, view field contract review, JS route trace, build verification.
8. Status  
   `Fixed`

### 9. FAD Readonly Pages / PAGE_ID Validation

1. Module / Page Name  
   `FAD / Quality_Assurance_checking` and `FAD / Draft_report_Checking`
2. File(s) affected  
   `AIS/Services/PageIdResolver.cs`  
   `AIS/Services/PageIdRouteValidator.cs`
3. Symptoms observed  
   On runtime startup, the validator logged missing page-id mappings for `/FAD/Quality_Assurance_checking` (`418`) and `/FAD/Draft_report_Checking` (`417`). It also logged noise for `/Home/Index` with `PAGE_ID 0`.
4. Root cause  
   The new FAD readonly routes had no resolver aliases, and the validator treated DB menu rows with invalid/non-positive page IDs as real mapping failures.
5. Fix applied  
   Added explicit route-to-page-id mappings for the two FAD pages and updated the validator to ignore `PAGE_ID <= 0` entries.
6. Risk / impact  
   Permission/page-id diagnostics were noisy and the new FAD pages were not centrally recognized by the page-id resolver.
7. Testing performed  
   Local runtime startup on `http://127.0.0.1:5100`, startup log inspection before/after fix, build verification.
8. Status  
   `Fixed`

### 10. Engagement / Index

1. Module / Page Name  
   `Engagement / Index`
2. File(s) affected  
   `AIS/Controllers/EngagementController.cs`
3. Symptoms observed  
   `EngagementController.Index()` returns `View()`, but `Views/Engagement/Index.cshtml` does not exist.
4. Root cause  
   A conventional controller landing action remains without a corresponding view.
5. Fix applied  
   None in this pass.
6. Risk / impact  
   Direct navigation to `/Engagement` or `/Engagement/Index` can fail with view-not-found.
7. Testing performed  
   Conventional route scan, manual controller/view verification.
8. Status  
   `Needs business clarification`

### 11. Execution / Index

1. Module / Page Name  
   `Execution / Index`
2. File(s) affected  
   `AIS/Controllers/ExecutionController.cs`
3. Symptoms observed  
   `ExecutionController.Index()` returns `View()`, but `Views/Execution/Index.cshtml` does not exist.
4. Root cause  
   The module root action remains without a corresponding view.
5. Fix applied  
   None in this pass.
6. Risk / impact  
   Direct navigation to `/Execution` or `/Execution/Index` can fail.
7. Testing performed  
   Conventional route scan, manual controller/view verification.
8. Status  
   `Needs business clarification`

### 12. Execution / Sub_voilation_audit_observation

1. Module / Page Name  
   `Execution / Sub_voilation_audit_observation`
2. File(s) affected  
   `AIS/Controllers/ExecutionController.cs`
3. Symptoms observed  
   The action returns a missing conventional view `Views/Execution/Sub_voilation_audit_observation.cshtml`.
4. Root cause  
   The action survived, but no corresponding Razor view exists in the active tree or archive under the same name.
5. Fix applied  
   None in this pass because there is no code-evident one-to-one surviving replacement.
6. Risk / impact  
   Direct navigation to this route will fail even though its dependent ViewData still populates.
7. Testing performed  
   Conventional route scan, repo-wide name search, controller inspection.
8. Status  
   `Needs business clarification`

### 13. AuditeePortal Legacy Route Cluster

1. Module / Page Name  
   `AuditeePortal`
2. File(s) affected  
   `AIS/Controllers/AuditeePortalController.cs`
3. Symptoms observed  
   The following actions still return `View()` with no matching Razor view:
   - `old_para_reply`
   - `paras_compliance_by_auditee`
   - `old_para_br_comp_ref`
   - `old_para_br_comp_review`
   - `old_para_br_comp_review_ref`
   - `Auditee_Branch_Response`
   - `Branch_Compliance`
   - `Zonal_Administration`
   - `Implementation_officer`
   - `Audit_Zone_Action`
4. Root cause  
   Legacy routes survived while their views were removed or never migrated into the active or archive render path.
5. Fix applied  
   None in this pass because no safe replacement mapping is provable from current source.
6. Risk / impact  
   Any active DB menu/page entry or hardcoded link to these actions will break at render time.
7. Testing performed  
   Conventional route scan, repo-wide reference search, controller inspection.
8. Status  
   `Pending`

### 14. PostCompliance Legacy Route Cluster

1. Module / Page Name  
   `PostCompliance`
2. File(s) affected  
   `AIS/Controllers/PostComplianceController.cs`
3. Symptoms observed  
   The following actions still return `View()` with no matching Razor view:
   - `compliance_submitted_by_auditee`
   - `compliance_submitted_by_auditee_ref`
   - `compliance_for_settlement`
4. Root cause  
   Stale routes remained after view cleanup/refactoring.
5. Fix applied  
   None in this pass because no source-backed replacement view could be proven.
6. Risk / impact  
   Any route/menu reference to these actions will fail at render time.
7. Testing performed  
   Conventional route scan, controller inspection, repo-wide reference search.
8. Status  
   `Pending`

### 15. Setup Legacy Route Cluster

1. Module / Page Name  
   `Setup`
2. File(s) affected  
   `AIS/Controllers/SetupController.cs`
3. Symptoms observed  
   The following actions return `View()` with no matching Razor view:
   - `manage_inspection_unit_branches`
   - `manage_reporting_offices`
4. Root cause  
   Controller actions remained after the underlying views were removed or renamed.
5. Fix applied  
   None in this pass because no code-evident replacement exists.
6. Risk / impact  
   Direct route usage or menu usage will fail with view-not-found.
7. Testing performed  
   Conventional route scan, controller inspection, repo-wide reference search.
8. Status  
   `Pending`

## Global Checks That Passed

- All local `src` / `href` asset references in `AIS/Views/**/*.cshtml` now resolve under `AIS/wwwroot`.
- No missing explicit `View("~/...")` or `PartialView("~/...")` targets were found.
- Application startup succeeded locally after the latest fixes.
- Startup page-id warning noise for the new FAD readonly routes is resolved.
- Final build passed successfully.

## Module-Wise View Coverage Checklist

The following grouped checklist reflects the full set of Razor views inspected during this review.

```text
AdministrationPanel (43): ais_post_compliance.cshtml, api_master.cshtml, audit_comp_management.cshtml, audit_criteria.cshtml, audit_period.cshtml, authorize_audit_checklist.cshtml, authorize_auditee_entities_update.cshtml, catalog_uploads.cshtml, compliance_flow.cshtml, dashboard_layout.cshtml, entity_addition.cshtml, Entity_Dashboard.cshtml, entity_gm_reporting_div_management.cshtml, entity_heirarchy.cshtml, entity_shifting.cshtml, gm_repo_line_management.cshtml, group_role_assignment.cshtml, groups.cshtml, hr_design_wise_role.cshtml, manage_ent_audit_dept.cshtml, manage_entity_mapping.cshtml, manage_entity_relations.cshtml, manage_entity_type.cshtml, manage_obs_status.cshtml, manage_user.cshtml, manage_user_rights.cshtml, ManagePublicHolidays.cshtml, ManageVersionHistory.cshtml, MasterAdminControlPanel.cshtml, menu_assignment.cshtml, menu_management.cshtml, pages_management.cshtml, review_audit_checklist.cshtml, risk_model.cshtml, setup_auditee_entities.cshtml, setup_engagement_reversal.cshtml, setup_observation_reversal.cshtml, status_reversal_audit_entities.cshtml, sub_menu_management.cshtml, SystemLogs.cshtml, update_auditee_entities.cshtml, User_Dashboard.cshtml, user_roles.cshtml
AR_Partials (25): _AccountDocumentReplica.cshtml, _AccountOpeningReplica.cshtml, _AccountTransactionReplica.cshtml, _AuditReportCompilationStep.cshtml, _CashCountReplica.cshtml, _DraftReportStep.cshtml, _ExceptionAccountReplica.cshtml, _ExceptionLoanReplica.cshtml, _ExceptionReportStep.cshtml, _ExitAuditStep.cshtml, _FixedAssetsReplica.cshtml, _JoiningStep.cshtml, _LoanDocumentReplica.cshtml, _LoanTransactionReplica.cshtml, _MemoCreationStep.cshtml, _SamplingAccountDocument.cshtml, _SamplingAccountTransaction.cshtml, _SamplingBiomet.cshtml, _SamplingLoanDocument.cshtml, _SamplingLoans.cshtml, _SamplingLoanTransaction.cshtml, _SamplingStep.cshtml, _SubmitToAuditeeStep.cshtml, _VoucherCheckingReplica.cshtml, _WorkingPaperStep.cshtml
Archive (45): Add_Legacy_Para.cshtml, Audit_Execution.cshtml, audit_observation_text.cshtml, audit_template.cshtml, auth_del_dup_para.cshtml, Authorize_Adding_Legacy_Para.cshtml, Authorize_Change_Settle_Para_Status.cshtml, Authorize_Update_Legacy_Para_Gist_Para.cshtml, branch_deposit_info.cshtml, change_para_status.cshtml, change_para_status_authorize.cshtml, change_status_new_Para.cshtml, change_status_new_Para_authorize.cshtml, control_violation.cshtml, dashboard.cshtml, deposit_account.cshtml, deposit_account_details.cshtml, disb_info.cshtml, entity_relationship.cshtml, functional_resp_wise_paras.cshtml, functional_resp_wise_paras_ho.cshtml, glhead_summary.cshtml, income_expenditure.cshtml, loan_case_details.cshtml, loan_case_document.cshtml, loan_scheme.cshtml, loan_scheme_yearly.cshtml, memo_status.cshtml, no_entities_risk_based_planning.cshtml, no_entities_risk_based_planning_b.cshtml, obs_management.cshtml, old_outstanding_paras.cshtml, old_para_reply_cad.cshtml, Para_Text_Update_FAD.cshtml, para_view.cshtml, pre_audit_info.cshtml, pre_audit_info_detail.cshtml, Reply.cshtml, reporting_wise_obs.cshtml, Settled_Para.cshtml, staff_pos_info.cshtml, Update_Gist_Para_No.cshtml, Update_Legacy_Paras.cshtml, Update_Legacy_Paras_FAD.cshtml, Update_Legacy_Paras_HO.cshtml
AuditeePortal (2): ccqs.cshtml, observation_assigned.cshtml
BAC (3): cia_analysis.cshtml, cia_analysis_detail.cshtml, dashboard.cshtml
BO_Partials (6): _CheckingDraftReportPartial.cshtml, _CheckingQualityReviewPartial.cshtml, _DraftReportPartial.cshtml, _IssueReportPartial.cshtml, _ObservationReferenceSection.cshtml, _QualityReviewPartial.cshtml
CAU (4): monitoring_oms.cshtml, om_creation.cshtml, om_reply.cshtml, reports.cshtml
Dashboard (11): annex_wise_obs.cshtml, audit_performance - Copy.cshtml, audit_performance.cshtml, compliance_summary.cshtml, dashboard.cshtml, entity_wise_obs.cshtml, entity_wise_obs_detail.cshtml, repetative_para.cshtml, serious_fraudulent_obs_gm.cshtml, violation_wise_paras.cshtml, zone_wise_paras.cshtml
DashboardPartials (18): _AuthorizeAuditeeEntitiesUpdate.cshtml, _EntityAddition.cshtml, _EntityRelationship.cshtml, _GroupRoleAssignment.cshtml, _ManageChecklist.cshtml, _ManageChecklistDetail.cshtml, _ManageSubChecklist.cshtml, _ManageUser.cshtml, _ManageUserRights.cshtml, _MenuAssignment.cshtml, _PagesManagement.cshtml, _ProcessDetailAuthorize.cshtml, _ReviewAuditChecklist.cshtml, _SetupAuditeeEntities.cshtml, _SubMenuManagement.cshtml, _SubProcessAuthorize.cshtml, _UpdateAuditeeEntities.cshtml, _EntityShifting.cshtml
Email (2): Edit.cshtml, Send.cshtml
Engagement (17): acceptance.cshtml, ccqs.cshtml, change_request.cshtml, create_audit_plan.cshtml, eng_plan_approvals.cshtml, eng_plan_list.cshtml, eng_plan_ref_list.cshtml, engagement_plan.cshtml, Join.cshtml, notifications.cshtml, ongoing_engagements_list.cshtml, post_changes_approved_plan.cshtml, post_changes_team_members.cshtml, preparation_ccqs.cshtml, submission_for_approval.cshtml, submission_for_review.cshtml, task_list.cshtml
Execution (27): auditee_observations_report.cshtml, auditee_observations_report_cau.cshtml, auditee_position_outlook.cshtml, cau_observation.cshtml, checklist.cshtml, checklist_details.cshtml, checklist_summary.cshtml, closing.cshtml, Concluding_Closing_Audit.cshtml, create_dsa.cshtml, draft_audit_report.cshtml, draft_audit_report_branch.cshtml, engagement_plan.cshtml, list_dsa.cshtml, manage_audit_paras.cshtml, manage_audit_paras_authorized.cshtml, manage_draft_report_paras.cshtml, manage_draft_report_paras_branch.cshtml, manage_dsa.cshtml, manage_legacy_paras.cshtml, manage_observations.cshtml, manage_observations_branches.cshtml, observation_export_pdf.cshtml, pre_concluding_audit.cshtml, pre_concluding_audit_ho.cshtml, search_checklistdetails.cshtml, subchecklist.cshtml
FAD (7): _AuthorizeParaStatusTable.cshtml, _ParaStatusTable.cshtml, AuthorizeParaStatus.cshtml, ChangeParaStatus.cshtml, Draft_report_Checking.cshtml, Quality_Assurance_checking.cshtml, risk_register.cshtml
FAD_TASK (3): Fad_Desk_rpt.cshtml, observation_review.cshtml, para_shifting.cshtml
FADRiskControlPanel (1): Index.cshtml
FieldAudit (10): _AuditReport.cshtml, _Closing.cshtml, _Exception.cshtml, _Join.cshtml, _ManageObservationBranches.cshtml, _Observation.cshtml, _Samples.cshtml, _WPaper.cshtml, AR_Dashboard.cshtml, BO_Dashboard.cshtml
FieldAuditReport (8): _ReportActionsNav.cshtml, Engagements.cshtml, FinalizeReport.cshtml, KpiSnapshot.cshtml, NarrativeSections.cshtml, NplSnapshot.cshtml, ReportOverview.cshtml, StaffSnapshot.cshtml
HM (5): ManageSbpPassword.cshtml, old_paras_monitoring.cshtml, old_paras_monitoring_ppno.cshtml, SbpObservationHistory.cshtml, SbpObservationRegister.cshtml
Home (3): change_password.cshtml, Index.cshtml, Privacy.cshtml
IAMS (2): old_para.cshtml, paras.cshtml
IID (19): _ComplaintSelector.cshtml, _InitialAssessmentSummary.cshtml, _InquiryReport_PrintTemplate.cshtml, Analysis.cshtml, CaseStudy.cshtml, FFR_PART1.cshtml, FFR_PART2.cshtml, FFR_PART3.cshtml, FinalApproval.cshtml, HeadReview.cshtml, InitialAssessment.cshtml, InquiryReport.cshtml, InquiryReportReadOnly.cshtml, InvestigationPlan.cshtml, MonitoringDashboard.cshtml, PlanApproval.cshtml, Reports.cshtml, SubmitComplaint.cshtml, TaskListIID.cshtml
Login (3): index.cshtml, index_dev.cshtml, Maintenance.cshtml
MA_Partials (6): _Closing.cshtml, _ConcludingClosingAudit.cshtml, _DraftAuditReport.cshtml, _Join.cshtml, _ManageObservations.cshtml, _PreConcludingAuditHO.cshtml
ManagementAudit (1): MA_Dashboard.cshtml
MANReport (9): _ManReportNav.cshtml, AuditObjectiveScope.cshtml, AuditObservations.cshtml, Cover.cshtml, ExecutiveSummary.cshtml, Finalize.cshtml, Home.cshtml, ParasSettledDuringAudit.cshtml, StaffSnapshot.cshtml
Observation (1): ObservationPrint.cshtml
OM (2): om_assignment.cshtml, om_response.cshtml
PageNotFound (1): index.cshtml
Partials (11): _AuditCriteriaApprovalStep.cshtml, _AuditCriteriaStep.cshtml, _CreateEngagementStep.cshtml, _EngagementApprovalStep.cshtml, _EngagementPlanReplica.cshtml, _GeneratePlanStep.cshtml, _ReferredBackAuditCriteriaStep.cshtml, _ReferredBackEngagementStep.cshtml, _TeamMembersStep.cshtml, _TentativeAuditPlanStep.cshtml, _TentativeEngagementPlanReplica.cshtml
Planning (20): audit_criteria.cshtml, audit_criteria_approval.cshtml, audit_period.cshtml, audit_plan.cshtml, holiday_calendar.cshtml, Planning.cshtml, post_changes_approved_plan.cshtml, post_changes_criteria.cshtml, post_changes_team_members.cshtml, refferedback_audit_criteria.cshtml, special_assignment.cshtml, special_audit_criteria.cshtml, special_audit_criteria_approval.cshtml, staff_position.cshtml, submission_for_approval.cshtml, submission_for_review.cshtml, team_members.cshtml, tentative_audit_plan.cshtml, tentative_audit_plan_ho_units.cshtml, tentative_engagement_plan.cshtml
PostCompliance (8): cau_post_compliance_to_branches.cshtml, cau_post_compliance_to_branches_reply.cshtml, cau_post_compliance_to_branches_review.cshtml, change_status_new_Para_reviewer.cshtml, monitoring_of_para_settlement.cshtml, post_compliance.cshtml, post_compliance_ho_monitoring.cshtml, post_compliance_review.cshtml
Reports (78): aging_observations.cshtml, analysis_of_settlement_paras.cshtml, annex_violation.cshtml, annexure_exercise_status.cshtml, approved_plan.cshtml, audit_para_recon.cshtml, Audit_Period_Or_Entity_Wise_Report.cshtml, Audit_Plan_Engagement.cshtml, audit_plan_report.cshtml, audit_report.cshtml, audit_report_dept.cshtml, audit_typewise_report.cshtml, auditee_observations_report.cshtml, br_au_pos.cshtml, cad_isad_achievement.cshtml, cnic_default_loan_report.cshtml, cnic_loan_report.cshtml, compliance_progress_report.cshtml, Concluding_Closing_Audit.cshtml, current_audit_progress.cshtml, current_sessions.cshtml, department_performance.cshtml, dept_wise_outstanding_paras.cshtml, div_out_paras.cshtml, div_seg_paras.cshtml, emp_info.cshtml, eng_plan_delay_analysis_report.cshtml, exec_stats.cshtml, FAD_Aging_of_Audit_Paras_Monthly.cshtml, FAD_Audit_Plan.cshtml, FAD_Branch_Audit_Status.cshtml, FAD_Branch_Risk_Rating.cshtml, FAD_Compliance_Pos_Fortnightly_FAD_Level.cshtml, FAD_Compliance_Pos_Fortnightly_Zone_Level.cshtml, FAD_DSA_Position_Fortnightly.cshtml, FAD_Gistwise_Details_Monthly.cshtml, FAD_HO_User_Legacy_Para_Performance.cshtml, FAD_Legacy_Para_Performance.cshtml, FAD_Legacy_User_Wise_Performance - Copy.cshtml, FAD_Legacy_Zone_Wise_Performance.cshtml, FAD_Monthly_Review.cshtml, FAD_New_Old_Para_Performance.cshtml, FAD_New_Para_Performance.cshtml, fad_para_res.cshtml, FAD_Process_Function_Wise_Analysis.cshtml, final_audit_report.cshtml, gist_wise_report.cshtml, glhead_details.cshtml, glhead_transaction_details.cshtml, glhead_yearwise_summary.cshtml, group_wise_pages.cshtml, group_wise_users_count.cshtml, ho_total_paras_details.cshtml, join_comp_report.cshtml, loan_report.cshtml, loan_transfer_report.cshtml, management_au_report_zonewise.cshtml, master_cdms_trns.cshtml, monthwise_settlement_paras.cshtml, para_position.cshtml, paras_compliance_summary_report.cshtml, post_compliance_settlement_report.cshtml, riskwise_observations.cshtml, role_activity_logs.cshtml, role_wise_user.cshtml, search_para_text_report.cshtml, settled_paras_report.cshtml, settlement_old_paras.cshtml, significant_observations_management_audit.cshtml, status_wise_compliance.cshtml, user_activity_graph.cshtml, user_activity_logs.cshtml, user_wise_performance.cshtml, year_wise_all_paras_details.cshtml, year_wise_outstanding_paras.cshtml, zone_branch_wise_para_position_report.cshtml, zone_wise_performance.cshtml, zonewise_settlement_paras.cshtml
RiskAssessment (11): _ViewImports.cshtml, inherited_risk_branches.cshtml, inherited_risk_dept.cshtml, risk_assessment_divisions.cshtml, risk_assessment_ent_types.cshtml, risk_assessment_entities.cshtml, risk_assessment_functions.cshtml, risk_assessment_ho_units.cshtml, risk_model.cshtml, risk_rating_annex_wise.cshtml, risk_rating_for_branches_working.cshtml
Sampling (17): account_document.cshtml, Account_exception.cshtml, Account_exception_details.cshtml, account_transaction.cshtml, account_transaction_master.cshtml, add_report.cshtml, biomet.cshtml, excep_account_str.cshtml, ExceptionMonitoring.cshtml, list_reports.cshtml, list_samples.cshtml, loan_documents.cshtml, loan_transactions.cshtml, loans.cshtml, loans_exception.cshtml, ManageExceptionReportFormat.cshtml, sample_monitoring.cshtml
Setup (26): annexure_assignment_para.cshtml, audit_zones.cshtml, authorize_remove_duplicate_checklists.cshtml, authorize_remove_duplicate_process.cshtml, authorize_remove_duplicate_sub_process.cshtml, branches.cshtml, Checklist_Dashboard.cshtml, compliance_hierarchy.cshtml, departments.cshtml, divisions.cshtml, inspection_unit.cshtml, manage_annexure.cshtml, Manage_audit_zone_branches.cshtml, manage_checklist.cshtml, manage_checklist_detail.cshtml, manage_sub_checklist.cshtml, para_migration.cshtml, process_detail_authorize.cshtml, process_detail_review.cshtml, processes.cshtml, ref_checklist_detail.cshtml, remove_duplicate_checklists.cshtml, remove_duplicate_process.cshtml, remove_duplicate_sub_process.cshtml, sub_entities.cshtml, sub_process_authorize.cshtml
Shared (8): _BlankLayout.cshtml, _DashboardStepAccessDenied.cshtml, _GetEngagementSelector.cshtml, _Layout.cshtml, _Layout_Minimal.cshtml, _ValidationScriptsPartial.cshtml, Error.cshtml, StatusCode.cshtml
Views (2): _ViewImports.cshtml, _ViewStart.cshtml
WorkingPaper (5): account_opening.cshtml, cash_count.cshtml, fixed_assets.cshtml, loan_case_file.cshtml, voucher_checking.cshtml
```
