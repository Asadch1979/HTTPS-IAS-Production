using AIS.Validation;

namespace AIS.Models
    {
    public class BACParaTextModel
        {
        public int OBSERVATION_ID { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [RichTextSanitize]
        public string PARA_TEXT { get; set; }
        }
    }
