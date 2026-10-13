using AIS.Exceptions;
using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;

namespace AIS.Controllers
{
    [Authorize]
    [Route("api/ExecutiveReports/[action]")]
    [EnableRateLimiting("GeneralApiPolicy")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class ExecutiveReportsController : Controller
    {
        private readonly DBConnection database;
        private readonly SessionHandler session;
        private readonly IPermissionService permissions;
        private readonly IPageIdResolver pageIds;
        private readonly IConfiguration configuration;
        private readonly ILogger<ExecutiveReportsController> logger;

        public ExecutiveReportsController(DBConnection database, SessionHandler session, IPermissionService permissions,
            IPageIdResolver pageIds, IConfiguration configuration, ILogger<ExecutiveReportsController> logger)
        {
            this.database = database; this.session = session; this.permissions = permissions;
            this.pageIds = pageIds; this.configuration = configuration; this.logger = logger;
        }

        [HttpGet]
        public IActionResult GetReportA([FromQuery] ExecutiveDashboardFilterRequest filters, string dashboardType = null, int? groupRootId = null)
            => Execute("A", filters, dashboardType, groupRootId, database.GetExecutiveReportA);
        [HttpGet]
        public IActionResult GetReportB([FromQuery] ExecutiveDashboardFilterRequest filters, string dashboardType = null, int? groupRootId = null)
            => Execute("B", filters, dashboardType, groupRootId, database.GetExecutiveReportB);
        [HttpGet]
        public IActionResult GetReportC([FromQuery] ExecutiveDashboardFilterRequest filters, string dashboardType = null, int? groupRootId = null)
            => Execute("C", filters, dashboardType, groupRootId, database.GetExecutiveReportC);
        [HttpGet]
        public IActionResult GetReportD([FromQuery] ExecutiveDashboardFilterRequest filters, string dashboardType = null, int? groupRootId = null)
            => Execute("D", filters, dashboardType, groupRootId, database.GetExecutiveReportD);
        [HttpGet]
        public IActionResult GetReportE([FromQuery] ExecutiveDashboardFilterRequest filters, string dashboardType = null, int? groupRootId = null)
            => Execute("E", filters, dashboardType, groupRootId, database.GetExecutiveReportE);

        private IActionResult Execute(string reportId, ExecutiveDashboardFilterRequest filters, string dashboardType, int? groupRootId,
            Func<ExecutiveDashboardScope, ExecutiveDashboardFilterRequest, ExecutiveReportResult> query)
        {
            var user = session.GetUser();
            if (User.Identity?.IsAuthenticated != true || user == null || session.IsSessionRevoked(user))
                return Error(401, "unauthenticated", "An active authenticated IAS session is required.");
            // No report-package/directory queries run while these flags are absent/false.
            if (!configuration.GetValue<bool>("ExecutiveReports:Enabled") || !configuration.GetValue<bool>("ExecutiveReports:HistoryVerified"))
                return Error(503, "reports_not_deployed", "Executive reports are disabled pending SQL deployment and historical verification.");
            if (!ModelState.IsValid || groupRootId <= 0 || dashboardType?.Length > 20 || filters == null)
                return Error(400, "invalid_request", "Provide valid report parameters and dates.");
            try
            {
                if (filters.Validate() == null || (filters.View != null && filters.View != "reports"))
                    return Error(400, "invalid_request", "Both dates and the Reports view are required.");
                var scope = database.ResolveExecutiveDashboardScope(dashboardType, groupRootId);
                var superUser = user.UserRoleID == 1 && session.IsSuperUser();
                if (!superUser) permissions.EnsurePermissionsCached(user);
                var pageId = pageIds.ResolvePageId(scope.PermissionPagePath);
                if (pageId <= 0 || (superUser ? !permissions.HasViewPermission(user, pageId) :
                    !session.TryGetAllowedViewIds(out var pages) || !pages.Contains(pageId)))
                    return Denied("PagePermission", user, scope.DashboardType);
                if (superUser ? !permissions.HasApiPermissionForPath(user, Request.Method, Request.PathBase.Value, Request.Path.Value) :
                    !session.HasApiPermissionForPath(Request.Method, Request.PathBase.Value, Request.Path.Value))
                    return Denied("ApiPermission", user, scope.DashboardType);
                var reports = ExecutiveDashboardViews.ForPerspective(scope.DashboardType).Single(v => v.Id == "reports").Reports;
                if (!reports.Contains(reportId)) return Denied("ReportPermission", user, scope.DashboardType);
                // A separate immutable root is retained for GROUP; filters only narrow it.
                database.NarrowExecutiveDashboardScope(scope, filters.EntityId);
                return Ok(new { success = true, data = query(scope, filters), dateRangeApplied = true, organizationalDimensionsApplied = true });
            }
            catch (UnauthenticatedException) { return Error(401, "unauthenticated", "The IAS session is no longer valid."); }
            catch (ExecutiveReportsNotDeployedException) { return Error(503, "reports_not_deployed", "Executive reports are not enabled."); }
            catch (UnauthorizedAccessException exception) { return Denied((exception as ExecutiveDashboardAccessException)?.Reason ?? "RoleMapping", user, dashboardType); }
            catch (ArgumentException) { return Error(400, "invalid_request", "Provide ordered ISO dates and valid report filters."); }
            catch (OracleException exception) when (exception.Number == 20801) { return Error(400, "invalid_request", "Invalid report parameters."); }
            catch (OracleException exception) when (exception.Number == 20802) { return Denied("EntityScope", user, dashboardType); }
            catch (OracleException exception) when (exception.Number == 20803)
            {
                logger.LogWarning(exception, "Executive report historical evidence unavailable for user {UserId}.", user.ID);
                return Error(503, "history_not_verified", "Historical report evidence is missing, ambiguous or incomplete for the selected scope and dates.");
            }
            catch (DatabaseUnavailableException exception)
            {
                logger.LogError(exception, "Executive report database unavailable.");
                return Error(503, "database_unavailable", "The executive reporting database is temporarily unavailable.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Executive report {ReportId} failed for user {UserId}.", reportId, user.ID);
                return Error(500, "server_error", "Unable to retrieve the executive report.");
            }
        }

        private IActionResult Denied(string reason, SessionUser user, string perspective)
        {
            logger.LogWarning("Executive report denied: {Reason}; user {UserId}, role {RoleId}, perspective {Perspective}, route {Route}.",
                reason, user.ID, user.UserRoleID, perspective, Request.Path.Value);
            return Error(403, "forbidden", "The report or selected scope is not authorized.");
        }
        private IActionResult Error(int status, string code, string message) => StatusCode(status, new { success = false, error = code, message });
    }
}
