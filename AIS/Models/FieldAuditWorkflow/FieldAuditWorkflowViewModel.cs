using System.Collections.Generic;
using System.Linq;

namespace AIS.Models.FieldAuditWorkflow
    {
    public class FieldAuditWorkflowViewModel
        {
        public int? SelectedEngagementId { get; set; }
        public List<FieldAuditEngagementOptionModel> AvailableEngagements { get; set; } = new List<FieldAuditEngagementOptionModel>();
        public string CurrentStepCode { get; set; }
        public List<FieldAuditWorkflowStepModel> Steps { get; set; } = new List<FieldAuditWorkflowStepModel>();
        public FieldAuditStepContextViewModel CurrentStepContext { get; set; }
        public List<TaskListModel> WorkbenchTasks { get; set; } = new List<TaskListModel>();
        // Null until the Oracle read-only role-dashboard rollout is enabled.
        public AIS.Models.TeamLeadDashboardSnapshot TeamLeadSnapshot { get; set; }

        public IEnumerable<FieldAuditWorkflowStepModel> VisibleSteps => Steps.Where(step => step.IsVisible);
        public FieldAuditWorkflowStepModel CurrentStep => VisibleSteps.FirstOrDefault(step => step.StepCode == CurrentStepCode);
        public bool HasEngagementSelection => SelectedEngagementId.GetValueOrDefault() > 0;
        }
    }
