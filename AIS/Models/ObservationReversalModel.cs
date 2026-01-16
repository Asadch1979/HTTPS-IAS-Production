using AIS.Validation;
namespace AIS.Models
    {
    public class ObservationReversalModel
        {

        [PlainText]
        public string PLAN_ID { get; set; }
        [PlainText]
        public string ENG_ID { get; set; }
        [PlainText]
        public string TEAM_NAME { get; set; }
        [PlainText]
        public string AUDIT_START_DATE { get; set; }
        [PlainText]
        public string AUDIT_END_DATE { get; set; }
        [PlainText]
        public string OP_START_DATE { get; set; }
        [PlainText]
        public string OP_END_DATE { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string AUDITED_BY_ID { get; set; }
        [PlainText]
        public string STATUS_ID { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string REPORT_ID { get; set; }


        }
    }
