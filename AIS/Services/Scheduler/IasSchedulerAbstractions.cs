using AIS.Models.Scheduler;
using System.Threading;
using System.Threading.Tasks;

namespace AIS.Services.Scheduler
{
    public interface IIasSchedulerStore
    {
        IasSchedulerClaimedJob ClaimDueJob();
        void CompleteJob(long executionId, int recordsProcessed, string responseMessage);
        void FailJob(long executionId, string errorMessage);
    }

    public interface IIasSchedulerJobHandler
    {
        string ApplicationHandler { get; }
        Task<IasSchedulerJobResult> HandleAsync(IasSchedulerClaimedJob job, CancellationToken cancellationToken);
    }

    public interface IIasSchedulerJobHandlerRegistry
    {
        bool TryGetHandler(string applicationHandler, out IIasSchedulerJobHandler handler);
    }
}
