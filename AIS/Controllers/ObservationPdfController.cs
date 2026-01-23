using AIS.Models;
using AIS.Services;
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace AIS.Controllers
    {
    [Route("Observation")]
    public class ObservationPdfController : BaseController
        {
        private readonly ILogger<ObservationPdfController> _logger;
        private readonly DBConnection _dbConnection;
        private readonly ObservationPdfBuilder _pdfBuilder;

        public ObservationPdfController(
            ILogger<ObservationPdfController> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            ObservationPdfBuilder pdfBuilder)
            : base(sessionHandler)
            {
            _logger = logger;
            _dbConnection = dbConnection;
            _pdfBuilder = pdfBuilder;
            }

        [HttpGet("GeneratePdf")]
        public IActionResult GeneratePdf(int obsId)
            {
            try
                {
                var (_, errorResult) = GetUserOr401();
                if (errorResult != null)
                    {
                    return errorResult;
                    }

                if (obsId <= 0)
                    {
                    return BadRequest("Observation id is required.");
                    }

                var data = _dbConnection.GetObservationPdfData(obsId);
                if (data == null || string.IsNullOrWhiteSpace(data.MemoNumber))
                    {
                    return BadRequest("Observation data is not available for the selected record.");
                    }

                var html = _pdfBuilder.BuildHtml(data);
                var pdfBytes = RenderPdf(html);
                var filename = $"Observation_{obsId}.pdf";
                return File(pdfBytes, "application/pdf", filename);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate observation PDF for OBS_ID {ObsId}.", obsId);
                return BadRequest("An error occurred while generating the PDF. Please try again later.");
                }
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
        }
    }
