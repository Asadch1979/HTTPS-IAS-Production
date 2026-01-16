using AIS.Validation;
namespace AIS.Models
    {
    public class DraftDSAList
        {
        [PlainText]
        public string ID { get; set; }
        [PlainText]
        public string DSA_NO { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string AZ_NAME { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string ENG_ID { get; set; }
        [PlainText]
        public string OBS_ID { get; set; }
        [PlainText]
        public string ROW_RESP_ID { get; set; }
        [PlainText]
        public string RESP_PP_NO { get; set; }
        [PlainText]
        public string EMP_NAME { get; set; }
        [PlainText]
        public string LOAN_CASE { get; set; }
        [PlainText]
        public string LC_AMOUNT { get; set; }
        [PlainText]
        public string AC_NUMBER { get; set; }
        [PlainText]
        public string AC_AMOUNT { get; set; }
        [PlainText]
        public string CREATED_BY_TEAM { get; set; }
        [PlainText]
        public string STATUS_UP { get; set; }
        [PlainText]
        public string STATUS_DOWN { get; set; }
        [PlainText]
        public string DSA_STATUS { get; set; }
        }
    }
