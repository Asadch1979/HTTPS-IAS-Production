using AIS.Models;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Services
{
    public sealed class HeadAuditPlanTotals
    {
        public int Plans { get; private set; }
        public long? Days { get; private set; }
        public int Engagements { get; private set; }
        public static HeadAuditPlanTotals From(IEnumerable<HeadAuditPlanDetail> rows)
        {
            var saved = rows.ToList();
            var plans = saved.GroupBy(p => p.PlanId).ToList();
            // Missing/conflicting days cannot safely be represented as a known total.
            var known = plans.All(g => g.All(p => p.Days.HasValue) && g.Select(p => p.Days).Distinct().Count() == 1);
            return new HeadAuditPlanTotals {
                Plans = plans.Count,
                Days = known ? plans.Sum(g => (long)g.First().Days.Value) : (long?)null,
                Engagements = saved.Where(p => p.EngagementId.HasValue).Select(p => p.EngagementId.Value).Distinct().Count()
            };
        }
    }
}
