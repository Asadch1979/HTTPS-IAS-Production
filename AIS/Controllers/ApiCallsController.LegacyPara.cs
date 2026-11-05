using AIS.Filters;
using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIS.Controllers
{
    // Reuse /ApiCalls action routing with explicit authorization and CSRF for this workflow.
    public partial class ApiCallsController
    {
        private static LegacyParaPage ResolveLegacyParaPage(LegacyParaOperation operation, bool checkerQueue)
        {
            return operation switch
            {
                LegacyParaOperation.Decide => LegacyParaPage.Checker,
                LegacyParaOperation.List or LegacyParaOperation.Detail
                    when checkerQueue => LegacyParaPage.Checker,
                LegacyParaOperation.Submit or LegacyParaOperation.Lookup
                    or LegacyParaOperation.List or LegacyParaOperation.Detail => LegacyParaPage.Maker,
                _ => throw new ArgumentOutOfRangeException(nameof(operation))
            };
        }

        private IActionResult LegacyParaApiFailure(int status, string code, string message) =>
            StatusCode(status, new { Status = false, Code = code, Message = message, Reference = HttpContext.TraceIdentifier });

        private IActionResult CheckLegacyParaAccess(LegacyParaPage page)
        {
            Response.Headers["Cache-Control"] = "no-store";
            Response.Headers["X-Trace-Id"] = HttpContext.TraceIdentifier;
            var user = sessionHandler.GetUser();
            if (User.Identity?.IsAuthenticated != true || user == null || user.ID <= 0)
                return LegacyParaApiFailure(401, "UNAUTHENTICATED", "Your session is missing or expired. Sign in again.");
            if (user.UserRoleID <= 0 || user.UserGroupID <= 0 || user.UserEntityID.GetValueOrDefault() <= 0
                || string.Equals(user.IsActive, "N", StringComparison.OrdinalIgnoreCase)
                || user.UserContextAssignmentId.GetValueOrDefault() <= 0 || string.IsNullOrWhiteSpace(user.PPNumber))
                return LegacyParaApiFailure(403, "CONTEXT_REQUIRED", "Select an active authorized user context.");
            HttpContext.RequestServices.GetRequiredService<IPermissionService>().EnsurePermissionsCached(user);
            // No inherited role or generic Super User shortcuts for legacy posting.
            if (!sessionHandler.TryGetAllowedViewIds(out var allowedPages) || !allowedPages.Contains((int)page))
                return LegacyParaApiFailure(403, "PAGE_FORBIDDEN", "You do not have permission for this page.");
            if (!sessionHandler.HasApiPermissionForPath(Request.Method, Request.PathBase.Value, Request.Path.Value))
                return LegacyParaApiFailure(403, "API_FORBIDDEN", "You do not have permission for this operation.");
            if (!_configuration.GetValue<bool>("LegacyParaAddition:Enabled"))
                return LegacyParaApiFailure(503, "MODULE_DISABLED", "Legacy para addition is not activated.");
            if (!ModelState.IsValid)
                return BadRequest(new { Status = false, Message = "Check the required fields and allowed values." });
            return null;
        }

        private IActionResult ExecuteLegacyParaRequest(LegacyParaOperation operation, object input, bool checkerQueue = false)
        {
            var page = ResolveLegacyParaPage(operation, checkerQueue);
            var accessFailure = CheckLegacyParaAccess(page);
            if (accessFailure != null) return accessFailure;
            if (input == null) return BadRequest(new { Status = false, Message = "A valid request body is required." });
            try
            {
                // The checker queue uses its own read-only procedure.
                var rows = operation == LegacyParaOperation.List
                    ? dBConnection.LegacyParaPending((LegacyParaQuery)input, page)
                    : dBConnection.LegacyParaRequests(operation, input, page);
                if (operation == LegacyParaOperation.Detail)
                    foreach (var row in rows)
                    {
                        if (!row.TryGetValue("payload_json", out var payload) || payload == null) continue;
                        var saved = JsonSerializer.Deserialize<LegacyParaRequest>(payload.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        saved.ObservationHtml = LegacyParaHtml.Sanitize(saved.ObservationHtml);
                        row["observation_html"] = saved.ObservationHtml;
                        row["payload_json"] = JsonSerializer.Serialize(saved, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    }
                if (rows.Count > 0 && rows[0].TryGetValue("request_id", out var id)) HttpContext.Items["LegacyParaRequestId"] = id;
                return Json(new { Status = true, Data = rows, Message = "Request processed." });
            }
            catch (OracleException exception)
            {
                _logger.LogWarning("Legacy para operation {Operation}, page {Page} failed with Oracle code {Code}, trace {Trace}", operation, (int)page, exception.Number, HttpContext.TraceIdentifier);
                // Only fixed, public messages cross the API boundary. Never return Oracle exception text.
                if (exception.Number == 20801) return LegacyParaApiFailure(500, "OPERATION_CONTRACT_MISMATCH", "Legacy para request configuration is inconsistent. Report the reference to support.");
                if (exception.Number == 20813) return LegacyParaApiFailure(409, "CAD_HO_UNSUPPORTED", "CAD/HO historical posting is disabled pending an approved authorization workflow.");
                if (exception.Number == 20814) return LegacyParaApiFailure(409, "ENTITY_SCOPE_CHANGED", "Entity scope cannot be established for this Audit By office. Organizational reassignment requires controlled resolution.");
                if (exception.Number == 20815) return LegacyParaApiFailure(409, "HISTORICAL_FIELDS_REQUIRED", "Enter the historical gist and review annexure details before resubmission.");
                if (exception.Number is 20810 or 20811)
                    return LegacyParaApiFailure(403, "DATABASE_FORBIDDEN", "The selected context or organizational scope is not authorized.");
                if (exception.Number == 20803)
                    return LegacyParaApiFailure(404, "NOT_FOUND", "The request or required reference is unavailable.");
                if (exception.Number == 20812)
                    return LegacyParaApiFailure(503, "MODULE_DISABLED", "Legacy para addition is not activated.");
                if (exception.Number == 20804)
                    return LegacyParaApiFailure(503, "POSTING_UNAVAILABLE", "Posting is unavailable until approved configuration is recorded.");
                if (exception.Number is >= 20800 and <= 20849)
                    return LegacyParaApiFailure(409, "REQUEST_CONFLICT", "The request cannot be processed in its current state. Refresh it and review the supplied details.");
                return LegacyParaApiFailure(500, "REQUEST_FAILED", "The request could not be processed.");
            }
            catch (UnauthorizedAccessException) { return LegacyParaApiFailure(403, "CONTEXT_FORBIDDEN", "The selected user context is unavailable."); }
            catch (Exception exception)
            {
                _logger.LogError("Legacy para operation {Operation} failed ({Type}), trace {Trace}", operation, exception.GetType().Name, HttpContext.TraceIdentifier);
                return LegacyParaApiFailure(500, "REQUEST_FAILED", "The request could not be processed.");
            }
        }

        [HttpPost, RequestSizeLimit(16000000)]
        [Authorize, ValidateAntiForgeryToken(Order = 1001)]
        [ApplicationAudit("LEGACY_PARA_REQUEST_SUBMITTED", "COMPLIANCE", "LEGACY PARA", "PKG_IAS_LEGACY_PARA", "P_EXECUTE", ObjectType = "LEGACY_PARA_REQUEST", ObjectIdItem = "LegacyParaRequestId", ActionNameItem = "LegacyParaAction", RequireResultMessage = true, LogFailures = true)]
        public IActionResult LegacyParaSubmit([FromBody] LegacyParaRequest request)
        {
            if (request == null) return BadRequest();
            request.ParaNo = request.ParaNo?.Trim();
            request.Heading = request.Heading?.Trim();
            if (!Regex.IsMatch(request.ParaNo ?? "", @"^[\p{L}\p{N} ._()/\-]{1,100}$")
                || Regex.IsMatch(request.Heading ?? "", "[<>\\p{Cc}]")
                || request.ResponsiblePersons == null || request.ResponsiblePersons.Count > 100
                || request.Attachments == null || request.Attachments.Count > 10)
                return BadRequest(new { Status = false, Message = "Invalid para details." });
            // Store the complete supported editor HTML, never a truncated gist.
            request.ObservationHtml = LegacyParaHtml.Sanitize(request.ObservationHtml);
            if (string.IsNullOrWhiteSpace(Regex.Replace(request.ObservationHtml, "<[^>]+>", "")))
                return BadRequest(new { Status = false, Message = "Observation text is required." });
            long total = 0;
            foreach (var attachment in request.Attachments)
            {
                byte[] data;
                try { data = Convert.FromBase64String(attachment.Base64 ?? ""); } catch (FormatException) { return BadRequest(); }
                total += data.Length;
                var extension = Path.GetExtension(attachment.Name ?? "").ToLowerInvariant();
                var valid = extension == ".pdf" && data.Length >= 5 && System.Text.Encoding.ASCII.GetString(data,0,5) == "%PDF-"
                    || extension == ".png" && data.Take(8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10})
                    || (extension == ".jpg" || extension == ".jpeg") && data.Length >= 3 && data[0] == 255 && data[1] == 216 && data[2] == 255;
                if (!valid || data.Length > 5000000 || total > 10000000 || attachment.Name != Path.GetFileName(attachment.Name))
                    return BadRequest(new { Status = false, Message = "Use PDF, PNG or JPEG files, up to 5 MB each and 10 MB total." });
                attachment.ContentType = extension == ".pdf" ? "application/pdf" : extension == ".png" ? "image/png" : "image/jpeg";
            }
            HttpContext.Items["LegacyParaAction"] = request.RequestId > 0 ? "LEGACY_PARA_REQUEST_RESUBMITTED" : "LEGACY_PARA_REQUEST_SUBMITTED";
            return ExecuteLegacyParaRequest(LegacyParaOperation.Submit, request);
        }
        [HttpPost, Authorize, ValidateAntiForgeryToken(Order = 1001)]
        public IActionResult LegacyParaList([FromBody] LegacyParaQuery query) =>
            ExecuteLegacyParaRequest(LegacyParaOperation.List, query, query?.Queue == true);
        [HttpPost, Authorize, ValidateAntiForgeryToken(Order = 1001)]
        public IActionResult LegacyParaDetail([FromBody] LegacyParaQuery query) =>
            ExecuteLegacyParaRequest(LegacyParaOperation.Detail, query, query?.Queue == true);
        [HttpPost, Authorize, ValidateAntiForgeryToken(Order = 1001)]
        public IActionResult LegacyParaLookup([FromBody] LegacyParaLookup query) =>
            ExecuteLegacyParaRequest(LegacyParaOperation.Lookup, query);
        [HttpPost, Authorize, ValidateAntiForgeryToken(Order = 1001)]
        [ApplicationAudit("LEGACY_PARA_REQUEST_DECIDED", "COMPLIANCE", "LEGACY PARA", "PKG_IAS_LEGACY_PARA", "P_EXECUTE", ObjectType = "LEGACY_PARA_REQUEST", ObjectIdItem = "LegacyParaRequestId", ActionNameItem = "LegacyParaAction", RequireResultMessage = true, LogFailures = true)]
        public IActionResult LegacyParaDecide([FromBody] LegacyParaAction request)
        {
            if (request == null || request.Action != "AUTHORIZE" && string.IsNullOrWhiteSpace(request.Remarks))
                return BadRequest(new { Status = false, Message = "Remarks are required for return or rejection." });
            HttpContext.Items["LegacyParaRequestId"] = request.RequestId;
            HttpContext.Items["LegacyParaAction"] = "LEGACY_PARA_REQUEST_" + request.Action;
            return ExecuteLegacyParaRequest(LegacyParaOperation.Decide, request);
        }
    }
}
