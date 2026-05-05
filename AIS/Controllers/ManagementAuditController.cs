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
        private static readonly HashSet<string> PostJoiningSteps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CLOSING",
            "MANAGE_OBSERVATIONS",
            "QUALITY_REVIEW_HO",
            "ISSUE_REPORT",
            "EXIT_AUDIT",
            "MANAGEMENT_REPORT"
        };

        private static readonly HashSet<string> ClosingPerformedDisabledSteps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "JOIN",
            "CLOSING",
            "EXIT_AUDIT"
        };

        private readonly ILogger<ManagementAuditController> _logger;
        private readonly TopMenus _topMenus;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;
        private readonly FieldAuditDashboardProgressStore _progressStore;

        public ManagementAuditController(
            ILogger<ManagementAuditController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            TopMenus topMenus,
            IPermissionService permissionService,
            IPageIdResolver pageIdResolver,
            FieldAuditDashboardProgressStore progressStore)
        {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _topMenus = topMenus;
            _permissionService = permissionService;
            _pageIdResolver = pageIdResolver;
            _progressStore = progressStore;
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

            var step = model.Steps.FirstOrDefault(item => string.Equals(item.StepCode, stepCode, StringComparison.OrdinalIgnoreCase));
            if (step == null)
            {
                return NotFound();
            }

            if (!step.IsVisible)
            {
                return CreateStepAccessDeniedResult();
            }

            if (!step.IsEnabled)
            {
                return BadRequest(step.DisabledMessage ?? "This step is not available right now.");
            }

            PopulateReplicaViewData(step.RequiredPermissionPageId);
            var selectedEngagement = model.AvailableEngagements.FirstOrDefault(item => item.EngagementId == engId);
            var partialModel = new FieldAuditGridReplicaViewModel
            {
                EngagementId = engId,
                IsTeamLead = selectedEngagement?.IsTeamLead
            };

            switch (step.StepCode)
            {
                case "JOIN":
                    var joinModel = BuildJoinReplicaViewModel(user, engId);
                    return PartialView("~/Views/FieldAudit/_Join.cshtml", joinModel);
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
                case "EXIT_AUDIT":
                    return PartialView("~/Views/FieldAudit/_Closing.cshtml", partialModel);
                default:
                    return NotFound();
            }
        }

        [HttpGet]
        public IActionResult GetDashboardEngagementState(int engId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!_sessionHandler.TryGetUser(out _))
            {
                return Unauthorized();
            }

            if (engId <= 0)
            {
                return BadRequest("A valid engagement is required.");
            }

            var engagementState = GetEngagementOption(engId);
            if (engagementState == null)
            {
                return NotFound();
            }

            return Json(new
            {
                engPlanId = engagementState.EngagementId,
                statusId = engagementState.StatusId,
                isTeamLead = engagementState.IsTeamLead ?? string.Empty,
                display = string.IsNullOrWhiteSpace(engagementState.Display) ? engagementState.Label : engagementState.Display
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkStepCompleted([FromForm] string stepCode, [FromForm] int engId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!_sessionHandler.TryGetUser(out var user) || user == null)
            {
                return Unauthorized();
            }

            if (engId <= 0 || string.IsNullOrWhiteSpace(stepCode))
            {
                return BadRequest(new { success = false, message = "Invalid request." });
            }

            var model = BuildWorkflowViewModel(user, stepCode, engId);
            var step = model.VisibleSteps.FirstOrDefault(item => string.Equals(item.StepCode, stepCode, StringComparison.OrdinalIgnoreCase));
            if (step == null || !step.IsEnabled)
            {
                return Forbid();
            }

            _progressStore.MarkCompleted(engId, stepCode);
            model = BuildWorkflowViewModel(user, stepCode, engId);
            return Json(new
            {
                success = true,
                stepCode,
                statusText = model.VisibleSteps.FirstOrDefault(item => string.Equals(item.StepCode, stepCode, StringComparison.OrdinalIgnoreCase))?.StatusText ?? "Saved"
            });
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
            var engagementOptions = _dbConnection.GetArDashboardDropdownOptions()
                .Where(item => item.EngagementId > 0)
                .GroupBy(item => item.EngagementId)
                .Select(group => group.First())
                .OrderBy(item => item.Label)
                .ToList();

            var selectedEngagementId = engId.GetValueOrDefault() > 0 && engagementOptions.Any(item => item.EngagementId == engId.Value)
                ? engId
                : (int?)null;

            var workflowSteps = BuildWorkflowSteps();
            var selectedId = selectedEngagementId.GetValueOrDefault();
            var selectedEngagement = engagementOptions.FirstOrDefault(item => item.EngagementId == selectedId);
            var completedSteps = selectedId > 0
                ? _progressStore.GetCompletedStepCodes(selectedId)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var joiningSubmitted = selectedId > 0 && IsJoinAlreadySubmitted(selectedId);

            foreach (var step in workflowSteps)
            {
                step.IsVisible = step.RequiredPermissionPageId > 0 && _permissionService.HasViewPermission(user, step.RequiredPermissionPageId);
                step.IsEnabled = step.IsVisible && selectedId > 0;
                step.DisabledMessage = step.IsEnabled
                    ? null
                    : "Please select an engagement first.";

                if (step.IsEnabled && selectedEngagement != null)
                {
                    ApplyWorkflowAvailability(step, selectedEngagement.StatusId);
                }

                var isPersistedComplete = selectedId > 0 && completedSteps.Contains(step.StepCode);
                var isBusinessComplete = selectedId > 0
                    && string.Equals(step.StepCode, "JOIN", StringComparison.OrdinalIgnoreCase)
                    && joiningSubmitted;

                step.IsCompleted = step.IsVisible && (isPersistedComplete || isBusinessComplete);
                step.IsSaved = step.IsCompleted;
                step.StatusText = step.IsCompleted ? "Saved" : "Pending";
            }

            var firstVisibleStep = workflowSteps.FirstOrDefault(step => step.IsVisible);
            var requestedStep = workflowSteps.FirstOrDefault(step => step.IsVisible && string.Equals(step.StepCode, requestedStepCode, StringComparison.OrdinalIgnoreCase));
            var firstEnabledStep = workflowSteps.FirstOrDefault(step => step.IsVisible && step.IsEnabled);
            var currentStep = requestedStep != null && requestedStep.IsEnabled
                ? requestedStep
                : firstEnabledStep ?? requestedStep ?? firstVisibleStep;

            return new FieldAuditWorkflowViewModel
            {
                SelectedEngagementId = selectedEngagementId,
                AvailableEngagements = engagementOptions,
                CurrentStepCode = currentStep?.StepCode,
                Steps = workflowSteps
            };
        }

        private FieldAuditEngagementOptionModel GetEngagementOption(int engId)
        {
            if (engId <= 0)
            {
                return null;
            }

            return _dbConnection.GetArDashboardDropdownOptions()
                .Where(item => item.EngagementId > 0)
                .GroupBy(item => item.EngagementId)
                .Select(group => group.First())
                .FirstOrDefault(item => item.EngagementId == engId);
        }

        private bool IsJoinAlreadySubmitted(int engId)
        {
            var details = _dbConnection.GetClosingDraftObservations(engId);
            return details.Any(item => item.ENG_PLAN_ID == engId && !string.IsNullOrWhiteSpace(item.JOINING_DATE));
        }

        private FieldAuditJoinReplicaViewModel BuildJoinReplicaViewModel(SessionUser user, int engId)
        {
            var joiningDetails = _dbConnection.GetJoiningDetails(engId) ?? new JoiningModel();
            var hasCompletionDate = joiningDetails.END_DATE.HasValue;
            var teamDetails = joiningDetails.TEAM_DETAILS ?? new List<JoiningTeamModel>();

            var userPPNo = 0;
            if (!string.IsNullOrWhiteSpace(user?.PPNumber))
            {
                int.TryParse(user.PPNumber, out userPPNo);
            }

            var selectedMember = teamDetails.FirstOrDefault(member => member.PP_NO == userPPNo)
                ?? teamDetails.FirstOrDefault(member => !string.IsNullOrWhiteSpace(member.EMP_NAME));

            var memberName = selectedMember?.EMP_NAME;
            if (string.IsNullOrWhiteSpace(memberName))
            {
                memberName = user?.Name;
            }

            if (string.IsNullOrWhiteSpace(memberName) && userPPNo > 0)
            {
                memberName = userPPNo.ToString();
            }

            if (string.IsNullOrWhiteSpace(memberName))
            {
                memberName = "User";
            }

            var isTeamLead = string.Equals(selectedMember?.IS_TEAM_LEAD?.Trim(), "Y", StringComparison.OrdinalIgnoreCase);

            return new FieldAuditJoinReplicaViewModel
            {
                EngagementId = engId,
                JoiningDetails = joiningDetails,
                CompletionDate = hasCompletionDate ? joiningDetails.END_DATE.Value.Date : DateTime.UtcNow.Date,
                IsSubmitted = IsJoinAlreadySubmitted(engId),
                CurrentMemberName = memberName,
                MemberRoleLabel = isTeamLead ? "Team Lead" : "Team Member",
                EntityDisplayName = string.IsNullOrWhiteSpace(joiningDetails.ENTITY_NAME) ? "-" : joiningDetails.ENTITY_NAME,
                SystemDateDisplay = DateTime.Now.ToString("dd-MM-yyyy")
            };
        }

        private static void ApplyWorkflowAvailability(FieldAuditWorkflowStepModel step, int statusId)
        {
            if (step == null || !step.IsEnabled)
            {
                return;
            }

            if (string.Equals(step.StepCode, "JOIN", StringComparison.OrdinalIgnoreCase))
            {
                if (statusId != 1)
                {
                    step.IsEnabled = false;
                    step.DisabledMessage = "Joining is not available for the selected engagement.";
                }

                return;
            }

            if (string.Equals(step.StepCode, "EXIT_AUDIT", StringComparison.OrdinalIgnoreCase))
            {
                if (statusId != 2)
                {
                    step.IsEnabled = false;
                    step.DisabledMessage = "Closing is not available for the selected engagement.";
                }

                return;
            }

            if (statusId <= 1 && PostJoiningSteps.Contains(step.StepCode))
            {
                step.IsEnabled = false;
                step.DisabledMessage = "Submit joining first.";
                return;
            }

            if (statusId == 5 && ClosingPerformedDisabledSteps.Contains(step.StepCode))
            {
                step.IsEnabled = false;
                step.DisabledMessage = "This step is disabled after closing is performed.";
            }
        }

        private List<FieldAuditWorkflowStepModel> BuildWorkflowSteps()
        {
            return new List<FieldAuditWorkflowStepModel>
            {
                CreateStep(1, "JOIN", "Join", "~/Views/ManagementAudit/MA_Partials/_Join.cshtml", "/Engagement/Join"),
                CreateStep(2, "CLOSING", "Create Observation", "~/Views/ManagementAudit/MA_Partials/_Closing.cshtml", "/Execution/Audit_Execution"),
                CreateStep(3, "MANAGE_OBSERVATIONS", "Manage Observation", "~/Views/ManagementAudit/MA_Partials/_ManageObservations.cshtml", "/Execution/manage_observations"),
                CreateStep(4, "QUALITY_REVIEW_HO", "Quality Review HO", "~/Views/ManagementAudit/MA_Partials/_PreConcludingAuditHO.cshtml", "/Execution/pre_concluding_audit_ho"),
                CreateStep(5, "ISSUE_REPORT", "Issue Report", "~/Views/ManagementAudit/MA_Partials/_ConcludingClosingAudit.cshtml", "/Execution/Concluding_Closing_Audit"),
                CreateStep(6, "EXIT_AUDIT", "Closing", "~/Views/FieldAudit/_Closing.cshtml", "/Execution/closing"),
                CreateStep(7, "MANAGEMENT_REPORT", "Write Report", string.Empty, "/MANReport/Home")
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

        private void PopulateReplicaViewData(int pageId)
        {
            ViewData["Layout"] = null;
            ViewData["HideTopHeader"] = true;
            ViewData["PageId"] = pageId;
        }

        private PartialViewResult CreateStepAccessDeniedResult()
        {
            Response.StatusCode = 403;
            return PartialView("~/Views/Shared/_DashboardStepAccessDenied.cshtml");
        }
    }
}
