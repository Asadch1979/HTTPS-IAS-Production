using AIS;
using AIS.Models.Notifications;
using AIS.Models.Scheduler;
using AIS.Services;
using AIS.Services.Scheduler;
using Microsoft.Extensions.Logging;
using System.Reflection;

var tests = new (string Name, Func<Task> Run)[]
{
    ("Empty due queue", EmptyDueQueueAsync),
    ("Successful claim dispatch complete", SuccessfulClaimDispatchCompleteAsync),
    ("Failure calls fail job", FailureCallsFailJobAsync),
    ("Multiple due jobs processed sequentially", MultipleDueJobsSequentialAsync),
    ("Manual run now jobs are dispatched", ManualRunNowDispatchedAsync),
    ("Registered database job completes", RegisteredDatabaseJobCompletesAsync),
    ("Unknown database job fails safely", UnknownDatabaseJobFailsSafelyAsync),
    ("Stale running executions are flagged", StaleRunningExecutionsFlaggedAsync),
    ("Explicit period passed to handler", ExplicitPeriodPassedToHandlerAsync),
    ("Unknown handler fails safely", UnknownHandlerFailsSafelyAsync),
    ("No legacy autonomous Management Audit trigger", NoLegacyAutonomousTriggerAsync),
    ("Management Audit email uses Compliance Review period", ManagementAuditEmailWordingAsync)
};

foreach (var test in tests)
{
    await test.Run();
    Console.WriteLine($"PASS {test.Name}");
}

Console.WriteLine($"{tests.Length} scheduler integration tests passed.");

static async Task EmptyDueQueueAsync()
{
    var store = new FakeSchedulerStore();
    var dispatcher = NewDispatcher(store, new RecordingHandler("APP_HANDLER"));

    var processed = await dispatcher.RunDueJobsAsync(CancellationToken.None);

    AssertEqual(0, processed, "processed count");
    AssertEqual(0, store.Completed.Count, "complete calls");
    AssertEqual(0, store.Failed.Count, "fail calls");
}

static async Task SuccessfulClaimDispatchCompleteAsync()
{
    var store = new FakeSchedulerStore(Job(1, "APP_HANDLER"));
    var handler = new RecordingHandler("APP_HANDLER", new IasSchedulerJobResult(3, "ok"));
    var dispatcher = NewDispatcher(store, handler);

    var processed = await dispatcher.RunDueJobsAsync(CancellationToken.None);

    AssertEqual(1, processed, "processed count");
    AssertEqual(1, handler.Seen.Count, "handler call count");
    AssertEqual(1, store.Completed.Count, "complete calls");
    AssertEqual(1L, store.Completed[0].ExecutionId, "completed execution");
    AssertEqual(3, store.Completed[0].RecordsProcessed, "records processed");
    AssertEqual(0, store.Failed.Count, "fail calls");
}

static async Task FailureCallsFailJobAsync()
{
    var store = new FakeSchedulerStore(Job(2, "APP_HANDLER"));
    var handler = new RecordingHandler("APP_HANDLER", exception: new InvalidOperationException("broken"));
    var dispatcher = NewDispatcher(store, handler);

    var processed = await dispatcher.RunDueJobsAsync(CancellationToken.None);

    AssertEqual(1, processed, "processed count");
    AssertEqual(0, store.Completed.Count, "complete calls");
    AssertEqual(1, store.Failed.Count, "fail calls");
    AssertEqual(2L, store.Failed[0].ExecutionId, "failed execution");
    AssertContains("broken", store.Failed[0].ErrorMessage, "failure message");
}

