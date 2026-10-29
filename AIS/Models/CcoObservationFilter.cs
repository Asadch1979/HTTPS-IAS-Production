using System;

namespace AIS.Models
{
    // Dedicated CCO contract; no role, authorization token or hierarchy root input.
    // Production activation still requires a separate exact API permission.
    public sealed class CcoObservationFilter
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Risk { get; set; } = "HIGH";
        public int Offset { get; set; }
        public int Limit { get; set; } = 100;
        public int? ComId { get; set; }

        public void Validate()
        {
            ComplianceDashboardPolicy.Validate(new ExecutiveDashboardFilterRequest
            { StartDate = StartDate, EndDate = EndDate });
            if (Risk != "ALL" && Risk != "HIGH" && Risk != "MEDIUM" && Risk != "LOW" && Risk != "UNRATED")
                throw new ArgumentException("Select a supported observation risk.");
            if (Offset < 0 || Limit < 1 || Limit > 250 || Offset > int.MaxValue - Limit || ComId <= 0)
                throw new ArgumentException("Invalid observation page or identifier.");
        }

        // Codes established by PKG_EXEC_DASHBOARD.P_GET_SNAP_CRITICAL.
        // UNRATED means the SQL CASE ELSE (including NULL), not risk_id = 0.
        public int? RatedRiskId => Risk switch
        { "HIGH" => 1, "MEDIUM" => 2, "LOW" => 3, _ => null };
    }
}
