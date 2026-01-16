using AIS.Validation;
namespace AIS.Models
    {
    public class RiskGroupModel
        {
        public int GR_ID { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }

        }
    }
