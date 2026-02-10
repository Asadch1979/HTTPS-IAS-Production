using AIS.Validation;

namespace AIS.Models.IID
    {
    public class EmailTemplateInputModel
        {
        [PlainText]
        public string EventCode { get; set; }
        public int ComplaintId { get; set; }
        public int? PlanId { get; set; }
        [PlainText]
        public string MailTo { get; set; }
        [PlainText]
        public string MailCc { get; set; }
        [PlainText]
        public string Subject { get; set; }
        [PlainText]
        public string Body { get; set; }
        }
    }
