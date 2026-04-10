using AIS.Models;
using AIS.Models.CAU;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using AIS.Services;
namespace AIS.Controllers
    {

    public class CAUController : Controller
        {
        private readonly ILogger<CAUController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        public CAUController(ILogger<CAUController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            }

        private bool PrepareCauView()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                {
                return false;
                }

            return this.UserHasPagePermissionForCurrentAction(sessionHandler);
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
            ViewData["ArpseYearList"] = dBConnection.GetParaPrintingYearsForCAU();
            ViewData["WorkflowStages"] = new List<CommercialAuditWorkflowStage>
                {
                new CommercialAuditWorkflowStage { Key = "workflow", Title = "Workflow", Description = "Choose a stage and continue the Commercial Audit workflow." },
                new CommercialAuditWorkflowStage { Key = "om", Title = "Stage 1: OM", Description = "Create and manage Office Memorandums." },
                new CommercialAuditWorkflowStage { Key = "pdp", Title = "Stage 2: PDP", Description = "Create PDPs and link one PDP with multiple OMs." },
                new CommercialAuditWorkflowStage { Key = "arpse", Title = "Stage 3: ARPSE", Description = "Manage ARPSE headers with DAC and PAC follow-up entries." }
                };
            }

        private IActionResult RenderCommercialAuditView(string viewName, string stageKey, string title)
            {
            if (!PrepareCauView())
                {
                return RedirectForUnauthorizedOrMissingPermission();
                }

            PrepareCommercialAuditWorkflowLookups();
            ViewData["Title"] = title;
            ViewData["CommercialAuditStage"] = stageKey;
            return View(viewName);
            }

        [HttpGet("CAU/workflow")]
        public IActionResult workflow()
            {
            return RenderCommercialAuditView("../CAU/workflow", "workflow", "Commercial Audit Workflow");
            }

        [HttpGet("CAU/om")]
        public IActionResult om()
            {
            return RenderCommercialAuditView("../CAU/om", "om", "Commercial Audit OM");
            }

        [HttpGet("CAU/pdp")]
        public IActionResult pdp()
            {
            return RenderCommercialAuditView("../CAU/pdp", "pdp", "Commercial Audit PDP");
            }

        [HttpGet("CAU/arpse")]
        public IActionResult arpse()
            {
            return RenderCommercialAuditView("../CAU/arpse", "arpse", "Commercial Audit ARPSE");
            }


        [HttpGet("CAU/om_creation")]
        public IActionResult om_creation()
            {
            return RenderCommercialAuditView("../CAU/om", "om", "Commercial Audit OM");
            }


        [HttpGet("CAU/om_reply")]
        public IActionResult om_reply()
            {
            return RenderCommercialAuditView("../CAU/pdp", "pdp", "Commercial Audit PDP");
            }

        [HttpGet("CAU/monitoring_oms")]
        public IActionResult monitoring_oms()
            {
            return RenderCommercialAuditView("../CAU/arpse", "arpse", "Commercial Audit ARPSE");
            }

        [HttpGet("CAU/reports")]
        public IActionResult reports()
            {
            return RenderCommercialAuditView("../CAU/workflow", "workflow", "Commercial Audit Workflow");
            }


        [HttpGet("CAU/OM/om_assignment")]
        public IActionResult om_assignment()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["DivisionList"] = dBConnection.GetDivisions(false);
            ViewData["InsYearList"] = dBConnection.GetInsYearsForCAU();
            ViewData["ParaPrintingYearList"] = dBConnection.GetParaPrintingYearsForCAU();
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View("../CAU/OM/om_assignment");
                }
            }

        [HttpGet("CAU/OM/om_response")]
        public IActionResult om_response()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["DepartmentList"] = dBConnection.GetDepartments(0, false);


            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View("../CAU/OM/om_response");
                }
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
