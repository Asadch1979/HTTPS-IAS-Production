using AIS.Models;
using System;
using System.Collections.Generic;

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
