using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public IActionResult Quality_Assurance_checking()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["EntitiesList"] = dBConnection.GetObservationEntitiesForPreConcluding();
            ViewData["AnnexList"] = dBConnection.GetAnnexuresForChecklistDetail();
            ViewData["ProcessList"] = dBConnection.GetAuditChecklist();
            ViewData["RiskList"] = dBConnection.GetRisks();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/Quality_Assurance_checking.cshtml");
                }
            }

        public IActionResult Draft_report_Checking()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["AnnexList"] = dBConnection.GetAnnexuresForChecklistDetail();
            ViewData["ProcessList"] = dBConnection.GetAuditChecklist();
            ViewData["EntitiesList"] = dBConnection.GetObservationEntities();
            ViewData["RiskList"] = dBConnection.GetRisks();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    return RedirectToAction("Index", "PageNotFound");
                else
                    return View("~/Views/FAD/Draft_report_Checking.cshtml");
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

        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
