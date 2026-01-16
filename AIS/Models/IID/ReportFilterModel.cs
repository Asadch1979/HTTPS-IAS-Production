using AIS.Validation;
namespace AIS.Models.IID
    {
    public class ReportFilterModel
        {
        [PlainText]
        public string Nature { get; set; }
        [PlainText]
        public string Source { get; set; }
        [PlainText]
        public string Category { get; set; }
        [PlainText]
        public string PertainsTo { get; set; }
        [PlainText]
        public string DateFrom { get; set; }
        [PlainText]
        public string DateTo { get; set; }
        public int? RegionId { get; set; }
        public int? BranchId { get; set; }
        public int? HOUnitTypeId { get; set; }
        public int? HOUnitId { get; set; }
        [PlainText]
        public string Complaint { get; set; }
        [PlainText]
        public string Accused { get; set; }
        [PlainText]
        public string Branch { get; set; }
        [PlainText]
        public string Region { get; set; }
        [PlainText]
        public string Unit { get; set; }
        [PlainText]
        public string Status { get; set; }
        }
    }
