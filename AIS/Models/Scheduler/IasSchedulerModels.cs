using System;

namespace AIS.Models.Scheduler
{
    public static class IasSchedulerExecutionTypes
    {
        public const string Database = "DATABASE";
        public const string Application = "APPLICATION";
    }

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
        public string RunSource { get; set; } = "SCHEDULED";
        public long? RunRequestId { get; set; }
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

    public sealed class IasSchedulerJobDefinition
    {
        public long JobId { get; set; }
        public string JobCode { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string ExecutionType { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string ApplicationHandler { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedOn { get; set; }
        public int ScheduleCount { get; set; }
    }

    public sealed class IasSchedulerSchedule
    {
        public long ScheduleId { get; set; }
        public long JobId { get; set; }
        public string JobCode { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string ExecutionType { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string ApplicationHandler { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FrequencyType { get; set; } = string.Empty;
        public int RunHour { get; set; }
        public int RunMinute { get; set; }
        public int? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public string PeriodType { get; set; } = string.Empty;
        public bool EmailRequired { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime? LastRunOn { get; set; }
        public string LastStatus { get; set; } = string.Empty;
        public DateTime? LastPeriodFrom { get; set; }
        public DateTime? LastPeriodTo { get; set; }
        public DateTime? NextDueOn { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetry { get; set; }
        public int RetryIntervalMinutes { get; set; }
        public string LastError { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedOn { get; set; }
        public bool IsDue { get; set; }
        public int OpenRunRequests { get; set; }
    }

    public sealed class IasSchedulerExecutionHistory
    {
        public long ExecutionId { get; set; }
        public long ScheduleId { get; set; }
        public long JobId { get; set; }
        public string JobCode { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string ExecutionType { get; set; } = string.Empty;
        public string ExecutionKey { get; set; } = string.Empty;
        public DateTime? ScheduledFor { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public int RetryNo { get; set; }
        public int? RecordsProcessed { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; }
        public string RunSource { get; set; } = string.Empty;
        public long? RunRequestId { get; set; }
    }

    public sealed class IasSchedulerRunRequest
    {
        public long RequestId { get; set; }
        public long RunRequestId { get; set; }
        public long ScheduleId { get; set; }
        public long JobId { get; set; }
        public string JobCode { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime? RequestedOn { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedOn { get; set; }
        public string RequestReason { get; set; } = string.Empty;
        public string CancelledBy { get; set; } = string.Empty;
        public DateTime? CancelledOn { get; set; }
        public string CancelReason { get; set; } = string.Empty;
        public long? ExecutionId { get; set; }
        public DateTime? ClaimedOn { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public sealed class IasSchedulerReconciliationExecution
    {
        public long ExecutionId { get; set; }
        public long ScheduleId { get; set; }
        public long JobId { get; set; }
        public string JobCode { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string ExecutionKey { get; set; } = string.Empty;
        public DateTime? ScheduledFor { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedOn { get; set; }
        public int RetryNo { get; set; }
        public string RunSource { get; set; } = string.Empty;
        public long? RunRequestId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string LastError { get; set; } = string.Empty;
    }

    public sealed class SaveIasSchedulerJobDefinitionRequest
    {
        public long? JobId { get; set; }
        public string JobCode { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string ExecutionType { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string ApplicationHandler { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public sealed class SaveIasSchedulerScheduleRequest
    {
        public long? ScheduleId { get; set; }
        public long JobId { get; set; }
        public string FrequencyType { get; set; } = string.Empty;
        public int RunHour { get; set; }
        public int RunMinute { get; set; }
        public int? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public string PeriodType { get; set; } = string.Empty;
        public bool EmailRequired { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int MaxRetry { get; set; }
        public int RetryIntervalMinutes { get; set; }
        public DateTime? NextDueOn { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
