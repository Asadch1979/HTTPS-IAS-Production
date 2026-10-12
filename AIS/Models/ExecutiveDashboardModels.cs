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

    // Server configuration only: ExecutiveDashboard:RoleMappings is an array
    // of verified IAS RoleId, DashboardType (TOP/GROUP/CCO/CIA), and PagePath.
    // No default role IDs or page grants are supplied. Missing/ambiguous mappings deny access.
    public sealed class ExecutiveDashboardRoleMapping
    {
        public int RoleId { get; set; }
        public string DashboardType { get; set; }
        public string PagePath { get; set; }
    }

    internal sealed class ExecutiveDashboardScope
    {
        internal ExecutiveDashboardScope(SessionUser user, string dashboardType, string pagePath, int? entityId)
        {
            UserId = user.ID;
            RoleId = user.UserRoleID;
            SessionEntityId = user.UserEntityID.Value;
            DashboardType = dashboardType;
            PermissionPagePath = pagePath;
            EntityId = entityId;
        }

        internal int UserId { get; }
        internal int RoleId { get; }
        internal int SessionEntityId { get; }
        internal string DashboardType { get; }
        internal string PermissionPagePath { get; }
        internal int? EntityId { get; }
    }
}
