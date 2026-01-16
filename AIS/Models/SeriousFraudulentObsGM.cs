using AIS.Validation;
namespace AIS.Models
    {
    public class SeriousFraudulentObsGM
        {
        public int PARENT_ID { get; set; }
        [PlainText]
        public string P_NAME { get; set; }
        [PlainText]
        public string TOTAL_NO { get; set; }
        [PlainText]
        public string C_TOTAL_NO { get; set; }
        [PlainText]
        public string A1 { get; set; }
        [PlainText]
        public string C_A1 { get; set; }
        [PlainText]
        public string AMOUNT { get; set; }
        [PlainText]
        public string C_AMOUNT { get; set; }
        [PlainText]
        public string PER_INV { get; set; }
        [PlainText]
        public string C_PER_INV { get; set; }

        }
    }
