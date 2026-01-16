using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class GetOldParasforComplianceSettlement
        {
        public int? ID { get; set; }
        [PlainText]
        public string AU_OBS_ID { get; set; }
        [PlainText]
        public string REPORTINGOFFICE { get; set; }
        [PlainText]
        public string AUDITEENAME { get; set; }
        [PlainText]
        public string AUDITPERIOD { get; set; }
        [PlainText]
        public string PARANO { get; set; }
        [PlainText]
        public string HEAD_REF_REMARKS { get; set; }
        [RichTextSanitize]
        public string GISTOFPARA { get; set; }
        [PlainText]
        public string AMOUNT { get; set; }
        [PlainText]
        public string REPLIEDBY { get; set; }
        [PlainText]
        public string VOL_I_II { get; set; }
        [RichTextSanitize]
        public string REPLY { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string REPLIEDDATE { get; set; }
        public DateTime LASTUPDATEDDATE { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string PARA_CATEGORY { get; set; }
        [PlainText]
        public string LASTUPDATEDBY { get; set; }
        public int? EVIDENCE_ID { get; set; }
        [PlainText]
        public string REVIEWED_BY { get; set; }
        [PlainText]
        public string REVIEWER_REMARKS { get; set; }
        [RichTextSanitize]
        public string REVIEWER_RECOMMENDATION { get; set; }
        [PlainText]
        public string SUBMITTED { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }
        [PlainText]
        public string C_STATUS { get; set; }
        [PlainText]
        public string SEQUENCE { get; set; }

        }
    }
