using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using AIS.Services;

namespace AIS.Controllers
    {

    public class PostComplianceController : Controller
        {
        private readonly ILogger<PostComplianceController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        public PostComplianceController(ILogger<PostComplianceController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            }

        public IActionResult post_compliance()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["UserEntityName"] = sessionHandler.GetUserOrThrow().UserEntityName;
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult cau_post_compliance_to_branches()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["UserEntityName"] = sessionHandler.GetUserOrThrow().UserEntityName;
            ViewData["Userrelationship"] = dBConnection.GetrealtionshiptypeForCAU();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult cau_post_compliance_to_branches_reply()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["UserEntityName"] = sessionHandler.GetUserOrThrow().UserEntityName;
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult cau_post_compliance_to_branches_review()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["UserEntityName"] = sessionHandler.GetUserOrThrow().UserEntityName;
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult post_compliance_review()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["UserEntityName"] = sessionHandler.GetUserOrThrow().UserEntityName;
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult post_compliance_ho_monitoring()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["UserEntityName"] = sessionHandler.GetUserOrThrow().UserEntityName;
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult compliance_submitted_by_auditee()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult compliance_submitted_by_auditee_ref()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }
        public IActionResult compliance_for_settlement()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult change_status_new_Para()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["EntitiesList"] = dBConnection.GetCurrentParasEntitiesForStatusChange();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult change_status_new_Para_reviewer()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult change_status_new_Para_authorize()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult change_para_status()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["Userrelationship"] = dBConnection.Getrealtionshiptype(ViewData["PageId"] as int? ?? 0);

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult change_para_status_authorize()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        public IActionResult monitoring_of_para_settlement()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["EntitiesList"] = dBConnection.GetSettledParasEntitiesForMonitoringFAD();
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");
            else
                {
                if (!this.UserHasPagePermissionForCurrentAction(sessionHandler)) //MIGRATION_PERMISSION_CHECK (Controller)
                    {
                    return RedirectToAction("Index", "PageNotFound");
                    }
                else
                    return View();
                }
            }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
