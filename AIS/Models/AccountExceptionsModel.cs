using AIS.Validation;
namespace AIS.Models
    {
    public class AccountExceptionsModel
        {
        [PlainText]
        public string BRANCH_CODE { get; set; }
        [PlainText]
        public string ACCOUNT_ID { get; set; }
        [PlainText]
        public string ACCOUNT_NO { get; set; }
        [PlainText]
        public string ACCOUNT_TITLE { get; set; }
        [PlainText]
        public string CUSTOMER_NAME { get; set; }
        [PlainText]
        public string DOB { get; set; }
        [PlainText]
        public string PHONECELL { get; set; }
        [PlainText]
        public string CNIC { get; set; }
        [PlainText]
        public string CNIC_EXPIRY_DATE { get; set; }
        [PlainText]
        public string OPENING_DATE { get; set; }
        [PlainText]
        public string BMVS_VERIFIED { get; set; }
        [PlainText]
        public string PURPOSE { get; set; }
        [PlainText]
        public string ACC_TYPE { get; set; }
        [PlainText]
        public string ACC_CATEGORY { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string MASTER_CODE { get; set; }
        [PlainText]
        public string TR_DESCRIPTION { get; set; }
        [PlainText]
        public string TR_DATE { get; set; }
        [PlainText]
        public string TR_AUTHDATE { get; set; }
        [PlainText]
        public string DR_AMOUNT { get; set; }
        [PlainText]
        public string CR_AMOUNT { get; set; }

        }
    }
