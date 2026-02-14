using AIS.Validation;
namespace AIS.Models.IID
    {
    public class InvestigationPlanModel
        {
        public int? ComplaintId { get; set; }
        [PlainText]
        public string PlanDetails { get; set; }
        public int? SubmittedBy { get; set; }
        [PlainText]
        public string Status { get; set; }
        }

    }
