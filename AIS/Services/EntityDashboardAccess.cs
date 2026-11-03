using System;

namespace AIS.Services
{
    public static class EntityDashboardAccess
    {
        public static bool CanViewStep(bool dashboardAccess, int pageId, Func<int, bool> hasViewPermission)
            => dashboardAccess && pageId > 0 && hasViewPermission != null && hasViewPermission(pageId);
    }
}
