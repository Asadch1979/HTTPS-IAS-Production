using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AIS.Services;

namespace AIS.Controllers
    {

    public class FADController : Controller
        {
        private readonly ILogger<FADController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly AIS.Security.Cryptography.SecurityTokenService _tokenService;
        public FADController(
            ILogger<FADController> logger,
            SessionHandler _sessionHandler,
            DBConnection _dbCon,
            TopMenus _tpMenu,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration, IPermissionService permissionService, AIS.Security.Cryptography.SecurityTokenService tokenService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _tokenService = tokenService;
            }

        private DBConnection CreateDbConnection()
            {
            if (_httpContextAccessor?.HttpContext == null)
                throw new InvalidOperationException("HTTP context accessor is not available for database operations.");
            if (_configuration == null)
                throw new InvalidOperationException("Configuration dependency is not available for database operations.");

            return DBConnection.CreateFromHttpContext(_httpContextAccessor, _configuration, sessionHandler, _tokenService);
            }


        public IActionResult observation_review()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            ViewData["statusList"] = dBConnection.GetObservationReversalStatus();

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
                    return View("~/Views/FAD/FAD_TASK/observation_review.cshtml");
                }
            }
        public IActionResult review_gist_recommendation()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ZonesList"] = dBConnection.GetZonesoldparamointoring();
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
                    return View("~/Views/FAD/review_gist_recommendation.cshtml");
                }
            }
        public IActionResult Para_shifting()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            ViewData["ZonesList"] = dBConnection.GetZonesoldparamointoring();

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
                    return View("~/Views/FAD/FAD_TASK/para_shifting.cshtml");
                }
            }

        public IActionResult Edit()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/Email/Edit.cshtml");
                }
            }

        public IActionResult Send()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/Email/Send.cshtml");
                }
            }
        public IActionResult risk_register()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/risk_register.cshtml");
                }
            }

        public IActionResult AllocateEntityToAuditor()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/AllocateEntityToAuditor.cshtml");
                }
            }

        public IActionResult ReferenceUpdateList()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/ReferenceUpdateList.cshtml");
                }
            }

        public IActionResult ReferenceEntitySummary()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/ReferenceEntitySummary.cshtml");
                }
            }

        public IActionResult ReferenceUpdateEdit(int comId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/ReferenceUpdateEdit.cshtml", comId);
                }
            }

        public IActionResult ReferenceUpdateLog(int comId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/ReferenceUpdateLog.cshtml", comId);
                }
            }

        public IActionResult ReferenceDisplay(int comId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/ReferenceDisplay.cshtml", comId);
                }
            }

        public IActionResult Fad_Desk_rpt()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/FAD_TASK/Fad_Desk_rpt.cshtml");
                }
            }

        public IActionResult ViewParaReferences(int comId)
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/ViewParaReferences.cshtml", comId);
                }
            }

        public IActionResult ChangeParaStatus()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/ChangeParaStatus.cshtml");
                }
            }

        public IActionResult AuthorizeParaStatus()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/AuthorizeParaStatus.cshtml");
                }
            }

        public IActionResult Upload_Circular_Document()
            {
            var db = CreateDbConnection();
            var circulars = db.GetAuditChecklistAnnexureCirculars();
            ViewBag.Circulars = circulars
                .Select(c => new SelectListItem { Value = c.ID.ToString(), Text = c.InstructionsTitle })
                .ToList();
            return View();
            }

        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
