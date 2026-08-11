using AIS.Validation;

namespace AIS.Models
    {
    public class BACDSADetailModel
        {
        public int OBSERVATION_ID { get; set; }
        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string EMP_NAME { get; set; }
        [RichTextSanitize]
        public string DSA_TEXT { get; set; }
        }
    }
