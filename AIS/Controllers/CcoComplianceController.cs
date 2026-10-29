using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace AIS.Controllers
{
    [Authorize]
    [Route("api/CcoCompliance/[action]")]
    [EnableRateLimiting("GeneralApiPolicy")]
    [ResponseCache(Duration=0,Location=ResponseCacheLocation.None,NoStore=true)]
    public sealed class CcoComplianceController : Controller
    {
        readonly DBConnection database;
        readonly SessionHandler session;
        readonly IPermissionService permissions;
        readonly IPageIdResolver pages;
        readonly ILogger<CcoComplianceController> logger;
        public CcoComplianceController(DBConnection database,SessionHandler session,IPermissionService permissions,IPageIdResolver pages,ILogger<CcoComplianceController> logger)
        { this.database=database;this.session=session;this.permissions=permissions;this.pages=pages;this.logger=logger; }
        [HttpGet] public IActionResult GetFunctionPosition([FromQuery] CcoOrganizationFilter filters)=>Read(filters,nameof(GetFunctionPosition));
        [HttpGet] public IActionResult GetOrgFilters([FromQuery] CcoOrganizationFilter filters)=>Read(filters,nameof(GetOrgFilters));
        [HttpGet] public IActionResult GetOrgPosition([FromQuery] CcoOrganizationFilter filters)=>Read(filters,nameof(GetOrgPosition));
        [HttpGet] public IActionResult GetOrgPerformance([FromQuery] CcoOrganizationFilter filters)=>Read(filters,nameof(GetOrgPerformance));
        [HttpGet] public IActionResult GetOrgSettlement([FromQuery] CcoOrganizationFilter filters)=>Read(filters,nameof(GetOrgSettlement));
        [HttpGet] public IActionResult GetOrgExceptions([FromQuery] CcoOrganizationFilter filters)=>Read(filters,nameof(GetOrgExceptions));
        IActionResult Read(CcoOrganizationFilter filters,string action)
        {
            var user=session.GetUser();var outcome="denied";var rowCount=0;
            try
            {
                if (User.Identity?.IsAuthenticated!=true || user==null || session.IsSessionRevoked(user)) return Error(401,"unauthenticated","An active IAS session is required.");
                // Reject unknown, duplicate and foreign selectors; never silently drop a root or export request.
                var allowed=action==nameof(GetFunctionPosition) ? new[]{"startDate","endDate","maxRows"} : new[]{"startDate","endDate","groupBy","groupId","divisionId","gmOfficeId","regionId","entityId","maxRows"};
                if (!ModelState.IsValid || filters==null || Request.Query.Any(q=>q.Value.Count!=1 || !allowed.Contains(q.Key,StringComparer.OrdinalIgnoreCase)))
                    return Error(400,"invalid_request","Provide only supported CCO organization filters.");
                var scope=database.ResolveExecutiveDashboardScope("CCO",null);
                database.ValidateCcoOrganizationContext(scope);
                var super=user.UserRoleID==1 && session.IsSuperUser();
                if (!super) permissions.EnsurePermissionsCached(user);
                if (pages.ResolvePageId("/ExecutiveOverview/CCO")!=1000302 ||
                    (super ? !permissions.HasViewPermission(user,1000302) : !session.TryGetAllowedViewIds(out var ids) || !ids.Contains(1000302)))
                    return Error(403,"forbidden","CCO page permission is required.");
                if (super ? !permissions.HasApiPermissionForPath(user,Request.Method,Request.PathBase.Value,Request.Path.Value) :
                    !session.HasApiPermissionForPath(Request.Method,Request.PathBase.Value,Request.Path.Value))
                    return Error(403,"forbidden","This exact CCO API permission is required.");
                filters.Validate();
                object data;
                if(action==nameof(GetFunctionPosition))
                    data=CcoReportVisibility.FunctionRows(database.GetCcoFunctionPosition(scope,filters),super);
                else {
                var paths=database.GetCcoOrganizationPaths(scope,filters);
                database.ValidateCcoOrganizationContext(scope);
                filters.ValidatePath(paths);
                data=action==nameof(GetOrgFilters) ? paths : database.GetCcoOrganizationRows(scope,filters,action);
                database.ValidateCcoOrganizationContext(scope);
                }
                database.ValidateCcoOrganizationContext(scope);
                rowCount=data is System.Collections.ICollection collection ? collection.Count : 0;
                outcome="success";
                return Ok(new {success=true,data,correlationId=HttpContext.TraceIdentifier});
            }
            catch (UnauthenticatedException) { return Error(401,"unauthenticated","The IAS context is no longer valid."); }
            catch (UnauthorizedAccessException) { return Error(403,"forbidden","The CCO perspective or scope is not authorized."); }
            catch (ArgumentException) { return Error(400,"invalid_request","Select a valid date range and verified organizational parent path."); }
            catch (CcoOrganizationUnavailableException) { outcome="pending_staging";return Error(503,"cco_not_verified","Organizational reporting awaits approved staging mappings and database authorization."); }
            catch (CcoFunctionPositionReadException ex)
            {
                // Only server logs receive the failing Oracle step and error number.
                // Never return raw Oracle text, the scope token or session details.
                outcome="database_failure";
                logger.LogError(ex,
                    "CCO Function Wise Position database failure at {Stage}; Oracle number {OracleNumber}; correlation {Correlation}",
                    ex.Stage, ex.OracleNumber, HttpContext.TraceIdentifier);
                return Error(503,"cco_database_unavailable","Function Wise Position database read failed. Contact the administrator with the request correlation ID.");
            }
            catch (Exception ex)
            {
                outcome="backend_failure";
                logger.LogError(ex,"CCO organization read failed at an unclassified step {Correlation}",HttpContext.TraceIdentifier);
                return Error(503,"unavailable","CCO organizational reporting is temporarily unavailable.");
            }
            finally
            {
                logger.LogInformation("CCO action {Action}; user {User}; role {Role}; posting {Posting}; session {Session}; context {Context}; group {Group}; division {Division}; gm {Gm}; start {Start}; end {End}; rows {Rows}; outcome {Outcome}; correlation {Correlation}",
                    action,user?.ID,user?.UserRoleID,user?.UserEntityID,user?.SessionId,user?.UserContextAssignmentId,filters?.GroupId,filters?.DivisionId,filters?.GmOfficeId,filters?.StartDate,filters?.EndDate,rowCount,outcome,HttpContext.TraceIdentifier);
            }
        }
        IActionResult Error(int status,string error,string message)=>StatusCode(status,new {success=false,error,message,correlationId=HttpContext.TraceIdentifier});
    }
}
