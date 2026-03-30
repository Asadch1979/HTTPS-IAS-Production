using System;
using AIS.Validation;

namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqProceedingRow
        {
        public long ProceedingId { get; set; }
        public long ComplaintId { get; set; }
        [PlainText]
        public string NoticeReference { get; set; }
        public DateTime? VisitDate { get; set; }
        [PlainText]
        public string PlaceVisited { get; set; }
        [PlainText]
        public string ParticipantsDetail { get; set; }
        [PlainText]
        public string MissingParticipantsReason { get; set; }
        public int SortOrder { get; set; }
        [PlainText]
        public string Status { get; set; }
        public long? UserId { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }
    }
