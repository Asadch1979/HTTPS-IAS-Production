using AIS.Validation;
namespace AIS.Models
    {
    public class AccountTransactionSampleModel
        {
        [PlainText]
        public string TransactionMasterCode { get; set; }
        [PlainText]
        public string Description { get; set; }
        [PlainText]
        public string Remarks { get; set; }
        [PlainText]
        public string TransactionDate { get; set; }
        [PlainText]
        public string? TransactionDateDisp { get; set; }
        [PlainText]
        public string AuthorizationDate { get; set; }
        [PlainText]
        public string? AuthorizationDateDisp { get; set; }
        [PlainText]
        public string DrAmount { get; set; }
        [PlainText]
        public string CrAmount { get; set; }
        [PlainText]
        public string ToAccountId { get; set; }
        [PlainText]
        public string ToAccountTitle { get; set; }
        [PlainText]
        public string ToAccountNo { get; set; }
        [PlainText]
        public string ToAccBranchId { get; set; }
        [PlainText]
        public string InstrumentNo { get; set; }



        }
    }
