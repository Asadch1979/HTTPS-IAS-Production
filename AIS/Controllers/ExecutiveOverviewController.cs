using AIS.Models;
using AIS.Services;
using AIS.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly IConfiguration configuration;

        public ExecutiveOverviewController(DBConnection database, SessionHandler session,
            IPermissionService permissions, IPageIdResolver pageIds, TopMenus menus,
            ILogger<ExecutiveOverviewController> logger, IConfiguration configuration)
        {
            this.database = database;
            this.session = session;
            this.permissions = permissions;
            this.pageIds = pageIds;
            this.menus = menus;
            this.logger = logger;
            this.configuration = configuration;
        }

        // Historical IAS menu entries may point to /ExecutiveOverview/Index.
        // This is a route-only entry point: resolve from the authenticated IAS
        // context, then reuse the existing perspective/page/API permission gates.
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var user = session.GetUser();
            if (User.Identity?.IsAuthenticated != true || user == null || session.IsSessionRevoked(user))
                return RedirectToAction("Index", "Login");

            if (user.UserRoleID == 1)
                return session.IsSuperUser() ? RedirectToAction(nameof(TOP)) : StatusCode(403);

            if (user.UserRoleID == 3) return RedirectToAction(nameof(CIA));
            if (user.UserRoleID == 38) return RedirectToAction(nameof(CCO));

            // TOP/GROUP users must have one explicit, verified role-to-page
            // mapping. Do not select a bank-wide view based on entity posting.
            var mappings = configuration.GetSection("ExecutiveDashboard:RoleMappings")
                .Get<ExecutiveDashboardRoleMapping[]>() ?? Array.Empty<ExecutiveDashboardRoleMapping>();
            var matches = mappings.Where(m => m != null && m.RoleId == user.UserRoleID).ToList();
            if (matches.Count > 1) return StatusCode(403);
            if (matches.Count == 1)
            {
                var m = matches[0];
                var type = m.DashboardType?.Trim().ToUpperInvariant();
                if (m.PagePath == "/ExecutiveOverview/TOP" && type == "TOP")
                    return RedirectToAction(nameof(TOP));
                if (m.PagePath == "/ExecutiveOverview/GROUP" && type == "GROUP")
                    return RedirectToAction(nameof(GROUP));
                return StatusCode(403);
            }

            // Reporting Line Management is not a bank-wide fallback.
            // It has its own dedicated page grant, never the organization page.
            if (user.UserEntityID.GetValueOrDefault() <= 0) return StatusCode(403);
            permissions.EnsurePermissionsCached(user);
            var rlmPage = pageIds.ResolvePageId("/ExecutiveOverview/RLM");
            if (rlmPage <= 0 || !session.TryGetAllowedViewIds(out var pages) || !pages.Contains(rlmPage))
                return StatusCode(403);
            return RedirectToAction(nameof(RLM));
        }

        [HttpGet("TOP")]
        public IActionResult TOP(int? entityId = null, int? groupRootId = null, string view = "glance", [FromQuery] ExecutiveDashboardFilterRequest filters = null) => Dashboard("TOP", entityId, groupRootId, view, filters);
        [HttpGet("GROUP")]
        public IActionResult GROUP(int? entityId = null, int? groupRootId = null, string view = "glance", [FromQuery] ExecutiveDashboardFilterRequest filters = null) => Dashboard("GROUP", entityId, groupRootId, view, filters);
        [HttpGet("CCO")]
        public IActionResult CCO(int? entityId = null, int? groupRootId = null, string view = "glance", [FromQuery] ExecutiveDashboardFilterRequest filters = null) => Dashboard("CCO", entityId, groupRootId, view, filters);
        [HttpGet("CIA")]
        public IActionResult CIA(int? entityId = null, int? groupRootId = null, string view = "command", [FromQuery] ExecutiveDashboardFilterRequest filters = null)
        {
            if (view != "command")
            {
                if (!ModelState.IsValid || groupRootId.HasValue || !AuditParaSnapPolicy.Views.Contains(view))
                    return BadRequest("Unknown CIA bookmark or invalid filters.");
                try
                {
                    filters ??= new ExecutiveDashboardFilterRequest();
                    filters.EntityId=entityId; filters.View=view;
                    filters.Validate();
                    var values=filters.RouteValues(view,null);
                    var report=Request.Query["report"].ToString().ToUpperInvariant();
                    if (view=="reports" && new[]{"A","B","C","D","E"}.Contains(report)) values["report"]=report;
                    return RedirectToAction(nameof(AuditParaSnap),values);
                }
                catch (ArgumentException exception) { return BadRequest(exception.Message); }
            }
            // CIA has its own IAS period state. Snap date state stays on Snap URLs.
            filters ??= new ExecutiveDashboardFilterRequest();
            filters.StartDate=null; filters.EndDate=null;
            return Dashboard("CIA",entityId,groupRootId,"command",filters);
        }
        [HttpGet("AuditParaSnap")]
        public IActionResult AuditParaSnap(int? entityId=null,string view="glance",
            [FromQuery] ExecutiveDashboardFilterRequest filters=null)
            => Dashboard("CIA",entityId,null,view,filters,auditParaSnap:true);
        [HttpGet("RLM")]
        public IActionResult RLM(int? entityId = null, int? groupRootId = null, string view = "glance", [FromQuery] ExecutiveDashboardFilterRequest filters = null)
            => Dashboard("RLM", entityId, groupRootId, view, filters);

        private IActionResult Dashboard(string requestedType, int? entityId, int? groupRootId, string view, ExecutiveDashboardFilterRequest filters, bool auditParaSnap=false)
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
                if (auditParaSnap)
                {
                    filters.EntityId = entityId;
                    filters.ValidateParentPerspective("SNAP");
                    AuditParaSnapPolicy.ApplyDefault(filters);
                }
                else
                {
                    filters.EntityId = entityId;
                    filters.ValidateParentPerspective(requestedType);
                }
                if (!auditParaSnap && requestedType != "CIA" &&
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
                    var defaultEndDate = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(5)).Date.AddDays(-1);
                    filters.StartDate = new DateTime(defaultEndDate.Year - (annualDefault ? 3 : 0), 1, 1).ToString("yyyy-MM-dd");
                    filters.EndDate = defaultEndDate.ToString("yyyy-MM-dd");
                }
                filters.EntityId = entityId;
                filters.View = view;
                filters.Validate();
                if (auditParaSnap) AuditParaSnapPolicy.Validate(filters);
                if (requestedType == "RLM" && (filters.DimensionId.HasValue || filters.SelectBy != "entityType"))
                    return BadRequest("Reporting Line Management supports Reporting Office and Entity only.");
                var superUser = user.UserRoleID == 1 && session.IsSuperUser();
                var scope = auditParaSnap ? database.ResolveAuditParaSnapScope() :
                    database.ResolveExecutiveDashboardScope(requestedType, groupRootId, allowGroupSelection: superUser);
                if (!superUser) permissions.EnsurePermissionsCached(user);
                var pageId = pageIds.ResolvePageId(scope.PermissionPagePath);
                if (auditParaSnap && pageId != AuditParaSnapPolicy.PageId) return StatusCode(403);
                if (pageId <= 0 || (superUser ? !permissions.HasViewPermission(user, pageId) :
                    !session.TryGetAllowedViewIds(out var pages) || !pages.Contains(pageId)))
                {
                    LogDenial("PagePermission", user, requestedType);
                    return StatusCode(403);
                }
                var views = auditParaSnap ? AuditParaSnapPolicy.Definitions() : ExecutiveDashboardViews.ForPerspective(scope.DashboardType);
                if (!auditParaSnap && requestedType=="CIA") views.RemoveAll(v => v.Id != "command");
                if (requestedType == "GROUP")
                {
                    // Six analysis tabs; legacy glance bookmarks open the management scorecard.
                    views.RemoveAll(v => v.Id == "glance");
                    if (view == "glance") view = "management";
                }
                var root = database.GetExecutiveDashboardRoot(scope);
                var regional = scope.DashboardType == "RLM" && (root?.EntityTypeId == 21 || root?.EntityTypeId == 5);
                // Divisional/Group RLM management eligibility requires an explicitly
                // approved IAS role ID in addition to posting type and page grant.
                var allowedManagementRoles = configuration.GetSection("RoleDashboards:ManagementRlmRoleIds")
                    .Get<int[]>() ?? Array.Empty<int>();
                var management = (scope.DashboardType == "GROUP" &&
                    (root?.EntityTypeId == 18 || (superUser && !scope.GroupRootId.HasValue))) ||
                    (scope.DashboardType == "RLM" &&
                     (allowedManagementRoles.Contains(scope.RoleId) ||
                      (superUser && scope.GroupRootId.HasValue)) &&
                     (root?.EntityTypeId == 18 || root?.EntityTypeId == 3));
                if ((view == "regional" && !regional) || (view == "management" && !management)) return StatusCode(403);
                if (!regional) views.RemoveAll(v => v.Id == "regional");
                if (!management) views.RemoveAll(v => v.Id == "management");
                // Preserve explicitly chosen views; only replace the initial default.
                if (!auditParaSnap && view == "glance" && !Request.Query.ContainsKey("view"))
                {
                    if (scope.DashboardType == "CIA") view = "command";
                    else if (regional) view = "regional";
                    else if (management) view = "management";
                }
                filters.View = view;
                var selectedView = views.FirstOrDefault(v => v.Id == view);
                if (selectedView == null) return BadRequest("Unknown dashboard view.");
                var needsGroup = scope.DashboardType == "GROUP" && !scope.GroupRootId.HasValue;
                var needsRlmRoot = scope.DashboardType == "RLM" && superUser && !scope.GroupRootId.HasValue;
                // The selector page may open without a root; reporting queries may not.
                if ((!needsGroup && !needsRlmRoot) || entityId.HasValue)
                    database.NarrowExecutiveDashboardScope(scope, entityId);
                if (needsGroup && (filters.DimensionId.HasValue || filters.ReportingOfficeId.HasValue))
                    throw new ExecutiveDashboardAccessException("EntityScope", "Select a Group root before selecting organizational filters.");
                var options = needsGroup || needsRlmRoot || (auditParaSnap || (requestedType != "CIA" && view != "regional")) ? new ExecutiveDashboardFilterOptions() :
                    database.GetExecutiveDashboardFilterOptions(scope, filters);
                // Load only the children of the selected audit division. An empty
                // or unseeded division is unavailable, never a bank-wide fallback.
                var snapDepartments = auditParaSnap
                    ? database.GetAuditParaSnapDepartments(scope, filters.SnapDivision, filters.SnapDepartmentId)
                    : new System.Collections.Generic.List<ExecutiveFilterOption>();
                // Fetch the actual IAS audit period IDs; a calendar year is not
                // interchangeable with T_AU_PERIOD.AUDITPERIODID.
                var ciaPeriods = scope.DashboardType == "CIA" && view == "command"
                    ? database.GetAuditPeriods().Where(p => p.AUDITPERIODID > 0)
                        .OrderByDescending(p => p.START_DATE).ToList()
                    : new System.Collections.Generic.List<AuditPeriodModel>();
                int parsedPeriod;
                var requestedCiaPeriod = int.TryParse(Request.Query["auditPeriodId"].ToString(), out parsedPeriod)
                    ? parsedPeriod : (int?)null;
                var selectedCiaPeriod = SelectCiaAuditPeriod(ciaPeriods, requestedCiaPeriod,
                    DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(5)).Date);
                var groups = superUser && scope.DashboardType == "GROUP" ?
                    database.GetExecutiveDashboardGroups(scope).OrderBy(n => n.EntityName).ToList() : new System.Collections.Generic.List<OrganizationNode>();
                var rlmRoots = superUser && scope.DashboardType == "RLM"
                    ? database.GetExecutiveDashboardRlmRoots(scope)
                    : new System.Collections.Generic.List<OrganizationNode>();
                ViewData["TopMenu"] = menus.GetTopMenus();
                ViewData["TopMenuPages"] = menus.GetTopMenusPages();
                return View(auditParaSnap ? "AuditParaSnap" : scope.DashboardType, new ExecutiveDashboardPageModel
                {
                    Views = views,
                    SelectedView = selectedView,
                    DashboardType = scope.DashboardType,
                    ScopeLabel = needsRlmRoot ? "Select a Group, Division, GM or Region reporting root" :
                        needsGroup ? "Select a Group Head entity to load its authorized hierarchy" :
                        scope.DashboardType == "GROUP" ? (superUser ? groups.First(n => n.EntityId == scope.GroupRootId).EntityName : user.UserEntityName) + " and descendants" :
                        scope.DashboardType == "RLM" ? (superUser && root != null ? root.EntityName : user.UserEntityName) + " and all reporting descendants" : "Authorized bank-wide perspective",
                    SelectedEntityId = entityId,
                    IsSuperUser = superUser,
                    GroupRootId = scope.GroupRootId,
                    Groups = groups,
                    RlmRoots = rlmRoots,
                    NeedsRlmRoot = needsRlmRoot,
                    Filters = filters,
                    FilterOptions = options,
                    SnapDepartments = snapDepartments,
                    RoleOracleEnabled = configuration.GetValue<bool>("RoleDashboards:OracleReadEnabled"),
                    IsAuditParaSnap = auditParaSnap,
                    AuditParaSnapEnabled = configuration.GetValue<bool>("RoleDashboards:AuditParaSnapEnabled"),
                    CiaPeriods = ciaPeriods,
                    SelectedCiaPeriodId = selectedCiaPeriod,
                    IsManagementPerspective = management,
                    ComparisonEntities = view == "regional" && !configuration.GetValue<bool>("RoleDashboards:OracleReadEnabled")
                        ? database.GetExecutiveComparisonEntities(scope, filters, options, branches: true) : new(),
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

        // Explicit selections may be future periods. Automatic selection must
        // prefer a current IAS period, then the latest period already started.
        internal static int? SelectCiaAuditPeriod(IEnumerable<AuditPeriodModel> periods, int? requested, DateTime today)
        {
            var valid = periods.Where(p => p.AUDITPERIODID > 0 && p.START_DATE.Date <= p.END_DATE.Date).ToList();
            if (requested.HasValue && valid.Any(p => p.AUDITPERIODID == requested)) return requested;
            var started = valid.Where(p => p.START_DATE.Date <= today.Date)
                .OrderByDescending(p => p.START_DATE).ThenByDescending(p => p.AUDITPERIODID).ToList();
            return started.FirstOrDefault(p => p.END_DATE.Date >= today.Date)?.AUDITPERIODID
                ?? started.FirstOrDefault()?.AUDITPERIODID;
        }

        private void LogDenial(string reason, SessionUser user, string perspective)
            => logger.LogWarning("Executive dashboard denied: {Reason}; user {UserId}, role {RoleId}, perspective {Perspective}, route {Route}.",
                reason, user?.ID, user?.UserRoleID, perspective, Request.Path.Value);
    }
}
