using AIS.Validation;
namespace AIS.Models
    {
    public class AccountOpeningDetailsModel
        {
        [PlainText]
        public string A_ID { get; set; }
        [PlainText]
        public string V_NUMBER { get; set; }
        [PlainText]
        public string A_NATURE { get; set; }
        [PlainText]
        public string OBSERVATION { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }

        }
    }
