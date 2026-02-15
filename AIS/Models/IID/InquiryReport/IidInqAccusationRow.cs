using System;
using AIS.Validation;

namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqAccusationRow
        {
        public long AccusationId { get; set; }
        public long ComplaintId { get; set; }
        [PlainText]
        public string AccusationText { get; set; }
        public int SortOrder { get; set; }
        [PlainText]
        public string Status { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }
    }
