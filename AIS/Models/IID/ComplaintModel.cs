using AIS.Validation;
namespace AIS.Models.IID
    {
    public class ComplaintModel
        {
        [PlainText]
        public string Nature { get; set; }
        [PlainText]
        public string Category { get; set; }
        [PlainText]
        public string Source { get; set; }
        [PlainText]
        public string SourceOtherText { get; set; }
        [PlainText]
        public string PertainsTo { get; set; }
        [PlainText]
        public string FieldType { get; set; }
        public int? HOUnitTypeId { get; set; }
        public int? HOUnitId { get; set; }
        public int? RegionId { get; set; }
        public int? BranchId { get; set; }
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
        public string ComplaintNo { get; set; }
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
        }

    }
