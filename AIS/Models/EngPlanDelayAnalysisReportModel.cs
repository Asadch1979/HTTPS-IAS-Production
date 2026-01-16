using AIS.Validation;
namespace AIS.Models
    {
    public class EngPlanDelayAnalysisReportModel
        {
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string PLACE_OF_POSTING { get; set; }
        [PlainText]
        public string AUDIT_START_DATE { get; set; }
        [PlainText]
        public string AUDIT_END_DATE { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string DELAY_DAYS { get; set; }

        }
    }
