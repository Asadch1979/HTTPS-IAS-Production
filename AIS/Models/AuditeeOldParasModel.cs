using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class AuditeeOldParasModel
        {
        public int? ID { get; set; }
        public int COM_ID { get; set; }
        [PlainText]
        public string ENG_ID { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        public String PARA_ID { get; set; }
        public int? ENTITY_CODE { get; set; }
        public int? TYPE_ID { get; set; }
        [PlainText]
        public string TYPE_DES { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string AUDIT_PERIOD_DES { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string PARA_CATEGORY { get; set; }
        [PlainText]
        public string PARA_RISK { get; set; }
        [PlainText]
        public string REPORT_NAME { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string MEMO_NO { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        [PlainText]
        public string OBS_ID { get; set; }
        [PlainText]
        public string AUDITEE_RESPONSE { get; set; }
        [PlainText]
        public string AUDITOR_REMARKS { get; set; }
        public DateTime? DATE_OF_LAST_COMPLIANCE_RECEIVED { get; set; }
        [PlainText]
        public string AUDITEDBY { get; set; }
        }
    }
