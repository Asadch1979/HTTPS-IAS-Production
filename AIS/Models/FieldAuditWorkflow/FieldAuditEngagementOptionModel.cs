using Microsoft.VisualBasic;

namespace AIS.Models.FieldAuditWorkflow
    {
    public class FieldAuditEngagementOptionModel
        {
        public int EngagementId { get; set; }
        public string EntityName { get; set; }
        public string EngStatus { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StageName { get; set; }
        public int StatusId { get; set; }
        public string IsClose { get; set; }
        public string Label => $"{EntityName} ({EngagementId}) {EngStatus} - {StartDate} to {EndDate} ";
        }
    }
