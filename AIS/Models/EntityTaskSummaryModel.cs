using AIS.Validation;
namespace AIS.Models
    {
    public class EntityTaskSummaryModel
        {
        public int EntityId { get; set; }
        [PlainText]
        public string EntityCode { get; set; }
        [PlainText]
        public string EntityName { get; set; }
        [PlainText]
        public string AuditYear { get; set; }
        public int TotalParas { get; set; }
        public int ParasUpdated { get; set; }
        }
    }
