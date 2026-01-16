using AIS.Validation;
namespace AIS.Models.IID
    {
    public class InitialAssessmentModel
        {
        // Complaint details
        public int? ComplaintId { get; set; }
        [PlainText]
        public string Nature { get; set; }
        [PlainText]
        public string Contents { get; set; }
        [PlainText]
        public string UploadedComplaint { get; set; }
        [PlainText]
        public string UploadedFFR { get; set; }
        [PlainText]
        public string UploadedEvidence { get; set; }
        [PlainText]
        public string ActionRequired { get; set; }
        [PlainText]
        public string Status { get; set; }
        public int? SubmittedBy { get; set; }
        [PlainText]
        public string SubmittedOn { get; set; }
        public int? LocationTypeId { get; set; }
        public int? GMOfficeId { get; set; }
        public int? RegionId { get; set; }
        public int? BranchId { get; set; }

        // Assessment details
        public int? ReceivedBy { get; set; }
        [PlainText]
        public string Assessment { get; set; }
        [PlainText]
        public string Recommendation { get; set; }
        }
    }
