using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class LoanCaseSampleModel
        {

        [PlainText]
        public string TYPE { get; set; }
        [PlainText]
        public string SCHEME { get; set; }
        [PlainText]
        public string L_PURPOSE { get; set; }
        [PlainText]
        public string LC_NO { get; set; }
        [PlainText]
        public string CNIC { get; set; }
        [PlainText]
        public string CUSTOMERNAME { get; set; }
        [PlainText]
        public string APP_DATE_DISP { get; set; }
        [PlainText]
        public string DISB_DATE_DISP { get; set; }
        public decimal DEV_AMOUNT { get; set; }
        public decimal OUTSTANDING { get; set; }
        [PlainText]
        public string LOAN_DISB_ID { get; set; }


        }
    }
