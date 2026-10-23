using System;
using System.Collections.Generic;
using System.Globalization;

namespace AIS.Models
{
    public static class CiaCommandPolicy
    {
        public static DateTime ResolvePosition(ExecutiveDashboardFilterRequest filters, AuditPeriodModel period, DateTime today)
        {
            if (period == null || period.START_DATE.Date > period.END_DATE.Date)
                throw new ArgumentException("Select a valid IAS audit period.");
            var upper = period.END_DATE.Date < today.Date ? period.END_DATE.Date : today.Date;
            DateTime selected;
            if (string.IsNullOrEmpty(filters.PositionAsOn))
                selected = period.END_DATE.Date < today.AddDays(-1).Date ? period.END_DATE.Date : today.AddDays(-1).Date;
            else if (!DateTime.TryParseExact(filters.PositionAsOn, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out selected))
                throw new ArgumentException("Position as on must be yyyy-MM-dd.");
            if (selected < period.START_DATE.Date || selected > upper)
                throw new ArgumentException("Position as on must be within the selected audit period and cannot be in the future.");
            if (filters.EntityId.HasValue || filters.DimensionId.HasValue || filters.ReportingOfficeId.HasValue || filters.SelectBy != "entityType")
                throw new ArgumentException("CIA Command Center accepts audit-portfolio filters only.");
            filters.SnapDivision ??= "1";
            if (filters.SnapDivision != "1" && filters.SnapDivision != "FAD" && filters.SnapDivision != "MAD")
                throw new ArgumentException("Select a valid audit division.");
            filters.PositionAsOn = selected.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return selected;
        }
    }
    // Isolated CIA command-center responses; TOP lifecycle KPIs are unaffected.
    public sealed class CiaCommandKpis
    {
        public long? PlannedEntities { get; set; }
        public long? AuditedEntities { get; set; }
        public decimal? PlanCoveragePct { get; set; }
        public long? DelayedAuditReports { get; set; }
        public long? FraudA1Paras { get; set; }
        public long? AwaitingAuditeeResponse { get; set; }
        public long? TeamsConductingAudit { get; set; }
        public string WorkflowVerificationNote { get; set; }
        public int AuditPeriodId { get; set; }
        public DateTime? AuditPeriodFrom { get; set; }
        public DateTime? AuditPeriodTo { get; set; }
        public DateTime? PositionAsOn { get; set; }
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
