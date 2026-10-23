using System;
using System.Collections.Generic;

namespace AIS.Models
{
    public sealed class AuditParaSnapPerformanceRow
    {
        public string RowKind { get; set; }
        public string Division { get; set; }
        public int? DepartmentId { get; set; }
        public string Department { get; set; }
        public ExecutiveKpiLifecycle Position { get; set; }
        public long? NetChange => Position.ClosingOpenParas - Position.OpeningOpenParas;
        // Same denominator as the existing selected-period Snap rate. Do not
        // force rates where lifecycle events cannot be compared with stock.
        public decimal? SettlementPercentage
        {
            get
            {
                var denominator = Position.OpeningOpenParas + Position.ParaAdded;
                return (Position.HistoricalOpeningVerified || Position.OpeningEstimated) &&
                    denominator > 0 && Position.ParasSettled >= 0 && Position.ParasSettled <= denominator
                    ? Math.Round(Position.ParasSettled.Value * 100m / denominator.Value, 2) : null;
            }
        }
        public long? ArithmeticDifference => Position.ClosingOpenParas -
            (Position.OpeningOpenParas + Position.ParaAdded - Position.ParasSettled);
    }
    public sealed class AuditParaSnapPerformance
    {
        public List<AuditParaSnapPerformanceRow> Rows { get; set; } = new();
        public string Limitations { get; set; } = "Estimates use the approved lifecycle source and current audit-office mapping. Division and overall totals are queried independently; direct division audits or offices outside the direct-child list may not be represented by department rows. Live data may change between these reads. Net change is closing minus opening. Added minus settled need not reconcile; the difference is shown separately. Rates use settled / (opening + added), with N/A for unavailable or inconsistent denominators.";
    }
}
