using AIS.Validation;
namespace AIS.Models
    {
    public class ComplianceOSParasModel
        {
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string PARENT_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ENTITY_TYPE { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string TOTAL_PARAS { get; set; }
        [PlainText]
        public string TOTAL_SETTLED_PARAS { get; set; }
        [PlainText]
        public string TOTAL_OUTSTANDING_PARAS { get; set; }
        [PlainText]
        public string SETTLEMENT_PERCENTAGE { get; set; }
        [PlainText]
        public string OUTSTANDING_PERCENTAGE { get; set; }
        [PlainText]
        public string COMPLIANCE_PENDING_OS_PARAS { get; set; }
        [PlainText]
        public string ZERO_COMPLIANCE_PARAS { get; set; }

        }
    }
