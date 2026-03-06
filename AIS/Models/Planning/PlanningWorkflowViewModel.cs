using System.Collections.Generic;
using System.Linq;

namespace AIS.Models.Planning
    {
    public class PlanningWorkflowViewModel
        {
        public int? ContextId { get; set; }
        public int? ContextSecondaryId { get; set; }
        public string CurrentStepCode { get; set; }
        public List<PlanningWorkflowStepModel> Steps { get; set; } = new List<PlanningWorkflowStepModel>();

        public IEnumerable<PlanningWorkflowStepModel> VisibleSteps => Steps.Where(step => step.IsVisible);
        public PlanningWorkflowStepModel CurrentStep => VisibleSteps.FirstOrDefault(step => step.StepCode == CurrentStepCode);
        }
    }
