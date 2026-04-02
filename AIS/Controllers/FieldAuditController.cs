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
        private static readonly HashSet<string> PostJoiningSteps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            "MEMO_CREATION",
            "MANAGE_OBSERVATION_BRANCHES",
            "EXIT_AUDIT",
            "AUDIT_REPORT"
            };

        private static readonly HashSet<string> ClosingPerformedDisabledSteps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            "JOINING",
            "MEMO_CREATION",
            "EXIT_AUDIT"
            };

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
        public IActionResult AR_Dashboard(string stepCode = null, int? engId = null)
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
            return View("~/Views/FieldAudit/AR_Dashboard.cshtml", model);
            }

        [HttpGet]
        public IActionResult BO_Dashboard()
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

            ViewData["BackOfficeVisibleStepCodes"] = BuildVisibleBackOfficeStepCodes(user);
            return View("~/Views/FieldAudit/BO_Dashboard.cshtml");
            }

        [HttpGet]
        public IActionResult LoadBackOfficeStep(string stepCode, int engId, bool isReadOnly = false)
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

            if (!TryGetBackOfficePermissionContext(stepCode, out _, out var backOfficePageId, out var backOfficeErrorResult))
                {
                return backOfficeErrorResult;
                }

            if (!_permissionService.HasViewPermission(user, backOfficePageId))
                {
                return CreateStepAccessDeniedResult();
                }

            PopulateReplicaViewData(backOfficePageId);
            ViewData["AnnexList"] = _dbConnection.GetAnnexuresForChecklistDetail();
            ViewData["ProcessList"] = _dbConnection.GetAuditChecklist();
            ViewData["RiskList"] = _dbConnection.GetRisks();
            ViewData["ReadOnlyMode"] = isReadOnly;

            var viewModel = new FieldAuditGridReplicaViewModel
                {
                EngagementId = engId,
                IsReadOnly = isReadOnly
                };

            switch ((stepCode ?? string.Empty).Trim().ToUpperInvariant())
                {
                case "DRAFT_REPORT":
                    return PartialView("~/Views/FieldAudit/BO_Partials/_DraftReportPartial.cshtml", viewModel);
                case "QUALITY_REVIEW":
                    return PartialView("~/Views/FieldAudit/BO_Partials/_QualityReviewPartial.cshtml", viewModel);
                case "ISSUE_REPORT":
                    return PartialView("~/Views/FieldAudit/BO_Partials/_IssueReportPartial.cshtml", viewModel);
                case "CHECKING_DRAFT_REPORT":
                    viewModel.IsReadOnly = true;
                    ViewData["ReadOnlyMode"] = true;
                    return PartialView("~/Views/FieldAudit/BO_Partials/_CheckingDraftReportPartial.cshtml", viewModel);
                case "CHECKING_QUALITY_REVIEW":
                    viewModel.IsReadOnly = true;
                    ViewData["ReadOnlyMode"] = true;
                    return PartialView("~/Views/FieldAudit/BO_Partials/_CheckingQualityReviewPartial.cshtml", viewModel);
                default:
                    return NotFound();
                }
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

            return LoadStepPartial(step.StepCode, engId);
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
                statusId = engagementState.StatusId,
                isClose = engagementState.IsClose ?? string.Empty
                });
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
        public IActionResult LoadNestedStepView(string viewCode, int engId, int? reportId = null, int? loanStatus = null, int? disbId = null, int? sampleId = null, string acNo = null, string title = null, string desc = null, string sampleType = null)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!_sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized();
                }

            if (engId <= 0 || string.IsNullOrWhiteSpace(viewCode))
                {
                return BadRequest("A valid engagement is required.");
                }

            var model = BuildWorkflowViewModel(user, null, engId);
            if (!model.HasEngagementSelection)
                {
                return BadRequest("A valid engagement is required.");
                }

            if (!TryGetNestedReplicaPermissionContext(viewCode, out _, out var nestedPageId, out var nestedErrorResult))
                {
                return nestedErrorResult;
                }

            if (!_permissionService.HasViewPermission(user, nestedPageId))
                {
                return CreateStepAccessDeniedResult();
                }

            PopulateReplicaViewData(nestedPageId);
            ViewData["ReportId"] = reportId;
            ViewData["LoanStatus"] = loanStatus;
            ViewData["DisbId"] = disbId;
            ViewData["SampleId"] = sampleId;
            ViewData["AccountNo"] = acNo;
            ViewData["ReportTitle"] = title;
            ViewData["ReportDescription"] = desc;
            ViewData["SampleType"] = sampleType;

            switch ((viewCode ?? string.Empty).Trim().ToUpperInvariant())
                {
                case "EXCEPTION_ACCOUNT":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_ExceptionAccountReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "EXCEPTION_LOAN":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_ExceptionLoanReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "ACCOUNT_DOCUMENT":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_AccountDocumentReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "ACCOUNT_TRANSACTION":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_AccountTransactionReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "LOAN_TRANSACTION":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_LoanTransactionReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "LOAN_DOCUMENT":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_LoanDocumentReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "WP_VOUCHER":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_VoucherCheckingReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "WP_ACCOUNT_OPENING":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_AccountOpeningReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "WP_FIXED_ASSETS":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_FixedAssetsReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "WP_CASH_COUNT":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_CashCountReplica.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "SAMPLING_BIOMET":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_SamplingBiomet.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "SAMPLING_LOANS":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_SamplingLoans.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "SAMPLING_ACCOUNT_DOCUMENT":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_SamplingAccountDocument.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "SAMPLING_ACCOUNT_TRANSACTION":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_SamplingAccountTransaction.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "SAMPLING_LOAN_DOCUMENT":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_SamplingLoanDocument.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                case "SAMPLING_LOAN_TRANSACTION":
                    return PartialView("~/Views/FieldAudit/AR_Partials/_SamplingLoanTransaction.cshtml", new FieldAuditGridReplicaViewModel { EngagementId = engId });
                default:
                    return NotFound();
                }
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
                statusText = model.VisibleSteps.FirstOrDefault(item => string.Equals(item.StepCode, stepCode, StringComparison.OrdinalIgnoreCase))?.StatusText ?? "Saved"
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
                return RedirectToAction(nameof(AR_Dashboard));
                }

            var model = BuildWorkflowViewModel(user, "AUDIT_REPORT", engId);
            if (!model.HasEngagementSelection)
                {
                return RedirectToAction(nameof(AR_Dashboard));
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
            switch (step.StepCode)
                {
                case "JOINING":
                    var joinModel = BuildJoinReplicaViewModel(user, engId);
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

        private void PopulateReplicaViewData(int pageId)
            {
            ViewData["Layout"] = null;
            ViewData["HideTopHeader"] = true;
            ViewData["PageId"] = pageId;
            }

        private bool TryGetBackOfficePermissionContext(string stepCode, out string permissionPath, out int permissionPageId, out IActionResult errorResult)
            {
            permissionPath = null;
            permissionPageId = 0;
            errorResult = null;

            permissionPath = ResolveBackOfficePermissionPath(stepCode);
            if (string.IsNullOrWhiteSpace(permissionPath))
                {
                errorResult = BadRequest("Invalid Back Office workflow step.");
                return false;
                }

            if (!_pageIdResolver.TryResolvePageId(permissionPath, out permissionPageId) || permissionPageId <= 0)
                {
                errorResult = Forbid();
                return false;
                }

            return true;
            }

        private List<string> BuildVisibleBackOfficeStepCodes(SessionUser user)
            {
            var stepCodes = new[]
                {
                "DRAFT_REPORT",
                "QUALITY_REVIEW",
                "ISSUE_REPORT",
                "CHECKING_DRAFT_REPORT",
                "CHECKING_QUALITY_REVIEW"
                };

            return stepCodes
                .Where(stepCode =>
                    TryGetBackOfficePermissionContext(stepCode, out _, out var pageId, out _)
                    && pageId > 0
                    && _permissionService.HasViewPermission(user, pageId))
                .ToList();
            }

        private static string ResolveBackOfficePermissionPath(string stepCode)
            {
            switch ((stepCode ?? string.Empty).Trim().ToUpperInvariant())
                {
                case "DRAFT_REPORT":
                    return "/Execution/manage_draft_report_paras_branch";
                case "QUALITY_REVIEW":
                    return "/Execution/pre_concluding_audit";
                case "ISSUE_REPORT":
                    return "/Execution/Concluding_Closing_Audit";
                case "CHECKING_DRAFT_REPORT":
                    return "/FAD/Draft_report_Checking";
                case "CHECKING_QUALITY_REVIEW":
                    return "/FAD/Quality_Assurance_checking";
                default:
                    return null;
                }
            }

        private PartialViewResult CreateStepAccessDeniedResult()
            {
            Response.StatusCode = 403;
            return PartialView("~/Views/Shared/_DashboardStepAccessDenied.cshtml");
            }

        private bool TryGetNestedReplicaPermissionContext(string viewCode, out string permissionPath, out int permissionPageId, out IActionResult errorResult)
            {
            permissionPath = null;
            permissionPageId = 0;
            errorResult = null;

            switch ((viewCode ?? string.Empty).Trim().ToUpperInvariant())
                {
                case "EXCEPTION_ACCOUNT":
                    permissionPath = "/Sampling/Account_exception";
                    break;
                case "EXCEPTION_LOAN":
                    permissionPath = "/Sampling/loans_Exception";
                    break;
                case "ACCOUNT_DOCUMENT":
                case "SAMPLING_ACCOUNT_DOCUMENT":
                    permissionPath = "/Sampling/account_document";
                    break;
                case "ACCOUNT_TRANSACTION":
                case "SAMPLING_ACCOUNT_TRANSACTION":
                    permissionPath = "/Sampling/account_transaction";
                    break;
                case "LOAN_DOCUMENT":
                case "SAMPLING_LOAN_DOCUMENT":
                    permissionPath = "/Sampling/loan_documents";
                    break;
                case "LOAN_TRANSACTION":
                case "SAMPLING_LOAN_TRANSACTION":
                    permissionPath = "/Sampling/loan_transactions";
                    break;
                case "WP_VOUCHER":
                    permissionPath = "/WorkingPaper/voucher_checking";
                    break;
                case "WP_ACCOUNT_OPENING":
                    permissionPath = "/WorkingPaper/account_opening";
                    break;
                case "WP_FIXED_ASSETS":
                    permissionPath = "/WorkingPaper/fixed_assets";
                    break;
                case "WP_CASH_COUNT":
                    permissionPath = "/WorkingPaper/cash_count";
                    break;
                case "SAMPLING_BIOMET":
                    permissionPath = "/Sampling/biomet";
                    break;
                case "SAMPLING_LOANS":
                    permissionPath = "/Sampling/loans";
                    break;
                default:
                    errorResult = BadRequest("Invalid workflow replica view.");
                    return false;
                }

            if (!_pageIdResolver.TryResolvePageId(permissionPath, out permissionPageId) || permissionPageId <= 0)
                {
                errorResult = Forbid();
                return false;
                }

            return true;
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
                    EngStatus = item.ENG_STATUS,
                    StartDate = item.AUDIT_START_DATE,
                    EndDate = item.AUDIT_END_DATE,
                    StageName = item.ENG_STATUS,
                    StatusId = item.STATUS_ID,
                    IsClose = item.ISCLOSE
                    })
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
                    ApplyWorkflowAvailability(step, selectedEngagement.StatusId, selectedEngagement.IsClose);
                    }

                var isPersistedComplete = selectedId > 0 && completedSteps.Contains(step.StepCode);
                var isBusinessComplete = selectedId > 0
                    && string.Equals(step.StepCode, "JOINING", StringComparison.OrdinalIgnoreCase)
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
                Steps = workflowSteps,
                CurrentStepContext = selectedEngagementId.GetValueOrDefault() > 0 && currentStep != null
                    ? BuildStepContext(currentStep.StepCode, selectedEngagementId.Value)
                    : null
                };
            }

        private FieldAuditEngagementOptionModel GetEngagementOption(int engId)
            {
            if (engId <= 0)
                {
                return null;
                }

            return _dbConnection.GetTaskList()
                .Where(item => item.ENG_PLAN_ID == engId)
                .GroupBy(item => item.ENG_PLAN_ID)
                .Select(group => group.First())
                .Select(item => new FieldAuditEngagementOptionModel
                    {
                    EngagementId = item.ENG_PLAN_ID,
                    EntityName = item.ENTITY_NAME,
                    StageName = item.ENG_STATUS,
                    StatusId = item.STATUS_ID,
                    IsClose = item.ISCLOSE
                    })
                .FirstOrDefault();
            }

        private static void ApplyWorkflowAvailability(FieldAuditWorkflowStepModel step, int statusId, string isClose)
            {
            if (step == null || !step.IsEnabled)
                {
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
                return;
                }

            if (string.Equals(step.StepCode, "EXIT_AUDIT", StringComparison.OrdinalIgnoreCase)
                && !string.Equals((isClose ?? string.Empty).Trim(), "Z", StringComparison.OrdinalIgnoreCase))
                {
                step.IsEnabled = false;
                step.DisabledMessage = "Closing is not available yet.";
                }
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
