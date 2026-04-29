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
