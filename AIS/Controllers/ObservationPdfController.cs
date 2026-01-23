using AIS.Models;
using AIS.Services;
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GeneratePdf(int obsId, int engId)
            {
            var totalStopwatch = Stopwatch.StartNew();
            try
                {
                if (!SessionHandler.TryGetUser(out var user))
                    {
                    return StatusCode(401, "Session expired. Please sign in again.");
                    }

                if (!User.Identity.IsAuthenticated)
                    {
                    return StatusCode(401, "User session is not authenticated.");
                    }

                if (!this.UserHasPagePermissionForCurrentAction(SessionHandler))
                    {
                    return StatusCode(403, "User is not authorized to access observation PDFs.");
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
                    return StatusCode(403, "Not authorized. Observation is not available for this engagement.");
                    }

                _logger.LogInformation("Generating observation PDF for OBS_ID {ObsId} ENG_ID {EngId} by user {UserId}.", obsId, engId, user?.USER_ID);

                var dataStopwatch = Stopwatch.StartNew();
                var data = _dbConnection.GetObservationPdfData(obsId);
                dataStopwatch.Stop();
                _logger.LogInformation("Observation PDF data retrieval completed in {ElapsedMs} ms for OBS_ID {ObsId}.", dataStopwatch.ElapsedMilliseconds, obsId);
                if (data == null || string.IsNullOrWhiteSpace(data.MemoNumber))
                    {
                    return BadRequest("Observation data is not available for the selected record.");
                    }

                var html = _pdfBuilder.BuildHtml(data);
                var htmlLength = html?.Length ?? 0;
                var htmlBytes = Encoding.UTF8.GetByteCount(html ?? string.Empty);
                var htmlSizeKb = htmlBytes / 1024d;
                var htmlSizeMb = htmlSizeKb / 1024d;
                _logger.LogInformation(
                    "Observation PDF HTML length {HtmlLength} chars, size {HtmlSizeKb:F2} KB ({HtmlSizeMb:F2} MB) for OBS_ID {ObsId}.",
                    htmlLength,
                    htmlSizeKb,
                    htmlSizeMb,
                    obsId);
                if (htmlLength == 0)
                    {
                    return StatusCode(500, "PDF content could not be prepared for this observation.");
                    }

                const int maxHtmlBytes = 800 * 1024;
                if (htmlBytes > maxHtmlBytes)
                    {
                    _logger.LogWarning(
                        "Observation PDF HTML exceeds safe size limit {HtmlSizeKb:F2} KB (limit {LimitKb} KB) for OBS_ID {ObsId}.",
                        htmlSizeKb,
                        maxHtmlBytes / 1024,
                        obsId);
                    return StatusCode(413, "Content too large for PDF export, please export per observation text block / reduce text.");
                    }

                var pdfStopwatch = Stopwatch.StartNew();
                var pdfBytes = await RenderPdfWithTimeoutAsync(html, TimeSpan.FromSeconds(45));
                pdfStopwatch.Stop();

                if (pdfBytes == null)
                    {
                    _logger.LogWarning("Observation PDF generation timed out after {ElapsedMs} ms for OBS_ID {ObsId}.", pdfStopwatch.ElapsedMilliseconds, obsId);
                    return StatusCode(504, "PDF generation timed out. Please try again.");
                    }

                _logger.LogInformation("Observation PDF generated in {ElapsedMs} ms with {PdfBytes} bytes for OBS_ID {ObsId}.", pdfStopwatch.ElapsedMilliseconds, pdfBytes.Length, obsId);
                if (pdfBytes.Length == 0)
                    {
                    return StatusCode(500, "Generated PDF is empty. Please try again.");
                    }

                var filename = $"Observation_{obsId}.pdf";
                totalStopwatch.Stop();
                _logger.LogInformation("Observation PDF request completed in {ElapsedMs} ms for OBS_ID {ObsId}.", totalStopwatch.ElapsedMilliseconds, obsId);
                return File(pdfBytes, "application/pdf", filename);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate observation PDF for OBS_ID {ObsId}.", obsId);
                return StatusCode(500, "An error occurred while generating the PDF. Please try again later.");
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
        }
    }
