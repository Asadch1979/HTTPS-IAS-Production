using AIS.Exceptions;
using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System;

namespace AIS.Controllers
{
    [Authorize]
    [Route("api/ExecutiveDashboard/[action]")]
    [EnableRateLimiting("GeneralApiPolicy")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class ExecutiveDashboardController : Controller
    {
        private readonly DBConnection database;
        private readonly SessionHandler session;
        private readonly IPermissionService permissions;
        private readonly IPageIdResolver pageIds;
        private readonly ILogger<ExecutiveDashboardController> logger;

        public ExecutiveDashboardController(DBConnection database, SessionHandler session,
            IPermissionService permissions, IPageIdResolver pageIds, ILogger<ExecutiveDashboardController> logger)
        {
            this.database = database;
            this.session = session;
            this.permissions = permissions;
            this.pageIds = pageIds;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult GetCiaAuditYears([FromQuery] int? entityId = null,
            [FromQuery] string dashboardType = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, null, dashboardType, null, filters,
                (scope, f) => database.GetCiaAuditYears(scope), ciaOnly: true);

        [HttpGet]
        public IActionResult GetCiaExecKpis([FromQuery] int? auditPeriodId = null, [FromQuery] int? entityId = null,
            [FromQuery] string dashboardType = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, null, dashboardType, null, filters,
                (scope, f) => database.GetCiaExecKpis(scope, auditPeriodId, f), ciaOnly: true);

        [HttpGet]
        public IActionResult GetCiaIssueNature([FromQuery] int? auditPeriodId = null, [FromQuery] int? entityId = null,
            [FromQuery] string dashboardType = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, null, dashboardType, null, filters,
                (scope, f) => database.GetCiaIssueNature(scope, auditPeriodId, f), ciaOnly: true);

        [HttpGet]
        public IActionResult GetKpiLifecycle([FromQuery] int? entityId = null,
            [FromQuery] string dashboardType = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, null, dashboardType, null, filters,
                (scope, f) => database.GetExecutiveKpiLifecycle(scope, f), lifecycle: true);

        [HttpGet]
        public IActionResult GetKpiSummary([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null,
            [FromQuery] string dashboardType = null, [FromQuery] int? groupRootId = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, (scope, f) => database.GetExecutiveKpiSummary(scope, auditPeriod, f));

        [HttpGet]
        public IActionResult GetRiskSummary([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null,
            [FromQuery] string dashboardType = null, [FromQuery] int? groupRootId = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, (scope, f) => database.GetExecutiveRiskSummary(scope, auditPeriod, f));

        [HttpGet]
        public IActionResult GetAgingSummary([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null,
            [FromQuery] string dashboardType = null, [FromQuery] int? groupRootId = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, (scope, f) => database.GetExecutiveAgingSummary(scope, auditPeriod, f));

        [HttpGet]
        public IActionResult GetCycleSummary([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null,
            [FromQuery] string dashboardType = null, [FromQuery] int? groupRootId = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, (scope, f) => database.GetExecutiveCycleSummary(scope, auditPeriod, f));

        [HttpGet]
        public IActionResult GetSettlementTrend([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null,
            [FromQuery] string dashboardType = null, [FromQuery] int? groupRootId = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, (scope, f) => database.GetExecutiveSettlementTrend(scope, auditPeriod, f));

        [HttpGet]
        public IActionResult GetCriticalItems([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null, [FromQuery] int maxRows = 100,
            [FromQuery] string dashboardType = null, [FromQuery] int? groupRootId = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, (scope, f) => database.GetExecutiveCriticalItems(scope, auditPeriod, maxRows, f));

        private IActionResult ExecuteSummary<T>(int? entityId, string auditPeriod, string dashboardType, int? groupRootId, ExecutiveDashboardFilterRequest filters,
            Func<ExecutiveDashboardScope, ExecutiveDashboardFilterRequest, T> query, bool lifecycle = false, bool ciaOnly = false)
        {
            var user = session.GetUser();
            if (User.Identity?.IsAuthenticated != true || user == null || session.IsSessionRevoked(user))
            {
                LogDenial("SessionValidation", user, dashboardType);
                return Error(401, "unauthenticated", "An active authenticated IAS session is required.");
            }
            if (!ModelState.IsValid || entityId <= 0 || groupRootId <= 0 || dashboardType?.Length > 20 ||
                (auditPeriod != null && System.Text.Encoding.UTF8.GetByteCount(auditPeriod) > 4000) || auditPeriod?.IndexOf('\0') >= 0)
                return Error(400, "invalid_request", "Provide a positive entity ID and a valid audit period.");
            try
            {
                var scope = database.ResolveExecutiveDashboardScope(dashboardType, groupRootId);
                if (ciaOnly && scope.DashboardType != "CIA")
                    throw new ExecutiveDashboardAccessException("RoleMapping", "CIA command datasets require the verified CIA perspective.");
                if (lifecycle && scope.DashboardType != "TOP")
                    throw new ExecutiveDashboardAccessException("RoleMapping", "Lifecycle KPIs require the TOP perspective.");
                var pageId = pageIds.ResolvePageId(scope.PermissionPagePath);
                var superUser = user.UserRoleID == 1 && session.IsSuperUser();
                if (!superUser) permissions.EnsurePermissionsCached(user);
                if (pageId <= 0 || (superUser ? !permissions.HasViewPermission(user, pageId) :
                    !session.TryGetAllowedViewIds(out var allowedPages) || !allowedPages.Contains(pageId)))
                {
                    LogDenial("PagePermission", user, scope.DashboardType);
                    return Error(403, "forbidden", "The executive dashboard page is not authorized or mapped.");
                }
                if (superUser ? !permissions.HasApiPermissionForPath(user, Request.Method, Request.PathBase.Value, Request.Path.Value) :
                    !session.HasApiPermissionForPath(Request.Method, Request.PathBase.Value, Request.Path.Value))
                {
                    LogDenial("ApiPermission", user, scope.DashboardType);
                    return Error(403, "forbidden", "The executive API is not authorized or registered.");
                }
                filters ??= new ExecutiveDashboardFilterRequest();
                filters.EntityId = entityId;
                var dates = filters.Validate();
                if (lifecycle && dates == null) throw new ArgumentException("Both KPI dates are required.");
                if (filters.View != null)
                {
                    var definition = System.Linq.Enumerable.FirstOrDefault(ExecutiveDashboardViews.ForPerspective(scope.DashboardType), v => v.Id == filters.View);
                    var action = Convert.ToString(RouteData.Values["action"]);
                    if (definition == null || !System.Linq.Enumerable.Contains(definition.Datasets, action))
                        return Error(400, "invalid_request", "The dataset is not enabled for this dashboard view.");
                }
                // Validate the chosen dimensions and cascading hierarchy on the server.
                if (filters.DimensionId.HasValue || filters.ReportingOfficeId.HasValue)
                    database.GetExecutiveDashboardFilterOptions(scope, filters);
                scope = database.NarrowExecutiveDashboardScope(scope, entityId);
                return Ok(new { success = true, data = query(scope, filters), dateRangeApplied = lifecycle,
                    organizationalDimensionsApplied = true, scopeMode = scope.DashboardType == "RLM" ? "loginReportingLine" : "authorizedHierarchy" });
            }
            catch (UnauthenticatedException)
            {
                LogDenial("SessionValidation", user, dashboardType);
                return Error(401, "unauthenticated", "The IAS session is no longer valid.");
            }
            catch (UnauthorizedAccessException exception)
            {
                LogDenial((exception as ExecutiveDashboardAccessException)?.Reason ?? "RoleMapping", user, dashboardType);
                return Error(403, "forbidden", "A verified executive role and permitted organizational scope are required.");
            }
            catch (ArgumentException)
            {
                return Error(400, "invalid_request", "Provide valid dates and dashboard filters; maxRows must be between 1 and 500.");
            }
            catch (DatabaseUnavailableException exception)
            {
                logger.LogError(exception, "Executive dashboard database unavailable.");
                return Error(503, "database_unavailable", "The executive reporting database is temporarily unavailable.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Executive dashboard summary failed for user {UserId}.", user.ID);
                return Error(500, "server_error", "Unable to retrieve the executive dashboard summary.");
            }
        }

        private void LogDenial(string reason, SessionUser user, string perspective)
            => logger.LogWarning("Executive dashboard denied: {Reason}; user {UserId}, role {RoleId}, perspective {Perspective}, route {Route}.",
                reason, user?.ID, user?.UserRoleID, perspective, Request.Path.Value);

        private IActionResult Error(int statusCode, string error, string message)
            => StatusCode(statusCode, new { success = false, error, message });
    }
}
