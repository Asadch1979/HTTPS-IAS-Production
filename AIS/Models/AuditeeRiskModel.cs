using AIS.Validation;
namespace AIS.Models
    {
    public class AuditeeRiskModel
        {

        [PlainText]
        public string RISK_AREAS { get; set; }
        [PlainText]
        public string MAX_NUMBER { get; set; }
        [PlainText]
        public string MARKS { get; set; }
        }
    }
