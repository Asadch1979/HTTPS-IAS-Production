using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

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
            ValidateSnapScope(scope);
            var dates = AuditParaSnapPolicy.Validate(filters);
            if (maxRows.HasValue && (maxRows < 1 || maxRows > 250))
                throw new ArgumentException("Critical item limit must be 1..250.");
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandText = procedure;
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("P_DASHBOARD_TYPE",OracleDbType.Varchar2).Value = "CIA";
            command.Parameters.Add("P_ENTITY_ID",OracleDbType.Int32).Value = (object)scope.EntityId ?? DBNull.Value;
            command.Parameters.Add("P_START_DATE",OracleDbType.Date).Value = dates.StartDate;
            command.Parameters.Add("P_END_DATE",OracleDbType.Date).Value = dates.EndDate;
            command.Parameters.Add("P_ENTITY_TYPE_ID",OracleDbType.Int32).Value =
                filters.SelectBy == "entityType" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
            command.Parameters.Add("P_AUDITED_BY_ID",OracleDbType.Int32).Value =
                filters.SelectBy == "auditedBy" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
            command.Parameters.Add("P_REPORTING_OFFICE_ID",OracleDbType.Int32).Value = (object)filters.ReportingOfficeId ?? DBNull.Value;
            if (maxRows.HasValue) command.Parameters.Add("P_MAX_ROWS",OracleDbType.Int32).Value = maxRows.Value;
            command.Parameters.Add("IO_CURSOR",OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            var rows = new List<T>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (rows.Count >= (maxRows ?? 1213)) throw new InvalidOperationException("Snap cursor exceeded its bound.");
                rows.Add(map(reader));
            }
            ValidateSnapScope(scope);
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
                ComCycle=OrgNumber(r,"COM_CYCLE"),QualityCode=ExecutiveText(r,"QUALITY_CODE")
            },maxRows);
    }
}
