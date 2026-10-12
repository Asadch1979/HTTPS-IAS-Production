using AIS.Models;
using AIS.Session;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        internal ExecutiveDashboardScope ResolveExecutiveDashboardScope()
        {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();
            if (session.IsSessionRevoked(user)) throw new UnauthenticatedException();
            var mappings = _configuration.GetSection("ExecutiveDashboard:RoleMappings")
                .Get<List<ExecutiveDashboardRoleMapping>>() ?? new List<ExecutiveDashboardRoleMapping>();
            var matches = mappings.Where(mapping => mapping.RoleId > 0 && mapping.RoleId == user.UserRoleID).ToList();
            if (matches.Count != 1)
                throw new UnauthorizedAccessException("A unique verified executive role mapping is required.");
            var mapping = matches[0];
            var type = mapping.DashboardType?.Trim().ToUpperInvariant();
            if (type != "TOP" && type != "GROUP" && type != "CCO" && type != "CIA")
                throw new UnauthorizedAccessException("The executive role mapping is not supported.");
            if (string.IsNullOrWhiteSpace(mapping.PagePath) || !mapping.PagePath.StartsWith("/", StringComparison.Ordinal))
                throw new UnauthorizedAccessException("An existing executive permission page mapping is required.");
            return new ExecutiveDashboardScope(user, type, mapping.PagePath.Trim(), type == "GROUP" ? user.UserEntityID : null);
        }

        internal ExecutiveDashboardScope NarrowExecutiveDashboardScope(ExecutiveDashboardScope scope, int? entityId)
        {
            var user = ValidateExecutiveDashboardSession(scope);
            if (!entityId.HasValue) return scope;
            if (entityId.Value <= 0) throw new ArgumentException("Entity ID must be positive.");
            // TOP/CCO ignore P_ENTITY_ID, including the CCO cycle query. Never
            // describe a bank-wide result as a filtered result or change its mode.
            if (scope.DashboardType != "GROUP")
                throw new ArgumentException("PKG_EXEC_DASHBOARD does not support entity filters for bank-wide dashboards.");
            if (entityId != scope.SessionEntityId &&
                !GetOrganizationNodes(scope.SessionEntityId).Any(node => node.EntityId == entityId.Value))
                throw new UnauthorizedAccessException("The requested entity is outside the authenticated reporting hierarchy.");
            return new ExecutiveDashboardScope(user, scope.DashboardType, scope.PermissionPagePath, entityId);
        }

        internal ExecutiveKpiSummary GetExecutiveKpiSummary(ExecutiveDashboardScope scope, string auditPeriod)
        {
            var rows = ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_KPI_SUMMARY", scope, auditPeriod,
                reader => new ExecutiveKpiSummary
                {
                    TotalCases = ExecutiveCount(reader, "TOTAL_CASES"),
                    OpenParas = ExecutiveCount(reader, "OPEN_PARAS"),
                    HighRiskParas = ExecutiveCount(reader, "HIGH_RISK_PARAS"),
                    ZeroCycleParas = ExecutiveCount(reader, "ZERO_CYCLE_PARAS"),
                    Over3CycleParas = ExecutiveCount(reader, "OVER_3_CYCLE_PARAS"),
                    Over1YearParas = ExecutiveCount(reader, "OVER_1_YEAR_PARAS"),
                    CriticalItems = ExecutiveCount(reader, "CRITICAL_ITEMS"),
                    SettledTotal = ExecutiveCount(reader, "SETTLED_TOTAL"),
                    SettledMtd = ExecutiveCount(reader, "SETTLED_MTD"),
                    SettlementRatio = ExecutiveDecimal(reader, "SETTLEMENT_RATIO")
                });
            if (rows.Count != 1) throw new InvalidOperationException("Executive KPI cursor must contain one summary row.");
            return rows[0];
        }

        internal List<ExecutiveRiskSummary> GetExecutiveRiskSummary(ExecutiveDashboardScope scope, string auditPeriod)
            => ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_RISK_SUMMARY", scope, auditPeriod,
                reader => new ExecutiveRiskSummary
                {
                    RiskId = reader.IsDBNull(reader.GetOrdinal("RISK_ID")) ? (int?)null : checked((int)ExecutiveDecimal(reader, "RISK_ID")),
                    RiskName = ExecutiveText(reader, "RISK_NAME"),
                    ParaCount = ExecutiveCount(reader, "PARA_COUNT"),
                    Percentage = ExecutiveDecimal(reader, "PERCENTAGE")
                });

        internal List<ExecutiveAgingSummary> GetExecutiveAgingSummary(ExecutiveDashboardScope scope, string auditPeriod)
            => ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_AGING_SUMMARY", scope, auditPeriod,
                reader => new ExecutiveAgingSummary
                {
                    BucketCode = ExecutiveText(reader, "BUCKET_CODE"),
                    BucketName = ExecutiveText(reader, "BUCKET_NAME"),
                    ParaCount = ExecutiveCount(reader, "PARA_COUNT"),
                    HighRiskCount = ExecutiveCount(reader, "HIGH_RISK_COUNT")
                });

        internal List<ExecutiveCycleSummary> GetExecutiveCycleSummary(ExecutiveDashboardScope scope, string auditPeriod)
            => ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_CYCLE_SUMMARY", scope, auditPeriod,
                reader => new ExecutiveCycleSummary
                {
                    BucketCode = ExecutiveText(reader, "BUCKET_CODE"),
                    BucketName = ExecutiveText(reader, "BUCKET_NAME"),
                    ParaCount = ExecutiveCount(reader, "PARA_COUNT"),
                    HighRiskCount = ExecutiveCount(reader, "HIGH_RISK_COUNT")
                });

        private SessionUser ValidateExecutiveDashboardSession(ExecutiveDashboardScope scope)
        {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();
            if (session.IsSessionRevoked(user) || scope == null || user.ID != scope.UserId ||
                user.UserRoleID != scope.RoleId || user.UserEntityID != scope.SessionEntityId)
                throw new UnauthenticatedException();
            return user;
        }

        private List<T> ReadExecutiveSummary<T>(string procedure, ExecutiveDashboardScope scope, string auditPeriod, Func<OracleDataReader, T> map)
        {
            ValidateExecutiveDashboardSession(scope);
            if (scope.DashboardType == "CIA")
                throw new NotSupportedException("PKG_EXEC_DASHBOARD does not support CIA reporting.");
            var rows = new List<T>();
            using (var connection = DatabaseConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = procedure;
                command.CommandType = CommandType.StoredProcedure;
                command.BindByName = true;
                GuardAgainstDynamicSql(command);
                command.Parameters.Add("P_DASHBOARD_TYPE", OracleDbType.Varchar2).Value = scope.DashboardType;
                command.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = (object)scope.EntityId ?? DBNull.Value;
                command.Parameters.Add("P_AUDIT_PERIOD", OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(auditPeriod) ? (object)DBNull.Value : auditPeriod.Trim();
                command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) rows.Add(map(reader));
            }
            return rows;
        }

        private static decimal ExecutiveDecimal(OracleDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
        }

        private static long ExecutiveCount(OracleDataReader reader, string column)
            => checked((long)ExecutiveDecimal(reader, column));

        private static string ExecutiveText(OracleDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }
    }
}
