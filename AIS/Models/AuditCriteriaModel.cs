using AIS.Validation;
namespace AIS.Models
    {
    public class AuditCriteriaModel
        {
        public int ID { get; set; }
        public int ENTITY_TYPEID { get; set; }
        public int ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ENTITY { get; set; }
        public int SIZE_ID { get; set; }
        [PlainText]
        public string SIZE { get; set; }
        public int RISK_ID { get; set; }
        [PlainText]
        public string RISK { get; set; }
        public int FREQUENCY_ID { get; set; }
        [PlainText]
        public string FREQUENCY { get; set; }
        public int NO_OF_DAYS { get; set; }
        [PlainText]
        public string VISIT { get; set; }
        public int APPROVAL_STATUS { get; set; }
        public int AUDITPERIODID { get; set; }
        [PlainText]
        public string PERIOD { get; set; }
        [PlainText]
        public string COMMENTS { get; set; }
        public int ENTITIES_COUNT { get; set; }
        public int CREATED_BY { get; set; }
        public bool CRITERIA_SUBMITTED { get; set; }

        }
    }
