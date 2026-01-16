using AIS.Validation;
namespace AIS.Models
    {
    public class DepositAccountCatModel
        {
        [PlainText]
        public string BRANCH_NAME { get; set; }
        [PlainText]
        public string ACCOUNTCATEGORY { get; set; }
        public int? ACCOUNTCATEGORYID { get; set; }
        public double AMOUNT { get; set; }
        [PlainText]
        public string ACCOCUNTSTATUS { get; set; }

        }
    }
