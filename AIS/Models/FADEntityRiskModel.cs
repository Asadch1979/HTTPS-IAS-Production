using AIS.Validation;
namespace AIS.Models
    {
    public class FADEntityRiskModel
        {
        [PlainText]
        public string R_ID { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string RATING { get; set; }

        }
    }
