using AIS.Validation;
namespace AIS.Models
    {
    public class ReferenceEntitySummaryModel
        {
        public int EntityId { get; set; }
        [PlainText]
        public string EntityCode { get; set; }
        [PlainText]
        public string EntityName { get; set; }
        [PlainText]
        public string AuditPeriod { get; set; }
        public int TotalParas { get; set; }
        public int UpdatedParas { get; set; }
        public int Pendency { get; set; }
        }
    }
