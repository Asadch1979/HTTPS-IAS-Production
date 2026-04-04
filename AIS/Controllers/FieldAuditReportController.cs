using AIS.Models.FieldAuditReport;
using AIS.Models.Notifications;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    public class FieldAuditReportController : Controller
        {
        private readonly ILogger<FieldAuditReportController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly TopMenus _topMenus;
        private readonly IPermissionService _permissionService;
        private readonly IConfiguration _configuration;
        private readonly FieldAuditReportPdfGenerator _pdfGenerator;

        public FieldAuditReportController(
            ILogger<FieldAuditReportController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            TopMenus topMenus,
            IPermissionService permissionService,
            IConfiguration configuration,
            FieldAuditReportPdfGenerator pdfGenerator)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _topMenus = topMenus;
            _permissionService = permissionService;
            _configuration = configuration;
            _pdfGenerator = pdfGenerator;
            }

        public IActionResult ReportOverview()
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            var selector = BuildEngagementSelector();
            ViewData["EngagementSelector"] = selector;
            if (!selector.HasActiveEngagement)
                {
                return View(new FieldAuditReportOverviewViewModel());
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (IsManagementAudit(selected))
                {
                return RedirectToAction("Home", "MANReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var overview = _dbConnection.GetFieldAuditReportOverview(engId);
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var checklist = _dbConnection.GetFieldAuditReportChecklist(engId);
            overview ??= new FieldAuditReportOverviewModel();

            var model = new FieldAuditReportOverviewViewModel
                {
                Overview = overview,
                Checklist = checklist,
                IsFinal = isFinal,
                CanFinalize = !isFinal && checklist.IsComplete,
                ReportStatus = isFinal ? "FINAL" : "DRAFT"
                };

            return View(model);
            }

        public IActionResult NarrativeSections()
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            var selector = BuildEngagementSelector();
            ViewData["EngagementSelector"] = selector;
            if (!selector.HasActiveEngagement)
                {
                return View(new NarrativeSectionsViewModel { IsReadOnly = true });
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (IsManagementAudit(selected))
                {
                return RedirectToAction("Home", "MANReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var baseModel = BuildInputSectionViewModel(engId, isFinal, NarrativeFieldCodes, FieldAuditReportSectionCodes.NarrativeInputs);
            var observationCount = _dbConnection.GetFieldAuditObservationCount(engId);
            var observationDetails = _dbConnection.GetFieldAuditObservationDetails(engId);
            var overview = _dbConnection.GetFieldAuditReportOverview(engId);
            overview ??= new FieldAuditReportOverviewModel();

            var model = new NarrativeSectionsViewModel
                {
                EngagementId = baseModel.EngagementId,
                EntityId = baseModel.EntityId,
                IsReadOnly = baseModel.IsReadOnly,
                SectionCode = baseModel.SectionCode,
                Fields = baseModel.Fields,
                ReportStatus = baseModel.ReportStatus,
                AuditTeam = baseModel.AuditTeam,
                StatisticsRows = baseModel.StatisticsRows,
                Overview = overview,
                ObservationCount = observationCount,
                Observations = observationDetails
                };

            return View(model);
            }

        public IActionResult KpiSnapshot()
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            var selector = BuildEngagementSelector();
            ViewData["EngagementSelector"] = selector;
            if (!selector.HasActiveEngagement)
                {
                return View(new FieldAuditInputSectionViewModel { IsReadOnly = true });
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (IsManagementAudit(selected))
                {
                return RedirectToAction("Home", "MANReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var model = BuildInputSectionViewModel(engId, isFinal, KpiFieldCodes, FieldAuditReportSectionCodes.KpiSnapshot);
            return View(model);
            }

        public IActionResult NplSnapshot()
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            var selector = BuildEngagementSelector();
            ViewData["EngagementSelector"] = selector;
            if (!selector.HasActiveEngagement)
                {
                return View(new FieldAuditInputSectionViewModel { IsReadOnly = true });
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (IsManagementAudit(selected))
                {
                return RedirectToAction("Home", "MANReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var model = BuildInputSectionViewModel(engId, isFinal, NplFieldCodes, FieldAuditReportSectionCodes.NplSnapshot);
            return View(model);
            }

        public IActionResult StaffSnapshot()
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            var selector = BuildEngagementSelector();
            ViewData["EngagementSelector"] = selector;
            if (!selector.HasActiveEngagement)
                {
                return View(new FieldAuditInputSectionViewModel { IsReadOnly = true });
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (IsManagementAudit(selected))
                {
                return RedirectToAction("Home", "MANReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var model = BuildInputSectionViewModel(engId, isFinal, StaffFieldCodes, FieldAuditReportSectionCodes.StaffSnapshot);
            return View(model);
            }

        [HttpGet]
        public IActionResult GetStaffDesignationOptions(int engId)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            var items = _dbConnection.GetStaffDesignationOptions(engId);
            return Json(new { success = true, items });
            }

        [HttpGet]
        public IActionResult GetKpiOptions(int engId)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            var items = _dbConnection.GetKpiOptions(engId);
            return Json(new { success = true, items });
            }

        [HttpGet]
        public IActionResult GetNplCategoryOptions(int engId)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            var items = _dbConnection.GetNplCategoryOptions(engId);
            return Json(new { success = true, items });
            }

        [HttpGet]
        public IActionResult GetStaffSnapshotRows(int engId)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            var rows = _dbConnection.GetFieldAuditStaffSnapshots(engId);
            return Json(new { success = true, rows });
            }

        [HttpPost]
        public IActionResult SaveStaffSnapshotRows(int engId, [FromBody] List<StaffSnapshotRowModel> rows)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            if (_dbConnection.IsFieldAuditReportFinal(engId))
                {
                return BadRequest(new { success = false, message = "Report is finalized and cannot be edited." });
                }

            foreach (var row in rows ?? new List<StaffSnapshotRowModel>())
                {
                if (row == null)
                    {
                    continue;
                    }

                if (!row.PpNo.HasValue
                    && string.IsNullOrWhiteSpace(row.Name)
                    && string.IsNullOrWhiteSpace(row.Rank)
                    && string.IsNullOrWhiteSpace(row.Designation))
                    {
                    continue;
                    }

                _dbConnection.SaveFieldAuditStaffSnapshot(engId, row);
                }

            var savedRows = _dbConnection.GetFieldAuditStaffSnapshots(engId);
            return Json(new { success = true, message = "Staff Snapshot saved.", rows = savedRows });
            }

        [HttpGet]
        public IActionResult GetNplSnapshotRows(int engId)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            var rows = _dbConnection.GetFieldAuditNplSnapshots(engId);
            return Json(new { success = true, rows });
            }

        [HttpPost]
        public IActionResult SaveNplSnapshotRows(int engId, [FromBody] List<NplSnapshotRowModel> rows)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            if (_dbConnection.IsFieldAuditReportFinal(engId))
                {
                return BadRequest(new { success = false, message = "Report is finalized and cannot be edited." });
                }

            foreach (var row in rows ?? new List<NplSnapshotRowModel>())
                {
                if (row == null)
                    {
                    continue;
                    }

                if (string.IsNullOrWhiteSpace(row.Category)
                    && !row.PeriodEnd.HasValue
                    && !row.CaseCount.HasValue
                    && !row.OutstandingAmount.HasValue
                    && !row.ProvisionAmount.HasValue)
                    {
                    continue;
                    }

                _dbConnection.SaveFieldAuditNplSnapshot(engId, row);
                }

            var savedRows = _dbConnection.GetFieldAuditNplSnapshots(engId);
            return Json(new { success = true, message = "NPL Snapshot saved.", rows = savedRows });
            }

        [HttpGet]
        public IActionResult GetKpiSnapshotRows(int engId)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            var rows = _dbConnection.GetFieldAuditKpiSnapshots(engId);
            return Json(new { success = true, rows });
            }

        [HttpPost]
        public IActionResult SaveKpiSnapshotRows(int engId, [FromBody] List<KpiSnapshotRowModel> rows)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            if (_dbConnection.IsFieldAuditReportFinal(engId))
                {
                return BadRequest(new { success = false, message = "Report is finalized and cannot be edited." });
                }

            foreach (var row in rows ?? new List<KpiSnapshotRowModel>())
                {
                if (row == null)
                    {
                    continue;
                    }

                if (string.IsNullOrWhiteSpace(row.KpiCode)
                    && string.IsNullOrWhiteSpace(row.KpiLabel)
                    && !row.PeriodEnd.HasValue
                    && !row.ActualValue.HasValue
                    && !row.TargetValue.HasValue
                    && string.IsNullOrWhiteSpace(row.Unit))
                    {
                    continue;
                    }

                _dbConnection.SaveFieldAuditKpiSnapshot(engId, row);
                }

            var savedRows = _dbConnection.GetFieldAuditKpiSnapshots(engId);
            return Json(new { success = true, message = "KPI Snapshot saved.", rows = savedRows });
            }

        [HttpGet]
        public IActionResult GetPdfStatistics(int engId, int? reportVersion)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            var rows = _dbConnection.GetPdfStatistics(engId, reportVersion);
            var locked = rows.Any(row => row != null &&
                (row.Reported.HasValue || row.Rectified.HasValue || row.Outstanding.HasValue || !string.IsNullOrWhiteSpace(row.Remarks)));
            return Json(new { success = true, rows, locked });
            }

        [HttpPost]
        public IActionResult SavePdfStatistics([FromBody] FieldAuditPdfStatisticsSaveRequest request)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (request == null || request.EngId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid request." });
                }

            if (_dbConnection.IsFieldAuditReportFinal(request.EngId))
                {
                return BadRequest(new { success = false, message = "Report is finalized and cannot be edited." });
                }

            foreach (var row in request.Rows ?? new List<FieldAuditPdfStatisticsRowInput>())
                {
                if (row == null)
                    {
                    continue;
                    }

                if (row.Reported.HasValue && row.Reported.Value < 0
                    || row.Rectified.HasValue && row.Rectified.Value < 0
                    || row.Outstanding.HasValue && row.Outstanding.Value < 0)
                    {
                    return BadRequest(new { success = false, message = "Statistics values cannot be negative." });
                    }
                }

            var user = _sessionHandler.GetUser();
            var userPpNo = user?.PPNumber ?? string.Empty;
            _dbConnection.SavePdfStatistics(request.EngId, request.ReportVersion, request.Rows ?? new List<FieldAuditPdfStatisticsRowInput>(), userPpNo);
            return Json(new { success = true, message = "Saved", locked = true });
            }

        [HttpGet]
        public IActionResult GetIncomeLeakage(int engId, int? reportVersion)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            var rows = _dbConnection.GetIncomeLeakage(engId, reportVersion);
            var locked = rows.Any(row => row != null &&
                (!string.IsNullOrWhiteSpace(row.Description)
                || !string.IsNullOrWhiteSpace(row.Area)
                || row.Amount.HasValue));
            return Json(new { success = true, rows, locked });
            }

        [HttpPost]
        public IActionResult SaveIncomeLeakage([FromBody] FieldAuditIncomeLeakageSaveRequest request)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (request == null || request.EngId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid request." });
                }

            if (_dbConnection.IsFieldAuditReportFinal(request.EngId))
                {
                return BadRequest(new { success = false, message = "Report is finalized and cannot be edited." });
                }

            var rows = new List<FieldAuditIncomeLeakageRowInput>();
            foreach (var row in request.Rows ?? new List<FieldAuditIncomeLeakageRowInput>())
                {
                if (row == null)
                    {
                    continue;
                    }

                if (string.IsNullOrWhiteSpace(row.Description)
                    && string.IsNullOrWhiteSpace(row.Area)
                    && !row.Amount.HasValue)
                    {
                    continue;
                    }

                if (row.Amount.HasValue && row.Amount.Value < 0)
                    {
                    return BadRequest(new { success = false, message = "Income leakage amount cannot be negative." });
                    }

                rows.Add(row);
                }

            var user = _sessionHandler.GetUser();
            var userPpNo = user?.PPNumber ?? string.Empty;
            _dbConnection.SaveIncomeLeakage(request.EngId, request.ReportVersion, rows, userPpNo);
            return Json(new { success = true, message = "Saved", locked = true });
            }

        [HttpGet]
        public IActionResult GetOverallConclusion(int engId, int? reportVersion)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid engagement." });
                }

            var model = _dbConnection.GetOverallConclusion(engId, reportVersion) ?? new FieldAuditOverallConclusionInput();
            var locked = !string.IsNullOrWhiteSpace(model.OverallConclusionHtml)
                         || !string.IsNullOrWhiteSpace(model.NonAddressableHtml)
                         || !string.IsNullOrWhiteSpace(model.FraudProneHtml)
                         || !string.IsNullOrWhiteSpace(model.RegulatoryHtml)
                         || !string.IsNullOrWhiteSpace(model.SafetySecurityHtml);
            return Json(new { success = true, model, locked });
            }

        [HttpPost]
        public IActionResult SaveOverallConclusion([FromBody] FieldAuditOverallConclusionSaveRequest request)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (request == null || request.EngId <= 0)
                {
                return BadRequest(new { success = false, message = "Invalid request." });
                }

            if (_dbConnection.IsFieldAuditReportFinal(request.EngId))
                {
                return BadRequest(new { success = false, message = "Report is finalized and cannot be edited." });
                }

            var user = _sessionHandler.GetUser();
            var userPpNo = user?.PPNumber ?? string.Empty;
            _dbConnection.SaveOverallConclusion(request.EngId, request.ReportVersion, request, userPpNo);
            return Json(new { success = true, message = "Saved", locked = true });
            }

        [HttpPost]
        public IActionResult SaveFieldAuditInputs(FieldAuditInputSectionViewModel model, string submitAction, string returnAction)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (!TryResolveEngagementId(out var engId))
                {
                TempData["FieldAuditReportMessage"] = "Select an engagement to continue.";
                return RedirectToAction(nameof(ReportOverview));
                }

            if (_dbConnection.IsFieldAuditReportFinal(engId))
                {
                TempData["FieldAuditReportMessage"] = "Report is finalized and cannot be edited.";
                return RedirectToAction(nameof(ReportOverview));
                }

            var fieldValues = model?.Fields ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in fieldValues)
                {
                _dbConnection.SaveFieldAuditTextBlock(engId, entry.Key, entry.Value ?? string.Empty);
                }

            if (string.Equals(submitAction, "complete", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(model?.SectionCode))
                {
                _dbConnection.SaveFieldAuditTextBlock(engId, model.SectionCode, "Y");
                TempData["FieldAuditReportMessage"] = "Section marked as complete.";
                }
            else
                {
                TempData["FieldAuditReportMessage"] = "Section saved successfully.";
                }

            return RedirectToAction(NormalizeReturnAction(returnAction));
            }

        public IActionResult FinalizeReport()
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            var selector = BuildEngagementSelector();
            ViewData["EngagementSelector"] = selector;
            if (!selector.HasActiveEngagement)
                {
                return View(new FinalizeReportViewModel());
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (IsManagementAudit(selected))
                {
                return RedirectToAction("Home", "MANReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var checklist = _dbConnection.GetFieldAuditReportChecklist(engId);

            var model = new FinalizeReportViewModel
                {
                EngagementId = engId,
                IsFinal = isFinal,
                Checklist = checklist,
                CanFinalize = !isFinal && checklist.IsComplete
                };

            return View(model);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeReport(FinalizeReportViewModel model)
            {
            try
                {
                var redirect = EnsureAuthorized();
                if (redirect != null)
                    {
                    return Json(new { success = false, message = "Session expired. Please log in again." });
                    }

                if (!TryResolveEngagementId(out var engId))
                    {
                    return Json(new { success = false, message = "Select an engagement to continue." });
                    }

                if (_dbConnection.IsFieldAuditReportFinal(engId))
                    {
                    return Json(new { success = true, message = "Report Finalized" });
                    }

                var result = _dbConnection.FinalizeFieldAuditReport(engId);
                if (result.IsFinalized)
                    {
                    var notificationData = _dbConnection.GetFinalReportIssuedNotificationData(engId);
                    NotificationEmailAttachmentData attachment = null;
                    var pdfDocument = await _pdfGenerator.GenerateAsync(engId);
                    if (pdfDocument.IsSuccess)
                        {
                        attachment = new NotificationEmailAttachmentData
                            {
                            FileName = pdfDocument.FileName,
                            ContentBytes = pdfDocument.ContentBytes,
                            ContentType = pdfDocument.ContentType
                            };
                        }
                    else
                        {
                        _logger.LogWarning("Final report PDF attachment could not be prepared for ENG_ID {EngId}. Status={StatusCode}; Error={ErrorMessage}", engId, pdfDocument.FailureStatusCode, pdfDocument.ErrorMessage);
                        }

                    await EmailNotification.SendFinalReportIssuedAsync(_configuration, notificationData, attachment, HttpContext?.RequestServices);
                    }

                return Json(new { success = result.IsFinalized, message = result.Message ?? string.Empty });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to finalize field audit report.");
                return Json(new { success = false, message = "Unable to finalize report. Please try again or contact support." });
                }
            }

        [HttpPost]
        public IActionResult SetActiveEngagement(int engagementId, string returnUrl)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engagementId <= 0 || !GetAuthorizedEngagementIds().Contains(engagementId))
                {
                TempData["FieldAuditReportMessage"] = "Select a valid engagement to continue.";
                return RedirectToAction(nameof(ReportOverview));
                }

            _sessionHandler.SetActiveEngagementId(engagementId);
            var selected = _dbConnection
                .GetReportEntities()
                .FirstOrDefault(option => option.EngagementId == engagementId);
            if (IsManagementAudit(selected))
                {
                return RedirectToAction("Home", "MANReport");
                }

            return RedirectToLocal(returnUrl, nameof(ReportOverview));
            }

        [HttpPost]
        public IActionResult SaveNarrativePara(FieldAuditParaNarrativeSaveRequest request)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (!TryResolveEngagementId(out var engId))
                {
                return BadRequest(new { Status = false, Message = "Select an engagement to continue." });
                }

            if (request == null || request.ParaId <= 0)
                {
                return BadRequest(new { Status = false, Message = "Invalid para selection." });
                }

            if (_dbConnection.IsFieldAuditReportFinal(engId))
                {
                return BadRequest(new { Status = false, Message = "Report is finalized and cannot be edited." });
                }

            var paraDetails = _dbConnection
                .GetFieldAuditObservationDetails(engId)
                .FirstOrDefault(detail => detail.ParaId == request.ParaId);

            if (paraDetails == null)
                {
                return BadRequest(new { Status = false, Message = "Invalid para selection." });
                }

            request.Action = string.IsNullOrWhiteSpace(request.Action) ? string.Empty : request.Action.Trim().ToUpperInvariant();

            if (paraDetails.IsFinalized && !string.Equals(request.Action, "UPDATE_REQUIRED", StringComparison.OrdinalIgnoreCase))
                {
                return BadRequest(new { Status = false, Message = "Para is finalized. Click Update Required to edit." });
                }

            if (string.Equals(request.Action, "FINALIZE", StringComparison.OrdinalIgnoreCase))
                {
                var missingField = GetMissingNarrativeField(request);
                if (!string.IsNullOrWhiteSpace(missingField))
                    {
                    return BadRequest(new { Status = false, Message = $"{missingField} is required." });
                    }
                }

            var result = _dbConnection.SaveFieldAuditParaNarrative(engId, request);
            if (!result.Success)
                {
                return BadRequest(new { Status = false, Message = result.Message });
                }

            return Ok(new { Status = true, Message = result.Message });
            }

        [HttpPost]
        public IActionResult ClearEngagement(string returnUrl)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            _sessionHandler.ClearActiveEngagementId();
            TempData["FieldAuditReportMessage"] = "Engagement cleared. Select a new engagement to continue.";
            return RedirectToLocal(returnUrl, nameof(ReportOverview));
            }

        private IActionResult EnsureAuthorized()
            {
            ViewData["TopMenu"] = _topMenus.GetTopMenus();
            ViewData["TopMenuPages"] = _topMenus.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            return null;
            }

        private bool TryResolveEngagementId(out int engId)
            {
            return _sessionHandler.TryGetActiveEngagementId(out engId);
            }

        private static bool IsManagementAudit(FieldAuditEngagementOptionModel selected)
            {
            return selected != null
                && string.Equals(selected.AuditType, "D", StringComparison.OrdinalIgnoreCase);
            }

        private FieldAuditEngagementSelectorViewModel BuildEngagementSelector()
            {
            var options = new List<FieldAuditEngagementOptionModel>();
            foreach (var item in _dbConnection.GetReportEntities())
                {
                if (item.EngagementId <= 0)
                    continue;

                options.Add(new FieldAuditEngagementOptionModel
                    {
                    EngagementId = item.EngagementId,
                    EntityName = item.EntityName ?? string.Empty,
                    AuditPeriod = item.AuditPeriod ?? string.Empty,
                    AuditType = item.AuditType
                    });
                }


            options = options.OrderBy(option => option.EngagementId).ToList();

            int? activeEngagementId = null;
            if (TryResolveEngagementId(out var resolvedEngagementId)
                && options.Any(option => option.EngagementId == resolvedEngagementId))
                {
                activeEngagementId = resolvedEngagementId;
                }
            else
                {
                _sessionHandler.ClearActiveEngagementId();
                }

            var selected = activeEngagementId.HasValue
                ? options.FirstOrDefault(option => option.EngagementId == activeEngagementId.Value)
                : null;

            if (activeEngagementId.HasValue)
                {
                var overview = _dbConnection.GetFieldAuditReportOverview(activeEngagementId.Value);
                if (overview != null && selected != null)
                    {
                    selected.EntityName = string.IsNullOrWhiteSpace(overview.EntityName) ? selected.EntityName : overview.EntityName;
                    selected.AuditPeriod = string.IsNullOrWhiteSpace(overview.AuditPeriod) ? selected.AuditPeriod : overview.AuditPeriod;
                    }
                }

            var hasExistingData = activeEngagementId.HasValue
                && _dbConnection.HasFieldAuditReportData(activeEngagementId.Value);

            return new FieldAuditEngagementSelectorViewModel
                {
                ActiveEngagementId = activeEngagementId,
                ActiveEngagementLabel = selected == null
                    ? string.Empty
                    : $"{selected.EngagementId} | {selected.EntityName} | {selected.AuditPeriod}",
                HasExistingData = hasExistingData,
                Options = options
                };
            }

        private HashSet<int> GetAuthorizedEngagementIds()
            {
            return _dbConnection
                .GetReportEntities()
                .Where(item => item.EngagementId > 0)
                .Select(item => item.EngagementId)
                .ToHashSet();
            }

        private static string GetMissingNarrativeField(FieldAuditParaNarrativeSaveRequest request)
            {
            if (string.IsNullOrWhiteSpace(request.Implications))
                {
                return "IMPLICATIONS";
                }

            if (string.IsNullOrWhiteSpace(request.Recommendations))
                {
                return "RECOMMENDATIONS";
                }

            if (string.IsNullOrWhiteSpace(request.ManagementComments))
                {
                return "MANAGEMENT / BRANCH COMMENTS";
                }

            if (string.IsNullOrWhiteSpace(request.AuditorFurtherComments))
                {
                return "AUDITOR’S FURTHER COMMENTS";
                }

            if (string.IsNullOrWhiteSpace(request.SvpRemarks))
                {
                return "REMARKS OF SVP / INCHARGE";
                }

            return string.Empty;
            }

        private IActionResult RedirectToLocal(string returnUrl, string fallbackAction)
            {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                return Redirect(returnUrl);
                }

            return RedirectToAction(fallbackAction);
            }

        private static IEnumerable<NplSnapshotRowModel> CreateDefaultNplRows()
            {
            return new List<NplSnapshotRowModel>
                {
                new NplSnapshotRowModel { Category = "OAEM" },
                new NplSnapshotRowModel { Category = "Substandard" },
                new NplSnapshotRowModel { Category = "Doubtful" },
                new NplSnapshotRowModel { Category = "Loss" }
                };
            }

        private FieldAuditInputSectionViewModel BuildInputSectionViewModel(int engId, bool isFinal, IEnumerable<string> fieldCodes, string sectionCode)
            {
            var sections = _dbConnection.GetFieldAuditNarrativeSections(engId);
            var lookup = sections.ToDictionary(
                section => section.SectionCode ?? string.Empty,
                section => section.TextBlock ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var code in fieldCodes ?? Array.Empty<string>())
                {
                fields[code] = lookup.TryGetValue(code, out var value) ? value : string.Empty;
                }

            if (string.Equals(sectionCode, FieldAuditReportSectionCodes.KpiSnapshot, StringComparison.OrdinalIgnoreCase))
                {
                foreach (var entry in lookup)
                    {
                    if (entry.Key.StartsWith("KPI_EXTRA_ROW_", StringComparison.OrdinalIgnoreCase))
                        {
                        fields[entry.Key] = entry.Value ?? string.Empty;
                        }
                    }
                }

            if (string.Equals(sectionCode, FieldAuditReportSectionCodes.NplSnapshot, StringComparison.OrdinalIgnoreCase))
                {
                foreach (var entry in lookup)
                    {
                    if (entry.Key.StartsWith("NPL_PERIOD_", StringComparison.OrdinalIgnoreCase)
                        || entry.Key.StartsWith("NPL_EXTRA_ROW_", StringComparison.OrdinalIgnoreCase))
                        {
                        fields[entry.Key] = entry.Value ?? string.Empty;
                        }
                    }
                }

            var overview = _dbConnection.GetFieldAuditReportOverview(engId);
            var statistics = _dbConnection.GetFieldAuditStatistics(engId);
            var auditTeam = _dbConnection.GetFieldAuditTeamDetails(engId);

            return new FieldAuditInputSectionViewModel
                {
                EngagementId = engId,
                EntityId = overview?.EntityId ?? 0,
                IsReadOnly = isFinal,
                SectionCode = sectionCode,
                Fields = fields,
                ReportStatus = isFinal ? "FINAL" : "DRAFT",
                AuditTeam = auditTeam,
                StatisticsRows = statistics,
                Overview = overview ?? new FieldAuditReportOverviewModel()
                };
            }

        private static string NormalizeReturnAction(string returnAction)
            {
            return returnAction switch
                {
                nameof(NarrativeSections) => nameof(NarrativeSections),
                nameof(StaffSnapshot) => nameof(StaffSnapshot),
                nameof(KpiSnapshot) => nameof(KpiSnapshot),
                nameof(NplSnapshot) => nameof(NplSnapshot),
                _ => nameof(ReportOverview)
                };
            }

        private static readonly string[] NarrativeFieldCodes =
            {
            "FIELD_029", "FIELD_030", "FIELD_031", "FIELD_032", "FIELD_033",
            "FIELD_161", "FIELD_163", "FIELD_164", "FIELD_166", "FIELD_167", "FIELD_169",
            "FIELD_170", "FIELD_172", "FIELD_173", "FIELD_175", "FIELD_176",
            "FIELD_204", "FIELD_205", "FIELD_206", "FIELD_207", "FIELD_208"
            };

        private static readonly string[] StaffFieldCodes =
            {
            "FIELD_034", "FIELD_035", "FIELD_036", "FIELD_037", "FIELD_038", "FIELD_039", "FIELD_040", "FIELD_041", "FIELD_042",
            "FIELD_043", "FIELD_044", "FIELD_045", "FIELD_046", "FIELD_047", "FIELD_048", "FIELD_049", "FIELD_050", "FIELD_051",
            "FIELD_052"
            };

        private static readonly string[] KpiFieldCodes =
            {
            "FIELD_053", "FIELD_054", "FIELD_055", "FIELD_056", "FIELD_057", "FIELD_058", "FIELD_059",
            "FIELD_060", "FIELD_061", "FIELD_062", "FIELD_063", "FIELD_064", "FIELD_065", "FIELD_066",
            "FIELD_067", "FIELD_068", "FIELD_069", "FIELD_070", "FIELD_071", "FIELD_072", "FIELD_073",
            "FIELD_074", "FIELD_075", "FIELD_076", "FIELD_077", "FIELD_078", "FIELD_079", "FIELD_080",
            "FIELD_081", "FIELD_082", "FIELD_083", "FIELD_084", "FIELD_085", "FIELD_086", "FIELD_087",
            "FIELD_088", "FIELD_089", "FIELD_090", "FIELD_091", "FIELD_092", "FIELD_093", "FIELD_094",
            "FIELD_095", "FIELD_096", "FIELD_097", "FIELD_098", "FIELD_099", "FIELD_100", "FIELD_101", "FIELD_102",
            "FIELD_103", "FIELD_104", "FIELD_105", "FIELD_106", "FIELD_107", "FIELD_108", "FIELD_109", "FIELD_110"
            };

        private static readonly string[] NplFieldCodes =
            {
            "FIELD_111", "FIELD_112", "FIELD_113", "FIELD_114", "FIELD_115", "FIELD_116", "FIELD_117", "FIELD_118", "FIELD_119",
            "FIELD_120", "FIELD_121", "FIELD_122", "FIELD_123", "FIELD_124", "FIELD_125", "FIELD_126", "FIELD_127", "FIELD_128",
            "FIELD_129", "FIELD_130", "FIELD_131", "FIELD_132", "FIELD_133", "FIELD_134", "FIELD_135", "FIELD_136", "FIELD_137",
            "FIELD_138", "FIELD_139", "FIELD_140", "FIELD_141", "FIELD_142", "FIELD_143", "FIELD_144", "FIELD_145", "FIELD_146",
            "FIELD_147", "FIELD_148", "FIELD_149", "FIELD_150", "FIELD_151", "FIELD_152", "FIELD_153", "FIELD_154", "FIELD_155",
            "FIELD_156", "FIELD_157", "FIELD_158", "FIELD_159", "FIELD_160"
            };
        }

    public class FieldAuditPdfStatisticsRowInput
        {
        public string Nature { get; set; }
        public int? Reported { get; set; }
        public int? Rectified { get; set; }
        public int? Outstanding { get; set; }
        public string Remarks { get; set; }
        }

    public class FieldAuditIncomeLeakageRowInput
        {
        public string Description { get; set; }
        public string Area { get; set; }
        public decimal? Amount { get; set; }
        }

    public class FieldAuditPdfStatisticsSaveRequest
        {
        public int EngId { get; set; }
        public int? ReportVersion { get; set; }
        public List<FieldAuditPdfStatisticsRowInput> Rows { get; set; } = new List<FieldAuditPdfStatisticsRowInput>();
        }

    public class FieldAuditIncomeLeakageSaveRequest
        {
        public int EngId { get; set; }
        public int? ReportVersion { get; set; }
        public List<FieldAuditIncomeLeakageRowInput> Rows { get; set; } = new List<FieldAuditIncomeLeakageRowInput>();
        }

    public class FieldAuditOverallConclusionSaveRequest
        {
        public int EngId { get; set; }
        public int? ReportVersion { get; set; }
        public string OverallConclusionHtml { get; set; }
        public string NonAddressableHtml { get; set; }
        public string FraudProneHtml { get; set; }
        public string RegulatoryHtml { get; set; }
        public string SafetySecurityHtml { get; set; }
        }

    public class FieldAuditOverallConclusionInput
        {
        public string OverallConclusionHtml { get; set; }
        public string NonAddressableHtml { get; set; }
        public string FraudProneHtml { get; set; }
        public string RegulatoryHtml { get; set; }
        public string SafetySecurityHtml { get; set; }
        }
    }
