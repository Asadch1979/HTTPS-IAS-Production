using AIS.Controllers;
using AIS.Models.Scheduler;

namespace AIS.Services.Scheduler
{
    public sealed class DbConnectionIasSchedulerStore : IIasSchedulerStore
    {
        private readonly DBConnection dbConnection;

        public DbConnectionIasSchedulerStore(DBConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public IasSchedulerClaimedJob ClaimDueJob()
        {
            return dbConnection.ClaimIasSchedulerDueJob();
        }

        public void CompleteJob(long executionId, int recordsProcessed, string responseMessage)
        {
            dbConnection.CompleteIasSchedulerJob(executionId, recordsProcessed, responseMessage);
        }

        public void FailJob(long executionId, string errorMessage)
        {
            dbConnection.FailIasSchedulerJob(executionId, errorMessage);
        }

        public int FlagStaleExecutions(int staleAfterMinutes, string updatedBy)
        {
            return dbConnection.FlagStaleIasSchedulerExecutions(staleAfterMinutes, updatedBy);
        }
    }
}
