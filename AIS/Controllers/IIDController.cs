using AIS.Models;
using AIS.Models.IID;
using AIS.Models.SM;
using AIS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIS.Controllers
    {
    public class IIDController : Controller
        {
        private const string IidDashboardPath = "/IID/IID_Dashboard";
        private readonly ILogger<IIDController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly DBConnection dBConnection;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;

        public IIDController(
            ILogger<IIDController> logger,
            SessionHandler _sessionHandler,
            DBConnection _dbCon,
            TopMenus _tpMenu,
            IWebHostEnvironment _hostingEnvironment,
            IPermissionService permissionService,
            IPageIdResolver pageIdResolver)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            hostingEnvironment = _hostingEnvironment;
            _permissionService = permissionService;
            _pageIdResolver = pageIdResolver;
            }

        public override void OnActionExecuting(ActionExecutingContext context)
            {
            base.OnActionExecuting(context);

            var u = sessionHandler.GetUser();
            ViewData["UserId"] = u?.PPNumber ?? "";
            }

        [HttpGet, HttpPost]
        public IActionResult SubmitComplaint()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["RegionList"] = dBConnection.GetRBHList(0);
            return View("../IID/SubmitComplaint");
            }

        [HttpGet]
        public IActionResult IID_Dashboard(string stepCode = null, string utilityCode = null, int? complaintId = null)
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

            var model = BuildIidDashboardViewModel(user, stepCode, utilityCode, complaintId ?? 0);
            ViewData["PageId"] = model.DashboardPageId;
            return View("../IID/IID_Dashboard", model);
            }

        [HttpGet]
        public IActionResult LoadDashboardPanel(string panelKey, int complaintId = 0)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized();
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return Unauthorized();
                }

            var model = BuildIidDashboardViewModel(user, panelKey, panelKey, complaintId);
            var panel = model.Steps.Concat(model.Utilities)
                .FirstOrDefault(item => string.Equals(item.ItemKey, panelKey, StringComparison.OrdinalIgnoreCase));

            if (panel == null)
                {
                return NotFound();
                }

            if (!panel.IsVisible)
                {
                return CreateDashboardAccessDeniedResult();
                }

            if (panel.RequiresComplaintSelection && complaintId <= 0)
                {
                return Content("<div class=\"alert alert-info mb-0\">Select a complaint from the dashboard selector to open this work area.</div>", "text/html");
                }

            var dashboardPageId = model.DashboardPageId;
            var panelPageId = panel.RequiredPermissionPageId > 0 ? panel.RequiredPermissionPageId : dashboardPageId;
            ViewData["DashboardMode"] = true;
            ViewData["ComplaintId"] = complaintId;
            ViewData["PageId"] = panelPageId;
            ViewData["DashboardPageId"] = model.ComplaintDropdownPageId;

            switch ((panel.ItemKey ?? string.Empty).Trim().ToUpperInvariant())
                {
                case "COMPLAINT":
                    ViewData["RegionList"] = dBConnection.GetRBHList(0);
                    return PartialView("../IID/SubmitComplaint", new ComplaintModel());

                case "INITIAL_ASSESSMENT":
                    return PartialView("../IID/InitialAssessment", new InitialAssessmentModel { ComplaintId = complaintId });

                case "HEAD_REVIEW":
                    return PartialView("../IID/HeadReview", new HeadReviewModel
                        {
                        ComplaintId = complaintId
                        });

                case "INVESTIGATION_PLAN":
                    return PartialView("../IID/InvestigationPlan", new InvestigationPlanModel { ComplaintId = complaintId });

                case "PLAN_APPROVAL":
                    ViewData["PlanId"] = ResolvePlanIdByComplaintId(complaintId);
                    return PartialView("../IID/PlanApproval", new PlanApprovalModel());

                case "INQUIRY_REPORT":
                    return PartialView("../IID/InquiryReport", new InquiryReportModel());

                case "ANALYSIS":
                    ViewData["ComplaintId"] = complaintId;
                    ViewData["ReportId"] = ResolveReportIdByComplaintId(complaintId);
                    return PartialView("../IID/Analysis", new AnalysisModel());

                case "CASE_STUDY":
                    return PartialView("../IID/CaseStudy", new CaseStudyModel { ComplaintId = complaintId });

                case "FINAL_APPROVAL":
                    ViewData["ReportId"] = ResolveReportIdByComplaintId(complaintId);
                    return PartialView("../IID/FinalApproval", new FinalApprovalModel());

                case "TASK_LIST":
                    return PartialView("../IID/TaskListIID");

                case "MONITORING_DASHBOARD":
                    return PartialView("../IID/MonitoringDashboard", dBConnection.GetComplaintsByUser());

                case "READ_ONLY_REPORT":
                    return PartialView("../IID/InquiryReportReadOnly");

                case "REPORTS":
                    ViewData["EngId"] = complaintId;
                    return PartialView("../IID/Exceptions_Reports");

                default:
                    return NotFound();
                }
            }

        [HttpGet, HttpPost]
        public IActionResult InitialAssessment(int complaintId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");

            ViewData["ComplaintId"] = complaintId;
            return View("../IID/InitialAssessment");
            }

        [HttpGet, HttpPost]
        public IActionResult HeadReview(int complaintId, int assessmentId = 0)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = complaintId;
            ViewData["AssessmentId"] = assessmentId;
            return View("../IID/HeadReview");
            }

        [HttpGet, HttpPost]
        public IActionResult InvestigationPlan(int complaintId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = complaintId;
            return View("../IID/InvestigationPlan");
            }

        [HttpGet, HttpPost]
        public IActionResult PlanApproval(int planId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = dBConnection.GetComplaintIdByPlanId(planId) ?? 0;
            ViewData["PlanId"] = planId;
            return View("../IID/PlanApproval");
            }

        [HttpGet, HttpPost]
        public IActionResult InquiryReport(int complaintId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = complaintId;
            return View("../IID/InquiryReport");
            }

        [HttpGet, HttpPost]
        public IActionResult Analysis(int reportId = 0, int complaintId = 0)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");

            if (complaintId <= 0 && reportId > 0)
                {
                complaintId = dBConnection.GetComplaintIdByReportId(reportId) ?? 0;
                }

            if (reportId <= 0 && complaintId > 0)
                {
                reportId = ResolveReportIdByComplaintId(complaintId);
                }

            ViewData["ComplaintId"] = complaintId;
            ViewData["ReportId"] = reportId;
            return View("../IID/Analysis");
            }

        [HttpGet, HttpPost]
        public IActionResult FinalApproval(int reportId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = dBConnection.GetComplaintIdByReportId(reportId) ?? 0;
            ViewData["ReportId"] = reportId;
            return View("../IID/FinalApproval");
            }

        [HttpGet, HttpPost]
        public IActionResult CaseStudy(int complaintId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = complaintId;
            return View("../IID/CaseStudy");
            }

        [HttpGet, HttpPost]
        public IActionResult Reports(int? complaintId = null, int? engId = null)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = complaintId ?? engId ?? 0;
            ViewData["EngId"] = engId ?? complaintId ?? 0;
            return View("../IID/Exceptions_Reports");
            }

        [HttpGet, HttpPost]
        public IActionResult Exceptions_Reports(int? engId = null, int? complaintId = null)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = complaintId ?? engId ?? 0;
            ViewData["EngId"] = engId ?? complaintId ?? 0;
            return View("../IID/Exceptions_Reports");
            }

        [HttpGet, HttpPost]
        public IActionResult Add_Exception_Reports(int? engId = null, int? complaintId = null, int? reportId = null)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = complaintId ?? engId ?? 0;
            ViewData["EngId"] = engId ?? complaintId ?? 0;
            ViewData["ReportId"] = reportId ?? 0;
            ViewData["LoanStatusList"] = dBConnection.GetLoanStatus();
            ViewBag.AllowedColumns = ExceptionReportFormatModel.AllowedColumnNames;
            return View("../IID/Add_Exception_Reports");
            }

        [HttpGet, HttpPost]
        public IActionResult Account_exception()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            return View("../IID/Account_exception");
            }

        [HttpGet, HttpPost]
        public IActionResult loans_exception()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            return View("../IID/loans_exception");
            }

        [HttpGet, HttpPost]
        public IActionResult account_document()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            return View("../IID/account_document");
            }

        [HttpGet, HttpPost]
        public IActionResult account_transaction()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            return View("../IID/account_transaction");
            }

        [HttpGet, HttpPost]
        public IActionResult loan_documents()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            return View("../IID/loan_documents");
            }

        [HttpGet, HttpPost]
        public IActionResult loan_transactions()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            return View("../IID/loan_transactions");
            }

        [HttpGet, HttpPost]
        public IActionResult TaskListIID()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            return View("../IID/TaskListIID");
            }

        [HttpGet]
        public IActionResult MonitoringDashboard()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            var rows = dBConnection.GetComplaintsByUser();
            return View("../IID/MonitoringDashboard", rows);
            }

        [HttpGet]
        public IActionResult InquiryReportReadOnly(int complaintId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["ComplaintId"] = complaintId;
            return View("../IID/InquiryReportReadOnly");
            }

        private IidDashboardViewModel BuildIidDashboardViewModel(SessionUser user, string requestedStepCode, string requestedUtilityCode, int complaintId)
            {
            var dashboardPageId = ResolveDashboardPageId(IidDashboardPath);
            var items = new List<IidDashboardItemModel>
                {
                CreateDashboardItem(1, "COMPLAINT", "Complaint", "/IID/SubmitComplaint", requiresComplaintSelection: false, reloadOnComplaintChange: false, isStep: true),
                CreateDashboardItem(2, "INITIAL_ASSESSMENT", "Initial Assessment", "/IID/InitialAssessment", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: true),
                CreateDashboardItem(3, "HEAD_REVIEW", "Head Review", "/IID/HeadReview", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: true),
                CreateDashboardItem(4, "INVESTIGATION_PLAN", "Investigation Plan", "/IID/InvestigationPlan", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: true),
                CreateDashboardItem(5, "PLAN_APPROVAL", "Plan Approval", "/IID/PlanApproval", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: true),
                CreateDashboardItem(6, "INQUIRY_REPORT", "Inquiry Report", "/IID/InquiryReport", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: true),
                CreateDashboardItem(7, "ANALYSIS", "Analysis", "/IID/Analysis", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: true),
                CreateDashboardItem(8, "CASE_STUDY", "Case Study", "/IID/CaseStudy", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: true),
                CreateDashboardItem(9, "FINAL_APPROVAL", "Finalize Report", "/IID/FinalApproval", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: true),
                CreateDashboardItem(0, "TASK_LIST", "Task List", "/IID/TaskListIID", requiresComplaintSelection: false, reloadOnComplaintChange: false, isStep: false),
                CreateDashboardItem(0, "MONITORING_DASHBOARD", "Monitoring Dashboard", "/IID/MonitoringDashboard", requiresComplaintSelection: false, reloadOnComplaintChange: false, isStep: false),
                CreateDashboardItem(0, "READ_ONLY_REPORT", "Read Only Report", "/IID/InquiryReportReadOnly", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: false),
                CreateDashboardItem(0, "REPORTS", "Exceptions_Reports", "/IID/Reports", requiresComplaintSelection: true, reloadOnComplaintChange: true, isStep: false)
                };

            foreach (var item in items)
                {
                item.RequiredPermissionPageId = ResolveDashboardPageId(item.SourcePath);
                item.IsVisible = item.RequiredPermissionPageId > 0 && _permissionService.HasViewPermission(user, item.RequiredPermissionPageId);
                item.IsEnabled = !item.RequiresComplaintSelection || complaintId > 0;
                item.DisabledMessage = item.IsEnabled
                    ? string.Empty
                    : "Select a complaint first to open this work area.";
                }

            var steps = items.Where(item => item.IsStep && item.IsVisible).OrderBy(item => item.SequenceNo).ToList();
            var utilities = items.Where(item => !item.IsStep && item.IsVisible).ToList();

            var requestedUtility = utilities.FirstOrDefault(item => string.Equals(item.ItemKey, requestedUtilityCode, StringComparison.OrdinalIgnoreCase));
            var requestedStep = steps.FirstOrDefault(item => string.Equals(item.ItemKey, requestedStepCode, StringComparison.OrdinalIgnoreCase));
            var currentItemKey = requestedUtility?.ItemKey
                ?? requestedStep?.ItemKey
                ?? steps.FirstOrDefault()?.ItemKey
                ?? utilities.FirstOrDefault()?.ItemKey
                ?? "COMPLAINT";
            var complaintDropdownPageId = ResolveComplaintDropdownPageId(dashboardPageId, items);

            return new IidDashboardViewModel
                {
                DashboardPageId = dashboardPageId,
                ComplaintDropdownPageId = complaintDropdownPageId,
                CurrentItemKey = currentItemKey,
                SelectedComplaintId = complaintId,
                Steps = steps,
                Utilities = utilities
                };
            }

        private IidDashboardItemModel CreateDashboardItem(
            int sequenceNo,
            string itemKey,
            string title,
            string sourcePath,
            bool requiresComplaintSelection,
            bool reloadOnComplaintChange,
            bool isStep)
            {
            return new IidDashboardItemModel
                {
                SequenceNo = sequenceNo,
                ItemKey = itemKey,
                Title = title,
                SourcePath = sourcePath,
                RequiresComplaintSelection = requiresComplaintSelection,
                ReloadOnComplaintChange = reloadOnComplaintChange,
                IsStep = isStep
                };
            }

        private int ResolveDashboardPageId(string pagePath)
            {
            if (_pageIdResolver == null || string.IsNullOrWhiteSpace(pagePath))
                {
                return 0;
                }

            if (_pageIdResolver.TryResolvePageId(pagePath, out var pageId) && pageId > 0)
                {
                return pageId;
                }

            return _pageIdResolver.ResolvePageId(pagePath);
            }

        private static int ResolveComplaintDropdownPageId(int dashboardPageId, IEnumerable<IidDashboardItemModel> items)
            {
            if (dashboardPageId > 0)
                {
                return dashboardPageId;
                }

            if (items == null)
                {
                return 0;
                }

            var preferredItem = items.FirstOrDefault(item =>
                    item != null &&
                    item.IsVisible &&
                    item.RequiredPermissionPageId > 0 &&
                    string.Equals(item.ItemKey, "COMPLAINT", StringComparison.OrdinalIgnoreCase))
                ?? items.FirstOrDefault(item =>
                    item != null &&
                    item.IsVisible &&
                    item.RequiredPermissionPageId > 0 &&
                    item.RequiresComplaintSelection)
                ?? items.FirstOrDefault(item =>
                    item != null &&
                    item.IsVisible &&
                    item.RequiredPermissionPageId > 0);

            return preferredItem?.RequiredPermissionPageId ?? 0;
            }

        private int ResolvePlanIdByComplaintId(int complaintId)
            {
            if (complaintId <= 0)
                {
                return 0;
                }

            var planDetails = dBConnection.GetIidPlanDetails(complaintId);
            if (planDetails != null &&
                planDetails.TryGetValue("planId", out var rawPlanId) &&
                int.TryParse(rawPlanId?.ToString(), out var planId) &&
                planId > 0)
                {
                return planId;
                }

            return 0;
            }

        private int ResolveReportIdByComplaintId(int complaintId)
            {
            if (complaintId <= 0)
                {
                return 0;
                }

            return dBConnection.GetLatestInquiryReportByComplaintId(complaintId)?.ReportId ?? 0;
            }

        private IActionResult CreateDashboardAccessDeniedResult()
            {
            Response.StatusCode = 403;
            return PartialView("~/Views/Shared/_DashboardStepAccessDenied.cshtml");
            }

        private string SaveUploadFile(IFormFile file)
            {
            if (file == null || file.Length == 0)
                {
                return string.Empty;
                }

            var uploadsPath = Path.Combine(hostingEnvironment.WebRootPath, "Uploads");
            Directory.CreateDirectory(uploadsPath);
            var safeFileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid():N}_{safeFileName}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                {
                file.CopyTo(stream);
                }

            return uniqueFileName;
            }


        [HttpPost]
        public IActionResult GenerateInquiryReportPdf([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            return BuildInquiryReportPdf(request?.ComplaintId ?? 0, false);
            }

        [HttpPost]
        public IActionResult RegenerateInquiryReportPdf([FromBody] AIS.Models.IID.InquiryReport.IidInqComplaintRequest request)
            {
            return BuildInquiryReportPdf(request?.ComplaintId ?? 0, true);
            }

        [HttpGet]
        public IActionResult ViewInquiryReportPdf(long complaintId)
            {
            if (complaintId <= 0)
                {
                return BadRequest("ComplaintId is required.");
                }

            var latest = dBConnection.GetLatestInquiryReportByComplaintId((int)complaintId);
            var fileName = latest?.UploadedReport;
            if (string.IsNullOrWhiteSpace(fileName))
                {
                return NotFound("No inquiry report PDF generated yet.");
                }

            var fullPath = Path.Combine(hostingEnvironment.WebRootPath, "Uploads", fileName);
            if (!System.IO.File.Exists(fullPath))
                {
                return NotFound("Stored inquiry report file was not found on disk.");
                }

            return PhysicalFile(fullPath, "application/pdf", enableRangeProcessing: true);
            }

        private IActionResult BuildInquiryReportPdf(long complaintId, bool regenerate)
            {
            try
                {
                if (complaintId <= 0)
                    {
                    return Json(new { ok = false, message = "ComplaintId is required." });
                    }

                var complaint = dBConnection.GetComplaint((int)complaintId);
                if (complaint == null)
                    {
                    return Json(new { ok = false, message = "Complaint not found." });
                    }

                var accusations = dBConnection.GetIidInqAccusationsByComplaintId(complaintId) ?? new List<AIS.Models.IID.InquiryReport.IidInqAccusationRow>();
                var accused = dBConnection.GetIidInqAccusedListByComplaintId(complaintId) ?? new List<AIS.Models.IID.InquiryReport.IidInqAccusedRow>();
                var statements = dBConnection.GetIidInqStatementsByComplaintId(complaintId) ?? new List<AIS.Models.IID.InquiryReport.IidInqStatementRow>();
                var evidence = dBConnection.GetIidInqEvidenceFilesByComplaintId(complaintId) ?? new List<AIS.Models.IID.InquiryReport.IidInqEvidenceFileRow>();
                var violations = dBConnection.GetIidInqViolationsByComplaintId(complaintId) ?? new List<AIS.Models.IID.InquiryReport.IidInqViolationRow>();
                var findingsRows = dBConnection.GetIidInqFindingsRecommByComplaintId(complaintId) ?? new List<AIS.Models.IID.InquiryReport.IidInqFindingsRecommRow>();
                var dsa = dBConnection.GetIidInqDsaByComplaintId(complaintId) ?? new List<AIS.Models.IID.InquiryReport.IidInqDsaRow>();
                var additionalCharges = accusations.Where(x => (x.AccusationText ?? string.Empty).Trim().Equals("Additional Charges", StringComparison.OrdinalIgnoreCase)).ToList();
                var finalOutcome = findingsRows.Select(x => x.Outcome).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;

                var uploadsPath = Path.Combine(hostingEnvironment.WebRootPath, "Uploads");
                Directory.CreateDirectory(uploadsPath);
                var complaintRef = string.IsNullOrWhiteSpace(complaint.ComplaintNo) ? complaintId.ToString() : complaint.ComplaintNo;
                var fileName = $"IID_InquiryReport_{complaintRef}_{complaintId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var fullPath = Path.Combine(uploadsPath, fileName);

                using (var writer = new PdfWriter(fullPath))
                using (var pdf = new PdfDocument(writer))
                using (var doc = new Document(pdf))
                    {
                    doc.Add(new Paragraph("Inquiry Report").SetBold().SetFontSize(16));
                    doc.Add(new Paragraph($"Complaint No: {complaint.ComplaintNo}"));
                    doc.Add(new Paragraph($"Complaint ID: {complaintId}"));
                    doc.Add(new Paragraph($"Complainant: {complaint.ComplainantName}"));
                    doc.Add(new Paragraph($"Accused / Respondents: {string.Join(", ", accused.Select(x => x.PersonName).Where(x => !string.IsNullOrWhiteSpace(x)))}"));
                    doc.Add(new Paragraph(" "));

                    doc.Add(new Paragraph("Complaint Snapshot").SetBold());
                    doc.Add(new Paragraph($"Nature: {complaint.Nature}\nSource: {complaint.ReceivedFrom}\nBranch: {complaint.AssignedUnit}\nStatus: {complaint.Status}"));

                    doc.Add(new Paragraph("Statement Register").SetBold());
                    doc.Add(new Paragraph(string.Join("\n", statements.Select((x, i) => $"{i + 1}. {x.PersonName} | {x.RoleType} | {x.StatementDatetime} | {x.Place} | {x.UploadedStatement}"))));

                    doc.Add(new Paragraph("Evidence").SetBold());
                    doc.Add(new Paragraph(string.Join("\n", evidence.Select((x, i) => $"{i + 1}. {x.FileName} ({x.EvidenceType})"))));

                    doc.Add(new Paragraph("Violations").SetBold());
                    doc.Add(new Paragraph(string.Join("\n", violations.Select((x, i) => $"{i + 1}. {x.Category}: {x.ViolationDetail} | {x.ReferenceText}"))));

                    doc.Add(new Paragraph("Findings & Recommendations").SetBold());
                    doc.Add(new Paragraph(string.Join("\n", findingsRows.Select((x, i) => $"{i + 1}. AccusationId {x.AccusationId}: Outcome={x.Outcome}\nFindings: {x.FindingText}\nRecommendations: {x.RecommendationText}"))));

                    doc.Add(new Paragraph("Additional Charges").SetBold());
                    doc.Add(new Paragraph(additionalCharges.Count == 0 ? "N/A" : string.Join("\n", additionalCharges.Select((x, i) => $"{i + 1}. {x.AccusationText}"))));

                    doc.Add(new Paragraph("Final Outcome").SetBold());
                    doc.Add(new Paragraph(string.IsNullOrWhiteSpace(finalOutcome) ? "N/A" : finalOutcome));

                    doc.Add(new Paragraph("DSA / Final Recommendations").SetBold());
                    doc.Add(new Paragraph(string.Join("\n", dsa.Select((x, i) => $"{i + 1}. {x.PersonName} ({x.DsaStatus})"))));
                    }

                dBConnection.AddInquiryReport(new InquiryReportModel
                    {
                    ComplaintId = (int)complaintId,
                    NameComplainant = complaint.ComplainantName,
                    NameAccused = string.Join(", ", accused.Select(x => x.PersonName).Where(x => !string.IsNullOrWhiteSpace(x))),
                    Findings = findingsRows.FirstOrDefault()?.FindingText ?? string.Empty,
                    Recommendation = findingsRows.FirstOrDefault()?.RecommendationText ?? string.Empty,
                    UploadedReport = fileName,
                    UploadedEvidence = string.Join(";", evidence.Select(x => x.FileName).Where(x => !string.IsNullOrWhiteSpace(x))),
                    UploadedDsa = string.Join(";", dsa.Select(x => x.PersonName).Where(x => !string.IsNullOrWhiteSpace(x)))
                    });

                return Json(new
                    {
                    ok = true,
                    message = regenerate ? "Inquiry report PDF regenerated successfully." : "Inquiry report PDF generated successfully.",
                    fileName,
                    viewUrl = Url.Action("ViewInquiryReportPdf", "IID", new { complaintId })
                    });
                }
            catch (Exception ex)
                {
                return Json(new { ok = false, message = ex.Message });
                }
            }

        }
    }
