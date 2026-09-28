using System;
using System.Collections.Generic;
using System.Linq;

namespace AIS.Services.Scheduler
{
    public sealed class IasSchedulerDatabaseJobExecutorRegistry : IIasSchedulerDatabaseJobExecutorRegistry
    {
        private readonly IReadOnlyDictionary<string, IIasSchedulerDatabaseJobExecutor> executors;

        public IasSchedulerDatabaseJobExecutorRegistry(IEnumerable<IIasSchedulerDatabaseJobExecutor> executors)
        {
            this.executors = (executors ?? Enumerable.Empty<IIasSchedulerDatabaseJobExecutor>())
                .Where(executor => !string.IsNullOrWhiteSpace(executor.PackageName)
                    && !string.IsNullOrWhiteSpace(executor.ProcedureName))
                .ToDictionary(
                    executor => BuildKey(executor.PackageName, executor.ProcedureName),
                    StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetExecutor(string packageName, string procedureName, out IIasSchedulerDatabaseJobExecutor executor)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
            {
                executor = null;
                return false;
            }

            return executors.TryGetValue(BuildKey(packageName, procedureName), out executor);
        }

        private static string BuildKey(string packageName, string procedureName)
        {
            return $"{(packageName ?? string.Empty).Trim()}.{procedureName.Trim()}";
        }
    }
}
