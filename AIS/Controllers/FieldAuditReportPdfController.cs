using AIS.Models.FieldAuditReport;
using AIS.Services;
using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Extgstate;
using iText.Layout;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GeneratePdf(int? engId, int? reportVersion = null)
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

                _logger.LogInformation("Generating field audit report PDF for ENG_ID {EngId} reportVersion {ReportVersion}.", resolvedEngId, reportVersion);

                var dataStopwatch = Stopwatch.StartNew();
                var data = _dbConnection.GetFieldAuditReportPdfData(resolvedEngId, reportVersion);
                dataStopwatch.Stop();
                _logger.LogInformation("Field audit PDF data retrieval completed in {ElapsedMs} ms for ENG_ID {EngId}.", dataStopwatch.ElapsedMilliseconds, resolvedEngId);
                if (data == null)
                    {
                    return BadRequest("Unable to generate PDF at this time.");
                    }

                PopulatePdfGeneratorIdentity(data);

                var html = _pdfBuilder.BuildHtml(data);
                var htmlLength = html?.Length ?? 0;
                _logger.LogInformation("Field audit PDF HTML length {HtmlLength} for ENG_ID {EngId}.", htmlLength, resolvedEngId);
                if (htmlLength == 0)
                    {
                    return StatusCode(500, "PDF content could not be prepared for this report.");
                    }

                var pdfStopwatch = Stopwatch.StartNew();
                var watermarkTexts = BuildWatermarkTexts(data, resolvedEngId);
                var pdfBytes = await RenderPdfWithTimeoutAsync(html, TimeSpan.FromSeconds(60), watermarkTexts);
                pdfStopwatch.Stop();
                if (pdfBytes == null)
                    {
                    _logger.LogWarning("Field audit PDF generation timed out after {ElapsedMs} ms for ENG_ID {EngId}.", pdfStopwatch.ElapsedMilliseconds, resolvedEngId);
                    return StatusCode(504, "PDF generation timed out. Please try again.");
                    }

                _logger.LogInformation("Field audit PDF generated in {ElapsedMs} ms with {PdfBytes} bytes for ENG_ID {EngId}.", pdfStopwatch.ElapsedMilliseconds, pdfBytes.Length, resolvedEngId);
                if (pdfBytes.Length == 0)
                    {
                    return StatusCode(500, "Generated PDF is empty. Please try again.");
                    }

                var filename = BuildFilename(data);
                totalStopwatch.Stop();
                _logger.LogInformation("Field audit PDF request completed in {ElapsedMs} ms for ENG_ID {EngId}.", totalStopwatch.ElapsedMilliseconds, resolvedEngId);
                return File(pdfBytes, "application/pdf", filename);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate field audit report PDF for ENG_ID {EngId}.", resolvedEngId);
                return StatusCode(500, "An error occurred while generating the PDF. Please try again later.");
                }
            }

        [HttpGet("Engagements")]
        public IActionResult Engagements()
            {
            var (user, redirectResult) = GetUserOrRedirect();
            if (redirectResult != null)
                {
                return redirectResult;
                }

            var allowedRoles = new[] { 1, 2, 15, 16 };
            if (!allowedRoles.Contains(user.UserRoleID))
                {
                return StatusCode(403, "User is not authorized to access PDF engagements.");
                }

            ViewData["TopMenu"] = _topMenus.GetTopMenus();
            ViewData["TopMenuPages"] = _topMenus.GetTopMenusPages();

            if (!int.TryParse(user.PPNumber, out var ppNo))
                {
                return BadRequest("Invalid user session for PDF engagements.");
                }

            var rows = _dbConnection.GetAllowedPdfEngagementDetails(ppNo, user.UserRoleID, user.UserEntityID ?? 0);
            return View("~/Views/FieldAuditReport/Engagements.cshtml", rows);
            }

        private IActionResult EnsureAuthorized()
            {
            if (!_sessionHandler.TryGetUser(out _))
                {
                return StatusCode(401, "Session expired. Please sign in again.");
                }

            _topMenus.GetTopMenus();
            _ = _permissionService;
            if (!User.Identity.IsAuthenticated)
                {
                return StatusCode(401, "User session is not authenticated.");
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, "User is not authorized to access field audit reports.");
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

            if (!requestedEngagementId.HasValue || requestedEngagementId.Value <= 0)
                {
                return BadRequest("engId is required.");
                }

            engId = requestedEngagementId.Value;

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
            return _dbConnection.IsEngagementAllowedForPdf(engId);
            }

        private static byte[] RenderPdf(string html, PdfWatermarkText watermarkTexts)
            {
            using (var output = new MemoryStream())
                {
                using (var writer = new PdfWriter(output))
                    {
                    using (var pdf = new PdfDocument(writer))
                        {
                        pdf.SetDefaultPageSize(PageSize.A4);
                        pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageWatermarkEventHandler(watermarkTexts));
                        var converterProperties = new ConverterProperties();
                        using (Document document = HtmlConverter.ConvertToDocument(html, pdf, converterProperties))
                            {
                            //document.Close();
                            }
                        }
                    }

                return output.ToArray();
                }
            }

        private static async Task<byte[]> RenderPdfWithTimeoutAsync(string html, TimeSpan timeout, PdfWatermarkText watermarkTexts)
            {
            var renderTask = Task.Run(() => RenderPdf(html, watermarkTexts));
            var completedTask = await Task.WhenAny(renderTask, Task.Delay(timeout));
            if (completedTask != renderTask)
                {
                return null;
                }

            return await renderTask;
            }

        private void PopulatePdfGeneratorIdentity(FieldAuditPdfReportData data)
            {
            if (data == null || !_sessionHandler.TryGetUser(out var user))
                {
                return;
                }

            data.GeneratedByName = user.Name?.Trim();
            data.GeneratedByPPNo = user.PPNumber?.Trim();
            }

        private PdfWatermarkText BuildWatermarkTexts(FieldAuditPdfReportData data, int engId)
            {
            var isFinal = _dbConnection.IsFieldAuditReportFinal(engId);
            var bigWatermarkText = isFinal ? "FINAL" : "DRAFT";
            var generatedOn = FormatKarachiTimestamp(DateTime.UtcNow);
            var generatedByName = string.IsNullOrWhiteSpace(data?.GeneratedByName) ? "Unknown User" : data.GeneratedByName.Trim();
            var generatedByPPNo = data?.GeneratedByPPNo?.Trim();

            var footerWatermarkText = string.IsNullOrWhiteSpace(generatedByPPNo)
                ? $"Generated by: {generatedByName} | {generatedOn}"
                : $"Generated by: {generatedByName} | PP No: {generatedByPPNo} | {generatedOn}";

            return new PdfWatermarkText
                {
                BigWatermarkText = bigWatermarkText,
                FooterWatermarkText = footerWatermarkText
                };
            }

        private static string FormatKarachiTimestamp(DateTime utcNow)
            {
            TimeZoneInfo karachiTimeZone;
            try
                {
                karachiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");
                }
            catch (TimeZoneNotFoundException)
                {
                karachiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                }

            var karachiTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), karachiTimeZone);
            return karachiTime.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
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

        private sealed class PdfWatermarkText
            {
            public string BigWatermarkText { get; set; }
            public string FooterWatermarkText { get; set; }
            }

        private sealed class PageWatermarkEventHandler : IEventHandler
            {
            private readonly PdfWatermarkText _watermarkText;

            public PageWatermarkEventHandler(PdfWatermarkText watermarkText)
                {
                _watermarkText = watermarkText;
                }

            public void HandleEvent(Event @event)
                {
                var docEvent = (PdfDocumentEvent)@event;
                var pdf = docEvent.GetDocument();
                var page = docEvent.GetPage();
                var pageNumber = pdf.GetPageNumber(page);
                var pageSize = page.GetPageSize();
                var pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);
                using (var canvas = new Canvas(pdfCanvas, pageSize))
                    {
                    var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                    canvas.SetFont(font);
                    pdfCanvas.SaveState();
                    pdfCanvas.SetExtGState(new PdfExtGState().SetFillOpacity(0.1f));
                    canvas.SetFontColor(ColorConstants.GRAY);
                    canvas.SetFontSize(70);
                    canvas.ShowTextAligned(_watermarkText.BigWatermarkText, pageSize.GetWidth() / 2, pageSize.GetHeight() / 2, TextAlignment.CENTER, VerticalAlignment.MIDDLE, (float)(Math.PI / 4));
                    pdfCanvas.RestoreState();

                    pdfCanvas.SaveState();
                    pdfCanvas.SetExtGState(new PdfExtGState().SetFillOpacity(0.4f));
                    canvas.SetFontColor(ColorConstants.DARK_GRAY);
                    canvas.SetFontSize(8);
                    canvas.ShowTextAligned(_watermarkText.FooterWatermarkText, pageSize.GetRight() - 30, pageSize.GetBottom() + 32, TextAlignment.RIGHT);
                    pdfCanvas.RestoreState();

                    canvas.SetFontColor(ColorConstants.BLACK);
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
