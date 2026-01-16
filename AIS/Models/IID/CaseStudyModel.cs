using AIS.Validation;
namespace AIS.Models.IID
    {
    public class CaseStudyModel
        {
        public int? ComplaintId { get; set; }
        [PlainText]
        public string OriginProcessOwner { get; set; }
        [PlainText]
        public string NameComplainant { get; set; }
        [PlainText]
        public string Branch { get; set; }
        [PlainText]
        public string Gist { get; set; }
        [PlainText]
        public string Outcome { get; set; }
        [PlainText]
        public string ModusOperandi { get; set; }
        [PlainText]
        public string Gaps { get; set; }
        [PlainText]
        public string RootCause { get; set; }
        [PlainText]
        public string ActionsRec { get; set; }
        [PlainText]
        public string Status { get; set; }
        [PlainText]
        public string PolicyGapsIdentified { get; set; }
        [PlainText]
        public string ControlViolations { get; set; }
        [PlainText]
        public string RiskIdentified { get; set; }
        [PlainText]
        public string RegulatoryComplianceFailure { get; set; }
        }

    }
