using AIS.Validation;
namespace AIS.Models
    {
    public class SpecialAuditPlanModel
        {
        [PlainText]
        public string PLAN_ID { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }
        [PlainText]
        public string AUDITED_BY_ID { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string REPORTING_OFFICE_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string AUDIT_PERIOD_ID { get; set; }
        [PlainText]
        public string NATURE { get; set; }
        [PlainText]
        public string NATURE_ID { get; set; }
        [PlainText]
        public string NO_DAYS { get; set; }

        }
    }
