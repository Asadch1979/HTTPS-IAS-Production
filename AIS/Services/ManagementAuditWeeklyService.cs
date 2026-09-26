using AIS.Controllers;
using AIS.Models.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIS.Services
{
    public sealed record ManagementAuditDecision(int DivisionId, string Entity, string Year, string Para,
        string Title, DateTime? Submitted, DateTime Decision,
        string Reason, bool Settled, string Risk = "", bool NoCompliance = false,
        DateTime? LastComplianceSubmitted = null);

    public sealed class ManagementAuditWeeklyService : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly NotificationExecutionStore store;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<ManagementAuditWeeklyService> logger;
        private DateTime? lastObservedReportingStart;
        private DateTimeOffset? lastPeriodCheckUtc;
        public static readonly TimeSpan FailedRetryCheckInterval = TimeSpan.FromMinutes(15);
        public ManagementAuditWeeklyService(IConfiguration configuration, NotificationExecutionStore store,
            IServiceScopeFactory scopeFactory,
            ILogger<ManagementAuditWeeklyService> logger)
        {
            this.configuration = configuration;
            this.store = store;
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        public static DateTime? ReportingStart(DateTime localNow, DayOfWeek day, TimeSpan time)
        {
            var monday = localNow.Date.AddDays(-((7 + (int)localNow.DayOfWeek - (int)DayOfWeek.Monday) % 7));
            var due = monday.AddDays((7 + (int)day - (int)DayOfWeek.Monday) % 7).Add(time);
            return localNow >= due ? monday.AddDays(-7) : null;
        }

        public static bool ShouldCheckPeriod(DateTime? reportingStart, DateTime? lastReportingStart,
            DateTimeOffset? lastCheckUtc, DateTimeOffset nowUtc)
        {
            if (!reportingStart.HasValue)
                return false;
            if (!lastReportingStart.HasValue || reportingStart.Value != lastReportingStart.Value)
                return true;
            return !lastCheckUtc.HasValue || nowUtc - lastCheckUtc.Value >= FailedRetryCheckInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#if DEBUG
            var debugZone = TimeZoneInfo.FindSystemTimeZoneById(
                configuration["ManagementAuditWeekly:TimeZone"] ?? "Asia/Karachi");
            var debugLocalNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, debugZone).DateTime;
            var debugMonday = debugLocalNow.Date.AddDays(
                -((7 + (int)debugLocalNow.DayOfWeek - (int)DayOfWeek.Monday) % 7));

            await RunPeriodAsync(debugMonday, stoppingToken);
            return;
#endif

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
                        var nowUtc = DateTimeOffset.UtcNow;
                        if (ShouldCheckPeriod(start, lastObservedReportingStart, lastPeriodCheckUtc, nowUtc))
                        {
                            lastObservedReportingStart = start.Value;
                            lastPeriodCheckUtc = nowUtc;
                            await RunPeriodAsync(start.Value, stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception e) { logger.LogError(e, "Management Audit weekly processing failed."); }
                try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            }
        }

        public async Task RunPeriodAsync(DateTime reportingStart, CancellationToken cancellationToken)
        {
            var fromDate = reportingStart.Date;
            var toDate = reportingStart.Date.AddDays(7);
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DBConnection>();
            var divisions = db.GetManagementAuditWeeklyDivisions(fromDate, toDate);

            await ProcessDivisionQueueAsync(
                fromDate,
                toDate,
                divisions,
                store.ClaimWeekly,
                divisionId => db.GetManagementAuditWeeklyDivisionData(divisionId, fromDate, toDate),
                divisionId => db.GetManagementAuditWeeklyNoCompliance(divisionId, fromDate, toDate),
                (division, records) => EmailNotification.SendManagementAuditWeeklyAsync(configuration, fromDate, division, records),
                store.Complete,
                (exception, key) => logger.LogError(exception, "Management Audit weekly Division processing failed for {Key}.", key),
                cancellationToken);
        }

        public static async Task ProcessDivisionQueueAsync(
            DateTime fromDate,
            DateTime toDate,
            IReadOnlyList<ManagementAuditWeeklyDivisionSummaryModel> divisions,
            Func<string, bool> claim,
            Func<int, IReadOnlyList<ManagementAuditWeeklyDivisionDetailModel>> getDivisionData,
            Func<ManagementAuditWeeklyDivisionSummaryModel, IReadOnlyList<ManagementAuditDecision>, Task<EmailSendResult>> send,
            Action<string, string, string> complete,
            Action<Exception, string> logFailure,
            CancellationToken cancellationToken)
        {
            await ProcessDivisionQueueAsync(
                fromDate,
                toDate,
                divisions,
                claim,
                getDivisionData,
                _ => Array.Empty<ManagementAuditWeeklyNoComplianceModel>(),
                send,
                complete,
                logFailure,
                cancellationToken);
        }

        public static async Task ProcessDivisionQueueAsync(
            DateTime fromDate,
            DateTime toDate,
            IReadOnlyList<ManagementAuditWeeklyDivisionSummaryModel> divisions,
            Func<string, bool> claim,
            Func<int, IReadOnlyList<ManagementAuditWeeklyDivisionDetailModel>> getDivisionData,
            Func<int, IReadOnlyList<ManagementAuditWeeklyNoComplianceModel>> getNoComplianceData,
            Func<ManagementAuditWeeklyDivisionSummaryModel, IReadOnlyList<ManagementAuditDecision>, Task<EmailSendResult>> send,
            Action<string, string, string> complete,
            Action<Exception, string> logFailure,
            CancellationToken cancellationToken)
        {
            foreach (var division in divisions ?? Array.Empty<ManagementAuditWeeklyDivisionSummaryModel>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var key = $"WEEKLY:{fromDate:yyyyMMdd}:{division.DivisionId}";

                bool claimed;
                try
                {
                    claimed = claim(key);
                }
                catch (Exception e)
                {
                    logFailure(e, key);
                    continue;
                }

                if (!claimed)
                {
                    continue;
                }

                var deliveryStarted = false;
                try
                {
                    var decisionDetails = getDivisionData(division.DivisionId);
                    var noComplianceDetails = getNoComplianceData(division.DivisionId);
                    ValidateDivisionData(division, decisionDetails, noComplianceDetails, fromDate, toDate);
                    var records = AdaptDivisionData(division, decisionDetails)
                        .Concat(AdaptNoComplianceData(division, noComplianceDetails))
                        .ToList();
                    deliveryStarted = true;
                    var result = await send(division, records);
                    var status = result.IsSuccess ? "COMPLETE" : "FAILED";
                    var response = result.ErrorMessage ?? "SMTP accepted the weekly notification.";
                    complete(key, status, response);
                    if (!result.IsSuccess)
                    {
                        logFailure(new InvalidOperationException(response), key);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception e)
                {
                    if (!deliveryStarted)
                    {
                        try
                        {
                            complete(key, "FAILED", e.Message);
                        }
                        catch (Exception trackingException)
                        {
                            logFailure(trackingException, key);
                        }
                    }

                    logFailure(e, key);
                }
            }
        }

        public static void ValidateDivisionData(
            ManagementAuditWeeklyDivisionSummaryModel summary,
            IReadOnlyList<ManagementAuditWeeklyDivisionDetailModel> details,
            DateTime fromDate,
            DateTime toDate)
        {
            ValidateDivisionData(
                summary,
                details,
                Array.Empty<ManagementAuditWeeklyNoComplianceModel>(),
                fromDate,
                toDate);
        }

        public static void ValidateDivisionData(
            ManagementAuditWeeklyDivisionSummaryModel summary,
            IReadOnlyList<ManagementAuditWeeklyDivisionDetailModel> decisionDetails,
            IReadOnlyList<ManagementAuditWeeklyNoComplianceModel> noComplianceDetails,
            DateTime fromDate,
            DateTime toDate)
        {
            if (decisionDetails == null)
                throw new InvalidOperationException("The Division decision dataset is unavailable.");
            if (noComplianceDetails == null)
                throw new InvalidOperationException("The Division no-compliance dataset is unavailable.");
            if (decisionDetails.Count == 0 && noComplianceDetails.Count == 0)
                throw new InvalidOperationException("The Division dataset is empty.");
            if (decisionDetails.GroupBy(item => item.DecisionHistoryId).Any(group => group.Count() > 1))
                throw new InvalidOperationException("The Division dataset contains duplicate decision history IDs.");
            if (decisionDetails.Any(item => item.DivisionId != summary.DivisionId))
                throw new InvalidOperationException("The Division dataset contains records for another Division.");
            if (decisionDetails.Any(item => item.DecisionOn < fromDate || item.DecisionOn >= toDate))
                throw new InvalidOperationException("The Division dataset contains a decision outside the reporting period.");
            if (decisionDetails.Any(item => !IsValidDecisionStatus(item)))
                throw new InvalidOperationException("The Division dataset contains an invalid decision status.");
            if (noComplianceDetails.Any(item => item.DivisionId != summary.DivisionId))
                throw new InvalidOperationException("The Division no-compliance dataset contains records for another Division.");
            if (noComplianceDetails.GroupBy(item => new { item.ComId, item.ComCycle }).Any(group => group.Count() > 1))
                throw new InvalidOperationException("The Division no-compliance dataset contains duplicate compliance records.");
            if (summary.TotalCount != decisionDetails.Count + noComplianceDetails.Count)
                throw new InvalidOperationException("The Division summary total does not match the detail count.");

            var settledCount = decisionDetails.Count(item => item.ComStatus == 16);
            var rejectedCount = decisionDetails.Count(item => item.ComStatus == 12 || item.ComStatus == 15 || item.ComStatus == 18);
            if (summary.SettledCount != settledCount || summary.RejectedCount != rejectedCount ||
                summary.NoComplianceCount != noComplianceDetails.Count)
                throw new InvalidOperationException("The Division summary status counts do not match the detail records.");
        }

        private static bool IsValidDecisionStatus(ManagementAuditWeeklyDivisionDetailModel item)
        {
            if (item.ComStatus == 16)
                return string.Equals(item.DecisionStatus, "SETTLED", StringComparison.OrdinalIgnoreCase);
            if (item.ComStatus == 12 || item.ComStatus == 15 || item.ComStatus == 18)
                return string.Equals(item.DecisionStatus, "REJECTED", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        private static IReadOnlyList<ManagementAuditDecision> AdaptDivisionData(
            ManagementAuditWeeklyDivisionSummaryModel division,
            IReadOnlyList<ManagementAuditWeeklyDivisionDetailModel> details)
        {
            return details.Select(item => new ManagementAuditDecision(
                division.DivisionId,
                item.EntityName,
                item.AuditPeriod,
                item.ParaNo,
                item.Title,
                item.SubmittedOn,
                item.DecisionOn,
                item.Reason,
                item.ComStatus == 16)).ToList();
        }

        private static IReadOnlyList<ManagementAuditDecision> AdaptNoComplianceData(
            ManagementAuditWeeklyDivisionSummaryModel division,
            IReadOnlyList<ManagementAuditWeeklyNoComplianceModel> details)
        {
            return details.Select(item => new ManagementAuditDecision(
                division.DivisionId,
                item.EntityName,
                item.AuditPeriod,
                item.ParaNo,
                item.Title,
                item.LastComplianceSubmittedOn,
                DateTime.MinValue,
                string.Empty,
                false,
                item.Risk,
                true,
                item.LastComplianceSubmittedOn)).ToList();
        }
    }
}
