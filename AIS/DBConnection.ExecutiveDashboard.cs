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
        internal ExecutiveDashboardScope ResolveExecutiveDashboardScope(string requestedType = null, int? groupRootId = null,
            bool allowGroupSelection = false)
        {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();
            if (session.IsSessionRevoked(user)) throw new UnauthenticatedException();
            var requested = requestedType?.Trim().ToUpperInvariant();
            // Reporting Line Management never accepts a browser-supplied root.
            // It always starts at the entity assigned to the authenticated user.
            if (requested == "RLM")
            {
                if (groupRootId.HasValue || user.UserEntityID.GetValueOrDefault() <= 0 ||
                    !GetOrganizationNodes(user.UserEntityID).Any(n => n.EntityId == user.UserEntityID))
                    throw new ExecutiveDashboardAccessException("EntityScope", "Reporting Line Management requires the authenticated entity.");
                return new ExecutiveDashboardScope(user, "RLM", "/Dashboard/entity_wise_obs", user.UserEntityID);
            }
            if (user.UserRoleID == 1 && session.IsSuperUser())
            {
                if (!IsExecutiveDashboardType(requested))
                    throw new ExecutiveDashboardAccessException("RoleMapping", "Super User must request TOP, GROUP, CCO or CIA.");
                if (requested != "GROUP" && groupRootId.HasValue)
                    throw new ExecutiveDashboardAccessException("EntityScope", "A Group root is valid only for GROUP.");
                if (requested == "GROUP")
                {
                    if (!groupRootId.HasValue && !allowGroupSelection)
                        throw new ExecutiveDashboardAccessException("EntityScope", "Select a valid Group Head entity before loading GROUP data.");
                    if (groupRootId.HasValue) RequireExecutiveGroupRoot(groupRootId.Value);
                }
                // Perspective is independent of the actual IAS role and posting.
                return new ExecutiveDashboardScope(user, requested, "/ExecutiveOverview/" + requested,
                    requested == "GROUP" ? groupRootId : null, requested == "GROUP" ? groupRootId : null);
            }
            if (user.UserRoleID == 1)
                throw new ExecutiveDashboardAccessException("SessionValidation", "The Super User session flag does not match the active IAS role.");
            var mappings = _configuration.GetSection("ExecutiveDashboard:RoleMappings")
                .Get<List<ExecutiveDashboardRoleMapping>>() ?? new List<ExecutiveDashboardRoleMapping>();
            var matches = mappings.Where(mapping => mapping != null && mapping.RoleId > 0 && mapping.RoleId == user.UserRoleID).ToList();
            if (matches.Count != 1)
                throw new ExecutiveDashboardAccessException("RoleMapping", "A unique verified executive role mapping is required.");
            var mapping = matches[0];
            var type = mapping.DashboardType?.Trim().ToUpperInvariant();
            if (!IsExecutiveDashboardType(type))
                throw new ExecutiveDashboardAccessException("RoleMapping", "The executive role mapping is not supported.");
            if (requested != null && requested != type)
                throw new ExecutiveDashboardAccessException("RoleMapping", "The requested perspective does not match the authenticated IAS role.");
            if (groupRootId.HasValue && (type != "GROUP" || groupRootId != user.UserEntityID))
                throw new ExecutiveDashboardAccessException("EntityScope", "An ordinary user cannot override the authenticated Group root.");
            if (!string.Equals(mapping.PagePath, "/ExecutiveOverview/" + type, StringComparison.Ordinal))
                throw new ExecutiveDashboardAccessException("RoleMapping", "A dedicated role permission page is required.");
            if (mappings.Where(m => m != null && m.RoleId > 0).GroupBy(m => m.PagePath).Any(g => g.Count() > 1))
                throw new ExecutiveDashboardAccessException("RoleMapping", "Executive permission pages must be separately mapped.");
            if (type == "GROUP" && (user.UserEntityID.GetValueOrDefault() <= 0 || mapping.AllowBankWide))
                throw new ExecutiveDashboardAccessException("EntityScope", "GROUP requires an active entity and restricted scope.");
            if (type != "GROUP" && !mapping.AllowBankWide)
                throw new ExecutiveDashboardAccessException("RoleMapping", "Bank-wide access requires explicit configuration approval.");
            if ((type == "CIA" && mapping.RoleId != 3) || (type == "CCO" && mapping.RoleId != 38) ||
                (type != "CIA" && mapping.RoleId == 3) || (type != "CCO" && mapping.RoleId == 38))
                throw new ExecutiveDashboardAccessException("RoleMapping", "The verified IAS role does not match the dashboard type.");
            if (type == "GROUP") RequireExecutiveGroupRoot(user.UserEntityID.Value);
            return new ExecutiveDashboardScope(user, type, mapping.PagePath.Trim(), type == "GROUP" ? user.UserEntityID : null,
                type == "GROUP" ? user.UserEntityID : null);
        }

        private static bool IsExecutiveDashboardType(string type)
            => type == "TOP" || type == "GROUP" || type == "CCO" || type == "CIA";

        private void RequireExecutiveGroupRoot(int rootId)
        {
            if (rootId <= 0 || !GetOrganizationNodes(rootId).Any(n => n.EntityId == rootId && n.EntityTypeId == 18 && !n.HasTypeConflict))
                throw new ExecutiveDashboardAccessException("EntityScope", "GROUP requires a valid Group Head entity (category 18).");
        }

        internal List<OrganizationNode> GetExecutiveDashboardGroups(ExecutiveDashboardScope scope)
        {
            var user = ValidateExecutiveDashboardSession(scope);
            if (user.UserRoleID != 1 || !CreateSessionHandler().IsSuperUser())
                throw new ExecutiveDashboardAccessException("RoleMapping", "Only Super User may select a Group root.");
            return GetOrganizationNodes(null).Where(n => n.EntityTypeId == 18 && !n.HasTypeConflict).ToList();
        }

        private static void RequireExecutiveGroupScope(ExecutiveDashboardScope scope)
        {
            if (scope.DashboardType == "GROUP" && (scope.GroupRootId.GetValueOrDefault() <= 0 || scope.EntityId.GetValueOrDefault() <= 0))
                throw new ExecutiveDashboardAccessException("EntityScope", "GROUP data always requires a selected Group root.");
        }

        internal ExecutiveDashboardScope NarrowExecutiveDashboardScope(ExecutiveDashboardScope scope, int? entityId)
        {
            var user = ValidateExecutiveDashboardSession(scope);
            RequireExecutiveGroupScope(scope);
            if (!entityId.HasValue) return scope;
            if (entityId.Value <= 0) throw new ArgumentException("Entity ID must be positive.");
            if (!GetExecutiveDashboardEntities(scope).Any(node => node.EntityId == entityId.Value))
                throw new ExecutiveDashboardAccessException("EntityScope", "The requested entity is outside the authenticated reporting hierarchy.");
            return new ExecutiveDashboardScope(user, scope.DashboardType, scope.PermissionPagePath, entityId, scope.GroupRootId);
        }

        internal List<OrganizationNode> GetExecutiveDashboardEntities(ExecutiveDashboardScope scope)
        {
            ValidateExecutiveDashboardSession(scope);
            RequireExecutiveGroupScope(scope);
            var nodes = GetOrganizationNodes(scope.DashboardType == "GROUP" ? scope.GroupRootId :
                scope.DashboardType == "RLM" ? scope.SessionEntityId : null);
            return scope.DashboardType != "GROUP" ? nodes : nodes.Where(n => n.EntityId == scope.GroupRootId ||
                n.Reporting1 == scope.GroupRootId || n.Reporting2 == scope.GroupRootId ||
                n.Reporting3 == scope.GroupRootId || n.Reporting4 == scope.GroupRootId ||
                n.Reporting5 == scope.GroupRootId || n.Reporting6 == scope.GroupRootId).ToList();
        }

        internal List<ExecutiveSettlementTrend> GetExecutiveSettlementTrend(ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters = null)
            => ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_SETTLEMENT_TREND", scope, auditPeriod,
                reader => new ExecutiveSettlementTrend {
                    SettlementMonth = ExecutiveText(reader, "SETTLEMENT_MONTH"),
                    SettledParas = ExecutiveCount(reader, "SETTLED_PARAS"),
                    HighRiskSettled = ExecutiveCount(reader, "HIGH_RISK_SETTLED") }, filters: filters);

        internal List<ExecutiveCriticalItem> GetExecutiveCriticalItems(ExecutiveDashboardScope scope, string auditPeriod, int maxRows, ExecutiveDashboardFilterRequest filters = null)
        {
            if (maxRows < 1 || maxRows > 500) throw new ArgumentException("Maximum rows must be between 1 and 500.");
            return ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_CRITICAL_ITEMS", scope, auditPeriod,
                reader => new ExecutiveCriticalItem {
                    ComId = ExecutiveCount(reader, "COM_ID"),
                    EntityId = OrgNumber(reader, "ENTITY_ID"), EntityName = ExecutiveText(reader, "ENTITY_NAME"),
                    DivisionId = OrgNumber(reader, "DIVISION_ID"), DivisionName = ExecutiveText(reader, "DIVISION_NAME"),
                    BusinessGroupId = OrgNumber(reader, "BUSINESS_GROUP_ID"), BusinessGroupName = ExecutiveText(reader, "BUSINESS_GROUP_NAME"),
                    AuditPeriod = ExecutiveText(reader, "AUDIT_PERIOD"), ParaNo = Convert.ToString(reader["PARA_NO"]),
                    GistOfParas = ExecutiveText(reader, "GIST_OF_PARAS"), RiskId = OrgNumber(reader, "RISK_ID"),
                    RiskName = ExecutiveText(reader, "RISK_NAME"), ComCycle = OrgNumber(reader, "COM_CYCLE"),
                    ParaAddedOn = reader.IsDBNull(reader.GetOrdinal("PARA_ADDED_ON")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("PARA_ADDED_ON")),
                    AgingDays = OrgNumber(reader, "AGING_DAYS"), DaysSinceLastMovement = OrgNumber(reader, "DAYS_SINCE_LAST_MOVEMENT")
                }, maxRows, filters);
        }

        internal ExecutiveKpiSummary GetExecutiveKpiSummary(ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters = null)
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
                }, filters: filters);
            if (rows.Count != 1) throw new InvalidOperationException("Executive KPI cursor must contain one summary row.");
            return rows[0];
        }

        internal List<ExecutiveRiskSummary> GetExecutiveRiskSummary(ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters = null)
            => ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_RISK_SUMMARY", scope, auditPeriod,
                reader => new ExecutiveRiskSummary
                {
                    RiskId = reader.IsDBNull(reader.GetOrdinal("RISK_ID")) ? (int?)null : checked((int)ExecutiveDecimal(reader, "RISK_ID")),
                    RiskName = ExecutiveText(reader, "RISK_NAME"),
                    ParaCount = ExecutiveCount(reader, "PARA_COUNT"),
                    Percentage = ExecutiveDecimal(reader, "PERCENTAGE")
                }, filters: filters);

        internal List<ExecutiveAgingSummary> GetExecutiveAgingSummary(ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters = null)
            => ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_AGING_SUMMARY", scope, auditPeriod,
                reader => new ExecutiveAgingSummary
                {
                    BucketCode = ExecutiveText(reader, "BUCKET_CODE"),
                    BucketName = ExecutiveText(reader, "BUCKET_NAME"),
                    ParaCount = ExecutiveCount(reader, "PARA_COUNT"),
                    HighRiskCount = ExecutiveCount(reader, "HIGH_RISK_COUNT")
                }, filters: filters);

        internal List<ExecutiveCycleSummary> GetExecutiveCycleSummary(ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters = null)
            => ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_CYCLE_SUMMARY", scope, auditPeriod,
                reader => new ExecutiveCycleSummary
                {
                    BucketCode = ExecutiveText(reader, "BUCKET_CODE"),
                    BucketName = ExecutiveText(reader, "BUCKET_NAME"),
                    ParaCount = ExecutiveCount(reader, "PARA_COUNT"),
                    HighRiskCount = ExecutiveCount(reader, "HIGH_RISK_COUNT")
                }, filters: filters);

        private SessionUser ValidateExecutiveDashboardSession(ExecutiveDashboardScope scope)
        {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();
            if (session.IsSessionRevoked(user) || scope == null || user.ID != scope.UserId ||
                user.UserRoleID != scope.RoleId || user.UserEntityID != scope.SessionEntityId ||
                user.SessionId != scope.SessionId || user.UserContextAssignmentId != scope.ContextAssignmentId)
                throw new UnauthenticatedException();
            return user;
        }

        private List<T> ReadExecutiveSummary<T>(string procedure, ExecutiveDashboardScope scope, string auditPeriod, Func<OracleDataReader, T> map, int? maxRows = null, ExecutiveDashboardFilterRequest filters = null)
        {
            ValidateExecutiveDashboardSession(scope);
            RequireExecutiveGroupScope(scope);
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
                if (maxRows.HasValue) command.Parameters.Add("P_MAX_ROWS", OracleDbType.Int32).Value = maxRows.Value;
                command.Parameters.Add("P_ENTITY_TYPE_ID", OracleDbType.Int32).Value =
                    filters?.SelectBy == "entityType" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
                command.Parameters.Add("P_AUDITED_BY_ID", OracleDbType.Int32).Value =
                    filters?.SelectBy == "auditedBy" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
                command.Parameters.Add("P_REPORTING_OFFICE_ID", OracleDbType.Int32).Value =
                    (object)filters?.ReportingOfficeId ?? DBNull.Value;
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
