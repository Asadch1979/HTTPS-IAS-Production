using AIS.Filters;
using AIS.Models;
using AIS.Services;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIS.Controllers
{
    [Authorize, ValidateAntiForgeryToken]
    [Route("LegacyParaRequests/[action]")]
    public sealed class LegacyParaRequestsController : Controller
    {
        private readonly DBConnection db;
        private readonly SessionHandler session;
        private readonly IPermissionService permissions;
        private readonly IConfiguration configuration;
        private readonly ILogger<LegacyParaRequestsController> logger;
        public LegacyParaRequestsController(DBConnection db, SessionHandler session,
            IPermissionService permissions, IConfiguration configuration, ILogger<LegacyParaRequestsController> logger)
        { this.db = db; this.session = session; this.permissions = permissions; this.configuration = configuration; this.logger = logger; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Response.Headers["Cache-Control"] = "no-store";
            var user = session.GetUser();
            var action = context.ActionDescriptor.RouteValues["action"];
            var query = context.ActionArguments.Values.OfType<LegacyParaQuery>().FirstOrDefault();
            var page = action == nameof(Decide) || query?.Queue == true ? 193 : 192;
            HttpContext.Items["LegacyParaPage"] = page;
            if (User.Identity?.IsAuthenticated != true || user == null || user.ID <= 0
                || user.UserRoleID <= 0 || user.UserEntityID.GetValueOrDefault() <= 0
                || string.Equals(user.IsActive, "N", StringComparison.OrdinalIgnoreCase)
                || user.UserContextAssignmentId.GetValueOrDefault() <= 0 || string.IsNullOrWhiteSpace(user.PPNumber))
                context.Result = Unauthorized(new { Status = false, Message = "Select an active user context." });
            else if (!permissions.HasViewPermission(user, page)) context.Result = Forbid();
            else if (!permissions.HasApiPermissionForPath(user, Request.Method, Request.PathBase.Value, Request.Path.Value)) context.Result = Forbid();
            else if (!configuration.GetValue<bool>("LegacyParaAddition:Enabled"))
                context.Result = StatusCode(503, new { Status = false, Message = "Legacy para addition is not activated." });
            else if (!ModelState.IsValid) context.Result = BadRequest(new { Status = false, Message = "Check the required fields and allowed values." });
            base.OnActionExecuting(context);
        }

        private IActionResult Execute(string operation, object input)
        {
            try
            {
                var rows = db.LegacyParaRequests(operation, input, (int)HttpContext.Items["LegacyParaPage"]);
                if (operation == "DETAIL")
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
                logger.LogWarning("Legacy para operation {Operation} failed, Oracle code {Code}, trace {Trace}", operation, exception.Number, HttpContext.TraceIdentifier);
                var message = exception.Number is >= 20800 and <= 20849
                    ? exception.Message.Split('\n')[0].Replace("ORA-" + exception.Number + ": ", "")
                    : "The request could not be processed. Reference: " + HttpContext.TraceIdentifier;
                return StatusCode(exception.Number is >= 20800 and <= 20849 ? 409 : 500, new { Status = false, Message = message });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception exception)
            {
                logger.LogError("Legacy para operation {Operation} failed ({Type}), trace {Trace}", operation, exception.GetType().Name, HttpContext.TraceIdentifier);
                return StatusCode(500, new { Status = false, Message = "Unable to process request. Reference: " + HttpContext.TraceIdentifier });
            }
        }

        [HttpPost, RequestSizeLimit(16000000)]
        [ApplicationAudit("LEGACY_PARA_REQUEST_SUBMITTED", "COMPLIANCE", "LEGACY PARA", "PKG_IAS_LEGACY_PARA", "P_EXECUTE", ObjectType = "LEGACY_PARA_REQUEST", ObjectIdItem = "LegacyParaRequestId", ActionNameItem = "LegacyParaAction", RequireResultMessage = true, LogFailures = true)]
        public IActionResult Submit([FromBody] LegacyParaRequest request)
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
            return Execute("SUBMIT", request);
        }
        [HttpPost] public IActionResult List([FromBody] LegacyParaQuery query) => Execute("LIST", query);
        [HttpPost] public IActionResult Detail([FromBody] LegacyParaQuery query) => Execute("DETAIL", query);
        [HttpPost] public IActionResult Lookup([FromBody] LegacyParaLookup query) => Execute("LOOKUP", query);
        [HttpPost]
        [ApplicationAudit("LEGACY_PARA_REQUEST_DECIDED", "COMPLIANCE", "LEGACY PARA", "PKG_IAS_LEGACY_PARA", "P_EXECUTE", ObjectType = "LEGACY_PARA_REQUEST", ObjectIdItem = "LegacyParaRequestId", ActionNameItem = "LegacyParaAction", RequireResultMessage = true, LogFailures = true)]
        public IActionResult Decide([FromBody] LegacyParaAction request)
        {
            if (request == null || request.Action != "AUTHORIZE" && string.IsNullOrWhiteSpace(request.Remarks))
                return BadRequest(new { Status = false, Message = "Remarks are required for return or rejection." });
            HttpContext.Items["LegacyParaRequestId"] = request.RequestId;
            HttpContext.Items["LegacyParaAction"] = "LEGACY_PARA_REQUEST_" + request.Action;
            return Execute("DECIDE", request);
        }
    }
}
