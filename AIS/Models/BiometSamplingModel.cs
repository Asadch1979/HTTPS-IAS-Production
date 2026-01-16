using AIS.Validation;
namespace AIS.Models
    {
    public class BiometSamplingModel
        {
        [PlainText]
        public string BRANCH_CODE { get; set; }
        [PlainText]
        public string ACCOUNT_NO { get; set; }
        [PlainText]
        public string ACCOUNT_TITLE { get; set; }
        [PlainText]
        public string CUSTOMER_NAME { get; set; }
        [PlainText]
        public string DOB { get; set; }
        [PlainText]
        public string? DOB_DISP { get; set; }
        [PlainText]
        public string PHONE_CELL { get; set; }
        [PlainText]
        public string CNIC { get; set; }
        [PlainText]
        public string CNIC_EXPIRY_DATE { get; set; }
        [PlainText]
        public string? CNIC_EXPIRY_DATE_DISP { get; set; }
        [PlainText]
        public string OPENING_DATE { get; set; }
        [PlainText]
        public string? OPENING_DATE_DISP { get; set; }
        [PlainText]
        public string BMVS_VERIFIED { get; set; }
        [PlainText]
        public string PURPOSE { get; set; }
        [PlainText]
        public string ACCOUNT_TYPE { get; set; }
        [PlainText]
        public string ACCOUNT_CATEGORY { get; set; }
        [PlainText]
        public string RISK { get; set; }

        }
    }
