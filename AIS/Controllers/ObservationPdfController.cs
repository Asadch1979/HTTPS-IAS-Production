using AIS.Models;
using AIS.Services;
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace AIS.Controllers
    {
    [Route("Observation")]
    public class ObservationPdfController : BaseController
        {
        private readonly ILogger<ObservationPdfController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly TopMenus _topMenus;
        private readonly IPermissionService _permissionService;

        public ObservationPdfController(
            ILogger<ObservationPdfController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            TopMenus topMenus,
            IPermissionService permissionService)
            : base(sessionHandler)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _topMenus = topMenus;
            _permissionService = permissionService;
            }

        [HttpGet("GeneratePdf")]
        public IActionResult GeneratePdf(int obsId)
            {
            if (obsId <= 0)
                {
                return BadRequest("Observation id is required.");
                }

            var engId = 0;
            try
                {
                var precheck = EnsureReadyForPdf(obsId, out engId);
                if (precheck != null)
                    {
                    return precheck;
                    }

                var details = _dbConnection.GetObservationDetailsForReport(obsId);
                if (details == null || details.Count == 0)
                    {
                    return BadRequest("Observation details are unavailable.");
                    }

                var observation = details.First();
                var html = BuildObservationHtml(observation, obsId);
                var pdfBytes = RenderPdf(html);
                var filename = BuildFilename(observation, obsId);
                return File(pdfBytes, "application/pdf", filename);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate observation PDF for OBS_ID {ObsId} and ENG_ID {EngId}.", obsId, engId);
                return BadRequest("An error occurred while generating the PDF. Please try again later.");
                }
            }

        private IActionResult EnsureAuthorized()
            {
            var (_, errorResult) = GetUserOr401();
            if (errorResult != null)
                {
                return errorResult;
                }

            _topMenus.GetTopMenus();
            _ = _permissionService;
            if (!User.Identity.IsAuthenticated)
                {
                return Unauthorized(new { message = "User session is not authenticated." });
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return Unauthorized(new { message = "User is not authorized to access observation PDFs." });
                }

            return null;
            }

        private IActionResult EnsureReadyForPdf(int obsId, out int engId)
            {
            engId = 0;

            var authorizationResult = EnsureAuthorized();
            if (authorizationResult != null)
                {
                return authorizationResult;
                }

            if (!_sessionHandler.TryGetActiveEngagementId(out engId))
                {
                return BadRequest("An active engagement is required before generating the observation PDF.");
                }

            if (!IsObservationAuthorized(engId, obsId))
                {
                return Unauthorized(new { message = "You are not authorized to access this observation." });
                }

            return null;
            }

        private bool IsObservationAuthorized(int engId, int obsId)
            {
            return _dbConnection
                .GetManagedObservations(engId, obsId)
                .Any();
            }

        private static string BuildObservationHtml(FADAuditParasReviewModel observation, int obsId)
            {
            var memoNo = EncodeText(observation.MEMO_NO);
            var paraNo = EncodeText(observation.PARA_NO);
            var annex = EncodeText(observation.ANNEX);
            var process = EncodeText(observation.PROCESS);
            var subProcess = EncodeText(observation.SUB_PROCESS);
            var checklist = EncodeText(observation.CHECK_LIST);
            var gist = EncodeText(observation.OBS_GIST);
            var amount = EncodeText(observation.AMOUNT_INV);
            var instances = EncodeText(observation.NO_INSTANCES);
            var ppNo = EncodeText(observation.PPNO);
            var respRole = EncodeText(observation.RESP_ROLE);
            var respAmount = EncodeText(observation.RESP_AMOUNT);
            var auditorComments = EncodeText(observation.AUDITOR_COMMENTS);
            var headComments = EncodeText(observation.HEADCOMMENTS);
            var rootCause = EncodeText(observation.ROOT_CAUSE);
            var paraText = observation.PARA_TEXT;
            var auditeeReply = observation.AUDITEE_REPLY;

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head><meta charset=\"utf-8\" />");
            html.AppendLine("<style>");
            html.AppendLine("body{font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#111;}");
            html.AppendLine(".header{margin-bottom:16px;}");
            html.AppendLine(".header h2{margin:0 0 4px 0;color:#be0e43;}");
            html.AppendLine(".meta{margin-bottom:12px;}");
            html.AppendLine(".meta span{display:inline-block;margin-right:16px;}");
            html.AppendLine(".section{margin-bottom:12px;}");
            html.AppendLine(".label{font-weight:bold;margin-bottom:4px;}");
            html.AppendLine(".value{border:1px solid #ddd;padding:8px;border-radius:4px;}");
            html.AppendLine("</style></head><body>");
            html.AppendLine("<div class=\"header\"><h2>Observation Report</h2>");
            html.AppendLine($"<div class=\"meta\"><span><strong>Observation ID:</strong> {obsId}</span><span><strong>Memo No:</strong> {memoNo}</span><span><strong>Para No:</strong> {paraNo}</span></div></div>");
            html.AppendLine("<div class=\"section\"><div class=\"label\">Observation Details</div>");
            html.AppendLine("<div class=\"value\">");
            html.AppendLine($"<p><strong>Annexure:</strong> {annex}</p>");
            html.AppendLine($"<p><strong>Process:</strong> {process}</p>");
            html.AppendLine($"<p><strong>Sub Process:</strong> {subProcess}</p>");
            html.AppendLine($"<p><strong>Checklist:</strong> {checklist}</p>");
            html.AppendLine($"<p><strong>Gist:</strong> {gist}</p>");
            html.AppendLine($"<p><strong>Amount Involved:</strong> {amount}</p>");
            html.AppendLine($"<p><strong>No. of Instances:</strong> {instances}</p>");
            html.AppendLine("</div></div>");
            html.AppendLine("<div class=\"section\"><div class=\"label\">Para Text</div>");
            html.AppendLine($"<div class=\"value\">{FormatRichText(paraText)}</div></div>");
            html.AppendLine("<div class=\"section\"><div class=\"label\">Responsible Details</div>");
            html.AppendLine("<div class=\"value\">");
            html.AppendLine($"<p><strong>PP No:</strong> {ppNo}</p>");
            html.AppendLine($"<p><strong>Role:</strong> {respRole}</p>");
            html.AppendLine($"<p><strong>Amount:</strong> {respAmount}</p>");
            html.AppendLine("</div></div>");
            html.AppendLine("<div class=\"section\"><div class=\"label\">Auditee Reply</div>");
            html.AppendLine($"<div class=\"value\">{FormatRichText(auditeeReply)}</div></div>");
            html.AppendLine("<div class=\"section\"><div class=\"label\">Auditor Comments</div>");
            html.AppendLine($"<div class=\"value\">{auditorComments}</div></div>");
            html.AppendLine("<div class=\"section\"><div class=\"label\">SVP AZ Reply</div>");
            html.AppendLine($"<div class=\"value\">{headComments}</div></div>");
            html.AppendLine("<div class=\"section\"><div class=\"label\">Root Cause / Recommendation</div>");
            html.AppendLine($"<div class=\"value\">{rootCause}</div></div>");
            html.AppendLine("</body></html>");
            return html.ToString();
            }

        private static string FormatRichText(string value)
            {
            return string.IsNullOrWhiteSpace(value) ? "<em>N/A</em>" : value;
            }

        private static string EncodeText(string value)
            {
            return WebUtility.HtmlEncode(value ?? string.Empty);
            }

        private static byte[] RenderPdf(string html)
            {
            using (var output = new System.IO.MemoryStream())
                {
                using (var writer = new PdfWriter(output))
                    {
                    using (var pdf = new PdfDocument(writer))
                        {
                        pdf.SetDefaultPageSize(PageSize.A4);
                        var converterProperties = new ConverterProperties();
                        using (Document document = HtmlConverter.ConvertToDocument(html, pdf, converterProperties))
                            {
                            document.Close();
                            }
                        }
                    }

                return output.ToArray();
                }
            }

        private static string BuildFilename(FADAuditParasReviewModel observation, int obsId)
            {
            var memo = SanitizeFilename(observation.MEMO_NO);
            var para = SanitizeFilename(observation.PARA_NO);
            return $"Observation_{memo}_{para}_OBS_{obsId}.pdf";
            }

        private static string SanitizeFilename(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return string.Empty;
                }

            var builder = new StringBuilder(value.Trim());
            foreach (var invalidChar in System.IO.Path.GetInvalidFileNameChars())
                {
                builder.Replace(invalidChar, '_');
                }

            return builder.ToString().Replace(' ', '_');
            }
        }
    }
