using AIS.Validation;
namespace AIS.Models.IID
    {
    public class HeadReviewModel
        {
        public int? ComplaintId { get; set; }
        public int? AssessmentId { get; set; }
        public int? ReviewedBy { get; set; }
        [PlainText]
        public string Directions { get; set; }
        public int AssignedToUnit { get; set; }
        public int? TeamLeadId { get; set; }
        [PlainText]
        public string TeamMembers { get; set; }
        [PlainText]
        public string AssignedOn { get; set; }
        [PlainText]
        public string DueDate { get; set; }
        [PlainText]
        public string ReferredBackComments { get; set; }
        [PlainText]
        public string Action { get; set; }
        }

    }