static async Task MultipleDueJobsSequentialAsync()
{
    var store = new FakeSchedulerStore(Job(3, "APP_HANDLER"), Job(4, "APP_HANDLER"), Job(5, "APP_HANDLER"));
    var handler = new RecordingHandler("APP_HANDLER");
    var dispatcher = NewDispatcher(store, handler);

    var processed = await dispatcher.RunDueJobsAsync(CancellationToken.None);

    AssertEqual(3, processed, "processed count");
    AssertEqual("3,4,5", string.Join(",", handler.Seen.Select(job => job.ExecutionId)), "handler order");
    AssertEqual("3,4,5", string.Join(",", store.Completed.Select(job => job.ExecutionId)), "complete order");
}

static async Task ManualRunNowDispatchedAsync()
{
    var store = new FakeSchedulerStore(Job(8, "APP_HANDLER", runSource: "MANUAL", runRequestId: 501));
    var handler = new RecordingHandler("APP_HANDLER");
    var dispatcher = NewDispatcher(store, handler);

    var processed = await dispatcher.RunDueJobsAsync(CancellationToken.None);

    AssertEqual(1, processed, "processed count");
    AssertEqual("MANUAL", handler.Seen[0].RunSource, "run source");
    AssertEqual(501L, handler.Seen[0].RunRequestId.Value, "run request id");
    AssertEqual(1, store.Completed.Count, "complete calls");
}

static async Task RegisteredDatabaseJobCompletesAsync()
{
    var store = new FakeSchedulerStore(DatabaseJob(9, "PKG_APPROVED", "P_APPROVED"));
    var executor = new RecordingDatabaseExecutor("PKG_APPROVED", "P_APPROVED", new IasSchedulerJobResult(4, "database ok"));
    var dispatcher = NewDispatcherWithDatabase(store, Array.Empty<IIasSchedulerJobHandler>(), executor);

    var processed = await dispatcher.RunDueJobsAsync(CancellationToken.None);

    AssertEqual(1, processed, "processed count");
    AssertEqual(1, executor.Seen.Count, "executor call count");
    AssertEqual(1, store.Completed.Count, "complete calls");
    AssertEqual(4, store.Completed[0].RecordsProcessed, "records processed");
    AssertEqual(0, store.Failed.Count, "fail calls");
}

static async Task UnknownDatabaseJobFailsSafelyAsync()
{
    var store = new FakeSchedulerStore(DatabaseJob(10, "PKG_NOT_APPROVED", "P_NOT_APPROVED"));
    var dispatcher = NewDispatcher(store, Array.Empty<IIasSchedulerJobHandler>());

    await dispatcher.RunDueJobsAsync(CancellationToken.None);

    AssertEqual(0, store.Completed.Count, "complete calls");
    AssertEqual(1, store.Failed.Count, "fail calls");
    AssertContains("not registered", store.Failed[0].ErrorMessage, "database target message");
}

static async Task StaleRunningExecutionsFlaggedAsync()
{
    var store = new FakeSchedulerStore();
    var dispatcher = NewDispatcher(store, new RecordingHandler("APP_HANDLER"));

    await dispatcher.RunDueJobsAsync(CancellationToken.None, flagStaleExecutions: true, staleAfterMinutes: 42);

    AssertEqual(1, store.FlagStaleCalls.Count, "flag stale calls");
    AssertEqual(42, store.FlagStaleCalls[0].StaleAfterMinutes, "stale minutes");
}

static async Task ExplicitPeriodPassedToHandlerAsync()
{
    var from = new DateTime(2026, 8, 1);
    var toExclusive = new DateTime(2026, 9, 1);
    var store = new FakeSchedulerStore(Job(6, "APP_HANDLER", from, toExclusive));
    var handler = new RecordingHandler("APP_HANDLER");
    var dispatcher = NewDispatcher(store, handler);

    await dispatcher.RunDueJobsAsync(CancellationToken.None);

    AssertEqual(from, handler.Seen[0].PeriodFrom.Value, "period from");
    AssertEqual(toExclusive, handler.Seen[0].PeriodTo.Value, "period to");
}

