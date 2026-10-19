using System.Collections.Generic;

namespace AIS.Models
{
    public sealed class ExecutiveDashboardPageModel
    {
        public List<ExecutiveDashboardViewDefinition> Views { get; set; }
        public ExecutiveDashboardViewDefinition SelectedView { get; set; }
        public string DashboardType { get; set; }
        public string ScopeLabel { get; set; }
        public int? SelectedEntityId { get; set; }
        public bool IsSuperUser { get; set; }
        public int? GroupRootId { get; set; }
        public List<OrganizationNode> Groups { get; set; }
        public bool CanViewOrganization { get; set; }
        public bool RoleOracleEnabled { get; set; }
        public bool IsManagementPerspective { get; set; }
        public bool NeedsRlmRoot { get; set; }
        public List<OrganizationNode> RlmRoots { get; set; } = new();
        public bool IsAuditParaSnap { get; set; }
        public bool AuditParaSnapEnabled { get; set; }
        public int? SelectedCiaPeriodId { get; set; }
        public List<AuditPeriodModel> CiaPeriods { get; set; } = new();
        public ExecutiveDashboardFilterRequest Filters { get; set; }
        public ExecutiveDashboardFilterOptions FilterOptions { get; set; }
        public List<OrganizationNode> ComparisonEntities { get; set; } = new();
    }
}
