using AIS.Validation;
namespace AIS.Models
    {
    public class AuditChecklistModel
        {
        public int T_ID { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string RISK_SEQUENCE { get; set; }
        [PlainText]
        public string RISK_WEIGHTAGE { get; set; }
        public int ENTITY_TYPE { get; set; }
        [PlainText]
        public string ENTITY_TYPE_NAME { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        }
    }
