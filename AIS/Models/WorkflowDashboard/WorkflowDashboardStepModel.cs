namespace AIS.Models.WorkflowDashboard
    {
    public class WorkflowDashboardStepModel
        {
        public int StepNo { get; set; }
        public string StepKey { get; set; }
        public string StepTitle { get; set; }
        public string PartialViewName { get; set; }
        public string LegacyPath { get; set; }
        public bool IsVisible { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSaved { get; set; } = true;
        public string StatusText { get; set; }
        public int RequiredPermissionPageId { get; set; }
        }
    }
