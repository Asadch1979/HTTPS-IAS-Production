using AIS.Models.IID;
using AIS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace AIS.Controllers
    {
    public class IIDController : Controller
        {
        private readonly ILogger<IIDController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly DBConnection dBConnection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public IIDController(ILogger<IIDController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IWebHostEnvironment hostingEnvironment)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _hostingEnvironment = hostingEnvironment;
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
            PopulateIidLists();
            return View("../IID/SubmitComplaint");
            }

        [HttpGet]
        public IActionResult FFR_PART1(int complaintId = 0)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["RegionList"] = dBConnection.GetRBHList(0);
            PopulateIidLists();
            if (complaintId > 0)
                {
                var dt = dBConnection.GetFFRPart1(complaintId);
                var model = MapFfrPart1(dt);
                if (model != null)
                    {
                    ViewData["FFRPart1"] = model;
                    }
                }
            return View("../IID/FFR_PART1");
            }

        [HttpPost]
        public IActionResult FFR_PART1(FFRPart1Model model)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");

            var attachments = model.Attachments ?? new List<IFormFile>();
            model.UploadedAttachments = SaveUploadFiles(attachments);
            if (model.ComplaintId == 0)
                {
                var (complaintId, _) = dBConnection.CreateComplaintHeader("FFR");
                model.ComplaintId = complaintId;
                }

            if (model.ComplaintId > 0)
                {
                dBConnection.SaveFFRPart1(model, model.ComplaintId);
                return RedirectToAction("FFR_PART2", new { complaintId = model.ComplaintId });
                }

            ViewData["RegionList"] = dBConnection.GetRBHList(0);
            PopulateIidLists();
            return View("../IID/FFR_PART1");
            }

        [HttpGet]
        public IActionResult FFR_PART2(int complaintId = 0)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["RegionList"] = dBConnection.GetRBHList(0);
            PopulateIidLists();
            if (complaintId > 0)
                {
                var dt = dBConnection.GetFFRPart2(complaintId);
                var model = MapFfrPart2(dt);
                if (model != null)
                    {
                    ViewData["FFRPart2"] = model;
                    }
                }
            return View("../IID/FFR_PART2");
            }

        [HttpPost]
        public IActionResult FFR_PART2(FFRPart2Model model)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");

            if (model.ComplaintId > 0)
                {
                dBConnection.SaveFFRPart2(model, model.ComplaintId);
                return RedirectToAction("FFR_PART3", new { complaintId = model.ComplaintId });
                }

            return RedirectToAction("FFR_PART1");
            }

        [HttpGet]
        public IActionResult FFR_PART3(int complaintId = 0)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["RegionList"] = dBConnection.GetRBHList(0);
            PopulateIidLists();
            if (complaintId > 0)
                {
                var dt = dBConnection.GetFFRPart3(complaintId);
                var model = MapFfrPart3(dt);
                if (model != null)
                    {
                    ViewData["FFRPart3"] = model;
                    }
                }
            return View("../IID/FFR_PART3");
            }

        [HttpPost]
        public IActionResult FFR_PART3(FFRPart3Model model)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");

            if (model.ComplaintId > 0)
                {
                dBConnection.SaveFFRPart3(model, model.ComplaintId);
                return RedirectToAction("TaskList");
                }

            return RedirectToAction("FFR_PART1");
            }

        private void PopulateIidLists()
            {
            ViewData["NatureList"] = new List<string>
                {
                "Fraud & Forgery",
                "Embezzlement of loan/partial loan amount",
                "Embezzlement of recovery amount",
                "Embezzlement of deposits",
                "Loan processing issues",
                "Fake/disputed collaterals",
                "Getting illegal gratification",
                "NOC/ Redemption issues",
                "Recovery/ Repayment issues",
                "Service Charges issues",
                "Misbehavior",
                "Miscellaneous",
                "Short payment of cash",
                "Timings related issues"
                };
            ViewData["SourceList"] = new List<string>
                {
                "Internal",
                "External",
                "Anonymous",
                "Regulatory",
                "Other"
                };
            }

        private string SaveUploadFiles(IEnumerable<IFormFile> files)
            {
            if (files == null)
                {
                return string.Empty;
                }

            var uploadsPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
            Directory.CreateDirectory(uploadsPath);
            var savedFiles = new List<string>();
            foreach (var file in files)
                {
                if (file == null || file.Length == 0)
                    {
                    continue;
                    }
                var safeFileName = Path.GetFileName(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid():N}_{safeFileName}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                    file.CopyTo(stream);
                    }
                savedFiles.Add(uniqueFileName);
                }

            return string.Join(";", savedFiles);
            }

        private static FFRPart1Model MapFfrPart1(DataTable dt)
            {
            if (dt == null || dt.Rows.Count == 0)
                {
                return null;
                }

            var row = dt.Rows[0];
            return new FFRPart1Model
                {
                ComplaintId = GetInt(row, "COMPLAINT_ID"),
                PertainsTo = GetString(row, "PERTAINS_TO"),
                FieldType = GetString(row, "FIELD_TYPE"),
                HOUnitTypeId = GetNullableInt(row, "HO_UNIT_TYPE_ID"),
                HOUnitId = GetNullableInt(row, "HO_UNIT_ID"),
                RegionId = GetNullableInt(row, "REGION_ID"),
                BranchId = GetNullableInt(row, "BRANCH_ID"),
                Source = GetString(row, "SOURCE"),
                SourceOtherText = GetString(row, "SOURCE_OTHER_TEXT"),
                Nature = GetString(row, "NATURE"),
                ReferenceNo = GetString(row, "REFERENCE_NO"),
                FFRDate = GetString(row, "FFR_DATE"),
                IncidentDate = GetString(row, "INCIDENT_DATE"),
                IncidentVenue = GetString(row, "INCIDENT_VENUE"),
                IncidentNarrative = GetString(row, "INCIDENT_NARRATIVE"),
                ComplainantName = GetString(row, "COMPLAINANT_NAME"),
                ComplainantCNIC = GetString(row, "COMPLAINANT_CNIC"),
                AccountNo = GetString(row, "ACCOUNT_NO"),
                ComplainantMobile = GetString(row, "COMPLAINANT_MOBILE"),
                ComplainantAddress = GetString(row, "COMPLAINANT_ADDRESS"),
                MainAccused = GetString(row, "MAIN_ACCUSED"),
                CoAccused = GetString(row, "CO_ACCUSED"),
                Accusations = GetString(row, "ACCUSATIONS"),
                ApproachAdopted = GetString(row, "APPROACH_ADOPTED"),
                RecordScrutinized = GetString(row, "RECORD_SCRUTINIZED"),
                RootCause = GetString(row, "ROOT_CAUSE"),
                KeyFindings = GetString(row, "KEY_FINDINGS"),
                ClearRecommendations = GetString(row, "CLEAR_RECOMMENDATIONS"),
                UploadedAttachments = GetString(row, "ATTACHMENTS_PATH")
                };
            }

        private static FFRPart2Model MapFfrPart2(DataTable dt)
            {
            if (dt == null || dt.Rows.Count == 0)
                {
                return null;
                }

            var row = dt.Rows[0];
            return new FFRPart2Model
                {
                ComplaintId = GetInt(row, "COMPLAINT_ID"),
                ComplainantStatementTime = GetString(row, "COMPLAINANT_STATEMENT_TIME"),
                ComplainantStatementPlace = GetString(row, "COMPLAINANT_STATEMENT_PLACE"),
                ComplainantStatementMode = GetString(row, "COMPLAINANT_STATEMENT_MODE"),
                ComplainantStatementPoints = GetString(row, "COMPLAINANT_STATEMENT_POINTS"),
                AccusedStatementTime = GetString(row, "ACCUSED_STATEMENT_TIME"),
                AccusedStatementPlace = GetString(row, "ACCUSED_STATEMENT_PLACE"),
                AccusedStatementMode = GetString(row, "ACCUSED_STATEMENT_MODE"),
                AccusedStatementPoints = GetString(row, "ACCUSED_STATEMENT_POINTS"),
                PrimaryEvidence = GetString(row, "PRIMARY_EVIDENCE"),
                SecondaryEvidence = GetString(row, "SECONDARY_EVIDENCE")
                };
            }

        private static FFRPart3Model MapFfrPart3(DataTable dt)
            {
            if (dt == null || dt.Rows.Count == 0)
                {
                return null;
                }

            var row = dt.Rows[0];
            return new FFRPart3Model
                {
                ComplaintId = GetInt(row, "COMPLAINT_ID"),
                AuditHighlighted = GetString(row, "AUDIT_HIGHLIGHTED"),
                AuditHighlightDetails = GetString(row, "AUDIT_HIGHLIGHT_DETAILS"),
                ImplicationReputational = GetBool(row, "IMPL_REPUTATIONAL"),
                ImplicationOperational = GetBool(row, "IMPL_OPERATIONAL"),
                ImplicationFinancial = GetBool(row, "IMPL_FINANCIAL"),
                ImplicationPrecedence = GetBool(row, "IMPL_PRECEDENCE"),
                ImplicationOther = GetBool(row, "IMPL_OTHER"),
                ImplicationOtherDetails = GetString(row, "IMPL_OTHER_DETAILS"),
                PolicyViolated = GetString(row, "POLICY_VIOLATED"),
                SopGaps = GetString(row, "SOP_GAPS"),
                ActionRecommended = GetString(row, "ACTION_RECOMMENDED")
                };
            }

        private static string GetString(DataRow row, string columnName)
            {
            if (row == null || row.Table?.Columns.Contains(columnName) != true)
                {
                return string.Empty;
                }
            return row[columnName] == DBNull.Value ? string.Empty : row[columnName].ToString();
            }

        private static int GetInt(DataRow row, string columnName)
            {
            if (row == null || row.Table?.Columns.Contains(columnName) != true || row[columnName] == DBNull.Value)
                {
                return 0;
                }
            return Convert.ToInt32(row[columnName]);
            }

        private static int? GetNullableInt(DataRow row, string columnName)
            {
            if (row == null || row.Table?.Columns.Contains(columnName) != true || row[columnName] == DBNull.Value)
                {
                return null;
                }
            return Convert.ToInt32(row[columnName]);
            }

        private static bool GetBool(DataRow row, string columnName)
            {
            if (row == null || row.Table?.Columns.Contains(columnName) != true || row[columnName] == DBNull.Value)
                {
                return false;
                }
            var value = row[columnName].ToString();
            return string.Equals(value, "Y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "YES", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "TRUE", StringComparison.OrdinalIgnoreCase)
                || value == "1";
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
        public IActionResult Analysis(int reportId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
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
        public IActionResult Reports()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            return View("../IID/Reports");
            }

        [HttpGet, HttpPost]
        public IActionResult TaskList()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            return View("../IID/TaskList");
            }
        }
    }
