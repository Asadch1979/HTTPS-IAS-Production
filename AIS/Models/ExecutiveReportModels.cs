using System;
using System.Collections.Generic;

namespace AIS.Models
{
    public sealed class ExecutiveReportRow
    {
        public string RowKind { get; set; }
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public int? ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public bool IsPartialYear => PeriodStart != new DateTime(PeriodStart.Year, 1, 1) || PeriodEnd != new DateTime(PeriodStart.Year, 12, 31);
        public long OpeningBalance { get; set; }
        public long ParasAdded { get; set; }
        public long TotalParas { get; set; }
        public long SettledParas { get; set; }
        public long OutstandingParas { get; set; }
        public decimal? SettlementPercentage { get; set; }
        public long LowRisk { get; set; }
        public long MediumRisk { get; set; }
        public long HighRisk { get; set; }
        public long UnratedRisk { get; set; }
        public long SettledByAudit { get; set; }
        public long SettledByCompliance { get; set; }
        public long SettledByUnknown { get; set; }
        public bool SettlementSourcesComplete => SettledByUnknown == 0;
        public long ReopeningEvents { get; set; }
        public long ResettlementEvents { get; set; }
        public long InitiallyClosedAdditions { get; set; }
        public long ReconciliationAdjustment => ReopeningEvents - ResettlementEvents - InitiallyClosedAdditions;
        public long ReconciliationDifference { get; set; }
        public long AuditorAssignmentMismatch { get; set; }
        public string RankingGroup { get; set; }
        public int? RankPosition { get; set; }
    }

    public sealed class ExecutiveReportResult
    {
        public string ReportId { get; set; }
        public ExecutiveDashboardFilterRequest Filters { get; set; }
        public int? GroupRootId { get; set; }
        public string DashboardType { get; set; }
        public string ScopeBasis => "Current authorized portfolio/hierarchy; historical lifecycle and risk states from certified evidence";
        public string SettlementMethod => "Unique COM_IDs with an effective close in the interval; last close determines source. Denominator: OpeningBalance + ParasAdded. Reopenings/resettlements are separate adjustments.";
        public string AuditorBasis => "Observation AUDITED_BY, not current entity-directory AUDITBY_ID";
        public List<ExecutiveReportRow> Rows { get; set; }
    }
}
