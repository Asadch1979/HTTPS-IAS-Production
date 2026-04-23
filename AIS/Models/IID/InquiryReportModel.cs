using System;
using AIS.Validation;
namespace AIS.Models.IID
    {
    public class InquiryReportModel
        {
        public int? ComplaintId { get; set; }
        [PlainText]
        public string NameComplainant { get; set; }
        [PlainText]
        public string NameAccused { get; set; }
        [PlainText]
        public string Gist { get; set; }
        [PlainText]
        public string Proceedings { get; set; }
        
        public string Findings { get; set; }
        
        public string Recommendation { get; set; }
       
        public string Conclusion { get; set; }
        [PlainText]
        public string ReportedInAuditReport { get; set; }
        [PlainText]
        public string AuditReportReferenceDetail { get; set; }
        public string FindingsHtml { get; set; }
        public string RecommendationsHtml { get; set; }
        [PlainText]
        public string UploadedReport { get; set; }
        [PlainText]
        public string UploadedEvidence { get; set; }
        [PlainText]
        public string UploadedDsa { get; set; }
        public DateTime? SubmittedOn { get; set; }
        }

    }
