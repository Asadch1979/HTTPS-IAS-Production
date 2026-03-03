using AIS.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIS.Controllers
{
    [ApiController]
    [Route("api/security")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
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
        public async Task<IActionResult> CspReport()
        {
            try
            {
                string rawJson;
                using (var reader = new StreamReader(Request.Body))
                {
                    rawJson = await reader.ReadToEndAsync();
                }

                var dto = BuildViolationDto(rawJson);
                dto.RawJson = rawJson;
                dto.UserAgent = Request.Headers["User-Agent"].FirstOrDefault();
                dto.ClientIp = GetClientIp();

                long? createdByPpNo = ResolveCreatedByPpNo();
                _dbConnection.SaveCspViolation(dto, createdByPpNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist CSP violation report.");
            }

            return NoContent();
        }

        private long? ResolveCreatedByPpNo()
        {
            var claimValue = User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "PPNO", StringComparison.OrdinalIgnoreCase))?.Value;
            if (long.TryParse(claimValue, out var claimPpNo))
            {
                return claimPpNo;
            }

            var sessionUser = _sessionHandler?.GetUser();
            if (sessionUser != null && long.TryParse(sessionUser.PPNumber, out var sessionPpNo))
            {
                return sessionPpNo;
            }

            return null;
        }

        private string GetClientIp()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                return forwardedFor.Split(',').FirstOrDefault()?.Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private static CspViolationDto BuildViolationDto(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return new CspViolationDto();
            }

            try
            {
                using (var document = JsonDocument.Parse(rawJson))
                {
                    var root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("csp-report", out var reportNode) && reportNode.ValueKind == JsonValueKind.Object)
                    {
                        root = reportNode;
                    }

                    return new CspViolationDto
                    {
                        Disposition = GetString(root, "disposition"),
                        DocumentUri = GetString(root, "document-uri"),
                        Referrer = GetString(root, "referrer"),
                        BlockedUri = GetString(root, "blocked-uri"),
                        EffectiveDirective = GetString(root, "effective-directive"),
                        ViolatedDirective = GetString(root, "violated-directive"),
                        OriginalPolicy = GetString(root, "original-policy"),
                        SourceFile = GetString(root, "source-file"),
                        LineNumber = GetLong(root, "line-number"),
                        ColumnNumber = GetLong(root, "column-number"),
                        StatusCode = GetLong(root, "status-code"),
                        ScriptSample = GetString(root, "script-sample")
                    };
                }
            }
            catch (JsonException)
            {
                return new CspViolationDto();
            }
        }

        private static string GetString(JsonElement root, string propertyName)
        {
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null
            };
        }

        private static long? GetLong(JsonElement root, string propertyName)
        {
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), out var parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}
