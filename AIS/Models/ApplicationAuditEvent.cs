namespace AIS.Models
    {
    public sealed class ApplicationAuditEvent
        {
        public string EventType { get; set; }
        public string ActionName { get; set; }
        public string ActionCategory { get; set; }
        public string ModuleName { get; set; }
        public string DbPackageName { get; set; }
        public string DbProcedureName { get; set; }
        public long? EngagementId { get; set; }
        public long? ParaId { get; set; }
        public long? OldParaId { get; set; }
        public long? NewParaId { get; set; }
        public long? ComId { get; set; }
        public string ObjectType { get; set; }
        public string ObjectId { get; set; }
        public string ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public string Details { get; set; }
        public string ActorPpno { get; set; }
        public int? ActorRoleId { get; set; }
        public int? ActorGroupId { get; set; }
        public int? ActorEntityId { get; set; }
        public int? ActorUserContextId { get; set; }
        public string ActorSessionId { get; set; }
        }
    }
