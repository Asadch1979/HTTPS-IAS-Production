using System;
using AIS.Validation;

namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqRecordRow
        {
        public long RecId { get; set; }
        public long ComplaintId { get; set; }
        [PlainText]
        public string RecordTitle { get; set; }
        [PlainText]
        public string RecordDetails { get; set; }
        public int SortOrder { get; set; }
        [PlainText]
        public string Status { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }
    }
