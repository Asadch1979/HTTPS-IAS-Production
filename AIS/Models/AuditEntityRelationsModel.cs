using AIS.Validation;
namespace AIS.Models
    {
    public class AuditEntityRelationsModel
        {
        public int ENTITY_REALTION_ID { get; set; }
        public int PARENT_ENTITY_TYPEID { get; set; }
        public int CHILD_ENTITY_TYPEID { get; set; }
        public int ID { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string PARENT_NAME { get; set; }
        [PlainText]
        public string CHILD_NAME { get; set; }


        }
    }
