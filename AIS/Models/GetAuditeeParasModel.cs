using AIS.Validation;
namespace AIS.Models
    {
    public class GetAuditeeParasModel
        {
        [PlainText]
        public string MEMO_NUMBER { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [RichTextSanitize]
        public string TEXT { get; set; }
        [RichTextSanitize]
        public string REPLY { get; set; }
        [RichTextSanitize]
        public string RECOMMENDATION { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string REPLYROLE { get; set; }
        [PlainText]
        public string HEADREMARKS { get; set; }
        [PlainText]
        public string ASSIGNEDTO { get; set; }
        [PlainText]
        public string REF_OUT { get; set; }
        [PlainText]
        public string MESSAGE { get; set; }

        }
    }
