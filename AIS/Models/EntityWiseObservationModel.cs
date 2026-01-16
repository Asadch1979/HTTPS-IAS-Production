using AIS.Validation;
namespace AIS.Models
    {
    public class EntityWiseObservationModel
        {
        public int? ENTITY_ID { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string OLD_TOTAL { get; set; }
        [PlainText]
        public string NEW_TOTAL { get; set; }
        [PlainText]
        public string TOTAL { get; set; }
        [PlainText]
        public string R1 { get; set; }
        [PlainText]
        public string R2 { get; set; }
        [PlainText]
        public string R3 { get; set; }

        }
    }
