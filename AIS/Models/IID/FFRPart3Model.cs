using AIS.Validation;

namespace AIS.Models.IID
    {
    public class FFRPart3Model
        {
        public int ComplaintId { get; set; }
        public int? FfrId { get; set; }
        [PlainText]
        public string AuditHighlighted { get; set; }
        [PlainText]
        public string AuditHighlightDetails { get; set; }
        public bool ImplicationReputational { get; set; }
        public bool ImplicationOperational { get; set; }
        public bool ImplicationFinancial { get; set; }
        public bool ImplicationPrecedence { get; set; }
        public bool ImplicationOther { get; set; }
        [PlainText]
        public string ImplicationOtherDetails { get; set; }
        [PlainText]
        public string PolicyViolated { get; set; }
        [PlainText]
        public string SopGaps { get; set; }
        [PlainText]
        public string ActionRecommended { get; set; }
        }
    }
