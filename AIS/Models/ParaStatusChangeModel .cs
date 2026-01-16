using AIS.Validation;
namespace AIS.Models
    {
    public class ParaStatusChangeModel
        {
        [PlainText]
        public string COM_ID { get; set; }
        [PlainText]
        public string NEW_PARA_ID { get; set; }
        [PlainText]
        public string OLD_PARA_ID { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string PARA_STATUS { get; set; }
        [PlainText]
        public string NEW_PARA_STATUS { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string IND { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }

        }
    }
