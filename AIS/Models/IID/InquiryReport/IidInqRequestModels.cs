using System;

namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqComplaintRequest
        {
        public long ComplaintId { get; set; }
        }

    public class IidInqDeleteRequest
        {
        public long Id { get; set; }
        public long UserId { get; set; }
        }

    public class IidEmployeeInfoRequest
        {
        public long PpNo { get; set; }
        }

    public class ComplaintIdRequest
        {
        public long ComplaintId { get; set; }
        }

    public class FindingsRecommGetRequest
        {
        public long ComplaintId { get; set; }
        public long AccusationId { get; set; }
        }

    public class FindingsRecommRequest
        {
        public long ComplaintId { get; set; }
        public long AccusationId { get; set; }
        public string FindingText { get; set; }
        public string RecomText { get; set; }
        }

    public class IidAccusationForFindingsRow
        {
        public long AccusationId { get; set; }
        public string AccusationText { get; set; }
        }

    public class IidFindingsRecommStatusRow
        {
        public long AccusationId { get; set; }
        public string AccusationText { get; set; }
        public string IsSaved { get; set; }
        public DateTime? SavedOn { get; set; }
        }
    }
