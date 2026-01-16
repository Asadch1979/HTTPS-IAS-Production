using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using AIS.Services;


namespace AIS.Controllers
    {

    public class SetupController : Controller
        {
        private readonly ILogger<SetupController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        public SetupController(ILogger<SetupController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
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

        public IActionResult Inspection_Unit()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ICList"] = dBConnection.GetInspectionUnits();
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
        public IActionResult control_violation()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ControlViolationList"] = dBConnection.GetControlViolations();
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
        public RiskProcessDefinition process_add(RiskProcessDefinition proc)
            {
            return dBConnection.AddRiskProcess(proc);
            }
        [HttpPost]
        public RiskProcessDetails sub_process_add(RiskProcessDetails subProc)
            {
            return dBConnection.AddRiskSubProcess(subProc);
            }
        [HttpPost]
        public RiskProcessTransactions sub_process_transaction_add(RiskProcessTransactions tran)
            {
            return dBConnection.AddRiskSubProcessTransaction(tran);
            }
        [HttpPost]
        public string recommend_process_transaction_by_reviewer(int T_ID, string COMMENTS, int PROCESS_DETAIL_ID = 0, int SUB_PROCESS_ID = 0, string HEADING = "", int V_ID = 0, int CONTROL_ID = 0, int ROLE_ID = 0, int RISK_ID = 0, string ANNEX_CODE = "")
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RecommendProcessTransactionByReviewer(T_ID, COMMENTS, PROCESS_DETAIL_ID, SUB_PROCESS_ID, HEADING, V_ID, CONTROL_ID, ROLE_ID, RISK_ID, ANNEX_CODE) + "\"}";

            }
        [HttpPost]
        public string reffered_back_process_transaction_by_reviewer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RefferedBackProcessTransactionByReviewer(T_ID, COMMENTS) + "\"}";

            }
        [HttpPost]
        public string authorize_process_transaction_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.AuthorizeProcessTransactionByAuthorizer(T_ID, COMMENTS) + "\"}";

            }
        [HttpPost]
        public string reffered_back_process_transaction_by_authorizer(int T_ID, string COMMENTS)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.RefferedBackProcessTransactionByAuthorizer(T_ID, COMMENTS) + "\"}";

            }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
