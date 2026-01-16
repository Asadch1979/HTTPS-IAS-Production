using AIS.Validation;
namespace AIS.Models
    {
    public class GetFinalReportModel
        {
        [PlainText]
        public string ASSIGNEDTO { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string MEMO_NUMBER { get; set; }


        [PlainText]
        public string V_HEADER { get; set; }
        [PlainText]
        public string V_DETAIL { get; set; }

        [PlainText]
        public string RISK { get; set; }


        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string OBSERVATION { get; set; }
        [RichTextSanitize]
        public string MANAGEMENT_REPLY { get; set; }
        [RichTextSanitize]
        public string RECOMMENDATION { get; set; }
        [PlainText]
        public string MESSAGE { get; set; }
        [PlainText]
        public string REMARKS { get; set; }


        [PlainText]
        public string REF_OUT { get; set; }


        }
    }
