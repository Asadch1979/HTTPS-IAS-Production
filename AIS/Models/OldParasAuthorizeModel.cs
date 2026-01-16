using AIS.Validation;
namespace AIS.Models
    {
    public class OldParasAuthorizeModel
        {
        public int ID { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        [PlainText]
        public string AU_OBS_ID { get; set; }
        [PlainText]
        public string IND { get; set; }
        [PlainText]
        public string ENTITY_CODE { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string TYPE_ID { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }

        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }

        [PlainText]
        public string ANNEXURE { get; set; }
        [PlainText]
        public string AMOUNT_INVOLVED { get; set; }
        [PlainText]
        public string VOL_I_II { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }

        [PlainText]
        public string NEW_PARA_ID { get; set; }

        public int PROCESS { get; set; }
        public int SUB_PROCESS { get; set; }
        public int PROCESS_DETAIL { get; set; }
        [PlainText]
        public string PROCESS_DES { get; set; }
        [PlainText]
        public string SUB_PROCESS_DES { get; set; }
        [PlainText]
        public string CHECK_LIST_DETAIL_DES { get; set; }

        public int STATUS { get; set; }
        public int C_STATUS { get; set; }
        [PlainText]
        public string PARA_STATUS { get; set; }
        [PlainText]
        public string PARA_CHANGE_REQUEST_STATUS { get; set; }
        public int PARASTATUSUPDATEDBY { get; set; }

        [RichTextSanitize]
        public string PARA_TEXT { get; set; }
        [PlainText]
        public string ENTERED_BY { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string ENTERED_ON { get; set; }


        }
    }
