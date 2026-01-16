using System;
using System.Collections.Generic;

namespace AIS.Models.FieldAuditReport
    {
    public class FieldAuditPdfReportData
        {
        public FieldAuditPdfHeaderModel Header { get; set; } = new FieldAuditPdfHeaderModel();
        public FieldAuditPdfReportMetaModel ReportMeta { get; set; } = new FieldAuditPdfReportMetaModel();
        public List<FieldAuditPdfSectionModel> Sections { get; set; } = new List<FieldAuditPdfSectionModel>();
        public List<FieldAuditPdfKpiRowModel> KpiRows { get; set; } = new List<FieldAuditPdfKpiRowModel>();
        public List<FieldAuditPdfNplRowModel> NplRows { get; set; } = new List<FieldAuditPdfNplRowModel>();
        public List<FieldAuditPdfStaffRowModel> StaffRows { get; set; } = new List<FieldAuditPdfStaffRowModel>();
        public List<FieldAuditPdfParaModel> Paras { get; set; } = new List<FieldAuditPdfParaModel>();
        public List<FieldAuditPdfStatisticsRowModel> StatisticsRows { get; set; } = new List<FieldAuditPdfStatisticsRowModel>();
        public List<FieldAuditPdfIncomeLeakageRowModel> IncomeLeakageRows { get; set; } = new List<FieldAuditPdfIncomeLeakageRowModel>();
        }

    public class FieldAuditPdfHeaderModel
        {
        public string BankName { get; set; }
        public string InternalAuditDivision { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string AuditPeriod { get; set; }
        public DateTime? AuditStartDate { get; set; }
        public DateTime? AuditEndDate { get; set; }
        public string ReportStatus { get; set; }
        public string VersionNumber { get; set; }
        public string EntityName { get; set; }
        public List<FieldAuditPdfKeyValueModel> BranchProfileRows { get; set; } = new List<FieldAuditPdfKeyValueModel>();
        }

    public class FieldAuditPdfReportMetaModel
        {
        public string ReportStatus { get; set; }
        public string VersionNumber { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime? GeneratedOn { get; set; }
        public string EntityName { get; set; }
        public string AuditPeriod { get; set; }
        }

    public class FieldAuditPdfSectionModel
        {
        public string SectionCode { get; set; }
        public string SectionTitle { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsMandatory { get; set; }
        public string HtmlContent { get; set; }
        }

    public class FieldAuditPdfKpiRowModel
        {
        public string KpiCode { get; set; }
        public string KpiLabel { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public decimal? ActualValue { get; set; }
        public decimal? TargetValue { get; set; }
        public string Unit { get; set; }
        }

    public class FieldAuditPdfNplRowModel
        {
        public string Category { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public int? CaseCount { get; set; }
        public decimal? OutstandingAmount { get; set; }
        public decimal? ProvisionAmount { get; set; }
        }

    public class FieldAuditPdfStaffRowModel
        {
        public string Designation { get; set; }
        public int? Strength { get; set; }
        public DateTime? AsOfDate { get; set; }
        }

    public class FieldAuditPdfStatisticsRowModel
        {
        public string RiskLevel { get; set; }
        public int? ReportedCount { get; set; }
        public int? RectifiedCount { get; set; }
        public int? OutstandingCount { get; set; }
        }

    public class FieldAuditPdfIncomeLeakageRowModel
        {
        public string CaseReference { get; set; }
        public string Description { get; set; }
        public decimal? Amount { get; set; }
        }

    public class FieldAuditPdfParaModel
        {
        public string ParaNo { get; set; }
        public string Gist { get; set; }
        public string Nature { get; set; }
        public string Risk { get; set; }
        public string AnnexureCode { get; set; }
        public string Instances { get; set; }
        public string Amount { get; set; }
        public string ParaDetail { get; set; }
        public string Implications { get; set; }
        public string Recommendations { get; set; }
        public string ManagementComments { get; set; }
        public string AuditorComments { get; set; }
        public string RemarksInCharge { get; set; }
        public bool IsSignificant { get; set; }
        }

    public class FieldAuditPdfKeyValueModel
        {
        public string Label { get; set; }
        public string Value { get; set; }
        }
    }
