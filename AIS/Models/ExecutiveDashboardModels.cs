namespace AIS.Models
{
    public sealed class ExecutiveKpiSummary
    {
        public long TotalCases { get; set; }
        public long OpenParas { get; set; }
        public long HighRiskParas { get; set; }
        public long ZeroCycleParas { get; set; }
        public long Over3CycleParas { get; set; }
        public long Over1YearParas { get; set; }
        public long CriticalItems { get; set; }
        public long SettledTotal { get; set; }
        public long SettledMtd { get; set; }
        public decimal SettlementRatio { get; set; }
    }

    public sealed class ExecutiveRiskSummary
    {
        public int? RiskId { get; set; }
        public string RiskName { get; set; }
        public long ParaCount { get; set; }
        public decimal Percentage { get; set; }
    }

    public sealed class ExecutiveAgingSummary
    {
        public string BucketCode { get; set; }
        public string BucketName { get; set; }
        public long ParaCount { get; set; }
        public long HighRiskCount { get; set; }
    }

    public sealed class ExecutiveCycleSummary
    {
        public string BucketCode { get; set; }
        public string BucketName { get; set; }
        public long ParaCount { get; set; }
        public long HighRiskCount { get; set; }
    }

    public sealed class ExecutiveSettlementTrend
    {
        public string SettlementMonth { get; set; }
        public long SettledParas { get; set; }
        public long HighRiskSettled { get; set; }
    }
    public sealed class ExecutiveCriticalItem
    {
        public long ComId { get; set; }
        public int? EntityId { get; set; }
        public string EntityName { get; set; }
        public int? DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int? BusinessGroupId { get; set; }
        public string BusinessGroupName { get; set; }
        public string AuditPeriod { get; set; }
        public string ParaNo { get; set; }
        public string GistOfParas { get; set; }
        public int? RiskId { get; set; }
        public string RiskName { get; set; }
        public int? ComCycle { get; set; }
        public System.DateTime? ParaAddedOn { get; set; }
        public int? AgingDays { get; set; }
        public int? DaysSinceLastMovement { get; set; }
    }

    // Server configuration only: ExecutiveDashboard:RoleMappings is an array
    // of verified IAS RoleId, DashboardType (TOP/GROUP/CCO/CIA), and PagePath.
    // No default role IDs or page grants are supplied. Missing/ambiguous mappings deny access.
    public sealed class ExecutiveDashboardRoleMapping
    {
        public int RoleId { get; set; }
        public string DashboardType { get; set; }
        public string PagePath { get; set; }
        public bool AllowBankWide { get; set; }
    }

    internal sealed class ExecutiveDashboardScope
    {
        internal ExecutiveDashboardScope(SessionUser user, string dashboardType, string pagePath, int? entityId, int? groupRootId = null)
        {
            UserId = user.ID;
            SessionId = user.SessionId;
            ContextAssignmentId = user.UserContextAssignmentId;
            RoleId = user.UserRoleID;
            SessionEntityId = user.UserEntityID;
            DashboardType = dashboardType;
            PermissionPagePath = pagePath;
            EntityId = entityId;
            GroupRootId = groupRootId;
        }

        internal string SessionId { get; }
        internal int? ContextAssignmentId { get; }
        internal int UserId { get; }
        internal int RoleId { get; }
        internal int? SessionEntityId { get; }
        internal string DashboardType { get; }
        internal string PermissionPagePath { get; }
        internal int? EntityId { get; }
        internal int? GroupRootId { get; }
    }

    internal sealed class ExecutiveDashboardAccessException : System.UnauthorizedAccessException
    {
        internal ExecutiveDashboardAccessException(string reason, string message) : base(message)
        {
            Reason = reason;
        }

        internal string Reason { get; }
    }
}
