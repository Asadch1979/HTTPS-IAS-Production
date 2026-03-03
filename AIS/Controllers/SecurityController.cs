using AIS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    [ApiController]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [Route("api/security")]
    public class SecurityController : ControllerBase
        {
        private readonly DBConnection _dbConnection;
        private readonly SessionHandler _sessionHandler;
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(DBConnection dbConnection, SessionHandler sessionHandler, ILogger<SecurityController> logger)
            {
            _dbConnection = dbConnection;
            _sessionHandler = sessionHandler;
            _logger = logger;
            }

        [HttpPost("csp-report")]
        [Consumes("application/csp-report", "application/json", "*/*")]
        public async Task<IActionResult> CspReport()
            {
            string rawJson = string.Empty;
            try
                {
                Request.EnableBuffering();
                using (var reader = new StreamReader(Request.Body, leaveOpen: true))
                    {
                    rawJson = await reader.ReadToEndAsync();
                    Request.Body.Position = 0;
                    }

                var payload = ParseCspPayload(rawJson);
                payload.RawJson = rawJson;
                payload.UserAgent = Request.Headers["User-Agent"].ToString();
                payload.ClientIp = ResolveClientIp();

                var createdByPpNo = GetCreatedByPpNo();
                _dbConnection.SaveCspViolation(payload, createdByPpNo);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to persist CSP violation report. Body length: {Length}", rawJson?.Length ?? 0);
                }

            return NoContent();
            }

        private long? GetCreatedByPpNo()
            {
            var user = _sessionHandler.GetUser();
            if (user == null || string.IsNullOrWhiteSpace(user.PPNumber))
                {
                return null;
                }

            if (long.TryParse(user.PPNumber, out var ppNo) && ppNo > 0)
                {
                return ppNo;
                }

            return null;
            }

        private string ResolveClientIp()
            {
            var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
                {
                var firstIp = forwardedFor.Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(firstIp))
                    {
                    return firstIp;
                    }
                }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
            }

        private static CspViolationDto ParseCspPayload(string rawJson)
            {
            if (string.IsNullOrWhiteSpace(rawJson))
                {
                return new CspViolationDto();
                }

            try
                {
                using var document = JsonDocument.Parse(rawJson);
                var root = document.RootElement;
                var reportRoot = root;

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("csp-report", out var cspReportElement) && cspReportElement.ValueKind == JsonValueKind.Object)
                    {
                    reportRoot = cspReportElement;
                    }

                return new CspViolationDto
                    {
                    Disposition = ReadString(reportRoot, "disposition"),
                    DocumentUri = ReadString(reportRoot, "document-uri"),
                    Referrer = ReadString(reportRoot, "referrer"),
                    BlockedUri = ReadString(reportRoot, "blocked-uri"),
                    EffectiveDirective = ReadString(reportRoot, "effective-directive"),
                    ViolatedDirective = ReadString(reportRoot, "violated-directive"),
                    OriginalPolicy = ReadString(reportRoot, "original-policy"),
                    SourceFile = ReadString(reportRoot, "source-file"),
                    LineNumber = ReadNullableLong(reportRoot, "line-number"),
                    ColumnNumber = ReadNullableLong(reportRoot, "column-number"),
                    StatusCode = ReadNullableLong(reportRoot, "status-code"),
                    ScriptSample = ReadString(reportRoot, "script-sample")
                    };
                }
            catch (JsonException)
                {
                return new CspViolationDto();
                }
            }

        private static string ReadString(JsonElement element, string property)
            {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(property, out var valueElement))
                {
                return null;
                }

            return valueElement.ValueKind switch
                {
                JsonValueKind.String => valueElement.GetString(),
                JsonValueKind.Number => valueElement.GetRawText(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => valueElement.GetRawText()
                };
            }

        private static long? ReadNullableLong(JsonElement element, string property)
            {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(property, out var valueElement))
                {
                return null;
                }

            if (valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetInt64(out var longValue))
                {
                return longValue;
                }

            if (valueElement.ValueKind == JsonValueKind.String && long.TryParse(valueElement.GetString(), out var parsed))
                {
                return parsed;
                }

            return null;
            }
        }
    }
