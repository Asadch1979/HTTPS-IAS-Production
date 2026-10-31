using AIS.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;

namespace AIS.Controllers
{
    public partial class DBConnection
    {
        // Function Wise Position reuses the existing authenticated Management
        // Audit Report E reader. No PKG_CCO_SCOPE, GTT, scope token or separate
        // PKG_CCO_FUNCTION_POSITION is used on this request path.
        //
        // The controller MUST first enforce the active CCO/Super User context,
        // page 1000302 and the exact GET /api/CcoCompliance/GetFunctionPosition
        // API permission. These gates remain mandatory.
        internal List<CcoFunctionPositionRow> GetCcoFunctionPosition(
            ExecutiveDashboardScope scope, CcoOrganizationFilter filters)
        {
            var stage = "validate_ias_context";
            try
            {
                if (filters == null) throw new ArgumentException("A valid reporting period is required.");
                ValidateCcoOrganizationContext(scope);
                filters.Validate();

                // This endpoint accepts dates and maxRows only. Never use a
                // client-supplied root or selector to broaden the CCO portfolio.
                if (filters.GroupId.HasValue || filters.DivisionId.HasValue ||
                    filters.GmOfficeId.HasValue || filters.RegionId.HasValue ||
                    filters.EntityId.HasValue)
                    throw new ArgumentException("Function Wise Position accepts dates only.");

                var reportFilters = new ExecutiveDashboardFilterRequest {
                    StartDate = filters.StartDate,
                    EndDate = filters.EndDate,
                    SelectBy = "entityType",
                    View = "reports"
                };
                ComplianceDashboardPolicy.Validate(reportFilters);

                stage = "execute_authorized_management_report_e";
                // Existing P_MANAGEMENT_SETTLEMENT, with bound Oracle parameters,
                // validated IAS session before/after read, fixed CCO perspective,
                // and no organizational root supplied by the browser.
                var report = GetExecutiveReportE(scope, reportFilters);

                stage = "validate_management_report_e_rows";
                if (report == null || report.ReportId != "E" ||
                    report.DashboardType != "CCO" || report.GroupRootId.HasValue ||
                    report.Rows == null || report.Rows.Count > filters.MaxRows)
                    throw new InvalidOperationException("Invalid CCO management reporting result.");

                var rows = new List<CcoFunctionPositionRow>(report.Rows.Count);
                foreach (var source in report.Rows)
                {
                    if (source == null ||
                        (source.RowKind != "DETAIL" && source.RowKind != "SUBTOTAL" &&
                         source.RowKind != "TOTAL") ||
                        source.OpeningBalance < 0 || source.ParasAdded < 0 ||
                        source.TotalParas < 0 || source.SettledParas < 0 ||
                        source.OutstandingParas < 0 ||
                        source.TotalParas != checked(source.OpeningBalance + source.ParasAdded) ||
                        (source.SettlementPercentage.HasValue &&
                          (source.SettlementPercentage.Value < 0 ||
                           source.SettlementPercentage.Value > 100000)))
                        throw new InvalidOperationException("Invalid CCO management reporting row.");

                    rows.Add(new CcoFunctionPositionRow {
                        RowKind = source.RowKind,
                        GroupId = source.GroupId,
                        GroupName = source.GroupName,
                        DivisionId = source.ItemId,
                        DivisionName = source.ItemName,
                        OpeningBalance = source.OpeningBalance,
                        ParasAdded = source.ParasAdded,
                        TotalParas = source.TotalParas,
                        SettledParas = source.SettledParas,
                        OutstandingParas = source.OutstandingParas,
                        SettlementPercentage = source.SettlementPercentage
                    });
                }

                // Validate again after reading and before the controller returns.
                stage = "revalidate_ias_context";
                ValidateCcoOrganizationContext(scope);
                return rows;
            }
            catch (Exception ex) when (ex is OracleException ||
                                        ex is InvalidOperationException ||
                                        ex is FormatException ||
                                        ex is OverflowException)
            {
                throw new CcoFunctionPositionReadException(stage, ex);
            }
        }
    }

    // Server-log-only diagnostics. The CCO endpoint does not expose Oracle
    // exception detail, authorization context or raw SQL to the browser.
    internal sealed class CcoFunctionPositionReadException : Exception
    {
        internal string Stage { get; }
        internal int? OracleNumber { get; }

        internal CcoFunctionPositionReadException(string stage, Exception cause)
            : base("The authorized CCO Function Wise Position read failed.", cause)
        {
            Stage = stage;
            OracleNumber = cause is OracleException oracle ? oracle.Number : (int?)null;
        }
    }
}
