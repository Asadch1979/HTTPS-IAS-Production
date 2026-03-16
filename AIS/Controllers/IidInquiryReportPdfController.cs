using AIS.Constants;
using AIS.Models.IID;
using AIS.Services;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    [Route("IidInquiryReportPdf")]
    public class IidInquiryReportPdfController : BaseController
        {
        private readonly ILogger<IidInquiryReportPdfController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly TopMenus _topMenus;
        private readonly IidInquiryReportPdfBuilder _pdfBuilder;

        public IidInquiryReportPdfController(
            ILogger<IidInquiryReportPdfController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            TopMenus topMenus,
            IidInquiryReportPdfBuilder pdfBuilder)
            : base(sessionHandler)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _topMenus = topMenus;
            _pdfBuilder = pdfBuilder;
            }

        [HttpGet("GeneratePdf")]
        public async Task<IActionResult> GeneratePdf(long complaintId)
            {
            var stopwatch = Stopwatch.StartNew();
            try
                {
                _logger.LogInformation("IID Inquiry PDF requested for complaintId={ComplaintId}", complaintId);

                var precheck = EnsureReadyForPdf(complaintId);
                if (precheck != null)
                    {
                    return precheck;
                    }

                _logger.LogInformation("Calling GetIidInquiryReportPdfData for complaintId={ComplaintId}", complaintId);
                var dBConnection = _dbConnection;
                var pdfData = dBConnection.GetIidInquiryReportPdfData(complaintId);
                if (pdfData == null)
                    {
                    return BadRequest("Unable to load IID report data for PDF generation.");
                    }

                PopulatePdfGeneratorIdentity(pdfData);

                var html = _pdfBuilder.BuildHtml(pdfData);
                if (string.IsNullOrWhiteSpace(html))
                    {
                    _logger.LogError("IID PDF builder returned empty html for complaintId={ComplaintId}.", complaintId);
                    return StatusCode(500, "IID PDF content could not be prepared.");
                    }

                if (!ContainsFullHtmlDocument(html))
                    {
                    _logger.LogError("IID PDF builder returned incomplete html document for complaintId={ComplaintId}.", complaintId);
                    return StatusCode(500, "IID PDF content is invalid before rendering.");
                    }

                _logger.LogInformation("IID PDF HTML prepared. complaintId={ComplaintId}, htmlLength={HtmlLength}", complaintId, html.Length);
                var pdfBytes = await RenderPdfWithTimeoutAsync(html, TimeSpan.FromSeconds(60), BuildWatermarkTexts(pdfData));
                if (pdfBytes == null)
                    {
                    return StatusCode(504, "PDF generation timed out. Please try again.");
                    }

                stopwatch.Stop();
                _logger.LogInformation("IID PDF generated for complaint {ComplaintId} in {ElapsedMs} ms.", complaintId, stopwatch.ElapsedMilliseconds);
                return File(pdfBytes, "application/pdf", BuildFilename(pdfData));
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "IID PDF generation failed for complaint {ComplaintId}.", complaintId);
                return StatusCode(500, "An error occurred while generating IID PDF.");
                }
            }

        [HttpGet("ViewPdf")]
        public IActionResult ViewPdf(long complaintId)
            {
            return RedirectToAction(nameof(GeneratePdf), new { complaintId });
            }

        [HttpGet("RegeneratePdf")]
        public IActionResult RegeneratePdf(long complaintId)
            {
            return RedirectToAction(nameof(GeneratePdf), new { complaintId });
            }

        private IActionResult EnsureReadyForPdf(long complaintId)
            {
            if (!_sessionHandler.TryGetUser(out _))
                {
                return StatusCode(401, "Session expired. Please sign in again.");
                }

            _topMenus.GetTopMenus();
            if (!User.Identity.IsAuthenticated)
                {
                return StatusCode(401, "User session is not authenticated.");
                }

            if (!this.UserHasPagePermissionForCurrentAction(_sessionHandler))
                {
                return StatusCode(403, "User is not authorized to access IID PDF.");
                }

            if (complaintId <= 0)
                {
                return BadRequest("complaintId is required.");
                }

            if (!_dbConnection.IsComplaintAllowedForIidPdf(complaintId))
                {
                return BadRequest("You are not authorized to generate a report for the selected complaint.");
                }

            var complaint = _dbConnection.GetComplaint((int)complaintId);
            if (complaint == null)
                {
                return NotFound("Complaint not found.");
                }

            var status = (complaint.Status ?? string.Empty).Trim().ToUpperInvariant();
            if (status != IidStatuses.ReportSubmitted && status != IidStatuses.Closed && status != IidStatuses.QcCleared)
                {
                return BadRequest("Inquiry report must be completed before PDF generation.");
                }

            return null;
            }

        private void PopulatePdfGeneratorIdentity(IidInquiryReportPdfData data)
            {
            if (data == null || !_sessionHandler.TryGetUser(out var user))
                {
                return;
                }

            data.Header ??= new IidInquiryHeaderModel();
            data.Header.GeneratedByName = user.Name?.Trim();
            data.Header.GeneratedByPPNo = user.PPNumber?.Trim();
            data.Header.GeneratedOn = DateTime.UtcNow;
            }

        private PdfWatermarkText BuildWatermarkTexts(IidInquiryReportPdfData data)
            {
            var status = data?.Header?.InquiryStatus?.Trim().ToUpperInvariant();
            var isFinal = status == IidStatuses.ReportSubmitted || status == IidStatuses.Closed || status == IidStatuses.QcCleared;
            var bigWatermarkText = isFinal ? "FINAL" : "DRAFT";
            var generatedOn = FormatKarachiTimestamp(DateTime.UtcNow);
            var generatedByName = string.IsNullOrWhiteSpace(data?.Header?.GeneratedByName) ? "Unknown User" : data.Header.GeneratedByName.Trim();
            var generatedByPPNo = data?.Header?.GeneratedByPPNo?.Trim();

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

        private async Task<byte[]> RenderPdfWithTimeoutAsync(string html, TimeSpan timeout, PdfWatermarkText watermarkTexts)
            {
            var renderTask = Task.Run(() => RenderPdf(html, watermarkTexts));
            var completedTask = await Task.WhenAny(renderTask, Task.Delay(timeout));
            if (completedTask != renderTask)
                {
                return null;
                }

            return await renderTask;
            }

        private byte[] RenderPdf(string html, PdfWatermarkText watermarkTexts)
            {
            if (string.IsNullOrWhiteSpace(html))
                {
                throw new InvalidOperationException("IID inquiry PDF HTML is null or empty before conversion.");
                }

            if (!ContainsFullHtmlDocument(html))
                {
                throw new InvalidOperationException("IID inquiry PDF HTML is not a complete HTML document before conversion.");
                }

            using MemoryStream output = CreateOutputStream();
            using PdfWriter writer = CreatePdfWriter(output);
            using PdfDocument pdf = CreatePdfDocument(writer);

            pdf.SetDefaultPageSize(PageSize.A4);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageWatermarkEventHandler(watermarkTexts));

            var converterProperties = new ConverterProperties();

            _logger.LogInformation("IID PDF Render start. htmlLength={HtmlLength}, pdfNull={PdfNull}, converterNull={ConverterNull}",
                html?.Length ?? 0,
                pdf == null,
                converterProperties == null);

            if (string.IsNullOrWhiteSpace(html))
                {
                throw new InvalidOperationException("IID inquiry PDF HTML is null or empty before conversion.");
                }

            if (pdf == null)
                {
                throw new InvalidOperationException("IID inquiry PDF document is null before conversion.");
                }

            if (converterProperties == null)
                {
                throw new InvalidOperationException("IID inquiry PDF converter properties are null before conversion.");
                }

            using Document document = HtmlConverter.ConvertToDocument(html, pdf, converterProperties);
            return output.ToArray();
            }

        private static MemoryStream CreateOutputStream()
            {
            MemoryStream output = new MemoryStream();
            if (output == null)
                {
                throw new InvalidOperationException("IID inquiry PDF output stream is null before conversion.");
                }

            return output;
            }

        private static PdfWriter CreatePdfWriter(Stream output)
            {
            PdfWriter writer = new PdfWriter(output);
            if (writer == null)
                {
                throw new InvalidOperationException("IID inquiry PDF writer is null before conversion.");
                }

            return writer;
            }

        private static PdfDocument CreatePdfDocument(PdfWriter writer)
            {
            PdfDocument pdf = new PdfDocument(writer);
            if (pdf == null)
                {
                throw new InvalidOperationException("IID inquiry PDF document is null before conversion.");
                }

            return pdf;
            }

        private static bool ContainsFullHtmlDocument(string html)
            {
            if (string.IsNullOrWhiteSpace(html))
                {
                return false;
                }

            return html.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0
                && html.IndexOf("</html>", StringComparison.OrdinalIgnoreCase) >= 0
                && html.IndexOf("<body", StringComparison.OrdinalIgnoreCase) >= 0
                && html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase) >= 0;
            }

        private static string BuildFilename(IidInquiryReportPdfData data)
            {
            var complaintNo = data?.ComplaintSnapshot?.ComplaintNo ?? data?.Header?.ComplaintNo ?? "Complaint";
            return $"IID_InquiryReport_{SanitizeFilename(complaintNo)}.pdf";
            }

        private static string SanitizeFilename(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return string.Empty;
                }

            var builder = new StringBuilder(value.Trim());
            foreach (var invalidChar in "<>:\"/\\|?*\0")
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
                    canvas.ShowTextAligned($"Page {pageNumber}", pageSize.GetWidth() / 2, pageSize.GetBottom() + 20, TextAlignment.CENTER);
                    }
                }
            }
        }
    }
