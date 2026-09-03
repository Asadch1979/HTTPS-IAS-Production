using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIS.Middleware
    {
    public class AjaxErrorNotificationMiddleware
        {
        private const string ErrorReferenceHeader = "X-Error-Reference-Id";
        private const string ModelErrorsItemKey = "AjaxModelErrors";
        private static readonly TimeSpan EmailDedupWindow = TimeSpan.FromMinutes(10);
        private static readonly ConcurrentDictionary<string, DateTimeOffset> RecentEmailCache = new ConcurrentDictionary<string, DateTimeOffset>();
        private readonly RequestDelegate _next;
        private readonly ILogger<AjaxErrorNotificationMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public AjaxErrorNotificationMiddleware(
            RequestDelegate next,
            ILogger<AjaxErrorNotificationMiddleware> logger,
            IConfiguration configuration)
            {
            _next = next;
            _logger = logger;
            _configuration = configuration;
            }

        public async Task InvokeAsync(HttpContext context)
            {
            if (context == null)
                {
                return;
                }

            await _next(context);

            if (!ShouldHandle(context.Request))
                {
                return;
                }

            var statusCode = context.Response?.StatusCode ?? StatusCodes.Status200OK;
            if (!ShouldNotify(statusCode))
                {
                return;
                }

            var errorReferenceId = Guid.NewGuid().ToString("N")[..12];
            TryAttachErrorReference(context.Response, errorReferenceId);

            var endpoint = context.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var actionName = actionDescriptor != null
                ? $"{actionDescriptor.ControllerName}.{actionDescriptor.ActionName}"
                : "Unknown";

            var request = context.Request;
            var path = request?.Path.HasValue == true ? request.Path.Value : string.Empty;
            var query = request?.QueryString.HasValue == true ? request.QueryString.Value : string.Empty;
            var endpointPath = string.Concat(path, query);
            var contentType = request?.ContentType ?? string.Empty;
            var acceptHeader = request?.Headers["Accept"].ToString();

            var sessionHandler = context?.RequestServices?.GetService<SessionHandler>();
            var userLabel = ResolveUserLabel(context, sessionHandler);
            var modelErrors = ResolveModelErrors(context);
            var exceptionSummary = ResolveExceptionSummary(context);

            _logger.LogWarning(
                "AJAX error detected. RefId={RefId} Status={StatusCode} Endpoint={Endpoint} Action={Action} User={User} ContentType={ContentType} Accept={Accept} Errors={Errors} Exception={Exception}",
                errorReferenceId,
                statusCode,
                endpointPath,
                actionName,
                userLabel,
                contentType,
                acceptHeader,
                modelErrors,
                exceptionSummary);

            await SendEmailIfAllowedAsync(context, errorReferenceId, statusCode, endpointPath, actionName, userLabel, contentType, modelErrors, exceptionSummary);
            }

        private static bool ShouldHandle(HttpRequest request)
            {
            if (request == null)
                {
                return false;
                }

            var path = request.Path.HasValue ? request.Path.Value : string.Empty;
            if (string.Equals(path, "/.well-known/appspecific/com.chrome.devtools.json", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
                {
                return false;
                }

            if (path.StartsWith("/ApiCalls", StringComparison.OrdinalIgnoreCase))
                {
                return true;
                }

            if (string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                {
                return true;
                }

            var accept = request.Headers["Accept"].ToString();
            return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
            }

        private static bool ShouldNotify(int statusCode)
            {
            return statusCode == StatusCodes.Status400BadRequest
                || statusCode == StatusCodes.Status415UnsupportedMediaType
                || statusCode == StatusCodes.Status302Found
                || statusCode >= StatusCodes.Status500InternalServerError;
            }

        private static void TryAttachErrorReference(HttpResponse response, string errorReferenceId)
            {
            if (response == null || response.HasStarted)
                {
                return;
                }

            if (!response.Headers.ContainsKey(ErrorReferenceHeader))
                {
                response.Headers[ErrorReferenceHeader] = errorReferenceId;
                }
            }

        private static string ResolveUserLabel(HttpContext context, SessionHandler sessionHandler)
            {
            if (sessionHandler != null && sessionHandler.TryGetUser(out var sessionUser))
                {
                var ppNumber = sessionUser?.PPNumber ?? string.Empty;
                var name = sessionUser?.Name ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(ppNumber) && !string.IsNullOrWhiteSpace(name))
                    {
                    return $"{name} ({ppNumber})";
                    }

                if (!string.IsNullOrWhiteSpace(ppNumber))
                    {
                    return ppNumber;
                    }
                }

            return context?.User?.Identity?.Name ?? "Unknown";
            }

        private static IReadOnlyDictionary<string, string[]> ResolveModelErrors(HttpContext context)
            {
            if (context?.Items == null)
                {
                return null;
                }

            if (context.Items.TryGetValue(ModelErrorsItemKey, out var value) && value is IReadOnlyDictionary<string, string[]> errors)
                {
                return errors;
                }

            if (context.Items.TryGetValue(ModelErrorsItemKey, out value) && value is IDictionary<string, string[]> dictErrors)
                {
                return new Dictionary<string, string[]>(dictErrors);
                }

            return null;
            }

        private static string ResolveExceptionSummary(HttpContext context)
            {
            var feature = context?.Features?.Get<IExceptionHandlerFeature>();
            var exception = feature?.Error;
            if (exception == null)
                {
                return string.Empty;
                }

            return exception.GetBaseException().Message;
            }

        private async Task SendEmailIfAllowedAsync(
            HttpContext context,
            string errorReferenceId,
            int statusCode,
            string endpoint,
            string actionName,
            string userLabel,
            string contentType,
            IReadOnlyDictionary<string, string[]> modelErrors,
            string exceptionSummary)
            {
            var dedupKey = $"{statusCode}:{endpoint}:{userLabel}";
            var now = DateTimeOffset.UtcNow;
            if (RecentEmailCache.TryGetValue(dedupKey, out var lastSent) && now - lastSent < EmailDedupWindow)
                {
                _logger.LogInformation("AJAX error email suppressed for RefId={RefId} (dedup key {Key}).", errorReferenceId, dedupKey);
                return;
                }

            RecentEmailCache[dedupKey] = now;

            var validationErrors = new StringBuilder();
            if (modelErrors != null && modelErrors.Count > 0)
                {
                foreach (var entry in modelErrors)
                    {
                    var joined = entry.Value != null ? string.Join("; ", entry.Value) : string.Empty;
                    validationErrors.AppendLine($"{entry.Key}: {joined}");
                    }
                }

            try
                {
                var errorNature = !string.IsNullOrWhiteSpace(exceptionSummary)
                    ? exceptionSummary
                    : $"HTTP {statusCode} returned for AJAX/API request.";
                var result = await AIS.EmailNotification.SendSystemErrorAlertAsync(
                    _configuration,
                    nameof(AjaxErrorNotificationMiddleware),
                    errorReferenceId,
                    $"AJAX/API HTTP {statusCode}",
                    errorNature,
                    new[]
                        {
                        new KeyValuePair<string, string>("Reference", errorReferenceId),
                        new KeyValuePair<string, string>("Status", statusCode.ToString()),
                        new KeyValuePair<string, string>("Endpoint", endpoint),
                        new KeyValuePair<string, string>("Action", actionName),
                        new KeyValuePair<string, string>("User", userLabel),
                        new KeyValuePair<string, string>("Time UTC", now.ToString("u")),
                        new KeyValuePair<string, string>("Content-Type", contentType),
                        new KeyValuePair<string, string>("Validation Errors", validationErrors.ToString()),
                        new KeyValuePair<string, string>("Exception", exceptionSummary)
                        },
                    context?.RequestServices);
                if (!result)
                    {
                    _logger.LogError("AJAX error notification email failed for RefId={RefId}.", errorReferenceId);
                    }
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to send AJAX error notification email for RefId={RefId}.", errorReferenceId);
                }
            }
        }
    }
