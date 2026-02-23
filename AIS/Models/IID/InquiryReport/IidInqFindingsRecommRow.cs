using System;

namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqFindingsRecommRow
        {
        public long ComplaintId { get; set; }
        public long AccusationId { get; set; }
        public string FindingText { get; set; }
        public string RecommendationText { get; set; }
        public string Ppno { get; set; }
        public bool Ok { get; set; }
        public string Message { get; set; }
        public DateTime? UpdatedOn { get; set; }
        }
    }
