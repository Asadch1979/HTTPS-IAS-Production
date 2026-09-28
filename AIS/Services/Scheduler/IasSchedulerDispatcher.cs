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
        private readonly ILogger<IasSchedulerDispatcher> logger;

        public IasSchedulerDispatcher(
            IIasSchedulerStore store,
            IIasSchedulerJobHandlerRegistry registry,
            ILogger<IasSchedulerDispatcher> logger)
        {
            this.store = store;
            this.registry = registry;
            this.logger = logger;
        }

        public async Task<int> RunDueJobsAsync(CancellationToken cancellationToken)
        {
            var processed = 0;

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
            if (!registry.TryGetHandler(job.ApplicationHandler, out var handler))
            {
                var message = $"Application handler '{job.ApplicationHandler}' is not registered for IAS scheduler execution.";
                logger.LogError("IAS scheduler execution {ExecutionId} failed safely. {Message}", job.ExecutionId, message);
                FailClaimedJob(job, message);
                return;
            }

            IasSchedulerJobResult result;
            try
            {
                result = await handler.HandleAsync(job, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "IAS scheduler handler {ApplicationHandler} failed for execution {ExecutionId}.",
                    job.ApplicationHandler, job.ExecutionId);
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

        private void FailClaimedJob(IasSchedulerClaimedJob job, string errorMessage)
        {
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
