using AIS.Validation;
namespace AIS.Models
    {
    public class ComplianceHierarchyModel
        {
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string COMPLIANCE_UNIT { get; set; }
        [PlainText]
        public string APPROVER_PPNO { get; set; }
        [PlainText]
        public string APPROVER_NAME { get; set; }
        [PlainText]
        public string REVIEWER_PPNO { get; set; }
        [PlainText]
        public string REVIEWER_NAME { get; set; }
        [PlainText]
        public string COM_KEY { get; set; }

        }
    }
