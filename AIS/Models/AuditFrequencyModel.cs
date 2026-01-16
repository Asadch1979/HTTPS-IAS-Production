using AIS.Validation;
namespace AIS.Models
    {
    public class AuditFrequencyModel
        {
        public int ID { get; set; }
        public int FREQUENCY_ID { get; set; }
        [PlainText]
        public string FREQUENCY_DISCRIPTION { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
