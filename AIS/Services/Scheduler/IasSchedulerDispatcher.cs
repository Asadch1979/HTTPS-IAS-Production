using AIS.Models.Scheduler;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIS.Services.Scheduler
{
    public sealed class IasSchedulerDispatcher
    {
        private readonly IIasSchedulerStore store;
        private readonly IIasSchedulerJobHandlerRegistry registry;
        private readonly IIasSchedulerDatabaseJobExecutorRegistry databaseRegistry;
        private readonly ILogger<IasSchedulerDispatcher> logger;

        public IasSchedulerDispatcher(
            IIasSchedulerStore store,
            IIasSchedulerJobHandlerRegistry registry,
            IIasSchedulerDatabaseJobExecutorRegistry databaseRegistry,
            ILogger<IasSchedulerDispatcher> logger)
        {
            this.store = store;
            this.registry = registry;
            this.databaseRegistry = databaseRegistry;
            this.logger = logger;
        }

        public async Task<int> RunDueJobsAsync(CancellationToken cancellationToken, bool flagStaleExecutions = false, int staleAfterMinutes = 180)
        {
            var processed = 0;
            if (flagStaleExecutions)
            {
                FlagStaleExecutions(staleAfterMinutes);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var job = store.ClaimDueJob();
                if (job == null)
                {
                    break;
                }

                await DispatchClaimedJobAsync(job, cancellationToken);
                processed++;
            }

            return processed;
        }

        private async Task DispatchClaimedJobAsync(IasSchedulerClaimedJob job, CancellationToken cancellationToken)
        {
            if (string.Equals(job.ExecutionType, IasSchedulerExecutionTypes.Database, StringComparison.OrdinalIgnoreCase))
            {
                await DispatchDatabaseJobAsync(job, cancellationToken);
                return;
            }

            if (!string.Equals(job.ExecutionType, IasSchedulerExecutionTypes.Application, StringComparison.OrdinalIgnoreCase))
            {
                FailClaimedJob(job, $"Unsupported IAS scheduler execution type '{job.ExecutionType}'.");
                return;
            }

            if (!registry.TryGetHandler(job.ApplicationHandler, out var handler))
            {
                FailClaimedJob(job, $"Application handler '{job.ApplicationHandler}' is not registered for IAS scheduler execution.");
                return;
            }

            await RunHandlerAsync(job, () => handler.HandleAsync(job, cancellationToken), cancellationToken);
        }

        private async Task DispatchDatabaseJobAsync(IasSchedulerClaimedJob job, CancellationToken cancellationToken)
        {
            if (!databaseRegistry.TryGetExecutor(job.PackageName, job.ProcedureName, out var executor))
            {
                FailClaimedJob(job, $"Database job '{job.PackageName}.{job.ProcedureName}' is not registered for IAS scheduler execution.");
                return;
            }

            await RunHandlerAsync(job, () => executor.ExecuteAsync(job, cancellationToken), cancellationToken);
        }

        private async Task RunHandlerAsync(
            IasSchedulerClaimedJob job,
            Func<Task<IasSchedulerJobResult>> handle,
            CancellationToken cancellationToken)
        {
            IasSchedulerJobResult result;
            try
            {
                result = await handle();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "IAS scheduler handler failed for execution {ExecutionId}.", job.ExecutionId);
                FailClaimedJob(job, exception.Message);
                return;
            }

            try
            {
                store.CompleteJob(job.ExecutionId, result?.RecordsProcessed ?? 0, result?.ResponseMessage ?? string.Empty);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "IAS scheduler could not complete execution {ExecutionId} after handler success.", job.ExecutionId);
            }
        }

        private void FlagStaleExecutions(int staleAfterMinutes)
        {
            try
            {
                var flagged = store.FlagStaleExecutions(staleAfterMinutes, "IAS_ASPNET_SCHEDULER");
                if (flagged > 0)
                {
                    logger.LogWarning("IAS scheduler flagged {FlaggedCount} stale RUNNING execution(s) for reconciliation.", flagged);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "IAS scheduler could not flag stale RUNNING executions.");
            }
        }

        private void FailClaimedJob(IasSchedulerClaimedJob job, string errorMessage)
        {
            logger.LogError("IAS scheduler execution {ExecutionId} failed safely. {Message}", job.ExecutionId, errorMessage);
            try
            {
                store.FailJob(job.ExecutionId, errorMessage);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "IAS scheduler could not mark execution {ExecutionId} as failed.", job.ExecutionId);
            }
        }
    }
}
