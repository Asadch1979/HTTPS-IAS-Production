using AIS.Models;
using AIS.Session;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        // The caller must first resolve its perspective from the authenticated
        // session and NarrowExecutiveDashboardScope; never accept a browser root.
        private List<T> ReadRoleDashboardCursor<T>(
            string procedure, ExecutiveDashboardScope scope,
            Func<OracleDataReader, T> map,
            string auditPeriod = null, int? entityTypeId = null, int? maxRows = null)
        {
            ValidateExecutiveDashboardSession(scope);
            RequireExecutiveGroupScope(scope);
            if (scope.DashboardType != "GROUP" && scope.DashboardType != "RLM")
                throw new ExecutiveDashboardAccessException("RoleMapping", "Management reporting requires GROUP or RLM.");
            if (scope.EntityId.GetValueOrDefault() <= 0)
                throw new ExecutiveDashboardAccessException("EntityScope", "An authorized reporting root is required.");
            if (maxRows.HasValue && (maxRows < 1 || maxRows > 500))
                throw new ArgumentException("Maximum rows must be between 1 and 500.");

            var rows = new List<T>();
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandText = procedure;
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("P_DASHBOARD_TYPE", OracleDbType.Varchar2).Value = scope.DashboardType;
            command.Parameters.Add("P_ROOT_ENTITY_ID", OracleDbType.Int32).Value = scope.EntityId.Value;
            command.Parameters.Add("P_AUDIT_PERIOD", OracleDbType.Varchar2).Value =
                string.IsNullOrWhiteSpace(auditPeriod) ? (object)DBNull.Value : auditPeriod.Trim();
            if (procedure.EndsWith("P_GET_ROLE_ENTITY_HEATMAP", StringComparison.Ordinal))
                command.Parameters.Add("P_ENTITY_TYPE_ID", OracleDbType.Int32).Value = (object)entityTypeId ?? DBNull.Value;
            if (maxRows.HasValue)
                command.Parameters.Add("P_MAX_ROWS", OracleDbType.Int32).Value = maxRows.Value;
            command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = command.ExecuteReader();
            while (reader.Read()) rows.Add(map(reader));
            ValidateExecutiveDashboardSession(scope);
            return rows;
        }

        internal List<RoleEntityHeatmapRow> GetRoleEntityHeatmap(
            ExecutiveDashboardScope scope, string auditPeriod, ExecutiveDashboardFilterRequest filters, int maxRows = 250)
            => ReadRoleDashboardCursor("PKG_EXEC_DASHBOARD.P_GET_ROLE_ENTITY_HEATMAP", scope,
                r => new RoleEntityHeatmapRow
                {
                    EntityId = RoleRequiredInt(r, "ENTITY_ID"),
                    EntityName = RoleText(r, "ENTITY_NAME"),
                    EntityTypeId = RoleNullableInt(r, "ENTITY_TYPE_ID"),
                    OpenParas = RoleLong(r, "OPEN_PARAS"),
                    HighRisk = RoleLong(r, "HIGH_RISK"),
                    MediumRisk = RoleLong(r, "MEDIUM_RISK"),
                    LowRisk = RoleLong(r, "LOW_RISK"),
                    Unrated = RoleLong(r, "UNRATED"),
                    AgedOver365 = RoleLong(r, "AGED_OVER_365"),
                    Over3Cycles = RoleLong(r, "OVER_3_CYCLES"),
                    HighRiskSharePct = RoleDecimal(r, "HIGH_RISK_SHARE_PCT")
                }, auditPeriod, filters?.SelectBy == "entityType" ? filters.DimensionId : null, maxRows);

        internal List<RoleUnitComparisonRow> GetRoleUnitComparison(ExecutiveDashboardScope scope, string auditPeriod)
            => ReadRoleDashboardCursor("PKG_EXEC_DASHBOARD.P_GET_ROLE_UNIT_COMPARISON", scope,
                r => new RoleUnitComparisonRow
                {
                    UnitId = RoleRequiredInt(r, "UNIT_ID"),
                    UnitName = RoleText(r, "UNIT_NAME"),
                    OpenParas = RoleLong(r, "OPEN_PARAS"),
                    HighRiskOpen = RoleLong(r, "HIGH_RISK_OPEN"),
                    AgedOver365 = RoleLong(r, "AGED_OVER_365"),
                    Over3Cycles = RoleLong(r, "OVER_3_CYCLES"),
                    SettledTotal = RoleLong(r, "SETTLED_TOTAL"),
                    SettledMtd = RoleLong(r, "SETTLED_MTD")
                }, auditPeriod);

        internal List<RoleFollowupRow> GetRoleFollowups(ExecutiveDashboardScope scope, string auditPeriod, int maxRows = 100)
            => ReadRoleDashboardCursor("PKG_EXEC_DASHBOARD.P_GET_ROLE_FOLLOWUPS", scope,
                r => new RoleFollowupRow
                {
                    ComId = RoleLong(r, "COM_ID"),
                    ParaNo = RoleText(r, "PARA_NO"),
                    AuditPeriod = RoleText(r, "AUDIT_PERIOD"),
                    GistOfParas = RoleText(r, "GIST_OF_PARAS"),
                    EntityId = RoleNullableInt(r, "ENTITY_ID"),
                    EntityName = RoleText(r, "ENTITY_NAME"),
                    DivisionId = RoleNullableInt(r, "DIVISION_ID"),
                    DivisionName = RoleText(r, "DIVISION_NAME"),
                    BusinessGroupId = RoleNullableInt(r, "BUSINESS_GROUP_ID"),
                    BusinessGroupName = RoleText(r, "BUSINESS_GROUP_NAME"),
                    RiskId = RoleNullableInt(r, "RISK_ID"),
                    RiskName = RoleText(r, "RISK_NAME"),
                    ParaStatus = RoleNullableInt(r, "PARA_STATUS"),
                    ComStatus = RoleNullableInt(r, "COM_STATUS"),
                    ComStage = RoleNullableInt(r, "COM_STAGE"),
                    ComCycle = RoleNullableInt(r, "COM_CYCLE"),
                    ParaAddedOn = RoleDate(r, "PARA_ADDED_ON"),
                    AgingDays = RoleNullableInt(r, "AGING_DAYS"),
                    LastMovementDate = RoleDate(r, "LAST_MOVEMENT_DATE"),
                    DaysSinceLastMovement = RoleNullableInt(r, "DAYS_SINCE_LAST_MOVEMENT"),
                    PriorityReason = RoleText(r, "PRIORITY_REASON")
                }, auditPeriod, maxRows: maxRows);

        // The Team Lead package's P_PPNO is sourced ONLY from the current IAS
        // session, even though the Oracle package accepts it as a parameter.
        internal TeamLeadDashboardSnapshot GetTeamLeadDashboardSnapshot(int? engId)
        {
            var session = CreateSessionHandler();
            var user = session.GetUserOrThrow();
            if (session.IsSessionRevoked(user) ||
                !int.TryParse(user.PPNumber, out var ppno) || ppno <= 0)
                throw new UnauthenticatedException();
            if (engId.HasValue && engId.Value <= 0)
                throw new ArgumentException("Invalid engagement.");

            var result = new TeamLeadDashboardSnapshot();
            result.Summary = ReadTeamLeadCursor("PKG_TEAM_LEAD_DASHBOARD.P_GET_SUMMARY", ppno, engId: null,
                r => new TeamLeadDashboardSummary
                {
                    ActiveEngagements = RoleLong(r, "ACTIVE_ENGAGEMENTS"),
                    AwaitingMyWorkpaperReview = RoleLong(r, "AWAITING_MY_WORKPAPER_REVIEW"),
                    ObservationsAwaitingAuditorReview = RoleLong(r, "OBSERVATIONS_AWAITING_AUDITOR_REVIEW"),
                    ObservationsAwaitingAuditeeResponse = RoleLong(r, "OBSERVATIONS_AWAITING_AUDITEE_RESPONSE"),
                    WorkpapersDueNext7Days = RoleLong(r, "WORKPAPERS_DUE_NEXT_7_DAYS"),
                    EngagementsEndingNext7Days = RoleLong(r, "ENGAGEMENTS_ENDING_NEXT_7_DAYS"),
                    CurrentStateAsOf = RoleDate(r, "CURRENT_STATE_AS_OF") ?? DateTime.MinValue
                })[0];
            result.Engagements = ReadTeamLeadCursor("PKG_TEAM_LEAD_DASHBOARD.P_GET_ENGAGEMENTS", ppno, null,
                r => new TeamLeadEngagementRow
                {
                    EngId = RoleLong(r, "ENG_ID"),
                    PlanId = RoleNullableLong(r, "PLAN_ID"),
                    PeriodId = RoleNullableLong(r, "PERIOD_ID"),
                    EntityId = RoleNullableInt(r, "ENTITY_ID"),
                    EntityName = RoleText(r, "ENTITY_NAME"),
                    EngagementStatusId = RoleNullableInt(r, "ENGAGEMENT_STATUS_ID"),
                    TaskStatusId = RoleNullableInt(r, "TASK_STATUS_ID"),
                    TaskStatusDescription = RoleText(r, "TASK_STATUS_DESCRIPTION"),
                    AuditStartDate = RoleDate(r, "AUDIT_STARTDATE"),
                    AuditEndDate = RoleDate(r, "AUDIT_ENDDATE"),
                    OperationStartDate = RoleDate(r, "OPERATION_STARTDATE"),
                    OperationEndDate = RoleDate(r, "OPERATION_ENDDATE"),
                    DaysToAuditEnd = RoleNullableInt(r, "DAYS_TO_AUDIT_END"),
                    AwaitingAuditorReview = RoleLong(r, "AWAITING_AUDITOR_REVIEW"),
                    AwaitingAuditeeResponse = RoleLong(r, "AWAITING_AUDITEE_RESPONSE")
                });
            result.Workpapers = ReadTeamLeadCursor("PKG_TEAM_LEAD_DASHBOARD.P_GET_WORKPAPERS", ppno, engId,
                r => new TeamLeadWorkpaperRow
                {
                    WpId = RoleLong(r, "WP_ID"), EngId = RoleLong(r, "ENG_ID"),
                    EntityId = RoleNullableInt(r, "ENTITY_ID"),
                    PaperType = RoleText(r, "PAPER_TYPE"),
                    ReferenceNo = RoleText(r, "REFERENCE_NO"),
                    VersionNo = RoleRequiredInt(r, "VERSION_NO"),
                    Status = RoleText(r, "STATUS"),
                    PreparerPpno = RoleText(r, "PREPARER_PPNO"),
                    ReviewerPpno = RoleText(r, "REVIEWER_PPNO"),
                    DueDate = RoleDate(r, "DUE_DATE"),
                    ReviewedOn = RoleDate(r, "REVIEWED_ON"),
                    ApprovedOn = RoleDate(r, "APPROVED_ON"),
                    TotalItems = RoleLong(r, "TOTAL_ITEMS"),
                    PassedItems = RoleLong(r, "PASSED_ITEMS"),
                    ExceptionItems = RoleLong(r, "EXCEPTION_ITEMS"),
                    PendingItems = RoleLong(r, "PENDING_ITEMS"),
                    DaysToDue = RoleNullableInt(r, "DAYS_TO_DUE")
                });
            result.Observations = ReadTeamLeadCursor("PKG_TEAM_LEAD_DASHBOARD.P_GET_OBSERVATIONS", ppno, engId,
                r => new TeamLeadObservationRow
                {
                    ObservationId = RoleLong(r, "OBSERVATION_ID"),
                    EngId = RoleLong(r, "ENG_ID"),
                    ObservationStatusId = RoleRequiredInt(r, "OBSERVATION_STATUS_ID"),
                    RiskId = RoleNullableInt(r, "RISK_ID"),
                    AnnexId = RoleNullableInt(r, "ANNEX_ID"),
                    EnteredDate = RoleDate(r, "ENTEREDDATE")
                }, maxRows: 100);
            var now = session.GetUserOrThrow();
            if (session.IsSessionRevoked(now) || now.ID != user.ID ||
                now.SessionId != user.SessionId || now.UserContextAssignmentId != user.UserContextAssignmentId)
                throw new UnauthenticatedException();
            return result;
        }

        private List<T> ReadTeamLeadCursor<T>(string procedure, int ppno, int? engId,
            Func<OracleDataReader, T> map, int? maxRows = null)
        {
            var rows = new List<T>();
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedure;
            command.BindByName = true;
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("P_PPNO", OracleDbType.Int32).Value = ppno;
            if (procedure.EndsWith("P_GET_WORKPAPERS", StringComparison.Ordinal) ||
                procedure.EndsWith("P_GET_OBSERVATIONS", StringComparison.Ordinal))
                command.Parameters.Add("P_ENG_ID", OracleDbType.Int32).Value = (object)engId ?? DBNull.Value;
            if (maxRows.HasValue) command.Parameters.Add("P_MAX_ROWS", OracleDbType.Int32).Value = maxRows.Value;
            command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = command.ExecuteReader();
            while (reader.Read()) rows.Add(map(reader));
            return rows;
        }

        private static string RoleText(OracleDataReader r, string name)
        {
            var i = r.GetOrdinal(name);
            return r.IsDBNull(i) ? null : Convert.ToString(r.GetValue(i));
        }
        private static long RoleLong(OracleDataReader r, string name)
        {
            var i = r.GetOrdinal(name);
            return r.IsDBNull(i) ? 0 : checked(Convert.ToInt64(r.GetDecimal(i)));
        }
        private static long? RoleNullableLong(OracleDataReader r, string name)
        {
            var i = r.GetOrdinal(name);
            return r.IsDBNull(i) ? null : checked(Convert.ToInt64(r.GetDecimal(i)));
        }
        private static int RoleRequiredInt(OracleDataReader r, string name)
            => checked((int)RoleLong(r, name));
        private static int? RoleNullableInt(OracleDataReader r, string name)
        {
            var n = RoleNullableLong(r, name);
            return n.HasValue ? checked((int)n.Value) : null;
        }
        private static decimal RoleDecimal(OracleDataReader r, string name)
        {
            var i = r.GetOrdinal(name);
            return r.IsDBNull(i) ? 0m : r.GetDecimal(i);
        }
        private static DateTime? RoleDate(OracleDataReader r, string name)
        {
            var i = r.GetOrdinal(name);
            return r.IsDBNull(i) ? null : r.GetDateTime(i);
        }
    }
}
