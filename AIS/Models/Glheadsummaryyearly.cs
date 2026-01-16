using AIS.Validation;
namespace AIS.Models
    {
    public class Glheadsummaryyearlymodel
        {

        [PlainText]
        public string COL1 { get; set; }
        [PlainText]
        public string COL2 { get; set; }
        [PlainText]
        public string COL3 { get; set; }
        public int GLSUBCODE { get; set; }
        public int BRANCHID { get; set; }
        [PlainText]
        public string GLSUBNAME { get; set; }
        public double DEBIT_2021 { get; set; }
        public double CREDIT_2021 { get; set; }
        public double BALANCE_2021 { get; set; }
        public double DEBIT_2022 { get; set; }
        public double CREDIT_2022 { get; set; }
        public double BALANCE_2022 { get; set; }
        [PlainText]
        public string LAST_CREDIT { get; set; }
        [PlainText]
        public string LAST_DEBIT { get; set; }
        [PlainText]
        public string LAST_BALANCE { get; set; }
        [PlainText]
        public string CURRENT_CREDIT { get; set; }
        [PlainText]
        public string CURRENT_DEBIT { get; set; }
        [PlainText]
        public string CURRENT_BALANCE { get; set; }

        }
    }
