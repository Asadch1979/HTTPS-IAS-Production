using AIS.Validation;
namespace AIS.Models
    {
    public class EntitiesMappingModel
        {

        [PlainText]
        public string PARENT_ID { get; set; }
        [PlainText]
        public string PARENT_CODE { get; set; }
        [PlainText]
        public string CHILD_CODE { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string AUDITEDBY { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string P_NAME { get; set; }
        [PlainText]
        public string C_NAME { get; set; }
        [PlainText]
        public string P_TYPE_ID { get; set; }
        [PlainText]
        public string C_TYPE_ID { get; set; }
        [PlainText]
        public string RELATION_TYPE_ID { get; set; }

        }
    }
