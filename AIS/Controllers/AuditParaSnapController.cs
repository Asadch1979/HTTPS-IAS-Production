using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace AIS.Controllers
{
    [Authorize]
    [Route("api/AuditParaSnap/[action]")]
    [EnableRateLimiting("GeneralApiPolicy")]
    [ResponseCache(Duration=0,Location=ResponseCacheLocation.None,NoStore=true)]
    public sealed class AuditParaSnapController : Controller
    {
        private readonly DBConnection database;
        private readonly SessionHandler session;
        private readonly IPermissionService permissions;
        private readonly IPageIdResolver pageIds;
        private readonly IConfiguration configuration;
        private readonly ILogger<AuditParaSnapController> logger;
        private readonly IWebHostEnvironment environment;
        public AuditParaSnapController(DBConnection database,SessionHandler session,IPermissionService permissions,
            IPageIdResolver pageIds,IConfiguration configuration,ILogger<AuditParaSnapController> logger,IWebHostEnvironment environment)
        {
            this.database=database;this.session=session;this.permissions=permissions;this.pageIds=pageIds;
            this.configuration=configuration;this.logger=logger;this.environment=environment;
        }
        [HttpGet]
        public IActionResult GetKpis([FromQuery] ExecutiveDashboardFilterRequest filters)
            => Execute(filters,(scope,f) => database.GetAuditParaSnapKpis(scope,f));
        [HttpGet]
        public IActionResult GetDistribution([FromQuery] ExecutiveDashboardFilterRequest filters)
            => Execute(filters,(scope,f) => database.GetAuditParaSnapDistribution(scope,f));
        [HttpGet]
        public IActionResult GetCriticalItems([FromQuery] ExecutiveDashboardFilterRequest filters,int maxRows=100)
            => Execute(filters,(scope,f) => database.GetAuditParaSnapCritical(scope,f,maxRows));
        // Reuse the approved A-E data readers, PDF generator and report presentation.
        // Their existing routes, grants and behavior remain untouched.
        [HttpGet]
        public IActionResult GetReportA([FromQuery] ExecutiveDashboardFilterRequest filters,string format=null) => Report("A",filters,format,database.GetExecutiveReportA);
        [HttpGet]
        public IActionResult GetReportB([FromQuery] ExecutiveDashboardFilterRequest filters,string format=null) => Report("B",filters,format,database.GetExecutiveReportB);
        [HttpGet]
        public IActionResult GetReportC([FromQuery] ExecutiveDashboardFilterRequest filters,string format=null) => Report("C",filters,format,database.GetExecutiveReportC);
        [HttpGet]
        public IActionResult GetReportD([FromQuery] ExecutiveDashboardFilterRequest filters,string format=null) => Report("D",filters,format,database.GetExecutiveReportD);
        [HttpGet]
        public IActionResult GetReportE([FromQuery] ExecutiveDashboardFilterRequest filters,string format=null) => Report("E",filters,format,database.GetExecutiveReportE);

        private IActionResult Report(string id,ExecutiveDashboardFilterRequest filters,string format,
            Func<ExecutiveDashboardScope,ExecutiveDashboardFilterRequest,ExecutiveReportResult> query)
            => Execute(filters,query,true,format,id);

        private IActionResult Execute<T>(ExecutiveDashboardFilterRequest filters,
            Func<ExecutiveDashboardScope,ExecutiveDashboardFilterRequest,T> query,
            bool report=false,string format=null,string reportId=null)
        {
            var user=session.GetUser();
            if (User.Identity?.IsAuthenticated != true || user==null || session.IsSessionRevoked(user))
                return Error(401,"unauthenticated","An active IAS session is required.");
            if (!ModelState.IsValid) return Error(400,"invalid_request","Invalid reporting parameters.");
            try
            {
                var scope=database.ResolveAuditParaSnapScope();
                var superUser=user.UserRoleID==1 && session.IsSuperUser();
                if (!superUser) permissions.EnsurePermissionsCached(user);
                var pageId=pageIds.ResolvePageId(AuditParaSnapPolicy.PagePath);
                if (pageId != AuditParaSnapPolicy.PageId ||
                    (superUser ? !permissions.HasViewPermission(user,pageId) :
                    !session.TryGetAllowedViewIds(out var pages) || !pages.Contains(pageId)))
                    return Error(403,"forbidden","Audit Para Snap page permission is required.");
                if (superUser ? !permissions.HasApiPermissionForPath(user,Request.Method,Request.PathBase.Value,Request.Path.Value) :
                    !session.HasApiPermissionForPath(Request.Method,Request.PathBase.Value,Request.Path.Value))
                    return Error(403,"forbidden","The exact Audit Para Snap API grant is required.");
                AuditParaSnapPolicy.Validate(filters);
                if (report != (filters.View=="reports"))
                    return Error(400,"invalid_request","Dataset is not enabled for this view.");
                if (!string.IsNullOrEmpty(format) && (!report || !string.Equals(format,"pdf",StringComparison.OrdinalIgnoreCase)))
                    return Error(400,"invalid_request","Only report PDF export is supported.");
                // Validate all hierarchy selections before narrowing scope.
                database.GetExecutiveDashboardFilterOptions(scope,filters);
                scope=database.NarrowExecutiveDashboardScope(scope,filters.EntityId);
                var data=query(scope,filters);
                if (report && string.Equals(format,"pdf",StringComparison.OrdinalIgnoreCase))
                    return File(ExecutiveReportsPdfGenerator.Create(reportId,(ExecutiveReportResult)(object)data,environment.WebRootPath),
                        "application/pdf",$"IAS_Report_{reportId}_{filters.StartDate}_to_{filters.EndDate}.pdf");
                return Ok(new { success=true,data,dateRangeApplied=true,
                    startDate=filters.StartDate,endDate=filters.EndDate,organizationalDimensionsApplied=true,
                    currentOrganizationMapping=true,historicalBalancesEstimated=true,settlementAttributionEstimated=true,
                    historicalLimitations="Estimates use latest master dates/status, present risk and organization; reopenings and missing events can change past counts. Historical cycle breakdown is unavailable. Settlement events use latest qualifying dates." });
            }
            catch (UnauthenticatedException) { return Error(401,"unauthenticated","IAS session changed."); }
            catch (UnauthorizedAccessException) { return Error(403,"forbidden","Audit Para Snap or the selected scope is not authorized."); }
            catch (ArgumentException exception) { return Error(400,"invalid_request",exception.Message); }
            catch (Exception exception)
            {
                logger.LogError(exception,"Audit Para Snap read failed for user {UserId}.",user.ID);
                return Error(503,"unavailable","Reporting is unavailable. Verify Oracle compilation, grants and historical evidence.");
            }
        }
        private IActionResult Error(int status,string code,string message)
            => StatusCode(status,new {success=false,error=code,message});
    }
}
