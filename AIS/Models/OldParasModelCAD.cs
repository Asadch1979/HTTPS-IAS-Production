using AIS.Validation;
namespace AIS.Models
    {
    public class OldParasModelCAD
        {
        public int PARA_ID { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        public int ENTITY_ID { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [RichTextSanitize]
        public string PARA_TEXT { get; set; }
        [PlainText]
        public string PARA_STATUS { get; set; }
        public int V_CAT_ID { get; set; }
        public int V_CAT_NATURE_ID { get; set; }
        public int RISK_ID { get; set; }
        }
    }
