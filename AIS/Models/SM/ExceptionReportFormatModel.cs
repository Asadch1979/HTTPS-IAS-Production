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
                "ACCOUNT_NO",
                "CODE",
                "LC_NO",
                "TITLE",
                "DATE",
                "CELL",
                "CNIC",
                "ACCOUNT_PURPOSE",
                "ACCOUNT_TYPE",
                "TEXT_1",
                "TEXT_2",
                "DR_AMOUNT",
                "CR_AMOUNT",
                "NET_AMOUNT",
                "REMARKS"
            };



        }
    }
