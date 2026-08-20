using AIS;
using AIS.Controllers;
using AIS.Models;
using AIS.Models.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public class EmailConfiguration
    {
    private readonly EmailCredentails emailCredentails;
    private readonly IServiceProvider _serviceProvider;
    private static int _emailNotConfiguredLogged;

    public EmailConfiguration(IConfiguration configuration, IServiceProvider serviceProvider = null)
        {
        if (configuration == null)
            {
            throw new ArgumentNullException(nameof(configuration));
            }

        emailCredentails = new EmailCredentails(configuration);
        _serviceProvider = serviceProvider;
        }

    private string SanitizeHeaderValue(string value, string paramName)
        {
        if (string.IsNullOrWhiteSpace(value))
            {
            return string.Empty;
            }

        if (value.Contains("\r") || value.Contains("\n"))
            {
            throw new ArgumentException("Email header values must not contain new lines.", paramName);
            }

        return value.Trim();
        }

    private MailAddress CreateSafeMailAddress(string address, string paramName)
        {
        var sanitizedAddress = SanitizeHeaderValue(address, paramName);
        return new MailAddress(sanitizedAddress);
        }

    private static void LogInfo(string message)
        {
        Console.WriteLine($"[EmailConfiguration] {message}");
        }

    private static void LogError(string message, Exception ex)
        {
        Console.WriteLine($"[EmailConfiguration] {message} Exception={ex}");
        }

    public bool ConfigEmail(string to = "", string cc = "", string subj = "", string body = "")
        {
        var result = Send(new EmailMessageRequest
            {
            ToRecipients = new[] { to },
            CcRecipients = new[] { cc },
            Subject = subj,
            Body = body,
            IsBodyHtml = true
            });

        return result.IsSuccess;
        }

    public async Task<bool> ConfigEmailAsync(string to = "", string cc = "", string subj = "", string body = "")
        {
        var result = await SendAsync(new EmailMessageRequest
            {
            ToRecipients = new[] { to },
            CcRecipients = new[] { cc },
            Subject = subj,
            Body = body,
            IsBodyHtml = true
            });

        return result.IsSuccess;
        }

    public EmailSendResult Send(EmailMessageRequest request)
        {
        request ??= new EmailMessageRequest();
        var logId = BeginAttemptLog(request);
        try
            {
            EmailCredentailsModel em = emailCredentails.GetEmailCredentails();
            var preparedRequest = PrepareRequest(request);
            if (!em.IsConfigured)
                {
                LogEmailNotConfigured("Send", preparedRequest, string.Empty);
                CompleteAttemptLog(logId, "CONFIGURATION_MISSING", "Email configuration is incomplete.", false);
                return new EmailSendResult
                    {
                    IsSuccess = false,
                    Status = "CONFIGURATION_MISSING",
                    ToRecipients = preparedRequest.ToRecipients,
                    CcRecipients = preparedRequest.CcRecipients,
                    ErrorMessage = "Email is disabled because credentials are not configured."
                    };
                }

            if (preparedRequest.ToRecipients.Count == 0)
                {
                CompleteAttemptLog(logId, "RECIPIENT_MISSING", "No valid recipient email addresses were supplied.", false);
                return new EmailSendResult
                    {
                    IsSuccess = false,
                    Status = "RECIPIENT_MISSING",
                    ToRecipients = preparedRequest.ToRecipients,
                    CcRecipients = preparedRequest.CcRecipients,
                    ErrorMessage = "No valid recipient email addresses were supplied."
                    };
                }

            LogInfo($"Preparing email send. From={em.EMAIL}; To={string.Join(";", preparedRequest.ToRecipients)}; Cc={string.Join(";", preparedRequest.CcRecipients)}; Subject={preparedRequest.Subject}; BodyLength={preparedRequest.Body.Length}; Host={em.Host}; Port={em.Port}; Html={preparedRequest.IsBodyHtml}; Attachments={preparedRequest.Attachments.Count}");

            using (var mail = CreateMailMessage(em, preparedRequest))
            using (var smtp = CreateSmtpClient(em))
                {
                smtp.Send(mail);
                }

            CompleteAttemptLog(logId, "SENT", string.Empty, true);
            return new EmailSendResult
                {
                IsSuccess = true,
                Status = "SENT",
                ToRecipients = preparedRequest.ToRecipients,
                CcRecipients = preparedRequest.CcRecipients
                };
            }
        catch (SmtpException ex)
            {
            LogError("SMTP error while sending email.", ex);
            CompleteAttemptLog(logId, "SMTP_FAILED", ex.Message, false);
            return new EmailSendResult { IsSuccess = false, Status = "SMTP_FAILED", ErrorMessage = ex.Message };
            }
        catch (Exception ex)
            {
            LogError("General error while sending email.", ex);
            CompleteAttemptLog(logId, "FAILED", ex.Message, false);
            return new EmailSendResult { IsSuccess = false, Status = "FAILED", ErrorMessage = ex.Message };
            }
        }

    public async Task<EmailSendResult> SendAsync(EmailMessageRequest request)
        {
        request ??= new EmailMessageRequest();
        var logId = BeginAttemptLog(request);
        try
            {
            EmailCredentailsModel em = emailCredentails.GetEmailCredentails();
            var preparedRequest = PrepareRequest(request);
            if (!em.IsConfigured)
                {
                LogEmailNotConfigured("SendAsync", preparedRequest, string.Empty);
                CompleteAttemptLog(logId, "CONFIGURATION_MISSING", "Email configuration is incomplete.", false);
                return new EmailSendResult
                    {
                    IsSuccess = false,
                    Status = "CONFIGURATION_MISSING",
                    ToRecipients = preparedRequest.ToRecipients,
                    CcRecipients = preparedRequest.CcRecipients,
                    ErrorMessage = "Email is disabled because credentials are not configured."
                    };
                }

            if (preparedRequest.ToRecipients.Count == 0)
                {
                CompleteAttemptLog(logId, "RECIPIENT_MISSING", "No valid recipient email addresses were supplied.", false);
                return new EmailSendResult
                    {
                    IsSuccess = false,
                    Status = "RECIPIENT_MISSING",
                    ToRecipients = preparedRequest.ToRecipients,
                    CcRecipients = preparedRequest.CcRecipients,
                    ErrorMessage = "No valid recipient email addresses were supplied."
                    };
                }

            LogInfo($"Preparing async email send. From={em.EMAIL}; To={string.Join(";", preparedRequest.ToRecipients)}; Cc={string.Join(";", preparedRequest.CcRecipients)}; Subject={preparedRequest.Subject}; BodyLength={preparedRequest.Body.Length}; Host={em.Host}; Port={em.Port}; Html={preparedRequest.IsBodyHtml}; Attachments={preparedRequest.Attachments.Count}");

            using (var mail = CreateMailMessage(em, preparedRequest))
            using (var smtp = CreateSmtpClient(em))
                {
                await smtp.SendMailAsync(mail);
                }

            CompleteAttemptLog(logId, "SENT", string.Empty, true);
            return new EmailSendResult
                {
                IsSuccess = true,
                Status = "SENT",
                ToRecipients = preparedRequest.ToRecipients,
                CcRecipients = preparedRequest.CcRecipients
                };
            }
        catch (SmtpException ex)
            {
            LogError("SMTP error while sending async email.", ex);
            CompleteAttemptLog(logId, "SMTP_FAILED", ex.Message, false);
            return new EmailSendResult { IsSuccess = false, Status = "SMTP_FAILED", ErrorMessage = ex.Message };
            }
        catch (Exception ex)
            {
            LogError("General error while sending async email.", ex);
            CompleteAttemptLog(logId, "FAILED", ex.Message, false);
            return new EmailSendResult { IsSuccess = false, Status = "FAILED", ErrorMessage = ex.Message };
            }
        }

    private long? BeginAttemptLog(EmailMessageRequest request)
        {
        try
            {
            var db = _serviceProvider?.GetService<DBConnection>();
            if (db == null)
                {
                LogInfo("Database email-attempt logging is unavailable because no request service provider was supplied.");
                return null;
                }

            return db.LogEmailTriggerAttempt(
                string.IsNullOrWhiteSpace(request.Module) ? "Email" : request.Module,
                string.IsNullOrWhiteSpace(request.TriggerPoint) ? "Unspecified" : request.TriggerPoint,
                request.ReferenceId,
                JoinRawRecipients(request.ToRecipients),
                JoinRawRecipients(request.CcRecipients),
                request.Subject);
            }
        catch (Exception ex)
            {
            LogError("Failed to create email-attempt log.", ex);
            return null;
            }
        }

    private void CompleteAttemptLog(long? logId, string status, string errorMessage, bool sent)
        {
        if (!logId.HasValue)
            {
            return;
            }

        try
            {
            _serviceProvider?.GetService<DBConnection>()?.CompleteEmailTriggerAttempt(logId.Value, status, errorMessage, sent);
            }
        catch (Exception ex)
            {
            LogError($"Failed to complete email-attempt log with status {status}.", ex);
            }
        }

    private static string JoinRawRecipients(IEnumerable<string> recipients) =>
        string.Join(";", recipients ?? Array.Empty<string>());

    private PreparedEmailRequest PrepareRequest(EmailMessageRequest request)
        {
        request ??= new EmailMessageRequest();

        var subject = SanitizeHeaderValue(request.Subject, nameof(request.Subject));
        var body = request.Body ?? string.Empty;
        var toRecipients = NormalizeRecipients(request.ToRecipients);
        var ccRecipients = NormalizeRecipients(request.CcRecipients)
            .Where(address => !toRecipients.Contains(address, StringComparer.OrdinalIgnoreCase))
            .ToList();
        var attachments = (request.Attachments ?? Array.Empty<NotificationEmailAttachmentData>())
            .Where(attachment => attachment != null
                && !string.IsNullOrWhiteSpace(attachment.FileName)
                && attachment.ContentBytes != null
                && attachment.ContentBytes.Length > 0)
            .Select(attachment => new NotificationEmailAttachmentData
                {
                FileName = attachment.FileName.Trim(),
                ContentBytes = attachment.ContentBytes,
                ContentType = string.IsNullOrWhiteSpace(attachment.ContentType) ? "application/octet-stream" : attachment.ContentType.Trim()
                })
            .ToList();

        return new PreparedEmailRequest
            {
            ToRecipients = toRecipients,
            CcRecipients = ccRecipients,
            Subject = subject,
            Body = body,
            IsBodyHtml = request.IsBodyHtml,
            Attachments = attachments
            };
        }

    private List<string> NormalizeRecipients(IEnumerable<string> recipients)
        {
        var normalized = new List<string>();
        if (recipients == null)
            {
            return normalized;
            }

        foreach (var entry in recipients)
            {
            if (string.IsNullOrWhiteSpace(entry))
                {
                continue;
                }

            var tokens = entry.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var token in tokens)
                {
                var sanitized = SanitizeHeaderValue(token, nameof(recipients));
                if (string.IsNullOrWhiteSpace(sanitized))
                    {
                    continue;
                    }

                if (!normalized.Contains(sanitized, StringComparer.OrdinalIgnoreCase))
                    {
                    normalized.Add(sanitized);
                    }
                }
            }

        return normalized;
        }

    private MailMessage CreateMailMessage(EmailCredentailsModel credentials, PreparedEmailRequest request)
        {
        var mail = new MailMessage
            {
            From = CreateSafeMailAddress(credentials.EMAIL, nameof(credentials.EMAIL)),
            Subject = request.Subject,
            Body = request.Body,
            IsBodyHtml = request.IsBodyHtml
            };

        AddRecipients(mail.To, request.ToRecipients, nameof(request.ToRecipients));
        AddRecipients(mail.CC, request.CcRecipients, nameof(request.CcRecipients));
        foreach (var attachment in request.Attachments)
            {
            var stream = new MemoryStream(attachment.ContentBytes, writable: false);
            mail.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
            }

        return mail;
        }

    private void AddRecipients(MailAddressCollection collection, IEnumerable<string> addresses, string paramName)
        {
        if (collection == null)
            {
            throw new ArgumentNullException(nameof(collection));
            }

        foreach (var address in addresses ?? Enumerable.Empty<string>())
            {
            if (string.IsNullOrWhiteSpace(address))
                {
                continue;
                }

            collection.Add(CreateSafeMailAddress(address, paramName));
            }
        }

    private static SmtpClient CreateSmtpClient(EmailCredentailsModel credentials)
        {
        return new SmtpClient(credentials.Host)
            {
            Port = credentials.Port,
            Credentials = new NetworkCredential(
                string.IsNullOrWhiteSpace(credentials.Username) ? credentials.EMAIL : credentials.Username,
                credentials.PASSWORD),
            EnableSsl = credentials.EnableSsl
            };
        }

    private void LogEmailNotConfigured(string actionName, PreparedEmailRequest request, string fallbackSubject)
        {
        if (Interlocked.Exchange(ref _emailNotConfiguredLogged, 1) == 1)
            {
            return;
            }

        var db = _serviceProvider?.GetService<DBConnection>();
        if (db == null)
            {
            return;
            }

        var sessionHandler = _serviceProvider.GetService<SessionHandler>();
        var pageId = sessionHandler?.GetPageId();
        int? engId = null;
        if (sessionHandler != null && sessionHandler.TryGetActiveEngagementId(out var engagementId))
            {
            engId = engagementId;
            }

        var userPpno = sessionHandler != null && sessionHandler.TryGetUser(out var sessionUser) ? sessionUser.PPNumber : null;
        var subject = request == null ? fallbackSubject : request.Subject;
        var message = $"Email send skipped because email is disabled. To={string.Join(";", request?.ToRecipients ?? new List<string>())}; Cc={string.Join(";", request?.CcRecipients ?? new List<string>())}; Subject={subject}.";
        try
            {
            db.LogWarning("Email", nameof(EmailConfiguration), actionName, message, "Email is disabled because credentials are not configured.", pageId, engId, userPpno);
            }
        catch (Exception ex)
            {
            LogError("Failed to log email-not-configured warning.", ex);
            }
        }

    private sealed class PreparedEmailRequest
        {
        public List<string> ToRecipients { get; set; } = new List<string>();
        public List<string> CcRecipients { get; set; } = new List<string>();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; }
        public List<NotificationEmailAttachmentData> Attachments { get; set; } = new List<NotificationEmailAttachmentData>();
        }
    }
