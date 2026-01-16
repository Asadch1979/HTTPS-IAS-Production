using AIS.Validation;
namespace AIS.Models
    {
    public class TentativePlanModel
        {
        public int PLAN_ID { get; set; }
        public int CRITERIA_ID { get; set; }
        public int AUDIT_PERIOD_ID { get; set; }
        public int AUDITEDBY { get; set; }
        [PlainText]
        public string ZONE_NAME { get; set; }
        [PlainText]
        public string NATURE_OF_AUDIT { get; set; }
        [PlainText]
        public string CODE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        public int? ENTITY_ID { get; set; }
        public int? ENTITY_TYPE_ID { get; set; }
        [PlainText]
        public string ENT_TYPE { get; set; }
        [PlainText]
        public string BR_NAME { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string BR_SIZE { get; set; }
        [PlainText]
        public string PERIOD_NAME { get; set; }
        [PlainText]
        public string FREQUENCY_DESCRIPTION { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        public int NO_OF_DAYS { get; set; }

        }
    }
