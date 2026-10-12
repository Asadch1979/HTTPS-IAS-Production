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
        public IActionResult TOP(int? entityId = null, int? groupRootId = null) => Dashboard("TOP", entityId, groupRootId);
        [HttpGet("GROUP")]
        public IActionResult GROUP(int? entityId = null, int? groupRootId = null) => Dashboard("GROUP", entityId, groupRootId);
        [HttpGet("CCO")]
        public IActionResult CCO(int? entityId = null, int? groupRootId = null) => Dashboard("CCO", entityId, groupRootId);
        [HttpGet("CIA")]
        public IActionResult CIA(int? entityId = null, int? groupRootId = null) => Dashboard("CIA", entityId, groupRootId);

        private IActionResult Dashboard(string requestedType, int? entityId, int? groupRootId)
        {
            var user = session.GetUser();
            if (User.Identity?.IsAuthenticated != true || user == null || session.IsSessionRevoked(user))
            {
                LogDenial("SessionValidation", user, requestedType);
                return Unauthorized();
            }
            if (!ModelState.IsValid || entityId <= 0 || groupRootId <= 0) return BadRequest();
            try
            {
                var superUser = user.UserRoleID == 1 && session.IsSuperUser();
                var scope = database.ResolveExecutiveDashboardScope(requestedType, groupRootId, allowGroupSelection: superUser);
                if (!superUser) permissions.EnsurePermissionsCached(user);
                var pageId = pageIds.ResolvePageId(scope.PermissionPagePath);
                if (pageId <= 0 || (superUser ? !permissions.HasViewPermission(user, pageId) :
                    !session.TryGetAllowedViewIds(out var pages) || !pages.Contains(pageId)))
                {
                    LogDenial("PagePermission", user, requestedType);
                    return StatusCode(403);
                }
                var needsGroup = scope.DashboardType == "GROUP" && !scope.GroupRootId.HasValue;
                // The selector page may open without a root; reporting queries may not.
                if (!needsGroup || entityId.HasValue) database.NarrowExecutiveDashboardScope(scope, entityId);
                var groups = superUser && scope.DashboardType == "GROUP" ?
                    database.GetExecutiveDashboardGroups(scope).OrderBy(n => n.EntityName).ToList() : new System.Collections.Generic.List<OrganizationNode>();
                ViewData["TopMenu"] = menus.GetTopMenus();
                ViewData["TopMenuPages"] = menus.GetTopMenusPages();
                return View("Index", new ExecutiveDashboardPageModel
                {
                    DashboardType = scope.DashboardType,
                    ScopeLabel = needsGroup ? "Select a Group Head entity to load its authorized hierarchy" :
                        scope.DashboardType == "GROUP" ? (superUser ? groups.First(n => n.EntityId == scope.GroupRootId).EntityName : user.UserEntityName) + " and descendants" : "Authorized bank-wide perspective",
                    SelectedEntityId = entityId,
                    IsSuperUser = superUser,
                    GroupRootId = scope.GroupRootId,
                    Groups = groups,
                    Entities = needsGroup ? new System.Collections.Generic.List<OrganizationNode>() : database.GetExecutiveDashboardEntities(scope).OrderBy(n => n.EntityName).ToList(),
                    AuditPeriods = database.GetAuditPeriods(),
                    CanViewOrganization = permissions.HasViewPermission(user, pageIds.ResolvePageId("/Dashboard/entity_wise_obs"))
                });
            }
            catch (UnauthenticatedException) { LogDenial("SessionValidation", user, requestedType); return Unauthorized(); }
            catch (UnauthorizedAccessException exception)
            {
                LogDenial((exception as ExecutiveDashboardAccessException)?.Reason ?? "RoleMapping", user, requestedType);
                return StatusCode(403);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to open executive dashboard for user {UserId}.", user.ID);
                return StatusCode(503, "Executive Dashboard is temporarily unavailable. Please refresh to retry.");
            }
        }

        private void LogDenial(string reason, SessionUser user, string perspective)
            => logger.LogWarning("Executive dashboard denied: {Reason}; user {UserId}, role {RoleId}, perspective {Perspective}, route {Route}.",
                reason, user?.ID, user?.UserRoleID, perspective, Request.Path.Value);
    }
}
