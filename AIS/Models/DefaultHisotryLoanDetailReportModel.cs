using AIS.Validation;
namespace AIS.Models
    {
    public class DefaultHisotryLoanDetailReportModel
        {
        [PlainText]
        public string NPL_LOAN_DISB_ID { get; set; }
        [PlainText]
        public string DEFAULT_PRINCIPAL { get; set; }
        [PlainText]
        public string DEFAULT_MARKUP { get; set; }
        [PlainText]
        public string OUTSTANDING_PRINCIPAL_TOTAL { get; set; }
        [PlainText]
        public string OUTSTANDING_MARKUP_TOTAL { get; set; }
        [PlainText]
        public string CURRENT_STATUS { get; set; }
        [PlainText]
        public string TRANSACTION_DATE { get; set; }
        [PlainText]
        public string CNIC { get; set; }
        [PlainText]
        public string LOAN_DISB_ID { get; set; }

        }




    }

