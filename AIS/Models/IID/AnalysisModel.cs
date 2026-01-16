using AIS.Validation;
namespace AIS.Models.IID
    {
    public class AnalysisModel
        {
        public int? ReportId { get; set; }
        [PlainText]
        public string PolicyGaps { get; set; }
        [PlainText]
        public string ControlGaps { get; set; }
        [PlainText]
        public string ProceduralViolations { get; set; }
        [PlainText]
        public string ForwardTo { get; set; }
        [PlainText]
        public string Comments { get; set; }
        [PlainText]
        public string Decision { get; set; }
        [PlainText]
        public string ReferBackComments { get; set; }
        }

    }
