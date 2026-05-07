using System;
using System.Collections.Generic;

namespace AIS.Models
    {
    public class OutstandingParasPdfReportData
        {
        public int AuditDepartmentId { get; set; }
        public string AuditDepartmentName { get; set; }
        public string ReportTitle { get; set; } = "Audit Report";
        public DateTime ExecutionStartDate { get; set; }
        public DateTime ExecutionEndDate { get; set; }
        public string GeneratedByName { get; set; }
        public string GeneratedByPPNo { get; set; }
        public DateTime GeneratedOn { get; set; } = DateTime.Now;
        public List<OutstandingParaEntityPdfModel> Entities { get; set; } = new List<OutstandingParaEntityPdfModel>();
        public List<OutstandingParaPdfModel> Paras { get; set; } = new List<OutstandingParaPdfModel>();
        }

    public class OutstandingParasSummaryPdfReportData
        {
        public int AuditDepartmentId { get; set; }
        public string AuditDepartmentName { get; set; }
        public string Risk { get; set; }
        public string ReportTitle { get; set; } = "Consolidated Outstanding Audit Paras Summary";
        public DateTime GeneratedOn { get; set; } = DateTime.Now;
        public List<OutstandingParasSummaryPdfModel> Paras { get; set; } = new List<OutstandingParasSummaryPdfModel>();
        }

    public class OutstandingParaEntityPdfModel
        {
        public int EngagementId { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string EntityCode { get; set; }
        public string AuditDepartment { get; set; }
        public string AuditPeriod { get; set; }
        public DateTime? ExecutionStartDate { get; set; }
        public DateTime? ExecutionEndDate { get; set; }
        public string TeamLead { get; set; }
        public string TeamMembers { get; set; }
        public int OutstandingParasCount { get; set; }
        }

    public class OutstandingParaPdfModel
        {
        public int EngagementId { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string ParaNo { get; set; }
        public string ParaTitle { get; set; }
        public string RiskCategory { get; set; }
        public string ObservationText { get; set; }
        public string LatestManagementResponse { get; set; }
        public string AuditRemarks { get; set; }
        public string CurrentComplianceStatus { get; set; }
        }

    public class OutstandingParasSummaryPdfModel
        {
        public int EntityId { get; set; }
        public string AuditDepartment { get; set; }
        public string EntityName { get; set; }
        public string ParaNo { get; set; }
        public string AuditPeriod { get; set; }
        public string GistHeading { get; set; }
        public string Risk { get; set; }
        public string ParaText { get; set; }
        public string CurrentComplianceStatus { get; set; }
        }

    public class OutstandingParasSummarySetModel
        {
        public int EntityId { get; set; }
        public string AuditDepartment { get; set; }
        public string EntityName { get; set; }
        public string Risk { get; set; }
        public int RowCount { get; set; }
        }

    public class OutstandingParasSummaryBatchFileModel
        {
        public int PdfId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/pdf";
        public long SizeBytes { get; set; }
        public bool IsFailureFile { get; set; }
        }

    public class OutstandingParasSummaryBatchResult
        {
        public string BatchId { get; set; } = string.Empty;
        public int TotalSets { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public string FolderRelativeUrl { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public List<OutstandingParasSummaryBatchFileModel> Files { get; set; } = new List<OutstandingParasSummaryBatchFileModel>();
        public int FailureStatusCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsSuccess => string.IsNullOrWhiteSpace(ErrorMessage);

        public static OutstandingParasSummaryBatchResult Fail(int failureStatusCode, string errorMessage)
            {
            return new OutstandingParasSummaryBatchResult
                {
                FailureStatusCode = failureStatusCode,
                ErrorMessage = errorMessage ?? string.Empty
                };
            }
        }

    public class OutstandingParasSummaryBatchDeleteRequest
        {
        public string BatchId { get; set; } = string.Empty;
        }

    public class OutstandingParasSummaryPdfStoreModel
        {
        public int PdfId { get; set; }
        public string BatchId { get; set; } = string.Empty;
        public int AuditDepartmentId { get; set; }
        public string AuditDepartmentName { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Risk { get; set; } = string.Empty;
        public int PartNo { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileMimeType { get; set; } = "application/pdf";
        public long FileSize { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public DateTime? GeneratedOn { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        }

    public class OutstandingParasSummaryPdfSaveResult
        {
        public int PdfId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess => string.Equals(Status, "SUCCESS", StringComparison.OrdinalIgnoreCase) && PdfId > 0;
        }

    public class OutstandingParasSummaryPdfDownloadModel
        {
        public int PdfId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileMimeType { get; set; } = "application/pdf";
        public long FileSize { get; set; }
        public byte[] ContentBytes { get; set; } = Array.Empty<byte>();
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess => string.Equals(Status, "SUCCESS", StringComparison.OrdinalIgnoreCase) && ContentBytes != null && ContentBytes.Length > 0;
        }

    public class OutstandingParasSummaryPdfDeleteRequest
        {
        public int PdfId { get; set; }
        }

    public class OutstandingParasSummaryPdfZipExportRequest
        {
        public List<int> PdfIds { get; set; } = new List<int>();
        }

    public class OutstandingParasSummaryPdfDeleteResult
        {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess => string.Equals(Status, "SUCCESS", StringComparison.OrdinalIgnoreCase);
        }

    public class OutstandingParasGeneratedPdfDocument
        {
        public byte[] ContentBytes { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/pdf";
        public int FailureStatusCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsSuccess => ContentBytes != null && ContentBytes.Length > 0;

        public static OutstandingParasGeneratedPdfDocument Fail(int failureStatusCode, string errorMessage)
            {
            return new OutstandingParasGeneratedPdfDocument
                {
                FailureStatusCode = failureStatusCode,
                ErrorMessage = errorMessage ?? string.Empty
                };
            }
        }

    public class OutstandingParasZipExportRequest
        {
        public List<int> EngagementIds { get; set; } = new List<int>();
        }
    }
