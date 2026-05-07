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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int SummaryZipRowsPerPdf = 200;
        private const int SummaryBatchRetentionHours = 24;

        public OutstandingParasPdfGenerator(
            ILogger<OutstandingParasPdfGenerator> logger,
            SessionHandler sessionHandler,
            DBConnection dbConnection,
            OutstandingParasPdfBuilder pdfBuilder,
            IWebHostEnvironment webHostEnvironment)
            {
            _logger = logger;
            _sessionHandler = sessionHandler;
            _dbConnection = dbConnection;
            _pdfBuilder = pdfBuilder;
            _webHostEnvironment = webHostEnvironment;
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

        public async Task<OutstandingParasGeneratedPdfDocument> GenerateSummaryAsync(int auditDepartmentId, string risk)
            {
            try
                {
                var stopwatch = Stopwatch.StartNew();
                var normalizedRisk = string.IsNullOrWhiteSpace(risk) || string.Equals(risk.Trim(), "ALL", StringComparison.OrdinalIgnoreCase)
                    ? "All"
                    : risk.Trim();
                var departments = _dbConnection.GetAuditDepartments();
                var departmentName = auditDepartmentId <= 0
                    ? "All"
                    : departments.FirstOrDefault(item => item.ID == auditDepartmentId)?.NAME ?? "All";

                if (auditDepartmentId <= 0 || IsAllRisk(normalizedRisk))
                    {
                    return await GenerateSummaryByEntityRiskSetsAsync(auditDepartmentId, departmentName, normalizedRisk);
                    }

                var data = new OutstandingParasSummaryPdfReportData
                    {
                    AuditDepartmentId = auditDepartmentId,
                    AuditDepartmentName = departmentName,
                    Risk = normalizedRisk,
                    GeneratedOn = GetKarachiNow(),
                    Paras = _dbConnection.GetOutstandingParasSummaryForPdf(auditDepartmentId, normalizedRisk)
                    };

                var html = _pdfBuilder.BuildSummaryHtml(data);
                _logger.LogInformation(
                    "CIA summary PDF generation starting. DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalRows={TotalRows}, HtmlLength={HtmlLength}.",
                    auditDepartmentId,
                    departmentName,
                    normalizedRisk,
                    data.Paras?.Count ?? 0,
                    html?.Length ?? 0);

                byte[] pdfBytes;
                try
                    {
                    pdfBytes = await RenderSummaryPdfAsync(html, data.GeneratedOn);
                    }
                catch (Exception ex)
                    {
                    stopwatch.Stop();
                    _logger.LogError(
                        ex,
                        "CIA summary PDF generation failed. DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalRows={TotalRows}, HtmlLength={HtmlLength}, GenerationDurationMs={GenerationDurationMs}. Falling back to ZIP split by {RowsPerPdf} rows.",
                        auditDepartmentId,
                        departmentName,
                        normalizedRisk,
                        data.Paras?.Count ?? 0,
                        html?.Length ?? 0,
                        stopwatch.ElapsedMilliseconds,
                        SummaryZipRowsPerPdf);
                    return await GenerateSummaryZipFallbackAsync(data);
                    }

                if (pdfBytes == null || pdfBytes.Length == 0)
                    {
                    stopwatch.Stop();
                    _logger.LogWarning(
                        "CIA summary PDF generation returned empty output. DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalRows={TotalRows}, HtmlLength={HtmlLength}, GenerationDurationMs={GenerationDurationMs}. Falling back to ZIP split by {RowsPerPdf} rows.",
                        auditDepartmentId,
                        departmentName,
                        normalizedRisk,
                        data.Paras?.Count ?? 0,
                        html?.Length ?? 0,
                        stopwatch.ElapsedMilliseconds,
                        SummaryZipRowsPerPdf);
                    return await GenerateSummaryZipFallbackAsync(data);
                    }

                stopwatch.Stop();
                _logger.LogInformation(
                    "CIA summary PDF generation completed. DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalRows={TotalRows}, HtmlLength={HtmlLength}, GenerationDurationMs={GenerationDurationMs}, PdfSizeBytes={PdfSizeBytes}.",
                    auditDepartmentId,
                    departmentName,
                    normalizedRisk,
                    data.Paras?.Count ?? 0,
                    html?.Length ?? 0,
                    stopwatch.ElapsedMilliseconds,
                    pdfBytes.Length);

                return new OutstandingParasGeneratedPdfDocument
                    {
                    ContentBytes = pdfBytes,
                    ContentType = "application/pdf",
                    FileName = BuildSummaryFilename(data)
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate CIA consolidated outstanding paras summary PDF.");
                return OutstandingParasGeneratedPdfDocument.Fail(500, "An error occurred while generating the PDF. Please try again later.");
                }
            }

        private async Task<OutstandingParasGeneratedPdfDocument> GenerateSummaryByEntityRiskSetsAsync(int auditDepartmentId, string departmentName, string normalizedRisk)
            {
            var zipStopwatch = Stopwatch.StartNew();
            var generatedOn = GetKarachiNow();
            var risks = IsAllRisk(normalizedRisk)
                ? new[] { "High", "Medium", "Low" }
                : new[] { normalizedRisk };
            var sets = new List<OutstandingParasSummarySetModel>();

            foreach (var setRisk in risks)
                {
                sets.AddRange(_dbConnection.GetOutstandingParasSummarySetsForPdf(auditDepartmentId, setRisk)
                    .Where(item => item.EntityId > 0 && !string.IsNullOrWhiteSpace(item.Risk)));
                }

            sets = sets
                .GroupBy(item => new { item.EntityId, Risk = item.Risk.Trim().ToUpperInvariant() })
                .Select(group => group.First())
                .OrderBy(item => RiskSortOrder(item.Risk))
                .ThenBy(item => item.AuditDepartment)
                .ThenBy(item => item.EntityName)
                .ToList();

            _logger.LogInformation(
                "CIA summary set ZIP generation starting. DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalSets={TotalSets}.",
                auditDepartmentId,
                departmentName,
                normalizedRisk,
                sets.Count);

            var successCount = 0;
            var failureCount = 0;
            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                if (sets.Count == 0)
                    {
                    var noDataEntry = archive.CreateEntry("NO_DATA.txt", CompressionLevel.Fastest);
                    using var writer = new StreamWriter(noDataEntry.Open(), Encoding.UTF8);
                    writer.WriteLine("No outstanding audit paras found for the selected filters.");
                    }

                for (var setIndex = 0; setIndex < sets.Count; setIndex++)
                    {
                    var set = sets[setIndex];
                    var setStopwatch = Stopwatch.StartNew();
                    try
                        {
                        var rows = _dbConnection.GetOutstandingParasSummaryForPdfSet(auditDepartmentId, set.EntityId, set.Risk);
                        if (rows.Count == 0)
                            {
                            setStopwatch.Stop();
                            _logger.LogInformation(
                                "CIA summary set skipped because no rows were returned. CurrentSet={CurrentSet}, TotalSets={TotalSets}, EntityId={EntityId}, Risk={Risk}, RowCount={RowCount}, GenerationDurationMs={GenerationDurationMs}, SuccessCount={SuccessCount}, FailureCount={FailureCount}.",
                                setIndex + 1,
                                sets.Count,
                                set.EntityId,
                                set.Risk,
                                rows.Count,
                                setStopwatch.ElapsedMilliseconds,
                                successCount,
                                failureCount);
                            continue;
                            }

                        var partCount = (int)Math.Ceiling(rows.Count / (double)SummaryZipRowsPerPdf);
                        _logger.LogInformation(
                            "CIA summary set rows loaded. CurrentSet={CurrentSet}, TotalSets={TotalSets}, EntityId={EntityId}, EntityName={EntityName}, Risk={Risk}, RowCount={RowCount}, Parts={Parts}, RowsPerPdf={RowsPerPdf}, SuccessCount={SuccessCount}, FailureCount={FailureCount}.",
                            setIndex + 1,
                            sets.Count,
                            set.EntityId,
                            set.EntityName,
                            set.Risk,
                            rows.Count,
                            partCount,
                            SummaryZipRowsPerPdf,
                            successCount,
                            failureCount);

                        for (var partIndex = 0; partIndex < partCount; partIndex++)
                            {
                            var partStopwatch = Stopwatch.StartNew();
                            var partRows = rows.Skip(partIndex * SummaryZipRowsPerPdf).Take(SummaryZipRowsPerPdf).ToList();
                            var setDepartmentName = string.IsNullOrWhiteSpace(set.AuditDepartment) ? departmentName : set.AuditDepartment;
                            var data = new OutstandingParasSummaryPdfReportData
                                {
                                AuditDepartmentId = auditDepartmentId,
                                AuditDepartmentName = setDepartmentName,
                                Risk = set.Risk,
                                GeneratedOn = generatedOn,
                                Paras = partRows
                                };
                            var html = _pdfBuilder.BuildSummaryHtml(data);
                            var pdfBytes = await RenderSummaryPdfAsync(html, generatedOn);
                            partStopwatch.Stop();

                            if (pdfBytes == null || pdfBytes.Length == 0)
                                {
                                throw new InvalidOperationException("Generated PDF is empty.");
                                }

                            var entryName = BuildSummarySetEntryFilename(set, partIndex + 1);
                            var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                            await using (var entryStream = entry.Open())
                                {
                                await entryStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                                }

                            successCount++;
                            _logger.LogInformation(
                                "CIA summary set PDF completed. CurrentSet={CurrentSet}, TotalSets={TotalSets}, EntityId={EntityId}, EntityName={EntityName}, Risk={Risk}, RowCount={RowCount}, Part={Part}, Parts={Parts}, HtmlLength={HtmlLength}, GenerationDurationMs={GenerationDurationMs}, PdfSizeBytes={PdfSizeBytes}, SuccessCount={SuccessCount}, FailureCount={FailureCount}.",
                                setIndex + 1,
                                sets.Count,
                                set.EntityId,
                                set.EntityName,
                                set.Risk,
                                partRows.Count,
                                partIndex + 1,
                                partCount,
                                html?.Length ?? 0,
                                partStopwatch.ElapsedMilliseconds,
                                pdfBytes.Length,
                                successCount,
                                failureCount);
                            }

                        setStopwatch.Stop();
                        }
                    catch (Exception ex)
                        {
                        setStopwatch.Stop();
                        failureCount++;
                        var failureEntryName = SafeZipEntryName($"FAILED_ENTITY_{set.EntityId}_{set.Risk}.txt");
                        var failureEntry = archive.CreateEntry(failureEntryName, CompressionLevel.Fastest);
                        using (var writer = new StreamWriter(failureEntry.Open(), Encoding.UTF8))
                            {
                            writer.WriteLine($"Entity ID: {set.EntityId}");
                            writer.WriteLine($"Entity Name: {set.EntityName}");
                            writer.WriteLine($"Risk: {set.Risk}");
                            writer.WriteLine("PDF generation failed for this Entity/Risk set.");
                            writer.WriteLine();
                            writer.WriteLine(ex.Message);
                            }

                        _logger.LogError(
                            ex,
                            "CIA summary set PDF failed. CurrentSet={CurrentSet}, TotalSets={TotalSets}, EntityId={EntityId}, EntityName={EntityName}, Risk={Risk}, ExpectedRowCount={ExpectedRowCount}, GenerationDurationMs={GenerationDurationMs}, SuccessCount={SuccessCount}, FailureCount={FailureCount}.",
                            setIndex + 1,
                            sets.Count,
                            set.EntityId,
                            set.EntityName,
                            set.Risk,
                            set.RowCount,
                            setStopwatch.ElapsedMilliseconds,
                            successCount,
                            failureCount);
                        }
                    }
                }

            zipStopwatch.Stop();
            var zipBytes = zipStream.ToArray();
            _logger.LogInformation(
                "CIA summary set ZIP generation completed. DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalSets={TotalSets}, GenerationDurationMs={GenerationDurationMs}, PdfSizeBytes={PdfSizeBytes}, SuccessCount={SuccessCount}, FailureCount={FailureCount}.",
                auditDepartmentId,
                departmentName,
                normalizedRisk,
                sets.Count,
                zipStopwatch.ElapsedMilliseconds,
                zipBytes.Length,
                successCount,
                failureCount);

            return new OutstandingParasGeneratedPdfDocument
                {
                ContentBytes = zipBytes,
                ContentType = "application/zip",
                FileName = $"Consolidated_Outstanding_Audit_Paras_Summary_{SanitizeFilename(departmentName)}_{SanitizeFilename(normalizedRisk)}_{generatedOn:yyyyMMdd_HHmmss}.zip"
                };
            }

        public async Task<OutstandingParasSummaryBatchResult> GenerateSummaryBatchAsync(int auditDepartmentId, string risk)
            {
            try
                {
                CleanupOldSummaryBatches();

                var generatedOn = GetKarachiNow();
                var batchId = $"{generatedOn:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
                var batchRootPath = GetSummaryBatchRootPath();
                var batchPath = GetSafeChildPath(batchRootPath, batchId);
                Directory.CreateDirectory(batchPath);

                var normalizedRisk = string.IsNullOrWhiteSpace(risk) || string.Equals(risk.Trim(), "ALL", StringComparison.OrdinalIgnoreCase)
                    ? "All"
                    : risk.Trim();
                var departments = _dbConnection.GetAuditDepartments();
                var departmentName = auditDepartmentId <= 0
                    ? "All"
                    : departments.FirstOrDefault(item => item.ID == auditDepartmentId)?.NAME ?? "All";

                var result = new OutstandingParasSummaryBatchResult
                    {
                    BatchId = batchId,
                    FolderPath = batchPath,
                    FolderRelativeUrl = $"/PDF/CIAOutstandingParas/{batchId}"
                    };

                if (auditDepartmentId <= 0 || IsAllRisk(normalizedRisk))
                    {
                    await GenerateSummaryBatchByEntityRiskSetsAsync(result, auditDepartmentId, departmentName, normalizedRisk, generatedOn, batchPath);
                    }
                else
                    {
                    await GenerateSpecificSummaryBatchAsync(result, auditDepartmentId, departmentName, normalizedRisk, generatedOn, batchPath);
                    }

                _logger.LogInformation(
                    "CIA summary batch completed. BatchId={BatchId}, DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalSets={TotalSets}, SuccessCount={SuccessCount}, FailureCount={FailureCount}, FolderPath={FolderPath}.",
                    batchId,
                    auditDepartmentId,
                    departmentName,
                    normalizedRisk,
                    result.TotalSets,
                    result.SuccessCount,
                    result.FailureCount,
                    batchPath);

                return result;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to generate CIA consolidated outstanding paras summary batch.");
                return OutstandingParasSummaryBatchResult.Fail(500, "An error occurred while generating the PDF batch. Please try again later.");
                }
            }

        public bool DeleteSummaryBatch(string batchId)
            {
            if (!IsSafeBatchId(batchId))
                {
                _logger.LogWarning("Rejected CIA summary batch delete for invalid BatchId={BatchId}.", batchId);
                return false;
                }

            try
                {
                var batchRootPath = GetSummaryBatchRootPath();
                var batchPath = GetSafeChildPath(batchRootPath, batchId.Trim());
                if (!Directory.Exists(batchPath))
                    {
                    _logger.LogInformation("CIA summary batch delete requested for missing folder. BatchId={BatchId}, FolderPath={FolderPath}.", batchId, batchPath);
                    return true;
                    }

                Directory.Delete(batchPath, true);
                _logger.LogInformation("CIA summary batch folder deleted. BatchId={BatchId}, FolderPath={FolderPath}.", batchId, batchPath);
                return true;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Failed to delete CIA summary batch folder. BatchId={BatchId}.", batchId);
                return false;
                }
            }

        private async Task GenerateSpecificSummaryBatchAsync(OutstandingParasSummaryBatchResult result, int auditDepartmentId, string departmentName, string normalizedRisk, DateTime generatedOn, string batchPath)
            {
            var rows = _dbConnection.GetOutstandingParasSummaryForPdf(auditDepartmentId, normalizedRisk);
            result.TotalSets = rows.Count > 0 ? 1 : 0;
            _logger.LogInformation(
                "CIA summary batch specific generation starting. BatchId={BatchId}, DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalRows={TotalRows}, TotalSets={TotalSets}.",
                result.BatchId,
                auditDepartmentId,
                departmentName,
                normalizedRisk,
                rows.Count,
                result.TotalSets);

            if (rows.Count == 0)
                {
                await SaveSummaryFailureFileAsync(result, batchPath, "NO_DATA.txt", "No outstanding audit paras found for the selected filters.");
                return;
                }

            var partCount = (int)Math.Ceiling(rows.Count / (double)SummaryZipRowsPerPdf);
            for (var partIndex = 0; partIndex < partCount; partIndex++)
                {
                var partRows = rows.Skip(partIndex * SummaryZipRowsPerPdf).Take(SummaryZipRowsPerPdf).ToList();
                var partName = $"Consolidated_Outstanding_Audit_Paras_Summary_{SanitizeFilename(departmentName)}_{SanitizeFilename(normalizedRisk)}_Part_{partIndex + 1:00}.pdf";
                try
                    {
                    await GenerateAndSaveSummaryPdfPartAsync(
                        result,
                        batchPath,
                        auditDepartmentId,
                        departmentName,
                        normalizedRisk,
                        generatedOn,
                        partRows,
                        partName,
                        1,
                        1,
                        0,
                        string.Empty,
                        partIndex + 1,
                        partCount);
                    }
                catch (Exception ex)
                    {
                    result.FailureCount++;
                    await SaveSummaryFailureFileAsync(
                        result,
                        batchPath,
                        $"FAILED_ENTITY_0_{normalizedRisk}_Part_{partIndex + 1:00}.txt",
                        $"Department ID: {auditDepartmentId}{Environment.NewLine}Department: {departmentName}{Environment.NewLine}Risk: {normalizedRisk}{Environment.NewLine}PDF generation failed for this summary part.{Environment.NewLine}{Environment.NewLine}{ex.Message}");
                    _logger.LogError(
                        ex,
                        "CIA summary batch specific part failed. BatchId={BatchId}, DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, Part={Part}, Parts={Parts}, RowCount={RowCount}, SuccessCount={SuccessCount}, FailureCount={FailureCount}, FolderPath={FolderPath}.",
                        result.BatchId,
                        auditDepartmentId,
                        departmentName,
                        normalizedRisk,
                        partIndex + 1,
                        partCount,
                        partRows.Count,
                        result.SuccessCount,
                        result.FailureCount,
                        batchPath);
                    }
                }
            }

        private async Task GenerateSummaryBatchByEntityRiskSetsAsync(OutstandingParasSummaryBatchResult result, int auditDepartmentId, string departmentName, string normalizedRisk, DateTime generatedOn, string batchPath)
            {
            var risks = IsAllRisk(normalizedRisk)
                ? new[] { "High", "Medium", "Low" }
                : new[] { normalizedRisk };
            var sets = new List<OutstandingParasSummarySetModel>();

            foreach (var setRisk in risks)
                {
                sets.AddRange(_dbConnection.GetOutstandingParasSummarySetsForPdf(auditDepartmentId, setRisk)
                    .Where(item => item.EntityId > 0 && !string.IsNullOrWhiteSpace(item.Risk)));
                }

            sets = sets
                .GroupBy(item => new { item.EntityId, Risk = item.Risk.Trim().ToUpperInvariant() })
                .Select(group => group.First())
                .OrderBy(item => RiskSortOrder(item.Risk))
                .ThenBy(item => item.AuditDepartment)
                .ThenBy(item => item.EntityName)
                .ToList();

            result.TotalSets = sets.Count;
            _logger.LogInformation(
                "CIA summary batch set generation starting. BatchId={BatchId}, DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalSets={TotalSets}.",
                result.BatchId,
                auditDepartmentId,
                departmentName,
                normalizedRisk,
                sets.Count);

            if (sets.Count == 0)
                {
                await SaveSummaryFailureFileAsync(result, batchPath, "NO_DATA.txt", "No outstanding audit paras found for the selected filters.");
                return;
                }

            for (var setIndex = 0; setIndex < sets.Count; setIndex++)
                {
                var set = sets[setIndex];
                var setStopwatch = Stopwatch.StartNew();
                try
                    {
                    var rows = _dbConnection.GetOutstandingParasSummaryForPdfSet(auditDepartmentId, set.EntityId, set.Risk);
                    if (rows.Count == 0)
                        {
                        setStopwatch.Stop();
                        _logger.LogInformation(
                            "CIA summary batch set skipped because no rows were returned. BatchId={BatchId}, CurrentSet={CurrentSet}, TotalSets={TotalSets}, EntityId={EntityId}, EntityName={EntityName}, Risk={Risk}, RowCount={RowCount}, DurationMs={DurationMs}, SuccessCount={SuccessCount}, FailureCount={FailureCount}.",
                            result.BatchId,
                            setIndex + 1,
                            sets.Count,
                            set.EntityId,
                            set.EntityName,
                            set.Risk,
                            rows.Count,
                            setStopwatch.ElapsedMilliseconds,
                            result.SuccessCount,
                            result.FailureCount);
                        continue;
                        }

                    var partCount = (int)Math.Ceiling(rows.Count / (double)SummaryZipRowsPerPdf);
                    _logger.LogInformation(
                        "CIA summary batch set rows loaded. BatchId={BatchId}, CurrentSet={CurrentSet}, TotalSets={TotalSets}, EntityId={EntityId}, EntityName={EntityName}, Risk={Risk}, RowCount={RowCount}, Parts={Parts}.",
                        result.BatchId,
                        setIndex + 1,
                        sets.Count,
                        set.EntityId,
                        set.EntityName,
                        set.Risk,
                        rows.Count,
                        partCount);

                    for (var partIndex = 0; partIndex < partCount; partIndex++)
                        {
                        var partRows = rows.Skip(partIndex * SummaryZipRowsPerPdf).Take(SummaryZipRowsPerPdf).ToList();
                        var setDepartmentName = string.IsNullOrWhiteSpace(set.AuditDepartment) ? departmentName : set.AuditDepartment;
                        var fileName = BuildSummarySetEntryFilename(set, partIndex + 1);
                        await GenerateAndSaveSummaryPdfPartAsync(
                            result,
                            batchPath,
                            auditDepartmentId,
                            setDepartmentName,
                            set.Risk,
                            generatedOn,
                            partRows,
                            fileName,
                            setIndex + 1,
                            sets.Count,
                            set.EntityId,
                            set.EntityName,
                            partIndex + 1,
                            partCount);
                        }

                    setStopwatch.Stop();
                    }
                catch (Exception ex)
                    {
                    setStopwatch.Stop();
                    result.FailureCount++;
                    var failureFileName = SafeZipEntryName($"FAILED_ENTITY_{set.EntityId}_{set.Risk}.txt");
                    await SaveSummaryFailureFileAsync(
                        result,
                        batchPath,
                        failureFileName,
                        $"Entity ID: {set.EntityId}{Environment.NewLine}Entity Name: {set.EntityName}{Environment.NewLine}Risk: {set.Risk}{Environment.NewLine}PDF generation failed for this Entity/Risk set.{Environment.NewLine}{Environment.NewLine}{ex.Message}");

                    _logger.LogError(
                        ex,
                        "CIA summary batch set failed. BatchId={BatchId}, CurrentSet={CurrentSet}, TotalSets={TotalSets}, EntityId={EntityId}, EntityName={EntityName}, Risk={Risk}, ExpectedRowCount={ExpectedRowCount}, DurationMs={DurationMs}, SuccessCount={SuccessCount}, FailureCount={FailureCount}, FolderPath={FolderPath}.",
                        result.BatchId,
                        setIndex + 1,
                        sets.Count,
                        set.EntityId,
                        set.EntityName,
                        set.Risk,
                        set.RowCount,
                        setStopwatch.ElapsedMilliseconds,
                        result.SuccessCount,
                        result.FailureCount,
                        batchPath);
                    }
                }
            }

        private async Task GenerateAndSaveSummaryPdfPartAsync(OutstandingParasSummaryBatchResult result, string batchPath, int auditDepartmentId, string departmentName, string risk, DateTime generatedOn, List<OutstandingParasSummaryPdfModel> rows, string fileName, int currentSet, int totalSets, int entityId, string entityName, int part, int parts)
            {
            var partStopwatch = Stopwatch.StartNew();
            var data = new OutstandingParasSummaryPdfReportData
                {
                AuditDepartmentId = auditDepartmentId,
                AuditDepartmentName = departmentName,
                Risk = risk,
                GeneratedOn = generatedOn,
                Paras = rows
                };
            var html = _pdfBuilder.BuildSummaryHtml(data);
            var pdfBytes = await RenderSummaryPdfAsync(html, generatedOn);
            partStopwatch.Stop();

            if (pdfBytes == null || pdfBytes.Length == 0)
                {
                throw new InvalidOperationException("Generated PDF is empty.");
                }

            var safeFileName = SanitizeFilename(fileName);
            var filePath = GetSafeChildPath(batchPath, safeFileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            var savedPdf = _dbConnection.SaveCiaSummaryPdf(
                result.BatchId,
                auditDepartmentId,
                departmentName,
                entityId,
                entityName,
                risk,
                part,
                safeFileName,
                "application/pdf",
                pdfBytes,
                GetGeneratedBy(),
                generatedOn.AddDays(30),
                "GENERATED",
                string.Empty);
            if (!savedPdf.IsSuccess)
                {
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(savedPdf.Message) ? "Unable to save generated PDF to database." : savedPdf.Message);
                }

            result.SuccessCount++;
            result.Files.Add(new OutstandingParasSummaryBatchFileModel
                {
                PdfId = savedPdf.PdfId,
                FileName = safeFileName,
                Url = $"/OutstandingParasPdf/DownloadSummaryPdf?pdfId={savedPdf.PdfId}",
                ContentType = "application/pdf",
                SizeBytes = pdfBytes.LongLength
                });

            _logger.LogInformation(
                "CIA summary batch PDF saved. BatchId={BatchId}, PdfId={PdfId}, CurrentSet={CurrentSet}, TotalSets={TotalSets}, EntityId={EntityId}, EntityName={EntityName}, Risk={Risk}, RowCount={RowCount}, Part={Part}, Parts={Parts}, HtmlLength={HtmlLength}, DurationMs={DurationMs}, PdfSizeBytes={PdfSizeBytes}, FilePath={FilePath}, SuccessCount={SuccessCount}, FailureCount={FailureCount}.",
                result.BatchId,
                savedPdf.PdfId,
                currentSet,
                totalSets,
                entityId,
                entityName,
                risk,
                rows?.Count ?? 0,
                part,
                parts,
                html?.Length ?? 0,
                partStopwatch.ElapsedMilliseconds,
                pdfBytes.Length,
                filePath,
                result.SuccessCount,
                result.FailureCount);
            }

        private async Task SaveSummaryFailureFileAsync(OutstandingParasSummaryBatchResult result, string batchPath, string fileName, string content)
            {
            var safeFileName = SanitizeFilename(fileName);
            var filePath = GetSafeChildPath(batchPath, safeFileName);
            await File.WriteAllTextAsync(filePath, content ?? string.Empty, Encoding.UTF8);
            var fileInfo = new FileInfo(filePath);
            result.Files.Add(new OutstandingParasSummaryBatchFileModel
                {
                FileName = safeFileName,
                Url = $"{result.FolderRelativeUrl}/{Uri.EscapeDataString(safeFileName)}",
                ContentType = "text/plain",
                SizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
                IsFailureFile = true
                });
            _logger.LogInformation("CIA summary batch failure file saved. BatchId={BatchId}, FilePath={FilePath}, SizeBytes={SizeBytes}.", result.BatchId, filePath, fileInfo.Exists ? fileInfo.Length : 0);
            }

        private async Task<OutstandingParasGeneratedPdfDocument> GenerateSummaryZipFallbackAsync(OutstandingParasSummaryPdfReportData data)
            {
            var rows = data.Paras ?? new List<OutstandingParasSummaryPdfModel>();
            var partCount = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)SummaryZipRowsPerPdf));
            var zipStopwatch = Stopwatch.StartNew();
            _logger.LogInformation(
                "CIA summary ZIP fallback starting. DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalRows={TotalRows}, Parts={Parts}, RowsPerPdf={RowsPerPdf}.",
                data.AuditDepartmentId,
                data.AuditDepartmentName,
                data.Risk,
                rows.Count,
                partCount,
                SummaryZipRowsPerPdf);

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                for (var partIndex = 0; partIndex < partCount; partIndex++)
                    {
                    var partStopwatch = Stopwatch.StartNew();
                    var partRows = rows.Skip(partIndex * SummaryZipRowsPerPdf).Take(SummaryZipRowsPerPdf).ToList();
                    var partData = new OutstandingParasSummaryPdfReportData
                        {
                        AuditDepartmentId = data.AuditDepartmentId,
                        AuditDepartmentName = data.AuditDepartmentName,
                        Risk = data.Risk,
                        ReportTitle = data.ReportTitle,
                        GeneratedOn = data.GeneratedOn,
                        Paras = partRows
                        };
                    var partHtml = _pdfBuilder.BuildSummaryHtml(partData);
                    byte[] partBytes;
                    try
                        {
                        partBytes = await RenderSummaryPdfAsync(partHtml, partData.GeneratedOn);
                        }
                    catch (Exception ex)
                        {
                        partStopwatch.Stop();
                        _logger.LogError(
                            ex,
                            "CIA summary ZIP fallback part failed. Part={Part}, DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, Rows={Rows}, HtmlLength={HtmlLength}, GenerationDurationMs={GenerationDurationMs}.",
                            partIndex + 1,
                            data.AuditDepartmentId,
                            data.AuditDepartmentName,
                            data.Risk,
                            partRows.Count,
                            partHtml?.Length ?? 0,
                            partStopwatch.ElapsedMilliseconds);
                        return OutstandingParasGeneratedPdfDocument.Fail(500, "PDF generation failed while creating split summary PDFs. Please try narrower filters.");
                        }

                    partStopwatch.Stop();

                    if (partBytes == null || partBytes.Length == 0)
                        {
                        _logger.LogError(
                            "CIA summary ZIP fallback part failed. Part={Part}, DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, Rows={Rows}, HtmlLength={HtmlLength}, GenerationDurationMs={GenerationDurationMs}.",
                            partIndex + 1,
                            data.AuditDepartmentId,
                            data.AuditDepartmentName,
                            data.Risk,
                            partRows.Count,
                            partHtml?.Length ?? 0,
                            partStopwatch.ElapsedMilliseconds);
                        return OutstandingParasGeneratedPdfDocument.Fail(500, "PDF generation failed while creating split summary PDFs. Please try narrower filters.");
                        }

                    var entry = archive.CreateEntry($"CIA_Outstanding_Paras_Summary_Part_{partIndex + 1:00}.pdf", CompressionLevel.Fastest);
                    await using (var entryStream = entry.Open())
                        {
                        await entryStream.WriteAsync(partBytes, 0, partBytes.Length);
                        }

                    _logger.LogInformation(
                        "CIA summary ZIP fallback part completed. Part={Part}, DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, Rows={Rows}, HtmlLength={HtmlLength}, GenerationDurationMs={GenerationDurationMs}, PdfSizeBytes={PdfSizeBytes}.",
                        partIndex + 1,
                        data.AuditDepartmentId,
                        data.AuditDepartmentName,
                        data.Risk,
                        partRows.Count,
                        partHtml?.Length ?? 0,
                        partStopwatch.ElapsedMilliseconds,
                        partBytes.Length);
                    }
                }

            zipStopwatch.Stop();
            var zipBytes = zipStream.ToArray();
            _logger.LogInformation(
                "CIA summary ZIP fallback completed. DepartmentId={AuditDepartmentId}, Department={AuditDepartment}, Risk={Risk}, TotalRows={TotalRows}, Parts={Parts}, GenerationDurationMs={GenerationDurationMs}, PdfSizeBytes={PdfSizeBytes}.",
                data.AuditDepartmentId,
                data.AuditDepartmentName,
                data.Risk,
                rows.Count,
                partCount,
                zipStopwatch.ElapsedMilliseconds,
                zipBytes.Length);

            return new OutstandingParasGeneratedPdfDocument
                {
                ContentBytes = zipBytes,
                ContentType = "application/zip",
                FileName = BuildSummaryZipFilename(data)
                };
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

        private string GetGeneratedBy()
            {
            return _sessionHandler.TryGetUser(out var user) && user != null
                ? user.PPNumber?.Trim()
                : string.Empty;
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

        private static byte[] RenderSummaryPdf(string html, DateTime generatedOn)
            {
            using var output = new MemoryStream();
            using (var writer = new PdfWriter(output))
            using (var pdf = new PdfDocument(writer))
                {
                pdf.SetDefaultPageSize(PageSize.A4.Rotate());
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new SummaryPageFooterEventHandler(generatedOn));
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

        private static async Task<byte[]> RenderSummaryPdfAsync(string html, DateTime generatedOn)
            {
            return await Task.Run(() => RenderSummaryPdf(html, generatedOn));
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

        private static string BuildSummaryFilename(OutstandingParasSummaryPdfReportData data)
            {
            return $"Consolidated_Outstanding_Audit_Paras_Summary_{SanitizeFilename(data.AuditDepartmentName)}_{SanitizeFilename(data.Risk)}_{data.GeneratedOn:yyyyMMdd_HHmmss}.pdf";
            }

        private static string BuildSummaryZipFilename(OutstandingParasSummaryPdfReportData data)
            {
            return $"Consolidated_Outstanding_Audit_Paras_Summary_{SanitizeFilename(data.AuditDepartmentName)}_{SanitizeFilename(data.Risk)}_{data.GeneratedOn:yyyyMMdd_HHmmss}.zip";
            }

        private static string BuildSummarySetEntryFilename(OutstandingParasSummarySetModel set, int partNumber)
            {
            return SafeZipEntryName($"CIA_Summary_{SanitizeFilename(set?.AuditDepartment)}_{SanitizeFilename(set?.EntityName)}_Entity_{set?.EntityId ?? 0}_{SanitizeFilename(set?.Risk)}_Part_{partNumber:00}.pdf");
            }

        private string GetSummaryBatchRootPath()
            {
            var webRootPath = string.IsNullOrWhiteSpace(_webHostEnvironment?.WebRootPath)
                ? System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                : _webHostEnvironment.WebRootPath;
            return System.IO.Path.GetFullPath(System.IO.Path.Combine(webRootPath, "PDF", "CIAOutstandingParas"));
            }

        private static string GetSafeChildPath(string parentPath, string childName)
            {
            var parentFullPath = System.IO.Path.GetFullPath(parentPath);
            var childFullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(parentFullPath, childName));
            var parentWithSeparator = parentFullPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? parentFullPath
                : parentFullPath + System.IO.Path.DirectorySeparatorChar;
            if (!childFullPath.StartsWith(parentWithSeparator, StringComparison.OrdinalIgnoreCase) && !string.Equals(childFullPath, parentFullPath, StringComparison.OrdinalIgnoreCase))
                {
                throw new InvalidOperationException("Invalid output path.");
                }

            return childFullPath;
            }

        private void CleanupOldSummaryBatches()
            {
            try
                {
                var rootPath = GetSummaryBatchRootPath();
                if (!Directory.Exists(rootPath))
                    {
                    return;
                    }

                var cutoffUtc = DateTime.UtcNow.AddHours(-SummaryBatchRetentionHours);
                foreach (var directory in Directory.EnumerateDirectories(rootPath))
                    {
                    try
                        {
                        var info = new DirectoryInfo(directory);
                        if (info.CreationTimeUtc < cutoffUtc)
                            {
                            info.Delete(true);
                            _logger.LogInformation("Deleted old CIA summary batch folder. FolderPath={FolderPath}.", info.FullName);
                            }
                        }
                    catch (Exception ex)
                        {
                        _logger.LogWarning(ex, "Unable to delete old CIA summary batch folder. FolderPath={FolderPath}.", directory);
                        }
                    }
                }
            catch (Exception ex)
                {
                _logger.LogWarning(ex, "Unable to clean old CIA summary batch folders.");
                }
            }

        private static bool IsSafeBatchId(string batchId)
            {
            if (string.IsNullOrWhiteSpace(batchId))
                {
                return false;
                }

            var value = batchId.Trim();
            return value.Length <= 80 && value.All(item => char.IsLetterOrDigit(item) || item == '_' || item == '-');
            }

        private static bool IsAllRisk(string risk)
            {
            return string.IsNullOrWhiteSpace(risk) || string.Equals(risk.Trim(), "ALL", StringComparison.OrdinalIgnoreCase) || string.Equals(risk.Trim(), "All", StringComparison.OrdinalIgnoreCase);
            }

        private static int RiskSortOrder(string risk)
            {
            if (string.Equals(risk, "High", StringComparison.OrdinalIgnoreCase))
                {
                return 1;
                }

            if (string.Equals(risk, "Medium", StringComparison.OrdinalIgnoreCase))
                {
                return 2;
                }

            if (string.Equals(risk, "Low", StringComparison.OrdinalIgnoreCase))
                {
                return 3;
                }

            return 4;
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

        private static string SafeZipEntryName(string value)
            {
            return SanitizeFilename(value).Replace('\\', '_').Replace('/', '_');
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

        private sealed class SummaryPageFooterEventHandler : IEventHandler
            {
            private readonly DateTime _generatedOn;

            public SummaryPageFooterEventHandler(DateTime generatedOn)
                {
                _generatedOn = generatedOn;
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

                canvas.SetFontColor(ColorConstants.DARK_GRAY);
                canvas.SetFontSize(8);
                canvas.ShowTextAligned($"CONFIDENTIAL | Generated on {FormatDateTime(_generatedOn)} | Page {pageNumber}", pageSize.GetWidth() / 2, pageSize.GetBottom() + 18, TextAlignment.CENTER);
                }
            }
        }
    }
