using System;
using System.Collections.Generic;

namespace AIS.Models
{
    // Isolated CIA command-center responses; TOP lifecycle KPIs are unaffected.
    public sealed class CiaCommandKpis
    {
        public long PlannedEntities { get; set; }
        public long AuditedEntities { get; set; }
        public decimal? PlanCoveragePct { get; set; }
        public long DelayedAuditReports { get; set; }
        public long FraudA1Paras { get; set; }
        public long AwaitingAuditeeResponse { get; set; }
        public long TeamsConductingAudit { get; set; }
        public int AuditPeriodId { get; set; }
        public DateTime? AuditPeriodFrom { get; set; }
        public DateTime? AuditPeriodTo { get; set; }
        public DateTime? CurrentStateAsOf { get; set; }
    }

    public sealed class CiaIssueNatureRow
    {
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public int DisplayOrder { get; set; }
        public long IssueCount { get; set; }
        public long TotalIssues { get; set; }
        public long UnmappedIssues { get; set; }
        public string MappingStatus { get; set; }
    }
}
