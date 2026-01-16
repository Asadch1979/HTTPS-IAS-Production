using AIS.Validation;
namespace AIS.Models
    {
    public class AnnexWiseObservationModel
        {
        public int? ID { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string ANNEX { get; set; }
        [PlainText]
        public string AUDIT_COMMENTS { get; set; }
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
