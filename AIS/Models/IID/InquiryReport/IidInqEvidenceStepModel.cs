using System.Collections.Generic;
using AIS.Validation;

namespace AIS.Models.IID.InquiryReport
    {
    public class IidInqEvidenceStepModel
        {
        public long ComplaintId { get; set; }
        [PlainText]
        public string MaterialEvidenceDetail { get; set; }
        [PlainText]
        public string CircumstantialEvidenceDetail { get; set; }
        public List<IidInqEvidenceFileRow> EvidenceFiles { get; set; } = new List<IidInqEvidenceFileRow>();
        }
    }
