using AIS.Validation;
namespace AIS.Models
    {
    public class FADAuditParasReviewModel
        {
        [PlainText]
        public string OLD_PARA_ID { get; set; }
        [PlainText]
        public string NEW_PARA_ID { get; set; }
        [PlainText]
        public string ANNEX { get; set; }
        [PlainText]
        public string PROCESS { get; set; }
        [PlainText]
        public string SUB_PROCESS { get; set; }
        [PlainText]
        public string CHECK_LIST { get; set; }
        [PlainText]
        public string MEMO_NO { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [RichTextSanitize]
        public string PARA_TEXT { get; set; }
        [RichTextSanitize]
        public string OBS_GIST { get; set; }
        [PlainText]
        public string OBS_RISK { get; set; }
        [PlainText]
        public string OBS_RISK_ID { get; set; }
        [PlainText]
        public string AMOUNT_INV { get; set; }
        [PlainText]
        public string NO_INSTANCES { get; set; }
        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string RESP_ROLE { get; set; }
        [PlainText]
        public string RESP_AMOUNT { get; set; }
        [PlainText]
        public string LOAN_CASE { get; set; }
        [PlainText]
        public string REPORT_ID { get; set; }
        [PlainText]
        public string REPORT_NAME { get; set; }

        [RichTextSanitize]
        public string AUDITEE_REPLY { get; set; }
        [PlainText]
        public string AUDITOR_COMMENTS { get; set; }
        [PlainText]
        public string HEADCOMMENTS { get; set; }
        [PlainText]
        public string ROOT_CAUSE { get; set; }

        }
    }
