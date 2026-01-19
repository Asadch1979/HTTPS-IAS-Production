using AIS.Models.FieldAuditReport;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
    {
    public class FieldAuditReportController : Controller
        {
        private readonly ILogger<FieldAuditReportController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly TopMenus _topMenus;
        private readonly IPermissionService _permissionService;

        public FieldAuditReportController(
            ILogger<FieldAuditReportController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            TopMenus topMenus,
            IPermissionService permissionService)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _topMenus = topMenus;
            _permissionService = permissionService;
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
            if (!IsBranchAudit(selected))
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
            if (!IsBranchAudit(selected))
                {
                return RedirectToAction("Home", "MANReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var baseModel = BuildInputSectionViewModel(engId, isFinal, NarrativeFieldCodes, FieldAuditReportSectionCodes.NarrativeInputs);
            var observationCount = _dbConnection.GetFieldAuditObservationCount(engId);
            var observationDetails = _dbConnection.GetFieldAuditObservationDetails(engId);

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
            if (!IsBranchAudit(selected))
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
            if (!IsBranchAudit(selected))
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
            if (!IsBranchAudit(selected))
                {
                return RedirectToAction("Home", "MANReport");
                }

            var engId = selector.ActiveEngagementId.Value;
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var model = BuildInputSectionViewModel(engId, isFinal, StaffFieldCodes, FieldAuditReportSectionCodes.StaffSnapshot);
            return View(model);
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
            if (!IsBranchAudit(selected))
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
        public IActionResult FinalizeReport(FinalizeReportViewModel model)
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
                return RedirectToAction(nameof(ReportOverview));
                }

            var checklist = _dbConnection.GetFieldAuditReportChecklist(engId);
            if (!checklist.IsComplete)
                {
                TempData["FieldAuditReportMessage"] = "Complete all mandatory sections before finalizing.";
                return RedirectToAction(nameof(FinalizeReport));
                }

            _dbConnection.FinalizeFieldAuditReport(engId);

            return RedirectToAction(nameof(ReportOverview));
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
            return RedirectToLocal(returnUrl, nameof(ReportOverview));
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

        private static bool IsBranchAudit(FieldAuditEngagementOptionModel selected)
            {
            return selected != null
                && string.Equals(selected.AuditType, "B", StringComparison.OrdinalIgnoreCase);
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
                .GetObservationEntitiesForPreConcluding()
                .Where(item => item.ENG_ID.HasValue && item.ENG_ID.Value > 0)
                .Select(item => item.ENG_ID.Value)
                .ToHashSet();
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
                StatisticsRows = statistics
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
    }
