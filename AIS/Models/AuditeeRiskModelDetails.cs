using AIS.Validation;
namespace AIS.Models
    {
    public class AuditeeRiskModeldetails
        {

        [PlainText]
        public string RISK_AREAS { get; set; }
        [PlainText]
        public string MAX_NUMBER { get; set; }
        [PlainText]
        public string NO_OBS { get; set; }
        [PlainText]
        public string RISK_MARKS { get; set; }
        [PlainText]
        public string AVG_MARKS { get; set; }
        [PlainText]
        public string G_RISK { get; set; }
        [PlainText]
        public string W_AVG { get; set; }
        }
    }
