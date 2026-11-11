using AIS.Models;
using Microsoft.AspNetCore.Mvc;
using AIS.Models.WorkflowDashboard;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
{
    public partial class AdministrationPanelController
    {
        [HttpGet]
        public IActionResult Entity_Workspace(string stepKey = null,
            string data = null, int? entityId = null, int? typeId = null,
            string query = null, string status = "ALL", int offset = 0, int pageSize = 50)
            {
            if (!User.Identity.IsAuthenticated)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return RedirectToAction("Index", "Login");
                }

            if (!CanAccessEntityWorkspace(user))
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            if (sessionHandler.IsSessionRevoked(user))
                return string.IsNullOrWhiteSpace(data)
                    ? (IActionResult)RedirectToAction("Index", "Login")
                    : Unauthorized(new { error = "session_expired", message = "Please sign in again." });

            // Use the same authorized page route for its read-only JSON operations.
            // No new unmapped API route or permission bypass is introduced.
            if (!string.IsNullOrWhiteSpace(data))
                return ReadEntityWorkspaceData(user, data, entityId, typeId, query, status, offset, pageSize);

            var model = BuildEntityDashboardViewModel(user, stepKey);
            var administrationLinks = BuildEntityAdministrationLinks(user);
            if (!model.VisibleSteps.Any() && !administrationLinks.Any())
                {
                return RedirectToAction("Index", "PageNotFound");
                }

            if (!string.IsNullOrWhiteSpace(stepKey) &&
                !TryGetRequestedStep(model, stepKey, out _, out var requestedStepError))
                return requestedStepError;

            PopulateDashboardChrome();
            ViewData["EntityAdministrationLinks"] = administrationLinks;
            ViewData["EntityReadEnabled"] = sessionHandler.IsSuperUser();
            return View("~/Views/AdministrationPanel/Entity_Workspace.cshtml", model);
            }

        private bool CanAccessEntityWorkspace(SessionUser user)
        {
            var pageId = ResolveWorkflowPageId("/AdministrationPanel/Entity_Workspace");
            if (user == null || pageId <= 0) return false;
            _permissionService.EnsurePermissionsCached(user);
            return sessionHandler.TryGetAllowedViewIds(out var allowedViewIds) && allowedViewIds.Contains(pageId);
        }

        // Single migration seam for dashboard and both shifting APIs. Dedicated permission
        // activation requires an approved page-ID/grant migration; retain current access meanwhile.
        private bool CanAccessEngagementShifting(SessionUser user)
            => HasPageAccess(user, "/AdministrationPanel/setup_engagement_reversal");

        private List<WorkflowDashboardStepModel> BuildEntityAdministrationLinks(SessionUser user)
        {
            var links = BuildEntityDashboardSteps();
            // Some original steps only have partial views, with no standalone MVC action.
            foreach (var link in links)
                link.LegacyPath = "/AdministrationPanel/Entity_Dashboard?stepKey=" + link.StepKey;
            var additional = new[] {
                ("entity_heirarchy", "Entity Hierarchy"),
                ("manage_entity_type", "Entity Types"),
                ("manage_entity_relations", "Entity Relations"),
                ("manage_entity_mapping", "Entity Mapping"),
                ("audit_comp_management", "Audit / Compliance Management"),
                ("gm_repo_line_management", "GM / Reporting Line Management"),
                ("entity_gm_reporting_div_management", "GM / Reporting / Division Management"),
                ("EntityShiftingHistory", "Entity Shifting History")
            };
            foreach (var (action, title) in additional)
                links.Add(CreateDashboardStep(0, action.ToUpperInvariant(), title,
                    "/AdministrationPanel/" + action, null));
            return links.Where(link => link.RequiredPermissionPageId > 0 &&
                (link.StepKey == "ENGAGEMENT_SHIFTING" ? CanAccessEngagementShifting(user)
                    : _permissionService.HasViewPermission(user, link.RequiredPermissionPageId))).ToList();
        }
    }
}
