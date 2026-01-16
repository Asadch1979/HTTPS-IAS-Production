using AIS.Validation;
namespace AIS.Models
    {
    public class ControlViolationsModel
        {
        public int ID { get; set; }
        [PlainText]
        public string V_NAME { get; set; }
        public int MAX_NUMBER { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
