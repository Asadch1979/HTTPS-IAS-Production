using AIS.Validation;
namespace AIS.Models.IID
    {
    public class InitialAssessmentModel
        {
        // Complaint details
        public int? ComplaintId { get; set; }
        [PlainText]
        public string ComplaintNo { get; set; }
        [PlainText]
        public string Nature { get; set; }
        [PlainText]
        public string Category { get; set; }
        [PlainText]
        public string ComplainantName { get; set; }
        [PlainText]
        public string CNIC { get; set; }
        [PlainText]
        public string CellularNumber { get; set; }
        [PlainText]
        public string MailingAddress { get; set; }
        [PlainText]
        public string Gender { get; set; }
        [PlainText]
        public string ReceivedFrom { get; set; }
        [PlainText]
        public string LocationTypeText { get; set; }
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
        [PlainText]
        public string GMOffice { get; set; }
        [PlainText]
        public string Region { get; set; }
        [PlainText]
        public string Branch { get; set; }

        // Assessment details
        public int? ReceivedBy { get; set; }
        [PlainText]
        public string Assessment { get; set; }
        [PlainText]
        public string Recommendation { get; set; }
        public int AssignedUnitId { get; set; }
        [PlainText]
        public string AssignedUnit { get; set; }
        }
    }
