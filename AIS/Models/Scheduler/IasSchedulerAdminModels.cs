using System;
using System.Collections.Generic;

namespace AIS.Models.Scheduler
{
    public sealed class IasSchedulerAdminViewModel
    {
        public List<IasSchedulerSchedule> Schedules { get; set; } = new();
        public List<IasSchedulerJobDefinition> JobDefinitions { get; set; } = new();
        public List<IasSchedulerExecutionHistory> ExecutionHistory { get; set; } = new();
        public List<IasSchedulerRunRequest> RunRequests { get; set; } = new();
        public List<IasSchedulerReconciliationExecution> ReconciliationRequired { get; set; } = new();
    }

    public sealed class IasSchedulerRunNowRequest
    {
        public long ScheduleId { get; set; }
        public string Mode { get; set; } = "AUTO";
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodToInclusive { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public sealed class IasSchedulerResolveExecutionRequest
    {
        public long ExecutionId { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public int? RecordsProcessed { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
