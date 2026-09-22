using AIS.Models.WorkingPaperV2;
using AIS.Services.WorkingPaperV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace AIS.Controllers
    {
    [Authorize]
    [AutoValidateAntiforgeryToken]
    [Route("WorkingPapers")]
    public sealed class WorkingPapersController : BaseController
        {
        private readonly DBConnection _db;
        private readonly TopMenus _menus;

        public WorkingPapersController(SessionHandler sessionHandler, DBConnection db, TopMenus menus) : base(sessionHandler)
            {
            _db = db;
            _menus = menus;
            }

        [HttpGet("")]
        public IActionResult Index(long engId, string paperType = WorkingPaperTypes.LoanCase)
            {
            if (!IsEnabled()) return NotFound();
            if (!HasDashboardAccess(engId)) return Forbid();
            return RedirectToAction("AR_Dashboard", "FieldAudit", new { engId, stepCode = "WORKING_PAPER" });
            }

        [HttpGet("api/summary")]
        public IActionResult Summary(long engId)
            {
            if (!IsEnabled()) return NotFound();
            if (!HasDashboardAccess(engId)) return Forbid();
            var paperTypes = new[]
            {
                WorkingPaperTypes.LoanCase,
                WorkingPaperTypes.Voucher,
                WorkingPaperTypes.AccountOpening,
                WorkingPaperTypes.FixedAsset,
                WorkingPaperTypes.CashCount
            };
            var papers = paperTypes.Select(type =>
                {
                var workspace = _db.GetWorkingPaperWorkspace(0, engId, type);
                var completedItems = workspace.Items.Count(item => !string.Equals(item.Result, "PENDING", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(item.Result, "NOT_TESTED", StringComparison.OrdinalIgnoreCase));
                var progress = workspace.WorkingPaperId <= 0 ? 0 : workspace.Items.Count == 0 ? 10
                    : Math.Min(95, 20 + (completedItems * 60 / workspace.Items.Count) + (string.IsNullOrWhiteSpace(workspace.Conclusion?.OverallConclusion) ? 0 : 15));
                if (string.Equals(workspace.Status, WorkingPaperStatuses.Approved, StringComparison.OrdinalIgnoreCase)) progress = 100;
                return new { paperType = type, status = workspace.WorkingPaperId <= 0 ? "NOT_STARTED" : workspace.Status, progress, workingPaperId = workspace.WorkingPaperId };
                });
            return Json(papers);
            }

        [HttpGet("api/workspace")]
        public IActionResult Workspace(long engId, string paperType, long workingPaperId = 0)
            {
            if (!IsEnabled()) return NotFound();
            if (!HasDashboardAccess(engId)) return Forbid();
            if (engId <= 0 || !WorkingPaperTypes.All.Contains(paperType)) return BadRequest(new { message = "Invalid engagement or paper type." });
            return Json(_db.GetWorkingPaperWorkspace(workingPaperId, engId, paperType.ToUpperInvariant()));
            }

        [HttpPost("api/create")]
        public IActionResult Create([FromBody] CreateWorkingPaperRequest request)
            {
            if (!IsEnabled()) return NotFound();
            if (!HasDashboardAccess(request?.EngagementId ?? 0)) return Forbid();
            if (!ModelState.IsValid || !WorkingPaperTypes.All.Contains(request?.PaperType)) return ValidationProblem(ModelState);
            request.PaperType = request.PaperType.ToUpperInvariant();
            return Operation(_db.CreateWorkingPaper(request));
            }

        [HttpPost("api/plan")]
        public IActionResult SavePlan([FromBody] SavePlanRequest request)
            {
            if (!CanModifyActiveEngagement()) return Forbid();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var errors = WorkingPaperRules.ValidatePlan(request.Plan);
            if (errors.Count > 0) return BadRequest(new { message = "Plan validation failed.", errors });
            return Operation(_db.SaveWorkingPaperPlan(request));
            }

        [HttpPost("api/item")]
        public IActionResult SaveItem([FromBody] SaveItemRequest request, [FromQuery] string paperType)
            {
            if (!CanModifyActiveEngagement()) return Forbid();
            if (!ModelState.IsValid || !WorkingPaperTypes.All.Contains(paperType)) return ValidationProblem(ModelState);
            request.Result = request.Result?.ToUpperInvariant();
            var errors = WorkingPaperRules.ValidateItem(paperType, request.Details, request.Result);
            if (errors.Count > 0) return BadRequest(new { message = "Test-item validation failed.", errors });
            var calculations = WorkingPaperRules.Calculate(paperType, request.Details);
            return Operation(_db.SaveWorkingPaperItem(request, calculations));
            }

        [HttpPost("api/evidence")]
        public IActionResult LinkEvidence([FromBody] WorkingPaperEvidenceModel request)
            {
            if (!CanModifyActiveEngagement()) return Forbid();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return Operation(_db.SaveWorkingPaperEvidence(request));
            }

        [HttpPost("api/exception")]
        public IActionResult SaveException([FromBody] WorkingPaperExceptionModel request)
            {
            if (!CanModifyActiveEngagement()) return Forbid();
            if (!ModelState.IsValid || request.ItemId <= 0 || request.WorkingPaperId <= 0) return ValidationProblem(ModelState);
            request.RiskRating = request.RiskRating?.ToUpperInvariant();
            return Operation(_db.SaveWorkingPaperException(request));
            }

        [HttpPost("api/conclusion")]
        public IActionResult SaveConclusion([FromBody] SaveConclusionRequest request)
            {
            if (!CanModifyActiveEngagement()) return Forbid();
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request?.Conclusion?.OverallConclusion)) return ValidationProblem(ModelState);
            request.Conclusion.ObjectiveAssessment = request.Conclusion.ObjectiveAssessment?.ToUpperInvariant();
            return Operation(_db.SaveWorkingPaperConclusion(request));
            }

        [HttpPost("api/review-note")]
        public IActionResult SaveReviewNote([FromBody] WorkingPaperReviewNoteModel request)
            {
            if (!CanModifyActiveEngagement()) return Forbid();
            if (!ModelState.IsValid || request.WorkingPaperId <= 0) return ValidationProblem(ModelState);
            request.Status = string.IsNullOrWhiteSpace(request.Status) ? "OPEN" : request.Status.ToUpperInvariant();
            return Operation(_db.SaveWorkingPaperReviewNote(request));
            }

        [HttpPost("api/workflow")]
        public IActionResult Workflow([FromBody] WorkflowActionRequest request)
            {
            if (!CanModifyActiveEngagement()) return Forbid();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            request.Action = request.Action.ToUpperInvariant();
            return Operation(_db.ApplyWorkingPaperWorkflow(request));
            }

        private IActionResult Operation(WorkingPaperOperationResult result) =>
            result.Success ? Ok(result) : BadRequest(result);

        private static bool IsEnabled() => false;

        private bool HasDashboardAccess(long engagementId)
            {
            if (engagementId <= 0 || engagementId > int.MaxValue) return false;
            return _db.GetArDashboardDropdownOptions((int)engagementId).Any(item => item.EngagementId == engagementId);
            }

        private bool CanModifyActiveEngagement()
            {
            if (!IsEnabled() || !SessionHandler.TryGetActiveEngagementId(out var engagementId)) return false;
            return HasDashboardAccess(engagementId);
            }
        }
    }
