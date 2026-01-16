using System;
using System.Diagnostics;
using AIS.Models;

namespace AIS.Controllers
    {
    public partial class DBConnection
        {
        public bool HasPermissionForView(SessionUser user, int pageId)
            {
            if (pageId <= 0)
                {
                return false;
                }

            try
                {
                if (sessionHandler.IsSuperUser())
                    {
                    return true;
                    }

                if (sessionHandler.TryGetAllowedViewIds(out var allowedViewIds))
                    {
                    return allowedViewIds.Contains(pageId);
                    }

                Trace.TraceWarning("Allowed views were unavailable in session while evaluating {0}. Denying access.", pageId);
                return false;
                }
            catch (Exception ex)
                {
                Trace.TraceError("Permission evaluation failed for view {0}: {1}", pageId, ex.Message);
                return false;
                }
            }
        }
    }
