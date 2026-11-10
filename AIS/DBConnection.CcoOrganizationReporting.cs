using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
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
        internal List<CcoOrganizationRow> GetCcoPerformance(ExecutiveDashboardScope scope, CcoOrganizationFilter filters)
        {
            ValidateCcoOrganizationContext(scope);
            filters.Validate();
            if (scope.EntityId.HasValue || scope.ContextAssignmentId.GetValueOrDefault() <= 0)
                throw new UnauthorizedAccessException();
            if (filters.GroupId.HasValue && !GetCcoGroupHeads(scope).Any(g => g.EntityId == filters.GroupId))
                throw new ArgumentException("Select a verified Group Head.");

            var dates = new ExecutiveDashboardFilterRequest {
                StartDate = filters.StartDate, EndDate = filters.EndDate
            }.Validate();
            // This credential has only CREATE SESSION and EXECUTE on the performance package.
            // Never fall back to the general IAS reporting connection.
            var connectionString = _configuration.GetConnectionString("CcoPerformance");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new CcoOrganizationUnavailableException();
            var connectionSettings = new OracleConnectionStringBuilder(connectionString);
            if (!string.Equals(connectionSettings.UserID, "IAS_CCO_PERF_RUNTIME", StringComparison.OrdinalIgnoreCase))
                throw new CcoOrganizationUnavailableException();

            ValidateCcoOrganizationContext(scope);
            using var connection = new OracleConnection(connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.BindByName = true;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PKG_EXEC_CCO_PERFORMANCE.P_GET_PERFORMANCE";
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("P_USER_ID", OracleDbType.Int32).Value = scope.UserId;
            command.Parameters.Add("P_CONTEXT_ID", OracleDbType.Int32).Value = scope.ContextAssignmentId.Value;
            command.Parameters.Add("P_ROLE_ID", OracleDbType.Int32).Value = scope.RoleId;
            command.Parameters.Add("P_POSTING_ID", OracleDbType.Int32).Value = (object)scope.SessionEntityId ?? DBNull.Value;
            command.Parameters.Add("P_START_DATE", OracleDbType.Date).Value = dates.StartDate;
            command.Parameters.Add("P_END_DATE", OracleDbType.Date).Value = dates.EndDate;
            command.Parameters.Add("P_GROUP_ID", OracleDbType.Int32).Value = (object)filters.GroupId ?? DBNull.Value;
            // Request one sentinel row beyond the inclusive limit.
            command.Parameters.Add("P_ROW_LIMIT", OracleDbType.Int32).Value = filters.MaxRows + 1;

            command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            var rows = new List<CcoOrganizationRow>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (rows.Count >= filters.MaxRows)
                    throw new CcoPerformanceRowLimitException();
                if (ExecutiveCount(reader, "FACT_CONFLICT") != 0)
                    throw new InvalidOperationException("Conflicting CCO para population or movement facts.");
                var opening = ExecutiveCount(reader, "OPENING_BALANCE");
                var added = ExecutiveCount(reader, "PARAS_ADDED");
                var settled = ExecutiveCount(reader, "SETTLED_PARAS");
                var outstanding = ExecutiveCount(reader, "OUTSTANDING_PARAS");
                var total = checked(opening + added);
                rows.Add(new CcoOrganizationRow {
                    RowKind = ExecutiveText(reader, "ROW_KIND"),
                    GroupId = OrgNumber(reader, "GROUP_ID"), GroupName = ExecutiveText(reader, "GROUP_NAME"),
                    DivisionId = OrgNumber(reader, "DIVISION_ID"), DivisionName = ExecutiveText(reader, "DIVISION_NAME"),
                    GmOfficeId = OrgNumber(reader, "GM_ID"), GmOfficeName = ExecutiveText(reader, "GM_NAME"),
                    ReportingEntityId = OrgNumber(reader, "ITEM_ID"),
                    ReportingEntityName = ExecutiveText(reader, "ITEM_NAME"),
                    AuditType = ExecutiveText(reader, "AUDIT_TYPE"),
                    OpeningCases = opening, ParaAdded = added, TotalCases = total, ParasSettled = settled,
                    Outstanding = outstanding,
                    SettlementRate = total == 0 ? null : Math.Round(100m * settled / total, 2, MidpointRounding.AwayFromZero),
                    NetBacklogChange = outstanding - opening,
                    HighRiskOpen = ExecutiveCount(reader, "HIGH_RISK_OPEN"),
                    ReconciliationDifference = outstanding - total + settled,
                    MappingExceptions = ExecutiveCount(reader, "MAPPING_EXCEPTIONS"),
                    MappingQuality = ExecutiveText(reader, "MAPPING_QUALITY")
                });
            }
            ValidateCcoOrganizationContext(scope);
            return rows;
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
