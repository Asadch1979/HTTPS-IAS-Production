using System;

namespace AIS.Models.Planning
{
    public sealed class PlanDeletionRequestInputModel
    {
        public int PlanId { get; set; }
        public string Reason { get; set; }
    }

    public sealed class PlanDeletionAuthorizationInputModel
    {
        public int RequestId { get; set; }
    }

    public sealed class AuditPlanDeletionRequestModel
    {
        public int RequestId { get; set; }
        public int PlanId { get; set; }
        public int AuditPeriodId { get; set; }
        public int EntityId { get; set; }
        public int AuditedBy { get; set; }
        public string EntityName { get; set; }
        public string PeriodName { get; set; }
        public string Reason { get; set; }
        public int RequestedBy { get; set; }
        public DateTime RequestedOn { get; set; }
    }
}
