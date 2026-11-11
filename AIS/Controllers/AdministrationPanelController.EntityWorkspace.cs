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
                return string.IsNullOrWhiteSpace(data) ? (IActionResult)RedirectToAction("Index", "Login")
                    : Unauthorized(new { error = "session_expired" });
                }

            if (!sessionHandler.TryGetUser(out var user) || user == null)
                {
                return string.IsNullOrWhiteSpace(data) ? (IActionResult)RedirectToAction("Index", "Login")
                    : Unauthorized(new { error = "session_expired" });
                }

            if (!CanAccessEntityWorkspace(user))
                {
                return StatusCode(403, new { error = "forbidden", message = "Entity Workspace permission is required." });
                }

            if (sessionHandler.IsSessionRevoked(user))
                return string.IsNullOrWhiteSpace(data)
                    ? (IActionResult)RedirectToAction("Index", "Login")
                    : Unauthorized(new { error = "session_expired", message = "Please sign in again." });

            // Use the same authorized page route for its read-only JSON operations.
            // No new unmapped API route or permission bypass is introduced.
            if (!string.IsNullOrWhiteSpace(data))
                return ReadEntityWorkspaceData(user, data, entityId, typeId, query, status, offset, pageSize);

            var model = BuildEntityWorkspaceViewModel(user, stepKey);
            var administrationLinks = BuildEntityAdministrationLinks(user);

            if (!string.IsNullOrWhiteSpace(stepKey) &&
                !TryGetRequestedStep(model, stepKey, out _, out var requestedStepError))
                return requestedStepError;

            PopulateDashboardChrome();
            ViewData["EntityAdministrationLinks"] = administrationLinks;
            ViewData["EntityReadEnabled"] = sessionHandler.IsSuperUser();
            return View("~/Views/AdministrationPanel/Entity_Workspace.cshtml", model);
            }

        // Shared legacy operations still require the unchanged loader's dashboard gate.
        // That gate never controls opening the workspace or its independent readers.
        private WorkflowDashboardViewModel BuildEntityWorkspaceViewModel(SessionUser user, string stepKey)
        {
            var steps = BuildEntityDashboardSteps();
            var legacyAccess = HasPageAccess(user, "/AdministrationPanel/Entity_Dashboard");
            foreach (var step in steps)
            {
                step.IsVisible = legacyAccess && step.RequiredPermissionPageId > 0 &&
                    (step.StepKey == "ENGAGEMENT_SHIFTING" ? CanAccessEngagementShifting(user)
                        : _permissionService.HasViewPermission(user, step.RequiredPermissionPageId));
                step.IsEnabled = step.IsVisible;
                step.IsSaved = true;
                step.StatusText = "Available";
            }
            return new WorkflowDashboardViewModel {
                DashboardKey = "ENTITY_WORKSPACE", DashboardTitle = "Entity Management Workspace",
                Steps = steps,
                CurrentStepKey = steps.FirstOrDefault(x => x.IsVisible && x.StepKey == stepKey)?.StepKey
                    ?? steps.FirstOrDefault(x => x.IsVisible)?.StepKey
            };
        }

        private bool CanAccessEntityWorkspace(SessionUser user)
        {
            var pageId = ResolveWorkflowPageId("/AdministrationPanel/Entity_Workspace");
            if (user == null || pageId != 467) return false;
            _permissionService.EnsurePermissionsCached(user);
            return sessionHandler.TryGetAllowedViewIds(out var allowedViewIds) && allowedViewIds.Contains(pageId);
        }

        // Single migration seam for dashboard and both shifting APIs. Dedicated permission
        // activation requires an approved page-ID/grant migration; retain current access meanwhile.
        private bool CanAccessEngagementShifting(SessionUser user)
            => HasPageAccess(user, "/AdministrationPanel/setup_engagement_reversal");

        private List<WorkflowDashboardStepModel> BuildEntityAdministrationLinks(SessionUser user)
        {
            var links = HasPageAccess(user, "/AdministrationPanel/Entity_Dashboard")
                ? BuildEntityDashboardSteps() : new List<WorkflowDashboardStepModel>();
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
            {
                var path = "/AdministrationPanel/" + action;
                if (!_pageIdResolver.TryResolvePageId(path, out var pageId) || pageId <= 0 ||
                    !_permissionService.HasViewPermission(user, pageId)) continue;
                links.Add(CreateDashboardStep(0, action.ToUpperInvariant(), title, path, null));
            }
            return links.Where(link => link.RequiredPermissionPageId > 0 &&
                (link.StepKey == "ENGAGEMENT_SHIFTING" ? CanAccessEngagementShifting(user)
                    : _permissionService.HasViewPermission(user, link.RequiredPermissionPageId))).ToList();
        }
    }
}
