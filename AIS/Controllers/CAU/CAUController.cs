using AIS.Models;
using AIS.Models.CAU;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            "/CAU/reports"
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

        private string ResolveCommercialAuditHostPath(string requestedHostPath = null)
            {
            var normalized = PageIdPathHelper.NormalizePath(string.IsNullOrWhiteSpace(requestedHostPath)
                ? HttpContext?.Request?.Path.Value
                : requestedHostPath);

            var allowedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                "/CAU/workflow",
                "/CAU/om",
                "/CAU/pdp",
                "/CAU/arpse",
                "/CAU/om_creation",
                "/CAU/om_reply",
                "/CAU/monitoring_oms",
                "/CAU/reports"
                };

            if (allowedPaths.Contains(normalized))
                {
                return normalized;
                }

            return "/CAU/workflow";
            }

        private IActionResult RenderCommercialAuditWorkflow(string requestedStepKey)
            {
            if (!PrepareCauView())
                {
                return RedirectForUnauthorizedOrMissingPermission();
                }

            var model = BuildCommercialAuditWorkflowModel(requestedStepKey);
            PrepareCommercialAuditWorkflowLookups();
            ViewData["Title"] = "Commercial Audit Workflow";
            ViewData["CommercialAuditStage"] = "workflow";
            ViewData["CommercialAuditHostPath"] = ResolveCommercialAuditHostPath();
            ViewData["CommercialAuditWorkflowModel"] = model;
            return View("../CAU/workflow", model);
            }

        private IActionResult RenderCommercialAuditStepPartial(string stepKey, string hostPath)
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
            ViewData["CommercialAuditHostPath"] = ResolveCommercialAuditHostPath(hostPath);
            return PartialView(step.PartialViewName);
            }

        [HttpGet("CAU/workflow")]
        public IActionResult workflow(string stepKey = null)
            {
            return RenderCommercialAuditWorkflow(stepKey);
            }

        [HttpGet("CAU/om")]
        public IActionResult om(string stepKey = null)
            {
            return RenderCommercialAuditWorkflow(string.IsNullOrWhiteSpace(stepKey) ? "om-entry" : stepKey);
            }

        [HttpGet("CAU/pdp")]
        public IActionResult pdp(string stepKey = null)
            {
            return RenderCommercialAuditWorkflow(string.IsNullOrWhiteSpace(stepKey) ? "pdp-entry" : stepKey);
            }

        [HttpGet("CAU/arpse")]
        public IActionResult arpse(string stepKey = null)
            {
            return RenderCommercialAuditWorkflow(string.IsNullOrWhiteSpace(stepKey) ? "arpse-header" : stepKey);
            }


        [HttpGet("CAU/om_creation")]
        public IActionResult om_creation(string stepKey = null)
            {
            return RenderCommercialAuditWorkflow(string.IsNullOrWhiteSpace(stepKey) ? "om-entry" : stepKey);
            }


        [HttpGet("CAU/om_reply")]
        public IActionResult om_reply(string stepKey = null)
            {
            return RenderCommercialAuditWorkflow(string.IsNullOrWhiteSpace(stepKey) ? "pdp-entry" : stepKey);
            }

        [HttpGet("CAU/monitoring_oms")]
        public IActionResult monitoring_oms(string stepKey = null)
            {
            return RenderCommercialAuditWorkflow(string.IsNullOrWhiteSpace(stepKey) ? "arpse-monitoring" : stepKey);
            }

        [HttpGet("CAU/reports")]
        public IActionResult reports(string stepKey = null)
            {
            return RenderCommercialAuditWorkflow(stepKey);
            }

        [HttpGet("CAU/LoadStep")]
        public IActionResult LoadStep(string stepKey, string hostPath = null)
            {
            return RenderCommercialAuditStepPartial(stepKey, hostPath);
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
