using AIS.Models.FieldAuditReport;
using AIS.Models.ManagementReport;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
    {
    [Route("MANReport")]
    public class MANReportController : Controller
        {
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly TopMenus _topMenus;
        private readonly IPermissionService _permissionService;

        public MANReportController(
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            TopMenus topMenus,
            IPermissionService permissionService)
            {
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _topMenus = topMenus;
            _permissionService = permissionService;
            }

        [HttpGet("Home")]
        public IActionResult Home()
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
                return View(new ManagementReportHomeViewModel { HasActiveEngagement = false });
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (!IsManagementAudit(selected))
                {
                return RedirectToAction("ReportOverview", "FieldAuditReport");
                }

            var model = new ManagementReportHomeViewModel
                {
                HasActiveEngagement = true,
                EntityName = selected?.EntityName ?? string.Empty,
                AuditPeriod = selected?.AuditPeriod ?? string.Empty
                };

            return View(model);
            }

        [HttpGet("Cover")]
        public IActionResult Cover()
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
                return View(new ManagementReportCoverViewModel { HasActiveEngagement = false });
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (!IsManagementAudit(selected))
                {
                return RedirectToAction("ReportOverview", "FieldAuditReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var model = new ManagementReportCoverViewModel
                {
                HasActiveEngagement = true,
                EngagementId = engId,
                Cover = _dbConnection.GetManagementAuditCover(engId),
                AuditTeam = _dbConnection.GetManagementAuditTeamDetails(engId)
                };

            return View(model);
            }

        [HttpGet("AuditObjectiveScope")]
        public IActionResult AuditObjectiveScope()
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
                return View(new ManagementReportObjectiveScopeViewModel { IsReadOnly = true });
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (!IsManagementAudit(selected))
                {
                return RedirectToAction("ReportOverview", "FieldAuditReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var fields = _dbConnection.GetManReportObjectiveScope(engId);
            return View(new ManagementReportObjectiveScopeViewModel
                {
                EngagementId = engId,
                IsReadOnly = isFinal,
                Fields = fields
                });
            }

        [HttpPost("AuditObjectiveScope")]
        [ValidateAntiForgeryToken]
        public IActionResult AuditObjectiveScope(ManagementReportObjectiveScopeViewModel model, string submitAction)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (!TryResolveEngagementId(out var engId))
                {
                TempData["ManReportMessage"] = "Select an engagement to continue.";
                return RedirectToAction(nameof(Home));
                }

            if (_dbConnection.IsFieldAuditReportFinal(engId))
                {
                TempData["ManReportMessage"] = "Report is finalized and cannot be edited.";
                return RedirectToAction(nameof(AuditObjectiveScope));
                }

            var fields = model?.Fields ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in fields)
                {
                _dbConnection.SaveManReportTextBlock(engId, entry.Key, entry.Value ?? string.Empty);
                }

            TempData["ManReportMessage"] = "Objective & Scope saved.";
            if (string.Equals(submitAction, "next", StringComparison.OrdinalIgnoreCase))
                {
                return RedirectToAction(nameof(ExecutiveSummary));
                }

            return RedirectToAction(nameof(AuditObjectiveScope));
            }

        [HttpGet("ExecutiveSummary")]
        public IActionResult ExecutiveSummary()
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
                return View(new ManagementReportExecutiveSummaryViewModel { IsReadOnly = true });
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (!IsManagementAudit(selected))
                {
                return RedirectToAction("ReportOverview", "FieldAuditReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var summary = _dbConnection.GetManReportExecutiveSummary(engId);
            return View(new ManagementReportExecutiveSummaryViewModel
                {
                EngagementId = engId,
                IsReadOnly = isFinal,
                ExecutiveSummary = summary
                });
            }

        [HttpPost("ExecutiveSummary")]
        [ValidateAntiForgeryToken]
        public IActionResult ExecutiveSummary(ManagementReportExecutiveSummaryViewModel model, string submitAction)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (!TryResolveEngagementId(out var engId))
                {
                TempData["ManReportMessage"] = "Select an engagement to continue.";
                return RedirectToAction(nameof(Home));
                }

            if (_dbConnection.IsFieldAuditReportFinal(engId))
                {
                TempData["ManReportMessage"] = "Report is finalized and cannot be edited.";
                return RedirectToAction(nameof(ExecutiveSummary));
                }

            _dbConnection.SaveManReportTextBlock(engId, ManagementReportSectionCodes.ExecutiveSummary, model?.ExecutiveSummary ?? string.Empty);
            TempData["ManReportMessage"] = "Executive Summary saved.";
            if (string.Equals(submitAction, "next", StringComparison.OrdinalIgnoreCase))
                {
                return RedirectToAction(nameof(StaffSnapshot));
                }

            return RedirectToAction(nameof(ExecutiveSummary));
            }

        [HttpGet("StaffSnapshot")]
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
            if (!IsManagementAudit(selected))
                {
                return RedirectToAction("ReportOverview", "FieldAuditReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            return View(new FieldAuditInputSectionViewModel
                {
                EngagementId = engId,
                IsReadOnly = isFinal
                });
            }

        [HttpGet("GetStaffSnapshotRows")]
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

        [HttpPost("SaveStaffSnapshotRows")]
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

                if (string.IsNullOrWhiteSpace(row.PpNo)
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

        [HttpGet("AuditObservations")]
        public IActionResult AuditObservations()
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
                return View(new ManagementReportObservationsViewModel());
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (!IsManagementAudit(selected))
                {
                return RedirectToAction("ReportOverview", "FieldAuditReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var observations = _dbConnection.GetManReportObservations(engId);
            return View(new ManagementReportObservationsViewModel
                {
                EngagementId = engId,
                Observations = observations
                });
            }

        [HttpGet("ParasSettledDuringAudit")]
        public IActionResult ParasSettledDuringAudit()
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
                return View(new ManagementReportSettledParasViewModel());
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (!IsManagementAudit(selected))
                {
                return RedirectToAction("ReportOverview", "FieldAuditReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var paras = _dbConnection.GetManReportSettledParas(engId);
            return View(new ManagementReportSettledParasViewModel
                {
                EngagementId = engId,
                Paras = paras
                });
            }

        [HttpGet("Finalize")]
        public IActionResult Finalize()
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
                return View(new ManagementReportFinalizeViewModel());
                }

            var selected = selector.Options.FirstOrDefault(option => option.EngagementId == selector.ActiveEngagementId);
            if (!IsManagementAudit(selected))
                {
                return RedirectToAction("ReportOverview", "FieldAuditReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var checklist = BuildChecklist(engId);
            return View(new ManagementReportFinalizeViewModel
                {
                EngagementId = engId,
                IsFinal = isFinal,
                Checklist = checklist,
                CanFinalize = !isFinal && checklist.IsComplete
                });
            }

        [HttpPost("Finalize")]
        [ValidateAntiForgeryToken]
        public IActionResult Finalize(ManagementReportFinalizeViewModel model)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (!TryResolveEngagementId(out var engId))
                {
                TempData["ManReportMessage"] = "Select an engagement to continue.";
                return RedirectToAction(nameof(Home));
                }

            if (_dbConnection.IsFieldAuditReportFinal(engId))
                {
                return RedirectToAction(nameof(Finalize));
                }

            var checklist = BuildChecklist(engId);
            if (!checklist.IsComplete)
                {
                TempData["ManReportMessage"] = "Complete all mandatory sections before finalizing.";
                return RedirectToAction(nameof(Finalize));
                }

            _dbConnection.FinalizeFieldAuditReport(engId);
            return RedirectToAction(nameof(Finalize));
            }

        [HttpPost("SetActiveEngagement")]
        public IActionResult SetActiveEngagement(int engagementId, string returnUrl)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (engagementId <= 0 || !GetAuthorizedEngagementIds().Contains(engagementId))
                {
                TempData["ManReportMessage"] = "Select a valid engagement to continue.";
                return RedirectToAction(nameof(Home));
                }

            _sessionHandler.SetActiveEngagementId(engagementId);

            var selected = _dbConnection
                .GetReportEntities()
                .FirstOrDefault(option => option.EngagementId == engagementId);
            if (IsManagementAudit(selected))
                {
                return RedirectToAction(nameof(Home));
                }

            return RedirectToLocal(returnUrl, nameof(Home));
            }

        [HttpPost("ClearEngagement")]
        public IActionResult ClearEngagement(string returnUrl)
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            _sessionHandler.ClearActiveEngagementId();
            TempData["ManReportMessage"] = "Engagement cleared. Select a new engagement to continue.";
            return RedirectToLocal(returnUrl, nameof(Home));
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
                    {
                    continue;
                    }

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

        private ManagementReportChecklistModel BuildChecklist(int engId)
            {
            var cover = _dbConnection.GetManagementAuditCover(engId);
            var objectiveScope = _dbConnection.GetManReportObjectiveScope(engId);
            var summary = _dbConnection.GetManReportExecutiveSummary(engId);
            var staffRows = _dbConnection.GetFieldAuditStaffSnapshots(engId);
            var observations = _dbConnection.GetManReportObservations(engId);
            var settledParas = _dbConnection.GetManReportSettledParas(engId);

            bool HasText(string value) => !string.IsNullOrWhiteSpace(value);

            var hasObjectiveScope = new[]
                {
                objectiveScope.TryGetValue(ManagementReportSectionCodes.Objective, out var obj) ? obj : string.Empty,
                objectiveScope.TryGetValue(ManagementReportSectionCodes.Scope, out var scope) ? scope : string.Empty,
                objectiveScope.TryGetValue(ManagementReportSectionCodes.Methodology, out var method) ? method : string.Empty,
                objectiveScope.TryGetValue(ManagementReportSectionCodes.Disclaimer, out var disclaimer) ? disclaimer : string.Empty,
                objectiveScope.TryGetValue(ManagementReportSectionCodes.Introduction, out var intro) ? intro : string.Empty
                }.Any(HasText);

            return new ManagementReportChecklistModel
                {
                HasCoverData = HasText(cover?.EntityName) || HasText(cover?.Reporting) || HasText(cover?.AuditedBy),
                HasObjectiveScope = hasObjectiveScope,
                HasExecutiveSummary = HasText(summary),
                HasStaffSnapshot = staffRows.Any(),
                HasObservations = observations.Any(),
                HasSettledParas = settledParas.Any()
                };
            }

        private IActionResult RedirectToLocal(string returnUrl, string fallbackAction)
            {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                return Redirect(returnUrl);
                }

            return RedirectToAction(fallbackAction);
            }
        }
    }
