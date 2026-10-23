using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;

namespace AIS.Controllers
{
    [Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public sealed class HeadAuditDepartmentController : Controller
    {
        private readonly DBConnection database;
        private readonly SessionHandler session;
        private readonly IPermissionService permissions;
        private readonly TopMenus menus;
        private readonly ILogger<HeadAuditDepartmentController> logger;

        public HeadAuditDepartmentController(DBConnection database, SessionHandler session,
            IPermissionService permissions, TopMenus menus, ILogger<HeadAuditDepartmentController> logger)
        {
            this.database = database; this.session = session; this.permissions = permissions;
            this.menus = menus; this.logger = logger;
        }

        private IActionResult Read(Func<IActionResult> action, bool api = false)
        {
            if (!session.TryGetUser(out var user) || session.IsSessionRevoked(user)) return Unauthorized();
            permissions.EnsurePermissionsCached(user);
            if (!HeadAuditMonitoringPolicy.IsEligible(user) ||
                !session.TryGetAllowedViewIds(out var pages) ||
                !pages.Contains(HeadAuditMonitoringPolicy.PageId)) return StatusCode(403);
            if (api && !permissions.HasApiPermissionForPath(user, "GET", Request.PathBase.Value, Request.Path.Value))
                return StatusCode(403);
            if (!ModelState.IsValid) return BadRequest();
            try
            {
                ViewData["TopMenu"] = menus.GetTopMenus();
                ViewData["TopMenuPages"] = menus.GetTopMenusPages();
                ViewData["HideTopHeader"] = true;
                ViewData["ScopeLabel"] = user.UserEntityName + " · Authorized Office and Subordinate Scope";
                return action();
            }
            catch (OracleException ex) when (ex.Number == 20151 || ex.Number == 20152) { return StatusCode(403); }
            catch (OracleException ex) when (ex.Number == 20153) { return BadRequest("Select a valid IAS audit period."); }
            catch (ArgumentException) { return BadRequest("Invalid dashboard filters."); }
            catch (UnauthorizedAccessException) { return StatusCode(403); }
            catch (AIS.Session.UnauthenticatedException) { return Unauthorized(); }
            catch (FormatException ex)
            {
                logger.LogWarning(ex, "Saved audit report has invalid encoding.");
                return StatusCode(503, "The saved report could not be read.");
            }
            catch (OracleException ex)
            {
                logger.LogError(ex, "Head audit monitoring read failed.");
                return StatusCode(503, "Audit monitoring is temporarily unavailable. Please retry.");
            }
        }

        [HttpGet]
        public IActionResult AuditTeamsMonitoring(int? auditPeriodId = null, int? officeId = null,
            int? statusId = null, string exception = null, DateTime? positionAsOn = null) => Read(() =>
            View("~/Views/Shared/EngagementWorkbench.cshtml", database.GetHeadAuditDashboard(auditPeriodId, officeId, statusId, exception, positionAsOn)));

        [HttpGet]
        public IActionResult Review(long engId, string section = "Plan", int? auditPeriodId = null,
            int? officeId = null, int? statusId = null, string exception = null, DateTime? positionAsOn = null) => Read(() =>
        {
            if (engId <= 0 || !HeadAuditMonitoringPolicy.Sections.Contains(section)) return BadRequest();
            if (new[] { "Plan", "Criteria", "Team" }.Contains(section)) section = "Tasks";
            if (!auditPeriodId.HasValue)
            {
                var saved = database.GetHeadAuditEngagements().SingleOrDefault(e => e.EngagementId == engId);
                if (saved == null) return StatusCode(403);
                auditPeriodId = saved.PeriodId;
            }
            var dashboard = database.GetHeadAuditDashboard(auditPeriodId, officeId, statusId, exception, positionAsOn);
            var engagement = dashboard.Engagements.SingleOrDefault(e => e.EngagementId == engId);
            if (engagement == null) return StatusCode(403);
            return View("~/Views/HeadAuditDepartment/Review.cshtml", new HeadAuditReview
            {
                Engagement = engagement, Section = section, Records = database.GetHeadAuditWork(engId, section),
                Plans = dashboard.Plans.Where(p => p.EngagementId == engId).ToList(),
                PositionAsOn=dashboard.PositionAsOn, AuditPeriodId=dashboard.AuditPeriodId, OfficeId=officeId, StatusId=statusId, Exception=exception
            });
        });

        [HttpGet("/api/HeadAuditDepartment/Engagements")]
        public IActionResult Engagements(int? auditPeriodId = null, int? officeId = null,
            int? statusId = null, string exception = null, DateTime? positionAsOn = null) => Read(() =>
            Json(database.GetHeadAuditDashboard(auditPeriodId, officeId, statusId, exception, positionAsOn).Register), api: true);

        [HttpGet("/api/HeadAuditDepartment/Work")]
        public IActionResult Work(long engId, string section = "Plan") => Read(() =>
        {
            if (engId <= 0 || !HeadAuditMonitoringPolicy.Sections.Contains(section)) return BadRequest();
            return Json(database.GetHeadAuditWork(engId, section));
        }, api: true);

        [HttpGet("/api/HeadAuditDepartment/Evidence")]
        public IActionResult Evidence(long engId, long evidenceId) => Read(() =>
        {
            if (engId <= 0 || evidenceId <= 0) return BadRequest();
            var document = database.GetHeadAuditDocument(engId, evidenceId, report: false);
            return document == null ? StatusCode(403) : File(document.Bytes, "application/octet-stream", document.Name ?? $"evidence-{evidenceId}.bin");
        }, api: true);

        [HttpGet("/api/HeadAuditDepartment/Report")]
        public IActionResult Report(long engId, long reportId) => Read(() =>
        {
            if (engId <= 0 || reportId <= 0) return BadRequest();
            var document = database.GetHeadAuditDocument(engId, reportId, report: true);
            return document == null ? StatusCode(403) : File(document.Bytes, "application/octet-stream", document.Name ?? $"report-{reportId}.bin");
        }, api: true);

        [HttpGet("/api/HeadAuditDepartment/ResponseEvidence")]
        public IActionResult ResponseEvidence(long engId, long evidenceId) => Read(() =>
        {
            if (engId <= 0 || evidenceId <= 0) return BadRequest();
            var document = database.GetHeadAuditDocument(engId, evidenceId, report: false, responseEvidence: true);
            return document == null ? StatusCode(403) : File(document.Bytes, "application/octet-stream", document.Name ?? $"evidence-{evidenceId}.bin");
        }, api: true);
    }
}
