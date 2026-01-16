using AIS.Validation;
namespace AIS.Models
    {
    public class FunctionalResponsibilityWiseParas
        {
        public int PROCESS_ID { get; set; }
        [PlainText]
        public string PROCESS { get; set; }
        public int SUB_PROCESS_ID { get; set; }
        [PlainText]
        public string VIOLATION { get; set; }
        public int CHECK_LIST_DETAIL_ID { get; set; }
        [PlainText]
        public string PERIOD { get; set; }
        public int OBS_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [RichTextSanitize]
        public string OBS_TEXT { get; set; }
        [PlainText]
        public string SUB_PROCESS { get; set; }
        [PlainText]
        public string MEMO_NO { get; set; }
        [PlainText]
        public string OBS_RISK { get; set; }
        public int OBS_RISK_ID { get; set; }
        [PlainText]
        public string OBS_STATUS { get; set; }
        public int OBS_STATUS_ID { get; set; }
        }
    }
