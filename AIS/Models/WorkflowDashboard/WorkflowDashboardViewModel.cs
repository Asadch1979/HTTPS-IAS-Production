using System.Collections.Generic;
using System.Linq;

namespace AIS.Models.WorkflowDashboard
    {
    public class WorkflowDashboardViewModel
        {
        public string DashboardKey { get; set; }
        public string DashboardTitle { get; set; }
        public string CurrentStepKey { get; set; }
        public List<WorkflowDashboardStepModel> Steps { get; set; } = new List<WorkflowDashboardStepModel>();

        public IEnumerable<WorkflowDashboardStepModel> VisibleSteps => Steps.Where(step => step.IsVisible);
        public WorkflowDashboardStepModel CurrentStep => VisibleSteps.FirstOrDefault(step => step.StepKey == CurrentStepKey);
        }
    }
