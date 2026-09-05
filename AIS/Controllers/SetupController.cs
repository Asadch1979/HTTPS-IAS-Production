using AIS.Models;
using AIS.Models.WorkflowDashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AIS.Services;


namespace AIS.Controllers
    {

    public class SetupController : Controller
        {
        private readonly ILogger<SetupController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;
        private readonly DBConnection dBConnection;
        public SetupController(ILogger<SetupController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService, IPageIdResolver pageIdResolver)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _pageIdResolver = pageIdResolver;
            }

        [HttpGet]
        public IActionResult Checklist_Dashboard(string stepKey = null)
            {
            PopulateDashboardChrome();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!HasPageAccess(user, "/Setup/Checklist_Dashboard"))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            var model = BuildChecklistDashboardViewModel(user, stepKey);
            if (!model.VisibleSteps.Any())
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            return View("~/Views/Setup/Checklist_Dashboard.cshtml", model);
            }

        [HttpGet]
        public IActionResult LoadChecklistDashboardStep(string stepKey)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized();
                }

            if (!TryGetRequestedStep(BuildChecklistDashboardViewModel(user, stepKey), stepKey, out var step, out var errorResult))
                {
                if (errorResult is ForbidResult)
                    {
                    return CreateStepAccessDeniedResult();
                    }

                return errorResult;
                }

            PopulateChecklistDashboardStepViewData(step.StepKey, step.RequiredPermissionPageId);
            return PartialView(step.PartialViewName);
            }
        public IActionResult branches()
            {

            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["BranchList"] = dBConnection.GetBranches();
            ViewData["ZoneList"] = dBConnection.GetZones();
            ViewData["BranchSizeList"] = dBConnection.GetBranchSizes();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult manage_audit_zone_branches()
            {

            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["BranchList"] = dBConnection.GetBranches();
            ViewData["ZoneList"] = dBConnection.GetAuditZones();
            ViewData["BranchSizeList"] = dBConnection.GetBranchSizes();
            ViewData["AuditZonesList"] = dBConnection.GetZones();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult manage_Checklist()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ChecklistTypes"] = dBConnection.GetAuditChecklist();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult manage_sub_Checklist()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ChecklistTypes"] = dBConnection.GetAuditChecklist();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        //engagement_shifting

        public IActionResult para_migration()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            var pageId = ViewData["PageId"] as int? ?? 0;
            ViewData["AuditEntitiesType"] = pageId > 0 ? dBConnection.GetAuditEntityTypes(pageId) : new List<AuditEntitiesModel>();
            ViewData["RelationshipList"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            ViewData["Audit_By"] = dBConnection.GetAuditBy();


            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult manage_checklist_detail()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();


            ViewData["ChecklistTypes"] = dBConnection.GetAuditChecklist();
            ViewData["ViolationsList"] = dBConnection.GetViolationsForChecklistDetail();
            ViewData["ProcOwnerList"] = dBConnection.GetProcOwnerForChecklistDetail();
            ViewData["RoleRespList"] = dBConnection.GetRoleResponsibleForChecklistDetail();
            ViewData["AnnexList"] = dBConnection.GetAnnexuresForChecklistDetail();
            ViewData["RiskList"] = dBConnection.GetRisks();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult remove_duplicate_process()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ProcessList"] = dBConnection.GetAuditChecklist();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult remove_duplicate_sub_process()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ProcessList"] = dBConnection.GetAuditChecklist();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult remove_duplicate_checklists()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ProcessList"] = dBConnection.GetAuditChecklist();
            ViewData["checkListDetailsList"] = dBConnection.SearchChecklistDetails();
            ViewData["ViolationsList"] = dBConnection.GetViolationsForChecklistDetail();
            ViewData["ProcOwnerList"] = dBConnection.GetProcOwnerForChecklistDetail();
            ViewData["RoleRespList"] = dBConnection.GetRoleResponsibleForChecklistDetail();
            ViewData["AnnexList"] = dBConnection.GetAnnexuresForChecklistDetail();
            ViewData["RiskList"] = dBConnection.GetRisks();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult authorize_remove_duplicate_process()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ProcessList"] = dBConnection.GetAuditChecklist();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult authorize_remove_duplicate_sub_process()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            //ViewData["ProcessList"] = dBConnection.GetAuditChecklist();
            ViewData["ProcessList"] = dBConnection.GetAuthorizeMergeSubChecklist();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult authorize_remove_duplicate_checklists()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ProcessList"] = dBConnection.GetAuditProcessListForMergeDuplicate();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult ref_checklist_detail()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ChecklistTypes"] = dBConnection.GetAuditChecklist();
            ViewData["ViolationsList"] = dBConnection.GetViolationsForChecklistDetail();
            ViewData["ProcOwnerList"] = dBConnection.GetProcOwnerForChecklistDetail();
            ViewData["RoleRespList"] = dBConnection.GetRoleResponsibleForChecklistDetail();
            ViewData["AnnexList"] = dBConnection.GetAnnexuresForChecklistDetail();
            ViewData["RiskList"] = dBConnection.GetRisks();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult manage_inspection_unit_branches()
            {

            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["BranchList"] = dBConnection.GetBranches();
            ViewData["ZoneList"] = dBConnection.GetZones();
            ViewData["BranchSizeList"] = dBConnection.GetBranchSizes();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult processes()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["DivisionList"] = dBConnection.GetDivisions(false);
            ViewData["ProcessList"] = dBConnection.GetRiskProcessDefinition();
            ViewData["AuditableEntityTypes"] = dBConnection.GetAuditEntities();
            ViewData["ControlViolationsList"] = dBConnection.GetControlViolations();
            ViewData["RoleRespList"] = dBConnection.GetRoleResponsibilities();
            ViewData["RiskList"] = dBConnection.GetRisks();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult sub_process_authorize()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["TransactionsList"] = dBConnection.GetUpdatedSubChecklistForReviewAndAuthorize(4);
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult process_detail_review()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ChecklistTypes"] = dBConnection.GetAuditChecklist();
            ViewData["ViolationsList"] = dBConnection.GetViolationsForChecklistDetail();
            ViewData["ProcOwnerList"] = dBConnection.GetProcOwnerForChecklistDetail();
            ViewData["RoleRespList"] = dBConnection.GetRoleResponsibleForChecklistDetail();
            ViewData["AnnexList"] = dBConnection.GetAnnexuresForChecklistDetail();
            ViewData["RiskList"] = dBConnection.GetRisks();

            // status ids required 1, 4 but 4 pass to procedure will bring 1 & 4 both processes
            ViewData["TransactionsList"] = dBConnection.GetUpdatedChecklistDetailsForReviewAndAuthorize(4);
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult process_detail_authorize()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["TransactionsList"] = dBConnection.GetUpdatedChecklistDetailsForReviewAndAuthorize(3);
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult sub_entities()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["SubEntitiesList"] = dBConnection.GetSubEntities();
            ViewData["DivisionList"] = dBConnection.GetDivisions(false);
            ViewData["DepartmentList"] = dBConnection.GetDepartments(0, false);
            return View();
            }

        public IActionResult manage_reporting_offices()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            return View();
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("BRANCH_ADD", "ADMINISTRATION", "ADMINISTRATION", "", "", ObjectType = "BRANCH_ADD")]
        public BranchModel branch_add(BranchModel br)
            {
            if (br.ISACTIVE == "Active")
                br.ISACTIVE = "Y";
            else if (br.ISACTIVE == "InActive")
                br.ISACTIVE = "N";

            if (br.BRANCHID == 0)
                br = dBConnection.AddBranch(br);
            else
                br = dBConnection.UpdateBranch(br);
            return br;
            }

        public IActionResult divisions()
            {

            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["DivisionList"] = dBConnection.GetDivisions(false);
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        [HttpPost]

        public IActionResult departments()
            {

            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["DivisionList"] = dBConnection.GetDivisions(false);
            ViewData["DepartmentList"] = dBConnection.GetDepartments(0, false);
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        [HttpPost]

        public IActionResult audit_zones()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditZoneList"] = dBConnection.GetAuditZones();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult annexure_assignment_para()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ZonesList"] = dBConnection.GetZonesForAnnexureAssignment();
            ViewData["AnnexList"] = dBConnection.GetAnnexuresForChecklistDetail();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult manage_annexure()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ProcList"] = dBConnection.GetAnnexureProcess();
            ViewData["AnnexList"] = dBConnection.GetAnnexuresForChecklistDetail();
            ViewData["ProcOwnerList"] = dBConnection.GetProcOwnerForChecklistDetail();
            ViewData["RiskList"] = dBConnection.GetRisks();
            ViewData["RiskModelList"] = dBConnection.GetRisks();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult compliance_hierarchy()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["complianceUnitList"] = dBConnection.GetComplianceHierarchies();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_CONTROL_VIOLATION", "ADMINISTRATION", "ADMINISTRATION", "", "", ObjectType = "ADD_CONTROL_VIOLATION")]
        public ControlViolationsModel add_control_violation(ControlViolationsModel cv)
            {
            return dBConnection.AddControlViolation(cv);
            }

        [HttpPost]
        public List<DepartmentModel> get_departments(int div_id)
            {
            return dBConnection.GetDepartments(div_id, false);
            }
        [HttpPost]
        public List<SubEntitiesModel> get_sub_entities(int div_id = 0, int dept_id = 0)
            {
            return dBConnection.GetSubEntities(div_id, dept_id);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("ADD_SUB_ENTITY", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_ADDSUBENTITY", ObjectType = "ADD_SUB_ENTITY")]
        public SubEntitiesModel add_sub_entity(SubEntitiesModel entity)
            {
            if (entity.STATUS == "Active")
                entity.STATUS = "Y";
            else
                entity.STATUS = "N";
            if (entity.ID == 0)
                return dBConnection.AddSubEntity(entity);
            else
                return dBConnection.UpdateSubEntity(entity);
            }
        [HttpPost]
        public List<RiskProcessDetails> process_details(int ProcessId)
            {
            return dBConnection.GetRiskProcessDetails(ProcessId);
            }
        [HttpPost]
        public List<RiskProcessTransactions> process_transactions(int ProcessDetailId = 0, int transactionId = 0)
            {
            return dBConnection.GetRiskProcessTransactions(ProcessDetailId, transactionId);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("PROCESS_ADD", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_AUDIT_CHECKLIST", ObjectType = "PROCESS_ADD")]
        public RiskProcessDefinition process_add(RiskProcessDefinition proc)
            {
            return dBConnection.AddRiskProcess(proc);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("SUB_PROCESS_ADD", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_AUDIT_CHECKLIST_SUB", ObjectType = "SUB_PROCESS_ADD")]
        public RiskProcessDetails sub_process_add(RiskProcessDetails subProc)
            {
            return dBConnection.AddRiskSubProcess(subProc);
            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("SUB_PROCESS_TRANSACTION_ADD", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_AUDIT_CHECKLIST_DETAIL", ObjectType = "SUB_PROCESS_TRANSACTION_ADD")]
        public RiskProcessTransactions sub_process_transaction_add(RiskProcessTransactions tran)
            {
            return dBConnection.AddRiskSubProcessTransaction(tran);
            }
        [HttpPost]
        public string recommend_process_transaction_by_reviewer(int T_ID, string COMMENTS, int? PROCESS_DETAIL_ID = null, int? SUB_PROCESS_ID = null, string HEADING = "", int? V_ID = null, int? CONTROL_ID = null, int? ROLE_ID = null, int? RISK_ID = null, string ANNEX_CODE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RecommendProcessTransactionByReviewer(T_ID, COMMENTS, PROCESS_DETAIL_ID.GetValueOrDefault(), SUB_PROCESS_ID.GetValueOrDefault(), HEADING, V_ID.GetValueOrDefault(), CONTROL_ID.GetValueOrDefault(), ROLE_ID.GetValueOrDefault(), RISK_ID.GetValueOrDefault(), ANNEX_CODE) + "\"}";

            }
        [HttpPost]
        public string reffered_back_process_transaction_by_reviewer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RefferedBackProcessTransactionByReviewer(T_ID, COMMENTS) + "\"}";

            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("AUTHORIZE_PROCESS_TRANSACTION_BY_AUTHORIZER", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_APPROVE_CHECKLIST_BY_AUTHORIZER", ObjectType = "AUTHORIZE_PROCESS_TRANSACTION_BY_AUTHORIZER", ObjectId = "T_ID", RequireResultMessage = true)]
        public string authorize_process_transaction_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeProcessTransactionByAuthorizer(T_ID, COMMENTS) + "\"}";

            }
        [HttpPost]
        [AIS.Filters.ApplicationAudit("REFFERED_BACK_PROCESS_TRANSACTION_BY_AUTHORIZER", "ADMINISTRATION", "ADMINISTRATION", "PKG_AD", "P_REFFEREDBACK_CHECKLIST_BY_AUTHORIZER", ObjectType = "REFFERED_BACK_PROCESS_TRANSACTION_BY_AUTHORIZER", ObjectId = "T_ID", RequireResultMessage = true)]
        public string reffered_back_process_transaction_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RefferedBackProcessTransactionByAuthorizer(T_ID, COMMENTS) + "\"}";

            }

        private void PopulateDashboardChrome()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["HideTopHeader"] = true;
            }

        private WorkflowDashboardViewModel BuildChecklistDashboardViewModel(SessionUser user, string requestedStepKey)
            {
            var steps = new List<WorkflowDashboardStepModel>
                {
                CreateDashboardStep(1, "MANAGE_CHECKLIST", "Manage Checklist", "/Setup/manage_Checklist", "~/Views/Setup/DashboardPartials/_ManageChecklist.cshtml"),
                CreateDashboardStep(2, "MANAGE_SUB_CHECKLIST", "Manage Sub Checklist", "/Setup/manage_sub_Checklist", "~/Views/Setup/DashboardPartials/_ManageSubChecklist.cshtml"),
                CreateDashboardStep(3, "MANAGE_CHECKLIST_DETAIL", "Manage Checklist Detail", "/Setup/manage_checklist_detail", "~/Views/Setup/DashboardPartials/_ManageChecklistDetail.cshtml"),
                CreateDashboardStep(4, "REVIEW_AUDIT_CHECKLIST", "Review Audit Checklist", "/AdministrationPanel/review_audit_checklist", "~/Views/AdministrationPanel/DashboardPartials/_ReviewAuditChecklist.cshtml"),
                CreateDashboardStep(5, "SUB_PROCESS_AUTHORIZE", "Sub Process Authorize", "/Setup/sub_process_authorize", "~/Views/Setup/DashboardPartials/_SubProcessAuthorize.cshtml"),
                CreateDashboardStep(6, "PROCESS_DETAIL_AUTHORIZE", "Process Detail Authorize", "/Setup/process_detail_authorize", "~/Views/Setup/DashboardPartials/_ProcessDetailAuthorize.cshtml")
                };

            foreach (var step in steps)
                {
                step.IsVisible = step.RequiredPermissionPageId > 0 && _permissionService.HasViewPermission(user, step.RequiredPermissionPageId);
                step.IsEnabled = step.IsVisible;
                step.IsCompleted = false;
                step.IsSaved = true;
                step.StatusText = "Available";
                }

            var firstVisibleStep = steps.FirstOrDefault(step => step.IsVisible);
            var currentStep = steps.FirstOrDefault(step => step.IsVisible && string.Equals(step.StepKey, requestedStepKey, StringComparison.OrdinalIgnoreCase))
                ?? firstVisibleStep;

            return new WorkflowDashboardViewModel
                {
                DashboardKey = "CHECKLIST_LIFECYCLE",
                DashboardTitle = "Checklist Lifecycle Workspace",
                CurrentStepKey = currentStep?.StepKey,
                Steps = steps
                };
            }

        private WorkflowDashboardStepModel CreateDashboardStep(int stepNo, string stepKey, string stepTitle, string legacyPath, string partialViewName)
            {
            return new WorkflowDashboardStepModel
                {
                StepNo = stepNo,
                StepKey = stepKey,
                StepTitle = stepTitle,
                LegacyPath = legacyPath,
                PartialViewName = partialViewName,
                RequiredPermissionPageId = _pageIdResolver?.ResolvePageId(legacyPath) ?? 0
                };
            }

        private bool TryGetRequestedStep(WorkflowDashboardViewModel model, string requestedStepKey, out WorkflowDashboardStepModel step, out IActionResult errorResult)
            {
            step = null;
            errorResult = null;

            if (model == null)
                {
                errorResult = BadRequest("Dashboard model could not be resolved.");
                return false;
                }

            if (string.IsNullOrWhiteSpace(requestedStepKey))
                {
                step = model.CurrentStep;
                if (step == null)
                    {
                    errorResult = Forbid();
                    return false;
                    }

                return true;
                }

            step = model.Steps.FirstOrDefault(item => string.Equals(item.StepKey, requestedStepKey, StringComparison.OrdinalIgnoreCase));
            if (step == null)
                {
                errorResult = BadRequest("Invalid workflow step.");
                return false;
                }

            if (!step.IsVisible)
                {
                errorResult = Forbid();
                return false;
                }

            return true;
            }

        private bool HasPageAccess(SessionUser user, string pagePath)
            {
            var pageId = _pageIdResolver?.ResolvePageId(pagePath) ?? 0;
            return pageId > 0 && _permissionService.HasViewPermission(user, pageId);
            }

        private PartialViewResult CreateStepAccessDeniedResult()
            {
            Response.StatusCode = 403;
            return PartialView("~/Views/Shared/_DashboardStepAccessDenied.cshtml");
            }

        private void PopulateChecklistDashboardStepViewData(string stepKey, int pageId)
            {
            ViewData["Layout"] = null;
            ViewData["PageId"] = pageId;
            ViewData["HideTopHeader"] = true;

            switch ((stepKey ?? string.Empty).Trim().ToUpperInvariant())
                {
                case "MANAGE_CHECKLIST":
                case "MANAGE_SUB_CHECKLIST":
                    ViewData["ChecklistTypes"] = dBConnection.GetAuditChecklist();
                    break;
                case "MANAGE_CHECKLIST_DETAIL":
                    ViewData["ChecklistTypes"] = dBConnection.GetAuditChecklist();
                    ViewData["ViolationsList"] = dBConnection.GetViolationsForChecklistDetail();
                    ViewData["ProcOwnerList"] = dBConnection.GetProcOwnerForChecklistDetail();
                    ViewData["RoleRespList"] = dBConnection.GetRoleResponsibleForChecklistDetail();
                    ViewData["AnnexList"] = dBConnection.GetAnnexuresForChecklistDetail();
                    ViewData["RiskList"] = dBConnection.GetRisks();
                    break;
                case "REVIEW_AUDIT_CHECKLIST":
                    ViewData["TransactionsList"] = dBConnection.GetUpdatedChecklistDetailsForReviewAndAuthorize(4);
                    break;
                case "SUB_PROCESS_AUTHORIZE":
                    ViewData["TransactionsList"] = dBConnection.GetUpdatedSubChecklistForReviewAndAuthorize(4);
                    break;
                case "PROCESS_DETAIL_AUTHORIZE":
                    ViewData["TransactionsList"] = dBConnection.GetUpdatedChecklistDetailsForReviewAndAuthorize(3);
                    break;
                default:
                    break;
                }
            }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
