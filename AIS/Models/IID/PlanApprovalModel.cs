using AIS.Validation;
namespace AIS.Models.IID
    {
    public class PlanApprovalModel
        {
        public int? PlanId { get; set; }
        public int? ApprovedBy { get; set; }
        [PlainText]
        public string IsApproved { get; set; }
        [PlainText]
        public string EditedPlan { get; set; }
        [PlainText]
        public string FurtherActions { get; set; }
        }

    }
