using AIS.Validation;
namespace AIS.Models
    {
    public class PostComplianceHistoryModel
        {
        public int HIST_ID { get; set; }
        public int COM_ID { get; set; }
        [PlainText]
        public string COM_CYCLE { get; set; }
        [PlainText]
        public string COM_STATUS { get; set; }
        [PlainText]
        public string COM_STAGE { get; set; }
        [PlainText]
        public string COMMENT_BY_ROLE { get; set; }
        [PlainText]
        public string PP_NO { get; set; }
        [PlainText]
        public string DESIGNATION { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string COMMENT_ON { get; set; }
        [PlainText]
        public string COMMENTS { get; set; }
        [PlainText]
        public string COM_FLOW { get; set; }
        }
    }
