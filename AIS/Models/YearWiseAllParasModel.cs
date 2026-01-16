using AIS.Validation;
namespace AIS.Models
    {
    public class YearWiseAllParasModel
        {
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string ENTITY_TYPE { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ENTITY_RISK_LEVEL { get; set; }
        [PlainText]
        public string FUNCTION_RESP { get; set; }
        [PlainText]
        public string AUDIT_ZONE { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string ANNEXURE { get; set; }
        [PlainText]
        public string AMOUNT_INVOLVED { get; set; }
        [PlainText]
        public string NO_OF_INSTANCES { get; set; }
        [PlainText]
        public string PARA_STATUS { get; set; }

        }
    }
