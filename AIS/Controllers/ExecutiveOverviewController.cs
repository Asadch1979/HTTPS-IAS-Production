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
        public IActionResult TOP(int? entityId = null, int? groupRootId = null, string view = "glance", [FromQuery] ExecutiveDashboardFilterRequest filters = null) => Dashboard("TOP", entityId, groupRootId, view, filters);
        [HttpGet("GROUP")]
        public IActionResult GROUP(int? entityId = null, int? groupRootId = null, string view = "glance", [FromQuery] ExecutiveDashboardFilterRequest filters = null) => Dashboard("GROUP", entityId, groupRootId, view, filters);
        [HttpGet("CCO")]
        public IActionResult CCO(int? entityId = null, int? groupRootId = null, string view = "glance", [FromQuery] ExecutiveDashboardFilterRequest filters = null) => Dashboard("CCO", entityId, groupRootId, view, filters);
        [HttpGet("CIA")]
        public IActionResult CIA(int? entityId = null, int? groupRootId = null, string view = "glance", [FromQuery] ExecutiveDashboardFilterRequest filters = null) => Dashboard("CIA", entityId, groupRootId, view, filters);
        [HttpGet("RLM")]
        public IActionResult RLM(int? entityId = null, string view = "glance", [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => Dashboard("RLM", entityId, null, view, filters);

        private IActionResult Dashboard(string requestedType, int? entityId, int? groupRootId, string view, ExecutiveDashboardFilterRequest filters)
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
                filters ??= new ExecutiveDashboardFilterRequest();
                if (((requestedType == "TOP" && view == "glance") || view == "reports") &&
                    string.IsNullOrEmpty(filters.StartDate) && string.IsNullOrEmpty(filters.EndDate))
                {
                    // The reference Annual Field Audit report compares four calendar years.
                    // Only use that default when Report A is the initial authorized report;
                    // never replace a reporting period explicitly selected by the user.
                    var requestedReport = Request.Query["report"].ToString();
                    var annualDefault = view == "reports" &&
                        (requestedType == "TOP" || requestedType == "CIA") &&
                        (string.IsNullOrEmpty(requestedReport) ||
                         string.Equals(requestedReport, "A", StringComparison.OrdinalIgnoreCase));
                    // Default complete days only: today's partial date can produce
                    // inconsistent snapshot/results during debugging and time zones.
                    var defaultEndDate = DateTime.Today.AddDays(-1);
                    filters.StartDate = new DateTime(defaultEndDate.Year - (annualDefault ? 3 : 0), 1, 1).ToString("yyyy-MM-dd");
                    filters.EndDate = defaultEndDate.ToString("yyyy-MM-dd");
                }
                filters.EntityId = entityId;
                filters.View = view;
                filters.Validate();
                if (requestedType == "RLM" && (filters.DimensionId.HasValue || filters.SelectBy != "entityType"))
                    return BadRequest("Reporting Line Management supports Reporting Office and Entity only.");
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
                var views = ExecutiveDashboardViews.ForPerspective(scope.DashboardType);
                var root = database.GetExecutiveDashboardRoot(scope);
                var regional = scope.DashboardType == "RLM" && (root?.EntityTypeId == 21 || root?.EntityTypeId == 5);
                if (!regional) views.RemoveAll(v => v.Id == "regional");
                if (regional && view == "glance") view = "regional";
                filters.View = view;
                var selectedView = views.FirstOrDefault(v => v.Id == view);
                if (selectedView == null) return BadRequest("Unknown dashboard view.");
                var needsGroup = scope.DashboardType == "GROUP" && !scope.GroupRootId.HasValue;
                // The selector page may open without a root; reporting queries may not.
                if (!needsGroup || entityId.HasValue) database.NarrowExecutiveDashboardScope(scope, entityId);
                if (needsGroup && (filters.DimensionId.HasValue || filters.ReportingOfficeId.HasValue))
                    throw new ExecutiveDashboardAccessException("EntityScope", "Select a Group root before selecting organizational filters.");
                var options = needsGroup ? new ExecutiveDashboardFilterOptions() : database.GetExecutiveDashboardFilterOptions(scope, filters);
                var groups = superUser && scope.DashboardType == "GROUP" ?
                    database.GetExecutiveDashboardGroups(scope).OrderBy(n => n.EntityName).ToList() : new System.Collections.Generic.List<OrganizationNode>();
                ViewData["TopMenu"] = menus.GetTopMenus();
                ViewData["TopMenuPages"] = menus.GetTopMenusPages();
                return View("Index", new ExecutiveDashboardPageModel
                {
                    Views = views,
                    SelectedView = selectedView,
                    DashboardType = scope.DashboardType,
                    ScopeLabel = needsGroup ? "Select a Group Head entity to load its authorized hierarchy" :
                        scope.DashboardType == "GROUP" ? (superUser ? groups.First(n => n.EntityId == scope.GroupRootId).EntityName : user.UserEntityName) + " and descendants" :
                        scope.DashboardType == "RLM" ? user.UserEntityName + " and all reporting descendants" : "Authorized bank-wide perspective",
                    SelectedEntityId = entityId,
                    IsSuperUser = superUser,
                    GroupRootId = scope.GroupRootId,
                    Groups = groups,
                    Filters = filters,
                    FilterOptions = options,
                    ComparisonEntities = view == "regional" ? database.GetExecutiveComparisonEntities(scope, filters, options, branches: true) : new(),
                    CanViewOrganization = permissions.HasViewPermission(user, pageIds.ResolvePageId("/Dashboard/entity_wise_obs"))
                });
            }
            catch (UnauthenticatedException) { LogDenial("SessionValidation", user, requestedType); return Unauthorized(); }
            catch (UnauthorizedAccessException exception)
            {
                LogDenial((exception as ExecutiveDashboardAccessException)?.Reason ?? "RoleMapping", user, requestedType);
                return StatusCode(403);
            }
            catch (ArgumentException exception) { return BadRequest(exception.Message); }
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
