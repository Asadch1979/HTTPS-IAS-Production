using AIS.Controllers;
using AIS.Models.EmailManagement;
using Ganss.Xss;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIS.Services.EmailManagement
    {
    public interface IStandaloneEmailManagementService
        {
        EmailManagementPreview BuildPreview(long eventId, long? templateId = null, IDictionary<string, string> values = null);
        Task<EmailManagementSendResult> SendTestAsync(EmailManagementTestRequest request, string initiatedBy, string callingComponent);
        }

    public class StandaloneEmailManagementService : IStandaloneEmailManagementService
        {
        private static readonly Regex TokenRegex = new Regex(@"\{[A-Za-z0-9_.]+\}", RegexOptions.Compiled);
        private static readonly HtmlSanitizer HtmlSanitizer = new HtmlSanitizer();
        private readonly IConfiguration _configuration;
        private readonly DBConnection _db;
        private readonly ILogger<StandaloneEmailManagementService> _logger;

        public StandaloneEmailManagementService(
            IConfiguration configuration,
            DBConnection db,
            ILogger<StandaloneEmailManagementService> logger)
            {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        public EmailManagementPreview BuildPreview(long eventId, long? templateId = null, IDictionary<string, string> values = null)
            {
            var emailEvent = _db.GetManagedEmailEvents(eventId).SingleOrDefault()
                ?? throw new InvalidOperationException("Email event was not found.");
            var selectedTemplateId = templateId ?? emailEvent.ActiveTemplateId;
            if (!selectedTemplateId.HasValue)
                {
                throw new InvalidOperationException("The event has no active template.");
                }

            var template = _db.GetManagedEmailTemplates(eventId, selectedTemplateId).SingleOrDefault()
                ?? throw new InvalidOperationException("Email template was not found.");
            if (!template.IsActive)
                {
                throw new InvalidOperationException("The selected template is inactive.");
                }

            var resolvedValues = _db.GetManagedEmailPlaceholders(eventId)
                .Where(item => item.IsActive)
                .ToDictionary(item => item.Token, item => item.TestValue ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            foreach (var pair in values ?? new Dictionary<string, string>())
                {
                resolvedValues[pair.Key] = pair.Value ?? string.Empty;
                }

            return new EmailManagementPreview
                {
                EventId = eventId,
                TemplateId = template.TemplateId,
                EventKey = emailEvent.EventKey,
                Values = resolvedValues,
                Subject = Resolve(template.SubjectTemplate, resolvedValues),
                BodyHtml = HtmlSanitizer.Sanitize(Resolve(template.BodyHtmlTemplate, resolvedValues))
                };
            }

        public async Task<EmailManagementSendResult> SendTestAsync(
            EmailManagementTestRequest request,
            string initiatedBy,
            string callingComponent)
            {
            request ??= new EmailManagementTestRequest();
            var correlationId = Guid.NewGuid().ToString("N");
            EmailManagementEvent emailEvent = null;
            EmailManagementTemplate template = null;
            EmailManagementPreview preview = null;
            List<string> to = new List<string>();
            List<string> cc = new List<string>();
            List<string> bcc = new List<string>();
            long? logId = null;

            try
                {
                emailEvent = _db.GetManagedEmailEvents(request.EventId).SingleOrDefault();
                if (emailEvent == null)
                    {
                    return await RecordSkippedAsync("UNKNOWN_EVENT", null, null, request, initiatedBy, callingComponent, correlationId, "Email event was not found.");
                    }
                if (!emailEvent.IsEnabled)
                    {
                    return await RecordSkippedAsync(emailEvent.EventKey, null, null, request, initiatedBy, callingComponent, correlationId, "Email event is disabled.");
                    }

                preview = BuildPreview(request.EventId, request.TemplateId);
                template = _db.GetManagedEmailTemplates(request.EventId, preview.TemplateId).Single();
                to = NormalizeAddresses(request.ToRecipients);
                cc = NormalizeAddresses(request.CcRecipients).Except(to, StringComparer.OrdinalIgnoreCase).ToList();
                bcc = NormalizeAddresses(request.BccRecipients)
                    .Except(to, StringComparer.OrdinalIgnoreCase)
                    .Except(cc, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (to.Count == 0)
                    {
                    return await RecordSkippedAsync(emailEvent.EventKey, template, preview, request, initiatedBy, callingComponent, correlationId, "At least one valid To recipient is required.");
                    }

                var log = CreateLog(emailEvent.EventKey, template, preview, request, initiatedBy, callingComponent,
                    correlationId, EmailManagementStatuses.Pending, to, cc, bcc, string.Empty);
                logId = TryCreateLog(log);

                var settings = ReadSettings();
                if (!settings.IsConfigured)
                    {
                    var message = "Standalone Email Management SMTP configuration is incomplete.";
                    TryCompleteLog(logId, EmailManagementStatuses.Skipped, string.Empty, message);
                    return Result(false, EmailManagementStatuses.Skipped, message, correlationId, logId);
                    }

                using var messageToSend = new MailMessage
                    {
                    From = new MailAddress(settings.From),
                    Subject = preview.Subject,
                    Body = preview.BodyHtml,
                    IsBodyHtml = true
                    };
                AddAddresses(messageToSend.To, to);
                AddAddresses(messageToSend.CC, cc);
                AddAddresses(messageToSend.Bcc, bcc);

                using var smtp = new SmtpClient(settings.Host)
                    {
                    Port = settings.Port,
                    EnableSsl = settings.EnableSsl,
                    Credentials = new NetworkCredential(settings.Username, settings.Password),
                    Timeout = settings.TimeoutMilliseconds
                    };
                await smtp.SendMailAsync(messageToSend);

                const string accepted = "SMTP server accepted the message for transmission; delivery is not confirmed.";
                TryCompleteLog(logId, EmailManagementStatuses.SentToSmtp, accepted, string.Empty);
                return Result(true, EmailManagementStatuses.SentToSmtp, accepted, correlationId, logId);
                }
            catch (Exception ex)
                {
                var safeError = $"{ex.GetType().Name}: {ex.Message}";
                _logger.LogError(ex, "Standalone email test failed. CorrelationId={CorrelationId}", correlationId);
                if (!logId.HasValue)
                    {
                    var fallbackLog = CreateLog(emailEvent?.EventKey ?? "UNKNOWN_EVENT", template, preview, request,
                        initiatedBy, callingComponent, correlationId, EmailManagementStatuses.SendFailed,
                        to, cc, bcc, safeError);
                    fallbackLog.FailureDetails = safeError;
                    logId = TryCreateLog(fallbackLog);
                    }
                else
                    {
                    TryCompleteLog(logId, EmailManagementStatuses.SendFailed, string.Empty, safeError);
                    }
                return Result(false, EmailManagementStatuses.SendFailed, safeError, correlationId, logId);
                }
            }

        private Task<EmailManagementSendResult> RecordSkippedAsync(
            string eventKey, EmailManagementTemplate template, EmailManagementPreview preview,
            EmailManagementTestRequest request, string initiatedBy, string callingComponent,
            string correlationId, string reason)
            {
            var log = CreateLog(eventKey, template, preview, request, initiatedBy, callingComponent,
                correlationId, EmailManagementStatuses.Skipped,
                NormalizeAddresses(request.ToRecipients), NormalizeAddresses(request.CcRecipients),
                NormalizeAddresses(request.BccRecipients), reason);
            log.FailureDetails = reason;
            var logId = TryCreateLog(log);
            return Task.FromResult(Result(false, EmailManagementStatuses.Skipped, reason, correlationId, logId));
            }

        private EmailManagementLog CreateLog(
            string eventKey, EmailManagementTemplate template, EmailManagementPreview preview,
            EmailManagementTestRequest request, string initiatedBy, string callingComponent,
            string correlationId, string status, IEnumerable<string> to, IEnumerable<string> cc,
            IEnumerable<string> bcc, string failure)
            {
            return new EmailManagementLog
                {
                EventKey = eventKey,
                TemplateId = template?.TemplateId,
                TemplateVersion = template?.VersionNo,
                SubjectSent = preview?.Subject ?? string.Empty,
                BodySent = preview?.BodyHtml ?? string.Empty,
                ToRecipients = string.Join(";", to ?? Array.Empty<string>()),
                CcRecipients = string.Join(";", cc ?? Array.Empty<string>()),
                BccRecipients = string.Join(";", bcc ?? Array.Empty<string>()),
                AttachmentMetadata = "[]",
                Status = status,
                AttemptNumber = 1,
                CallingComponent = callingComponent,
                InitiatedBy = initiatedBy,
                CorrelationId = correlationId,
                ReferenceId = request.ReferenceId,
                FailureDetails = failure
                };
            }

        private long? TryCreateLog(EmailManagementLog log)
            {
            try
                {
                return _db.CreateManagedEmailLog(log);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Standalone email log creation failed. CorrelationId={CorrelationId}", log.CorrelationId);
                return null;
                }
            }

        private void TryCompleteLog(long? logId, string status, string smtpResponse, string failure)
            {
            if (!logId.HasValue)
                {
                return;
                }
            try
                {
                _db.CompleteManagedEmailLog(logId.Value, status, smtpResponse, failure);
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Standalone email log completion failed. LogId={LogId}", logId);
                }
            }

        private SmtpSettings ReadSettings()
            {
            var section = _configuration.GetSection("EmailManagement:Smtp");
            var from = section["From"] ?? string.Empty;
            var username = section["Username"] ?? from;
            var password = section["Password"] ?? string.Empty;
            var host = section["Host"] ?? string.Empty;
            return new SmtpSettings
                {
                From = from,
                Username = username,
                Password = password,
                Host = host,
                Port = section.GetValue<int?>("Port") ?? 587,
                EnableSsl = section.GetValue<bool?>("EnableSsl") ?? true,
                TimeoutMilliseconds = Math.Clamp(section.GetValue<int?>("TimeoutMilliseconds") ?? 30000, 1000, 120000),
                IsConfigured = !string.IsNullOrWhiteSpace(from)
                    && !string.IsNullOrWhiteSpace(password)
                    && !string.IsNullOrWhiteSpace(host)
                };
            }

        private static string Resolve(string template, IReadOnlyDictionary<string, string> values)
            {
            return TokenRegex.Replace(template ?? string.Empty, match =>
                values.TryGetValue(match.Value, out var value) ? value ?? string.Empty : match.Value);
            }

        private static List<string> NormalizeAddresses(string addresses)
            {
            var result = new List<string>();
            foreach (var token in (addresses ?? string.Empty).Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                try
                    {
                    var normalized = new MailAddress(token).Address;
                    if (!result.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                        {
                        result.Add(normalized);
                        }
                    }
                catch (FormatException)
                    {
                    }
                }
            return result;
            }

        private static void AddAddresses(MailAddressCollection target, IEnumerable<string> addresses)
            {
            foreach (var address in addresses)
                {
                target.Add(new MailAddress(address));
                }
            }

        private static EmailManagementSendResult Result(bool success, string status, string message, string correlationId, long? logId) =>
            new EmailManagementSendResult
                {
                IsSuccess = success,
                Status = status,
                Message = message,
                CorrelationId = correlationId,
                LogId = logId
                };

        private sealed class SmtpSettings
            {
            public string From { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public bool EnableSsl { get; set; }
            public int TimeoutMilliseconds { get; set; }
            public bool IsConfigured { get; set; }
            }
        }
    }
