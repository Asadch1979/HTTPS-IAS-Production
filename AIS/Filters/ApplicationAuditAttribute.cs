using AIS.Models;
using AIS.Services;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIS.Filters
    {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ApplicationAuditAttribute : TypeFilterAttribute
        {
        private static readonly HashSet<string> ApprovedCategories = new(StringComparer.Ordinal)
            {
            "AUTHENTICATION", "AUDIT_PLANNING", "AUDIT_EXECUTION", "CHECKLIST", "COMPLIANCE",
            "AUDIT_REPORT", "ANNEXURE", "ADMINISTRATION", "FILES_EVIDENCE", "INQUIRY_COMPLAINT", "EXPORT"
            };

        public ApplicationAuditAttribute(string actionName, string actionCategory, string moduleName,
            string dbPackageName, string dbProcedureName)
            : base(typeof(ApplicationAuditActionFilter))
            {
            if (string.IsNullOrWhiteSpace(actionName)) throw new ArgumentException("Audit action name is required.", nameof(actionName));
            if (!ApprovedCategories.Contains(actionCategory)) throw new ArgumentException($"Unsupported audit category: {actionCategory}", nameof(actionCategory));
            if (string.IsNullOrWhiteSpace(moduleName)) throw new ArgumentException("Audit module name is required.", nameof(moduleName));
            Arguments = new object[] { actionName, actionCategory, moduleName, dbPackageName, dbProcedureName };
            }

        public string EventType { get; set; } = "BUSINESS_ACTION";
        public string EngagementId { get; set; }
        public string ParaId { get; set; }
        public string OldParaId { get; set; }
        public string NewParaId { get; set; }
        public string ComId { get; set; }
        public string ObjectType { get; set; }
        public string ObjectId { get; set; }
        public string ObjectIdItem { get; set; }
        public string Details { get; set; }
        public string RequireNonEmpty { get; set; }
        public string SuccessMessageContains { get; set; }
        public string FailureMessageContains { get; set; }
        public bool RequireResultMessage { get; set; }
        }

    public sealed class ApplicationAuditActionFilter : IAsyncActionFilter
        {
        private readonly IApplicationAuditLogger _auditLogger;
        private readonly string _actionName;
        private readonly string _actionCategory;
        private readonly string _moduleName;
        private readonly string _dbPackageName;
        private readonly string _dbProcedureName;

        public ApplicationAuditActionFilter(IApplicationAuditLogger auditLogger, string actionName,
            string actionCategory, string moduleName, string dbPackageName, string dbProcedureName)
            {
            _auditLogger = auditLogger;
            _actionName = actionName;
            _actionCategory = actionCategory;
            _moduleName = moduleName;
            _dbPackageName = dbPackageName;
            _dbProcedureName = dbProcedureName;
            }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
            var stopwatch = Stopwatch.StartNew();
            var executed = await next();
            stopwatch.Stop();
            if (executed.Exception != null || !IsConfirmedSuccess(executed.Result, context.HttpContext.Response.StatusCode, out var code, out var message))
                return;

            var metadata = context.ActionDescriptor.EndpointMetadata;
            var attribute = FindAttribute(metadata);
            if (attribute == null)
                return;
            if (!string.IsNullOrWhiteSpace(attribute.RequireNonEmpty) && IsEmpty(ResolveValue(context.ActionArguments, attribute.RequireNonEmpty)))
                return;
            if (attribute.RequireResultMessage && string.IsNullOrWhiteSpace(message))
                return;
            if (!string.IsNullOrWhiteSpace(attribute.SuccessMessageContains)
                && !ContainsAny(message, attribute.SuccessMessageContains))
                return;
            if (!string.IsNullOrWhiteSpace(attribute.FailureMessageContains)
                && ContainsAny(message, attribute.FailureMessageContains))
                return;

            await _auditLogger.LogSuccessAsync(new ApplicationAuditEvent
                {
                EventType = attribute.EventType,
                ActionName = _actionName,
                ActionCategory = _actionCategory,
                ModuleName = _moduleName,
                DbPackageName = _dbPackageName,
                DbProcedureName = _dbProcedureName,
                EngagementId = ResolveLong(context.ActionArguments, attribute.EngagementId),
                ParaId = ResolveLong(context.ActionArguments, attribute.ParaId),
                OldParaId = ResolveLong(context.ActionArguments, attribute.OldParaId),
                NewParaId = ResolveLong(context.ActionArguments, attribute.NewParaId),
                ComId = ResolveLong(context.ActionArguments, attribute.ComId),
                ObjectType = attribute.ObjectType,
                ObjectId = !string.IsNullOrWhiteSpace(attribute.ObjectIdItem)
                    ? context.HttpContext.Items[attribute.ObjectIdItem]?.ToString()
                    : ResolveValue(context.ActionArguments, attribute.ObjectId)?.ToString(),
                ResultCode = code,
                ResultMessage = message,
                Details = ApplicationAuditLogger.Truncate(attribute.Details, 4000)
                }, stopwatch.ElapsedMilliseconds);
            }

        private static ApplicationAuditAttribute FindAttribute(IList<object> metadata)
            {
            foreach (var item in metadata)
                if (item is ApplicationAuditAttribute value) return value;
            return null;
            }

        private static bool IsConfirmedSuccess(IActionResult result, int responseStatus, out string code, out string message)
            {
            code = null;
            message = null;
            if (responseStatus >= 400 || result is BadRequestResult or UnauthorizedResult or ForbidResult or NotFoundResult)
                return false;
            if (result is StatusCodeResult status && status.StatusCode >= 400)
                return false;

            object value = result switch
                {
                ObjectResult objectResult when objectResult.StatusCode.GetValueOrDefault(200) < 400 => objectResult.Value,
                JsonResult jsonResult when jsonResult.StatusCode.GetValueOrDefault(200) < 400 => jsonResult.Value,
                _ => null
                };

            if (value == null)
                return result is RedirectResult or RedirectToActionResult or LocalRedirectResult or FileResult or OkResult;

            try
                {
                var json = value is string text ? text : JsonSerializer.Serialize(value);
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                    {
                    if (TryProperty(document.RootElement, "status", out var statusValue))
                        {
                        if (statusValue.ValueKind == JsonValueKind.False) return false;
                        if (statusValue.ValueKind == JsonValueKind.String && !IsSuccessText(statusValue.GetString())) return false;
                        }
                    if (TryProperty(document.RootElement, "success", out var successValue) && successValue.ValueKind == JsonValueKind.False)
                        return false;
                    if (TryProperty(document.RootElement, "resultCode", out var codeValue)) code = Scalar(codeValue);
                    if (TryProperty(document.RootElement, "message", out var messageValue)) message = Scalar(messageValue);
                    }
                return true;
                }
            catch (JsonException)
                {
                // Plain scalar responses are only accepted when they carry an explicit success marker.
                return value is string scalar && IsSuccessText(scalar);
                }
            }

        private static bool IsSuccessText(string value) => !string.IsNullOrWhiteSpace(value)
            && (value.Contains("\"Status\":true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("OK", StringComparison.OrdinalIgnoreCase)
                || value.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase));

        private static bool TryProperty(JsonElement element, string name, out JsonElement value)
            {
            foreach (var property in element.EnumerateObject())
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) { value = property.Value; return true; }
            value = default;
            return false;
            }

        private static string Scalar(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();

        private static long? ResolveLong(IDictionary<string, object> args, string path)
            => long.TryParse(ResolveValue(args, path)?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) && number > 0 ? number : null;

        private static object ResolveValue(IDictionary<string, object> args, string path)
            {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (!args.TryGetValue(parts[0], out var value))
                foreach (var pair in args)
                    if (pair.Key.Equals(parts[0], StringComparison.OrdinalIgnoreCase)) { value = pair.Value; break; }
            for (var i = 1; value != null && i < parts.Length; i++)
                value = value.GetType().GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(value);
            return value;
            }

        private static bool IsEmpty(object value)
            {
            if (value == null) return true;
            if (value is string text) return string.IsNullOrWhiteSpace(text);
            if (value is ICollection collection) return collection.Count == 0;
            return false;
            }

        private static bool ContainsAny(string value, string alternatives)
            {
            if (string.IsNullOrWhiteSpace(value)) return false;
            foreach (var alternative in alternatives.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (value.Contains(alternative, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
            }
        }
    }
