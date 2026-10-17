using AIS.Exceptions;
using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration configuration;

        public ExecutiveDashboardController(DBConnection database, SessionHandler session,
            IPermissionService permissions, IPageIdResolver pageIds, ILogger<ExecutiveDashboardController> logger,
            IConfiguration configuration)
        {
            this.database = database;
            this.session = session;
            this.permissions = permissions;
            this.pageIds = pageIds;
            this.logger = logger;
            this.configuration = configuration;
        }

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


        // CIA Command Center is not the TOP lifecycle perspective.
        // These are read-only, CIA/Super User authorized, audit-period-ID queries.
        [HttpGet]
        public IActionResult GetCiaCommandKpis([FromQuery] int auditPeriodId,
            [FromQuery] int? entityId = null, [FromQuery] string dashboardType = null,
            [FromQuery] ExecutiveDashboardFilterRequest filters = null)
        {
            if (!configuration.GetValue<bool>("RoleDashboards:CiaCommandCenterEnabled"))
                return Error(503, "not_deployed", "CIA Command Center Oracle cursors are not yet enabled.");
            if (auditPeriodId <= 0) return Error(400, "invalid_request", "Select a valid IAS audit period.");
            return ExecuteSummary(entityId, null, dashboardType, null, filters, (scope, f) =>
            {
                if (scope.DashboardType != "CIA" || f.View != "command")
                    throw new ExecutiveDashboardAccessException("RoleMapping", "CIA Command Center perspective is required.");
                return database.GetCiaCommandKpis(scope, f, auditPeriodId);
            });
        }

        [HttpGet]
        public IActionResult GetCiaIssueNature([FromQuery] int auditPeriodId,
            [FromQuery] int? entityId = null, [FromQuery] string dashboardType = null,
            [FromQuery] ExecutiveDashboardFilterRequest filters = null)
        {
            if (!configuration.GetValue<bool>("RoleDashboards:CiaCommandCenterEnabled"))
                return Error(503, "not_deployed", "CIA Command Center Oracle cursors are not yet enabled.");
            if (auditPeriodId <= 0) return Error(400, "invalid_request", "Select a valid IAS audit period.");
            return ExecuteSummary(entityId, null, dashboardType, null, filters, (scope, f) =>
            {
                if (scope.DashboardType != "CIA" || f.View != "command")
                    throw new ExecutiveDashboardAccessException("RoleMapping", "CIA Command Center perspective is required.");
                return database.GetCiaIssueNature(scope, f, auditPeriodId);
            });
        }

        // The new role datasets use separate, typed Oracle cursors.
        // Their API permission grants must be registered by IAS before rollout.
        [HttpGet]
        public IActionResult GetRoleEntityHeatmap([FromQuery] int? entityId = null,
            [FromQuery] string auditPeriod = null, [FromQuery] string dashboardType = null,
            [FromQuery] int? groupRootId = null, [FromQuery] int maxRows = 250,
            [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteRoleSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, "regional",
                (scope, f) => database.GetRoleEntityHeatmap(scope, auditPeriod, f, maxRows));

        [HttpGet]
        public IActionResult GetRoleUnitComparison([FromQuery] int? entityId = null,
            [FromQuery] string auditPeriod = null, [FromQuery] string dashboardType = null,
            [FromQuery] int? groupRootId = null, [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteRoleSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, "management",
                (scope, f) => database.GetRoleUnitComparison(scope, auditPeriod));

        [HttpGet]
        public IActionResult GetRoleFollowups([FromQuery] int? entityId = null,
            [FromQuery] string auditPeriod = null, [FromQuery] string dashboardType = null,
            [FromQuery] int? groupRootId = null, [FromQuery] int maxRows = 100,
            [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => ExecuteRoleSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, null,
                (scope, f) => database.GetRoleFollowups(scope, auditPeriod, maxRows));

        private IActionResult ExecuteRoleSummary<T>(int? entityId, string auditPeriod, string dashboardType,
            int? groupRootId, ExecutiveDashboardFilterRequest filters, string requiredPerspective,
            Func<ExecutiveDashboardScope, ExecutiveDashboardFilterRequest, T> read)
        {
            if (!configuration.GetValue<bool>("RoleDashboards:OracleReadEnabled"))
                return StatusCode(503, new { success = false, error = "not_deployed",
                    message = "Role reporting awaits verified Oracle deployment." });
            return ExecuteSummary(entityId, auditPeriod, dashboardType, groupRootId, filters, (scope, f) =>
            {
                var root = database.GetExecutiveDashboardRoot(scope);
                var managementRoles = configuration.GetSection("RoleDashboards:ManagementRlmRoleIds")
                    .Get<int[]>() ?? Array.Empty<int>();
                var isManagement = scope.DashboardType == "GROUP" && root?.EntityTypeId == 18 ||
                    scope.DashboardType == "RLM" &&
                    (managementRoles.Contains(scope.RoleId) ||
                     (scope.RoleId == 1 && scope.GroupRootId.HasValue && session.IsSuperUser())) &&
                    (root?.EntityTypeId == 3 || root?.EntityTypeId == 18);
                var isRegional = scope.DashboardType == "RLM" &&
                    (root?.EntityTypeId == 5 || root?.EntityTypeId == 21);
                if ((!isManagement && !isRegional) ||
                    (requiredPerspective == "management" && !isManagement) ||
                    (requiredPerspective == "regional" && !isRegional) ||
                    (f.View == "management" && !isManagement) ||
                    (f.View == "regional" && !isRegional))
                    throw new ExecutiveDashboardAccessException("RoleMapping", "Role dashboard scope is not authorized.");
                if (f.DimensionId.HasValue && requiredPerspective != "regional")
                    throw new ArgumentException("This aggregate cursor does not accept a dimension filter.");
                // The Oracle role cursors do not accept a separate office ID.
                // Narrow the validated root so office selection is never ignored.
                if (!f.EntityId.HasValue && f.ReportingOfficeId.HasValue)
                    scope = database.NarrowExecutiveDashboardScope(scope, f.ReportingOfficeId);
                return read(scope, f);
            });
        }

        private IActionResult ExecuteSummary<T>(int? entityId, string auditPeriod, string dashboardType, int? groupRootId, ExecutiveDashboardFilterRequest filters,
            Func<ExecutiveDashboardScope, ExecutiveDashboardFilterRequest, T> query, bool lifecycle = false)
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
                if (scope.DashboardType == "RLM" && (filters.DimensionId.HasValue || filters.SelectBy != "entityType"))
                    return Error(400, "invalid_request", "Reporting Line Management supports Reporting Office and Entity only.");
                if (filters.View == "regional")
                {
                    var root = database.GetExecutiveDashboardRoot(scope);
                    if (scope.DashboardType != "RLM" || (root?.EntityTypeId != 21 && root?.EntityTypeId != 5))
                        throw new ExecutiveDashboardAccessException("RoleMapping", "The regional view requires a verified GM or region posting.");
                }
                var dates = filters.Validate();
                if (lifecycle && dates == null) throw new ArgumentException("Both KPI dates are required.");
                if (filters.View != null)
                {
                    var definition = System.Linq.Enumerable.FirstOrDefault(ExecutiveDashboardViews.ForPerspective(scope.DashboardType), v => v.Id == filters.View);
                    var action = Convert.ToString(RouteData.Values["action"]);
                    if (definition == null || (!System.Linq.Enumerable.Contains(definition.Datasets, action) &&
                        !System.Linq.Enumerable.Contains(definition.RoleDatasets, action)))
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
