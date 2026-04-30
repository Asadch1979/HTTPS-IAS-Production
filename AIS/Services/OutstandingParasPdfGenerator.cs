using AIS.Controllers;
using AIS.Models;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIS.Services
    {
    public class OutstandingParasPdfGenerator
        {
        private readonly ILogger<OutstandingParasPdfGenerator> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly DBConnection _dbConnection;
        private readonly OutstandingParasPdfBuilder _pdfBuilder;

        public OutstandingParasPdfGenerator(
            ILogger<OutstandingParasPdfGenerator> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            OutstandingParasPdfBuilder pdfBuilder)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _pdfBuilder = pdfBuilder;
            }

        public async Task<OutstandingParasGeneratedPdfDocument> GenerateAsync(int auditDepartmentId, DateTime executionStartDate, DateTime executionEndDate)
            {
            try
                {
                var departments = _dbConnection.GetAuditDepartments();
                var departmentName = departments.FirstOrDefault(item => item.ID == auditDepartmentId)?.NAME ?? string.Empty;
                var data = new OutstandingParasPdfReportData
                    {
                    AuditDepartmentId = auditDepartmentId,
                    AuditDepartmentName = departmentName,
                    ExecutionStartDate = executionStartDate,
                    ExecutionEndDate = executionEndDate,
                    GeneratedOn = GetKarachiNow(),
                    Entities = _dbConnection.GetOutstandingParaEntitiesForPdf(auditDepartmentId, executionStartDate, executionEndDate),
                    Paras = _dbConnection.GetOutstandingParasForPdf(auditDepartmentId, executionStartDate, executionEndDate)
                    };

                PopulateIdentity(data);
                var html = _pdfBuilder.BuildHtml(data);
                var pdfBytes = await RenderPdfWithTimeoutAsync(html, TimeSpan.FromMinutes(5), BuildWatermarkTexts(data));
                if (pdfBytes == null)
                    {
                    _logger.LogWarning("Consolidated outstanding paras PDF generation timed out for department {AuditDepartmentId}.", auditDepartmentId);
                    return OutstandingParasGeneratedPdfDocument.Fail(504, "PDF generation timed out. Please try again.");
                    }

                if (pdfBytes.Length == 0)
                    {
                    return OutstandingParasGeneratedPdfDocument.Fail(500, "Generated PDF is empty. Please try again.");
                    }

                return new OutstandingParasGeneratedPdfDocument
                    {
                    ContentBytes = pdfBytes,
                    ContentType = "application/pdf",
                    FileName = BuildFilename(data)
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate consolidated outstanding paras PDF.");
                return OutstandingParasGeneratedPdfDocument.Fail(500, "An error occurred while generating the PDF. Please try again later.");
                }
            }

        public async Task<OutstandingParasGeneratedPdfDocument> GenerateByEngIdAsync(int engId)
            {
            try
                {
                var entity = _dbConnection.GetOutstandingParaEntityForPdfByEngId(engId);
                if (entity == null || entity.EngagementId <= 0)
                    {
                    return OutstandingParasGeneratedPdfDocument.Fail(404, "No outstanding paras were found for the selected engagement.");
                    }

                var startDate = entity.ExecutionStartDate ?? DateTime.Today;
                var endDate = entity.ExecutionEndDate ?? startDate;
                var data = new OutstandingParasPdfReportData
                    {
                    AuditDepartmentId = 0,
                    AuditDepartmentName = entity.AuditDepartment,
                    ReportTitle = "Outstanding Audit Paras Report",
                    ExecutionStartDate = startDate,
                    ExecutionEndDate = endDate,
                    GeneratedOn = GetKarachiNow(),
                    Entities = new[] { entity }.ToList(),
                    Paras = _dbConnection.GetOutstandingParasForPdfByEngId(engId)
                    };

                PopulateIdentity(data);
                var html = _pdfBuilder.BuildHtml(data);
                var pdfBytes = await RenderPdfWithTimeoutAsync(html, TimeSpan.FromMinutes(5), BuildWatermarkTexts(data));
                if (pdfBytes == null)
                    {
                    _logger.LogWarning("Outstanding paras PDF generation timed out for engagement {EngId}.", engId);
                    return OutstandingParasGeneratedPdfDocument.Fail(504, "PDF generation timed out. Please try again.");
                    }

                if (pdfBytes.Length == 0)
                    {
                    return OutstandingParasGeneratedPdfDocument.Fail(500, "Generated PDF is empty. Please try again.");
                    }

                return new OutstandingParasGeneratedPdfDocument
                    {
                    ContentBytes = pdfBytes,
                    ContentType = "application/pdf",
                    FileName = BuildEngagementFilename(entity)
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate outstanding paras PDF for engagement {EngId}.", engId);
                return OutstandingParasGeneratedPdfDocument.Fail(500, "An error occurred while generating the PDF. Please try again later.");
                }
            }

        private void PopulateIdentity(OutstandingParasPdfReportData data)
            {
            if (data == null || !_sessionHandler.TryGetUser(out var user))
                {
                return;
                }

            data.GeneratedByName = user.Name?.Trim();
            data.GeneratedByPPNo = user.PPNumber?.Trim();
            }

        private static byte[] RenderPdf(string html, PdfWatermarkText watermarkTexts)
            {
            using var output = new MemoryStream();
            using (var writer = new PdfWriter(output))
            using (var pdf = new PdfDocument(writer))
                {
                pdf.SetDefaultPageSize(PageSize.A4);
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageWatermarkEventHandler(watermarkTexts));
                var converterProperties = new ConverterProperties();
                using (HtmlConverter.ConvertToDocument(html, pdf, converterProperties))
                    {
                    }
                }

            return output.ToArray();
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

        private static PdfWatermarkText BuildWatermarkTexts(OutstandingParasPdfReportData data)
            {
            var footer = $"CONFIDENTIAL | Generated on {FormatDateTime(data.GeneratedOn)}";

            return new PdfWatermarkText
                {
                BigWatermarkText = "CONFIDENTIAL",
                FooterWatermarkText = footer
                };
            }

        private static DateTime GetKarachiNow()
            {
            try
                {
                var zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);
                }
            catch (TimeZoneNotFoundException)
                {
                var zone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);
                }
            }

        private static string FormatDateTime(DateTime value)
            {
            return value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture);
            }

        private static string BuildFilename(OutstandingParasPdfReportData data)
            {
            return $"Consolidated_Outstanding_Audit_Paras_{SanitizeFilename(data.AuditDepartmentName)}_{data.ExecutionStartDate:yyyyMMdd}_{data.ExecutionEndDate:yyyyMMdd}.pdf";
            }

        private static string BuildEngagementFilename(OutstandingParaEntityPdfModel entity)
            {
            var entityName = SanitizeFilename(entity?.EntityName);
            var engId = entity?.EngagementId ?? 0;
            return $"Audit Report of {entityName}_ENG_{engId}.pdf";
            }

        private static string SanitizeFilename(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return "Department";
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
                using var canvas = new Canvas(pdfCanvas, pageSize);
                var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                canvas.SetFont(font);

                pdfCanvas.SaveState();
                pdfCanvas.SetExtGState(new PdfExtGState().SetFillOpacity(0.08f));
                canvas.SetFontColor(ColorConstants.GRAY);
                canvas.SetFontSize(54);
                canvas.ShowTextAligned(_watermarkText.BigWatermarkText, pageSize.GetWidth() / 2, pageSize.GetHeight() / 2, TextAlignment.CENTER, VerticalAlignment.MIDDLE, (float)(Math.PI / 4));
                pdfCanvas.RestoreState();

                pdfCanvas.SaveState();
                pdfCanvas.SetExtGState(new PdfExtGState().SetFillOpacity(0.45f));
                canvas.SetFontColor(ColorConstants.DARK_GRAY);
                canvas.SetFontSize(8);
                canvas.ShowTextAligned($"{_watermarkText.FooterWatermarkText} | Page {pageNumber}", pageSize.GetWidth() / 2, pageSize.GetBottom() + 24, TextAlignment.CENTER);
                pdfCanvas.RestoreState();
                }
            }
        }
    }
