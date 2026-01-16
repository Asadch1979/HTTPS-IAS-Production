using AIS.Validation;
namespace AIS.Models
    {
    public class SettledParasModel
        {
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string PARENT_ID { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string PLACE_OF_POSTING { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [RichTextSanitize]
        public string GIST { get; set; }
        [PlainText]
        public string SETTLED_ON { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }

        }
    }
