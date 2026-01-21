using AIS.Validation;
namespace AIS.Models
    {
    public class ObservationResponsiblePPNOModel
        {

        public int? RESP_ROW_ID { get; set; }
        [PlainText]
        public string EMP_NAME { get; set; }
        [PlainText]
        public string PP_NO { get; set; }
        [PlainText]
        public string LOAN_CASE { get; set; }
        [PlainText]
        public string LC_AMOUNT { get; set; }
        [PlainText]
        public string ACCOUNT_NUMBER { get; set; }
        [PlainText]
        public string ACC_AMOUNT { get; set; }
        [PlainText]
        public string BR_CODE { get; set; }
        [PlainText]
        public string RESP_ACTIVE { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string ACTION { get; set; }
        [PlainText]
        public string INDICATOR { get; set; }

        // additional fields for responsibility operations
        public int? NEW_PARA_ID { get; set; }
        public int? OLD_PARA_ID { get; set; }
        public int? PARA_STATUS { get; set; }
        public int? COM_ID { get; set; }
        public int? ENG_ID { get; set; }

        }
    }
