using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using AIS.Utilities;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIS.Filters
{
    /// <summary>
    /// Ensures POST requests cannot bypass ModelState validation.
    /// </summary>
    public class PostModelValidationFilter : IActionFilter
    {
        private static readonly Regex SensitiveValueRegex = new Regex(
            @"(?i)\b(authorization|cookie|password|secret|token)\b\s*[:=]\s*\S+",
            RegexOptions.Compiled);
        private readonly ILogger<PostModelValidationFilter> _logger;

        public PostModelValidationFilter(ILogger<PostModelValidationFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                return;
            }

            if (context.ModelState.IsValid)
            {
                return;
            }

            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            // Only explicitly opted-in form actions may rebuild their GET model.
            // They must check ModelState before any mutation.
            if (descriptor?.MethodInfo.IsDefined(typeof(HandlesInvalidPostModelAttribute), true) == true)
                return;

            var endpointName = descriptor?.ActionName ?? context.ActionDescriptor?.DisplayName ?? "Unknown";
            LogStructuredValidationFailure(context, endpointName);
            // fetch() need not send X-Requested-With. Processing endpoints have no
            // implied Razor view; use the established structured failure contract.
            context.Result = new BadRequestObjectResult(
                ValidationErrorHelper.BuildInvalidRequestResponse(context.ModelState,
                    "Please correct the submitted values and try again."));
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private void LogStructuredValidationFailure(ActionExecutingContext context, string endpointName)
        {
            var request = context.HttpContext.Request;
            var errors = ValidationErrorHelper.BuildModelErrors(context.ModelState)
                .ToDictionary(
                    item => SanitizeLogValue(item.Key, 128),
                    item => item.Value
                        .Select(message => SanitizeLogValue(
                            string.IsNullOrWhiteSpace(message) ? "The submitted value is invalid." : message,
                            500))
                        .ToArray());

            int? fileCount = null;
            long? aggregateFileSizeBytes = null;
            try
            {
                if (request.HasFormContentType)
                {
                    var files = request.Form.Files;
                    fileCount = files.Count;
                    aggregateFileSizeBytes = files.Sum(file => file.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(
                    "Unable to read form-file metadata for validation trace {TraceId}. ExceptionType={ExceptionType}.",
                    context.HttpContext.TraceIdentifier,
                    ex.GetType().FullName);
            }

            _logger.LogWarning(
                "Validation failed for {Endpoint}. TraceId={TraceId}; Fields={Fields}; Errors={@Errors}; FileCount={FileCount}; AggregateFileSizeBytes={AggregateFileSizeBytes}.",
                endpointName,
                context.HttpContext.TraceIdentifier,
                errors.Keys.ToArray(),
                errors,
                fileCount,
                aggregateFileSizeBytes);
        }

        private static string SanitizeLogValue(string value, int maxLength)
        {
            var normalized = Regex.Replace(value ?? string.Empty, @"[\r\n\t]+", " ").Trim();
            normalized = SensitiveValueRegex.Replace(normalized, "$1=[REDACTED]");
            return normalized.Length <= maxLength ? normalized : normalized.Substring(0, maxLength);
        }
    }
}
