using AIS.Validation;

namespace AIS.Models
    {
    public class UpdateAuditParaRequest
        {
        public int? COM_ID { get; set; }
        public int? OLD_PARA_ID { get; set; }
        public int? NEW_PARA_ID { get; set; }

        [PlainText]
        public string PARA_NO { get; set; }

        [RichTextSanitize]
        public string PARA_TEXT { get; set; }

        [RichTextSanitize]
        public string OBS_GIST { get; set; }
        public int? OBS_RISK_ID { get; set; }
        public int? ANNEX_ID { get; set; }

        public string INDICATOR { get; set; }
        public string AUDIT_PERIOD { get; set; }
        public string AMOUNT_INV { get; set; }
        public string NO_INSTANCES { get; set; }
        }

    }
