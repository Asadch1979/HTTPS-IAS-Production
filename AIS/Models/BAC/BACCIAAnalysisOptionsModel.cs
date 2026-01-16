using AIS.Validation;
namespace AIS.Models
    {
    public class BACCIAAnalysisOptionsModel
        {
        public int ID { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string AUDIT_COMMENTS { get; set; }
        [PlainText]
        public string AUTOMATION { get; set; }
        [PlainText]
        public string MONITORING { get; set; }



        }
    }
