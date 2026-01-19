using AIS.Models;
using System;
using System.Collections.Generic;

namespace AIS.Models.FieldAuditReport
    {
    public class FieldAuditReportOverviewModel
        {
        public int EngId { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string EntityCode { get; set; }
        public string AuditPeriod { get; set; }
        public DateTime? AuditStartDate { get; set; }
        public DateTime? AuditEndDate { get; set; }
        public string TeamName { get; set; }
        public string VersionNo { get; set; }
        public DateTime? GeneratedOn { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime? FinalizedOn { get; set; }
        public string FinalizedBy { get; set; }
        }

    public class FieldAuditReportChecklistModel
        {
        public bool HasKpiSnapshot { get; set; }
        public bool HasNplSnapshot { get; set; }
        public bool HasStaffSnapshot { get; set; }
        public bool MandatoryNarrativesComplete { get; set; }
        public bool IsComplete => HasKpiSnapshot && HasNplSnapshot && HasStaffSnapshot && MandatoryNarrativesComplete;
        }

    public class FieldAuditReportOverviewViewModel
        {
        public FieldAuditReportOverviewModel Overview { get; set; } = new FieldAuditReportOverviewModel();
        public FieldAuditReportChecklistModel Checklist { get; set; } = new FieldAuditReportChecklistModel();
        public bool IsFinal { get; set; }
        public bool CanFinalize { get; set; }
        public string ReportStatus { get; set; }
        }

    public class FieldAuditEngagementOptionModel
        {
        // Used by selector
        public int EngagementId { get; set; }
        public string EntityName { get; set; }
        public string AuditPeriod { get; set; }

        // Internal / DB context (optional but valid)
        public int EngId { get; set; }
        public int EntityId { get; set; }
        }


    public class FieldAuditEngagementSelectorViewModel
        {
        public int? ActiveEngagementId { get; set; }
        public string ActiveEngagementLabel { get; set; }
        public bool HasExistingData { get; set; }
        public List<FieldAuditEngagementOptionModel> Options { get; set; } = new List<FieldAuditEngagementOptionModel>();
        public bool HasActiveEngagement => ActiveEngagementId.HasValue && ActiveEngagementId.Value > 0;
        }

    public class FieldAuditNarrativeSectionModel
        {
        public string SectionCode { get; set; }
        public string SectionTitle { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsMandatory { get; set; }
        public string TextBlock { get; set; }
        }

    public class NarrativeSectionsViewModel
        {
        public int EngagementId { get; set; }
        public bool IsReadOnly { get; set; }
        public List<FieldAuditNarrativeSectionModel> Sections { get; set; } = new List<FieldAuditNarrativeSectionModel>();
        }

    public class KpiSnapshotRowModel
        {
        public string KpiCode { get; set; }
        public string KpiLabel { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public decimal? ActualValue { get; set; }
        public decimal? TargetValue { get; set; }
        public string Unit { get; set; }
        }

    public class KpiSnapshotViewModel
        {
        public int EngagementId { get; set; }
        public bool IsReadOnly { get; set; }
        public List<KpiSnapshotRowModel> Rows { get; set; } = new List<KpiSnapshotRowModel>();
        }

    public class NplSnapshotRowModel
        {
        public string Category { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public int? CaseCount { get; set; }
        public decimal? OutstandingAmount { get; set; }
        public decimal? ProvisionAmount { get; set; }
        }

    public class NplSnapshotViewModel
        {
        public int EngagementId { get; set; }
        public bool IsReadOnly { get; set; }
        public List<NplSnapshotRowModel> Rows { get; set; } = new List<NplSnapshotRowModel>();
        }

    public class StaffSnapshotRowModel
        {
        public string Designation { get; set; }
        public int? StaffCount { get; set; }
        public DateTime? AsOfDate { get; set; }
        }

    public class StaffSnapshotViewModel
        {
        public int EngagementId { get; set; }
        public bool IsReadOnly { get; set; }
        public List<StaffSnapshotRowModel> Rows { get; set; } = new List<StaffSnapshotRowModel>();
        }

    public class FinalizeReportViewModel
        {
        public int EngagementId { get; set; }
        public bool IsFinal { get; set; }
        public bool CanFinalize { get; set; }
        public FieldAuditReportChecklistModel Checklist { get; set; } = new FieldAuditReportChecklistModel();
        }

    public class FieldAuditInputSectionViewModel
        {
        public int EngagementId { get; set; }
        public int EntityId { get; set; }
        public bool IsReadOnly { get; set; }
        public string SectionCode { get; set; }
        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string ReportStatus { get; set; }
        public List<GetTeamDetailsModel> AuditTeam { get; set; } = new List<GetTeamDetailsModel>();
        public List<FieldAuditPdfStatisticsRowModel> StatisticsRows { get; set; } = new List<FieldAuditPdfStatisticsRowModel>();
        }
    }
