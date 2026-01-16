using AIS.Validation;
namespace AIS.Models
    {
    public class FinalAuditReportModel
        {
        [PlainText]
        public string ID { get; set; }
        [PlainText]
        public string ENG_ID { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string DOC_TYPE { get; set; }
        [PlainText]
        public string DOC_NAME { get; set; }


        }
    }
