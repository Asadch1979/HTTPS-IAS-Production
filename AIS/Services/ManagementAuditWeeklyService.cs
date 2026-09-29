using AIS.Controllers;
using AIS.Models.Notifications;
using AIS.Models.Scheduler;
using AIS.Services.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIS.Services
{
    public sealed record ManagementAuditDecision(int DivisionId, string Entity, string Year, string Para,
        string Title, DateTime? Submitted, DateTime Decision,
        string Reason, bool Settled, string Risk = "", bool NoCompliance = false,
        DateTime? LastComplianceSubmitted = null);

    public sealed class ManagementAuditWeeklyService : IIasSchedulerJobHandler
    {
        public const string HandlerName = "MGMT_AUDIT_COMPLIANCE_NOTIFICATION";

        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;
        private readonly NotificationExecutionStore store;
        private readonly DBConnection db;
        private readonly ILogger<ManagementAuditWeeklyService> logger;

        public ManagementAuditWeeklyService(
            IConfiguration configuration,
            NotificationExecutionStore store,
            DBConnection db,
            ILogger<ManagementAuditWeeklyService> logger)
        {
            this.configuration = configuration;
            this.store = store;
            this.db = db;
            this.logger = logger;
        }

        public string ApplicationHandler => HandlerName;

        public async Task<IasSchedulerJobResult> HandleAsync(IasSchedulerClaimedJob job, CancellationToken cancellationToken)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            if (!job.PeriodFrom.HasValue || !job.PeriodTo.HasValue)
                throw new InvalidOperationException("Management Audit Compliance Review requires PERIOD_FROM and PERIOD_TO from PKG_IAS_SCHEDULER.");

            var result = await RunPeriodAsync(job.PeriodFrom.Value, job.PeriodTo.Value, cancellationToken);
            if (result.FailedDivisions > 0)
                throw new InvalidOperationException($"Management Audit Compliance Review failed for {result.FailedDivisions} Division(s).");

            return new IasSchedulerJobResult(
                result.RecordsProcessed,
                $"Management Audit Compliance Review completed. Divisions={result.CompletedDivisions}; Records={result.RecordsProcessed}.");
        }

        public async Task<ManagementAuditProcessingResult> RunPeriodAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken)
        {
            fromDate = fromDate.Date;
            toDate = toDate.Date;
            if (toDate <= fromDate)
                throw new InvalidOperationException("Management Audit Compliance Review PERIOD_TO must be later than PERIOD_FROM.");

            var divisions = db.GetManagementAuditWeeklyDivisions(fromDate, toDate);

            return await ProcessDivisionQueueAsync(
                fromDate,
                toDate,
                divisions,
                key => store.ClaimPeriod(key, "MGMT_AUDIT_COMPLIANCE_NOTIFICATION"),
                divisionId => db.GetManagementAuditWeeklyDivisionData(divisionId, fromDate, toDate),
                divisionId => db.GetManagementAuditWeeklyNoCompliance(divisionId, fromDate, toDate),
                (division, records) => EmailNotification.SendManagementAuditComplianceReviewAsync(configuration, serviceProvider, fromDate, toDate, division, records),
                store.Complete,
                (exception, key) => logger.LogError(exception, "Management Audit Compliance Review Division processing failed for {Key}.", key),
                cancellationToken);
        }

        public static async Task<ManagementAuditProcessingResult> ProcessDivisionQueueAsync(
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
            var completedDivisions = 0;
            var failedDivisions = 0;
            var recordsProcessed = 0;

            foreach (var division in divisions ?? Array.Empty<ManagementAuditWeeklyDivisionSummaryModel>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var key = BuildDivisionExecutionKey(fromDate, toDate, division.DivisionId);

                bool claimed;
                try
                {
                    claimed = claim(key);
                }
                catch (Exception e)
                {
                    failedDivisions++;
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
                    var response = result.ErrorMessage ?? "SMTP accepted the Management Audit Compliance Review notification.";
                    complete(key, status, response);
                    if (result.IsSuccess)
                    {
                        completedDivisions++;
                        recordsProcessed += records.Count;
                    }
                    else
                    {
                        failedDivisions++;
                        logFailure(new InvalidOperationException(response), key);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception e)
                {
                    failedDivisions++;
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

            return new ManagementAuditProcessingResult(completedDivisions, failedDivisions, recordsProcessed);
        }

        public static string BuildDivisionExecutionKey(DateTime fromDate, DateTime toDate, int divisionId)
        {
            return $"MGMT_AUDIT:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}:{divisionId}";
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
                throw new InvalidOperationException("The Division dataset contains a decision outside the Compliance Review Period.");
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

    public sealed record ManagementAuditProcessingResult(int CompletedDivisions, int FailedDivisions, int RecordsProcessed);
}
