using AIS.Validation;
namespace AIS.Models
    {
    public class GetOldParasForComplianceReviewer
        {
        [PlainText]
        public string AUDITEENAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string PARENT_ID { get; set; }
        [RichTextSanitize]
        public string GISTOFPARA { get; set; }
        [PlainText]
        public string AMOUNT_INVOLVED { get; set; }
        [PlainText]
        public string PARA_CATEGORY { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        [PlainText]
        public string AU_OBS_ID { get; set; }
        [PlainText]
        public string ID { get; set; }
        [RichTextSanitize]
        public string REPLY { get; set; }
        [PlainText]
        public string SEQUENCE { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }
        [PlainText]
        public string REPLY_DATE { get; set; }








        }
    }
