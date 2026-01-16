using AIS.Validation;
namespace AIS.Models
    {
    public class ObservationReferenceModel
        {
        public int ComId { get; set; }
        public int EntId { get; set; }
        [PlainText]
        public string ParaTitle { get; set; }
        public int? ReferenceId { get; set; }
        [PlainText]
        public string ReferenceType { get; set; }
        public int? AssignedAuditorId { get; set; }
        [PlainText]
        public string Status { get; set; }
        }
    }
