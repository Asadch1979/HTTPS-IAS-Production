using AIS.Validation;

namespace AIS.Models.IID
    {
    public class FFRPart2Model
        {
        public int ComplaintId { get; set; }
        public int? FfrId { get; set; }
        [PlainText]
        public string ComplainantStatementTime { get; set; }
        [PlainText]
        public string ComplainantStatementPlace { get; set; }
        [PlainText]
        public string ComplainantStatementMode { get; set; }
        [PlainText]
        public string ComplainantStatementPoints { get; set; }
        [PlainText]
        public string AccusedStatementTime { get; set; }
        [PlainText]
        public string AccusedStatementPlace { get; set; }
        [PlainText]
        public string AccusedStatementMode { get; set; }
        [PlainText]
        public string AccusedStatementPoints { get; set; }
        [PlainText]
        public string PrimaryEvidence { get; set; }
        [PlainText]
        public string SecondaryEvidence { get; set; }
        }
    }
