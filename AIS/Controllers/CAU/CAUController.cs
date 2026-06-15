using AIS.Models;
using AIS.Models.CAU;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using AIS.Services;
namespace AIS.Controllers
    {

    public class CAUController : Controller
        {
        private static readonly string[] CommercialAuditPermissionPaths =
            {
            "/CAU/om",
            "/CAU/pdp",
            "/CAU/arpse",
            "/CAU/om_creation",
            "/CAU/om_reply",
            "/CAU/monitoring_oms",
            "/CAU/reports",
            "/CAU/ARPSEYearWiseReport",
            "/CAU/ArpseFollowUp",
            "/CAU/ParaDetailedView"
            };

        private readonly ILogger<CAUController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly IPageIdResolver _pageIdResolver;
        private readonly DBConnection dBConnection;
        public CAUController(ILogger<CAUController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService, IPageIdResolver pageIdResolver)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _pageIdResolver = pageIdResolver;
            }

        private bool PrepareCauView(bool includeMenuData = true)
            {
            if (includeMenuData)
                {
                ViewData["TopMenu"] = tm.GetTopMenus();
                ViewData["TopMenuPages"] = tm.GetTopMenusPages();
                }

            if (!User.Identity.IsAuthenticated)
                {
                return false;
                }

            return HasCommercialAuditAccess();
            }

        private bool HasCommercialAuditAccess()
            {
            if (!sessionHandler.TryGetUser(out var user))
                {
                return false;
                }

            _permissionService.EnsurePermissionsCached(user);

            var currentPageId = sessionHandler.GetPageId();
            if (currentPageId > 0 && _permissionService.HasViewPermission(user, currentPageId))
                {
                return true;
                }

            foreach (var path in CommercialAuditPermissionPaths)
                {
                var pageId = _pageIdResolver.ResolvePageId(path);
                if (pageId > 0 && _permissionService.HasViewPermission(user, pageId))
                    {
                    return true;
                    }
                }

            return false;
            }

        private IActionResult RedirectForUnauthorizedOrMissingPermission()
            {
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            return RedirectToAction("Index", "PageNotFound");
            }

        private void PrepareCommercialAuditWorkflowLookups()
            {
            ViewData["AuditYearList"] = dBConnection.GetInsYearsForCAU();
            ViewData["OmAuditYearList"] = GetOrderedOmAuditYears();
            ViewData["ArpseYearList"] = dBConnection.GetParaPrintingYearsForCAU();
            ViewData["WorkflowStages"] = new List<CommercialAuditWorkflowStage>
                {
                new CommercialAuditWorkflowStage { Key = "workflow", Title = "Workflow", Description = "Choose a stage and continue the Commercial Audit workflow." },
                new CommercialAuditWorkflowStage { Key = "om", Title = "Stage 1: OM", Description = "Create and manage Office Memorandums." },
                new CommercialAuditWorkflowStage { Key = "pdp", Title = "Stage 2: PDP", Description = "Create PDPs and link one PDP with multiple OMs." },
                new CommercialAuditWorkflowStage { Key = "arpse", Title = "Stage 3: ARPSE", Description = "Manage ARPSE headers with DAC and PAC follow-up entries." }
                };
            }

        private IReadOnlyList<AuditPeriodModel> GetOrderedOmAuditYears()
            {
            var periods = dBConnection.GetInsYearsForCAU() ?? new List<AuditPeriodModel>();
            return periods
                .OrderByDescending(ResolveAuditYearSortValue)
                .ThenByDescending(item => item?.AUDITPERIODID ?? 0)
                .ToList();
            }

        private static int ResolveAuditYearSortValue(AuditPeriodModel period)
            {
            if (period == null)
                {
                return int.MinValue;
                }

            var description = period.DESCRIPTION ?? string.Empty;
            var match = Regex.Match(description, @"(19|20)\d{2}");
            if (match.Success && int.TryParse(match.Value, out var parsedYear))
                {
                return parsedYear;
                }

            return period.AUDITPERIODID;
            }

        private CommercialAuditWorkflowViewModel BuildCommercialAuditWorkflowModel(string requestedStepKey)
            {
            var steps = new List<CommercialAuditWorkflowStep>
                {
                new CommercialAuditWorkflowStep
                    {
                    StepNo = 1,
                    StepKey = "om-entry",
                    StageKey = "om",
                    Title = "OM Entry",
                    Description = "Create or update Office Memorandums without squeezing the form beside the register.",
                    PartialViewName = "~/Views/CAU/_OmEntry.cshtml"
                    },
                new CommercialAuditWorkflowStep
                    {
                    StepNo = 2,
                    StepKey = "om-register",
                    StageKey = "om",
                    Title = "OM Register",
                    Description = "Review saved OMs and reopen any record for editing.",
                    PartialViewName = "~/Views/CAU/_OmRegister.cshtml"
                    },
                new CommercialAuditWorkflowStep
                    {
                    StepNo = 3,
                    StepKey = "pdp-entry",
                    StageKey = "pdp",
                    Title = "PDP Entry",
                    Description = "Capture the PDP header and narrative details in a dedicated full-width step.",
                    PartialViewName = "~/Views/CAU/_PdpEntry.cshtml"
                    },
                new CommercialAuditWorkflowStep
                    {
                    StepNo = 4,
                    StepKey = "pdp-linking",
                    StageKey = "pdp",
                    Title = "PDP Linking",
                    Description = "Select a PDP, review the register, and link one or more OMs from a separate step.",
                    PartialViewName = "~/Views/CAU/_PdpLinking.cshtml"
                    },
                new CommercialAuditWorkflowStep
                    {
                    StepNo = 5,
                    StepKey = "arpse-header",
                    StageKey = "arpse",
                    Title = "ARPSE Header",
                    Description = "Maintain the ARPSE header details in a dedicated full-width editor.",
                    PartialViewName = "~/Views/CAU/_ArpseHeader.cshtml"
                    },
                new CommercialAuditWorkflowStep
                    {
                    StepNo = 6,
                    StepKey = "arpse-linking",
                    StageKey = "arpse",
                    Title = "ARPSE Linking",
                    Description = "Select an ARPSE, review the register, and link one or more PDPs from a separate step.",
                    PartialViewName = "~/Views/CAU/_ArpseLinking.cshtml"
                    },
                new CommercialAuditWorkflowStep
                    {
                    StepNo = 7,
                    StepKey = "arpse-monitoring",
                    StageKey = "arpse",
                    Title = "ARPSE Monitoring",
                    Description = "Manage ARPSE register selection plus DAC and PAC follow-up in one wide monitoring workspace.",
                    PartialViewName = "~/Views/CAU/_ArpseMonitoring.cshtml"
                    }
                };

            var resolvedStep = steps.FirstOrDefault(step =>
                string.Equals(step.StepKey, requestedStepKey, StringComparison.OrdinalIgnoreCase));

            return new CommercialAuditWorkflowViewModel
                {
                CurrentStepKey = resolvedStep?.StepKey ?? steps.First().StepKey,
                Steps = steps
                };
            }

        private void PrepareCommercialAuditStepContext(long? omId, long? pdpId, long? arpseId)
            {
            ViewData["CommercialAuditSelectedOmId"] = omId;
            ViewData["CommercialAuditSelectedPdpId"] = pdpId;
            ViewData["CommercialAuditSelectedArpseId"] = arpseId;
            }

        private IActionResult RenderCommercialAuditWorkflow(string initialStepKey)
            {
            try
                {
                if (!PrepareCauView())
                    {
                    return RedirectForUnauthorizedOrMissingPermission();
                    }

                var model = BuildCommercialAuditWorkflowModel(initialStepKey);
                PrepareCommercialAuditWorkflowLookups();
                ViewData["Title"] = "Commercial Audit Workflow";
                ViewData["CommercialAuditStage"] = "workflow";
                ViewData["CommercialAuditWorkflowModel"] = model;
                return View("../CAU/workflow", model);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to render Commercial Audit workflow for step {StepKey}.", initialStepKey);
                return StatusCode(500, "Unable to load the Commercial Audit workflow right now.");
                }
            }

        private IActionResult RenderCommercialAuditStepPartial(string stepKey, long? omId, long? pdpId, long? arpseId)
            {
            try
                {
                if (!PrepareCauView(includeMenuData: false))
                    {
                    return User.Identity.IsAuthenticated ? Forbid() : Unauthorized();
                    }

                var workflowModel = BuildCommercialAuditWorkflowModel(stepKey);
                var step = workflowModel.Steps.FirstOrDefault(item =>
                    string.Equals(item.StepKey, workflowModel.CurrentStepKey, StringComparison.OrdinalIgnoreCase));
                if (step == null)
                    {
                    return NotFound();
                    }

                PrepareCommercialAuditWorkflowLookups();
                ViewData["CommercialAuditStage"] = step.StageKey;
                PrepareCommercialAuditStepContext(omId, pdpId, arpseId);
                return PartialView(step.PartialViewName);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to load Commercial Audit step partial for step {StepKey}.", stepKey);
                return StatusCode(500, "Unable to load the requested Commercial Audit step right now.");
                }
            }

        private IActionResult RenderARPSEYearWiseReport()
            {
            try
                {
                if (!PrepareCauView())
                    {
                    return RedirectForUnauthorizedOrMissingPermission();
                    }

                ViewData["Title"] = "ARPSE Year DAC / PAC Report";
                ViewData["CommercialAuditStage"] = "reports";
                ViewData["ARPSEYears"] = dBConnection.GetARPSEYears();
                return View("../CAU/ARPSEYearWiseReport");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to render ARPSE Year DAC / PAC Report.");
                return StatusCode(500, "Unable to load the ARPSE Year DAC / PAC Report right now.");
                }
            }
        private IActionResult RenderParaDetailedView()
            {
            try
                {
                if (!PrepareCauView())
                    {
                    return RedirectForUnauthorizedOrMissingPermission();
                    }

                ViewData["Title"] = "Para Detailed View";
                ViewData["CommercialAuditStage"] = "reports";
                ViewData["ARPSEYears"] = dBConnection.GetARPSEYears();
                return View("../CAU/ParaDetailedView");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to render Para Detailed View report.");
                return StatusCode(500, "Unable to load the Para Detailed View report right now.");
                }
            }

        private IActionResult RenderArpseFollowUp(long? arpseId)
            {
            try
                {
                if (!PrepareCauView())
                    {
                    return RedirectForUnauthorizedOrMissingPermission();
                    }

                ViewData["Title"] = "ARPSE DAC / PAC Follow-up";
                ViewData["CommercialAuditStage"] = "arpse";
                ViewData["SelectedArpseId"] = arpseId.GetValueOrDefault();
                return View("../CAU/ArpseFollowUp");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to render ARPSE follow-up for ARPSE {ArpseId}.", arpseId);
                return StatusCode(500, "Unable to load ARPSE follow-up right now.");
                }
            }

        private List<CommercialAuditParaDetailedGridRow> BuildParaDetailedViewGrid(int arpseYear)
            {
            var headers = dBConnection.GetCommercialAuditArpseHeaders() ?? new List<CommercialAuditArpseHeaderModel>();

            return headers
                .Where(item => item != null && item.ArpseId.GetValueOrDefault() > 0)
                .Where(item => ResolveArpseYearForHeader(item) == arpseYear || item.ArpseYearId.GetValueOrDefault() == arpseYear)
                .OrderBy(item => item.ParaNo)
                .Select(item => new CommercialAuditParaDetailedGridRow
                    {
                    ArpseId = item.ArpseId.GetValueOrDefault(),
                    ParaNo = item.ParaNo,
                    ParaTitle = item.GistOfPara
                    })
                .ToList();
            }

        private CommercialAuditParaDetailedViewModel BuildParaDetailedViewDetail(int arpseId)
            {
            var headers = dBConnection.GetCommercialAuditArpseHeaders() ?? new List<CommercialAuditArpseHeaderModel>();
            var header = headers.FirstOrDefault(item => item != null && item.ArpseId.GetValueOrDefault() == arpseId);
            if (header == null)
                {
                return null;
                }

            var arpsePdpMappings = dBConnection.GetCommercialAuditArpsePdpMappings(arpseId) ?? new List<CommercialAuditArpsePdpMappingModel>();
            var pdpIds = arpsePdpMappings
                .Select(item => item == null ? 0 : item.PdpId.GetValueOrDefault())
                .Where(item => item > 0)
                .Distinct()
                .ToList();
            var pdps = (dBConnection.GetCommercialAuditPdps() ?? new List<CommercialAuditPdpModel>())
                .Where(item => item != null && pdpIds.Contains(item.PdpId.GetValueOrDefault()))
                .ToList();

            var omIds = new HashSet<int>();
            foreach (var pdp in pdps)
                {
                var pdpId = pdp == null ? 0 : pdp.PdpId.GetValueOrDefault();
                if (pdpId <= 0)
                    {
                    continue;
                    }

                foreach (var mapping in dBConnection.GetCommercialAuditPdpMappings(pdpId) ?? new List<CommercialAuditPdpOmMappingModel>())
                    {
                    var omId = mapping == null ? 0 : mapping.OmId.GetValueOrDefault();
                    if (omId > 0)
                        {
                        omIds.Add(omId);
                        }
                    }
                }

            var oms = (dBConnection.GetCommercialAuditOms() ?? new List<CommercialAuditOmModel>())
                .Where(item => item != null && omIds.Contains(item.OmId.GetValueOrDefault()))
                .ToList();
            var dacEntries = dBConnection.GetCommercialAuditArpseDacEntries(arpseId) ?? new List<CommercialAuditArpseDacEntryModel>();
            var pacEntries = dBConnection.GetCommercialAuditArpsePacEntries(arpseId) ?? new List<CommercialAuditArpsePacEntryModel>();

            var model = new CommercialAuditParaDetailedViewModel
                {
                ArpseId = arpseId,
                ParaNo = header.ParaNo,
                ParaTitle = header.GistOfPara
                };

            model.Sections.Add(new CommercialAuditParaDetailedSection
                {
                Title = "OM / Original Observation",
                Items = oms.Select(item => BuildCommercialAuditDetailEntry(BuildParaDetailHeading("OM", item.OmNo, item.GistOfOm, null), item.BodyOfOm)).Where(HasMeaningfulDetailHtml).ToList()
                });
            model.Sections.Add(new CommercialAuditParaDetailedSection
                {
                Title = "Management Reply",
                Items = oms.Select(item => BuildCommercialAuditDetailEntry(BuildParaDetailHeading("OM", item.OmNo, item.GistOfOm, null), item.ManagementResponse)).Where(HasMeaningfulDetailHtml).ToList()
                });
            model.Sections.Add(new CommercialAuditParaDetailedSection
                {
                Title = "PDP",
                Items = pdps.Select(item => BuildCommercialAuditDetailEntry(BuildParaDetailHeading("PDP", item.PdpNo, item.GistOfPdp, null), item.BodyOfPdp)).Where(HasMeaningfulDetailHtml).ToList()
                });
            model.Sections.Add(new CommercialAuditParaDetailedSection
                {
                Title = "Management Response",
                Items = pdps.Select(item => BuildCommercialAuditDetailEntry(BuildParaDetailHeading("PDP", item.PdpNo, item.GistOfPdp, null), item.ManagementResponse)).Where(HasMeaningfulDetailHtml).ToList()
                });
            model.Sections.Add(new CommercialAuditParaDetailedSection
                {
                Title = "ARPSE",
                Items = new List<string> { BuildCommercialAuditDetailEntry(BuildParaDetailHeading("ARPSE", header.ParaNo, header.GistOfPara, null), header.BodyOfPara) }.Where(HasMeaningfulDetailHtml).ToList()
                });
            model.Sections.Add(new CommercialAuditParaDetailedSection
                {
                Title = "DAC Recommendations",
                Items = dacEntries.Select(item => BuildCommercialAuditDetailEntry(BuildParaDetailHeading("DAC", null, null, item.DacDate), item.DacRecommendation)).Where(HasMeaningfulDetailHtml).ToList()
                });
            model.Sections.Add(new CommercialAuditParaDetailedSection
                {
                Title = "PAC Directives",
                Items = pacEntries.Select(item => BuildCommercialAuditDetailEntry(BuildParaDetailHeading("PAC", null, null, item.PacDate), item.PacDirective)).Where(HasMeaningfulDetailHtml).ToList()
                });

            var postArpseResponses = dacEntries
                .Select(item => BuildCommercialAuditDetailEntry(BuildParaDetailHeading("DAC Management Response", null, null, item.DacDate), item.UpdatedStatus))
                .Concat(pacEntries.Select(item => BuildCommercialAuditDetailEntry(BuildParaDetailHeading("PAC Management Response", null, null, item.PacDate), item.UpdatedStatus)))
                .Where(HasMeaningfulDetailHtml)
                .ToList();
            model.Sections.Add(new CommercialAuditParaDetailedSection
                {
                Title = "Management Responses after ARPSE",
                Items = postArpseResponses
                });

            return model;
            }

        private static int ResolveArpseYearForHeader(CommercialAuditArpseHeaderModel header)
            {
            if (header == null)
                {
                return 0;
                }

            var match = Regex.Match(header.ArpseYearText ?? string.Empty, @"(19|20)\d{2}");
            if (match.Success && int.TryParse(match.Value, out var parsedYear))
                {
                return parsedYear;
                }

            return header.ArpseYearId.GetValueOrDefault();
            }

        private static string BuildParaDetailHeading(string prefix, string number, string title, DateTime? date)
            {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(prefix))
                {
                parts.Add(prefix.Trim());
                }
            if (!string.IsNullOrWhiteSpace(number))
                {
                parts.Add(number.Trim());
                }
            if (date.HasValue)
                {
                parts.Add(date.Value.ToString("dd-MMM-yyyy"));
                }

            var heading = string.Join(" - ", parts.Where(item => !string.IsNullOrWhiteSpace(item)));
            if (!string.IsNullOrWhiteSpace(title))
                {
                heading = string.IsNullOrWhiteSpace(heading) ? title.Trim() : heading + ": " + title.Trim();
                }

            return heading;
            }

        private static string BuildCommercialAuditDetailEntry(string heading, string html)
            {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(heading))
                {
                parts.Add("<div class=\"para-detail-entry-heading\"><strong>" + WebUtility.HtmlEncode(heading.Trim()) + "</strong></div>");
                }
            if (HasMeaningfulDetailHtml(html))
                {
                parts.Add("<div class=\"para-detail-entry-body\">" + html + "</div>");
                }

            return string.Join(string.Empty, parts);
            }

        private static bool HasMeaningfulDetailHtml(string html)
            {
            if (string.IsNullOrWhiteSpace(html))
                {
                return false;
                }

            var text = Regex.Replace(html, "<.*?>", " ");
            text = WebUtility.HtmlDecode(text)?.Replace("\u00A0", " ") ?? string.Empty;
            return !string.IsNullOrWhiteSpace(text);
            }

        [HttpGet("CAU/workflow")]
        public IActionResult workflow()
            {
            return RenderCommercialAuditWorkflow(null);
            }

        [HttpGet("CAU/om")]
        public IActionResult om()
            {
            return RenderCommercialAuditWorkflow("om-entry");
            }

        [HttpGet("CAU/pdp")]
        public IActionResult pdp()
            {
            return RenderCommercialAuditWorkflow("pdp-entry");
            }

        [HttpGet("CAU/arpse")]
        public IActionResult arpse()
            {
            return RenderCommercialAuditWorkflow("arpse-header");
            }


        [HttpGet("CAU/om_creation")]
        public IActionResult om_creation()
            {
            return RenderCommercialAuditWorkflow("om-entry");
            }


        [HttpGet("CAU/om_reply")]
        public IActionResult om_reply()
            {
            return RenderCommercialAuditWorkflow("pdp-entry");
            }

        [HttpGet("CAU/monitoring_oms")]
        public IActionResult monitoring_oms()
            {
            return RenderCommercialAuditWorkflow("arpse-monitoring");
            }

        [HttpGet("CAU/reports")]
        public IActionResult reports()
            {
            return RenderARPSEYearWiseReport();
            }

        [HttpGet("CAU/ARPSEYearWiseReport")]
        public IActionResult ARPSEYearWiseReport()
            {
            return RenderARPSEYearWiseReport();
            }
        [HttpGet("CAU/ArpseFollowUp")]
        public IActionResult ArpseFollowUp(long? arpseId)
            {
            return RenderArpseFollowUp(arpseId);
            }

        [HttpGet("CAU/ParaDetailedView")]
        public IActionResult ParaDetailedView()
            {
            return RenderParaDetailedView();
            }

        [HttpGet("CAU/GetParaDetailedViewGrid")]
        public IActionResult GetParaDetailedViewGrid(int arpseYear)
            {
            try
                {
                if (!PrepareCauView(includeMenuData: false))
                    {
                    return User.Identity.IsAuthenticated ? Forbid() : Unauthorized();
                    }

                if (arpseYear <= 0)
                    {
                    return BadRequest(new { success = false, message = "ARPSE Year is required." });
                    }

                return Json(BuildParaDetailedViewGrid(arpseYear));
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to load Para Detailed View grid for year {ArpseYear}.", arpseYear);
                return StatusCode(500, new { success = false, message = "Unable to load para list right now." });
                }
            }

        [HttpGet("CAU/GetParaDetailedViewDetail")]
        public IActionResult GetParaDetailedViewDetail(int arpseId)
            {
            try
                {
                if (!PrepareCauView(includeMenuData: false))
                    {
                    return User.Identity.IsAuthenticated ? Forbid() : Unauthorized();
                    }

                if (arpseId <= 0)
                    {
                    return BadRequest(new { success = false, message = "ARPSE para is required." });
                    }

                var detail = BuildParaDetailedViewDetail(arpseId);
                if (detail == null)
                    {
                    return NotFound(new { success = false, message = "No record found." });
                    }

                return Json(detail);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to load Para Detailed View detail for ARPSE {ArpseId}.", arpseId);
                return StatusCode(500, new { success = false, message = "Unable to load para detail right now." });
                }
            }

        [HttpGet("CAU/GetARPSEYearWiseReport")]
        public IActionResult GetARPSEYearWiseReport(int arpseYear)
            {
            try
                {
                if (!PrepareCauView(includeMenuData: false))
                    {
                    return User.Identity.IsAuthenticated ? Forbid() : Unauthorized();
                    }

                if (arpseYear <= 0)
                    {
                    return BadRequest(new { success = false, message = "ARPSE Year is required." });
                    }

                return Json(dBConnection.GetARPSEYearWiseReport(arpseYear));
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to load ARPSE Year Wise report data for year {ArpseYear}.", arpseYear);
                return StatusCode(500, new { success = false, message = "Unable to load report data right now." });
                }
            }

        [HttpGet("CAU/LoadStepPartial")]
        public IActionResult LoadStepPartial(string stepKey, long? omId = null, long? pdpId = null, long? arpseId = null)
            {
            return RenderCommercialAuditStepPartial(stepKey, omId, pdpId, arpseId);
            }

        [HttpGet("CAU/LoadStep")]
        public IActionResult LoadStep(string stepKey, long? omId = null, long? pdpId = null, long? arpseId = null)
            {
            return LoadStepPartial(stepKey, omId, pdpId, arpseId);
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
