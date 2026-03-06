using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using AIS.Services;
using AIS.Models.Planning;
using System.Linq;
namespace AIS.Controllers
    {

    public class PlanningController : Controller
        {
        private readonly ILogger<PlanningController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        private readonly IPageIdResolver _pageIdResolver;
        public PlanningController(ILogger<PlanningController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService, IPageIdResolver pageIdResolver)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _pageIdResolver = pageIdResolver;
            }
        public IActionResult audit_criteria()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditEntities"] = dBConnection.GetAuditEntities();
            ViewData["AuditPeriodList"] = dBConnection.GetAuditPeriods();
            ViewData["AuditFrequencies"] = dBConnection.GetAuditFrequencies();
            ViewData["BranchSizesList"] = dBConnection.GetBranchSizes();
            ViewData["RiskList"] = dBConnection.GetRisks();
            ViewData["PendingCriteriaList"] = dBConnection.GetPendingAuditCriterias();
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
        public IActionResult special_audit_criteria()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditNatureList"] = dBConnection.GetAuditNatureForAddLegacyPara();
            ViewData["AuditPeriodList"] = dBConnection.GetAuditPeriods();
            ViewData["ReportingOfficeList"] = dBConnection.Getparentrepoffice(5);

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
        public IActionResult special_audit_criteria_approval()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

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
        public IActionResult refferedback_audit_criteria()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ReferedBackAuditCriteriaList"] = dBConnection.GetRefferedBackAuditCriterias();
            ViewData["AuditEntities"] = dBConnection.GetAuditEntities();
            ViewData["AuditPeriodList"] = dBConnection.GetAuditPeriods();
            ViewData["AuditFrequencies"] = dBConnection.GetAuditFrequencies();
            ViewData["BranchSizesList"] = dBConnection.GetBranchSizes();
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
        public IActionResult audit_criteria_approval()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ToAuthorizeAuditCriteriaList"] = dBConnection.GetAuditCriteriasToAuthorize();
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
        public IActionResult audit_period()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            bool sessionCheck = true;
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser != null
                && loggedInUser.UserEntityID.GetValueOrDefault() > 0
                && !string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                && loggedInUser.UserRoleID == 1)
                sessionCheck = false;
            ViewData["AuditDepartments"] = dBConnection.GetDepartments(354, sessionCheck);
            ViewData["AuditPeriodStatus"] = dBConnection.GetAuditPeriodStatus();
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
        [HttpGet]
        public IActionResult audit_plan(int dept_code, int periodId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditTeams"] = dBConnection.GetAuditTeams(dept_code);
            ViewData["AuditPlan"] = dBConnection.GetAuditPlan(periodId);
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
        public IActionResult holiday_calendar()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
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
        public IActionResult post_changes_criteria()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["PostChangesAuditCriteriaList"] = dBConnection.GetPostChangesAuditCriterias();
            ViewData["AuditEntities"] = dBConnection.GetAuditEntities();
            ViewData["AuditPeriodList"] = dBConnection.GetAuditPeriods();
            ViewData["AuditFrequencies"] = dBConnection.GetAuditFrequencies();
            ViewData["BranchSizesList"] = dBConnection.GetBranchSizes();
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
        public IActionResult post_changes_approved_plan()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
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
        public IActionResult post_changes_team_members()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
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
        public IActionResult special_assignment()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditDepartments"] = dBConnection.GetDepartments(354);
            ViewData["DivisionsList"] = dBConnection.GetDivisions(false);
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
        public IActionResult submission_for_approval()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
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
        public IActionResult submission_for_review()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["TentativePlansList"] = dBConnection.GetTentativePlansForFields();
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
        public IActionResult staff_position()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            bool sessionCheck = true;
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser != null
                && loggedInUser.UserEntityID.GetValueOrDefault() > 0
                && !string.IsNullOrWhiteSpace(loggedInUser.PPNumber)
                && loggedInUser.UserRoleID == 1)
                sessionCheck = false;
            ViewData["AuditDepartments"] = dBConnection.GetDepartments(354, sessionCheck);
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
        public IActionResult team_members()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditDepartments"] = dBConnection.GetDepartments(354);
            var loggedInUser = sessionHandler.GetUser();
            ViewData["AuditEmployees"] = loggedInUser == null
                ? new List<AuditEmployeeModel>()
                : dBConnection.GetAuditEmployees((int)loggedInUser.UserEntityID.GetValueOrDefault());
            ViewData["AuditTeams"] = dBConnection.GetAuditTeams();

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
        public IActionResult tentative_audit_plan_ho_units()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditDepartments"] = dBConnection.GetDepartments(354);
            var loggedInUser = sessionHandler.GetUser();
            if (loggedInUser == null)
                {
                ViewData["AuditEmployees"] = new List<AuditEmployeeModel>();
                }
            else if (loggedInUser.UserPostingAuditZone != null && loggedInUser.UserPostingAuditZone != 0)
                ViewData["AuditEmployees"] = dBConnection.GetAuditEmployees((int)loggedInUser.UserPostingAuditZone);
            else if (loggedInUser.UserPostingBranch != null && loggedInUser.UserPostingBranch != 0)
                ViewData["AuditEmployees"] = dBConnection.GetAuditEmployees((int)loggedInUser.UserPostingBranch);
            else if (loggedInUser.UserPostingDept != null && loggedInUser.UserPostingDept != 0)
                ViewData["AuditEmployees"] = dBConnection.GetAuditEmployees((int)loggedInUser.UserPostingDept);
            else if (loggedInUser.UserPostingDiv != null && loggedInUser.UserPostingDiv != 0)
                ViewData["AuditEmployees"] = dBConnection.GetAuditEmployees((int)loggedInUser.UserPostingDiv);
            else if (loggedInUser.UserPostingZone != null && loggedInUser.UserPostingZone != 0)
                ViewData["AuditEmployees"] = dBConnection.GetAuditEmployees((int)loggedInUser.UserPostingZone);
            else
                ViewData["AuditEmployees"] = new List<AuditEmployeeModel>();


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
        


        public IActionResult tentative_audit_plan()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditDepartments"] = dBConnection.GetDepartments(354);
            ViewData["DivisionsList"] = dBConnection.GetDivisions(false);
            ViewData["AuditZonesList"] = dBConnection.GetZones();
            List<TentativePlanModel> pl = new List<TentativePlanModel>();
            pl = dBConnection.GetTentativePlansForFields();
            ViewData["TotalPlanEntities"] = pl.Count;
            ViewData["TentativePlansList"] = pl;
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
        public IActionResult tentative_engagement_plan()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AuditDepartments"] = dBConnection.GetDepartments(354);
            ViewData["DivisionsList"] = dBConnection.GetDivisions(false);
            ViewData["AuditZonesList"] = dBConnection.GetZones();
            ViewData["AuditTeamsList"] = dBConnection.GetAuditTeams();
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
        
        public IActionResult Planning(string stepCode = null, int? contextId = null, int? contextSecondaryId = null)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return RedirectToAction("Index", "Login");
                }

            var model = BuildPlanningWorkflowViewModel(user, stepCode, contextId, contextSecondaryId);
            if (!model.VisibleSteps.Any())
                {
                return View(model);
                }

            return View(model);
            }

        [HttpGet]
        public IActionResult LoadPlanningStep(string stepCode, int? contextId = null, int? contextSecondaryId = null)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized();
                }

            var model = BuildPlanningWorkflowViewModel(user, stepCode, contextId, contextSecondaryId);
            var step = model.VisibleSteps.FirstOrDefault(item => string.Equals(item.StepCode, model.CurrentStepCode, StringComparison.OrdinalIgnoreCase));
            if (step == null)
                {
                return Forbid();
                }

            PopulatePlanningStepViewData(step.StepCode);
            return PartialView(step.PartialViewName);
            }

        private PlanningWorkflowViewModel BuildPlanningWorkflowViewModel(SessionUser user, string requestedStepCode, int? contextId, int? contextSecondaryId)
            {
            var workflowSteps = BuildPlanningWorkflowSteps();
            foreach (var step in workflowSteps)
                {
                step.IsVisible = step.RequiredPermissionPageId > 0 && _permissionService.HasViewPermission(user, step.RequiredPermissionPageId);
                step.IsEnabled = step.IsVisible;
                step.IsSaved = step.IsVisible;
                step.IsCompleted = false;
                step.StatusText = step.IsSaved ? "Saved" : "Not Saved";
                }

            var firstVisibleStep = workflowSteps.FirstOrDefault(step => step.IsVisible);
            var selectedStep = workflowSteps.FirstOrDefault(step => step.IsVisible && string.Equals(step.StepCode, requestedStepCode, StringComparison.OrdinalIgnoreCase))
                ?? firstVisibleStep;

            if (selectedStep != null)
                {
                PopulatePlanningStepViewData(selectedStep.StepCode);
                }

            return new PlanningWorkflowViewModel
                {
                ContextId = contextId,
                ContextSecondaryId = contextSecondaryId,
                CurrentStepCode = selectedStep?.StepCode,
                Steps = workflowSteps
                };
            }

        private List<PlanningWorkflowStepModel> BuildPlanningWorkflowSteps()
            {
            return new List<PlanningWorkflowStepModel>
                {
                CreateStep(1, "AUDIT_CRITERIA", "Audit Criteria", "~/Views/Planning/Partials/_AuditCriteriaStep.cshtml", "/Planning/audit_criteria"),
                CreateStep(2, "CRITERIA_APPROVAL", "Criteria Approval", "~/Views/Planning/Partials/_AuditCriteriaApprovalStep.cshtml", "/Planning/audit_criteria_approval"),
                CreateStep(3, "REFERRED_BACK_CRITERIA", "Referred Back Criteria", "~/Views/Planning/Partials/_ReferredBackAuditCriteriaStep.cshtml", "/Planning/refferedback_audit_criteria"),
                CreateStep(4, "GENERATE_PLAN", "Generate Plan", "~/Views/Planning/Partials/_GeneratePlanStep.cshtml", "/Planning/post_changes_criteria"),
                CreateStep(5, "AUDIT_PLAN", "Audit Plan", "~/Views/Planning/Partials/_TentativeAuditPlanStep.cshtml", "/Planning/tentative_audit_plan"),
                CreateStep(6, "AUDIT_TEAM", "Audit Team", "~/Views/Planning/Partials/_TeamMembersStep.cshtml", "/Planning/team_members"),
                CreateStep(7, "REFERRED_BACK_ENGAGEMENT", "Referred Back Engagement", "~/Views/Planning/Partials/_ReferredBackEngagementStep.cshtml", "/Engagement/eng_plan_ref_list"),
                CreateStep(8, "ENGAGEMENT_APPROVAL", "Engagement Approval", "~/Views/Planning/Partials/_EngagementApprovalStep.cshtml", "/Engagement/eng_plan_list")
                };
            }

        private PlanningWorkflowStepModel CreateStep(int stepNo, string stepCode, string title, string partialViewName, string mappedPath)
            {
            _pageIdResolver.TryResolvePageId(mappedPath, out var pageId);
            return new PlanningWorkflowStepModel
                {
                StepNo = stepNo,
                StepCode = stepCode,
                StepTitle = title,
                PartialViewName = partialViewName,
                RequiredPermissionPageId = pageId
                };
            }

        private void PopulatePlanningStepViewData(string stepCode)
            {
            if (string.IsNullOrWhiteSpace(stepCode))
                {
                return;
                }

            switch (stepCode)
                {
                case "AUDIT_CRITERIA":
                    ViewData["AuditEntities"] = dBConnection.GetAuditEntities();
                    ViewData["AuditPeriodList"] = dBConnection.GetAuditPeriods();
                    ViewData["AuditFrequencies"] = dBConnection.GetAuditFrequencies();
                    ViewData["BranchSizesList"] = dBConnection.GetBranchSizes();
                    ViewData["RiskList"] = dBConnection.GetRisks();
                    ViewData["PendingCriteriaList"] = dBConnection.GetPendingAuditCriterias();
                    break;
                case "CRITERIA_APPROVAL":
                    ViewData["ToAuthorizeAuditCriteriaList"] = dBConnection.GetAuditCriteriasToAuthorize();
                    break;
                case "REFERRED_BACK_CRITERIA":
                    ViewData["ReferedBackAuditCriteriaList"] = dBConnection.GetRefferedBackAuditCriterias();
                    ViewData["AuditEntities"] = dBConnection.GetAuditEntities();
                    ViewData["AuditPeriodList"] = dBConnection.GetAuditPeriods();
                    ViewData["AuditFrequencies"] = dBConnection.GetAuditFrequencies();
                    ViewData["BranchSizesList"] = dBConnection.GetBranchSizes();
                    ViewData["RiskList"] = dBConnection.GetRisks();
                    break;
                case "AUDIT_PLAN":
                    ViewData["AuditDepartments"] = dBConnection.GetDepartments(354);
                    ViewData["DivisionsList"] = dBConnection.GetDivisions(false);
                    ViewData["AuditZonesList"] = dBConnection.GetZones();
                    var tentativePlans = dBConnection.GetTentativePlansForFields();
                    ViewData["TotalPlanEntities"] = tentativePlans.Count;
                    ViewData["TentativePlansList"] = tentativePlans;
                    break;
                case "AUDIT_TEAM":
                    ViewData["AuditDepartments"] = dBConnection.GetDepartments(354);
                    var loggedInUser = sessionHandler.GetUser();
                    ViewData["AuditEmployees"] = loggedInUser == null
                        ? new List<AuditEmployeeModel>()
                        : dBConnection.GetAuditEmployees((int)loggedInUser.UserEntityID.GetValueOrDefault());
                    ViewData["AuditTeams"] = dBConnection.GetAuditTeams();
                    break;
                case "GENERATE_PLAN":
                    ViewData["PostChangesAuditCriteriaList"] = dBConnection.GetPostChangesAuditCriterias();
                    ViewData["AuditEntities"] = dBConnection.GetAuditEntities();
                    ViewData["AuditPeriodList"] = dBConnection.GetAuditPeriods();
                    ViewData["AuditFrequencies"] = dBConnection.GetAuditFrequencies();
                    ViewData["BranchSizesList"] = dBConnection.GetBranchSizes();
                    ViewData["RiskList"] = dBConnection.GetRisks();
                    break;
                case "REFERRED_BACK_ENGAGEMENT":
                    ViewData["EngagementPlans"] = dBConnection.GetRefferedBackAuditEngagementPlans();
                    break;
                case "ENGAGEMENT_APPROVAL":
                    ViewData["EngagementPlans"] = dBConnection.GetAuditEngagementPlans();
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
