using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace AIS.Controllers
{
    [Authorize]
    [Route("ExecutiveOverview")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class ExecutiveOverviewController : Controller
    {
        private readonly DBConnection database;
        private readonly SessionHandler session;
        private readonly IPermissionService permissions;
        private readonly IPageIdResolver pageIds;
        private readonly TopMenus menus;
        private readonly ILogger<ExecutiveOverviewController> logger;

        public ExecutiveOverviewController(DBConnection database, SessionHandler session,
            IPermissionService permissions, IPageIdResolver pageIds, TopMenus menus,
            ILogger<ExecutiveOverviewController> logger)
        {
            this.database = database;
            this.session = session;
            this.permissions = permissions;
            this.pageIds = pageIds;
            this.menus = menus;
            this.logger = logger;
        }

        [HttpGet("TOP")]
        public IActionResult TOP(int? entityId = null) => Dashboard("TOP", entityId);
        [HttpGet("GROUP")]
        public IActionResult GROUP(int? entityId = null) => Dashboard("GROUP", entityId);
        [HttpGet("CCO")]
        public IActionResult CCO(int? entityId = null) => Dashboard("CCO", entityId);
        [HttpGet("CIA")]
        public IActionResult CIA(int? entityId = null) => Dashboard("CIA", entityId);

        private IActionResult Dashboard(string requestedType, int? entityId)
        {
            var user = session.GetUser();
            if (User.Identity?.IsAuthenticated != true || user == null || session.IsSessionRevoked(user))
                return Unauthorized();
            if (!ModelState.IsValid || entityId <= 0) return BadRequest();
            try
            {
                var scope = database.ResolveExecutiveDashboardScope();
                permissions.EnsurePermissionsCached(user);
                var pageId = pageIds.ResolvePageId(scope.PermissionPagePath);
                if (requestedType != scope.DashboardType || pageId <= 0 ||
                    !session.TryGetAllowedViewIds(out var pages) || !pages.Contains(pageId))
                    return StatusCode(403);
                database.NarrowExecutiveDashboardScope(scope, entityId);
                ViewData["TopMenu"] = menus.GetTopMenus();
                ViewData["TopMenuPages"] = menus.GetTopMenusPages();
                return View("Index", new ExecutiveDashboardPageModel
                {
                    DashboardType = scope.DashboardType,
                    ScopeLabel = scope.DashboardType == "GROUP" ? user.UserEntityName + " and descendants" : "Authorized bank-wide perspective",
                    SelectedEntityId = entityId,
                    Entities = database.GetExecutiveDashboardEntities(scope).OrderBy(n => n.EntityName).ToList(),
                    AuditPeriods = database.GetAuditPeriods(),
                    CanViewOrganization = permissions.HasViewPermission(user, pageIds.ResolvePageId("/Dashboard/entity_wise_obs"))
                });
            }
            catch (UnauthenticatedException) { return Unauthorized(); }
            catch (UnauthorizedAccessException) { return StatusCode(403); }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to open executive dashboard for user {UserId}.", user.ID);
                return StatusCode(503, "Executive Dashboard is temporarily unavailable. Please refresh to retry.");
            }
        }
    }
}
