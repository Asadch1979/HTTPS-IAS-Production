namespace AIS.Models.FieldAuditWorkflow
    {
    public class FieldAuditEngagementOptionModel
        {
        public int EngPlanId { get; set; }
        public int EngagementId
            {
            get => EngPlanId;
            set => EngPlanId = value;
            }
        public string EntityName { get; set; }
        public string EngStatus { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StageName { get; set; }
        public int StatusId { get; set; }
        public string IsClose { get; set; }
        public string Display { get; set; }
        public string IsTeamLead { get; set; }
        public string Label => !string.IsNullOrWhiteSpace(Display)
            ? Display
            : $"{EntityName} ({EngagementId}) {EngStatus} - {StartDate} to {EndDate}";
        }
    }
