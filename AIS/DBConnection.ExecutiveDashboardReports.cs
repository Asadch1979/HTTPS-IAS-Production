using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        internal ExecutiveReportResult GetExecutiveReportA(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
            => ReadExecutiveReport("A", "PKG_EXEC_REPORTS.P_ANNUAL_FIELD_PROGRESS", scope, filters);
        internal ExecutiveReportResult GetExecutiveReportB(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
            => ReadExecutiveReport("B", "PKG_EXEC_REPORTS.P_FIELD_OFFICE_POSITION", scope, filters);
        internal ExecutiveReportResult GetExecutiveReportC(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
            => ReadExecutiveReport("C", "PKG_EXEC_REPORTS.P_GM_REGION_POSITION", scope, filters);
        internal ExecutiveReportResult GetExecutiveReportD(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
            => ReadExecutiveReport("D", "PKG_EXEC_REPORTS.P_REGION_RANKINGS", scope, filters);
        internal ExecutiveReportResult GetExecutiveReportE(ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
            => ReadExecutiveReport("E", "PKG_EXEC_REPORTS.P_MANAGEMENT_SETTLEMENT", scope, filters);

        private ExecutiveReportResult ReadExecutiveReport(string reportId, string procedure, ExecutiveDashboardScope scope, ExecutiveDashboardFilterRequest filters)
        {
            ValidateExecutiveDashboardSession(scope);
            RequireExecutiveGroupScope(scope);
            if (filters == null) throw new ArgumentException("Report filters and dates are required.");
            ManagementDashboardPeriod.Apply(filters, scope.DashboardType);
            var dates = filters.Validate() ?? throw new ArgumentException("Both report dates are required.");
            // Preserve the fixed authenticated root independently of any selected entity.
            // Dimensional validation uses observation-level SQL data, not a directory scan.
            NarrowExecutiveDashboardScope(scope, filters.EntityId);
            var rows = new List<ExecutiveReportRow>();
            using var connection = DatabaseConnection();
            using var command = connection.CreateCommand();
            command.CommandText = procedure;
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;
            GuardAgainstDynamicSql(command);
            command.Parameters.Add("P_DASHBOARD_TYPE", OracleDbType.Varchar2).Value = scope.DashboardType;
            command.Parameters.Add("P_SCOPE_ROOT_ID", OracleDbType.Int32).Value = (object)scope.GroupRootId ?? DBNull.Value;
            command.Parameters.Add("P_START_DATE", OracleDbType.Date).Value = dates.StartDate;
            command.Parameters.Add("P_END_DATE", OracleDbType.Date).Value = dates.EndDate;
            command.Parameters.Add("P_ENTITY_TYPE_ID", OracleDbType.Int32).Value = filters.SelectBy == "entityType" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
            command.Parameters.Add("P_AUDITED_BY_ID", OracleDbType.Int32).Value =
                scope.PermissionPagePath == AuditParaSnapPolicy.PagePath
                    ? (object)filters.SnapDepartmentId ?? DBNull.Value
                    : filters.SelectBy == "auditedBy" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
            command.Parameters.Add("P_REPORTING_OFFICE_ID", OracleDbType.Int32).Value = (object)filters.ReportingOfficeId ?? DBNull.Value;
            command.Parameters.Add("P_ENTITY_ID", OracleDbType.Int32).Value = (object)filters.EntityId ?? DBNull.Value;
            command.Parameters.Add("IO_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            if (scope.PermissionPagePath == AuditParaSnapPolicy.PagePath)
                {
                    command.Parameters.Add("P_SNAP_DIVISION_CODE", OracleDbType.Varchar2).Value = filters.SnapDivision;
                    command.Parameters.Add("P_SNAP_AUDITED_BY_ID", OracleDbType.Int32).Value =
                        filters.SelectBy == "auditedBy" ? (object)filters.DimensionId ?? DBNull.Value : DBNull.Value;
                }
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new ExecutiveReportRow {
                    RowKind = ExecutiveText(reader, "ROW_KIND"), GroupId = OrgNumber(reader, "GROUP_ID"), GroupName = ExecutiveText(reader, "GROUP_NAME"),
                    ItemId = OrgNumber(reader, "ITEM_ID"), ItemCode = ExecutiveText(reader, "ITEM_CODE"), ItemName = ExecutiveText(reader, "ITEM_NAME"),
                    PeriodStart = reader.GetDateTime(reader.GetOrdinal("PERIOD_START")), PeriodEnd = reader.GetDateTime(reader.GetOrdinal("PERIOD_END")),
                    OpeningBalance = ExecutiveCount(reader, "OPENING_BALANCE"), ParasAdded = ExecutiveCount(reader, "PARAS_ADDED"),
                    TotalParas = ExecutiveCount(reader, "TOTAL_PARAS"), SettledParas = ExecutiveCount(reader, "SETTLED_PARAS"), OutstandingParas = ExecutiveCount(reader, "OUTSTANDING_PARAS"),
                    SettlementPercentage = reader.IsDBNull(reader.GetOrdinal("SETTLEMENT_PERCENTAGE")) ? null : reader.GetDecimal(reader.GetOrdinal("SETTLEMENT_PERCENTAGE")),
                    LowRisk = ExecutiveCount(reader, "LOW_RISK"), MediumRisk = ExecutiveCount(reader, "MEDIUM_RISK"), HighRisk = ExecutiveCount(reader, "HIGH_RISK"),
                    UnratedRisk = ExecutiveCount(reader, "UNRATED_RISK"), SettledByAudit = ExecutiveCount(reader, "SETTLED_AUDIT"),
                    SettledByCompliance = ExecutiveCount(reader, "SETTLED_COMPLIANCE"), SettledByUnknown = ExecutiveCount(reader, "SETTLED_UNKNOWN"),
                    ReopeningEvents = ExecutiveCount(reader, "REOPENING_EVENTS"), ResettlementEvents = ExecutiveCount(reader, "RESETTLEMENT_EVENTS"),
                    InitiallyClosedAdditions = ExecutiveCount(reader, "INITIALLY_CLOSED_ADDITIONS"), ReconciliationDifference = ExecutiveCount(reader, "RECONCILIATION_DIFFERENCE"),
                    AuditorAssignmentMismatch = ExecutiveCount(reader, "AUDITOR_ASSIGNMENT_MISMATCH"), RankingGroup = ExecutiveText(reader, "RANKING_GROUP"), RankPosition = OrgNumber(reader, "RANK_POSITION")
                });
            }
            ValidateExecutiveDashboardSession(scope);
            return new ExecutiveReportResult { ReportId = reportId, Filters = filters, DashboardType = scope.DashboardType, GroupRootId = scope.GroupRootId, Rows = rows };
        }
    }
}
