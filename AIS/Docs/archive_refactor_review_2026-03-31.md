# Archive Refactor Review - 2026-03-31

## Scope

This note captures the archive segregation refactor for the legacy screens moved under `AIS\Views\Archive\` and `AIS\wwwroot\js\Archive\`.

It contains two required outputs:

1. Final archived view list
2. Stored procedure and DB table mapping trace for those archived views, based only on code paths found in controllers, page JS, and `DBConnection*` methods

## Notes And Exceptions

- `FAD/review_gist_recommendation` was present in `AIS\wwwroot\Images\Page ID.xlsx`, so its page-id row was moved into the archive registry, but no current MVC view, controller action, or dedicated JS file was found in the codebase to move.
- These source paths were requested but had no matching active page-id row in `Page ID.xlsx`, so there was nothing to remove from the active workbook for them:
  - `AuditeePortal/old_para_reply_cad`
  - `AuditeePortal/Para_Text_Update_FAD`
  - `AuditeePortal/Reply`
  - `Execution/auth_del_dup_para`
- For the traced archive code paths below, table names are not directly visible in code unless explicitly noted. Nearly all access is through stored procedures, so the table mapping is marked `SP only / not directly visible in code`.

## Final Archived View List

### AuditeePortal

- `AIS\Views\Archive\old_outstanding_paras.cshtml`
- `AIS\Views\Archive\old_para_reply_cad.cshtml`
- `AIS\Views\Archive\Para_Text_Update_FAD.cshtml`
- `AIS\Views\Archive\Reply.cshtml`

### Dashboard

- `AIS\Views\Archive\para_view.cshtml`
- `AIS\Views\Archive\reporting_wise_obs.cshtml`
- `AIS\Views\Archive\no_entities_risk_based_planning.cshtml`
- `AIS\Views\Archive\no_entities_risk_based_planning_b.cshtml`
- `AIS\Views\Archive\functional_resp_wise_paras.cshtml`
- `AIS\Views\Archive\functional_resp_wise_paras_ho.cshtml`

### Execution

- `AIS\Views\Archive\Add_Legacy_Para.cshtml`
- `AIS\Views\Archive\Audit_Execution.cshtml`
- `AIS\Views\Archive\Authorize_Adding_Legacy_Para.cshtml`
- `AIS\Views\Archive\Authorize_Change_Settle_Para_Status.cshtml`
- `AIS\Views\Archive\Authorize_Update_Legacy_Para_Gist_Para.cshtml`
- `AIS\Views\Archive\auth_del_dup_para.cshtml`
- `AIS\Views\Archive\branch_deposit_info.cshtml`
- `AIS\Views\Archive\deposit_account.cshtml`
- `AIS\Views\Archive\deposit_account_details.cshtml`
- `AIS\Views\Archive\disb_info.cshtml`
- `AIS\Views\Archive\loan_case_details.cshtml`
- `AIS\Views\Archive\loan_case_document.cshtml`
- `AIS\Views\Archive\loan_scheme.cshtml`
- `AIS\Views\Archive\loan_scheme_yearly.cshtml`
- `AIS\Views\Archive\obs_management.cshtml`
- `AIS\Views\Archive\pre_audit_info.cshtml`
- `AIS\Views\Archive\pre_audit_info_detail.cshtml`
- `AIS\Views\Archive\Settled_Para.cshtml`
- `AIS\Views\Archive\staff_pos_info.cshtml`
- `AIS\Views\Archive\Update_Gist_Para_No.cshtml`
- `AIS\Views\Archive\Update_Legacy_Paras.cshtml`
- `AIS\Views\Archive\Update_Legacy_Paras_FAD.cshtml`
- `AIS\Views\Archive\Update_Legacy_Paras_HO.cshtml`

### Reports

- `AIS\Views\Archive\income_expenditure.cshtml`
- `AIS\Views\Archive\glhead_summary.cshtml`

### Setup

- `AIS\Views\Archive\control_violation.cshtml`

### AdministrationPanel

- `AIS\Views\Archive\audit_template.cshtml`
- `AIS\Views\Archive\audit_observation_text.cshtml`
- `AIS\Views\Archive\memo_status.cshtml`
- `AIS\Views\Archive\entity_relationship.cshtml`

### PostCompliance

- `AIS\Views\Archive\change_status_new_Para.cshtml`
- `AIS\Views\Archive\change_status_new_Para_authorize.cshtml`
- `AIS\Views\Archive\change_para_status.cshtml`
- `AIS\Views\Archive\change_para_status_authorize.cshtml`

### HM

- `AIS\Views\Archive\dashboard.cshtml`

## Render Action Trace

These are the MVC actions now served by `ArchiveController`, with the render-time DB methods they call.

| Archived Route | View | ArchiveController Action | DBConnectionArchive Render Method(s) | Stored Procedure(s) | Classification | DB Table Mapping |
| --- | --- | --- | --- | --- | --- | --- |
| `/AuditeePortal/Para_Text_Update_FAD` | `Para_Text_Update_FAD.cshtml` | `Para_Text_Update_FAD()` | `GetAuditeeAssignedEntities()` | `pkg_ae.P_GetAuditeeAssignedEntities` | Read SP | SP only / not directly visible in code |
| `/AuditeePortal/old_outstanding_paras` | `old_outstanding_paras.cshtml` | `old_outstanding_paras()` | `GetAuditeeOldParasEntities()` | `pkg_ae.P_GetAuditeeOldParasentitiesFAD` | Read SP | SP only / not directly visible in code |
| `/AuditeePortal/old_para_reply_cad` | `old_para_reply_cad.cshtml` | `old_para_reply_cad()` | `GetAuditVoilationcats()`, `GetRisks()` | `pkg_ar.P_GetAuditVoilationcats`, `pkg_ad.P_GetRisks` | Read SP | SP only / not directly visible in code |
| `/AuditeePortal/Reply` | `Reply.cshtml` | `reply()` | None | None | Render only | None visible in render action |
| `/Dashboard/functional_resp_wise_paras` | `functional_resp_wise_paras.cshtml` | `functional_resp_wise_paras()` | `GetFunctionalListForDashboard()` | `pkg_db.P_GET_Dash_table_functionwise_names` | Read SP | SP only / not directly visible in code |
| `/Dashboard/functional_resp_wise_paras_ho` | `functional_resp_wise_paras_ho.cshtml` | `functional_resp_wise_paras_ho()` | None | None | Render only | None visible in render action |
| `/dashboard/no_entities_risk_based_planning` | `no_entities_risk_based_planning.cshtml` | `no_entities_risk_based_planning()` | `GetRiskProcessDefinition()` | `pkg_lg.P_GetRiskProcessDefinition` | Read SP | SP only / not directly visible in code |
| `/dashboard/no_entities_risk_based_planning_b` | `no_entities_risk_based_planning_b.cshtml` | `no_entities_risk_based_planning_b()` | `GetRiskProcessDefinition()` | `pkg_lg.P_GetRiskProcessDefinition` | Read SP | SP only / not directly visible in code |
| `/Dashboard/reporting_wise_obs` | `reporting_wise_obs.cshtml` | `reporting_wise_obs()` | None | None | Render only | None visible in render action |
| `/Dashboard/para_view` | `para_view.cshtml` | `para_view()` | `GetAnnexuresForChecklistDetail()`, `GetrealtionshiptypeForDashboardPanel()` | `pkg_ad.p_get_annexure`, `pkg_db.P_Getrealtionshiptype` | Read SP | SP only / not directly visible in code |
| `/Execution/obs_management` | `obs_management.cshtml` | `obs_management()` | `GetRisks()`, `GetEntitiesDropDownForManageObservations()` | `pkg_ad.P_GetRisks`, `pkg_ar.P_get_entities_for_manage_observations` | Read SP | SP only / not directly visible in code |
| `/Execution/Settled_Para` | `Settled_Para.cshtml` | `Settled_Para()` | `GetLegacyParasEntities()` | `pkg_ar.P_GetEntitiesForLegacyPara` | Read SP | SP only / not directly visible in code |
| `/Execution/Authorize_Change_Settle_Para_Status` | `Authorize_Change_Settle_Para_Status.cshtml` | `Authorize_Change_Settle_Para_Status()` | None | None | Render only | None visible in render action |
| `/Execution/Authorize_Update_Legacy_Para_Gist_Para` | `Authorize_Update_Legacy_Para_Gist_Para.cshtml` | `Authorize_Update_Legacy_Para_Gist_Para()` | None | None | Render only | None visible in render action |
| `/Execution/staff_pos_info` | `staff_pos_info.cshtml` | `staff_pos_info()` | `GetStaffPosition()` | `pkg_ai.P_GetStaffPosition` | Read SP | SP only / not directly visible in code |
| `/Execution/loan_case_document` | `loan_case_document.cshtml` | `loan_case_document()` | `GetDepartments(354)`, `GetDivisions(false)`, `GetZones()` | `pkg_rpt.R_GetDepartments`, `pkg_ais.P_GetDepartments`, `pkg_ad.P_GetZones` | Read SP | SP only / not directly visible in code |
| `/Execution/auth_del_dup_para` | `auth_del_dup_para.cshtml` | `auth_del_dup_para()` | `GetDuplicateParasAuthorizationEntityList()` | `pkg_hd.P_GET_DUPLICATE_PARAS_ENT_FOR_AUTH` | Read SP | SP only / not directly visible in code |
| `/Execution/audit_execution` | `Audit_Execution.cshtml` | `audit_execution()` | `GetDivisions(false)`, `GetRiskProcessDefinition()`, `GetAuditVoilationcats()`, `GetAuditEntitiesForOtherEntitySelection()`, `GetRisks()` | `pkg_ais.P_GetDepartments`, `pkg_lg.P_GetRiskProcessDefinition`, `pkg_ar.P_GetAuditVoilationcats`, `pkg_ar.p_get_auditee_submission_list`, `pkg_ad.P_GetRisks` | Mixed | SP only / not directly visible in code |
| `/Execution/pre_audit_info` | `pre_audit_info.cshtml` | `pre_audit_info()` | None | None | Render only | None visible in render action |
| `/Execution/pre_audit_info_detail` | `pre_audit_info_detail.cshtml` | `pre_audit_info_detail()` | `GetDivisions(false)`, `GetRiskProcessDefinition()`, `GetAuditVoilationcats()`, `GetRisks()` | `pkg_ais.P_GetDepartments`, `pkg_lg.P_GetRiskProcessDefinition`, `pkg_ar.P_GetAuditVoilationcats`, `pkg_ad.P_GetRisks` | Read SP | SP only / not directly visible in code |
| `/Execution/loan_case_details` | `loan_case_details.cshtml` | `loan_case_details()` | None | None | Render only | None visible in render action |
| `/Execution/branch_deposit_info` | `branch_deposit_info.cshtml` | `branch_deposit_info()` | None | None | Render only | None visible in render action |
| `/Execution/deposit_account` | `deposit_account.cshtml` | `deposit_account()` | `GetDepositCat()` | `pkg_AI.P_GetDepositACCOUNTCATEGORY` | Read SP | SP only / not directly visible in code |
| `/Execution/loan_scheme` | `loan_scheme.cshtml` | `loan_scheme(int engId)` | `GetLoansScheme(engId)` | `pkg_ai.P_preauditinfo_loan_scheme` | Read SP | SP only / not directly visible in code |
| `/Execution/loan_scheme_yearly` | `loan_scheme_yearly.cshtml` | `loan_scheme_yearly(int engId)` | `GetLoansSchemeYearly(engId)` | `pkg_ai.P_preauditinfo_loan_scheme_yearly` | Read SP | SP only / not directly visible in code |
| `/Execution/Add_Legacy_Para` | `Add_Legacy_Para.cshtml` | `Add_Legacy_Para()` | `GetAuditeeEntitiesType()`, `GetAuditYearForAddLegacyPara()`, `GetAuditNatureForAddLegacyPara()` | `pkg_hd.P_GetAuditEntitiestype`, `pkg_hd.P_GetAuditYear`, `pkg_hd.P_GetAuditnature` | Read SP | SP only / not directly visible in code |
| `/Execution/Authorize_Adding_Legacy_Para` | `Authorize_Adding_Legacy_Para.cshtml` | `Authorize_Adding_Legacy_Para()` | None | None | Render only | None visible in render action |
| `/Execution/Update_Gist_Para_No` | `Update_Gist_Para_No.cshtml` | `Update_Gist_Para_No()` | `GetLegacyParasEntities()` | `pkg_ar.P_GetEntitiesForLegacyPara` | Read SP | SP only / not directly visible in code |
| `/Execution/Update_Legacy_Paras` | `Update_Legacy_Paras.cshtml` | `Update_Legacy_Paras()` | `GetLegacyParasEntities()`, `GetAuditChecklistCAD()`, `GetAuditVoilationcats()`, `GetRisks()` | `pkg_ar.P_GetEntitiesForLegacyPara`, `pkg_ad.P_GetAuditChecklistCAD`, `pkg_ar.P_GetAuditVoilationcats`, `pkg_ad.P_GetRisks` | Mixed | SP only / not directly visible in code |
| `/Execution/Update_Legacy_Paras_HO` | `Update_Legacy_Paras_HO.cshtml` | `Update_Legacy_Paras_HO()` | `GetLegacyParasEntitiesHO()`, `GetAuditVoilationcats()`, `GetRisks()` | `pkg_ar.P_GetEntitiesForLegacyPara_HO`, `pkg_ar.P_GetAuditVoilationcats`, `pkg_ad.P_GetRisks` | Mixed | SP only / not directly visible in code |
| `/Execution/Update_Legacy_Paras_FAD` | `Update_Legacy_Paras_FAD.cshtml` | `Update_Legacy_Paras_FAD()` | `GetLegacyParasEntitiesFAD()`, `GetAuditChecklistCAD()`, `GetRisks()` | `pkg_FAD.P_GetEntitiesForLegacyPara`, `pkg_ad.P_GetAuditChecklistCAD`, `pkg_ad.P_GetRisks` | Mixed | SP only / not directly visible in code |
| `/Execution/deposit_account_details` | `deposit_account_details.cshtml` | `deposit_account_details()` | None | None | Render only | None visible in render action |
| `/Execution/disb_info` | `disb_info.cshtml` | `disb_info()` | `GetDivisions(false)`, `GetRiskProcessDefinition()`, `GetAuditVoilationcats()`, `GetRisks()` | `pkg_ais.P_GetDepartments`, `pkg_lg.P_GetRiskProcessDefinition`, `pkg_ar.P_GetAuditVoilationcats`, `pkg_ad.P_GetRisks` | Read SP | SP only / not directly visible in code |
| `/Reports/glhead_summary` | `glhead_summary.cshtml` | `glhead_summary(int engId)` | `GetGlheadDetails(engId)` | `pkg_ai.p_getglheadsummary` | Read SP | SP only / not directly visible in code |
| `/Reports/income_expenditure` | `income_expenditure.cshtml` | `income_expenditure()` | None | None | Render only | None visible in render action |
| `/Setup/control_violation` | `control_violation.cshtml` | `control_violation()` | `GetControlViolations()` | `pkg_ad.P_GetControlViolations` | Read SP | SP only / not directly visible in code |
| `/AdministrationPanel/Audit_Observation_text` | `audit_observation_text.cshtml` | `audit_observation_text()` | None | None | Render only | None visible in render action |
| `/AdministrationPanel/Audit_Template` | `audit_template.cshtml` | `audit_template()` | None | None | Render only | None visible in render action |
| `/AdministrationPanel/entity_relationship` | `entity_relationship.cshtml` | `entity_relationship()` | `Getrealtionshiptype(pageId)` | `pkg_ad.P_Getrealtionshiptype` | Read SP | SP only / not directly visible in code |
| `/AdministrationPanel/Memo_Status` | `memo_status.cshtml` | `memo_status()` | None | None | Render only | None visible in render action |
| `/PostCompliance/change_status_new_Para` | `change_status_new_Para.cshtml` | `change_status_new_Para()` | `GetCurrentParasEntitiesForStatusChange()` | `pkg_hd.P_GetEntitiesForNewPara` | Mixed | SP only / not directly visible in code |
| `/PostCompliance/change_status_new_Para_authorize` | `change_status_new_Para_authorize.cshtml` | `change_status_new_Para_authorize()` | None | None | Mixed | None visible in render action |
| `/PostCompliance/change_para_status` | `change_para_status.cshtml` | `change_para_status()` | `Getrealtionshiptype(pageId)` | `pkg_ad.P_Getrealtionshiptype` | Mixed | SP only / not directly visible in code |
| `/PostCompliance/change_para_status_authorize` | `change_para_status_authorize.cshtml` | `change_para_status_authorize()` | None | None | Mixed | None visible in render action |
| `/HM/dashboard` | `dashboard.cshtml` | `dashboard()` | `GetActiveInactiveChartData()` | `pkg_rpt.P_GetActiveInactiveChartData` | Read SP | SP only / not directly visible in code |

## Archive-Only AJAX Or API Routes Now Routed Through `DBConnectionArchive`

These routes were confirmed as archive-only from current JS usage and were switched to use `DBConnectionArchive` without changing their URL or action name.

| Used By Archived View(s) | Route | Controller Action | DBConnectionArchive Method | Stored Procedure | Classification | DB Table Mapping |
| --- | --- | --- | --- | --- | --- | --- |
| `old_outstanding_paras.cshtml` | `/ApiCalls/get_assigned_observation_old_paras` | `get_assigned_observation_old_paras(int ENTITY_ID = 0)` | `GetAuditeeOldParas()` | `pkg_ae.P_GetAuditeeOldParas` | Read SP | SP only / not directly visible in code |
| `old_para_reply_cad.cshtml` | `/ApiCalls/get_old_para_management` | `get_old_para_management()` | `GetOldParasManagement()` | `pkg_ae.P_GetOldParaManagement` | Read SP | SP only / not directly visible in code |
| `old_para_reply_cad.cshtml` | `/ApiCalls/add_legacy_para_cad_reply` | `add_legacy_para_cad_reply(...)` | `AddOldParasCADReply(...)` | `pkg_ae.P_updateoldparamanagement` | Save/Update SP | SP only / not directly visible in code |
| `old_para_reply_cad.cshtml` | `/ApiCalls/add_legacy_para_cad_compliance` | `add_legacy_para_cad_compliance(...)` | `AddOldParasCADCompliance(...)` | `pkg_ae.P_UpdateAuditeeOldParasresponse` | Save/Update SP | SP only / not directly visible in code |
| `Para_Text_Update_FAD.cshtml` | `/ApiCalls/get_old_para_br_compliance_text_update` | `get_old_para_br_compliance_text_update()` | `GetOldParasBranchComplianceTextupdate()` | `pkg_ae.P_GetAuditeeAllParasFAD` | Read SP | SP only / not directly visible in code |
| `reporting_wise_obs.cshtml` | `/ApiCalls/get_reporting_wise_observations` | `get_reporting_wise_observations()` | `GetReportingOfficeWiseObservations()` | `pkg_db.P_Functional_Reporting_office_WISE_ANALYSIS` | Read SP | SP only / not directly visible in code |
| `no_entities_risk_based_planning.cshtml`, `no_entities_risk_based_planning_b.cshtml` | `/ApiCalls/get_risk_base_plan_for_dashboard` | `get_risk_base_plan_for_dashboard()` | `GetEntitiesRiskBasePlanForDashboard()` | `pkg_db.p_get_risk_baseplan` | Read SP | SP only / not directly visible in code |
| `functional_resp_wise_paras.cshtml`, `functional_resp_wise_paras_ho.cshtml` | `/ApiCalls/get_functional_responsibility_wise_paras_for_dashboard` | `get_functional_responsibility_wise_paras_for_dashboard(...)` | `GetFunctionalResponsibilityWiseParaForDashboard(...)` | `pkg_db.P_GET_Dash_table_functionwise` | Read SP | SP only / not directly visible in code |
| `functional_resp_wise_paras_ho.cshtml` | `/ApiCalls/get_functional_responsibility_wise_paras_for_dashboard_ho` | `get_functional_responsibility_wise_paras_for_dashboard_ho(...)` | `GetHOFunctionalResponsibilityWiseParaForDashboard(...)` | `pkg_db.P_GET_Dash_table_functionwise_ho` | Read SP | SP only / not directly visible in code |
| `functional_resp_wise_paras_ho.cshtml` | `/ApiCalls/get_violation_area_for_functional_responsibility_wise_paras_ho` | `get_violation_area_for_functional_responsibility_wise_paras_ho(...)` | `GetHOViolationListForDashboard(...)` | `pkg_db.P_GET_Dash_table_functionwise_names_checklist_ho` | Read SP | SP only / not directly visible in code |
| `functional_resp_wise_paras_ho.cshtml` | `/ApiCalls/get_sub_violation_area_for_functional_responsibility_wise_paras_ho` | `get_sub_violation_area_for_functional_responsibility_wise_paras_ho(...)` | `GetHOSubViolationListForDashboard(...)` | `pkg_db.P_GET_Dash_table_functionwise_names_checklist_sub_ho` | Read SP | SP only / not directly visible in code |
| `para_view.cshtml` | `/ApiCalls/get_functional_observations` | `get_functional_observations(int ANNEX_ID, int ENTITY_ID)` | `GetFunctionalObservations(...)` | `pkg_db.P_Function_Annexure_Paras` | Read SP | SP only / not directly visible in code |
| `Add_Legacy_Para.cshtml` | `/ApiCalls/get_auditee_entities_by_entity_type_id` | `get_auditee_entities_by_entity_type_id(int ENTITY_TYPE_ID)` | `GetAuditEntitiesByTypeId(...)` | `pkg_hd.P_GetAuditEntities` | Read SP | SP only / not directly visible in code |
| `Add_Legacy_Para.cshtml` | `/ApiCalls/add_new_legacy_para` | `add_new_legacy_para(AddNewLegacyParaModel LEGACY_PARA)` | `AddNewLegacyPara(...)` | `pkg_hd.P_add_legacy_Para` | Save/Update SP | SP only / not directly visible in code |
| `Authorize_Adding_Legacy_Para.cshtml` | `/ApiCalls/get_add_legacy_paras_autorize` | `get_add_legacy_paras_autorize()` | `GetAddedLegacyParaForAuthorize()` | `pkg_hd.P_get_legacy_para_to_authorize` | Read SP | SP only / not directly visible in code |
| `Authorize_Adding_Legacy_Para.cshtml` | `/ApiCalls/Authorize_Legacy_Para_addition` | `Authorize_Legacy_Para_addition(string PARA_REF)` | `AuthorizeLegacyParaAddition(...)` | `pkg_hd.P_Authorize_legacy_para_addition` | Save/Update SP | SP only / not directly visible in code |
| `Authorize_Adding_Legacy_Para.cshtml` | `/ApiCalls/Delete_Legacy_Para_addition_request` | `Delete_Legacy_Para_addition_request(string PARA_REF)` | `DeleteLegacyParaAdditionRequest(...)` | `pkg_hd.P_referedback_Del_para` | Save/Update SP | SP only / not directly visible in code |
| `Authorize_Change_Settle_Para_Status.cshtml` | `/ApiCalls/get_legacy_settled_paras_autorize` | `get_legacy_settled_paras_autorize()` | `GetOldSettledParasForResponseAuthorize()` | `pkg_fad.p_getoldparasforresponseauthorize` | Read SP | SP only / not directly visible in code |
| `Authorize_Change_Settle_Para_Status.cshtml` | `/ApiCalls/Add_Authorization_Old_Para_Change_status` | `Add_Authorization_Old_Para_Change_status(...)` | `AddAuthorizeChangeStatusRequestForSettledPara(...)` | `pkg_fad.p_authorizechangestatusrequestforsettledpara` | Save/Update SP | SP only / not directly visible in code |
| `Authorize_Update_Legacy_Para_Gist_Para.cshtml` | `/ApiCalls/get_update_gist_paraNo_legacy_paras_autorize` | `get_update_gist_paraNo_legacy_paras_autorize()` | `GetUpdatedGistParaOfLegacyParaForAuthorize()` | `pkg_ar.P_get_legacy_para_to_authorize` | Read SP | SP only / not directly visible in code |
| `Authorize_Update_Legacy_Para_Gist_Para.cshtml` | `/ApiCalls/Authorize_Legacy_Para_Gist_ParaNo` | `Authorize_Legacy_Para_Gist_ParaNo(...)` | `AuthorizeLegacyParaGistParaNoUpdate(...)` | `pkg_ar.P_Authorize_Para_Gist` | Save/Update SP | SP only / not directly visible in code |
| `auth_del_dup_para.cshtml` | `/ApiCalls/get_duplicate_paras_for_authorize` | `get_duplicate_paras_for_authorize()` | `GetDuplicateParasForAuthorization()` | `pkg_hd.P_GET_DUPLICATE_PARAS_FOR_AUTH` | Read SP | SP only / not directly visible in code |
| `auth_del_dup_para.cshtml` | `/ApiCalls/reject_delete_duplicate_para` | `reject_delete_duplicate_para(int D_ID = 0)` | `RejectDeleteDuplicatePara(...)` | `pkg_hd.P_REJECT_DUPLICATE_PARAS` | Save/Update SP | SP only / not directly visible in code |
| `auth_del_dup_para.cshtml` | `/ApiCalls/authorize_delete_duplicate_para` | `authorize_delete_duplicate_para(int D_ID = 0)` | `AuthDeleteDuplicatePara(...)` | `pkg_hd.P_AUTH_DUPLICATE_PARAS` | Save/Update SP | SP only / not directly visible in code |
| `branch_deposit_info.cshtml` | `/ApiCalls/GetDepositAccountSubdetails` | `GetDepositAccountSubdetails(string b_name)` | `GetDepositAccountSubdetails(...)` | `pkg_ai.P_GetDepositAccountSubdetails` | Read SP | SP only / not directly visible in code |
| `deposit_account_details.cshtml` | `/ApiCalls/GetDepositAccountcatdetails` | `GetDepositAccountcatdetails(int catid)` | `GetDepositAccountcatdetails(...)` | `pkg_AIS.P_GetDepositACCOUNTCATEGORY_details` | Read SP | SP only / not directly visible in code |
| `Reports/income_expenditure` | `/ApiCalls/GetIncomeExpenceDetails` | `GetIncomeExpenceDetails(int b_id, int ENG_ID)` | `GetIncomeExpenceDetails(...)` | `pkg_ai.P_GetIncomeExpenceDetails` | Read SP | SP only / not directly visible in code |
| `change_status_new_Para_authorize.cshtml` | `/ApiCalls/get_current_paras_for_status_change_request_authorize` | `get_current_paras_for_status_change_request_authorize()` | `GetCurrentParasForStatusChangeRequestAuthorize()` | `pkg_fad.p_GetnewParasForResponseAuthorize` | Read SP | SP only / not directly visible in code |
| `change_status_new_Para.cshtml` | `/ApiCalls/Add_New_Para_Change_status_Request` | `Add_New_Para_Change_status_Request(...)` | `AddChangeStatusRequestForCurrentPara(...)` | `pkg_hd.P_ChangeStatusRequestForSettledPara_new` | Save/Update SP | SP only / not directly visible in code |
| `change_status_new_Para_authorize.cshtml` | `/ApiCalls/Add_Old_Para_Change_status_Authorize` | `Add_Old_Para_Change_status_Authorize(...)` | `AuthorizerAddChangeStatusRequestForSettledPara(...)` remained on active `DBConnection` because that path is also used outside the archive surface | `pkg_fad.P_AuthorizeChangeStatusRequestForSettledPara_new` | Save/Update SP | SP only / not directly visible in code |
| `change_para_status.cshtml`, `change_para_status_authorize.cshtml` | `/ApiCalls/get_paras_for_status_change` | `get_paras_for_status_change(int ENTITY_ID = 0)` | `GetParasForStatusChange(...)` | `pkg_hd.P_Get_Paras_For_Status_Change` | Read SP | SP only / not directly visible in code |
| `change_para_status.cshtml` | `/ApiCalls/Add_Para_Change_status_Request` | `Add_Para_Change_status_Request(...)` | `AddChangeStatusRequestForPara(...)` | `pkg_hd.P_Add_Paras_For_Status_Change` | Save/Update SP | SP only / not directly visible in code |
| `change_para_status_authorize.cshtml` | `/ApiCalls/get_paras_for_status_change_authorize` | `get_paras_for_status_change_authorize()` | `GetParasForStatusChangeToAuthorize()` | `pkg_hd.P_Get_Paras_For_Status_Change_For_Authorize` | Read SP | SP only / not directly visible in code |
| `change_para_status_authorize.cshtml` | `/ApiCalls/authorize_para_change_status` | `authorize_para_change_status(...)` | `AuthorizeChangeStatusRequestForPara(...)` | `pkg_hd.P_Authorize_Paras_For_Status` | Save/Update SP | SP only / not directly visible in code |
| `dashboard.cshtml` | `/ApiCalls/get_pie_chart_data` | `get_pie_chart_data()` | `GetActiveInactiveChartData()` | `pkg_rpt.P_GetActiveInactiveChartData` | Read SP | SP only / not directly visible in code |

## Shared Routes Still Used By Archived Views And Intentionally Left In Existing Controllers

These routes are still used by archived JS, but they are also shared with active modules or remain in non-archive controllers by design. They were not renamed and were not moved out of their existing controllers in this refactor.

| Used By Archived View(s) | Route | Current Controller Action | DBConnection Method | Stored Procedure | Classification | DB Table Mapping |
| --- | --- | --- | --- | --- | --- | --- |
| `old_para_reply_cad.cshtml`, `Audit_Execution.cshtml`, `Update_Legacy_Paras.cshtml`, `Update_Legacy_Paras_HO.cshtml`, `audit_template.cshtml` | `/Execution/risk_activities` | `ExecutionController.risk_activities(RiskSubGroupModel)` | `GetRiskActivities(int)` | `pkg_ar.p_GetRiskActivities` | Read SP | SP only / not directly visible in code |
| `old_para_reply_cad.cshtml`, `Audit_Execution.cshtml`, `Update_Legacy_Paras.cshtml`, `Update_Legacy_Paras_HO.cshtml` | `/Execution/sub_voilation` | `ExecutionController.sub_voilation(AuditSubVoilationcatModel)` | `GetVoilationSubGroup(int)` | `pkg_ar.P_GetVoilationSubGroup` | Read SP | SP only / not directly visible in code |
| `para_view.cshtml` | `/ApiCalls/getparentrelForDashboardPanel` | `ApiCallsController.getparentrelForDashboardPanel(int)` | `GetparentrepofficeForDashboardPanel(int)` | `pkg_db.P_Getparentrepoffice` | Read SP | SP only / not directly visible in code |
| `para_view.cshtml` | `/ApiCalls/getpostplaceForDashboardPanel` | `ApiCallsController.getpostplaceForDashboardPanel(int)` | `GetchildpostingForDashboardPanel(int)` | `pkg_db.P_Getchildposting` | Read SP | SP only / not directly visible in code |
| `para_view.cshtml`, `change_para_status.cshtml` | `/ApiCalls/get_all_para_text` | `ApiCallsController.get_all_para_text(int)` | `GetAllParaText(int)` | `pkg_hd.P_GET_ALL_PARA_TEXT` | Read SP | SP only / not directly visible in code |
| `functional_resp_wise_paras.cshtml` | `/ApiCalls/get_functional_observation_text` | `ApiCallsController.get_functional_observation_text(int,string)` | `GetFunctionalObservationText(int,string)` | `pkg_db.P_Function_Annexure_Paras_text` | Read SP | SP only / not directly visible in code |
| `functional_resp_wise_paras_ho.cshtml` | `/ApiCalls/get_functional_owner_area_for_functional_responsibility_wise_paras_ho` | `ApiCallsController.get_functional_owner_area_for_functional_responsibility_wise_paras_ho(int)` | `GetHOFunctionalListForDashboard(int)` | `pkg_db.P_GET_Dash_table_functionwise_names_ho` | Read SP | SP only / not directly visible in code |
| `Audit_Execution.cshtml` | `/ApiCalls/save_observations` | `ApiCallsController.save_observations([FromBody] SaveObservationRequest)` | Shared CAU/Execution save flow on active `DBConnection` | Shared observation save path, not archived here | Save/Update SP | Shared flow, table mapping not isolated in this archive pass |
| `change_status_new_Para.cshtml` | `/ApiCalls/get_current_paras_for_status_change_request` | `ApiCallsController.get_current_paras_for_status_change_request(int)` | `GetCurrentParasForStatusChangeRequest(int)` | `pkg_hd.P_GetnewParasForResponse` | Read SP | SP only / not directly visible in code |
| `change_status_new_Para.cshtml`, `change_status_new_Para_authorize.cshtml` | `/ApiCalls/GetIASPARATEXT` | `ApiCallsController.GetIASPARATEXT(int)` | `GetIASParaText(int)` | `pkg_FAD.P_GET_IAS_PARA_TEXT` | Read SP | SP only / not directly visible in code |
| `change_para_status.cshtml` | `/ApiCalls/getparentrel` | `ApiCallsController.getparentrel(int)` | `Getparentrepoffice(int)` | `pkg_ad.P_Getparentrepoffice` | Read SP | SP only / not directly visible in code |
| `change_para_status.cshtml` | `/ApiCalls/getpostplace` | `ApiCallsController.getpostplace(int)` | `Getchildposting(int)` | `pkg_ad.P_Getchildposting` | Read SP | SP only / not directly visible in code |

## Archived Views With No Dedicated Archive Data Surface Beyond Rendering

These views were archived as files and render actions, but no dedicated archive-only stored-procedure path was found in their current page JS.

- `Reply.cshtml`
- `reporting_wise_obs.cshtml` render is controller-only; the page data load itself is covered above
- `pre_audit_info.cshtml`
- `loan_case_details.cshtml`
- `branch_deposit_info.cshtml` render-only; data API is covered above
- `deposit_account_details.cshtml` render-only; data API is covered above
- `income_expenditure.cshtml` render-only; data API is covered above
- `audit_observation_text.cshtml`
- `audit_template.cshtml`
- `memo_status.cshtml`
- `change_status_new_Para_authorize.cshtml` render-only; data APIs are covered above
- `change_para_status_authorize.cshtml` render-only; data APIs are covered above

## Page Registry Migration

- Created: `AIS\wwwroot\Images\Page ID - Archive.csv`
- Removed matching archive-page rows from: `AIS\wwwroot\Images\Page ID.xlsx`
- `PAGE_ID` values were preserved exactly for all migrated rows
- The archive CSV uses the same column structure as the active workbook source

