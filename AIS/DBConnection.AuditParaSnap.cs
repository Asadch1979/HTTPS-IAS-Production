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
        internal ExecutiveDashboardScope ResolveAuditParaSnapScope()
        {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();
            // Do not allow environment role mappings to create another bank-wide role.
            if (user.UserRoleID != 3 && !(user.UserRoleID == 1 && session.IsSuperUser()))
                throw new ExecutiveDashboardAccessException("RoleMapping", "Audit Para Snap requires CIA or Super User.");
            var cia = ResolveExecutiveDashboardScope("CIA");
            return new ExecutiveDashboardScope(user, "CIA", AuditParaSnapPolicy.PagePath, cia.EntityId);
        }
        private void ValidateSnapScope(ExecutiveDashboardScope scope)
        {
            var user = ValidateExecutiveDashboardSession(scope);
            if (scope.DashboardType != "CIA" || scope.PermissionPagePath != AuditParaSnapPolicy.PagePath ||
                (user.UserRoleID != 3 && !(user.UserRoleID == 1 && CreateSessionHandler().IsSuperUser())))
                throw new ExecutiveDashboardAccessException("RoleMapping", "A verified Audit Para Snap scope is required.");
        }
        internal List<ExecutiveFilterOption> GetAuditParaSnapDepartments(
            ExecutiveDashboardScope scope, string divisionCode, int? selectedDepartmentId = null)
        {
            ValidateSnapScope(scope);
            if (divisionCode == "1")
            {
                if (selectedDepartmentId.HasValue) throw new ArgumentException("All Divisions does not accept a Department / Cluster.");
                return new List<ExecutiveFilterOption>();
            }
            if (divisionCode != "FAD" && divisionCode != "MAD")
                throw new ArgumentException("Invalid audit division.");
            using var conn = DatabaseConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PKG_AUDIT_SNAP_SCOPE.P_GET_CHILDREN";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            GuardAgainstDynamicSql(cmd);
            cmd.Parameters.Add("P_DIVISION_CODE", OracleDbType.Varchar2).Value = divisionCode;
            cmd.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            var items = new List<ExecutiveFilterOption>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = checked((int)ExecutiveCount(reader,"ID"));
                if (id <= 0) throw new InvalidOperationException("Invalid IAS audited-by ID.");
                items.Add(new ExecutiveFilterOption { Id = id, Name = ExecutiveText(reader,"NAME") });
            }
            ValidateSnapScope(scope);
            if (items.Count == 0 || items.Select(e=>e.Id).Distinct().Count() != items.Count)
                throw new InvalidOperationException("Audit division has no valid distinct mapped departments.");
            if (selectedDepartmentId.HasValue && !items.Any(e=>e.Id==selectedDepartmentId.Value))
                throw new ExecutiveDashboardAccessException("EntityScope",
                    "Selected Department / Cluster does not belong to the chosen Division.");
            return items;
        }
        internal ExecutiveKpiLifecycle GetAuditParaSnapKpis(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            ValidateSnapScope(scope);
            AuditParaSnapPolicy.Validate(filters);
            var result = ReadExecutiveKpiLifecycle(scope, filters);
            ValidateSnapScope(scope);
            return result;
        }
        private List<T> ReadSnap<T>(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters,
            string procedure, Func<OracleDataReader,T> map, int? maxRows = null)
        {
            ValidateDatedDashboardScope(scope, filters);
            var dates = filters.Validate();
            if (maxRows.HasValue && (maxRows < 1 || maxRows > 250))
                throw new ArgumentException("Critical item limit must be 1..250.");
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandText = procedure;
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("P_DASHBOARD_TYPE",OracleDbType.Varchar2).Value = scope.DashboardType;
            command.Parameters.Add("P_ENTITY_ID",OracleDbType.Int32).Value = (object)scope.EntityId ?? DBNull.Value;
            command.Parameters.Add("P_START_DATE",OracleDbType.Date).Value = dates.StartDate;
            command.Parameters.Add("P_END_DATE",OracleDbType.Date).Value = dates.EndDate;
            command.Parameters.Add("P_ENTITY_TYPE_ID",OracleDbType.Int32).Value =
                filters.SelectBy == "entityType" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
            command.Parameters.Add("P_AUDITED_BY_ID",OracleDbType.Int32).Value =
                (object)(scope.DashboardType == "CCO" ? (filters.SelectBy == "auditedBy" ? filters.DimensionId : null) : filters.SnapDepartmentId) ?? DBNull.Value;
            command.Parameters.Add("P_REPORTING_OFFICE_ID",OracleDbType.Int32).Value = (object)filters.ReportingOfficeId ?? DBNull.Value;
            if (maxRows.HasValue) command.Parameters.Add("P_MAX_ROWS",OracleDbType.Int32).Value = maxRows.Value;
            command.Parameters.Add("IO_CURSOR",OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            command.Parameters.Add("P_SNAP_DIVISION_CODE",OracleDbType.Varchar2).Value = (object)filters.SnapDivision ?? DBNull.Value;
            var rows = new List<T>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (rows.Count >= (maxRows ?? 1213)) throw new InvalidOperationException("Snap cursor exceeded its bound.");
                rows.Add(map(reader));
            }
            ValidateDatedDashboardScope(scope, filters);
            return rows;
        }
        internal List<AuditParaSnapBucket> GetAuditParaSnapDistribution(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
            => ReadSnap(scope,filters,"PKG_EXEC_DASHBOARD.P_GET_SNAP_DISTRIBUTION",r => new AuditParaSnapBucket
            {
                Dataset=ExecutiveText(r,"DATASET"),BucketCode=ExecutiveText(r,"BUCKET_CODE"),
                BucketName=ExecutiveText(r,"BUCKET_NAME"),ParaCount=ExecutiveNullableCount(r,"PARA_COUNT"),
                HighRiskCount=ExecutiveNullableCount(r,"HIGH_RISK_COUNT"),
                QualityCode=ExecutiveText(r,"QUALITY_CODE")
            });
        internal List<AuditParaSnapCritical> GetAuditParaSnapCritical(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters, int maxRows)
            => ReadSnap(scope,filters,"PKG_EXEC_DASHBOARD.P_GET_SNAP_CRITICAL",r => new AuditParaSnapCritical
            {
                ComId=ExecutiveCount(r,"COM_ID"),EntityId=OrgNumber(r,"ENTITY_ID"),EntityName=ExecutiveText(r,"ENTITY_NAME"),
                ParaNo=ExecutiveText(r,"PARA_NO"),GistOfParas=ExecutiveText(r,"GIST_OF_PARAS"),
                RiskName=ExecutiveText(r,"RISK_NAME"),AgingDays=OrgNumber(r,"AGING_DAYS"),
                ComCycle=OrgNumber(r,"COM_CYCLE"),QualityCode=ExecutiveText(r,"QUALITY_CODE"),
                AuditPeriod=ExecutiveText(r,"AUDIT_PERIOD"), ParaAddedOn=RoleDate(r,"PARA_ADDED_ON"),
                DaysSinceLastMovement=OrgNumber(r,"DAYS_SINCE_MOVEMENT")
            },maxRows);
    }
}
