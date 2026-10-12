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
        public IActionResult GetKpiSummary([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null)
            => ExecuteSummary(entityId, auditPeriod, scope => database.GetExecutiveKpiSummary(scope, auditPeriod));

        [HttpGet]
        public IActionResult GetRiskSummary([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null)
            => ExecuteSummary(entityId, auditPeriod, scope => database.GetExecutiveRiskSummary(scope, auditPeriod));

        [HttpGet]
        public IActionResult GetAgingSummary([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null)
            => ExecuteSummary(entityId, auditPeriod, scope => database.GetExecutiveAgingSummary(scope, auditPeriod));

        [HttpGet]
        public IActionResult GetCycleSummary([FromQuery] int? entityId = null, [FromQuery] string auditPeriod = null)
            => ExecuteSummary(entityId, auditPeriod, scope => database.GetExecutiveCycleSummary(scope, auditPeriod));

        private IActionResult ExecuteSummary<T>(int? entityId, string auditPeriod, Func<ExecutiveDashboardScope, T> query)
        {
            var user = session.GetUser();
            if (User.Identity?.IsAuthenticated != true || user == null || session.IsSessionRevoked(user))
                return Error(401, "unauthenticated", "An active authenticated IAS session is required.");
            if (!ModelState.IsValid || entityId <= 0 || (auditPeriod?.Length > 4000) || auditPeriod?.IndexOf('\0') >= 0)
                return Error(400, "invalid_request", "Provide a positive entity ID and a valid audit period.");
            try
            {
                // This additional role gate is mandatory even when the common
                // permission infrastructure applies its Super User bypass.
                var scope = database.ResolveExecutiveDashboardScope();
                var pageId = pageIds.ResolvePageId(scope.PermissionPagePath);
                if (pageId <= 0 || !permissions.HasViewPermission(user, pageId))
                    return Error(403, "forbidden", "The executive dashboard page is not authorized or mapped.");
                if (scope.DashboardType == "CIA")
                    throw new NotSupportedException("PKG_EXEC_DASHBOARD does not support CIA reporting.");
                if (entityId.HasValue && scope.DashboardType != "GROUP")
                    return Error(400, "unsupported_filter", "PKG_EXEC_DASHBOARD does not support entity filters for bank-wide dashboards.");
                scope = database.NarrowExecutiveDashboardScope(scope, entityId);
                return Ok(new { success = true, data = query(scope) });
            }
            catch (UnauthenticatedException)
            {
                return Error(401, "unauthenticated", "The IAS session is no longer valid.");
            }
            catch (UnauthorizedAccessException exception)
            {
                logger.LogWarning(exception, "Executive dashboard access denied for user {UserId}, role {RoleId}.", user.ID, user.UserRoleID);
                return Error(403, "forbidden", "A verified executive role and permitted organizational scope are required.");
            }
            catch (NotSupportedException exception)
            {
                logger.LogWarning(exception, "Executive dashboard package compatibility gap for role {RoleId}.", user.UserRoleID);
                return Error(501, "unsupported_dashboard", "PKG_EXEC_DASHBOARD supports TOP, GROUP and CCO; CIA is not supported.");
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

        private IActionResult Error(int statusCode, string error, string message)
            => StatusCode(statusCode, new { success = false, error, message });
    }
}
