using System;
using System.Collections.Generic;

namespace AIS.Models
{
    // Read-only Oracle cursor contracts. These are NOT replacements for the
    // existing executive KPI/critical-item response models.
    public sealed class RoleEntityHeatmapRow
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public int? EntityTypeId { get; set; }
        public long OpenParas { get; set; }
        public long HighRisk { get; set; }
        public long MediumRisk { get; set; }
        public long LowRisk { get; set; }
        public long Unrated { get; set; }
        public long AgedOver365 { get; set; }
        public long Over3Cycles { get; set; }
        public decimal HighRiskSharePct { get; set; }
    }

    public sealed class RoleUnitComparisonRow
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public long OpenParas { get; set; }
        public long HighRiskOpen { get; set; }
        public long AgedOver365 { get; set; }
        public long Over3Cycles { get; set; }
        public long SettledTotal { get; set; }
        public long SettledMtd { get; set; }
    }

    public sealed class RoleFollowupRow
    {
        public long ComId { get; set; }
        public string ParaNo { get; set; }
        public string AuditPeriod { get; set; }
        public string GistOfParas { get; set; }
        public int? EntityId { get; set; }
        public string EntityName { get; set; }
        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int? BusinessGroupId { get; set; }
        public string BusinessGroupName { get; set; }
        public int? RiskId { get; set; }
        public string RiskName { get; set; }
        public int? ParaStatus { get; set; }
        public int? ComStatus { get; set; }
        public int? ComStage { get; set; }
        public int? ComCycle { get; set; }
        public DateTime? ParaAddedOn { get; set; }
        public int? AgingDays { get; set; }
        public DateTime? LastMovementDate { get; set; }
        public int? DaysSinceLastMovement { get; set; }
        public string PriorityReason { get; set; }
    }

    public sealed class TeamLeadDashboardSnapshot
    {
        public TeamLeadDashboardSummary Summary { get; set; }
        public List<TeamLeadEngagementRow> Engagements { get; set; } = new();
        public List<TeamLeadWorkpaperRow> Workpapers { get; set; } = new();
        public List<TeamLeadObservationRow> Observations { get; set; } = new();
    }

    public sealed class TeamLeadDashboardSummary
    {
        public long ActiveEngagements { get; set; }
        public long AwaitingMyWorkpaperReview { get; set; }
        public long ObservationsAwaitingAuditorReview { get; set; }
        public long ObservationsAwaitingAuditeeResponse { get; set; }
        public long WorkpapersDueNext7Days { get; set; }
        public long EngagementsEndingNext7Days { get; set; }
        public DateTime CurrentStateAsOf { get; set; }
    }

    public sealed class TeamLeadEngagementRow
    {
        public long EngId { get; set; }
        public long? PlanId { get; set; }
        public long? PeriodId { get; set; }
        public int? EntityId { get; set; }
        public string EntityName { get; set; }
        public int? EngagementStatusId { get; set; }
        public int? TaskStatusId { get; set; }
        public string TaskStatusDescription { get; set; }
        public DateTime? AuditStartDate { get; set; }
        public DateTime? AuditEndDate { get; set; }
        public DateTime? OperationStartDate { get; set; }
        public DateTime? OperationEndDate { get; set; }
        public int? DaysToAuditEnd { get; set; }
        public long AwaitingAuditorReview { get; set; }
        public long AwaitingAuditeeResponse { get; set; }
    }

    public sealed class TeamLeadWorkpaperRow
    {
        public long WpId { get; set; }
        public long EngId { get; set; }
        public int? EntityId { get; set; }
        public string PaperType { get; set; }
        public string ReferenceNo { get; set; }
        public int VersionNo { get; set; }
        public string Status { get; set; }
        public string PreparerPpno { get; set; }
        public string ReviewerPpno { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public long TotalItems { get; set; }
        public long PassedItems { get; set; }
        public long ExceptionItems { get; set; }
        public long PendingItems { get; set; }
        public int? DaysToDue { get; set; }
    }

    public sealed class TeamLeadObservationRow
    {
        public long ObservationId { get; set; }
        public long EngId { get; set; }
        public int ObservationStatusId { get; set; }
        public int? RiskId { get; set; }
        public int? AnnexId { get; set; }
        public DateTime? EnteredDate { get; set; }
    }
}
