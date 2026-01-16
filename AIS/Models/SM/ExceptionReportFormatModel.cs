using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using AIS.Validation;
namespace AIS.Models.SM
    {
    public class ExceptionReportFormatModel
        {
        public long? FormatId { get; set; }
        public long? ReportId { get; set; }
        [Required]
        [PlainText]
        public string ColumnName { get; set; }
        [Required]
        [MaxLength(100)]
        [PlainText]
        public string ColumnHeader { get; set; }
        public int? ColumnOrder { get; set; }
        [PlainText]
        public string DataType { get; set; }
        [PlainText]
        public string IsActive { get; set; }

        // Hardcoded list of allowed DB columns
        public static readonly List<string> AllowedColumnNames = new List<string>
            {
                "ACCOUNT_ID",
                "ACCOUNT_NO",
                "TITLE",
                "CUSTOMERNAME",

                "DOB",
                "DOB_DISP",

                "PHONE_CELL",
                "CNIC",

                "CNICEXPIRYDATE_DISP",

                "OPENINGDATE_DISP",

                "BMVS_VERIFIED",
                "ACCOUNT_PURPOSE",
                "ACCOUNT_TYPE",
                "ACCOUNT_CATEGORY",
                "ACCOUNT_STATUS",
                "RISK",

                "TRAN_DATE_DISP",

                "TRAN_AUTH_DATE_DISP",

                "TRANSACTIONMASTERCODE",
                "TRANSACTION_DISCRIPTION",
                "TR_AMOUNT",
                "ACCOUNT_BALANCE",

                "LAST_TRAN_DATE_DISP",

                "SOURCE_OF_FUND",
                "NATURE_OF_TRANSACTION",

                "DATE_OF_CLOSURE_DISP",

                "ZAKAT_EXEMPTED_DATE_DISP",

                "TXN_COUNT_PERIOD",
                "TOTAL_DR_PERIOD",
                "TOTAL_CR_PERIOD",

                "LAST_AUTH_DATE_DISP",

                "LAST_DR_AMOUNT",
                "LAST_CR_AMOUNT",
                "BANK_EMPLOYEE"
            };



        }
    }
