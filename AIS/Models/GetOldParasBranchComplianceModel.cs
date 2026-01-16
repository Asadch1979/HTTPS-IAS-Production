using AIS.Validation;
namespace AIS.Models
    {
    public class GetOldParasBranchComplianceModel
        {
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string AUDIT_DATE { get; set; }
        [PlainText]
        public string PARA_RISK { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        public int? NEW_PARA_ID { get; set; }
        public int? OLD_PARA_ID { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        [PlainText]
        public string AUDITOR_REMARKS { get; set; }
        [PlainText]
        public string AUDIT_BY_ID { get; set; }
        [PlainText]
        public string NEXT_R_ID { get; set; }
        [PlainText]
        public string PREV_R_ID { get; set; }
        [PlainText]
        public string STATUS_UP { get; set; }
        [PlainText]
        public string STATUS_DOWN { get; set; }
        [PlainText]
        public string INDICATOR { get; set; }
        [PlainText]
        public string PREV_ROLE { get; set; }
        [PlainText]
        public string NEXT_ROLE { get; set; }
        [PlainText]
        public string RECEIVED_FROM { get; set; }
        [PlainText]
        public string COM_ID { get; set; }

        }
    }
