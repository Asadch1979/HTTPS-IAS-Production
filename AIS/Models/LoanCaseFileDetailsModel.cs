using AIS.Validation;
namespace AIS.Models
    {
    public class LoanCaseFileDetailsModel
        {
        [PlainText]
        public string LC_ID { get; set; }
        [PlainText]
        public string LC_NUMBER { get; set; }
        [PlainText]
        public string AMOUNT { get; set; }
        [PlainText]
        public string DISB_DATE { get; set; }
        [PlainText]
        public string CATEGORY { get; set; }
        [PlainText]
        public string OBSERVATION { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }

        }
    }
