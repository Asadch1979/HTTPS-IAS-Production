using AIS.Validation;
namespace AIS.Models
    {
    public class AISEntitiesModel
        {

        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_CODE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string TYPE_ID { get; set; }
        [PlainText]
        public string AUDIT_BY_ID { get; set; }
        [PlainText]
        public string AUDIT_BY { get; set; }
        [PlainText]
        public string AUDITABLE { get; set; }



        }
    }
