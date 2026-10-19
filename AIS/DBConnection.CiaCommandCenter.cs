using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        private List<T> ReadCiaCommandCursor<T>(
            ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters,
            int auditPeriodId, string procedure, Func<OracleDataReader, T> mapper)
        {
            ValidateExecutiveDashboardSession(scope);
            if (scope.DashboardType != "CIA")
                throw new ExecutiveDashboardAccessException("RoleMapping", "CIA Command Center is restricted to the verified CIA perspective.");
            if (auditPeriodId <= 0) throw new ArgumentException("Select a valid audit period.");
            filters ??= new ExecutiveDashboardFilterRequest();
            filters.Validate();

            var rows = new List<T>();
            using var con = DatabaseConnection();
            using var command = con.CreateCommand();
            command.CommandText = procedure;
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("P_DASHBOARD_TYPE", OracleDbType.Varchar2).Value = "CIA";
            command.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = (object)scope.EntityId ?? DBNull.Value;
            command.Parameters.Add("P_AUDIT_PERIOD_ID", OracleDbType.Int32).Value = auditPeriodId;
            command.Parameters.Add("P_ENTITY_TYPE_ID", OracleDbType.Int32).Value =
                filters.SelectBy == "entityType" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
            command.Parameters.Add("P_AUDITED_BY_ID", OracleDbType.Int32).Value =
                filters.SelectBy == "auditedBy" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
            command.Parameters.Add("P_REPORTING_OFFICE_ID", OracleDbType.Int32).Value =
                (object)filters.ReportingOfficeId ?? DBNull.Value;
            command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            using var reader = command.ExecuteReader();
            while (reader.Read()) rows.Add(mapper(reader));
            ValidateExecutiveDashboardSession(scope);
            return rows;
        }

        internal CiaCommandKpis GetCiaCommandKpis(
            ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters, int auditPeriodId)
        {
            var items = ReadCiaCommandCursor(scope, filters, auditPeriodId,
                "PKG_EXEC_DASHBOARD.P_GET_CIA_EXEC_KPIS", r => new CiaCommandKpis
                {
                    PlannedEntities = RoleNullableLong(r, "PLANNED_ENTITIES"),
                    AuditedEntities = RoleNullableLong(r, "AUDITED_ENTITIES"),
                    PlanCoveragePct = CiaNullableDecimal(r, "PLAN_COVERAGE_PCT"),
                    DelayedAuditReports = RoleNullableLong(r, "DELAYED_AUDIT_REPORTS"),
                    FraudA1Paras = RoleNullableLong(r, "FRAUD_A1_PARAS"),
                    AwaitingAuditeeResponse = RoleNullableLong(r, "AWAITING_AUDITEE_RESPONSE"),
                    TeamsConductingAudit = RoleNullableLong(r, "TEAMS_CONDUCTING_AUDIT"),
                    AuditPeriodId = RoleRequiredInt(r, "AUDIT_PERIOD_ID"),
                    AuditPeriodFrom = RoleDate(r, "AUDIT_PERIOD_FROM"),
                    AuditPeriodTo = RoleDate(r, "AUDIT_PERIOD_TO"),
                    CurrentStateAsOf = RoleDate(r, "CURRENT_STATE_AS_OF")
                });
            if (items.Count != 1)
                throw new InvalidOperationException("CIA command KPI cursor must return exactly one row.");
            // Return actual Oracle values, including NULL (display N/A) and zero
            // (display 0). Availability is not controlled by environment flags.
            // These operational estimates still require independent reconciliation
            // against live IAS workflow records before wider role rollout.
            items[0].WorkflowVerificationNote =
                items[0].DelayedAuditReports.HasValue && items[0].TeamsConductingAudit.HasValue
                    ? "Values returned by IAS; reporting deadline and active-stage criteria are pending production validation."
                    : "N/A means the IAS procedure returned NULL for at least one workflow KPI. Zero, when returned, remains zero.";
            return items[0];
        }

        internal List<CiaIssueNatureRow> GetCiaIssueNature(
            ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters, int auditPeriodId)
            => ReadCiaCommandCursor(scope, filters, auditPeriodId,
                "PKG_EXEC_DASHBOARD.P_GET_CIA_ISSUE_NATURE", r => new CiaIssueNatureRow
                {
                    CategoryCode = RoleText(r, "CATEGORY_CODE"),
                    CategoryName = RoleText(r, "CATEGORY_NAME"),
                    DisplayOrder = RoleRequiredInt(r, "DISPLAY_ORDER"),
                    IssueCount = RoleLong(r, "ISSUE_COUNT"),
                    TotalIssues = RoleLong(r, "TOTAL_ISSUES"),
                    UnmappedIssues = RoleLong(r, "UNMAPPED_ISSUES"),
                    MappingStatus = RoleText(r, "MAPPING_STATUS")
                });

        private static decimal? CiaNullableDecimal(OracleDataReader r, string column)
        {
            var index = r.GetOrdinal(column);
            return r.IsDBNull(index) ? null : r.GetDecimal(index);
        }
    }
}
