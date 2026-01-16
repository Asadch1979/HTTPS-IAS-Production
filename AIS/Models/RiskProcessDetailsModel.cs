using AIS.Validation;
namespace AIS.Models
    {
    public class RiskProcessDetails
        {
        public int ID { get; set; }
        public int P_ID { get; set; }
        public int ENTITY_TYPE { get; set; }
        [PlainText]
        public string TITLE { get; set; }
        [PlainText]
        public string ACTIVE { get; set; }
        }
    }
