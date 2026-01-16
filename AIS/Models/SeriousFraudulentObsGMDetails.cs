using AIS.Validation;
namespace AIS.Models
    {
    public class SeriousFraudulentObsGMDetails
        {
        [PlainText]
        public string P_NAME { get; set; }
        [PlainText]
        public string C_NAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string ANNEX_HEADING { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        [PlainText]
        public string AMOUNT_INVOLVED { get; set; }

        }
    }
