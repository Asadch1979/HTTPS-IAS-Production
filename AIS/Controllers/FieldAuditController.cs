using AIS.Models;
using AIS.Models.Requests;
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
        private readonly IPermissionService _permissionService;
        private readonly DBConnection _dbConnection;
        private readonly IPageIdResolver _pageIdResolver;

        public FieldAuditController(ILogger<FieldAuditController> logger, SessionHandler sessionHandler, DBConnection dbConnection, TopMenus topMenus, IPermissionService permissionService, IPageIdResolver pageIdResolver)
        {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _topMenus = topMenus;
            _permissionService = permissionService;
            _pageIdResolver = pageIdResolver;
        }

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

        [HttpPost]
        public IActionResult SaveJoining([FromBody] AddJoiningPostModel request)
        {
            if (!User.Identity.IsAuthenticated || !_sessionHandler.TryGetUser(out var user) || user == null)
            {
                return Unauthorized();
            }

            var step = BuildWorkflowSteps().FirstOrDefault(s => s.StepCode == "JOINING");
            if (step == null || !_permissionService.HasViewPermission(user, step.RequiredPermissionPageId))
            {
                return Forbid();
            }

            if (request == null || !request.ENG_PLAN_ID.HasValue || !request.TEAM_MEM_PPNO.HasValue)
            {
                return BadRequest(new { status = false, message = "Invalid request." });
            }

            var model = new AddJoiningModel
            {
                ID = request.ID ?? 0,
                ENG_PLAN_ID = request.ENG_PLAN_ID.Value,
                TEAM_MEM_PPNO = request.TEAM_MEM_PPNO.Value,
                JOINING_DATE = request.JOINING_DATE,
                COMPLETION_DATE = request.COMPLETION_DATE
            };

            var message = _dbConnection.AddJoiningReport(model);
            return Json(new { status = true, message });
        }

        private FieldAuditWorkflowViewModel BuildWorkflowViewModel(SessionUser user, string requestedStepCode, int? engId)
        {
            var allEngagements = _dbConnection.GetTaskList() ?? new List<TaskListModel>();
            var workflowSteps = BuildWorkflowSteps();

            foreach (var step in workflowSteps)
            {
                step.IsVisible = step.RequiredPermissionPageId > 0 && _permissionService.HasViewPermission(user, step.RequiredPermissionPageId);
                step.IsEnabled = step.IsVisible;
                step.IsSaved = step.IsVisible;
                step.IsCompleted = false;
                step.StatusText = step.IsSaved ? "Saved" : "Pending";
            }

            var firstVisible = workflowSteps.FirstOrDefault(s => s.IsVisible);
            var selectedStep = workflowSteps.FirstOrDefault(s => s.IsVisible && string.Equals(s.StepCode, requestedStepCode, StringComparison.OrdinalIgnoreCase)) ?? firstVisible;

            var model = new FieldAuditWorkflowViewModel
            {
                SelectedEngagementId = engId,
                CurrentStepCode = selectedStep?.StepCode,
                Steps = workflowSteps,
                AvailableEngagements = allEngagements
            };

            if (engId.HasValue && engId.Value > 0)
            {
                model.ReportOverview = _dbConnection.GetFieldAuditReportOverview(engId.Value);
                model.ReportChecklist = _dbConnection.GetFieldAuditReportChecklist(engId.Value);
            }

            return model;
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
    }
}
