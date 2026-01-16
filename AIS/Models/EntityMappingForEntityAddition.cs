using AIS.Validation;
namespace AIS.Models
    {
    public class EntityMappingForEntityAddition
        {
        [PlainText]
        public string PARENT_ID { get; set; }
        [PlainText]
        public string PARENT_CODE { get; set; }
        [PlainText]
        public string CHILD_CODE { get; set; }
        [PlainText]
        public string CHILD_ID { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string PARENT_NAME { get; set; }
        [PlainText]
        public string CHILD_NAME { get; set; }
        [PlainText]
        public string PARENT_TYPE_ID { get; set; }
        [PlainText]
        public string CHILD_TYPE_ID { get; set; }
        [PlainText]
        public string RELATION_TYPE_ID { get; set; }

        }
    }
