using System;

namespace AIS.Models
{
    public static class ComplianceDashboardPolicy
    {
        public static void Validate(ExecutiveDashboardFilterRequest filters)
        {
            var dates = filters?.Validate() ?? throw new ArgumentException("Both reporting dates are required.");
            if (dates.EndDate > AuditParaSnapPolicy.Today ||
                (dates.EndDate.Year - dates.StartDate.Year) * 12 + dates.EndDate.Month - dates.StartDate.Month >= 1200)
                throw new ArgumentException("Select ordered, non-future dates spanning fewer than 1200 months.");
            if (filters.SnapDivision != null || filters.SnapDepartmentId.HasValue)
                throw new ArgumentException("Compliance does not accept Audit Para Snap selectors.");
        }
    }
}
