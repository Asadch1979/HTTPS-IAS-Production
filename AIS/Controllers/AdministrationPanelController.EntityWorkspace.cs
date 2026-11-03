using AIS.Models;
using AIS.Models.WorkflowDashboard;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Controllers
{
    public partial class AdministrationPanelController
    {
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
                _permissionService.HasViewPermission(user, link.RequiredPermissionPageId)).ToList();
        }
    }
}
