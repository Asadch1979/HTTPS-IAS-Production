using AIS.Validation;
namespace AIS.Models
    {
    public class VoucherCheckingDetailsModel
        {
        [PlainText]
        public string V_ID { get; set; }
        [PlainText]
        public string V_NUMBER { get; set; }
        [PlainText]
        public string CATEGORY { get; set; }
        [PlainText]
        public string OBSERVATION { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }

        }
    }
