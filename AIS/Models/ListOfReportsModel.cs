using AIS.Validation;
namespace AIS.Models
    {
    public class ListOfReportsModel
        {
        public int? REPORT_ID { get; set; }
        [PlainText]
        public string REPORT_TITLE { get; set; }
        [PlainText]
        public string DISCRIPTION { get; set; }
        [PlainText]
        public string REPORT_INDICATOR { get; set; }
        [PlainText]
        public string LOAN_STATUS { get; set; }
        [PlainText]
        public string REPORT_TYPE { get; set; }




        }
    }
