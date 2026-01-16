using AIS.Validation;
namespace AIS.Models
    {
    public class BACCIAAnalysisModel
        {
        public int ID { get; set; }
        [PlainText]
        public string ANNEX { get; set; }
        [PlainText]
        public string INDICATOR { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string AUTOMATION { get; set; }
        [PlainText]
        public string MONITORING { get; set; }
        [PlainText]
        public string AUDITCOMMENTS { get; set; }
        [PlainText]
        public string COUNT { get; set; }
        [PlainText]
        public string OLDCOUNT { get; set; }
        [PlainText]
        public string NEWCOUNT { get; set; }

        }
    }
