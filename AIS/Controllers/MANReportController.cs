using AIS.Models.ManagementReport;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace AIS.Controllers
    {
    public class MANReportController : Controller
        {
        private readonly ILogger<MANReportController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly TopMenus _topMenus;
        private readonly IPermissionService _permissionService;

        public MANReportController(
            ILogger<MANReportController> logger,
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

        public IActionResult Home()
            {
            var redirect = EnsureAuthorized();
            if (redirect != null)
                {
                return redirect;
                }

            if (!_sessionHandler.TryGetActiveEngagementId(out var engId))
                {
                return View(new ManagementReportHomeViewModel
                    {
                    HasActiveEngagement = false
                    });
                }

            var engagement = _dbConnection
                .GetReportEntities()
                .FirstOrDefault(option => option.EngagementId == engId);

            var model = new ManagementReportHomeViewModel
                {
                HasActiveEngagement = true,
                EntityName = engagement?.EntityName ?? string.Empty,
                AuditPeriod = engagement?.AuditPeriod ?? string.Empty
                };

            return View(model);
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
        }
    }
