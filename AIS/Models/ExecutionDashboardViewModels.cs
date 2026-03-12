using System.Collections.Generic;

namespace AIS.Models
    {
    public class ExecutionDashboardViewModel
        {
        public List<ExecutionDashboardStepViewModel> Steps { get; set; } = new List<ExecutionDashboardStepViewModel>();
        }

    public class ExecutionDashboardStepViewModel
        {
        public string StepCode { get; set; } = string.Empty;
        public string StepTitle { get; set; } = string.Empty;
        public int StepNo { get; set; }
        public bool IsReadOnly { get; set; }
        }

    public class ExecutionDashboardEngagementOptionModel
        {
        public int EngId { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        }
    }
