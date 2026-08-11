using AIS.Validation;

namespace AIS.Models
    {
    public class BACAnalysisModel
        {
        public int ID { get; set; }
        public int ANNEX_ID { get; set; }
        [PlainText]
        public string PROCESS { get; set; }
        [PlainText]
        public string ANNEXURE_CODE { get; set; }
        [PlainText]
        public string ANNEXURE { get; set; }
        public decimal ISSUES_IDENTIFIED { get; set; }
        public decimal RECTIFIED { get; set; }
        public decimal OPEN { get; set; }
        public decimal DSA { get; set; }
        [PlainText]
        public string AFFECTED_ENTITIES { get; set; }
        [PlainText]
        public string OPEN_ENTITIES { get; set; }
        public bool HAS_OPEN_ENTITIES_COLUMN { get; set; }
        public decimal AMOUNT_INVOLVED { get; set; }
        }
    }
