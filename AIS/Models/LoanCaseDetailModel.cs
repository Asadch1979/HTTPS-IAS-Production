using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class LoanCaseDetailModel
        {
        [PlainText]
        public string Name { get; set; }
        [PlainText]
        public string Cnic { get; set; }
        [PlainText]
        public string LoanCaseNo { get; set; }
        public DateTime? DisbDate { get; set; }
        public DateTime? AppDate { get; set; }
        public DateTime? CadReceiveDate { get; set; }
        public DateTime? SanctionDate { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        [PlainText]
        public string McoPPNo { get; set; }
        [PlainText]
        public string McoName { get; set; }
        [PlainText]
        public string ManagerPPNo { get; set; }
        [PlainText]
        public string ManagerName { get; set; }
        [PlainText]
        public string RgmPPNo { get; set; }
        [PlainText]
        public string RgmName { get; set; }
        [PlainText]
        public string CadReviewerPPNo { get; set; }
        [PlainText]
        public string CadReviewerName { get; set; }
        [PlainText]
        public string CadAuthorizerPPNo { get; set; }
        [PlainText]
        public string CadAuthorizerName { get; set; }

        }
    }
