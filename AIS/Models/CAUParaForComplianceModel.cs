using AIS.Validation;
namespace AIS.Models
    {
    public class CAUParaForComplianceModel
        {
        [PlainText]
        public string COM_ID { get; set; }
        public int? NEW_PARA_ID { get; set; }
        public int? OLD_PARA_ID { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        [PlainText]
        public string INDICATOR { get; set; }
        [PlainText]
        public string CAU_STATUS { get; set; }
        [PlainText]
        public string CAU_NAME { get; set; }
        [RichTextSanitize]
        public string CAU_INSTRUCTIONS { get; set; }
        [PlainText]
        public string CAU_ASSIGNED_ENT_ID { get; set; }

        }
    }
