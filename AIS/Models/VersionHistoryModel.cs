using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class VersionHistoryModel
        {
        public int? VersionId { get; set; }
        [PlainText]
        public string VersionNo { get; set; }
        public DateTime? ReleaseDate { get; set; }
        [PlainText]
        public string Description { get; set; }
        [PlainText]
        public string ReleasedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        [PlainText]
        public string IsActive { get; set; }
        }
    }
