using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace AIS.Services
{
    public sealed record ManagementAuditDecision(int DivisionId, string Division, string To, string Cc,
        string Entity, string Year, string Para, string Title, DateTime? Submitted, DateTime Decision,
        string Reason, bool Settled);

    public sealed class ManagementAuditWeeklyService : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly NotificationExecutionStore store;
        private readonly ILogger<ManagementAuditWeeklyService> logger;
        private DateTime? lastObservedReportingStart;
        public ManagementAuditWeeklyService(IConfiguration configuration, NotificationExecutionStore store,
            ILogger<ManagementAuditWeeklyService> logger)
        { this.configuration = configuration; this.store = store; this.logger = logger; }

        public static DateTime? ReportingStart(DateTime localNow, DayOfWeek day, TimeSpan time)
        {
            var monday = localNow.Date.AddDays(-((7 + (int)localNow.DayOfWeek - (int)DayOfWeek.Monday) % 7));
            var due = monday.AddDays((7 + (int)day - (int)DayOfWeek.Monday) % 7).Add(time);
            return localNow >= due ? monday.AddDays(-7) : null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (configuration.GetValue("ManagementAuditWeekly:Enabled", false))
                    {
                        var zone = TimeZoneInfo.FindSystemTimeZoneById(configuration["ManagementAuditWeekly:TimeZone"] ?? "Asia/Karachi");
                        var day = Enum.Parse<DayOfWeek>(configuration["ManagementAuditWeekly:Day"] ?? "Monday", true);
                        var time = TimeSpan.Parse(configuration["ManagementAuditWeekly:Time"] ?? "07:00", System.Globalization.CultureInfo.InvariantCulture);
                        if (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1)) throw new InvalidOperationException("Weekly time must be within one day.");
                        var start = ReportingStart(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime, day, time);
                        if (start.HasValue && start.Value != lastObservedReportingStart)
                        {
                            await RunPeriodAsync(start.Value, stoppingToken);
                            lastObservedReportingStart = start.Value;
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception e) { logger.LogError(e, "Management Audit weekly processing failed."); }
                try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            }
        }

        public async Task RunPeriodAsync(DateTime start, CancellationToken cancellationToken)
        {
            var rows = new List<ManagementAuditDecision>();
            using (var connection = store.Open())
            using (var command = connection.CreateCommand())
            {
                command.BindByName = true;
                command.CommandText = @"SELECT DIVISION_ID,DIVISION_NAME,DIVISION_EMAIL,REPORTING_EMAIL,
                    ENTITY_NAME,AUDIT_PERIOD,PARA_NO,TITLE,SUBMITTED_ON,DECISION_ON,REASON,COM_STATUS
                    FROM V_IAS_MGMT_WEEKLY_DATA WHERE DECISION_ON>=:d1 AND DECISION_ON<:d2
                    ORDER BY DIVISION_ID,DECISION_ON,HIST_ID";
                command.Parameters.Add("d1", OracleDbType.Date).Value = start.Date;
                command.Parameters.Add("d2", OracleDbType.Date).Value = start.Date.AddDays(7);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    rows.Add(new ManagementAuditDecision(Convert.ToInt32(reader.GetValue(0)), reader[1].ToString(),
                        reader[2].ToString(), reader[3].ToString(), reader[4].ToString(), reader[5].ToString(),
                        reader[6].ToString(), reader[7].ToString(), reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                        reader.GetDateTime(9), reader[10].ToString(), Convert.ToInt32(reader.GetValue(11)) == 16));
            }
            foreach (var division in rows.GroupBy(x => x.DivisionId))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var key = $"WEEKLY:{start:yyyyMMdd}:{division.Key}";
                if (!store.ClaimWeekly(key)) continue;
                var records = division.ToList();
                try
                {
                    var result = await EmailNotification.SendManagementAuditWeeklyAsync(configuration, start, records);
                    store.Complete(key, result.IsSuccess ? "COMPLETE" : "FAILED", result.ErrorMessage ?? "SMTP accepted the weekly notification.");
                    if (!result.IsSuccess) logger.LogError("Weekly SMTP failure for {Key}: {Error}", key, result.ErrorMessage);
                    else logger.LogInformation("Weekly notification sent: {Key}", key);
                }
                catch (Exception e)
                {
                    // A crash after SMTP acceptance cannot safely be retried automatically.
                    logger.LogError(e, "Weekly delivery outcome requires reconciliation: {Key}", key);
                }
            }
        }
    }
}
