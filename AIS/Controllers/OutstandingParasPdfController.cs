using AIS.Models;
using AIS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIS.Controllers
    {
    [Route("OutstandingParasPdf")]
    public class OutstandingParasPdfController : BaseController
        {
        private readonly ILogger<OutstandingParasPdfController> _logger;
        private readonly SessionHandler _sessionHandler;
        private readonly TopMenus _topMenus;
        private readonly DBConnection _dbConnection;
        private readonly OutstandingParasPdfGenerator _pdfGenerator;

        public OutstandingParasPdfController(
            ILogger<OutstandingParasPdfController> logger,
            SessionHandler sessionHandler,
            TopMenus topMenus,
            DBConnection dbConnection,
            OutstandingParasPdfGenerator pdfGenerator)
            : base(sessionHandler)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _topMenus = topMenus;
            _dbConnection = dbConnection;
            _pdfGenerator = pdfGenerator;
            }

        [HttpGet("LoadEntities")]
        public IActionResult LoadEntities(int auditDepartmentId, string executionStartDate, string executionEndDate)
            {
            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                if (!TryValidateFilters(auditDepartmentId, executionStartDate, executionEndDate, out var startDate, out var endDate, out var errorMessage))
                    {
                    return BadRequest(errorMessage);
                    }

                var entities = _dbConnection.GetOutstandingParaEntitiesForPdf(auditDepartmentId, startDate.Date, endDate.Date);
                return Json(new
                    {
                    success = true,
                    data = entities.Select(item => new
                        {
                        engagementId = item.EngagementId,
                        entityId = item.EntityId,
                        entityName = item.EntityName,
                        entityCode = item.EntityCode,
                        auditDepartment = item.AuditDepartment,
                        auditPeriod = item.AuditPeriod,
                        executionStartDate = FormatDate(item.ExecutionStartDate),
                        executionEndDate = FormatDate(item.ExecutionEndDate),
                        teamLead = item.TeamLead,
                        teamMembers = item.TeamMembers,
                        outstandingParasCount = item.OutstandingParasCount
                        })
                    });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to load outstanding paras entities.");
                return StatusCode(500, "An error occurred while loading engagements/entities. Please try again later.");
                }
            }

        [HttpGet("GeneratePdf")]
        public IActionResult GeneratePdf(int auditDepartmentId, string executionStartDate, string executionEndDate)
            {
            var stopwatch = Stopwatch.StartNew();
            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                if (auditDepartmentId <= 0)
                    {
                    return BadRequest("Audit Department is required.");
                    }

                if (!TryValidateFilters(auditDepartmentId, executionStartDate, executionEndDate, out var startDate, out var endDate, out var errorMessage))
                    {
                    return BadRequest(errorMessage);
                    }

                stopwatch.Stop();
                _logger.LogInformation("Rejected consolidated outstanding paras PDF request after {ElapsedMs} ms; use ENG_ID-based export.", stopwatch.ElapsedMilliseconds);
                return BadRequest("Consolidated outstanding paras PDF export is disabled for large data ranges. Please use Search / Load and Export Selected PDFs.");
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate consolidated outstanding paras PDF.");
                return StatusCode(500, "An error occurred while generating the PDF. Please try again later.");
                }
            }

        [HttpGet("GenerateSummaryPdf")]
        public async Task<IActionResult> GenerateSummaryPdf(int auditDepartmentId = 0, string risk = "All")
            {
            var stopwatch = Stopwatch.StartNew();
            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                var normalizedRisk = string.IsNullOrWhiteSpace(risk) ? "All" : risk.Trim();
                var result = await _pdfGenerator.GenerateSummaryBatchAsync(auditDepartmentId, normalizedRisk);
                if (!result.IsSuccess)
                    {
                    return StatusCode(result.FailureStatusCode == 0 ? 500 : result.FailureStatusCode, result.ErrorMessage);
                    }

                stopwatch.Stop();
                _logger.LogInformation("CIA consolidated outstanding paras summary batch completed for department {AuditDepartmentId}, risk {Risk}, batch {BatchId} in {ElapsedMs} ms.", auditDepartmentId, normalizedRisk, result.BatchId, stopwatch.ElapsedMilliseconds);
                return Json(new
                    {
                    success = true,
                    batchId = result.BatchId,
                    totalSets = result.TotalSets,
                    successCount = result.SuccessCount,
                    failureCount = result.FailureCount,
                    folderUrl = result.FolderRelativeUrl,
                        files = result.Files.Select(item => new
                            {
                            pdfId = item.PdfId,
                            fileName = item.FileName,
                            url = item.Url,
                            contentType = item.ContentType,
                        sizeBytes = item.SizeBytes,
                        isFailureFile = item.IsFailureFile
                        })
                    });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate CIA consolidated outstanding paras summary PDF.");
                return StatusCode(500, "An error occurred while generating the PDF. Please try again later.");
                }
            }

        [HttpPost("DeleteSummaryBatch")]
        public IActionResult DeleteSummaryBatch([FromBody] OutstandingParasSummaryBatchDeleteRequest request)
            {
            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                var batchId = request?.BatchId?.Trim();
                if (string.IsNullOrWhiteSpace(batchId))
                    {
                    return BadRequest("Batch ID is required.");
                    }

                var deleted = _pdfGenerator.DeleteSummaryBatch(batchId);
                if (!deleted)
                    {
                    return StatusCode(500, "Unable to remove the generated PDF batch. Please try again.");
                    }

                return Json(new { success = true, batchId });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to delete CIA consolidated outstanding paras summary batch.");
                return StatusCode(500, "An error occurred while removing the PDF batch. Please try again later.");
                }
            }

        [HttpGet("ListSummaryPdfs")]
        public IActionResult ListSummaryPdfs()
            {
            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                var records = _dbConnection.GetCiaSummaryPdfList();
                return Json(new
                    {
                    success = true,
                    data = records.Select(item => new
                        {
                        pdfId = item.PdfId,
                        batchId = item.BatchId,
                        auditDepartmentId = item.AuditDepartmentId,
                        auditDepartmentName = item.AuditDepartmentName,
                        entityId = item.EntityId,
                        entityName = item.EntityName,
                        risk = item.Risk,
                        partNo = item.PartNo,
                        fileName = item.FileName,
                        fileMimeType = item.FileMimeType,
                        fileSize = item.FileSize,
                        generatedBy = item.GeneratedBy,
                        generatedOn = FormatDateTime(item.GeneratedOn),
                        expiresOn = FormatDateTime(item.ExpiresOn),
                        status = item.Status,
                        errorMessage = item.ErrorMessage,
                        downloadUrl = Url.Action(nameof(DownloadSummaryPdf), "OutstandingParasPdf", new { pdfId = item.PdfId })
                        })
                    });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to list stored CIA summary PDFs.");
                return StatusCode(500, "An error occurred while loading previously generated CIA summary PDFs. Please try again later.");
                }
            }

        [HttpGet("DownloadSummaryPdf")]
        public IActionResult DownloadSummaryPdf(int pdfId)
            {
            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                if (pdfId <= 0)
                    {
                    return BadRequest("A valid PDF ID is required.");
                    }

                var document = _dbConnection.DownloadCiaSummaryPdf(pdfId);
                if (!document.IsSuccess)
                    {
                    return NotFound(string.IsNullOrWhiteSpace(document.Message) ? "PDF not found or not available for download." : document.Message);
                    }

                var contentType = string.IsNullOrWhiteSpace(document.FileMimeType) ? "application/pdf" : document.FileMimeType;
                var fileName = string.IsNullOrWhiteSpace(document.FileName) ? $"CIA_Summary_{pdfId}.pdf" : SafeFileName(document.FileName);
                return File(document.ContentBytes, contentType, fileName);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to download stored CIA summary PDF {PdfId}.", pdfId);
                return StatusCode(500, "An error occurred while downloading the PDF. Please try again later.");
                }
            }

        [HttpPost("DeleteSummaryPdf")]
        [AIS.Filters.ApplicationAudit("CIA_SUMMARY_PDF_DELETED", "COMPLIANCE", "OUTSTANDING PARAS PDF", "PKG_CIA_SUMMARY_PDF", "P_DELETE_CIA_SUMMARY_PDF", ObjectType = "CIA_SUMMARY_PDF", ObjectId = "request.PdfId")]
        public IActionResult DeleteSummaryPdf([FromBody] OutstandingParasSummaryPdfDeleteRequest request)
            {
            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                var pdfId = request?.PdfId ?? 0;
                if (pdfId <= 0)
                    {
                    return BadRequest("A valid PDF ID is required.");
                    }

                _sessionHandler.TryGetUser(out var user);
                var result = _dbConnection.DeleteCiaSummaryPdf(pdfId, user?.PPNumber);
                if (!result.IsSuccess)
                    {
                    return StatusCode(500, string.IsNullOrWhiteSpace(result.Message) ? "Unable to delete the selected PDF." : result.Message);
                    }

                return Json(new { success = true, pdfId, message = result.Message });
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to delete stored CIA summary PDF.");
                return StatusCode(500, "An error occurred while deleting the PDF. Please try again later.");
                }
            }

        [HttpPost("ExportSelectedSummaryPdfsZip")]
        [AIS.Filters.ApplicationAudit("CIA_SUMMARY_PDFS_EXPORTED", "COMPLIANCE", "OUTSTANDING PARAS PDF", "PKG_CIA_SUMMARY_PDF", "P_DOWNLOAD_CIA_SUMMARY_PDF", ObjectType = "CIA_SUMMARY_PDF_COLLECTION", RequireNonEmpty = "request.PdfIds")]
        public async Task<IActionResult> ExportSelectedSummaryPdfsZip([FromBody] OutstandingParasSummaryPdfZipExportRequest request)
            {
            var stopwatch = Stopwatch.StartNew();
            var pdfIds = request?.PdfIds?
                .Where(pdfId => pdfId > 0)
                .Distinct()
                .ToList() ?? new System.Collections.Generic.List<int>();

            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                if (pdfIds.Count == 0)
                    {
                    return BadRequest("Please select at least one CIA summary PDF before export.");
                    }

                var successCount = 0;
                var failedCount = 0;
                var failures = new List<string>();
                using var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                    {
                    var usedZipEntryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var pdfId in pdfIds)
                        {
                        try
                            {
                            var document = _dbConnection.DownloadCiaSummaryPdf(pdfId);
                            if (document.IsSuccess)
                                {
                                var entryName = SafeFileName(string.IsNullOrWhiteSpace(document.FileName) ? $"CIA_Summary_{pdfId}.pdf" : document.FileName);
                                entryName = EnsureUniqueZipEntryName(usedZipEntryNames, entryName);
                                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                                await using (var entryStream = entry.Open())
                                    {
                                    await entryStream.WriteAsync(document.ContentBytes, 0, document.ContentBytes.Length);
                                    }

                                successCount++;
                                }
                            else
                                {
                                failedCount++;
                                var message = string.IsNullOrWhiteSpace(document.Message) ? "PDF not found or not available for download." : document.Message;
                                failures.Add($"PDF_ID {pdfId}: {message}");
                                TryAddPdfFailureEntry(archive, usedZipEntryNames, pdfId, message);
                                }
                            }
                        catch (Exception ex)
                            {
                            failedCount++;
                            failures.Add($"PDF_ID {pdfId}: {ex.Message}");
                            TryAddPdfFailureEntry(archive, usedZipEntryNames, pdfId, ex.Message);
                            _logger.LogError(ex, "Stored CIA summary PDF {PdfId} failed during ZIP export.", pdfId);
                            }
                        }
                    }

                stopwatch.Stop();
                _logger.LogInformation("Stored CIA summary PDF ZIP export completed in {ElapsedMs} ms with {SuccessCount} successful and {FailedCount} failed PDF(s).", stopwatch.ElapsedMilliseconds, successCount, failedCount);

                if (successCount == 0 && failedCount > 0)
                    {
                    var detail = failures.Count > 0 ? string.Join(" ", failures.Take(3)) : "No selected PDFs could be downloaded.";
                    return StatusCode(500, $"Unable to export selected CIA summary PDFs. {detail}");
                    }

                var zipFileName = $"CIA_Summary_PDFs_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                return File(zipStream.ToArray(), "application/zip", zipFileName);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to export stored CIA summary PDFs ZIP for {TotalCount} PDF(s).", pdfIds.Count);
                return StatusCode(500, $"An error occurred while generating the ZIP export: {ex.Message}");
                }
            }

        [HttpGet("GeneratePdfByEngId")]
        public async Task<IActionResult> GeneratePdfByEngId(int engId)
            {
            var stopwatch = Stopwatch.StartNew();
            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                if (engId <= 0)
                    {
                    return BadRequest("A valid engagement is required.");
                    }

                var document = await _pdfGenerator.GenerateByEngIdAsync(engId);
                if (!document.IsSuccess)
                    {
                    return StatusCode(document.FailureStatusCode == 0 ? 500 : document.FailureStatusCode, document.ErrorMessage);
                    }

                stopwatch.Stop();
                _logger.LogInformation("Outstanding paras PDF completed for engagement {EngId} in {ElapsedMs} ms.", engId, stopwatch.ElapsedMilliseconds);
                return File(document.ContentBytes, document.ContentType, document.FileName);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate outstanding paras PDF for engagement {EngId}.", engId);
                return StatusCode(500, "An error occurred while generating the PDF. Please try again later.");
                }
            }

        [HttpPost("GenerateSelectedZip")]
        public async Task<IActionResult> GenerateSelectedZip([FromBody] OutstandingParasZipExportRequest request)
            {
            var stopwatch = Stopwatch.StartNew();
            var engagementIds = request?.EngagementIds?
                .Where(engId => engId > 0)
                .Distinct()
                .ToList() ?? new System.Collections.Generic.List<int>();

            try
                {
                var precheck = EnsureAuthorized();
                if (precheck != null)
                    {
                    return precheck;
                    }

                if (engagementIds.Count == 0)
                    {
                    return BadRequest("Please select at least one engagement/entity before export.");
                    }

                _logger.LogInformation("Outstanding paras ZIP export started with {TotalCount} engagement(s).", engagementIds.Count);

                var successCount = 0;
                var failedCount = 0;
                using var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                    {
                    foreach (var engId in engagementIds)
                        {
                        try
                            {
                            var document = await _pdfGenerator.GenerateByEngIdAsync(engId);
                            if (document.IsSuccess)
                                {
                                var pdfEntryName = SafeFileName(string.IsNullOrWhiteSpace(document.FileName) ? $"Audit Report of ENG_{engId}.pdf" : document.FileName);
                                var pdfEntry = archive.CreateEntry(pdfEntryName, CompressionLevel.Fastest);
                                await using (var entryStream = pdfEntry.Open())
                                    {
                                    await entryStream.WriteAsync(document.ContentBytes, 0, document.ContentBytes.Length);
                                    }

                                successCount++;
                                _logger.LogInformation("Outstanding paras PDF for engagement {EngId} added to ZIP as {FileName}.", engId, pdfEntryName);
                                }
                            else
                                {
                                failedCount++;
                                AddFailureEntry(archive, engId, document.ErrorMessage);
                                _logger.LogWarning("Outstanding paras PDF for engagement {EngId} failed during ZIP export: {ErrorMessage}", engId, document.ErrorMessage);
                                }
                            }
                        catch (Exception ex)
                            {
                            failedCount++;
                            AddFailureEntry(archive, engId, ex.Message);
                            _logger.LogError(ex, "Outstanding paras PDF for engagement {EngId} failed during ZIP export.", engId);
                            }
                        }
                    }

                stopwatch.Stop();
                _logger.LogInformation("Outstanding paras ZIP export completed in {ElapsedMs} ms with {SuccessCount} successful and {FailedCount} failed engagement(s).", stopwatch.ElapsedMilliseconds, successCount, failedCount);

                var zipFileName = $"Outstanding_Audit_Reports_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                return File(zipStream.ToArray(), "application/zip", zipFileName);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate outstanding paras ZIP export for {TotalCount} engagement(s).", engagementIds.Count);
                return StatusCode(500, "An error occurred while generating the ZIP export. Please try again later.");
                }
            }

        private IActionResult EnsureAuthorized()
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
                return StatusCode(403, "User is not authorized to access consolidated outstanding paras reports.");
                }

            return null;
            }

        private static bool TryParseDate(string input, out DateTime value)
            {
            return DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value)
                || DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out value);
            }

        private static bool TryValidateFilters(int auditDepartmentId, string executionStartDate, string executionEndDate, out DateTime startDate, out DateTime endDate, out string errorMessage)
            {
            startDate = default;
            endDate = default;
            errorMessage = string.Empty;

            if (auditDepartmentId <= 0)
                {
                errorMessage = "Audit Department is required.";
                return false;
                }

            if (!TryParseDate(executionStartDate, out startDate) || !TryParseDate(executionEndDate, out endDate))
                {
                errorMessage = "Valid execution start and end dates are required.";
                return false;
                }

            if (startDate.Date > endDate.Date)
                {
                errorMessage = "Execution start date cannot be after execution end date.";
                return false;
                }

            return true;
            }

        private static string FormatDate(DateTime? value)
            {
            return value.HasValue ? value.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : string.Empty;
            }

        private static string FormatDateTime(DateTime? value)
            {
            return value.HasValue ? value.Value.ToString("dd-MMM-yyyy HH:mm", CultureInfo.InvariantCulture) : string.Empty;
            }

        private static void AddFailureEntry(ZipArchive archive, int engId, string errorMessage)
            {
            var entry = archive.CreateEntry(SafeFileName($"FAILED_ENG_{engId}.txt"), CompressionLevel.Fastest);
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.WriteLine($"Engagement ID: {engId}");
            writer.WriteLine("PDF generation failed.");
            writer.WriteLine();
            writer.WriteLine(string.IsNullOrWhiteSpace(errorMessage) ? "No error message was returned." : errorMessage);
            }

        private static void AddPdfFailureEntry(ZipArchive archive, HashSet<string> usedZipEntryNames, int pdfId, string errorMessage)
            {
            var entry = archive.CreateEntry(EnsureUniqueZipEntryName(usedZipEntryNames, SafeFileName($"FAILED_PDF_{pdfId}.txt")), CompressionLevel.Fastest);
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.WriteLine($"PDF ID: {pdfId}");
            writer.WriteLine("PDF export failed.");
            writer.WriteLine();
            writer.WriteLine(string.IsNullOrWhiteSpace(errorMessage) ? "No error message was returned." : errorMessage);
            }

        private void TryAddPdfFailureEntry(ZipArchive archive, HashSet<string> usedZipEntryNames, int pdfId, string errorMessage)
            {
            try
                {
                AddPdfFailureEntry(archive, usedZipEntryNames, pdfId, errorMessage);
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Unable to add ZIP failure note for stored CIA summary PDF {PdfId}.", pdfId);
                }
            }

        private static string SafeFileName(string value)
            {
            var safeValue = string.IsNullOrWhiteSpace(value) ? "file" : value.Trim();
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                {
                safeValue = safeValue.Replace(invalidChar, '_');
                }

            return safeValue;
            }

        private static string EnsureUniqueZipEntryName(HashSet<string> usedZipEntryNames, string fileName)
            {
            var safeName = SafeFileName(fileName);
            if (usedZipEntryNames == null)
                {
                return safeName;
                }

            if (usedZipEntryNames.Add(safeName))
                {
                return safeName;
                }

            var extension = Path.GetExtension(safeName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(safeName);
            for (var index = 1; index < 10000; index++)
                {
                var candidate = $"{nameWithoutExtension}_{index}{extension}";
                if (usedZipEntryNames.Add(candidate))
                    {
                    return candidate;
                    }
                }

            var fallback = $"{nameWithoutExtension}_{Guid.NewGuid():N}{extension}";
            usedZipEntryNames.Add(fallback);
            return fallback;
            }
        }
    }
