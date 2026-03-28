namespace AIS.Models.Planning
    {
    public class PlanningWorkflowStepModel
        {
        public int StepNo { get; set; }
        public string StepCode { get; set; }
        public string StepTitle { get; set; }
        public string MappedPath { get; set; }
        public string PartialViewName { get; set; }
        public bool IsVisible { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSaved { get; set; }
        public string StatusText { get; set; }
        public int RequiredPermissionPageId { get; set; }
        }
    }
