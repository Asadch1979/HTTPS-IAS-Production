using System.Collections.Generic;

namespace AIS.Models
{
    public sealed class ExecutiveDashboardPageModel
    {
        public string DashboardType { get; set; }
        public string ScopeLabel { get; set; }
        public int? SelectedEntityId { get; set; }
        public bool CanViewOrganization { get; set; }
        public List<OrganizationNode> Entities { get; set; }
        public List<AuditPeriodModel> AuditPeriods { get; set; }
    }
}