static async Task UnknownHandlerFailsSafelyAsync()
{
    var store = new FakeSchedulerStore(Job(7, "UNKNOWN_HANDLER"));
    var dispatcher = NewDispatcher(store, new RecordingHandler("APP_HANDLER"));

    await dispatcher.RunDueJobsAsync(CancellationToken.None);

    AssertEqual(0, store.Completed.Count, "complete calls");
    AssertEqual(1, store.Failed.Count, "fail calls");
    AssertContains("not registered", store.Failed[0].ErrorMessage, "unknown handler message");
}

static Task NoLegacyAutonomousTriggerAsync()
{
    AssertFalse(typeof(Microsoft.Extensions.Hosting.BackgroundService).IsAssignableFrom(typeof(ManagementAuditWeeklyService)),
        "ManagementAuditWeeklyService must not be a hosted BackgroundService");
    AssertNull(typeof(ManagementAuditWeeklyService).GetMethod("ReportingStart", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
        "legacy ReportingStart method");
    AssertNull(typeof(ManagementAuditWeeklyService).GetMethod("ShouldCheckPeriod", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
        "legacy ShouldCheckPeriod method");
    AssertNull(typeof(ManagementAuditWeeklyService).GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.NonPublic),
        "legacy ExecuteAsync method");
    return Task.CompletedTask;
}

static Task ManagementAuditEmailWordingAsync()
{
    var request = EmailNotification.BuildManagementAuditComplianceReviewEmail(
        new DateTime(2026, 8, 1),
        new DateTime(2026, 9, 1),
        new ManagementAuditWeeklyDivisionSummaryModel { DivisionId = 10, DivisionName = "Operations", ToEmail = "to@example.com" },
        new[]
        {
            new ManagementAuditDecision(10, "Ops", "2026", "1", "Title", new DateTime(2026, 8, 5), new DateTime(2026, 8, 10), "", true)
        });

    AssertContains("Management Audit Compliance Review", request.Subject, "subject");
    AssertContains("01-Aug-2026 to 31-Aug-2026", request.Subject, "inclusive displayed period");
    AssertContains("Compliance Review Period", request.Body, "period label");
    AssertFalse(request.Subject.Contains("Weekly", StringComparison.OrdinalIgnoreCase), "subject should not say Weekly");
    AssertFalse(request.Body.Contains("Reporting Period", StringComparison.OrdinalIgnoreCase), "body should not say Reporting Period");
    AssertFalse(request.Body.Contains("Weekly", StringComparison.OrdinalIgnoreCase), "body should not say Weekly");
    return Task.CompletedTask;
}

static IasSchedulerDispatcher NewDispatcherWithDatabase(
    FakeSchedulerStore store,
    IEnumerable<IIasSchedulerJobHandler> handlers,
    params IIasSchedulerDatabaseJobExecutor[] databaseExecutors)
{
    return new IasSchedulerDispatcher(
        store,
        new IasSchedulerJobHandlerRegistry(handlers),
        new IasSchedulerDatabaseJobExecutorRegistry(databaseExecutors),
        new TestLogger<IasSchedulerDispatcher>());
}

static IasSchedulerDispatcher NewDispatcher(FakeSchedulerStore store, params IIasSchedulerJobHandler[] handlers)
{
    return NewDispatcherWithDatabase(store, handlers, Array.Empty<IIasSchedulerDatabaseJobExecutor>());
}

static IasSchedulerClaimedJob Job(
    long executionId,
    string handler,
    DateTime? periodFrom = null,
    DateTime? periodTo = null,
    string runSource = "SCHEDULED",
    long? runRequestId = null)
{
    return new IasSchedulerClaimedJob
    {
        ExecutionId = executionId,
        ScheduleId = executionId + 100,
        JobId = executionId + 200,
        JobCode = $"JOB_{executionId}",
        JobName = $"Job {executionId}",
        ExecutionType = "APPLICATION",
        ApplicationHandler = handler,
        ExecutionKey = $"SCH:{executionId}",
        ScheduledFor = new DateTime(2026, 8, 3, 7, 0, 0),
        PeriodFrom = periodFrom ?? new DateTime(2026, 8, 3),
        PeriodTo = periodTo ?? new DateTime(2026, 8, 10),
        RetryNo = 0,
        RunSource = runSource,
        RunRequestId = runRequestId
    };
}

static IasSchedulerClaimedJob DatabaseJob(long executionId, string packageName, string procedureName)
{
    var job = Job(executionId, string.Empty);
    job.ExecutionType = IasSchedulerExecutionTypes.Database;
    job.PackageName = packageName;
    job.ProcedureName = procedureName;
    return job;
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"{message}: expected {expected}, got {actual}.");
}

