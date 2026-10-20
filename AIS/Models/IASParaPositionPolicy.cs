using System;

namespace AIS.Models
{
    public static class IASParaPositionPolicy
    {
        public static readonly DateTime MinimumDate = new DateTime(2024, 1, 1);
        public static DateTime Today => AuditParaSnapPolicy.Today;
        public static DateTime DefaultEnd => Today.AddDays(-1);
        // On January 1 use the previous complete year.
        public static DateTime DefaultStart => StartFor(Today);
        public static DateTime StartFor(DateTime pakistanToday) => new DateTime(pakistanToday.AddDays(-1).Year, 1, 1);
        public static bool IsValid(DateTime start, DateTime end) =>
            start.Date >= MinimumDate && start.Date <= end.Date && end.Date <= Today;
    }
}
