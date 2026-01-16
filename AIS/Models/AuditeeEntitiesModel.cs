using AIS.Validation;
namespace AIS.Models
    {
    public class AuditeeEntitiesModel
        {
        public int? ENTITY_ID { get; set; }
        public int? CODE { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string ACTIVE { get; set; }
        public int? TYPE_ID { get; set; }
        [PlainText]
        public string TYPE_NAME { get; set; }
        public int? AUDITBY_ID { get; set; }

        [PlainText]
        public string AUDITBY_NAME { get; set; }

        [PlainText]
        public string AUDITABLE { get; set; }

        [PlainText]
        public string STATUS { get; set; }


        public int? INSPECTEDBY_ID { get; set; }
        [PlainText]
        public string COST_CENTER { get; set; }
        [PlainText]
        public string AUDITOR { get; set; }
        [PlainText]
        public string IAD { get; set; }
        [PlainText]
        public string COMPLICE_BY { get; set; }
        [PlainText]
        public string COMPLIANCE_UNIT { get; set; }
        [PlainText]
        public string ADDRESS { get; set; }
        [PlainText]
        public string TELEPHONE { get; set; }
        [RichTextSanitize]
        public string EMAIL_ADDRESS { get; set; }
        [PlainText]
        public string ERISK { get; set; }
        [PlainText]
        public string ESIZE { get; set; }
        public int? RISK_ID { get; set; }
        public int? SIZE_ID { get; set; }
        public int? ENG_ID { get; set; }

        [PlainText]
        public string COM_BY { get; set; }
        }
    }
