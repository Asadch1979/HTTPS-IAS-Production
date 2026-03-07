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
    public class FieldAuditController : Controller
        {
        private readonly ILogger<FieldAuditController> _logger;
        private readonly TopMenus _topMenus;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;

        public FieldAuditController(
            ILogger<FieldAuditController> logger,
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

            var stepModel = BuildStepContext(step.StepCode, engId);
            return PartialView(step.PartialViewName, stepModel);
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
            foreach (var step in workflowSteps)
                {
                step.IsVisible = step.RequiredPermissionPageId > 0 && _permissionService.HasViewPermission(user, step.RequiredPermissionPageId);
                step.IsEnabled = step.IsVisible && selectedEngagementId.GetValueOrDefault() > 0;
                step.IsSaved = selectedEngagementId.GetValueOrDefault() > 0 && step.IsVisible;
                step.IsCompleted = false;
                step.StatusText = step.IsSaved ? "Saved" : "Pending";
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
                CreateStep(1, "JOINING", "Joining", "~/Views/FieldAudit/Partials/_JoiningStep.cshtml", "/Engagement/Join"),
                CreateStep(2, "SAMPLING", "Sampling", "~/Views/FieldAudit/Partials/_SamplingStep.cshtml", "/sampling/list_samples"),
                CreateStep(3, "EXCEPTION_REPORT", "Exception Report", "~/Views/FieldAudit/Partials/_ExceptionReportStep.cshtml", "/sampling/list_reports"),
                CreateStep(4, "WORKING_PAPER", "Working Paper", "~/Views/FieldAudit/Partials/_WorkingPaperStep.cshtml", "/WorkingPaper/loan_case_file"),
                CreateStep(5, "MEMO_CREATION", "Memo Creation", "~/Views/FieldAudit/Partials/_MemoCreationStep.cshtml", "/Execution/cau_observation"),
                CreateStep(6, "SUBMIT_TO_AUDITEE", "Submit to Auditee", "~/Views/FieldAudit/Partials/_SubmitToAuditeeStep.cshtml", "/Execution/manage_observations_branches"),
                CreateStep(7, "EXIT_AUDIT", "Exit Audit", "~/Views/FieldAudit/Partials/_ExitAuditStep.cshtml", "/Execution/closing"),
                CreateStep(8, "DRAFT_REPORT", "Draft Report", "~/Views/FieldAudit/Partials/_DraftReportStep.cshtml", "/Execution/manage_observations_branches"),
                CreateStep(9, "AUDIT_REPORT_COMPILATION", "Audit Report Compilation", "~/Views/FieldAudit/Partials/_AuditReportCompilationStep.cshtml", "/FieldAuditReport/ReportOverview")
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
                    return BuildStep(engId, "/Execution/cau_observation", "Memo Creation", "Create and manage memo observations (cross zone/branch forwarding removed in dashboard flow).");
                case "SUBMIT_TO_AUDITEE":
                    return BuildStep(engId, "/Execution/manage_observations_branches", "Submit to Auditee", "Submit observations to auditee (add-to-draft flow intentionally excluded).");
                case "EXIT_AUDIT":
                    return BuildStep(engId, "/Execution/closing", "Exit Audit", "Execute closing and exit audit milestones for the selected engagement.");
                case "DRAFT_REPORT":
                    return BuildStep(engId, "/Execution/manage_observations_branches", "Draft Report", "Prepare draft report package (submit-to-auditee flow intentionally excluded).");
                case "AUDIT_REPORT_COMPILATION":
                    return BuildStep(engId, "/FieldAuditReport/ReportOverview", "Audit Report Compilation", "Compile the final field audit report overview and narrative sections.");
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
