using AIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        internal void ValidateCcoOrganizationContext(ExecutiveDashboardScope scope)
        {
            var user=ValidateExecutiveDashboardSession(scope);
            if (scope.DashboardType!="CCO" || scope.PermissionPagePath!="/ExecutiveOverview/CCO" || scope.GroupRootId.HasValue ||
                (user.UserRoleID!=38 && !(user.UserRoleID==1 && CreateSessionHandler().IsSuperUser())))
                throw new UnauthorizedAccessException();
        }
        // Group Head selector: read the existing IAS entity hierarchy only after
        // the authenticated CCO role/context/page has been checked by its caller.
        // Do not use user-provided IDs as an authorization root.
        internal List<OrganizationNode> GetCcoGroupHeads(ExecutiveDashboardScope scope)
        {
            ValidateCcoOrganizationContext(scope);
            var groups = GetOrganizationNodes(null)
                .Where(n => n.EntityTypeId == 18 && !n.HasTypeConflict && n.EntityId > 0)
                .GroupBy(n => n.EntityId)
                .Where(g => g.Count() == 1)
                .Select(g => g.Single())
                .OrderBy(g => g.EntityName).ToList();
            ValidateCcoOrganizationContext(scope);
            return groups;
        }
        internal List<CcoOrganizationPath> GetCcoOrganizationPaths(ExecutiveDashboardScope scope, CcoOrganizationFilter filters)
        {
            ValidateCcoOrganizationContext(scope);
            filters.Validate();
            // No configuration switch can bypass this gate. The approved Oracle
            // context/path adapter does not yet exist; never fall back to unscoped
            // directory data or assume a NULL root authorizes the whole bank.
            throw new CcoOrganizationUnavailableException();
        }
        internal List<CcoOrganizationRow> GetCcoOrganizationRows(ExecutiveDashboardScope scope, CcoOrganizationFilter filters, string action)
        {
            ValidateCcoOrganizationContext(scope);
            filters.Validate();
            // Implement bound package calls only after approved scope-token minting,
            // taxonomy/preflight, package compile and source reconciliation. Recheck
            // context before/after execution, reject source_row_count > MaxRows.
            throw new CcoOrganizationUnavailableException();
        }
    }
}
