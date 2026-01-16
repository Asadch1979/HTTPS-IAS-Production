using System.Collections.Generic;

using AIS.Validation;
namespace AIS.Models
    {
    public class OldParasModel
        {
        public int ID { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        [PlainText]
        public string ENTITY_CODE { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string AU_OBS_ID { get; set; }
        [PlainText]
        public string TYPE_ID { get; set; }
        [PlainText]
        public string RISK_ID { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string PARA_RISK { get; set; }
        [PlainText]
        public string RESPONSIBLE_PP_NO { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        [PlainText]
        public string PARA_STATUS { get; set; }
        [PlainText]
        public string ENTTY_NAME { get; set; }
        [PlainText]
        public string ANNEXURE { get; set; }
        [PlainText]
        public string AMOUNT_INVOLVED { get; set; }
        [PlainText]
        public string VOL_I_II { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }
        [PlainText]
        public string AUDITEDBY { get; set; }

        [PlainText]
        public string NEW_PARA_ID { get; set; }
        [PlainText]
        public string OLD_PARA_ID { get; set; }
        [PlainText]
        public string ENT_TYPE { get; set; }
        public int STATUS { get; set; }
        [PlainText]
        public string IND { get; set; }
        [RichTextSanitize]
        public string PARA_TEXT { get; set; }
        [PlainText]
        public string ENTERED_BY { get; set; }
        [PlainText]
        public string MAKER_REMARKS { get; set; }
        [PlainText]
        public string REVIEWER_REMARKS { get; set; }

        public int PROCESS { get; set; }
        public int SUB_PROCESS { get; set; }
        public int PROCESS_DETAIL { get; set; }

        public List<ObservationResponsiblePPNOModel> PARA_RESP { get; set; }

        }
    }
