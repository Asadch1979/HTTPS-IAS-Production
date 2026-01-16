using AIS.Validation;
namespace AIS.Models
    {
    public class ManageAuditParasModel
        {
        public int COM_ID { get; set; }
        
        public int OLD_PARA_ID { get; set; }
        
        public int NEW_PARA_ID { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        public int PARA_ID { get; set; }
        [PlainText]
        public string AUDITEE { get; set; }
        [RichTextSanitize]
        public string PARA_TEXT { get; set; }
        [PlainText]
        public string OBS_GIST { get; set; }
        [PlainText]
        public string OBS_RISK { get; set; }
        public int OBS_RISK_ID { get; set; }
        [PlainText]
        public string INDICATOR { get; set; }
        [PlainText]
        public string P_TYPE_IND { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string ANNEX { get; set; }
        public int ANNEX_ID { get; set; }
        [PlainText]
        public string AMOUNT_INV { get; set; }
        [PlainText]
        public string NO_INSTANCES { get; set; }
        [PlainText]
        public string UPDATED_BY { get; set; }
        [PlainText]
        public string UPDATED_ON { get; set; }
        [PlainText]
        public string P_DECISION { get; set; }

        }
    }
