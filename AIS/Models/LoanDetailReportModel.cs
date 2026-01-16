using AIS.Validation;
namespace AIS.Models
    {
    public class LoanDetailReportModel
        {
        [PlainText]
        public string CNIC { get; set; }
        [PlainText]
        public string BRANCHID { get; set; }
        [PlainText]
        public string LOAN_CASE_NO { get; set; }
        [PlainText]
        public string CUSTOMERNAME { get; set; }
        [PlainText]
        public string GLSUBCODE { get; set; }
        [PlainText]
        public string GLSUBNAME { get; set; }
        [PlainText]
        public string LOAN_DISB_ID { get; set; }
        [PlainText]
        public string DISB_DATE { get; set; }
        [PlainText]
        public string LAST_TRANSACTION_DATE { get; set; }
        [PlainText]
        public string VALID_UNTIL { get; set; }
        [PlainText]
        public string LAST_RECOVERY_AMOUNT { get; set; }
        [PlainText]
        public string DISB_STATUSID { get; set; }
        [PlainText]
        public string PRINCIPLE { get; set; }
        [PlainText]
        public string MARKUP { get; set; }

        }




    }

