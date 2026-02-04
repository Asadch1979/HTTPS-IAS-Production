using AIS.Models.ManagementReport;
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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    [Route("MANReport")]
    public class ManagementAuditReportPdfController : BaseController
        {
        private readonly ILogger<ManagementAuditReportPdfController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly TopMenus _topMenus;
        private readonly IPermissionService _permissionService;
        private readonly ManagementAuditReportPdfBuilder _pdfBuilder;

        public ManagementAuditReportPdfController(
            ILogger<ManagementAuditReportPdfController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            TopMenus topMenus,
            IPermissionService permissionService,
            ManagementAuditReportPdfBuilder pdfBuilder)
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
        public async Task<IActionResult> GeneratePdf(int? engId)
            {
            var resolvedEngId = 0;
            var totalStopwatch = Stopwatch.StartNew();
            try
                {
                var precheck = EnsureReadyForPdf(engId, out resolvedEngId);
                if (precheck != null)
                    {
                    return precheck;
                    }

                _logger.LogInformation("Generating management audit report PDF for ENG_ID {EngId}.", resolvedEngId);
                var data = _dbConnection.GetManagementAuditReportPdfData(resolvedEngId);
                if (data == null)
                    {
                    return BadRequest("Unable to generate PDF at this time.");
                    }

                var html = _pdfBuilder.BuildHtml(data);
                if (string.IsNullOrWhiteSpace(html))
                    {
                    return StatusCode(500, "PDF content could not be prepared for this report.");
                    }

                var pdfBytes = await RenderPdfWithTimeoutAsync(html, TimeSpan.FromSeconds(60));
                if (pdfBytes == null)
                    {
                    return StatusCode(504, "PDF generation timed out. Please try again.");
                    }

                var filename = BuildFilename(data);
                totalStopwatch.Stop();
                _logger.LogInformation("Management audit PDF request completed in {ElapsedMs} ms for ENG_ID {EngId}.", totalStopwatch.ElapsedMilliseconds, resolvedEngId);
                return File(pdfBytes, "application/pdf", filename);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate management audit report PDF for ENG_ID {EngId}.", resolvedEngId);
                return StatusCode(500, "An error occurred while generating the PDF. Please try again later.");
                }
            }

        private IActionResult EnsureAuthorized()
            {
            ViewData["TopMenu"] = _topMenus.GetTopMenus();
            ViewData["TopMenuPages"] = _topMenus.GetTopMenusPages();

            if (!_sessionHandler.TryGetUser(out _))
                {
                return StatusCode(401, "Session expired. Please sign in again.");
                }

            _ = _permissionService;
            if (!User.Identity.IsAuthenticated)
                {
                return StatusCode(401, "User session is not authenticated.");
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, "User is not authorized to access management audit reports.");
                }

            return null;
            }

        private IActionResult EnsureReadyForPdf(int? requestedEngagementId, out int engId)
            {
            engId = 0;

            var authorizationResult = EnsureAuthorized();
            if (authorizationResult != null)
                {
                return authorizationResult;
                }

            if (requestedEngagementId.HasValue && requestedEngagementId.Value > 0)
                {
                engId = requestedEngagementId.Value;
                }
            else if (!_sessionHandler.TryGetActiveEngagementId(out engId))
                {
                return BadRequest("engId is required.");
                }

            return null;
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
                            }
                        }
                    }

                return output.ToArray();
                }
            }

        private static async Task<byte[]> RenderPdfWithTimeoutAsync(string html, TimeSpan timeout)
            {
            var renderTask = Task.Run(() => RenderPdf(html));
            var completedTask = await Task.WhenAny(renderTask, Task.Delay(timeout));
            if (completedTask != renderTask)
                {
                return null;
                }

            return await renderTask;
            }

        private static string BuildFilename(ManagementAuditPdfReportData data)
            {
            var entityName = data.Cover?.EntityName ?? "Entity";
            var auditPeriod = data.Cover?.AuditedOn ?? "Period";
            var reportTitle = data.Cover?.AuditedBy ?? "Management";
            return $"AuditReport_{SanitizeFilename(reportTitle)}_{SanitizeFilename(entityName)}_{SanitizeFilename(auditPeriod)}.pdf";
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
