using AIS.Validation;
namespace AIS.Models
    {
    public class LoanCaseSampleTransactionsModel
        {

        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string MANUAL_VOUCHER_NO { get; set; }
        [PlainText]
        public string TRANSACTION_DATE { get; set; }
        [PlainText]
        public string? TRANSACTION_DATE_DISP { get; set; }
        public decimal DR_AMOUNT { get; set; }
        public decimal CR_AMOUNT { get; set; }
        [PlainText]
        public string LN_ACCOUNT_ID { get; set; }
        [PlainText]
        public string CREATED_ON { get; set; }
        [PlainText]
        public string? CREATED_ON_DISP { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string REJECTION_DATE { get; set; }
        [PlainText]
        public string? REJECTION_DATE_DISP { get; set; }
        [PlainText]
        public string REVERSAL_DATE { get; set; }
        [PlainText]
        public string? REVERSAL_DATE_DISP { get; set; }
        [PlainText]
        public string WORKING_DATE { get; set; }
        [PlainText]
        public string? WORKING_DATE_DISP { get; set; }
        [PlainText]
        public string AUTHORIZATION_DATE { get; set; }
        [PlainText]
        public string? AUTHORIZATION_DATE_DISP { get; set; }
        [PlainText]
        public string MCO_RECEIPT_NO { get; set; }
        [PlainText]
        public string MCO_BOOK_NO { get; set; }

        }
    }
