using AIS.Validation;
namespace AIS.Models
    {
    public class GetOldParasForFinalSettlement
        {

        [PlainText]
        public string REPORTINGOFFICE { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        [PlainText]
        public string AUDITEENAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string AU_OBS_ID { get; set; }
        [PlainText]
        public string PARA_CATEGORY { get; set; }
        [RichTextSanitize]
        public string GISTOFPARA { get; set; }
        [PlainText]
        public string AMOUNT_INVOLVED { get; set; }
        [RichTextSanitize]
        public string REPLY { get; set; }
        public int? REPLIEDBY { get; set; }
        [PlainText]
        public string REPLIEDDATE { get; set; }
        [PlainText]
        public string LASTUPDATEDBY { get; set; }
        [PlainText]
        public string LASTUPDATEDDATE { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string SEQUENCE { get; set; }

        [PlainText]
        public string IMP_REMARKS { get; set; }
        [PlainText]
        public string SUBMITTED { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }
        public int? ENTITY_ID { get; set; }
        [PlainText]
        public string C_STATUS { get; set; }
        public int? EVIDENCE_ID { get; set; }
        public int? ID { get; set; }



        }
    }
