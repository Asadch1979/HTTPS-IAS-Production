using AIS.Validation;
namespace AIS.Models
    {
    public class ComplianceProgressReportDetailModel
        {

        [PlainText]
        public string COMPLIANCE_UNIT { get; set; }
        [PlainText]
        public string PARENT_ID { get; set; }
        [PlainText]
        public string PARENT_NAME { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ENTITY_CODE { get; set; }
        [PlainText]
        public string COM_KEY { get; set; }
        [PlainText]
        public string PP_NO { get; set; }
        [PlainText]
        public string EMP_NAME { get; set; }
        [PlainText]
        public string TOTAL { get; set; }
        [PlainText]
        public string REFERRED_BACK { get; set; }
        [RichTextSanitize]
        public string RECOMMENDED { get; set; }
        [PlainText]
        public string PENDING { get; set; }

        }
    }
