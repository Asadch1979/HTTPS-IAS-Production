#nullable enable

using AIS.Validation;
namespace AIS.Models
    {
    public class CDMSMasterTransactionModel
        {

        [PlainText]
        public string TRANSACTION_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string OLD_ACCOUNT_NO { get; set; }
        [PlainText]
        public string CNIC { get; set; }
        [PlainText]
        public string ACCOUNT_NAME { get; set; }
        [PlainText]
        public string CUSTOMER_NAME { get; set; }
        [PlainText]
        public string TR_MASTER_CODE { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string TRANSACTION_DATE { get; set; }
        [PlainText]
        public string AUTHORIZATION_DATE { get; set; }
        [PlainText]
        public string? DR_AMOUNT { get; set; }
        [PlainText]
        public string? CR_AMOUNT { get; set; }
        [PlainText]
        public string? TO_ACCOUNT_ID { get; set; }
        [PlainText]
        public string TO_ACCOUNT_TITLE { get; set; }
        [PlainText]
        public string TO_ACCOUNT_NO { get; set; }
        [PlainText]
        public string? TO_ACC_BRANCH_ID { get; set; }
        [PlainText]
        public string INSTRUMENT_NO { get; set; }
        }
    }
