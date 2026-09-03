using AIS.Controllers;
using AIS.Models.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIS
    {
    public static class EmailNotification
        {
        private const string NotificationHeader = "Internal Audit System (IAS)";
        private const string StandardFooter = "This is a system-generated notification from Internal Audit System (IAS). Please do not reply to this email unless required under official process.";
        private static readonly Regex HtmlTagRegex = new Regex("<.*?>", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex WhitespaceRegex = new Regex("\\s+", RegexOptions.Compiled);

        private static void LogNotification(string triggerName, string toEmail, string ccEmail, string subject, string body)
            {
            var bodyLength = string.IsNullOrWhiteSpace(body) ? 0 : body.Length;
            Console.WriteLine($"[EmailNotification] Trigger={triggerName}; To={toEmail}; Cc={ccEmail}; Subject={subject}; BodyLength={bodyLength}");
            }

        public static async Task<bool> SendJoiningNotificationAsync(IConfiguration configuration, string engagementId, string toEmail, string ccEmail, string auditEntity, string teamLead, string teamMembers, IServiceProvider serviceProvider = null)
            {
            string emailSubject = $"IAS Notification: Audit Team has Joined for {auditEntity}";
            string emailBody = BuildHtmlBody(
                "Audit Team has Joined",
                $"The audit team has officially joined for the audit of {auditEntity}. Please coordinate with the Audit Team and provide all information requested during the course of the audit.",
                BuildDetails(
                    ("Entity", auditEntity),
                    ("Team Lead", teamLead),
                    ("Team Members", teamMembers),
                    ("Status / Instructions", "The Audit Team has joined. Please provide the necessary assistance and all information requested during the audit process.")));
            LogNotification(nameof(SendJoiningNotificationAsync), toEmail, ccEmail, emailSubject, emailBody);
            EmailConfiguration econ = new EmailConfiguration(configuration, serviceProvider);
            var result = await econ.SendAsync(CreateRequest("Audit", nameof(SendJoiningNotificationAsync), engagementId, toEmail, ccEmail, emailSubject, emailBody, true));
            return result.IsSuccess;
            }

        public static bool SendPasswordResetSuccess(IConfiguration configuration, string userFullName, string ppNumber, string pass, string userEmail, string userCCEmail, IServiceProvider serviceProvider = null)
            {
            string subject = "IAS~ Password Reset Successful";
            string body = BuildHtmlBody(
                "Password Reset Successful",
                $"Dear {ChooseFirstNonEmpty(userFullName, "User")}, your password has been successfully reset. Please use the login details below and change this password immediately after logging in. If you did not request this reset, contact support immediately.",
                BuildDetails(
                    ("Username", ppNumber),
                    ("Temporary Password", pass)));
            LogNotification(nameof(SendPasswordResetSuccess), userEmail, userCCEmail, subject, body);
            EmailConfiguration email = new EmailConfiguration(configuration, serviceProvider);
            return email.Send(CreateRequest("Authentication", nameof(SendPasswordResetSuccess), ppNumber, userEmail, userCCEmail, subject, body, true)).IsSuccess;
            }

        public static bool NotifyAuditSampleIssue(IConfiguration configuration, string engagementId, string toEmail, string ccEmail, IServiceProvider serviceProvider = null)
            {
            string subject = $"IAS~Notification: Issue in Audit Sample for Engagement ID: {engagementId}";
            string body = BuildHtmlBody(
                "Issue in Audit Sample Creation",
                "An issue was identified while creating the audit sample. Please review and resolve it.",
                BuildDetails(("Engagement ID", engagementId)));
            LogNotification(nameof(NotifyAuditSampleIssue), toEmail, ccEmail, subject, body);
            EmailConfiguration econ = new EmailConfiguration(configuration, serviceProvider);
            return econ.Send(CreateRequest("Sampling", nameof(NotifyAuditSampleIssue), engagementId, toEmail, ccEmail, subject, body, true)).IsSuccess;
            }

        public static bool NotifyAuditExceptionIssue(IConfiguration configuration, string engagementId, string toEmail, string ccEmail, IServiceProvider serviceProvider = null)
            {
            string subject = $"IAS~Notification: Issue in Audit Exception for Engagement ID: {engagementId}";
            string body = BuildHtmlBody(
                "Issue in Audit Exception Creation",
                "An issue was identified while creating the audit exception report. Please review and resolve it.",
                BuildDetails(("Engagement ID", engagementId)));
            LogNotification(nameof(NotifyAuditExceptionIssue), toEmail, ccEmail, subject, body);
            EmailConfiguration econ = new EmailConfiguration(configuration, serviceProvider);
            return econ.Send(CreateRequest("Exception Monitoring", nameof(NotifyAuditExceptionIssue), engagementId, toEmail, ccEmail, subject, body, true)).IsSuccess;
            }

        public static bool NotifyAuditCriteriaSubmission(IConfiguration configuration, string toEmail, string ccEmail, string subject, string body, IServiceProvider serviceProvider = null)
            {
            var htmlBody = BuildHtmlBody(
                "Audit Criteria Submitted",
                ChooseFirstNonEmpty(NormalizePlainText(body), "Audit criteria have been submitted for review."),
                BuildDetails());
            LogNotification(nameof(NotifyAuditCriteriaSubmission), toEmail, ccEmail, subject, htmlBody);
            EmailConfiguration email = new EmailConfiguration(configuration, serviceProvider);
            return email.Send(CreateRequest("Planning", nameof(NotifyAuditCriteriaSubmission), string.Empty, toEmail, ccEmail, subject, htmlBody, true)).IsSuccess;
            }

        public static bool NotifyParaStatus(IConfiguration configuration, string paraNo, string paraStatus, string paraGist, string toEmail, string ccEmail, string cc2Email, IServiceProvider serviceProvider = null)
            {
            string subject = $"IAS~Notification: Para No: {paraNo} is {paraStatus}";
            string body = BuildHtmlBody(
                "Audit Para Status Updated",
                $"Para No. {paraNo} has been {paraStatus}.",
                BuildDetails(
                    ("Para No.", paraNo),
                    ("Status", paraStatus),
                    ("Gist of Para", paraGist)));
            string ccCombined = string.Join(";", new[] { ccEmail, cc2Email }.Where(e => !string.IsNullOrWhiteSpace(e)));
            LogNotification(nameof(NotifyParaStatus), toEmail, ccCombined, subject, body);
            EmailConfiguration econ = new EmailConfiguration(configuration, serviceProvider);
            return econ.Send(CreateRequest("Audit", nameof(NotifyParaStatus), paraNo, toEmail, ccCombined, subject, body, true)).IsSuccess;
            }

        private static EmailMessageRequest CreateRequest(string module, string triggerPoint, string referenceId, string toEmail, string ccEmail, string subject, string body, bool isBodyHtml = true)
            {
            return new EmailMessageRequest
                {
                Module = module,
                TriggerPoint = triggerPoint,
                ReferenceId = referenceId,
                ToRecipients = new[] { toEmail },
                CcRecipients = new[] { ccEmail },
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
                };
            }

        public static Task<bool> SendObservationSubmittedToAuditeeAsync(IConfiguration configuration, ObservationSubmittedNotificationData data, IServiceProvider serviceProvider = null)
            {
            data ??= new ObservationSubmittedNotificationData();
            var reference = ChooseFirstNonEmpty(data.ObservationReference, $"OBS-{data.ObservationId}", "Observation");
            return SendHtmlNotificationAsync(
                configuration,
                nameof(SendObservationSubmittedToAuditeeAsync),
                data.EngagementId,
                data.ToRecipients,
                data.CcRecipients,
                $"IAS Notification: Observation Submitted to Auditee - {reference}" ,
                "Observation Submitted to Auditee",
                $"An audit observation has been submitted to the auditee for review and response. The reference details are provided below for onward action.",
                BuildDetails(
                    ("Reference", reference),
                    ("Entity", data.EntityName),
                    ("Audit Period", data.AuditPeriod),
                    ("Observation Heading", data.ObservationHeading),
                    ("Status", ChooseFirstNonEmpty(data.ObservationStatus, "Submitted to Auditee")),
                    ("Observation Summary", LimitText(data.ObservationSummary, 500))),
                serviceProvider);
            }

        public static Task<bool> SendAuditTaskAssignedAsync(IConfiguration configuration, AuditTaskAssignedNotificationData data, IServiceProvider serviceProvider = null)
            {
            data ??= new AuditTaskAssignedNotificationData();
            var engagementLabel = ChooseFirstNonEmpty(data.EngagementLabel, data.EntityName, $"Plan {data.PlanId}", "Engagement");
            return SendHtmlNotificationAsync(
                configuration,
                nameof(SendAuditTaskAssignedAsync),
                data.EngagementId,
                data.ToRecipients,
                data.CcRecipients,
                $"IAS Notification: Audit Task Assigned - {engagementLabel}",
                "Audit Task Assigned",
                $"An audit task has been allocated to the assigned team. Please review the engagement details and proceed in line with the approved plan.",
                BuildDetails(
                    ("Engagement", engagementLabel),
                    ("Entity", data.EntityName),
                    ("Audit Period", data.AuditPeriod),
                    ("Reporting Office", data.ReportingOffice),
                    ("Team", data.TeamName),
                    ("Audit Execution Dates", FormatDateRange(data.AuditStartDate, data.AuditEndDate)),
                    ("Operational Period", FormatDateRange(data.OperationalStartDate, data.OperationalEndDate)),
                    ("Team Members", string.Join(", ", data.TeamMembers.Where(item => !string.IsNullOrWhiteSpace(item))))),
                serviceProvider);
            }

        public static Task<bool> SendInquiryAssignedToUnitAsync(IConfiguration configuration, InquiryAssignedNotificationData data, IServiceProvider serviceProvider = null)
            {
            data ??= new InquiryAssignedNotificationData();
            var inquiryReference = ChooseFirstNonEmpty(data.InquiryReference, $"Inquiry {data.ComplaintId}", "Inquiry");
            return SendHtmlNotificationAsync(
                configuration,
                nameof(SendInquiryAssignedToUnitAsync),
                data.EngagementId,
                data.ToRecipients,
                data.CcRecipients,
                $"IAS Notification: Inquiry Assigned to I&I Unit - {inquiryReference}",
                "Inquiry Assigned to I&I Unit",
                $"A complaint inquiry has been approved and allocated to the relevant I&I Unit. Please take up the matter as per assigned directions and timelines.",
                BuildDetails(
                    ("Inquiry Reference", inquiryReference),
                    ("Inquiry Nature", data.InquiryNature),
                    ("Assigned Unit", data.AssignedUnitName),
                    ("Assigned On", data.AssignedOn),
                    ("Due Date", data.DueDate),
                    ("Directions", LimitText(data.Directions, 500))),
                serviceProvider);
            }

        public static Task<bool> SendFinalReportIssuedAsync(IConfiguration configuration, FinalReportIssuedNotificationData data, NotificationEmailAttachmentData attachment, IServiceProvider serviceProvider = null)
            {
            data ??= new FinalReportIssuedNotificationData();
            var engagementLabel = ChooseFirstNonEmpty(data.EngagementLabel, data.EntityName, data.AuditPeriod, "Final Report");
            if (attachment == null || attachment.ContentBytes == null || attachment.ContentBytes.Length == 0)
                {
                LogNotificationStatus(serviceProvider, data.EngagementId, "Warning", nameof(SendFinalReportIssuedAsync), "Notification skipped. Final report attachment could not be prepared.", $"Subject=IAS Notification: Final Audit Report Issued - {engagementLabel}");
                return Task.FromResult(false);
                }

            return SendHtmlNotificationAsync(
                configuration,
                nameof(SendFinalReportIssuedAsync),
                data.EngagementId,
                data.ToRecipients,
                data.CcRecipients,
                $"IAS Notification: Final Audit Report Issued - {engagementLabel}",
                "Final Audit Report Issued",
                $"The final audit report has been issued. The generated PDF report is attached for record and necessary follow-up under the approved audit process.",
                BuildDetails(
                    ("Engagement", engagementLabel),
                    ("Entity", data.EntityName),
                    ("Audit Period", data.AuditPeriod),
                    ("Reporting Office", data.ReportingOffice),
                    ("Team", data.TeamName),
                    ("Report Version", data.ReportVersion),
                    ("Issued On", FormatDate(data.FinalizedOn))),
                serviceProvider,
                new[] { attachment });
            }

        public static Task<bool> SendSystemErrorAlertAsync(
            IConfiguration configuration,
            string triggerName,
            string referenceId,
            string errorSubject,
            string errorNature,
            IEnumerable<KeyValuePair<string, string>> details,
            IServiceProvider serviceProvider = null,
            IEnumerable<string> toRecipients = null,
            IEnumerable<string> ccRecipients = null)
            {
            var nature = ChooseFirstNonEmpty(errorNature, "System error");
            var subject = LimitText(ChooseFirstNonEmpty(errorSubject, $"IAS Error Alert - Application - {referenceId}", nature), 180);
            if (toRecipients == null && serviceProvider != null)
                {
                try
                    {
                    var configuredRecipients = serviceProvider.GetService<DBConnection>()?.GetSystemErrorNotificationRecipients();
                    toRecipients = configuredRecipients?.ToRecipients;
                    ccRecipients ??= configuredRecipients?.CcRecipients;
                    }
                catch (Exception ex)
                    {
                    LogNotificationStatus(serviceProvider, null, "Warning", nameof(SendSystemErrorAlertAsync), "Unable to load system error recipients.", ex.Message);
                    }
                }

            return SendHtmlNotificationAsync(
                configuration,
                ChooseFirstNonEmpty(triggerName, nameof(SendSystemErrorAlertAsync)),
                null,
                toRecipients ?? Array.Empty<string>(),
                ccRecipients ?? Array.Empty<string>(),
                subject,
                "System Error",
                $"Nature of error: {nature}",
                details ?? BuildDetails(),
                serviceProvider);
            }

        private static async Task<bool> SendHtmlNotificationAsync(
            IConfiguration configuration,
            string triggerName,
            int? engId,
            IEnumerable<string> toRecipients,
            IEnumerable<string> ccRecipients,
            string subject,
            string title,
            string summary,
            IEnumerable<KeyValuePair<string, string>> details,
            IServiceProvider serviceProvider,
            IEnumerable<NotificationEmailAttachmentData> attachments = null)
            {
            try
                {
                var body = BuildHtmlBody(title, summary, details);
                LogNotification(triggerName, string.Join(";", toRecipients ?? Array.Empty<string>()), string.Join(";", ccRecipients ?? Array.Empty<string>()), subject, body);

                var email = new EmailConfiguration(configuration, serviceProvider);
                var result = await email.SendAsync(new EmailMessageRequest
                    {
                    Module = "Audit",
                    TriggerPoint = triggerName,
                    ReferenceId = engId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                    ToRecipients = toRecipients ?? Array.Empty<string>(),
                    CcRecipients = ccRecipients ?? Array.Empty<string>(),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    Attachments = attachments ?? Array.Empty<NotificationEmailAttachmentData>()
                    });

                var detailsText = $"Subject={subject}; To={string.Join(";", result.ToRecipients)}; Cc={string.Join(";", result.CcRecipients)}";
                if (result.IsSuccess)
                    {
                    LogNotificationStatus(serviceProvider, engId, "Info", triggerName, "Notification sent.", detailsText);
                    return true;
                    }

                var level = result.ToRecipients.Count == 0 ? "Warning" : "Error";
                LogNotificationStatus(serviceProvider, engId, level, triggerName, "Notification failed.", string.IsNullOrWhiteSpace(result.ErrorMessage) ? detailsText : $"{detailsText}; Error={result.ErrorMessage}");
                return false;
                }
            catch (Exception ex)
                {
                LogNotificationStatus(serviceProvider, engId, "Error", triggerName, "Notification failed with an unexpected error.", ex.ToString());
                return false;
                }
            }

        private static string BuildHtmlBody(string title, string summary, IEnumerable<KeyValuePair<string, string>> details)
            {
            var detailRows = (details ?? Enumerable.Empty<KeyValuePair<string, string>>())
                .Where(item => !string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                .ToList();

            var builder = new StringBuilder();
            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine("<html><head><meta charset=\"utf-8\" /></head>");
            builder.AppendLine("<body style=\"margin:0;padding:0;background-color:#f4f6f8;font-family:Arial,Helvetica,sans-serif;color:#1f2933;\">");
            builder.AppendLine("<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background-color:#f4f6f8;padding:24px 0;\"><tr><td align=\"center\">");
            builder.AppendLine("<table role=\"presentation\" width=\"680\" cellspacing=\"0\" cellpadding=\"0\" style=\"width:680px;max-width:680px;background-color:#ffffff;border-collapse:collapse;border:1px solid #d9e2ec;\">");
            builder.AppendLine($"<tr><td style=\"padding:18px 28px;background-color:#173f5f;color:#ffffff;font-size:20px;font-weight:bold;letter-spacing:0.2px;\">{Encode(NotificationHeader)}</td></tr>");
            builder.AppendLine($"<tr><td style=\"padding:28px 28px 16px 28px;font-size:24px;font-weight:bold;color:#102a43;\">{Encode(title)}</td></tr>");
            builder.AppendLine($"<tr><td style=\"padding:0 28px 20px 28px;font-size:15px;line-height:1.7;color:#334e68;\">{Encode(summary)}</td></tr>");
            if (detailRows.Count > 0)
                {
                builder.AppendLine("<tr><td style=\"padding:0 28px 28px 28px;\">");
                builder.AppendLine("<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"border-collapse:collapse;width:100%;border:1px solid #d9e2ec;\">");
                builder.AppendLine("<tr><th align=\"left\" style=\"padding:12px 14px;background-color:#eef2f6;border-bottom:1px solid #d9e2ec;font-size:13px;color:#243b53;width:34%;\">Detail</th><th align=\"left\" style=\"padding:12px 14px;background-color:#eef2f6;border-bottom:1px solid #d9e2ec;font-size:13px;color:#243b53;\">Information</th></tr>");
                foreach (var row in detailRows)
                    {
                    builder.AppendLine("<tr>");
                    builder.AppendLine($"<td style=\"padding:12px 14px;border-bottom:1px solid #d9e2ec;font-size:13px;font-weight:bold;color:#102a43;vertical-align:top;\">{Encode(row.Key)}</td>");
                    builder.AppendLine($"<td style=\"padding:12px 14px;border-bottom:1px solid #d9e2ec;font-size:13px;color:#334e68;vertical-align:top;\">{FormatHtmlValue(row.Value)}</td>");
                    builder.AppendLine("</tr>");
                    }
                builder.AppendLine("</table>");
                builder.AppendLine("</td></tr>");
                }
            builder.AppendLine($"<tr><td style=\"padding:18px 28px;background-color:#f8fafc;border-top:1px solid #d9e2ec;font-size:12px;line-height:1.6;color:#52606d;\">{Encode(StandardFooter)}</td></tr>");
            builder.AppendLine("</table></td></tr></table></body></html>");
            return builder.ToString();
            }

        private static IEnumerable<KeyValuePair<string, string>> BuildDetails(params (string Label, string Value)[] items)
            {
            foreach (var item in items ?? Array.Empty<(string Label, string Value)>())
                {
                if (string.IsNullOrWhiteSpace(item.Label) || string.IsNullOrWhiteSpace(item.Value))
                    {
                    continue;
                    }

                yield return new KeyValuePair<string, string>(item.Label.Trim(), item.Value.Trim());
                }
            }

        private static string FormatHtmlValue(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return "-";
                }

            var encoded = Encode(value.Trim());
            return encoded.Replace("\r\n", "<br />").Replace("\n", "<br />");
            }

        private static void LogNotificationStatus(IServiceProvider serviceProvider, int? engId, string level, string actionName, string message, string techDetails)
            {
            Console.WriteLine($"[EmailNotification] Level={level}; Action={actionName}; Message={message}; Details={techDetails}");
            var db = serviceProvider?.GetService<DBConnection>();
            if (db == null)
                {
                return;
                }

            var sessionHandler = serviceProvider.GetService<SessionHandler>();
            var pageId = sessionHandler?.GetPageId();
            var effectiveEngId = engId;
            if (!effectiveEngId.HasValue && sessionHandler != null && sessionHandler.TryGetActiveEngagementId(out var activeEngagementId))
                {
                effectiveEngId = activeEngagementId;
                }

            var userPpno = sessionHandler != null && sessionHandler.TryGetUser(out var sessionUser) ? sessionUser.PPNumber : null;
            try
                {
                if (string.Equals(level, "Info", StringComparison.OrdinalIgnoreCase))
                    {
                    db.LogInfo("Email", nameof(EmailNotification), actionName, message, techDetails, pageId, effectiveEngId, userPpno);
                    }
                else if (string.Equals(level, "Warning", StringComparison.OrdinalIgnoreCase))
                    {
                    db.LogWarning("Email", nameof(EmailNotification), actionName, message, techDetails, pageId, effectiveEngId, userPpno);
                    }
                else
                    {
                    db.LogError("Email", nameof(EmailNotification), actionName, message, techDetails, pageId, effectiveEngId, userPpno);
                    }
                }
            catch (Exception ex)
                {
                Console.WriteLine($"[EmailNotification] Failed to persist notification log. Exception={ex}");
                }
            }

        private static string ChooseFirstNonEmpty(params string[] values)
            {
            return values?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
            }

        private static string LimitText(string value, int maxLength)
            {
            var normalized = NormalizePlainText(value);
            if (string.IsNullOrWhiteSpace(normalized) || normalized.Length <= maxLength)
                {
                return normalized;
                }

            return normalized.Substring(0, maxLength).TrimEnd() + "...";
            }

        private static string NormalizePlainText(string value)
            {
            if (string.IsNullOrWhiteSpace(value))
                {
                return string.Empty;
                }

            var withoutHtml = HtmlTagRegex.Replace(value, " ");
            var decoded = WebUtility.HtmlDecode(withoutHtml);
            return WhitespaceRegex.Replace(decoded ?? string.Empty, " ").Trim();
            }

        private static string FormatDate(DateTime? value)
            {
            return value.HasValue ? value.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : string.Empty;
            }

        private static string FormatDateRange(DateTime? startDate, DateTime? endDate)
            {
            var start = FormatDate(startDate);
            var end = FormatDate(endDate);
            if (string.IsNullOrWhiteSpace(start) && string.IsNullOrWhiteSpace(end))
                {
                return string.Empty;
                }

            if (string.IsNullOrWhiteSpace(start))
                {
                return end;
                }

            if (string.IsNullOrWhiteSpace(end))
                {
                return start;
                }

            return $"{start} to {end}";
            }

        private static string Encode(string value)
            {
            return WebUtility.HtmlEncode(value ?? string.Empty);
            }
        }
    }
