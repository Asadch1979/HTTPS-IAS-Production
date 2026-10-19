using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AIS.Models
{
    public static class AuditParaSnapPolicy
    {
        public const string PagePath = "/ExecutiveOverview/AuditParaSnap";
        public const int PageId = 1000304;
        // IAS reports use Pakistan calendar dates independently of browser/server TZ.
        public static DateTime Today => DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(5)).Date;
        public static readonly string[] Views = { "glance", "position", "settlement", "risk", "aging", "reports" };
        public static ExecutiveDashboardDateRange Validate(ExecutiveDashboardFilterRequest filters)
        {
            var dates = filters?.Validate() ?? throw new ArgumentException("Both reporting dates are required.");
            if (dates.EndDate > Today || (dates.EndDate.Year - dates.StartDate.Year) * 12 +
                dates.EndDate.Month - dates.StartDate.Month >= 1200)
                throw new ArgumentException("End Date cannot be in the future; select fewer than 1200 months.");
            if (!Views.Contains(filters.View))
                throw new ArgumentException("Select a valid Audit Para Snap view.");
            return dates;
        }
        public static void ApplyDefault(ExecutiveDashboardFilterRequest filters)
        {
            // Year to the last complete Pakistan calendar day. On Jan 1 this
            // intentionally means the previous complete calendar year.
            if (!string.IsNullOrEmpty(filters.StartDate) || !string.IsNullOrEmpty(filters.EndDate)) return;
            var end = Today.AddDays(-1);
            filters.StartDate = new DateTime(end.Year, 1, 1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            filters.EndDate = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        public static List<ExecutiveDashboardViewDefinition> Definitions()
            => new()
            {
                Definition("glance","At a Glance",
                    new[]{"openingOpenParas","issueIdentified","issueRectified","paraAdded","parasSettled",
                        "closingOpenParas","highRiskOpenParas","over1YearParas","over3ComplianceCycles"},
                    new[]{"trend","risk"}),
                Definition("position","Audit Para Position",
                    new[]{"openingOpenParas","paraAdded","parasSettled","closingOpenParas"},
                    Array.Empty<string>()),
                Definition("settlement","Settlement & Performance",
                    new[]{"parasSettled","periodSettlementRate"},new[]{"trend"}),
                Definition("risk","Risk",new[]{"highRiskOpenParas"},new[]{"risk"}),
                Definition("aging","Aging & Compliance",
                    new[]{"over1YearParas","over3ComplianceCycles"},new[]{"aging","cycle"}),
                new() { Id="reports", Title="Reports", Partial="_Reports", Kpis=Array.Empty<string>(),
                    Sections=Array.Empty<string>(), Reports=new[]{"A","B","C","D","E"} }
            };
        private static ExecutiveDashboardViewDefinition Definition(
            string id,string title,string[] kpis,string[] sections)
            => new() { Id=id, Title=title, Partial="_AuditParaSnapAnalysis",
                Description="Authorized, selected-period IAS analysis.",
                Kpis=kpis,Sections=sections,UseLifecycleKpis=true };
    }
    public sealed class AuditParaSnapBucket
    {
        public string Dataset { get; set; }
        public string BucketCode { get; set; }
        public string BucketName { get; set; }
        public long? ParaCount { get; set; }
        // For monthly trend only: settled high-risk paras in the same month.
        public long? HighRiskCount { get; set; }
        public string QualityCode { get; set; }
    }
    public sealed class AuditParaSnapCritical
    {
        public long ComId { get; set; }
        public int? EntityId { get; set; }
        public string EntityName { get; set; }
        public string ParaNo { get; set; }
        public string GistOfParas { get; set; }
        public string RiskName { get; set; }
        public int? AgingDays { get; set; }
        public int? ComCycle { get; set; }
        public string QualityCode { get; set; }
    }
}
