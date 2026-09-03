using System;
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
        private readonly RequestDelegate _next;
        private readonly ILogger<AjaxErrorNotificationMiddleware> _logger;

        public AjaxErrorNotificationMiddleware(
            RequestDelegate next,
            ILogger<AjaxErrorNotificationMiddleware> logger)
            {
            _next = next;
            _logger = logger;
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

            await Task.CompletedTask;
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

        }
    }
