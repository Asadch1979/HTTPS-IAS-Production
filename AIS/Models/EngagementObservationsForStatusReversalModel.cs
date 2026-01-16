using AIS.Validation;
namespace AIS.Models
    {
    public class EngagementObservationsForStatusReversalModel
        {

        [PlainText]
        public string ID { get; set; }
        [PlainText]
        public string MEMO_NO { get; set; }
        [RichTextSanitize]
        public string GIST { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string MEMO_DATE { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string FINAL_PARA { get; set; }
        }
    }
