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
            if (user.UserRoleID <= 0 || (user.UserRoleID == 1 && !session.IsSuperUser()))
                throw new ExecutiveDashboardAccessException("RoleMapping", "An active verified IAS role is required.");
            var requested = requestedType?.Trim().ToUpperInvariant();
            // Reporting Line Management never accepts a browser-supplied root.
            // It always starts at the entity assigned to the authenticated user.
            if (requested == "RLM")
            {
                // Explicit Super User preview is bounded to an approved
                // organizational reporting root. Never impersonate an IAS user.
                if (user.UserRoleID == 1 && session.IsSuperUser())
                {
                    if (groupRootId.HasValue)
                    {
                        var roots = GetOrganizationNodes(groupRootId.Value).Where(n =>
                            n.EntityId == groupRootId.Value && !n.HasTypeConflict &&
                            (n.EntityTypeId == 3 || n.EntityTypeId == 18 ||
                             n.EntityTypeId == 5 || n.EntityTypeId == 21)).ToList();
                        if (roots.Count != 1)
                            throw new ExecutiveDashboardAccessException("EntityScope", "Super User must select one verified Group, Division, GM or Region root.");
                    }
                    return new ExecutiveDashboardScope(user, "RLM", "/ExecutiveOverview/RLM",
                        groupRootId, groupRootId);
                }
                if (groupRootId.HasValue || user.UserEntityID.GetValueOrDefault() <= 0 ||
                    !GetOrganizationNodes(user.UserEntityID).Any(n => n.EntityId == user.UserEntityID && !n.HasTypeConflict && n.EntityTypeId.HasValue))
                    throw new ExecutiveDashboardAccessException("EntityScope", "Reporting Line Management requires the authenticated entity.");
                return new ExecutiveDashboardScope(user, "RLM", "/ExecutiveOverview/RLM", user.UserEntityID);
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

            // CIA and CCO have verified, fixed IAS role IDs. Resolve the perspective
            // from the authenticated session, not from environment-specific appsettings.
            // The MVC page and each API still require their normal database-backed grants.
            if (user.UserRoleID == 3 || user.UserRoleID == 38)
            {
                var verifiedType = user.UserRoleID == 3 ? "CIA" : "CCO";
                if (requested != null && requested != verifiedType)
                    throw new ExecutiveDashboardAccessException("RoleMapping", "The requested executive perspective does not match the authenticated IAS role.");
                if (groupRootId.HasValue)
                    throw new ExecutiveDashboardAccessException("EntityScope", "CIA and CCO do not accept a client-supplied Group root.");

                return new ExecutiveDashboardScope(user, verifiedType, "/ExecutiveOverview/" + verifiedType, null);
            }

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
            if (groupRootId.HasValue)
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

        internal OrganizationNode GetExecutiveDashboardRoot(ExecutiveDashboardScope scope)
        {
            ValidateExecutiveDashboardSession(scope);
            if (scope.DashboardType != "RLM" && scope.DashboardType != "GROUP") return null;
            var rootId = scope.DashboardType == "GROUP" ||
                (scope.DashboardType == "RLM" && scope.RoleId == 1)
                    ? scope.GroupRootId : scope.SessionEntityId;
            if (!rootId.HasValue) return null;
            return GetOrganizationNodes(rootId).SingleOrDefault(n => n.EntityId == rootId && !n.HasTypeConflict);
        }

        internal List<OrganizationNode> GetExecutiveComparisonEntities(ExecutiveDashboardScope scope,
            ExecutiveDashboardFilterRequest filters, ExecutiveDashboardFilterOptions options, bool branches)
        {
            ValidateExecutiveDashboardSession(scope);
            RequireExecutiveGroupScope(scope);
            var rootId = filters.EntityId ?? filters.ReportingOfficeId ?? scope.EntityId;
            if (!rootId.HasValue) return new List<OrganizationNode>();
            NarrowExecutiveDashboardScope(scope, rootId);
            var allowed = options.Entities.Select(e => e.Id).ToHashSet();
            return GetOrganizationNodes(rootId).Where(n => !n.HasTypeConflict && allowed.Contains(n.EntityId) &&
                (branches ? n.EntityTypeId == 6 || n.EntityTypeId == 28 : n.ParentEntityId == rootId))
                .OrderBy(n => n.EntityName).ToList();
        }

        private void RequireExecutiveGroupRoot(int rootId)
        {
            if (rootId <= 0 || !GetOrganizationNodes(rootId).Any(n => n.EntityId == rootId && n.EntityTypeId == 18 && !n.HasTypeConflict))
                throw new ExecutiveDashboardAccessException("EntityScope", "GROUP requires a valid Group Head entity (category 18).");
        }

        internal List<OrganizationNode> GetExecutiveDashboardRlmRoots(ExecutiveDashboardScope scope)
        {
            var user = ValidateExecutiveDashboardSession(scope);
            if (scope.DashboardType != "RLM" || user.UserRoleID != 1 ||
                !CreateSessionHandler().IsSuperUser())
                throw new ExecutiveDashboardAccessException("RoleMapping", "Super User selection only.");
            return GetOrganizationNodes(null).Where(n => !n.HasTypeConflict &&
                (n.EntityTypeId == 3 || n.EntityTypeId == 18 ||
                 n.EntityTypeId == 5 || n.EntityTypeId == 21))
                .GroupBy(n => n.EntityId).Where(g => g.Count() == 1)
                .Select(g => g.Single()).OrderBy(n => n.EntityName).ToList();
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
            if (scope.DashboardType == "RLM" && scope.RoleId == 1 &&
                (scope.GroupRootId.GetValueOrDefault() <= 0 || scope.EntityId.GetValueOrDefault() <= 0))
                throw new ExecutiveDashboardAccessException("EntityScope", "Super User RLM data requires a selected verified organizational root.");
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
            var root = scope.DashboardType == "GROUP" ||
                (scope.DashboardType == "RLM" && scope.RoleId == 1)
                    ? scope.GroupRootId : scope.SessionEntityId;
            // Fail closed: Super User's RLM selector without a root must NOT
            // become a bank-wide, null-root query.
            if (scope.DashboardType == "RLM" && !root.HasValue)
                return new List<OrganizationNode>();
            var nodes = GetOrganizationNodes(scope.DashboardType == "GROUP" ||
                scope.DashboardType == "RLM" ? root : null);
            return scope.DashboardType != "GROUP" && scope.DashboardType != "RLM" ? nodes : nodes.Where(n => (scope.DashboardType != "RLM" || !n.HasTypeConflict) &&
                (n.EntityId == root || n.Reporting1 == root || n.Reporting2 == root ||
                 n.Reporting3 == root || n.Reporting4 == root || n.Reporting5 == root || n.Reporting6 == root)).ToList();
        }
        internal List<ExecutiveSettlementTrend> GetExecutiveSettlementTrend(ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters = null)
            => scope.DashboardType == "CCO" ? ComplianceDistribution(scope, filters).Where(b => b.Dataset == "trend")
                .Select(b => new ExecutiveSettlementTrend { SettlementMonth = b.BucketCode, SettledParas = b.ParaCount ?? 0,
                    HighRiskSettled = b.HighRiskCount ?? 0 }).ToList()
            : ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_SETTLEMENT_TREND", scope, auditPeriod,
                reader => new ExecutiveSettlementTrend {
                    SettlementMonth = ExecutiveText(reader, "SETTLEMENT_MONTH"),
                    SettledParas = ExecutiveCount(reader, "SETTLED_PARAS"),
                    HighRiskSettled = ExecutiveCount(reader, "HIGH_RISK_SETTLED") }, filters: filters);

        internal List<ExecutiveCriticalItem> GetExecutiveCriticalItems(ExecutiveDashboardScope scope, string auditPeriod, int maxRows, ExecutiveDashboardFilterRequest filters = null)
        {
            if (maxRows < 1 || maxRows > 500) throw new ArgumentException("Maximum rows must be between 1 and 500.");
            if (scope.DashboardType == "CCO") return ComplianceCritical(scope, filters, maxRows);
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
            if (scope.DashboardType == "CCO") return ComplianceKpis(scope, filters);
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
                    SettlementRatio = ExecutiveDecimal(reader, "SETTLEMENT_RATIO"),
                    SettlementMtdRate = reader.IsDBNull(reader.GetOrdinal("SETTLEMENT_MTD_RATE"))
                        ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("SETTLEMENT_MTD_RATE"))
                }, filters: filters);
            if (rows.Count != 1) throw new InvalidOperationException("Executive KPI cursor must contain one summary row.");
            var summary = rows[0];
            // CIA issue events use a separate authorized, independently
            // date-filtered procedure; current-state para KPIs are unchanged.
            if (scope.DashboardType == "CIA" && filters?.View == "glance")
            {
                var issueCounts = GetExecutiveCiaIssues(scope, filters);
                summary.IssueIdentified = issueCounts.identified;
                summary.IssueRectified = issueCounts.rectified;
            }
            return summary;
        }

        private (long identified, long rectified) GetExecutiveCiaIssues(
            ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            ValidateExecutiveDashboardSession(scope);
            if (scope.DashboardType != "CIA")
                throw new ExecutiveDashboardAccessException("RoleMapping", "Issue counts require CIA scope.");
            var dates = filters.Validate(); // null = all eligible issue history
            using (var connection = DatabaseConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUES";
                command.CommandType = CommandType.StoredProcedure;
                command.BindByName = true;
                GuardAgainstDynamicSql(command);
                command.Parameters.Add("P_DASHBOARD_TYPE", OracleDbType.Varchar2).Value = scope.DashboardType;
                command.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = (object)scope.EntityId ?? DBNull.Value;
                command.Parameters.Add("P_START_DATE", OracleDbType.Date).Value =
                    dates == null ? (object)DBNull.Value : dates.StartDate;
                command.Parameters.Add("P_END_DATE", OracleDbType.Date).Value =
                    dates == null ? (object)DBNull.Value : dates.EndDate;
                command.Parameters.Add("P_ENTITY_TYPE_ID", OracleDbType.Int32).Value =
                    filters.SelectBy == "entityType" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
                command.Parameters.Add("P_AUDITED_BY_ID", OracleDbType.Int32).Value =
                    filters.SelectBy == "auditedBy" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
                command.Parameters.Add("P_REPORTING_OFFICE_ID", OracleDbType.Int32).Value =
                    (object)filters.ReportingOfficeId ?? DBNull.Value;
                command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) throw new InvalidOperationException("CIA issue cursor must contain one row.");
                    var identified = ExecutiveCount(reader, "ISSUE_IDENTIFIED");
                    var rectified = ExecutiveCount(reader, "ISSUE_RECTIFIED");
                    if (reader.Read()) throw new InvalidOperationException("CIA issue cursor returned multiple rows.");
                    return (identified, rectified);
                }
            }
        }

        internal List<ExecutiveRiskSummary> GetExecutiveRiskSummary(ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters = null)
            => scope.DashboardType == "CCO" ? ComplianceRisk(scope, filters) : ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_RISK_SUMMARY", scope, auditPeriod,
                reader => new ExecutiveRiskSummary
                {
                    RiskId = reader.IsDBNull(reader.GetOrdinal("RISK_ID")) ? (int?)null : checked((int)ExecutiveDecimal(reader, "RISK_ID")),
                    RiskName = ExecutiveText(reader, "RISK_NAME"),
                    ParaCount = ExecutiveCount(reader, "PARA_COUNT"),
                    Percentage = ExecutiveDecimal(reader, "PERCENTAGE")
                }, filters: filters);

        internal List<ExecutiveAgingSummary> GetExecutiveAgingSummary(ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters = null)
            => scope.DashboardType == "CCO" ? ComplianceDistribution(scope, filters).Where(b => b.Dataset == "aging")
                .Select(b => new ExecutiveAgingSummary { BucketCode = b.BucketCode, BucketName = b.BucketName,
                    ParaCount = b.ParaCount ?? 0, HighRiskCount = b.HighRiskCount ?? 0 }).ToList()
            : ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_AGING_SUMMARY", scope, auditPeriod,
                reader => new ExecutiveAgingSummary
                {
                    BucketCode = ExecutiveText(reader, "BUCKET_CODE"),
                    BucketName = ExecutiveText(reader, "BUCKET_NAME"),
                    ParaCount = ExecutiveCount(reader, "PARA_COUNT"),
                    HighRiskCount = ExecutiveCount(reader, "HIGH_RISK_COUNT")
                }, filters: filters);

        internal List<ExecutiveCycleSummary> GetExecutiveCycleSummary(ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters = null)
            => scope.DashboardType == "CCO" ? ComplianceDistribution(scope, filters).Where(b => b.Dataset == "cycle")
                .Select(b => new ExecutiveCycleSummary { BucketCode = b.BucketCode, BucketName = b.BucketName,
                    ParaCount = b.ParaCount ?? 0, HighRiskCount = b.HighRiskCount ?? 0 }).ToList()
            : ReadExecutiveSummary("PKG_EXEC_DASHBOARD.P_GET_CYCLE_SUMMARY", scope, auditPeriod,
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

        internal ExecutiveKpiLifecycle GetExecutiveKpiLifecycle(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            ValidateExecutiveDashboardSession(scope);
            if (scope.DashboardType != "TOP")
                throw new ExecutiveDashboardAccessException("RoleMapping", "Lifecycle KPIs require the TOP perspective.");
            return ReadExecutiveKpiLifecycle(scope, filters);
        }

        // One typed reader and one Oracle lifecycle procedure; public entry points
        // independently enforce their existing TOP or dedicated Snap permissions.
        private ExecutiveKpiLifecycle ReadExecutiveKpiLifecycle(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            var dates = filters.Validate() ?? throw new ArgumentException("Both KPI dates are required.");
            using (var connection = DatabaseConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PKG_EXEC_DASHBOARD.P_GET_KPI_LIFECYCLE";
                command.CommandType = CommandType.StoredProcedure;
                command.BindByName = true;
                GuardAgainstDynamicSql(command);
                command.Parameters.Add("P_DASHBOARD_TYPE", OracleDbType.Varchar2).Value = scope.DashboardType;
                command.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = (object)scope.EntityId ?? DBNull.Value;
                command.Parameters.Add("P_START_DATE", OracleDbType.Date).Value = dates.StartDate;
                command.Parameters.Add("P_END_DATE", OracleDbType.Date).Value = dates.EndDate;
                command.Parameters.Add("P_ENTITY_TYPE_ID", OracleDbType.Int32).Value =
                    filters.SelectBy == "entityType" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
                command.Parameters.Add("P_AUDITED_BY_ID", OracleDbType.Int32).Value =
                    scope.PermissionPagePath == AuditParaSnapPolicy.PagePath
                        ? (object)filters.SnapDepartmentId ?? DBNull.Value
                        : filters.SelectBy == "auditedBy" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
                command.Parameters.Add("P_REPORTING_OFFICE_ID", OracleDbType.Int32).Value = (object)filters.ReportingOfficeId ?? DBNull.Value;
                command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                if (scope.PermissionPagePath == AuditParaSnapPolicy.PagePath)
                    command.Parameters.Add("P_SNAP_DIVISION_CODE", OracleDbType.Varchar2).Value = filters.SnapDivision;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) throw new InvalidOperationException("Lifecycle KPI cursor must contain one summary row.");
                    var openingQuality = ExecutiveText(reader, "HISTORICAL_OPENING_VERIFIED");
                    var closingQuality = ExecutiveText(reader, "CLOSING_ASOF_VERIFIED");
                    var openingVerified = openingQuality == "Y";
                    var closingVerified = closingQuality == "Y";
                    var openingEstimated = openingQuality == "E";
                    var closingEstimated = closingQuality == "E";
                    var result = new ExecutiveKpiLifecycle
                    {
                        OpeningOpenParas = openingVerified || openingEstimated ? ExecutiveNullableCount(reader, "OPENING_OPEN_PARAS") : null,
                        IssueIdentified = ExecutiveNullableCount(reader, "ISSUE_IDENTIFIED"),
                        IssueRectified = ExecutiveNullableCount(reader, "ISSUE_RECTIFIED"),
                        ParaAdded = ExecutiveNullableCount(reader, "PARA_ADDED"),
                        ParasSettled = ExecutiveNullableCount(reader, "PARAS_SETTLED"),
                        ClosingOpenParas = closingVerified || closingEstimated ? ExecutiveNullableCount(reader, "CLOSING_OPEN_PARAS") : null,
                        HighRiskOpenParas = closingVerified || closingEstimated ? ExecutiveNullableCount(reader, "HIGH_RISK_OPEN_PARAS") : null,
                        Over1YearParas = closingVerified || closingEstimated ? ExecutiveNullableCount(reader, "OVER_1_YEAR_PARAS") : null,
                        Over3ComplianceCycles = closingVerified || closingEstimated ? ExecutiveNullableCount(reader, "OVER_3_COMPLIANCE_CYCLES") : null,
                        HistoricalOpeningVerified = openingVerified,
                        ClosingAsofVerified = closingVerified,
                        OpeningEstimated = openingEstimated,
                        ClosingEstimated = closingEstimated
                    };
                    if (reader.Read()) throw new InvalidOperationException("Lifecycle KPI cursor must contain one summary row.");
                    return result;
                }
            }
        }

        private static long? ExecutiveNullableCount(OracleDataReader reader, string column)
            => reader.IsDBNull(reader.GetOrdinal(column)) ? (long?)null : checked((long)reader.GetDecimal(reader.GetOrdinal(column)));

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
