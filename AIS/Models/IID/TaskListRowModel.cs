using AIS.Validation;

namespace AIS.Models.IID
    {
    public class TaskListRowModel
        {
        public int ComplaintId { get; set; }
        [PlainText]
        public string ComplaintNo { get; set; }
        [PlainText]
        public string ComplainantName { get; set; }
        [PlainText]
        public string AssignedOn { get; set; }
        [PlainText]
        public string Status { get; set; }
        public int AssignedUnitId { get; set; }
        public int? PlanId { get; set; }
        }
    }
