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
using System.Linq;

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
        public IActionResult GeneratePdf(int obsId, int engId)
            {
            try
                {
                var (_, errorResult) = GetUserOr401();
                if (errorResult != null)
                    {
                    return errorResult;
                    }

                if (!User.Identity.IsAuthenticated)
                    {
                    return Unauthorized(new { message = "User session is not authenticated." });
                    }

                if (!this.UserHasPagePermissionForCurrentAction(SessionHandler))
                    {
                    return Unauthorized(new { message = "User is not authorized to access observation PDFs." });
                    }

                if (obsId <= 0)
                    {
                    return BadRequest("Observation id is required.");
                    }

                if (engId <= 0)
                    {
                    return BadRequest("Engagement id is required.");
                    }

                if (!IsObservationAuthorized(engId, obsId))
                    {
                    return Unauthorized(new { message = "Not authorized / observation not in your list." });
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

        private bool IsObservationAuthorized(int engId, int obsId)
            {
            if (engId <= 0 || obsId <= 0)
                {
                return false;
                }

            return _dbConnection.GetManagedObservationsForBranches(engId, obsId).Any();
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
