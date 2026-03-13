using AIS.Models;
using AIS.Models.FieldAuditWorkflow;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
{
    public class ManagementAuditController : Controller
    {
        private readonly ILogger<ManagementAuditController> _logger;
        private readonly TopMenus _topMenus;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;

        public ManagementAuditController(
            ILogger<ManagementAuditController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            TopMenus topMenus,
            IPermissionService permissionService,
            IPageIdResolver pageIdResolver)
        {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _topMenus = topMenus;
            _permissionService = permissionService;
            _pageIdResolver = pageIdResolver;
        }

        [HttpGet]
        public IActionResult MA_Dashboard(string stepCode = null, int? engId = null)
        {
            ViewData["TopMenu"] = _topMenus.GetTopMenus();
            ViewData["TopMenuPages"] = _topMenus.GetTopMenusPages();
            ViewData["HideTopHeader"] = true;

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!_sessionHandler.TryGetUser(out var user) || user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var model = BuildWorkflowViewModel(user, stepCode, engId);
            return View("~/Views/ManagementAudit/MA_Dashboard.cshtml", model);
        }

        [HttpGet]
        public IActionResult LoadStep(string stepCode, int engId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!_sessionHandler.TryGetUser(out var user) || user == null)
            {
                return Unauthorized();
            }

            if (engId <= 0)
            {
                return BadRequest("A valid engagement is required.");
            }

            var model = BuildWorkflowViewModel(user, stepCode, engId);
            if (!model.HasEngagementSelection)
            {
                return BadRequest("A valid engagement is required.");
            }

            var step = model.VisibleSteps.FirstOrDefault(item => string.Equals(item.StepCode, model.CurrentStepCode, StringComparison.OrdinalIgnoreCase));
            if (step == null)
            {
                return Forbid();
            }

            var partialModel = new FieldAuditGridReplicaViewModel { EngagementId = engId };

            switch (step.StepCode)
            {
                case "JOIN":
                    return PartialView("~/Views/ManagementAudit/MA_Partials/_Join.cshtml", partialModel);
                case "CLOSING":
                    ViewData["Voilation_Cat"] = _dbConnection.GetAuditVoilationcats();
                    ViewData["RiskList"] = _dbConnection.GetRisks();
                    ViewData["OtherEntityList"] = _dbConnection.GetAuditEntitiesForOtherEntitySelection();
                    return PartialView("~/Views/ManagementAudit/MA_Partials/_Closing.cshtml", partialModel);
                case "MANAGE_OBSERVATIONS":
                    ViewData["RiskList"] = _dbConnection.GetRisks();
                    ViewData["ManageObservations"] = string.Empty;
                    return PartialView("~/Views/ManagementAudit/MA_Partials/_ManageObservations.cshtml", partialModel);
                case "QUALITY_REVIEW_HO":
                    ViewData["EntitiesList"] = _dbConnection.GetObservationEntitiesForPreConcluding();
                    ViewData["RiskList"] = _dbConnection.GetRisks();
                    ViewData["ProcessList"] = _dbConnection.GetRiskProcessDefinition();
                    ViewData["Voilation_Cat"] = _dbConnection.GetAuditVoilationcats();
                    return PartialView("~/Views/ManagementAudit/MA_Partials/_PreConcludingAuditHO.cshtml", partialModel);
                case "ISSUE_REPORT":
                    ViewData["EntitiesList"] = _dbConnection.GetObservationEntitiesForPreConcluding();
                    return PartialView("~/Views/ManagementAudit/MA_Partials/_ConcludingClosingAudit.cshtml", partialModel);
                default:
                    return NotFound();
            }
        }

        [HttpGet]
        public IActionResult OpenManagementReport(int engId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!_sessionHandler.TryGetUser(out var user) || user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (engId <= 0)
            {
                return RedirectToAction(nameof(MA_Dashboard));
            }

            _sessionHandler.SetActiveEngagementId(engId);
            return RedirectToAction("Home", "MANReport");
        }

        private FieldAuditWorkflowViewModel BuildWorkflowViewModel(SessionUser user, string requestedStepCode, int? engId)
        {
            var engagementOptions = _dbConnection.GetTaskList()
                .Where(item => item.ENG_PLAN_ID > 0)
                .GroupBy(item => item.ENG_PLAN_ID)
                .Select(group => group.First())
                .OrderBy(item => item.ENTITY_NAME)
                .Select(item => new FieldAuditEngagementOptionModel
                {
                    EngagementId = item.ENG_PLAN_ID,
                    EntityName = item.ENTITY_NAME,
                    StageName = item.ENG_STATUS
                })
                .ToList();

            var selectedEngagementId = engId.GetValueOrDefault() > 0 && engagementOptions.Any(item => item.EngagementId == engId.Value)
                ? engId
                : (int?)null;

            var workflowSteps = BuildWorkflowSteps();
            var selectedId = selectedEngagementId.GetValueOrDefault();

            foreach (var step in workflowSteps)
            {
                step.IsVisible = step.RequiredPermissionPageId > 0 && _permissionService.HasViewPermission(user, step.RequiredPermissionPageId);
                step.IsEnabled = step.IsVisible && selectedId > 0;
                step.IsCompleted = false;
                step.IsSaved = false;
                step.StatusText = "Pending";
            }

            var firstVisibleStep = workflowSteps.FirstOrDefault(step => step.IsVisible);
            var currentStep = workflowSteps.FirstOrDefault(step => step.IsVisible && string.Equals(step.StepCode, requestedStepCode, StringComparison.OrdinalIgnoreCase))
                ?? firstVisibleStep;

            return new FieldAuditWorkflowViewModel
            {
                SelectedEngagementId = selectedEngagementId,
                AvailableEngagements = engagementOptions,
                CurrentStepCode = currentStep?.StepCode,
                Steps = workflowSteps
            };
        }

        private List<FieldAuditWorkflowStepModel> BuildWorkflowSteps()
        {
            return new List<FieldAuditWorkflowStepModel>
            {
                CreateStep(1, "JOIN", "Join", "~/Views/ManagementAudit/MA_Partials/_Join.cshtml", "/Engagement/Join"),
                CreateStep(2, "CLOSING", "Closing", "~/Views/ManagementAudit/MA_Partials/_Closing.cshtml", "/Execution/Audit_Execution"),
                CreateStep(3, "MANAGE_OBSERVATIONS", "Manage Observation", "~/Views/ManagementAudit/MA_Partials/_ManageObservations.cshtml", "/Execution/manage_observations"),
                CreateStep(4, "QUALITY_REVIEW_HO", "Quality Review HO", "~/Views/ManagementAudit/MA_Partials/_PreConcludingAuditHO.cshtml", "/Execution/pre_concluding_audit_ho"),
                CreateStep(5, "ISSUE_REPORT", "Issue Report", "~/Views/ManagementAudit/MA_Partials/_ConcludingClosingAudit.cshtml", "/Execution/Concluding_Closing_Audit"),
                CreateStep(6, "MANAGEMENT_REPORT", "Write Report", string.Empty, "/MANReport/Home")
            };
        }

        private FieldAuditWorkflowStepModel CreateStep(int stepNo, string stepCode, string title, string partialViewName, string mappedPath)
        {
            _pageIdResolver.TryResolvePageId(mappedPath, out var pageId);
            return new FieldAuditWorkflowStepModel
            {
                StepNo = stepNo,
                StepCode = stepCode,
                StepTitle = title,
                PartialViewName = partialViewName,
                RequiredPermissionPageId = pageId
            };
        }
    }
}
