using AIS.Validation;

namespace AIS.Models
    {
    public class ResponsibilitySearchResultModel
        {
        [PlainText]
        public string Role { get; set; }
        [PlainText]
        public string PPNo { get; set; }
        [PlainText]
        public string EmpName { get; set; }
        [PlainText]
        public string LoanCase { get; set; }
        [PlainText]
        public string LCAmount { get; set; }
        [PlainText]
        public string AccountNumber { get; set; }
        [PlainText]
        public string AccAmount { get; set; }
        }
    }
