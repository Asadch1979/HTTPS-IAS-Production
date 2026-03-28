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
            ViewData["HideTopHeader"] = true;

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!HasPageAccess(user, "/Planning/Planning"))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            var model = BuildPlanningWorkflowViewModel(user, stepCode, contextId, contextSecondaryId);
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
            if (!TryGetRequestedPlanningStep(model, stepCode, out var step, out var errorResult))
                {
                if (errorResult is ForbidResult)
                    {
                    return CreateStepAccessDeniedResult();
                    }

                return errorResult;
                }

            ViewData["PageId"] = step.RequiredPermissionPageId;
            PopulatePlanningStepViewData(step.StepCode, user);
            return PartialView(step.PartialViewName);
            }

        [HttpGet]
        public IActionResult LoadPlanningChildStep(string stepKey, string childKey, int? planId = null, string name = null, string size = null, string risk = null, string freq = null, string days = null, string period = null, int? periodId = null, string code = null, int? zoneId = null, int? entityId = null, int? entityType = null)
            {
            return LoadPlanningSubChildStep(stepKey, childKey, "CREATE", planId, name, size, risk, freq, days, period, periodId, code, zoneId, entityId, entityType);
            }

        [HttpGet]
        public IActionResult LoadPlanningSubChildStep(string stepKey, string childKey, string actionKey, int? planId = null, string name = null, string size = null, string risk = null, string freq = null, string days = null, string period = null, int? periodId = null, string code = null, int? zoneId = null, int? entityId = null, int? entityType = null)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized();
                }

            var workflowModel = BuildPlanningWorkflowViewModel(user, stepKey, null, null);
            if (!TryGetRequestedPlanningStep(workflowModel, stepKey, out _, out var stepErrorResult))
                {
                if (stepErrorResult is ForbidResult)
                    {
                    return CreateStepAccessDeniedResult();
                    }

                return stepErrorResult;
                }

            if (!TryResolvePlanningSubChild(stepKey, childKey, actionKey, out var subChildViewPath, out var permissionPath, out var permissionPageId, out var errorResult))
                {
                if (errorResult is ForbidResult)
                    {
                    return CreateStepAccessDeniedResult();
                    }

                return errorResult;
                }

            if (permissionPageId <= 0 || !_permissionService.HasViewPermission(user, permissionPageId))
                {
                return CreateStepAccessDeniedResult();
                }

            PopulatePlanningSubChildViewData(stepKey, childKey, actionKey, permissionPageId, planId, name, size, risk, freq, days, period, periodId, code, zoneId, entityId, entityType);
            return PartialView(subChildViewPath);
            }

        [HttpGet]
        public IActionResult LoadPlanningNestedView(string viewCode, int? planId = null, string name = null, string size = null, string risk = null, string freq = null, string days = null, string period = null, int? periodId = null, string code = null, int? zoneId = null, int? entityId = null, int? entityType = null)
            {
            switch ((viewCode ?? string.Empty).Trim().ToUpperInvariant())
                {
                case "TENTATIVE_ENGAGEMENT_PLAN":
                    return LoadPlanningSubChildStep("AUDIT_PLAN", "ENGAGEMENT_PLAN", "CREATE", planId, name, size, risk, freq, days, period, periodId, code, zoneId, entityId, entityType);
                default:
                    return NotFound();
                }
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
                MappedPath = mappedPath,
                PartialViewName = partialViewName,
                RequiredPermissionPageId = pageId
                };
            }

        private void PopulatePlanningStepViewData(string stepCode, SessionUser user = null)
            {
            if (string.IsNullOrWhiteSpace(stepCode))
                {
                return;
                }

            ViewData["Layout"] = null;
            ViewData["HideTopHeader"] = true;

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

        private bool TryGetRequestedPlanningStep(PlanningWorkflowViewModel model, string requestedStepCode, out PlanningWorkflowStepModel step, out IActionResult errorResult)
            {
            step = null;
            errorResult = null;

            if (model == null)
                {
                errorResult = BadRequest("Planning workflow model could not be resolved.");
                return false;
                }

            if (string.IsNullOrWhiteSpace(requestedStepCode))
                {
                step = model.CurrentStep;
                if (step == null)
                    {
                    errorResult = Forbid();
                    return false;
                    }

                return true;
                }

            step = model.Steps.FirstOrDefault(item => string.Equals(item.StepCode, requestedStepCode, StringComparison.OrdinalIgnoreCase));
            if (step == null)
                {
                errorResult = BadRequest("Invalid planning step.");
                return false;
                }

            if (!step.IsVisible)
                {
                errorResult = Forbid();
                return false;
                }

            return true;
            }

        private bool TryResolvePlanningSubChild(string stepKey, string childKey, string actionKey, out string partialViewPath, out string permissionPath, out int permissionPageId, out IActionResult errorResult)
            {
            partialViewPath = null;
            permissionPath = null;
            permissionPageId = 0;
            errorResult = null;

            var normalizedStepKey = (stepKey ?? string.Empty).Trim().ToUpperInvariant();
            var normalizedChildKey = (childKey ?? string.Empty).Trim().ToUpperInvariant();
            var normalizedActionKey = (actionKey ?? string.Empty).Trim().ToUpperInvariant();

            if (normalizedStepKey == "AUDIT_PLAN" &&
                normalizedChildKey == "ENGAGEMENT_PLAN" &&
                normalizedActionKey == "CREATE")
                {
                permissionPath = "/Planning/tentative_engagement_plan";
                _pageIdResolver.TryResolvePageId(permissionPath, out permissionPageId);
                partialViewPath = "~/Views/Planning/Partials/_TentativeEngagementPlanReplica.cshtml";
                return true;
                }

            errorResult = BadRequest("Invalid planning child action.");
            return false;
            }

        private void PopulatePlanningSubChildViewData(string stepKey, string childKey, string actionKey, int permissionPageId, int? planId, string name, string size, string risk, string freq, string days, string period, int? periodId, string code, int? zoneId, int? entityId, int? entityType)
            {
            ViewData["Layout"] = null;
            ViewData["HideTopHeader"] = true;
            ViewData["PageId"] = permissionPageId;

            var normalizedStepKey = (stepKey ?? string.Empty).Trim().ToUpperInvariant();
            var normalizedChildKey = (childKey ?? string.Empty).Trim().ToUpperInvariant();
            var normalizedActionKey = (actionKey ?? string.Empty).Trim().ToUpperInvariant();

            if (normalizedStepKey == "AUDIT_PLAN" &&
                normalizedChildKey == "ENGAGEMENT_PLAN" &&
                normalizedActionKey == "CREATE")
                {
                ViewData["AuditDepartments"] = dBConnection.GetDepartments(354);
                ViewData["DivisionsList"] = dBConnection.GetDivisions(false);
                ViewData["AuditZonesList"] = dBConnection.GetZones();
                ViewData["AuditTeamsList"] = dBConnection.GetAuditTeams();
                ViewData["PlanningPlanId"] = planId;
                ViewData["PlanningEntityName"] = name;
                ViewData["PlanningSize"] = size;
                ViewData["PlanningRisk"] = risk;
                ViewData["PlanningFrequency"] = freq;
                ViewData["PlanningDays"] = days;
                ViewData["PlanningPeriodName"] = period;
                ViewData["PlanningPeriodId"] = periodId;
                ViewData["PlanningCode"] = code;
                ViewData["PlanningZoneId"] = zoneId;
                ViewData["PlanningEntityId"] = entityId;
                ViewData["PlanningEntityType"] = entityType;
                ViewData["PlanId"] = planId;
                ViewData["EntityName"] = name;
                ViewData["Size"] = size;
                ViewData["Risk"] = risk;
                ViewData["Frequency"] = freq;
                ViewData["Days"] = days;
                ViewData["PeriodName"] = period;
                ViewData["PeriodId"] = periodId;
                ViewData["Code"] = code;
                ViewData["ZoneId"] = zoneId;
                ViewData["EntityId"] = entityId;
                ViewData["EntityType"] = entityType;
                }
            }

        private bool HasPageAccess(SessionUser user, string path)
            {
            if (user == null || string.IsNullOrWhiteSpace(path))
                {
                return false;
                }

            _pageIdResolver.TryResolvePageId(path, out var pageId);
            return pageId > 0 && _permissionService.HasViewPermission(user, pageId);
            }

        private PartialViewResult CreateStepAccessDeniedResult()
            {
            Response.StatusCode = 403;
            return PartialView("~/Views/Shared/_DashboardStepAccessDenied.cshtml");
            }

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
