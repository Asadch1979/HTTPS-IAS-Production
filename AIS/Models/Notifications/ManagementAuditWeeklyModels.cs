using System;

namespace AIS.Models.Notifications
    {
    public class ManagementAuditWeeklyDivisionSummaryModel
        {
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string CcEmail { get; set; } = string.Empty;
        public int SettledCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalCount { get; set; }
        }

    public class ManagementAuditWeeklyDivisionDetailModel
        {
        public int DecisionHistoryId { get; set; }
        public int ComId { get; set; }
        public int ComCycle { get; set; }
        public int EntityId { get; set; }
        public int AuditedBy { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string AuditPeriod { get; set; } = string.Empty;
        public string ParaNo { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime? SubmittedOn { get; set; }
        public DateTime DecisionOn { get; set; }
        public string DecisionStatus { get; set; } = string.Empty;
        public int ComStatus { get; set; }
        public string Reason { get; set; } = string.Empty;
        }
    }
