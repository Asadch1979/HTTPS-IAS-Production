using AIS.Validation;
namespace AIS.Models
    {
    public class ListOfSamplesModel
        {
        public int? SAMPLE_ID { get; set; }
        [PlainText]
        public string SAMPLE_TYPE { get; set; }
        [PlainText]
        public string SAMPLE_PERCENTAGE { get; set; }
        [PlainText]
        public string TOTAL_COUNT { get; set; }
        [PlainText]
        public string SAMPLE_COUNT { get; set; }
        [PlainText]
        public string LOAN_STATUS { get; set; }
        [PlainText]
        public string SAMPLE_INDICATOR { get; set; }



        }
    }
