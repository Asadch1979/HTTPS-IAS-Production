using Microsoft.AspNetCore.Mvc;

namespace AIS.Controllers
    {
    public partial class ApiCallsController
        {
        // LEGACY / UNUSED / REPLACED API ACTIONS

        // Original file: AIS/Controllers/ApiCallsController.cs
        // Reason moved: No active JS/View endpoint reference found; compliance flow updates are consolidated through add_compliance_flow.
        // Replacement action/method: add_compliance_flow -> DBConnection.AddComplianceFlow -> pkg_ad.P_ADD_UPDATE_COMPLIANCE_FLOW.
        // Date moved: 2026-06-01.
        [NonAction]
        public string update_compliance_flow(string ID, string ENTITY_TYPE_ID, string GROUP_ID, string PREV_GROUP_ID, string NEXT_GROUP_ID)
            {
            return "{\"Status\":true,\"Message\":\"" + dBConnection.UpdateComplianceFlow(ID, ENTITY_TYPE_ID, GROUP_ID, PREV_GROUP_ID, NEXT_GROUP_ID) + "\"}";
            }
        }
    }
