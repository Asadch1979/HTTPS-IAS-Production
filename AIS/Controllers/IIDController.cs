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
        private readonly IWebHostEnvironment hostingEnvironment;
        public IIDController(ILogger<IIDController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IWebHostEnvironment _hostingEnvironment)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            hostingEnvironment = _hostingEnvironment;
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
        public IActionResult FFR_PART1(int complaintId = 0)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["RegionList"] = dBConnection.GetRBHList(0);
            var model = new FFRPart1Model { ComplaintId = complaintId };
            if (complaintId > 0)
                {
                var dt = dBConnection.GetFFRPart1(complaintId);
                model = MapFfrPart1(dt) ?? model;
                }
            return View("../IID/FFR_PART1", model);
            }

        [HttpPost]
        public IActionResult FFR_PART1(FFRPart1Model model)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["RegionList"] = dBConnection.GetRBHList(0);

            if (model.ComplaintId == 0)
                {
                var (complaintId, _) = dBConnection.CreateComplaintHeader("FFR");
                model.ComplaintId = complaintId;
                }

            if (model.Attachments?.Any() == true)
                {
                var attachmentNames = new List<string>();
                foreach (var file in model.Attachments)
                    {
                    var savedFile = SaveUploadFile(file);
                    if (!string.IsNullOrEmpty(savedFile))
                        {
                        attachmentNames.Add(savedFile);
                        }
                    }
                model.AttachmentsPath = string.Join(";", attachmentNames);
                }

            if (model.ComplaintId > 0)
                {
                dBConnection.SaveFFRPart1(model, model.ComplaintId);
                return RedirectToAction("FFR_PART2", new { complaintId = model.ComplaintId });
                }

            return View("../IID/FFR_PART1", model);
            }

        [HttpGet]
        public IActionResult FFR_PART2(int complaintId = 0)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["RegionList"] = dBConnection.GetRBHList(0);
            var model = new FFRPart2Model { ComplaintId = complaintId };
            if (complaintId > 0)
                {
                var dt = dBConnection.GetFFRPart2(complaintId);
                model = MapFfrPart2(dt) ?? model;
                }
            return View("../IID/FFR_PART2", model);
            }

        [HttpPost]
        public IActionResult FFR_PART2(FFRPart2Model model)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["RegionList"] = dBConnection.GetRBHList(0);

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
            var model = new FFRPart3Model { ComplaintId = complaintId };
            if (complaintId > 0)
                {
                var dt = dBConnection.GetFFRPart3(complaintId);
                model = MapFfrPart3(dt) ?? model;
                }
            return View("../IID/FFR_PART3", model);
            }

        [HttpPost]
        public IActionResult FFR_PART3(FFRPart3Model model)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            ViewData["RegionList"] = dBConnection.GetRBHList(0);

            if (model.ComplaintId > 0)
                {
                dBConnection.SaveFFRPart3(model, model.ComplaintId);
                return RedirectToAction("TaskList");
                }

            return RedirectToAction("FFR_PART1");
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

        private static FFRPart1Model MapFfrPart1(DataTable dt)
            {
            if (dt == null || dt.Rows.Count == 0)
                {
                return null;
                }

            var row = dt.Rows[0];
            var model = new FFRPart1Model
                {
                ComplaintId = GetInt(row, "COMPLAINT_ID"),
                Source = GetString(row, "ORIGINATING_DEPT_SOURCE"),
                Nature = GetString(row, "NATURE"),
                ReferenceNo = GetString(row, "REFERENCE_NO"),
                FFRDate = GetDateValue(row, "FFR_DATE"),
                IncidentDate = GetDateValue(row, "INCIDENT_DATE"),
                IncidentVenue = GetString(row, "INCIDENT_VENUE"),
                IncidentNarrative = GetString(row, "INCIDENT_HOW"),
                ComplainantName = GetString(row, "COMPLAINANT_NAME"),
                ComplainantCNIC = GetString(row, "COMPLAINANT_CNIC"),
                AccountNo = GetString(row, "COMPLAINANT_ACCOUNT_NO"),
                ComplainantMobile = GetString(row, "COMPLAINANT_MOBILE"),
                ComplainantAddress = GetString(row, "COMPLAINANT_ADDRESS"),
                MainAccused = GetString(row, "MAIN_ACCUSED_DETAILS"),
                CoAccused = GetString(row, "CO_ACCUSED_DETAILS"),
                Accusations = GetString(row, "ACCUSATION_DETAILS"),
                ApproachAdopted = GetString(row, "APPROACH_ADOPTED"),
                RecordScrutinized = GetString(row, "RECORD_SCRUTINIZED"),
                RootCause = GetString(row, "ROOT_CAUSE"),
                KeyFindings = GetString(row, "KEY_FINDINGS"),
                ClearRecommendations = GetString(row, "RECOMMENDATIONS"),
                AttachmentsPath = GetString(row, "ATTACHMENTS_PATH")
                };

            var locationTypeId = GetInt(row, "LOCATION_TYPE_ID");
            var gmOfficeId = GetNullableInt(row, "GM_OFFICE_ID");
            var regionId = GetNullableInt(row, "REGION_ID");
            var branchId = GetNullableInt(row, "BRANCH_ID");
            ApplyLocationDetails(model, locationTypeId, gmOfficeId, regionId, branchId);

            return model;
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
                ComplainantStatementTime = GetString(row, "COMP_STMT_TIME"),
                ComplainantStatementPlace = GetString(row, "COMP_STMT_PLACE"),
                ComplainantStatementMode = GetString(row, "COMP_STMT_MODE"),
                ComplainantStatementPoints = GetString(row, "COMPLAINANT_KEY_POINTS"),
                AccusedStatementTime = GetString(row, "ACC_STMT_TIME"),
                AccusedStatementPlace = GetString(row, "ACC_STMT_PLACE"),
                AccusedStatementMode = GetString(row, "ACC_STMT_MODE"),
                AccusedStatementPoints = GetString(row, "ACCUSED_KEY_POINTS"),
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
            var auditFlag = GetString(row, "AUDIT_REPORT_FLAG");
            return new FFRPart3Model
                {
                ComplaintId = GetInt(row, "COMPLAINT_ID"),
                AuditHighlighted = NormalizeAuditFlag(auditFlag),
                AuditHighlightDetails = GetString(row, "AUDIT_REPORT_DETAILS"),
                ImplicationReputational = GetFlag(row, "IMPL_REPUTATIONAL_LOSS"),
                ImplicationOperational = GetFlag(row, "IMPL_OPERATIONAL_RISK"),
                ImplicationFinancial = GetFlag(row, "IMPL_FINANCIAL_RISK"),
                ImplicationPrecedence = GetFlag(row, "IMPL_PRECEDENCE"),
                ImplicationOther = GetFlag(row, "IMPL_OTHER_FLAG"),
                ImplicationOtherDetails = GetString(row, "IMPL_OTHER_TEXT"),
                PolicyViolated = GetString(row, "SOP_VIOLATIONS"),
                SopGaps = GetString(row, "CONTROL_GAPS"),
                ActionRecommended = GetString(row, "ACTION_RECOMMENDED")
                };
            }

        private static string GetString(DataRow row, string columnName)
            {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                {
                return null;
                }
            return row[columnName].ToString();
            }

        private static int GetInt(DataRow row, string columnName)
            {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                {
                return 0;
                }
            return Convert.ToInt32(row[columnName]);
            }

        private static int? GetNullableInt(DataRow row, string columnName)
            {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                {
                return null;
                }
            return Convert.ToInt32(row[columnName]);
            }

        private static string GetDateValue(DataRow row, string columnName)
            {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                {
                return null;
                }
            var value = row[columnName];
            if (value is DateTime dateTime)
                {
                return dateTime.ToString("yyyy-MM-dd");
                }
            if (DateTime.TryParse(value.ToString(), out var parsed))
                {
                return parsed.ToString("yyyy-MM-dd");
                }
            return value.ToString();
            }

        private static bool GetFlag(DataRow row, string columnName)
            {
            var value = GetString(row, columnName);
            return string.Equals(value, "Y", StringComparison.OrdinalIgnoreCase);
            }

        private static void ApplyLocationDetails(FFRPart1Model model, int locationTypeId, int? gmOfficeId, int? regionId, int? branchId)
            {
            if (locationTypeId == 1)
                {
                model.PertainsTo = "HO";
                return;
                }

            if (locationTypeId == 2)
                {
                model.PertainsTo = "FIELD";
                model.FieldType = "HO_UNIT";
                model.HOUnitId = gmOfficeId;
                return;
                }

            if (locationTypeId == 3)
                {
                model.PertainsTo = "FIELD";
                model.FieldType = "BRANCH";
                model.RegionId = regionId;
                model.BranchId = branchId;
                }
            }

        private static string NormalizeAuditFlag(string auditFlag)
            {
            if (string.IsNullOrWhiteSpace(auditFlag))
                {
                return string.Empty;
                }
            var normalized = auditFlag.Trim().ToUpperInvariant();
            return normalized switch
                {
                "YES" => "Yes",
                "NO" => "No",
                "NA" => "NA",
                _ => auditFlag
                };
            }
        }
    }
