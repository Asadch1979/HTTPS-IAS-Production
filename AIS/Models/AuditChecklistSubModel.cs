using AIS.Validation;
namespace AIS.Models
    {
    public class AuditChecklistSubModel
        {
        public int S_ID { get; set; }
        public int T_ID { get; set; }
        [PlainText]
        public string T_NAME { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        public int ENTITY_TYPE { get; set; }
        [PlainText]
        public string ENTITY_TYPE_NAME { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        }
    }
