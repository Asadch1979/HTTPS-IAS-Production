using AIS.Validation;
namespace AIS.Models
    {
    public class YearWiseOutstandingObservationsModel
        {

        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string TOTAL_PARAS { get; set; }
        [PlainText]
        public string SETTLED_PARA { get; set; }
        [PlainText]
        public string UN_SETTLED_PARA { get; set; }
        [PlainText]
        public string R1 { get; set; }
        [PlainText]
        public string R2 { get; set; }
        [PlainText]
        public string R3 { get; set; }

        }
    }
