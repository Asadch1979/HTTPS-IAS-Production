using AIS.Controllers;
using AIS.Models.Scheduler;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIS.Services.Scheduler
{
    /// <summary>
    /// Strictly allowlisted DB executor. Only the known PKG_DAILY entry point
    /// can run; arbitrary names stored in scheduler configuration are rejected
    /// by IasSchedulerDatabaseJobExecutorRegistry.
    /// </summary>
    public sealed class PkgDailyDayEndExecutor : IIasSchedulerDatabaseJobExecutor
    {
        private readonly DBConnection dbConnection;

        public PkgDailyDayEndExecutor(DBConnection dbConnection)
        {
            this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public string PackageName => "PKG_DAILY";
        public string ProcedureName => "SESSION_KILL_DAY_END";

        public Task<IasSchedulerJobResult> ExecuteAsync(
            IasSchedulerClaimedJob job, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (!string.Equals(job.ExecutionType, IasSchedulerExecutionTypes.Database, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(job.PackageName, PackageName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(job.ProcedureName, ProcedureName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Unapproved database job target.");
            }

            // All mutations, logging and commit/rollback occur inside Oracle.
            return Task.FromResult(dbConnection.ExecuteIasSchedulerDayEnd(job.ExecutionId));
        }
    }
}
