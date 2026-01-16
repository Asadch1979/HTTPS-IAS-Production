using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AIS.Services;
namespace AIS.Controllers
    {

    public class AMSController : Controller
        {
        private readonly ILogger<AMSController> _logger;
        private readonly TopMenus tm;
        private readonly SessionHandler sessionHandler;
        private readonly IPermissionService _permissionService;
        private readonly DBConnection dBConnection;
        public AMSController(ILogger<AMSController> logger, SessionHandler _sessionHandler, DBConnection _dbCon, TopMenus _tpMenu, IPermissionService permissionService)
            {
            _logger = logger;
            sessionHandler = _sessionHandler;
            dBConnection = _dbCon;
            tm = _tpMenu;
            _permissionService = permissionService;
            }
        [HttpGet("IAMS/paras")]
        public IActionResult paras()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ProcessList"] = dBConnection.GetRiskProcessDefinition();
            ViewData["EntitiesList"] = dBConnection.GetAuditeeEntitiesForOutstandingParas(0);
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
                    return View("../IAMS/paras");
                }
            }

        [HttpGet("IAMS/old_para")]
        public IActionResult old_para()
            {
            ViewData["TopMenu"] = tm.GetTopMenus();
            ViewData["TopMenuPages"] = tm.GetTopMenusPages();
            ViewData["ProcessList"] = dBConnection.GetRiskProcessDefinition();
            ViewData["EntitiesList"] = dBConnection.GetAuditeeEntitiesForOldParas(0);
            ViewData["AuditYearList"] = dBConnection.GetOldParasAuditYear();
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
                    return View("../IAMS/old_para");
                }
            }
        }
    }
