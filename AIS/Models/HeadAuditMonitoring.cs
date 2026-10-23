using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Models
{
    public sealed class HeadAuditReportingDateException : ArgumentException
    {
        public HeadAuditReportingDateException(string message) : base(message) { }
    }

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
        public int OfficeId { get; set; }
        public int PeriodId { get; set; }
        public DateTime? OperationStart { get; set; }
        public bool IsCompleted => StatusId == 14;
        public bool IsOngoing => StatusId >= 4 && StatusId <= 13;
        public bool IsDelayed => IsOngoing && DaysToEnd < 0;
        public bool HasPendingReport => StatusId == 12 || StatusId == 13;
        public bool IsApproaching => IsOngoing && DaysToEnd >= 0 && DaysToEnd <= 7;
        public string PendingAction => string.Join("; ", new[] {
            IsDelayed ? "Past scheduled completion" : null,
            StatusId == 12 ? "Draft report review" : StatusId == 13 ? "Final report issuance / conclusion" : null,
            AwaitingReview > 0 ? "Supervisory observation review" : null,
            IsApproaching ? "Completion due within 7 days" : null
        }.Where(v => v != null));
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
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Memo { get; set; }
        public string DraftPara { get; set; }
        public string FinalPara { get; set; }
        public string Risk { get; set; }
        public long Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Author { get; set; }
        public DateTime? SavedOn { get; set; }
        public string Content { get; set; }
    }

    public sealed class HeadAuditReview
    {
        public DateTime? PositionAsOn { get; set; }
        public string Tab { get; set; } = "register";
        public int? AuditPeriodId { get; set; }
        public int? OfficeId { get; set; }
        public int? StatusId { get; set; }
        public string Exception { get; set; }
        public HeadAuditEngagement Engagement { get; set; }
        public List<HeadAuditPlanDetail> Plans { get; set; } = new();
        public string Section { get; set; }
        public List<HeadAuditWorkRecord> Records { get; set; } = new();
    }

    public sealed class HeadAuditOfficeSummary
    {
        public int OfficeId { get; set; }
        public string Office { get; set; }
        public long Planned { get; set; }
        public long PlannedEntities { get; set; }
        public long CompletedPlans { get; set; }
        public long OngoingPlans { get; set; }
        public long NotStarted { get; set; }
        public long UnknownPlans { get; set; }
        public long Completed { get; set; }
        public long Ongoing { get; set; }
        public long Delayed { get; set; }
        public long PendingReports { get; set; }
        public long AwaitingReview { get; set; }
        public decimal? Achievement => Planned > 0 ? Math.Round(100m * CompletedPlans / Planned, 1) : null;
    }

    public sealed class HeadAuditDashboard
    {
        public DateTime? PositionAsOn { get; set; }
        public string Tab { get; set; } = "progress";
        public const string HistoricalLimitations = "Reconstructed historical position (not a verified historical snapshot): engagement stages use the latest recorded workflow event by cutoff (Pakistan end of day); missing, unsupported or ambiguous events are Unknown / Historical Data Unavailable. History is incomplete; deleted engagements cannot be reconstructed. Current-state information: approved-plan membership/days, schedules, office/team assignments, observation reviews, task/workpaper and report summaries reflect current saved records, not verified historical values. Plan progress classifies the current approved-plan cohort using available engagement history; unclassified plans are reported separately as unknown history. Review Saved Work shows current saved work.";
        public static DateTime ReportingDate(AuditPeriodModel period, DateTime? requested, DateTime today)
        {
            if (period == null || period.START_DATE.Date > period.END_DATE.Date) throw new ArgumentException("Select a valid IAS audit period.");
            var last = period.END_DATE.Date < today.Date ? period.END_DATE.Date : today.Date.AddDays(-1);
            if (last < period.START_DATE.Date) throw new HeadAuditReportingDateException("The selected IAS audit period has no completed eligible day yet (Pakistan time).");
            var date = (requested ?? last).Date;
            if (date < period.START_DATE.Date || date > period.END_DATE.Date || date > last)
                throw new HeadAuditReportingDateException("Position as on must be within the selected IAS audit period and no later than yesterday (Pakistan time).");
            return date;
        }
        public List<AuditPeriodModel> Periods { get; set; } = new();
        public int? AuditPeriodId { get; set; }
        public List<HeadAuditOfficeSummary> Offices { get; set; } = new();
        public List<HeadAuditEngagement> Engagements { get; set; } = new();
        public List<HeadAuditPlanDetail> Plans { get; set; } = new();
        public int? OfficeId { get; set; }
        public int? StatusId { get; set; }
        public string Exception { get; set; }
        public HeadAuditOfficeSummary Total => new() {
            Planned = Offices.Sum(o => o.Planned), CompletedPlans = Offices.Sum(o => o.CompletedPlans),
            OngoingPlans = Offices.Sum(o => o.OngoingPlans), NotStarted = Offices.Sum(o => o.NotStarted), UnknownPlans=Offices.Sum(o => o.UnknownPlans),
            Completed = Offices.Sum(o => o.Completed), Ongoing = Offices.Sum(o => o.Ongoing),
            Delayed = Offices.Sum(o => o.Delayed), PendingReports = Offices.Sum(o => o.PendingReports),
            AwaitingReview = Offices.Sum(o => o.AwaitingReview)
        };
        public int? ActiveTeamCount { get; set; }
        public int ActiveTeams => ActiveTeamCount ?? CountActiveTeams();
        private int CountActiveTeams() => Engagements.Where(e => e.StatusId >= 4 && e.StatusId <= 11 &&
            (e.OperationStart ?? e.Start)?.Date <= (PositionAsOn ?? AuditParaSnapPolicy.Today) && e.TeamId.HasValue)
            .Select(e => e.TeamId).Distinct().Count();
        public IEnumerable<HeadAuditEngagement> Register => Engagements.Where(e =>
            (!OfficeId.HasValue || e.OfficeId == OfficeId) && (!StatusId.HasValue || e.StatusId == StatusId) &&
            (string.IsNullOrEmpty(Exception) || Exception == "delayed" && e.IsDelayed ||
             Exception == "reports" && e.HasPendingReport || Exception == "review" && e.AwaitingReview > 0 ||
             Exception == "approaching" && e.IsApproaching));
        public static bool ValidException(string value) => string.IsNullOrEmpty(value) ||
            new[] { "delayed", "reports", "review", "approaching" }.Contains(value);
        public static int? DefaultPeriod(IEnumerable<AuditPeriodModel> periods, DateTime today)
        {
            var current = periods.Where(p => p.START_DATE.Date <= today && p.END_DATE.Date >= today).ToList();
            return current.Count == 1 ? current[0].AUDITPERIODID : (int?)null;
        }
    }

    public sealed class HeadAuditPlanDetail
    {
        public long PlanId { get; set; }
        public long? EngagementId { get; set; }
        public string Entity { get; set; }
        public int? Days { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Team { get; set; }
        public string Stage { get; set; }
        public string Status { get; set; }
    }

    public sealed class HeadAuditDocument
    {
        public byte[] Bytes { get; set; }
        public string Name { get; set; }
    }
}
