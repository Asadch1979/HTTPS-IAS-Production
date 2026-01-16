using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class LoanCaseModel
        {

        [PlainText]
        public string TEAM_MEM_PPNO { get; set; }
        public int BRANCHID { get; set; }

        public int ENTITY_ID { get; set; }
        public int LOAN_CASE_NO { get; set; }
        public double CNIC { get; set; }
        [PlainText]
        public string CUSTOMERNAME { get; set; }
        [PlainText]
        public string FATHERNAME { get; set; }
        public double DISBURSED_AMOUNT { get; set; }
        public double PRIN { get; set; }

        public double MARKUP { get; set; }

        public int GLSUBCODE { get; set; }



        public DateTime DISB_DATE { get; set; }
        public DateTime VALID_UNTIL { get; set; }

        public int DISB_STATUSID { get; set; }


        }
    }
