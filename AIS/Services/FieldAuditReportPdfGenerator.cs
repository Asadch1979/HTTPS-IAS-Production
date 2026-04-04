using AIS.Models.FieldAuditReport;
using AIS.Controllers;
using iText.Html2pdf;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.Layout;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AIS.Services
    {
    public class FieldAuditReportPdfGenerator
        {
        private readonly ILogger<FieldAuditReportPdfGenerator> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly FieldAuditReportPdfBuilder _pdfBuilder;

        public FieldAuditReportPdfGenerator(
            ILogger<FieldAuditReportPdfGenerator> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            FieldAuditReportPdfBuilder pdfBuilder)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _pdfBuilder = pdfBuilder;
            }

        public async Task<FieldAuditGeneratedPdfDocument> GenerateAsync(int engId, int? reportVersion = null)
            {
            try
                {
                _logger.LogInformation("Generating field audit report PDF for ENG_ID {EngId} reportVersion {ReportVersion}.", engId, reportVersion);
                var data = _dbConnection.GetFieldAuditReportPdfData(engId, reportVersion);
                if (data == null)
                    {
                    return FieldAuditGeneratedPdfDocument.Fail(500, "Unable to generate PDF at this time.");
                    }

                PopulatePdfGeneratorIdentity(data);

                var html = _pdfBuilder.BuildHtml(data);
                var htmlLength = html?.Length ?? 0;
                if (htmlLength == 0)
                    {
                    return FieldAuditGeneratedPdfDocument.Fail(500, "PDF content could not be prepared for this report.");
                    }

                var watermarkTexts = BuildWatermarkTexts(data, engId);
                var pdfBytes = await RenderPdfWithTimeoutAsync(html, TimeSpan.FromSeconds(60), watermarkTexts);
                if (pdfBytes == null)
                    {
                    _logger.LogWarning("Field audit PDF generation timed out for ENG_ID {EngId}.", engId);
                    return FieldAuditGeneratedPdfDocument.Fail(504, "PDF generation timed out. Please try again.");
                    }

                if (pdfBytes.Length == 0)
                    {
                    return FieldAuditGeneratedPdfDocument.Fail(500, "Generated PDF is empty. Please try again.");
                    }

                return new FieldAuditGeneratedPdfDocument
                    {
                    ContentBytes = pdfBytes,
                    FileName = BuildFilename(data),
                    ContentType = "application/pdf"
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate field audit report PDF for ENG_ID {EngId}.", engId);
                return FieldAuditGeneratedPdfDocument.Fail(500, "An error occurred while generating the PDF. Please try again later.");
                }
            }

        private static byte[] RenderPdf(string html, PdfWatermarkText watermarkTexts)
            {
            using (var output = new MemoryStream())
                {
                using (var writer = new PdfWriter(output))
                using (var pdf = new PdfDocument(writer))
                    {
                    pdf.SetDefaultPageSize(PageSize.A4);
                    pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageWatermarkEventHandler(watermarkTexts));
                    var converterProperties = new ConverterProperties();
                    using (Document document = HtmlConverter.ConvertToDocument(html, pdf, converterProperties))
                        {
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

    public class FieldAuditGeneratedPdfDocument
        {
        public byte[] ContentBytes { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/pdf";
        public int FailureStatusCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsSuccess => ContentBytes != null && ContentBytes.Length > 0;

        public static FieldAuditGeneratedPdfDocument Fail(int failureStatusCode, string errorMessage)
            {
            return new FieldAuditGeneratedPdfDocument
                {
                FailureStatusCode = failureStatusCode,
                ErrorMessage = errorMessage ?? string.Empty
                };
            }
        }
    }



