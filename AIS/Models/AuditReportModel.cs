using AIS.Validation;
namespace AIS.Models
    {
    public class AuditReportModel
        {
        [PlainText]
        public string ID { get; set; }
        [PlainText]
        public string ENG_ID { get; set; }
        [PlainText]
        public string AUDIT_REPORT { get; set; }

        [PlainText]
        public string ADDED_BY { get; set; }
        [PlainText]
        public string ADDED_ON { get; set; }

        [PlainText]
        public string DOC_TYPE { get; set; }

        [PlainText]
        public string DOC_NAME { get; set; }

        }
    }
