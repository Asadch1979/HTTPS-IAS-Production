using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class DepositAccountModel
        {
        [PlainText]
        public string BRANCH_NAME { get; set; }
        public double ACC_NUMBER { get; set; }
        [PlainText]
        public string ACCOUNTCATEGORY { get; set; }
        public DateTime OPENINGDATE { get; set; }
        public double CNIC { get; set; }
        [PlainText]
        public string TITLE { get; set; }
        [PlainText]
        public string CUSTOMERNAME { get; set; }
        [PlainText]
        public string BMVS_VERIFIED { get; set; }


        [PlainText]
        public string ACCOUNTSTATUS { get; set; }

        public DateTime LASTTRANSACTIONDATE { get; set; }

        public DateTime CNICEXPIRYDATE { get; set; }

        }
    }
