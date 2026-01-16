using AIS.Validation;
namespace AIS.Models
    {
    public class UserRelationshipModel
        {

        public int SR { get; set; }
        public int ENTITY_REALTION_ID { get; set; }

        public int PARENT_ENTITY_TYPEID { get; set; }
        public int CHILD_ENTITY_TYPEID { get; set; }
        public int ENTITY_ID { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        [PlainText]
        public string FIELD_NAME { get; set; }


        [PlainText]
        public string PARENT_NAME { get; set; }
        [PlainText]
        public string ENTITYTYPEDESC { get; set; }

        [PlainText]
        public string DESCRIPTION { get; set; }

        [PlainText]
        public string CHILD_NAME { get; set; }
        [PlainText]
        public string C_NAME { get; set; }
        [PlainText]
        public string C_TYPE_ID { get; set; }

        [PlainText]
        public string COMPLICE_BY { get; set; }

        [PlainText]
        public string AUDIT_BY { get; set; }
        [PlainText]
        public string GM_OFFICE { get; set; }
        [PlainText]
        public string REPORTING { get; set; }

        [PlainText]
        public string ACTIVE { get; set; }

        }
    }
