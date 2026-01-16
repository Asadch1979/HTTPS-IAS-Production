using AIS.Validation;
namespace AIS.Models
    {
    public class RiskAssessmentEntTypeModel
        {
        [PlainText]
        public string PARENT_OFFICE { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string BRANCH_CODE { get; set; }
        [PlainText]
        public string RISK_RATING { get; set; }
        [PlainText]
        public string RISK_CATEGORY { get; set; }

        }
    }
