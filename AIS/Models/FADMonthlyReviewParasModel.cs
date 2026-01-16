using AIS.Validation;
namespace AIS.Models
    {
    public class FADMonthlyReviewParasModel
        {
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string PLACE_OF_POSTING { get; set; }
        [PlainText]
        public string CHILD_CODE { get; set; }
        [PlainText]
        public string OPENING_BALANCE { get; set; }
        [PlainText]
        public string PARA_ADDED { get; set; }
        [PlainText]
        public string TOTAL { get; set; }
        [PlainText]
        public string SETTLED_COM { get; set; }
        [PlainText]
        public string SETTLED_AUDIT { get; set; }
        [PlainText]
        public string OUTSTANDING { get; set; }
        [PlainText]
        public string R1 { get; set; }
        [PlainText]
        public string R2 { get; set; }
        [PlainText]
        public string R3 { get; set; }

        }
    }
