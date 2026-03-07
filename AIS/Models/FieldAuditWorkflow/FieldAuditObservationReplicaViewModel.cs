namespace AIS.Models.FieldAuditWorkflow
    {
    public class FieldAuditObservationReplicaViewModel
        {
        public int EngagementId { get; set; }
        public int DefaultAmountInvolved { get; set; } = 0;
        public int DefaultNoOfInstances { get; set; } = 1;
        public int DefaultReplyDays { get; set; } = 3;
        }
    }
