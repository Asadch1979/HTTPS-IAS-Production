using System;

namespace AIS.Models
{
    public static class ManagementDashboardPeriod
    {
        public static bool Applies(string perspective) => perspective == "GROUP" || perspective == "RLM";
        public static (DateTime Start, DateTime End)? CompletedDays(DateTime pakistanToday)
            => pakistanToday.DayOfYear == 1 ? null :
                (new DateTime(pakistanToday.Year, 1, 1), pakistanToday.Date.AddDays(-1));
        public static void Apply(ExecutiveDashboardFilterRequest filters, string perspective)
        {
            if (!Applies(perspective)) return;
            // January 1 has no completed day in the current year. Do not silently
            // expand to the previous year or present an invalid default range.
            var period = CompletedDays(AuditParaSnapPolicy.Today) ??
                throw new ArgumentException("No completed reporting day is available in the current year yet.");
            filters.StartDate = period.Start.ToString("yyyy-MM-dd");
            filters.EndDate = period.End.ToString("yyyy-MM-dd");
        }
    }
}
