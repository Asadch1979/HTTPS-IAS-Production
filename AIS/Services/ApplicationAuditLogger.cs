using AIS.Controllers;
using AIS.Models;
using AIS.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AIS.Services
    {
    public interface IApplicationAuditLogger
        {
        Task LogSuccessAsync(ApplicationAuditEvent auditEvent, long durationMs = 0);
        }

    public sealed class ApplicationAuditLogger : IApplicationAuditLogger
        {
        private readonly DBConnection _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SessionHandler _sessionHandler;
        private readonly IClientIpResolver _clientIpResolver;
        private readonly SystemErrorMonitor _systemErrorMonitor;
        private readonly ILogger<ApplicationAuditLogger> _logger;

        public ApplicationAuditLogger(DBConnection db, IHttpContextAccessor httpContextAccessor,
            SessionHandler sessionHandler, IClientIpResolver clientIpResolver,
            SystemErrorMonitor systemErrorMonitor, ILogger<ApplicationAuditLogger> logger)
            {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _sessionHandler = sessionHandler;
            _clientIpResolver = clientIpResolver;
            _systemErrorMonitor = systemErrorMonitor;
            _logger = logger;
            }

        public async Task LogSuccessAsync(ApplicationAuditEvent auditEvent, long durationMs = 0)
            {
            if (auditEvent == null || string.IsNullOrWhiteSpace(auditEvent.ActionName))
                return;

            var context = _httpContextAccessor.HttpContext;
            try
                {
                _sessionHandler.TryGetUser(out var user);
                var request = context?.Request;
                var descriptor = context?.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
                var clientIp = _clientIpResolver.GetClientIp(context);
                var proxyIp = ResolveProxyIp(context, clientIp);
                var requestId = FirstNonEmpty(request?.Headers["X-Request-ID"].FirstOrDefault(), context?.TraceIdentifier);
                var traceId = FirstNonEmpty(Activity.Current?.TraceId.ToString(), context?.TraceIdentifier);

                _db.LogApplicationActivity(auditEvent, new ApplicationAuditContext
                    {
                    Ppno = FirstNonEmpty(auditEvent.ActorPpno, user?.PPNumber),
                    RoleId = Positive(auditEvent.ActorRoleId) ?? Positive(user?.UserRoleID),
                    GroupId = Positive(auditEvent.ActorGroupId) ?? Positive(user?.UserGroupID),
                    EntityId = Positive(auditEvent.ActorEntityId) ?? Positive(user?.UserEntityID),
                    UserContextId = Positive(auditEvent.ActorUserContextId) ?? Positive(user?.UserContextAssignmentId),
                    SessionId = FirstNonEmpty(user?.SessionId, context?.Session?.Id),
                    PageId = Positive(context?.Session?.GetInt32(SessionKeys.PageId)),
                    ControllerName = descriptor?.ControllerName ?? context?.Request?.RouteValues["controller"]?.ToString(),
                    ControllerAction = descriptor?.ActionName ?? context?.Request?.RouteValues["action"]?.ToString(),
                    ApiPath = request?.Path.Value,
                    HttpMethod = request?.Method,
                    ClientIp = clientIp,
                    ProxyIp = proxyIp,
                    UserAgent = Truncate(request?.Headers.UserAgent.ToString(), 1000),
                    TraceId = traceId,
                    RequestId = requestId,
                    DurationMs = Math.Max(0, durationMs)
                    });
                }
            catch (Exception ex)
                {
                // Audit is deliberately outside the business success path and can never change its result.
                _logger.LogError(ex, "Application audit logging failed for {AuditAction}.", auditEvent.ActionName);
                try
                    {
                    await _systemErrorMonitor.ReportAsync(ex, context, "ApplicationAudit", "PKG_LG.P_LOG_APPLICATION_ACTIVITY");
                    }
                catch (Exception monitorException)
                    {
                    // No recursion: the system-error failure is only written to the process logger.
                    _logger.LogError(monitorException, "Application audit failure could not be recorded by the system-error monitor.");
                    }
                }
            }

        private static int? Positive(int? value) => value.GetValueOrDefault() > 0 ? value : null;
        private static int? Positive(int value) => value > 0 ? value : null;

        private static string ResolveProxyIp(HttpContext context, string clientIp)
            {
            var original = context?.Request?.Headers["X-Original-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(original))
                return Truncate(original.Split(',').LastOrDefault()?.Trim(), 100);
            var remote = context?.Connection?.RemoteIpAddress?.ToString();
            return !string.Equals(remote, clientIp, StringComparison.OrdinalIgnoreCase) ? Truncate(remote, 100) : null;
            }

        private static string FirstNonEmpty(params string[] values) => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        internal static string Truncate(string value, int maximum) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, maximum)];
        }

    public sealed class ApplicationAuditContext
        {
        public string Ppno { get; set; }
        public int? RoleId { get; set; }
        public int? GroupId { get; set; }
        public int? EntityId { get; set; }
        public int? UserContextId { get; set; }
        public string SessionId { get; set; }
        public int? PageId { get; set; }
        public string ControllerName { get; set; }
        public string ControllerAction { get; set; }
        public string ApiPath { get; set; }
        public string HttpMethod { get; set; }
        public string ClientIp { get; set; }
        public string ProxyIp { get; set; }
        public string UserAgent { get; set; }
        public string TraceId { get; set; }
        public string RequestId { get; set; }
        public long DurationMs { get; set; }
        }
    }
