using AIS.Validation;
namespace AIS.Models
    {
    public class RiskProcessDefinition
        {
        public int P_ID { get; set; }
        [PlainText]
        public string P_NAME { get; set; }
        public int RISK_ID { get; set; }
        }
    }
