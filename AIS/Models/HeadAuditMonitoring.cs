using System;
using System.Collections.Generic;

namespace AIS.Models
{
    public static class HeadAuditMonitoringPolicy
    {
        public const int PageId = 1000401;
        public static bool IsEligible(SessionUser user) => user != null &&
            user.UserRoleID > 0 &&
            user.UserEntityID.GetValueOrDefault() > 0;
        public static readonly string[] Sections = { "Plan", "Criteria", "Team", "Tasks", "Workpapers", "Observations", "History", "Evidence", "ResponseEvidence", "DraftReport", "Reports" };
    }

    public sealed class HeadAuditEngagement
    {
        public long EngagementId { get; set; }
        public long? TeamId { get; set; }
        public string Entity { get; set; }
        public string Department { get; set; }
        public string Team { get; set; }
        public string Members { get; set; }
        public string Period { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? DaysToEnd { get; set; }
        public long AwaitingReview { get; set; }
        public string TaskProgress { get; set; }
        public string ObservationCounts { get; set; }
        public long Reports { get; set; }
        public string ReportStatus { get; set; }
        public long? WorkpapersAwaitingReview { get; set; }
        public string WorkpaperProgress { get; set; }
    }

    // Encoded text only: this model is never rendered as trusted HTML or sent to workflow editors.
    public sealed class HeadAuditWorkRecord
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Author { get; set; }
        public DateTime? SavedOn { get; set; }
        public string Content { get; set; }
    }

    public sealed class HeadAuditReview
    {
        public HeadAuditEngagement Engagement { get; set; }
        public string Section { get; set; }
        public List<HeadAuditWorkRecord> Records { get; set; } = new();
    }

    public sealed class HeadAuditDocument
    {
        public byte[] Bytes { get; set; }
        public string Name { get; set; }
    }
}
