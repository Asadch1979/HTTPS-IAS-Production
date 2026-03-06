using AIS.Models;
using AIS.Models.FieldAuditReport;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Models.FieldAuditWorkflow
{
    public class FieldAuditWorkflowViewModel
    {
        public int? SelectedEngagementId { get; set; }
        public string CurrentStepCode { get; set; }
        public List<TaskListModel> AvailableEngagements { get; set; } = new List<TaskListModel>();
        public List<FieldAuditWorkflowStepModel> Steps { get; set; } = new List<FieldAuditWorkflowStepModel>();
        public FieldAuditReportOverviewModel ReportOverview { get; set; }
        public FieldAuditReportChecklistModel ReportChecklist { get; set; }

        public IEnumerable<FieldAuditWorkflowStepModel> VisibleSteps => Steps.Where(step => step.IsVisible);
        public FieldAuditWorkflowStepModel CurrentStep => VisibleSteps.FirstOrDefault(step => step.StepCode == CurrentStepCode);
        public bool HasEngagementSelected => SelectedEngagementId.HasValue && SelectedEngagementId.Value > 0;
    }
}
