using AIS.Validation;
namespace AIS.Models
    {
    public class SettledPostCompliancesModel
        {
        [PlainText]
        public string COMPLIANCE_UNIT { get; set; }
        [PlainText]
        public string COMPLIANCE_SETTLEMENT_OFFICER { get; set; }
        [PlainText]
        public string COMPLIANCE_UNIT_INCHARGE { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string COM_KEY { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_STATUS { get; set; }
        [PlainText]
        public string PARA_RISK { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        public int? NEW_PARA_ID { get; set; }
        public int? OLD_PARA_ID { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
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
        public string COM_ID { get; set; }
        [PlainText]
        public string SETTLED_ON { get; set; }
        [PlainText]
        public string COM_STAGE { get; set; }
        [PlainText]
        public string COM_STATUS { get; set; }
        [PlainText]
        public string COM_CYCLE { get; set; }

        }
    }
