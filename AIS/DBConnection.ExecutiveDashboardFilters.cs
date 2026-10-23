using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        internal ExecutiveDashboardFilterOptions GetExecutiveDashboardFilterOptions(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            ValidateExecutiveDashboardSession(scope);
            RequireExecutiveGroupScope(scope);
            filters.Validate();
            var nodes = GetExecutiveDashboardEntities(scope).Where(n => !n.HasTypeConflict).ToDictionary(n => n.EntityId);
            var rows = new List<ExecutiveFilterEntity>();
            // Reuse the existing IAS directory lookup; only scoped rows leave this method.
            // Reporting office is REPORTING_1, matching V_EXEC_ENTITY_SCOPE.
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "PKG_AD.P_GET_AIS_ENTITIES";
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("ENT_CODE", OracleDbType.Int32).Value = DBNull.Value;
            command.Parameters.Add("ENT_NAME", OracleDbType.Varchar2).Value = "%";
            command.Parameters.Add("ENT_TYPE", OracleDbType.Int32).Value = 0;
            command.Parameters.Add("io_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = OrgNumber(reader, "entity_id");
                if (!id.HasValue || !nodes.TryGetValue(id.Value, out var node)) continue;
                // Avoid advertising an office outside GROUP even when it is a parent of its root.
                var office = node.Reporting1.HasValue && nodes.TryGetValue(node.Reporting1.Value, out var parent) ? parent : null;
                rows.Add(new ExecutiveFilterEntity { EntityId = node.EntityId, EntityName = node.EntityName,
                    EntityTypeId = node.EntityTypeId, EntityTypeName = node.EntityCategory,
                    AuditedById = OrgNumber(reader, "auditby_id"), AuditedByName = OrgText(reader, "audit_by"),
                    ReportingOfficeId = office?.EntityId, ReportingOfficeName = office?.EntityName });
            }
            ValidateExecutiveDashboardSession(scope);
            return ExecutiveDashboardFilterOptions.FromScopedEntities(rows, filters, scope.DashboardType != "RLM" && scope.PermissionPagePath != AuditParaSnapPolicy.PagePath ? null : (entityId, officeId) =>
            {
                var node = nodes[entityId];
                return node.EntityId == officeId || node.Reporting1 == officeId || node.Reporting2 == officeId ||
                    node.Reporting3 == officeId || node.Reporting4 == officeId || node.Reporting5 == officeId || node.Reporting6 == officeId;
            }, snapEntityTypes: scope.PermissionPagePath == AuditParaSnapPolicy.PagePath);
        }
    }
}
