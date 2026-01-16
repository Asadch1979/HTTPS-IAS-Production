using AIS.Validation;
namespace AIS.Models
    {
    public class ExecptionAccountReportModel
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
