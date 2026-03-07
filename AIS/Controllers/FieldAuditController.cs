using AIS.Models;
using AIS.Models.FieldAuditWorkflow;
using AIS.Models.Requests;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
    {
    public class FieldAuditController : Controller
        {
        private readonly ILogger<FieldAuditController> _logger;
        private readonly TopMenus _topMenus;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;
        private readonly FieldAuditDashboardProgressStore _progressStore;

        public FieldAuditController(
            ILogger<FieldAuditController> logger,
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
        public IActionResult Dashboard(string stepCode = null, int? engId = null)
            {
            ViewData["TopMenu"] = _topMenus.GetTopMenus();
            ViewData["TopMenuPages"] = _topMenus.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!_sessionHandler.TryGetUser(out var user) || user == null)
                {
                return RedirectToAction("Index", "Login");
                }

            var model = BuildWorkflowViewModel(user, stepCode, engId);
            return View("~/Views/FieldAudit/Dashboard.cshtml", model);
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

            return LoadStepPartial(step.StepCode, engId);
            }

        [HttpGet]
        public IActionResult LoadJoin(int engId)
            {
            return LoadStepPartial("JOINING", engId);
            }

        [HttpGet]
        public IActionResult LoadSamples(int engId)
            {
            return LoadStepPartial("SAMPLING", engId);
            }

        [HttpGet]
        public IActionResult LoadException(int engId)
            {
            return LoadStepPartial("EXCEPTION_REPORT", engId);
            }

        [HttpGet]
        public IActionResult LoadWPaper(int engId)
            {
            return LoadStepPartial("WORKING_PAPER", engId);
            }

        [HttpGet]
        public IActionResult LoadClosing(int engId)
            {
            return LoadStepPartial("EXIT_AUDIT", engId);
            }

        [HttpGet]
        public IActionResult LoadObservation(int engId)
            {
            return LoadStepPartial("MEMO_CREATION", engId);
            }

        [HttpGet]
        public IActionResult _ManageObservationBranches(int engId)
            {
            return LoadStepPartial("MANAGE_OBSERVATION_BRANCHES", engId);
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
            if (step == null)
                {
                return Forbid();
                }

            _progressStore.MarkCompleted(engId, stepCode);
            model = BuildWorkflowViewModel(user, stepCode, engId);
            return Json(new
                {
                success = true,
                stepCode,
                statusText = model.VisibleSteps.FirstOrDefault(item => string.Equals(item.StepCode, stepCode, StringComparison.OrdinalIgnoreCase))?.StatusText ?? "Completed"
                });
            }

        [HttpGet]
        public IActionResult OpenAuditReport(int engId)
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
                return RedirectToAction(nameof(Dashboard));
                }

            var model = BuildWorkflowViewModel(user, "AUDIT_REPORT", engId);
            if (!model.HasEngagementSelection)
                {
                return RedirectToAction(nameof(Dashboard));
                }

            _sessionHandler.SetActiveEngagementId(engId);
            return RedirectToAction("ReportOverview", "FieldAuditReport");
            }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult SubmitJoin([FromForm] AddJoiningPostModel model)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!_sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized();
                }

            if (model == null || !model.ENG_PLAN_ID.HasValue || model.ENG_PLAN_ID.Value <= 0)
                {
                return Json(new
                    {
                    status = false,
                    message = "A valid engagement is required."
                    });
                }

            var joiningDetails = _dbConnection.GetJoiningDetails(model.ENG_PLAN_ID.Value);
            var selectedMember = joiningDetails?.TEAM_DETAILS?.FirstOrDefault();
            if (selectedMember == null)
                {
                return Json(new
                    {
                    status = false,
                    message = "Unable to resolve team member details for joining submission."
                    });
                }

            var request = new AddJoiningModel
                {
                ID = model.ID ?? 0,
                ENG_PLAN_ID = model.ENG_PLAN_ID.Value,
                TEAM_MEM_PPNO = selectedMember.PP_NO,
                JOINING_DATE = model.JOINING_DATE,
                COMPLETION_DATE = model.COMPLETION_DATE
                };

            var responseMessage = _dbConnection.AddJoiningReport(request);
            var isSubmitted = IsJoinAlreadySubmitted(model.ENG_PLAN_ID.Value);

            return Json(new
                {
                status = true,
                message = responseMessage,
                isSubmitted
                });
            }

        private IActionResult LoadStepPartial(string stepCode, int engId)
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

            var step = model.VisibleSteps.FirstOrDefault(item => string.Equals(item.StepCode, stepCode, StringComparison.OrdinalIgnoreCase));
            if (step == null)
                {
                return Forbid();
                }

            switch (step.StepCode)
                {
                case "JOINING":
                    var joinModel = BuildJoinReplicaViewModel(engId);
                    return PartialView("~/Views/FieldAudit/_Join.cshtml", joinModel);
                case "SAMPLING":
                    return PartialView("~/Views/FieldAudit/_Samples.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "EXCEPTION_REPORT":
                    return PartialView("~/Views/FieldAudit/_Exception.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "WORKING_PAPER":
                    return PartialView("~/Views/FieldAudit/_WPaper.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "MEMO_CREATION":
                    var observationModel = BuildObservationReplicaViewModel(engId);
                    return PartialView("~/Views/FieldAudit/_Observation.cshtml", observationModel);
                case "EXIT_AUDIT":
                    return PartialView("~/Views/FieldAudit/_Closing.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "MANAGE_OBSERVATION_BRANCHES":
                    var manageObservationBranchesModel = BuildManageObservationBranchesReplicaViewModel(engId);
                    return PartialView("~/Views/FieldAudit/_ManageObservationBranches.cshtml", manageObservationBranchesModel);
                case "AUDIT_REPORT":
                    return PartialView("~/Views/FieldAudit/_AuditReport.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                default:
                    var stepModel = BuildStepContext(step.StepCode, engId);
                    return PartialView(step.PartialViewName, stepModel);
                }
            }

        private FieldAuditJoinReplicaViewModel BuildJoinReplicaViewModel(int engId)
            {
            var joiningDetails = _dbConnection.GetJoiningDetails(engId) ?? new JoiningModel();
            var hasCompletionDate = joiningDetails.END_DATE.HasValue;

            return new FieldAuditJoinReplicaViewModel
                {
                EngagementId = engId,
                JoiningDetails = joiningDetails,
                CompletionDate = hasCompletionDate ? joiningDetails.END_DATE.Value.Date : DateTime.UtcNow.Date,
                IsSubmitted = IsJoinAlreadySubmitted(engId)
                };
            }

        private FieldAuditObservationReplicaViewModel BuildObservationReplicaViewModel(int engId)
            {
            ViewData["AnnexList"] = _dbConnection.GetAnnexuresForChecklistDetail();
            ViewData["ProcessList"] = _dbConnection.GetAuditChecklistCAD();

            return new FieldAuditObservationReplicaViewModel
                {
                EngagementId = engId,
                DefaultAmountInvolved = 0,
                DefaultNoOfInstances = 1,
                DefaultReplyDays = 3
                };
            }

        private FieldAuditGridReplicaViewModel BuildManageObservationBranchesReplicaViewModel(int engId)
            {
            ViewData["ProcessList"] = _dbConnection.GetAuditChecklistCAD();
            ViewData["AnnexList"] = _dbConnection.GetAnnexuresForChecklistDetail();
            ViewData["RiskList"] = _dbConnection.GetRisks();

            return new FieldAuditGridReplicaViewModel
                {
                EngagementId = engId
                };
            }

        private bool IsJoinAlreadySubmitted(int engId)
            {
            var details = _dbConnection.GetClosingDraftObservations(engId);
            return details.Any(item => item.ENG_PLAN_ID == engId && !string.IsNullOrWhiteSpace(item.JOINING_DATE));
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
            var completedSteps = selectedId > 0
                ? _progressStore.GetCompletedStepCodes(selectedId)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var joiningSubmitted = selectedId > 0 && IsJoinAlreadySubmitted(selectedId);

            foreach (var step in workflowSteps)
                {
                step.IsVisible = step.RequiredPermissionPageId > 0 && _permissionService.HasViewPermission(user, step.RequiredPermissionPageId);
                step.IsEnabled = step.IsVisible && selectedId > 0;

                var isPersistedComplete = selectedId > 0 && completedSteps.Contains(step.StepCode);
                var isBusinessComplete = selectedId > 0
                    && string.Equals(step.StepCode, "JOINING", StringComparison.OrdinalIgnoreCase)
                    && joiningSubmitted;

                step.IsCompleted = step.IsVisible && (isPersistedComplete || isBusinessComplete);
                step.IsSaved = step.IsCompleted;
                step.StatusText = step.IsCompleted ? "Completed" : "Pending";
                }

            var firstVisibleStep = workflowSteps.FirstOrDefault(step => step.IsVisible);
            var currentStep = workflowSteps.FirstOrDefault(step => step.IsVisible && string.Equals(step.StepCode, requestedStepCode, StringComparison.OrdinalIgnoreCase))
                ?? firstVisibleStep;

            return new FieldAuditWorkflowViewModel
                {
                SelectedEngagementId = selectedEngagementId,
                AvailableEngagements = engagementOptions,
                CurrentStepCode = currentStep?.StepCode,
                Steps = workflowSteps,
                CurrentStepContext = selectedEngagementId.GetValueOrDefault() > 0 && currentStep != null
                    ? BuildStepContext(currentStep.StepCode, selectedEngagementId.Value)
                    : null
                };
            }

        private List<FieldAuditWorkflowStepModel> BuildWorkflowSteps()
            {
            return new List<FieldAuditWorkflowStepModel>
                {
                CreateStep(1, "JOINING", "Joining", "~/Views/FieldAudit/_Join.cshtml", "/Engagement/Join"),
                CreateStep(2, "SAMPLING", "Sampling", "~/Views/FieldAudit/_Samples.cshtml", "/sampling/list_samples"),
                CreateStep(3, "EXCEPTION_REPORT", "Exception Report", "~/Views/FieldAudit/_Exception.cshtml", "/sampling/list_reports"),
                CreateStep(4, "WORKING_PAPER", "Working Paper", "~/Views/FieldAudit/_WPaper.cshtml", "/WorkingPaper/loan_case_file"),
                CreateStep(5, "MEMO_CREATION", "Observation", "~/Views/FieldAudit/_Observation.cshtml", "/Execution/cau_observation"),
                CreateStep(6, "MANAGE_OBSERVATION_BRANCHES", "Manage Observation", "~/Views/FieldAudit/_ManageObservationBranches.cshtml", "/Execution/manage_observations_branches"),
                CreateStep(7, "EXIT_AUDIT", "Closing", "~/Views/FieldAudit/_Closing.cshtml", "/Execution/closing"),
                CreateStep(8, "AUDIT_REPORT", "Audit Report", "~/Views/FieldAudit/_AuditReport.cshtml", "/FieldAuditReport/ReportOverview")
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

        private static FieldAuditStepContextViewModel BuildStepContext(string stepCode, int engId)
            {
            switch (stepCode)
                {
                case "JOINING":
                    return BuildStep(engId, "/Engagement/Join", "Joining", "Capture joining report data for the selected engagement.");
                case "SAMPLING":
                    return BuildStep(engId, "/sampling/list_samples", "Sampling", "Manage sampling records within this engagement context.");
                case "EXCEPTION_REPORT":
                    return BuildStep(engId, "/sampling/list_reports", "Exception Report", "Review and maintain exception report records for this engagement.");
                case "WORKING_PAPER":
                    return BuildStep(engId, "/WorkingPaper/loan_case_file", "Working Paper", "Maintain loan case files and working papers for the selected engagement.");
                case "MEMO_CREATION":
                    return BuildStep(engId, "/Execution/cau_observation", "Observation", "Create and manage observations inside dashboard (zone/branch forwarding removed in dashboard flow).");
                case "MANAGE_OBSERVATION_BRANCHES":
                    return BuildStep(engId, "/Execution/manage_observations_branches", "Manage Observation", "Manage branch observations directly in the dashboard for the selected engagement.");
                case "EXIT_AUDIT":
                    return BuildStep(engId, "/Execution/closing", "Closing", "Execute closing milestones for the selected engagement.");
                case "AUDIT_REPORT":
                    return BuildStep(engId, "/FieldAudit/OpenAuditReport", "Audit Report", "Open the field audit report module for the selected engagement.");
                default:
                    return BuildStep(engId, "/Engagement/task_list", "Field Audit", "Select a step from the workflow.");
                }
            }

        private static FieldAuditStepContextViewModel BuildStep(int engId, string basePath, string stepTitle, string description)
            {
            return new FieldAuditStepContextViewModel
                {
                EngagementId = engId,
                LegacyPath = $"{basePath}?engId={engId}",
                StepTitle = stepTitle,
                Description = description
                };
            }
        }
    }
