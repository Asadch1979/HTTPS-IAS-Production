using System;
using AIS.Validation;

namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqEvidenceFileRow
        {
        public long EvidenceId { get; set; }
        public long ComplaintId { get; set; }
        [PlainText]
        public string EvidenceType { get; set; }
        [PlainText]
        public string Description { get; set; }
        [PlainText]
        public string FileName { get; set; }
        [PlainText]
        public string FilePath { get; set; }
        [PlainText]
        public string FileExt { get; set; }
        public int? FileSizeKb { get; set; }
        [PlainText]
        public string Status { get; set; }
        public long? UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }
    }
