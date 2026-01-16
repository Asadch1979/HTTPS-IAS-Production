using AIS.Validation;
namespace AIS.Models
    {
    public class AuditeeEntityUpdateModel
        {
        public int? ID { get; set; }
        public int? ENTITY_ID { get; set; }
        public int? CODE { get; set; }
        public int? CODE_OLD { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string DESCRIPTION_OLD { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string NAME_OLD { get; set; }
        [PlainText]
        public string ACTIVE { get; set; }
        [PlainText]
        public string ACTIVE_OLD { get; set; }
        public int? TYPE_ID { get; set; }
        public int? TYPE_ID_OLD { get; set; }
        [PlainText]
        public string TYPE_NAME { get; set; }
        [PlainText]
        public string TYPE_NAME_OLD { get; set; }
        public int? AUDITBY_ID { get; set; }
        public int? AUDITBY_ID_OLD { get; set; }

        [PlainText]
        public string AUDITBY_NAME { get; set; }
        [PlainText]
        public string AUDITBY_NAME_OLD { get; set; }

        [PlainText]
        public string AUDITABLE { get; set; }
        [PlainText]
        public string AUDITABLE_OLD { get; set; }

        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string STATUS_OLD { get; set; }


        public int? INSPECTEDBY_ID { get; set; }
        [PlainText]
        public string UP_STATUS { get; set; }
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
        public string ADDRESS_OLD { get; set; }
        [PlainText]
        public string TELEPHONE { get; set; }
        [PlainText]
        public string TELEPHONE_OLD { get; set; }
        [RichTextSanitize]
        public string EMAIL_ADDRESS { get; set; }
        [RichTextSanitize]
        public string EMAIL_ADDRESS_OLD { get; set; }
        [PlainText]
        public string ERISK { get; set; }
        [PlainText]
        public string ERISK_OLD { get; set; }
        [PlainText]
        public string ESIZE { get; set; }
        [PlainText]
        public string ESIZE_OLD { get; set; }
        public int? RISK_ID { get; set; }
        public int? RISK_ID_OLD { get; set; }
        public int? SIZE_ID { get; set; }
        public int? SIZE_ID_OLD { get; set; }
        [PlainText]
        public string UPDATED_BY { get; set; }
        [PlainText]
        public string UPDATE_ON { get; set; }
        [PlainText]
        public string AUTHORIZED_BY { get; set; }
        [PlainText]
        public string AUTHORIZED_ON { get; set; }
        }
    }
