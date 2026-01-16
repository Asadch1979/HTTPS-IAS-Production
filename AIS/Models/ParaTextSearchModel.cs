using AIS.Validation;
namespace AIS.Models
    {
    public class ParaTextSearchModel
        {
        [PlainText]
        public string AUDIT_ZONE { get; set; }
        [PlainText]
        public string PARENT_NAME { get; set; }
        [PlainText]
        public string CHILD_NAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string ANNEXURE { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        [RichTextSanitize]
        public string TEXT { get; set; }

        }
    }
