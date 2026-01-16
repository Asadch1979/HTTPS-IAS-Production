using AIS.Validation;
namespace AIS.Models
    {
    public class LoanCaseSampleDocumentsModel
        {

        [PlainText]
        public string BRANCH_CODE { get; set; }
        [PlainText]
        public string LOAN_APP_ID { get; set; }
        [PlainText]
        public string CNIC { get; set; }
        [PlainText]
        public string CUSTOMER_NAME { get; set; }
        [PlainText]
        public string LOAN_CASE_NO { get; set; }
        [PlainText]
        public string LOAN_DISB_ID { get; set; }
        [PlainText]
        public string DOC_NAME { get; set; }
        [PlainText]
        public string IMAGE_DATA { get; set; }
        [PlainText]
        public string IMAGE_ID { get; set; }


        }
    }
