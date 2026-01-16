using AIS.Validation;
namespace AIS.Models
    {
    public class AuditPlanEngDetailReport
        {
        [PlainText]
        public string ENG_ID { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string AUDIT_TEAM { get; set; }
        [PlainText]
        public string OP_START_DATE { get; set; }
        [PlainText]
        public string OP_END_DATE { get; set; }
        [PlainText]
        public string AUDIT_START_DATE { get; set; }
        [PlainText]
        public string AUDIT_END_DATE { get; set; }
        [PlainText]
        public string TRAVEL_DAYS { get; set; }
        [PlainText]
        public string DISCUSSION_DAYS { get; set; }
        [PlainText]
        public string REVENUE_RECORD_DAYS { get; set; }
        [PlainText]
        public string WEEKEND_DAYS { get; set; }
        [PlainText]
        public string TOTAL_DAYS { get; set; }
        [PlainText]
        public string DELAY_DAYS { get; set; }
        [PlainText]
        public string ENG_STATUS { get; set; }




        }
    }
