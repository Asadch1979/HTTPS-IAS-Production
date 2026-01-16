using AIS.Validation;
namespace AIS.Models.IID
    {
    public class InquiryReportFilesModel
        {
        public int? ReportId { get; set; }
        [PlainText]
        public string UploadedReport { get; set; }
        [PlainText]
        public string UploadedEvidence { get; set; }
        [PlainText]
        public string UploadedDsa { get; set; }
        }
    }
