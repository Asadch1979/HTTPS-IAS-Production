using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIS.Services.Scheduler
{
    public sealed class IasSchedulerBackgroundService : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory scopeFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<IasSchedulerBackgroundService> logger;

        public IasSchedulerBackgroundService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<IasSchedulerBackgroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.configuration = configuration;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var dispatcher = scope.ServiceProvider.GetRequiredService<IasSchedulerDispatcher>();
                    var staleMinutes = Math.Max(5, configuration.GetValue("IasScheduler:StaleExecutionMinutes", 180));
                    await dispatcher.RunDueJobsAsync(stoppingToken, flagStaleExecutions: true, staleAfterMinutes: staleMinutes);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "IAS scheduler polling cycle failed.");
                }

                try
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
