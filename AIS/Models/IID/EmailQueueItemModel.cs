using AIS.Validation;

namespace AIS.Models.IID
    {
    public class EmailQueueItemModel
        {
        public int EmailId { get; set; }
        [PlainText]
        public string EventCode { get; set; }
        public int? RefId1 { get; set; }
        public int? RefId2 { get; set; }
        [PlainText]
        public string MailTo { get; set; }
        [PlainText]
        public string MailCc { get; set; }
        [PlainText]
        public string Subject { get; set; }
        [PlainText]
        public string Body { get; set; }
        [PlainText]
        public string Status { get; set; }
        [PlainText]
        public string CreatedOn { get; set; }
        [PlainText]
        public string SentOn { get; set; }
        [PlainText]
        public string ErrorText { get; set; }
        }
    }
