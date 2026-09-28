using System;

namespace AIS.Models.Scheduler
{
    public sealed class IasSchedulerClaimedJob
    {
        public long ExecutionId { get; set; }
        public long ScheduleId { get; set; }
        public long JobId { get; set; }
        public string JobCode { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string ExecutionType { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string ApplicationHandler { get; set; } = string.Empty;
        public bool EmailRequired { get; set; }
        public string ExecutionKey { get; set; } = string.Empty;
        public DateTime ScheduledFor { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public int RetryNo { get; set; }
    }

    public sealed class IasSchedulerJobResult
    {
        public IasSchedulerJobResult(int recordsProcessed, string responseMessage)
        {
            RecordsProcessed = recordsProcessed;
            ResponseMessage = responseMessage ?? string.Empty;
        }

        public int RecordsProcessed { get; }
        public string ResponseMessage { get; }
    }
}
