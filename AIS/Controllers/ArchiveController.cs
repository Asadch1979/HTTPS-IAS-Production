using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AIS.Controllers
    {
    public class ArchiveController : Controller
        {
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly DBConnectionArchive archiveDbConnection;

        public ArchiveController(
            SessionHandler sessionHandler,
            DBConnectionArchive archiveDbConnection,
            TopMenus topMenus)
            {
            this.sessionHandler = sessionHandler;
            this.archiveDbConnection = archiveDbConnection;
            tm = topMenus;
            }

        [HttpGet("/AuditeePortal/Para_Text_Update_FAD")]
        public IActionResult Para_Text_Update_FAD()
            {
            return RenderArchiveView("Para_Text_Update_FAD", () =>
            {
                ViewData["EntitiesList"] = archiveDbConnection.GetAuditeeAssignedEntities();
            });
            }

        [HttpGet("/AuditeePortal/old_outstanding_paras")]
        public IActionResult old_outstanding_paras()
            {
            return RenderArchiveView("old_outstanding_paras", () =>
            {
                ViewData["OldParasEntities"] = archiveDbConnection.GetAuditeeOldParasEntities();
            });
            }

        [HttpGet("/AuditeePortal/old_para_reply_cad")]
        public IActionResult old_para_reply_cad()
            {
            return RenderArchiveView("old_para_reply_cad", () =>
            {
                ViewData["Voilation_Cat"] = archiveDbConnection.GetAuditVoilationcats();
                ViewData["RiskList"] = archiveDbConnection.GetRisks();
            });
            }

        [HttpGet("/AuditeePortal/Reply")]
        public IActionResult reply()
            {
            return RenderArchiveView("Reply");
            }

        [HttpGet("/Dashboard/functional_resp_wise_paras")]
        public IActionResult functional_resp_wise_paras()
            {
            return RenderArchiveView("functional_resp_wise_paras", () =>
            {
                ViewData["FunctionalList"] = archiveDbConnection.GetFunctionalListForDashboard();
            });
            }

        [HttpGet("/Dashboard/functional_resp_wise_paras_ho")]
        public IActionResult functional_resp_wise_paras_ho()
            {
            return RenderArchiveView("functional_resp_wise_paras_ho");
            }

        [HttpGet("/dashboard/no_entities_risk_based_planning")]
        public IActionResult no_entities_risk_based_planning()
            {
            return RenderArchiveView("no_entities_risk_based_planning", () =>
            {
                ViewData["ProcessList"] = archiveDbConnection.GetRiskProcessDefinition();
            });
            }

        [HttpGet("/dashboard/no_entities_risk_based_planning_b")]
        public IActionResult no_entities_risk_based_planning_b()
            {
            return RenderArchiveView("no_entities_risk_based_planning_b", () =>
            {
                ViewData["ProcessList"] = archiveDbConnection.GetRiskProcessDefinition();
            });
            }

        [HttpGet("/Dashboard/reporting_wise_obs")]
        public IActionResult reporting_wise_obs()
            {
            return RenderArchiveView("reporting_wise_obs");
            }

        [HttpGet("/Dashboard/para_view")]
        public IActionResult para_view()
            {
            return RenderArchiveView("para_view", () =>
            {
                ViewData["AnnexList"] = archiveDbConnection.GetAnnexuresForChecklistDetail();
                ViewData["Userrelationship"] = archiveDbConnection.GetrealtionshiptypeForDashboardPanel();
            });
            }

        [HttpGet("/Execution/obs_management")]
        public IActionResult obs_management(int engId = 0)
            {
            return RenderArchiveView("obs_management", () =>
            {
                ViewData["RiskList"] = archiveDbConnection.GetRisks();
                ViewData["EntitiesList"] = archiveDbConnection.GetEntitiesDropDownForManageObservations();
            });
            }

        [HttpGet("/Execution/Settled_Para")]
        public IActionResult Settled_Para()
            {
            return RenderArchiveView("Settled_Para", () =>
            {
                ViewData["EntitiesList"] = archiveDbConnection.GetLegacyParasEntities();
            });
            }

        [HttpGet("/Execution/Authorize_Change_Settle_Para_Status")]
        public IActionResult Authorize_Change_Settle_Para_Status()
            {
            return RenderArchiveView("Authorize_Change_Settle_Para_Status");
            }

        [HttpGet("/Execution/Authorize_Update_Legacy_Para_Gist_Para")]
        public IActionResult Authorize_Update_Legacy_Para_Gist_Para()
            {
            return RenderArchiveView("Authorize_Update_Legacy_Para_Gist_Para");
            }

        [HttpGet("/Execution/staff_pos_info")]
        public IActionResult staff_pos_info()
            {
            return RenderArchiveView("staff_pos_info", () =>
            {
                ViewData["staffpos"] = archiveDbConnection.GetStaffPosition();
            });
            }

        [HttpGet("/Execution/loan_case_document")]
        public IActionResult loan_case_document()
            {
            return RenderArchiveView("loan_case_document", () =>
            {
                ViewData["AuditDepartments"] = archiveDbConnection.GetDepartments(354);
                ViewData["DivisionsList"] = archiveDbConnection.GetDivisions(false);
                ViewData["AuditZonesList"] = archiveDbConnection.GetZones();
            });
            }

        [HttpGet("/Execution/auth_del_dup_para")]
        public IActionResult auth_del_dup_para()
            {
            return RenderArchiveView("auth_del_dup_para", () =>
            {
                ViewData["DupParasEntList"] = archiveDbConnection.GetDuplicateParasAuthorizationEntityList();
            });
            }

        [HttpGet("/Execution/audit_execution")]
        public IActionResult audit_execution()
            {
            return RenderArchiveView("Audit_Execution", () =>
            {
                ViewData["DivisionList"] = archiveDbConnection.GetDivisions(false);
                ViewData["ProcessList"] = archiveDbConnection.GetRiskProcessDefinition();
                ViewData["Voilation_Cat"] = archiveDbConnection.GetAuditVoilationcats();
                ViewData["OtherEntityList"] = archiveDbConnection.GetAuditEntitiesForOtherEntitySelection();
                ViewData["RiskList"] = archiveDbConnection.GetRisks();
            });
            }

        [HttpGet("/Execution/pre_audit_info")]
        public IActionResult pre_audit_info()
            {
            return RenderArchiveView("pre_audit_info");
            }

        [HttpGet("/Execution/pre_audit_info_detail")]
        public IActionResult pre_audit_info_detail()
            {
            return RenderArchiveView("pre_audit_info_detail", () =>
            {
                ViewData["DivisionList"] = archiveDbConnection.GetDivisions(false);
                ViewData["ProcessList"] = archiveDbConnection.GetRiskProcessDefinition();
                ViewData["Voilation_Cat"] = archiveDbConnection.GetAuditVoilationcats();
                ViewData["RiskList"] = archiveDbConnection.GetRisks();
            });
            }

        [HttpGet("/Execution/loan_case_details")]
        public IActionResult loan_case_details()
            {
            return RenderArchiveView("loan_case_details");
            }

        [HttpGet("/Execution/branch_deposit_info")]
        public IActionResult branch_deposit_info()
            {
            return RenderArchiveView("branch_deposit_info");
            }

        [HttpGet("/Execution/deposit_account")]
        public IActionResult deposit_account()
            {
            return RenderArchiveView("deposit_account", () =>
            {
                ViewData["Depositcat"] = archiveDbConnection.GetDepositCat();
            });
            }

        [HttpGet("/Execution/loan_scheme")]
        public IActionResult loan_scheme(int engId)
            {
            return RenderArchiveView("loan_scheme", () =>
            {
                ViewData["GetLoanScheme"] = archiveDbConnection.GetLoansScheme(engId);
            });
            }

        [HttpGet("/Execution/loan_scheme_yearly")]
        public IActionResult loan_scheme_yearly(int engId)
            {
            return RenderArchiveView("loan_scheme_yearly", () =>
            {
                ViewData["GetLoanScheme"] = archiveDbConnection.GetLoansSchemeYearly(engId);
            });
            }

        [HttpGet("/Execution/Add_Legacy_Para")]
        public IActionResult Add_Legacy_Para()
            {
            return RenderArchiveView("Add_Legacy_Para", () =>
            {
                ViewData["EntitiesTypeList"] = archiveDbConnection.GetAuditeeEntitiesType();
                ViewData["AuditYearList"] = archiveDbConnection.GetAuditYearForAddLegacyPara();
                ViewData["AuditNatureList"] = archiveDbConnection.GetAuditNatureForAddLegacyPara();
            });
            }

        [HttpGet("/Execution/Authorize_Adding_Legacy_Para")]
        public IActionResult Authorize_Adding_Legacy_Para()
            {
            return RenderArchiveView("Authorize_Adding_Legacy_Para");
            }

        [HttpGet("/Execution/Update_Gist_Para_No")]
        public IActionResult Update_Gist_Para_No()
            {
            return RenderArchiveView("Update_Gist_Para_No", () =>
            {
                ViewData["EntitiesList"] = archiveDbConnection.GetLegacyParasEntities();
            });
            }

        [HttpGet("/Execution/Update_Legacy_Paras")]
        public IActionResult Update_Legacy_Paras()
            {
            return RenderArchiveView("Update_Legacy_Paras", () =>
            {
                ViewData["EntitiesList"] = archiveDbConnection.GetLegacyParasEntities();
                ViewData["ProcessList"] = archiveDbConnection.GetAuditChecklistCAD();
                ViewData["Voilation_Cat"] = archiveDbConnection.GetAuditVoilationcats();
                ViewData["RiskList"] = archiveDbConnection.GetRisks();
            });
            }

        [HttpGet("/Execution/Update_Legacy_Paras_HO")]
        public IActionResult Update_Legacy_Paras_HO()
            {
            return RenderArchiveView("Update_Legacy_Paras_HO", () =>
            {
                ViewData["EntitiesList"] = archiveDbConnection.GetLegacyParasEntitiesHO();
                ViewData["Voilation_Cat"] = archiveDbConnection.GetAuditVoilationcats();
                ViewData["RiskList"] = archiveDbConnection.GetRisks();
            });
            }

        [HttpGet("/Execution/Update_Legacy_Paras_FAD")]
        public IActionResult Update_Legacy_Paras_FAD()
            {
            return RenderArchiveView("Update_Legacy_Paras_FAD", () =>
            {
                ViewData["EntitiesList"] = archiveDbConnection.GetLegacyParasEntitiesFAD();
                ViewData["ProcessList"] = archiveDbConnection.GetAuditChecklistCAD();
                ViewData["RiskList"] = archiveDbConnection.GetRisks();
            });
            }

        [HttpGet("/Execution/deposit_account_details")]
        public IActionResult deposit_account_details()
            {
            return RenderArchiveView("deposit_account_details");
            }

        [HttpGet("/Execution/disb_info")]
        public IActionResult disb_info()
            {
            return RenderArchiveView("disb_info", () =>
            {
                ViewData["DivisionList"] = archiveDbConnection.GetDivisions(false);
                ViewData["ProcessList"] = archiveDbConnection.GetRiskProcessDefinition();
                ViewData["Voilation_Cat"] = archiveDbConnection.GetAuditVoilationcats();
                ViewData["RiskList"] = archiveDbConnection.GetRisks();
            });
            }

        [HttpGet("/Reports/glhead_summary")]
        public IActionResult glhead_summary(int engId)
            {
            return RenderArchiveView("glhead_summary", () =>
            {
                ViewData["GlHeadDetails"] = archiveDbConnection.GetGlheadDetails(engId);
            });
            }

        [HttpGet("/Reports/income_expenditure")]
        public IActionResult income_expenditure()
            {
            return RenderArchiveView("income_expenditure");
            }

        [HttpGet("/Setup/control_violation")]
        public IActionResult control_violation()
            {
            return RenderArchiveView("control_violation", () =>
            {
                ViewData["ControlViolationList"] = archiveDbConnection.GetControlViolations();
            });
            }

        [HttpGet("/AdministrationPanel/Audit_Observation_text")]
        public IActionResult audit_observation_text()
            {
            return RenderArchiveView("audit_observation_text");
            }

        [HttpGet("/AdministrationPanel/Audit_Template")]
        public IActionResult audit_template()
            {
            return RenderArchiveView("audit_template");
            }

        [HttpGet("/AdministrationPanel/entity_relationship")]
        public IActionResult entity_relationship()
            {
            return RenderArchiveView("entity_relationship", () =>
            {
                ViewData["Userrelationship"] = archiveDbConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            });
            }

        [HttpGet("/AdministrationPanel/Memo_Status")]
        public IActionResult memo_status()
            {
            return RenderArchiveView("memo_status");
            }

        [HttpGet("/PostCompliance/change_status_new_Para")]
        public IActionResult change_status_new_Para()
            {
            return RenderArchiveView("change_status_new_Para", () =>
            {
                ViewData["EntitiesList"] = archiveDbConnection.GetCurrentParasEntitiesForStatusChange();
            });
            }

        [HttpGet("/PostCompliance/change_status_new_Para_authorize")]
        public IActionResult change_status_new_Para_authorize()
            {
            return RenderArchiveView("change_status_new_Para_authorize");
            }

        [HttpGet("/PostCompliance/change_para_status")]
        public IActionResult change_para_status()
            {
            return RenderArchiveView("change_para_status", () =>
            {
                ViewData["Userrelationship"] = archiveDbConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            });
            }

        [HttpGet("/PostCompliance/change_para_status_authorize")]
        public IActionResult change_para_status_authorize()
            {
            return RenderArchiveView("change_para_status_authorize");
            }

        [HttpGet("/HM/dashboard")]
        public IActionResult dashboard()
            {
            return RenderArchiveView("dashboard");
            }

        private IActionResult RenderArchiveView(string viewName, Action prepareViewData = null)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            prepareViewData?.Invoke();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!this.UserHasPagePermissionForCurrentAction(sessionHandler))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            return View($"~/Views/Archive/{viewName}.cshtml");
            }
        }
    }