static void AssertContains(string expected, string actual, string message)
{
    if (actual == null || !actual.Contains(expected, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"{message}: expected to contain '{expected}'.");
}

static void AssertFalse(bool condition, string message)
{
    if (condition)
        throw new InvalidOperationException(message);
}

static void AssertNull(object value, string message)
{
    if (value != null)
        throw new InvalidOperationException($"{message}: expected null.");
}

sealed class FakeSchedulerStore : IIasSchedulerStore
{
    private readonly Queue<IasSchedulerClaimedJob> jobs;

    public FakeSchedulerStore(params IasSchedulerClaimedJob[] jobs)
    {
        this.jobs = new Queue<IasSchedulerClaimedJob>(jobs);
    }

    public List<(long ExecutionId, int RecordsProcessed, string ResponseMessage)> Completed { get; } = new();
    public List<(long ExecutionId, string ErrorMessage)> Failed { get; } = new();
    public List<(int StaleAfterMinutes, string UpdatedBy)> FlagStaleCalls { get; } = new();

    public IasSchedulerClaimedJob ClaimDueJob()
    {
        return jobs.Count == 0 ? null : jobs.Dequeue();
    }

    public void CompleteJob(long executionId, int recordsProcessed, string responseMessage)
    {
        Completed.Add((executionId, recordsProcessed, responseMessage));
    }

    public void FailJob(long executionId, string errorMessage)
    {
        Failed.Add((executionId, errorMessage));
    }

    public int FlagStaleExecutions(int staleAfterMinutes, string updatedBy)
    {
        FlagStaleCalls.Add((staleAfterMinutes, updatedBy));
        return 0;
    }
}

sealed class RecordingHandler : IIasSchedulerJobHandler
{
    private readonly IasSchedulerJobResult result;
    private readonly Exception exception;

    public RecordingHandler(string applicationHandler, IasSchedulerJobResult result = null, Exception exception = null)
    {
        ApplicationHandler = applicationHandler;
        this.result = result ?? new IasSchedulerJobResult(1, "ok");
        this.exception = exception;
    }

    public string ApplicationHandler { get; }
    public List<IasSchedulerClaimedJob> Seen { get; } = new();

    public Task<IasSchedulerJobResult> HandleAsync(IasSchedulerClaimedJob job, CancellationToken cancellationToken)
    {
        Seen.Add(job);
        if (exception != null)
            throw exception;
        return Task.FromResult(result);
    }
}

sealed class RecordingDatabaseExecutor : IIasSchedulerDatabaseJobExecutor
{
    private readonly IasSchedulerJobResult result;

    public RecordingDatabaseExecutor(string packageName, string procedureName, IasSchedulerJobResult result = null)
    {
        PackageName = packageName;
        ProcedureName = procedureName;
        this.result = result ?? new IasSchedulerJobResult(1, "database ok");
    }

    public string PackageName { get; }
    public string ProcedureName { get; }
    public List<IasSchedulerClaimedJob> Seen { get; } = new();

    public Task<IasSchedulerJobResult> ExecuteAsync(IasSchedulerClaimedJob job, CancellationToken cancellationToken)
    {
        Seen.Add(job);
        return Task.FromResult(result);
    }
}

sealed class TestLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();
        public void Dispose() { }
    }
}
