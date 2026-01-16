using AIS.Validation;
namespace AIS.Models
    {
    public class AuditEntitiesModel
        {
        public int TYPE_ID { get; set; }
        public int AUTID { get; set; }
        public int? E_AUTID { get; set; }
        [PlainText]
        public string ENTITYCODE { get; set; }
        [PlainText]
        public string ENTITYTYPEDESC { get; set; }
        [PlainText]
        public string AUDITABLE { get; set; }
        [PlainText]
        public string AUDITEDBY { get; set; }
        [PlainText]
        public string AUDITED_BY_ENTITY { get; set; }
        [PlainText]
        public string AUDIT_TYPE { get; set; }
        [PlainText]
        public string D_RISK { get; set; }

        }
    }
