namespace AIS.Models.FieldAuditWorkflow
    {
    public class FieldAuditEngagementOptionModel
        {
        public int EngagementId { get; set; }
        public string EntityName { get; set; }
        public string StageName { get; set; }
        public string Label => $"{EntityName} ({EngagementId})";
        }
    }
