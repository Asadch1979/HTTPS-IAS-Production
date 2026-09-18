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
            var (_, error) = GetUserOrRedirect();
            if (error != null) return error;
            if (engId <= 0 || !WorkingPaperTypes.All.Contains(paperType)) return BadRequest("A valid engagement and paper type are required.");
            ViewData["TopMenu"] = _menus.GetTopMenus();
            ViewData["TopMenuPages"] = _menus.GetTopMenusPages();
            ViewData["EngagementId"] = engId;
            ViewData["PaperType"] = paperType.ToUpperInvariant();
            return View();
            }

        [HttpGet("api/workspace")]
        public IActionResult Workspace(long engId, string paperType, long workingPaperId = 0)
            {
            if (engId <= 0 || !WorkingPaperTypes.All.Contains(paperType)) return BadRequest(new { message = "Invalid engagement or paper type." });
            return Json(_db.GetWorkingPaperWorkspace(workingPaperId, engId, paperType.ToUpperInvariant()));
            }

        [HttpPost("api/create")]
        public IActionResult Create([FromBody] CreateWorkingPaperRequest request)
            {
            if (!ModelState.IsValid || !WorkingPaperTypes.All.Contains(request?.PaperType)) return ValidationProblem(ModelState);
            request.PaperType = request.PaperType.ToUpperInvariant();
            return Operation(_db.CreateWorkingPaper(request));
            }

        [HttpPost("api/plan")]
        public IActionResult SavePlan([FromBody] SavePlanRequest request)
            {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var errors = WorkingPaperRules.ValidatePlan(request.Plan);
            if (errors.Count > 0) return BadRequest(new { message = "Plan validation failed.", errors });
            return Operation(_db.SaveWorkingPaperPlan(request));
            }

        [HttpPost("api/item")]
        public IActionResult SaveItem([FromBody] SaveItemRequest request, [FromQuery] string paperType)
            {
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
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return Operation(_db.SaveWorkingPaperEvidence(request));
            }

        [HttpPost("api/exception")]
        public IActionResult SaveException([FromBody] WorkingPaperExceptionModel request)
            {
            if (!ModelState.IsValid || request.ItemId <= 0 || request.WorkingPaperId <= 0) return ValidationProblem(ModelState);
            request.RiskRating = request.RiskRating?.ToUpperInvariant();
            return Operation(_db.SaveWorkingPaperException(request));
            }

        [HttpPost("api/conclusion")]
        public IActionResult SaveConclusion([FromBody] SaveConclusionRequest request)
            {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request?.Conclusion?.OverallConclusion)) return ValidationProblem(ModelState);
            request.Conclusion.ObjectiveAssessment = request.Conclusion.ObjectiveAssessment?.ToUpperInvariant();
            return Operation(_db.SaveWorkingPaperConclusion(request));
            }

        [HttpPost("api/review-note")]
        public IActionResult SaveReviewNote([FromBody] WorkingPaperReviewNoteModel request)
            {
            if (!ModelState.IsValid || request.WorkingPaperId <= 0) return ValidationProblem(ModelState);
            request.Status = string.IsNullOrWhiteSpace(request.Status) ? "OPEN" : request.Status.ToUpperInvariant();
            return Operation(_db.SaveWorkingPaperReviewNote(request));
            }

        [HttpPost("api/workflow")]
        public IActionResult Workflow([FromBody] WorkflowActionRequest request)
            {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            request.Action = request.Action.ToUpperInvariant();
            return Operation(_db.ApplyWorkingPaperWorkflow(request));
            }

        private IActionResult Operation(WorkingPaperOperationResult result) =>
            result.Success ? Ok(result) : BadRequest(result);
        }
    }
