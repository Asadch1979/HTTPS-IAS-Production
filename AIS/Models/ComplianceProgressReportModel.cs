using AIS.Validation;
namespace AIS.Models
    {
    public class ComplianceProgressReportModel
        {

        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string TOTAL { get; set; }
        [PlainText]
        public string REFERRED_BACK { get; set; }
        [RichTextSanitize]
        public string RECOMMENDED { get; set; }
        [PlainText]
        public string PENDING { get; set; }
        [PlainText]
        public string LAST_LOGIN_ON { get; set; }

        }
    }
