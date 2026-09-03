using AIS.Controllers;
using AIS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIS.Services
    {
    public class SystemErrorMonitor
        {
        private static readonly Regex VolatileTokenRegex = new Regex(@"\b([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}|[0-9a-f]{16,}|trace\s*id\s*[:=]\s*\S+)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SystemErrorMonitor> _logger;
        private readonly IClientIpResolver _clientIpResolver;

        public SystemErrorMonitor(
            IConfiguration configuration,
            IHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider,
            ILogger<SystemErrorMonitor> logger,
            IClientIpResolver clientIpResolver)
            {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientIpResolver = clientIpResolver;
            }

        public async Task<SystemErrorRecord> ReportAsync(Exception exception, HttpContext context = null, string module = null, string storedProcedure = null)
            {
            if (exception == null)
                {
                throw new ArgumentNullException(nameof(exception));
                }

            context ??= _httpContextAccessor?.HttpContext;
            var errorContext = BuildContext(exception, context, module, storedProcedure);
            var fingerprint = BuildFingerprint(exception, errorContext);
            var technicalDetails = BuildTechnicalDetails(exception, errorContext);
            var errorCode = ResolveErrorCode(exception);
            var message = RedactSensitiveValues(exception.GetBaseException().Message);

            var db = _serviceProvider.GetService<DBConnection>();
            if (db == null)
                {
                _logger.LogError(exception, "System error monitor could not persist error because DBConnection is unavailable.");
                return new SystemErrorRecord { ErrorReference = BuildFallbackReference(fingerprint), Fingerprint = fingerprint };
                }

            SystemErrorRecord record;
            try
                {
                record = db.RegisterSystemError(fingerprint, exception.GetType().FullName, errorCode, message, technicalDetails, errorContext);
                }
            catch (Exception logException)
                {
                _logger.LogError(logException, "System error monitor could not persist error fingerprint {Fingerprint}.", fingerprint);
                return new SystemErrorRecord
                    {
                    ErrorReference = BuildFallbackReference(fingerprint),
                    Fingerprint = fingerprint
                    };
                }

            if (!record.EmailAlreadySent)
                {
                var sent = await SendFirstOccurrenceEmailAsync(db, record, exception, errorContext, errorCode, message);
                db.MarkSystemErrorEmailStatus(record.ErrorId, sent);
                }

            return record;
            }

        public Task<SystemErrorRecord> ReportStatusCodeAsync(HttpContext context, int statusCode)
            {
            var path = context?.Request?.Path.Value ?? string.Empty;
            var ex = new InvalidOperationException($"HTTP {statusCode} response completed without an exception for {path}.");
            return ReportAsync(ex, context, "HTTP", null);
            }

        public static string BuildSafeUserMessage(SystemErrorRecord record)
            {
            var reference = string.IsNullOrWhiteSpace(record?.ErrorReference) ? "IAS-ERR-UNKNOWN" : record.ErrorReference;
            return $"An unexpected system error occurred and has been reported automatically. Reference: {reference}";
            }

        private SystemErrorContext BuildContext(Exception exception, HttpContext context, string module, string storedProcedure)
            {
            var endpoint = context?.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var sessionHandler = context?.RequestServices?.GetService<SessionHandler>();
            SessionUser user = null;
            sessionHandler?.TryGetUser(out user);
            int? engagementId = null;
            if (sessionHandler != null && sessionHandler.TryGetActiveEngagementId(out var activeEngagementId))
                {
                engagementId = activeEngagementId;
                }

            return new SystemErrorContext
                {
                Module = ChooseFirst(module, actionDescriptor?.ControllerName, "Application"),
                Controller = actionDescriptor?.ControllerName ?? string.Empty,
                Action = actionDescriptor?.ActionName ?? string.Empty,
                ApiPath = context == null ? string.Empty : $"{context.Request?.Method} {context.Request?.Path.Value}{context.Request?.QueryString.Value}",
                StoredProcedure = ChooseFirst(storedProcedure, ResolveStoredProcedure(exception)),
                Ppno = user?.PPNumber ?? string.Empty,
                UserName = user?.Name ?? context?.User?.Identity?.Name ?? string.Empty,
                Role = user?.UserRoleName ?? string.Empty,
                Entity = user?.UserEntityName ?? string.Empty,
                PageId = sessionHandler?.GetPageId() > 0 ? sessionHandler.GetPageId() : null,
                EngagementId = engagementId,
                TraceId = context?.TraceIdentifier ?? string.Empty,
                IpAddress = _clientIpResolver?.GetClientIp(context) ?? context?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = context?.Request?.Headers["User-Agent"].ToString() ?? string.Empty,
                EnvironmentName = _environment?.EnvironmentName ?? _configuration["ASPNETCORE_ENVIRONMENT"] ?? string.Empty,
                OccurredOnUtc = DateTime.UtcNow
                };
            }

        private static string BuildFingerprint(Exception exception, SystemErrorContext context)
            {
            var parts = new[]
                {
                exception.GetType().FullName,
                ResolveErrorCode(exception),
                context?.Controller,
                context?.Action,
                context?.ApiPath?.Split('?')[0],
                context?.StoredProcedure,
                NormalizeForFingerprint(exception.GetBaseException().Message)
                };

            using var sha = SHA256.Create();
            var material = string.Join("|", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part.Trim().ToUpperInvariant()));
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(material));
            return Convert.ToHexString(hash);
            }

        private static string NormalizeForFingerprint(string message)
            {
            if (string.IsNullOrWhiteSpace(message))
                {
                return string.Empty;
                }

            var normalized = VolatileTokenRegex.Replace(message, "{volatile}");
            normalized = WhitespaceRegex.Replace(normalized, " ").Trim();
            return normalized.Length <= 1000 ? normalized : normalized.Substring(0, 1000);
            }

        private static string ResolveErrorCode(Exception exception)
            {
            var oracleException = exception as OracleException ?? exception.InnerException as OracleException;
            if (oracleException != null)
                {
                return $"ORA-{oracleException.Number}";
                }

            return exception.HResult == 0 ? string.Empty : exception.HResult.ToString("X");
            }

        private static string ResolveStoredProcedure(Exception exception)
            {
            var message = exception?.ToString() ?? string.Empty;
            var match = Regex.Match(message, @"\b(PKG_[A-Z0-9_]+\.[A-Z0-9_]+|P_[A-Z0-9_]+)\b", RegexOptions.IgnoreCase);
            return match.Success ? match.Value : string.Empty;
            }

        private static string BuildTechnicalDetails(Exception exception, SystemErrorContext context)
            {
            var builder = new StringBuilder();
            builder.AppendLine(exception.ToString());
            builder.AppendLine();
            builder.AppendLine("Request context:");
            builder.AppendLine($"TraceId={context.TraceId}");
            builder.AppendLine($"Endpoint={context.ApiPath}");
            builder.AppendLine($"Controller={context.Controller}; Action={context.Action}");
            builder.AppendLine($"PageId={context.PageId}; EngagementId={context.EngagementId}; ParaId={context.ParaId}; ComId={context.ComId}");
            builder.AppendLine($"PPNO={context.Ppno}; Role={context.Role}; Entity={context.Entity}");
            builder.AppendLine($"IP={context.IpAddress}; UserAgent={context.UserAgent}");
            return RedactSensitiveValues(builder.ToString());
            }

        private async Task<bool> SendFirstOccurrenceEmailAsync(DBConnection db, SystemErrorRecord record, Exception exception, SystemErrorContext context, string errorCode, string message)
            {
            var recipients = db.GetSystemErrorNotificationRecipients();
            var details = new[]
                {
                Pair("Error Reference", record.ErrorReference),
                Pair("Date/Time UTC", context.OccurredOnUtc.ToString("u")),
                Pair("Environment", context.EnvironmentName),
                Pair("User Name / PPNO", $"{context.UserName} / {context.Ppno}".Trim(' ', '/')),
                Pair("Role", context.Role),
                Pair("Entity / Context", context.Entity),
                Pair("Page / Controller / API", ChooseFirst(context.ApiPath, $"{context.Controller}/{context.Action}")),
                Pair("Engagement / Para / COM_ID", $"{context.EngagementId} / {context.ParaId} / {context.ComId}".Trim(' ', '/')),
                Pair("Error Type / Code", $"{exception.GetType().FullName} {errorCode}".Trim()),
                Pair("Oracle Package / Procedure", context.StoredProcedure),
                Pair("Error Message", message),
                Pair("Trace ID", context.TraceId),
                Pair("IP / User Agent", $"{context.IpAddress} / {context.UserAgent}".Trim(' ', '/')),
                Pair("First Occurrence", record.FirstOccurrenceUtc.ToString("u")),
                Pair("Occurrence Count", record.OccurrenceCount.ToString())
                };

            return await EmailNotification.SendSystemErrorAlertAsync(
                _configuration,
                nameof(SystemErrorMonitor),
                record.ErrorReference,
                $"IAS Error Alert - {ChooseFirst(context.Module, "Application")} - {record.ErrorReference}",
                message,
                details,
                _serviceProvider,
                recipients.ToRecipients,
                recipients.CcRecipients);
            }

        private static KeyValuePair<string, string> Pair(string key, string value) => new KeyValuePair<string, string>(key, value);

        private static string BuildFallbackReference(string fingerprint)
            {
            return $"IAS-ERR-{(string.IsNullOrWhiteSpace(fingerprint) ? Guid.NewGuid().ToString("N") : fingerprint).Substring(0, 10)}";
            }

        private static string RedactSensitiveValues(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return string.Empty;
                }

            var redacted = Regex.Replace(value, @"(?i)(authorization|bearer|api[-_ ]?key|client[-_ ]?secret|secret|password|token|cookie|connection\s*string|pwd|userid|user\s*id)\s*[:=]\s*[^;\r\n]+", "$1=[REDACTED]");
            redacted = Regex.Replace(redacted, @"(?i)bearer\s+[a-z0-9._~+/=-]+", "Bearer [REDACTED]");
            return redacted;
            }

        private static string ChooseFirst(params string[] values)
            {
            return values?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
            }
        }
    }
