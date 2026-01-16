using AIS.Models.FieldAuditReport;
using AIS.Services;
using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace AIS.Controllers
    {
    [Route("FieldAuditReport")]
    public class FieldAuditReportPdfController : BaseController
        {
        private readonly ILogger<FieldAuditReportPdfController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly TopMenus _topMenus;
        private readonly IPermissionService _permissionService;
        private readonly FieldAuditReportPdfBuilder _pdfBuilder;

        public FieldAuditReportPdfController(
            ILogger<FieldAuditReportPdfController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            TopMenus topMenus,
            IPermissionService permissionService,
            FieldAuditReportPdfBuilder pdfBuilder)
            : base(sessionHandler)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _topMenus = topMenus;
            _permissionService = permissionService;
            _pdfBuilder = pdfBuilder;
            }

        [HttpGet("GeneratePdf")]
        public IActionResult GeneratePdf(int? reportVersion = null)
            {
            var engId = 0;
            try
                {
                var precheck = EnsureReadyForPdf(out engId);
                if (precheck != null)
                    {
                    return precheck;
                    }

                var data = _dbConnection.GetFieldAuditReportPdfData(engId, reportVersion);
                if (data == null)
                    {
                    return BadRequest("Unable to generate PDF at this time.");
                    }

                var missingMandatory = data.Sections.Any(section =>
                    section.IsMandatory && string.IsNullOrWhiteSpace(section.HtmlContent));
                if (missingMandatory)
                    {
                    return BadRequest("Mandatory sections are missing. Please complete all required sections before generating the PDF.");
                    }

                var html = _pdfBuilder.BuildHtml(data);
                var pdfBytes = RenderPdf(html);
                var filename = BuildFilename(data);
                return File(pdfBytes, "application/pdf", filename);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate field audit report PDF for ENG_ID {EngId}.", engId);
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
                return Unauthorized(new { message = "User is not authorized to access field audit reports." });
                }

            return null;
            }

        private IActionResult EnsureReadyForPdf(out int engId)
            {
            engId = 0;

            var authorizationResult = EnsureAuthorized();
            if (authorizationResult != null)
                {
                return authorizationResult;
                }

            if (!_sessionHandler.TryGetActiveEngagementId(out engId))
                {
                return BadRequest("An active engagement is required before generating the report.");
                }

            if (!IsEngagementAuthorized(engId))
                {
                return BadRequest("You are not authorized to generate a report for the selected engagement.");
                }

            var overview = _dbConnection.GetFieldAuditReportOverview(engId);
            if (overview == null)
                {
                return BadRequest("Report data is not available for the selected engagement.");
                }

            if (!_dbConnection.IsFieldAuditReportFinal(engId))
                {
                return BadRequest("Report must be finalized before PDF generation.");
                }

            return null;
            }

        private bool IsEngagementAuthorized(int engId)
            {
            return _dbConnection
                .GetObservationEntitiesForPreConcluding()
                .Any(item => item.ENG_ID.HasValue && item.ENG_ID.Value == engId);
            }

        private static byte[] RenderPdf(string html)
            {
            using (var output = new MemoryStream())
                {
                using (var writer = new PdfWriter(output))
                    {
                    using (var pdf = new PdfDocument(writer))
                        {
                        pdf.SetDefaultPageSize(PageSize.A4);
                        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageNumberEventHandler());
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

        private static string BuildFilename(FieldAuditPdfReportData data)
            {
            var entityName = data.ReportMeta?.EntityName ?? data.Header?.EntityName ?? "Entity";
            var auditPeriod = data.ReportMeta?.AuditPeriod ?? data.Header?.AuditPeriod ?? "Period";
            var version = data.ReportMeta?.VersionNumber ?? data.Header?.VersionNumber ?? "1";
            return $"AuditReport_{SanitizeFilename(entityName)}_{SanitizeFilename(auditPeriod)}_v{SanitizeFilename(version)}.pdf";
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

        private sealed class PageNumberEventHandler : IEventHandler
            {
            public void HandleEvent(Event @event)
                {
                var docEvent = (PdfDocumentEvent)@event;
                var pdf = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var pageNumber = pdf.GetPageNumber(page);
                var pageSize = pdf.GetDefaultPageSize();
                var pdfCanvas = new PdfCanvas(page);
                using (var canvas = new Canvas(pdfCanvas, pageSize))
                    {
                    var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                    canvas.SetFont(font);
                    canvas.SetFontSize(9);
                    var text = $"Page {pageNumber}";
                    var x = pageSize.GetWidth() / 2;
                    var y = pageSize.GetBottom() + 20;
                    canvas.ShowTextAligned(text, x, y, TextAlignment.CENTER);
                    }
                }
            }
        }
    }
