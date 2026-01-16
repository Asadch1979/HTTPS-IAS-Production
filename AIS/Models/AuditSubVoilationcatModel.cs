using AIS.Validation;
namespace AIS.Models
    {
    public class AuditSubVoilationcatModel
        {
        public int ID { get; set; }
        public int V_ID { get; set; }
        [PlainText]
        public string SUB_V_NAME { get; set; }
        [PlainText]
        public string RISK_ID { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
