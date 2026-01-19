namespace AIS.Models
    {
    public class FieldAuditEngagementLookupModel
        {
        public int EngId { get; set; }

        public int EntityId { get; set; }

        public string EntityName { get; set; }

        public string AuditPeriod { get; set; }
        public object ENG_ID { get; internal set; }
        }

    }
