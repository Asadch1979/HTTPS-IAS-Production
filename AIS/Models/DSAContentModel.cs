using AIS.Validation;
namespace AIS.Models
    {
    public class DSAContentModel
        {
        [PlainText]
        public string ID { get; set; }
        [PlainText]
        public string NO { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [RichTextSanitize]
        public string TEXT { get; set; }
        }
    }
