using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models.EmailManagement
    {
    public static class EmailManagementStatuses
        {
        public const string Pending = "PENDING";
        public const string SentToSmtp = "SENT_TO_SMTP";
        public const string SendFailed = "SEND_FAILED";
        public const string Skipped = "SKIPPED";
        public const string Bounced = "BOUNCED";
        public const string Delivered = "DELIVERED";

        public static readonly IReadOnlyList<string> All = new[]
            {
            Pending, SentToSmtp, SendFailed, Skipped, Bounced, Delivered
            };
        }

    public class EmailManagementEvent
        {
        public long EventId { get; set; }

        [Required, StringLength(100)]
        [RegularExpression(@"^[A-Za-z0-9_]+$", ErrorMessage = "Use letters, numbers and underscores only.")]
        public string EventKey { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;
        public long? ActiveTemplateId { get; set; }
        public string ActiveTemplateName { get; set; } = string.Empty;
        }

    public class EmailManagementTemplate
        {
        public long TemplateId { get; set; }

        [Required]
        public long EventId { get; set; }

        [Required, StringLength(150)]
        public string TemplateName { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Culture { get; set; } = "en";

        [Required]
        public string SubjectTemplate { get; set; } = string.Empty;

        [Required]
        public string BodyHtmlTemplate { get; set; } = string.Empty;

        public int VersionNo { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public string EventKey { get; set; } = string.Empty;
        }

    public class EmailManagementRule
        {
        public long RuleId { get; set; }

        [Required]
        public long EventId { get; set; }

        [StringLength(2000)]
        public string ToRecipients { get; set; } = string.Empty;

        [StringLength(2000)]
        public string CcRecipients { get; set; } = string.Empty;

        [StringLength(2000)]
        public string BccRecipients { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public string EventKey { get; set; } = string.Empty;
        }

    public class EmailManagementPlaceholder
        {
        public long PlaceholderId { get; set; }

        [Required]
        public long EventId { get; set; }

        [Required, StringLength(100)]
        [RegularExpression(@"^\{[A-Za-z0-9_.]+\}$", ErrorMessage = "Use a token such as {Entity.Name}.")]
        public string Token { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        public string TestValue { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string EventKey { get; set; } = string.Empty;
        }

    public class EmailManagementAttachmentDefinition
        {
        public long AttachmentId { get; set; }
        public long EventId { get; set; }
        public string AttachmentName { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string SourceReference { get; set; } = string.Empty;
        public string FileNamePattern { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        }

    public class EmailManagementPreview
        {
        public long EventId { get; set; }
        public long TemplateId { get; set; }
        public string EventKey { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
        public bool HtmlWasSanitized { get; set; }
        public List<string> UnresolvedPlaceholders { get; set; } = new List<string>();
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

    public class EmailManagementTestRequest
        {
        [Required]
        public long EventId { get; set; }

        public long? TemplateId { get; set; }

        [Required, StringLength(2000)]
        public string ToRecipients { get; set; } = string.Empty;

        [StringLength(2000)]
        public string CcRecipients { get; set; } = string.Empty;

        [StringLength(2000)]
        public string BccRecipients { get; set; } = string.Empty;

        [StringLength(200)]
        public string ReferenceId { get; set; } = string.Empty;

        public bool UseConfiguredRecipientRule { get; set; }
        }

    public class EmailManagementLogFilter
        {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string EventKey { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        }

    public class EmailManagementLog
        {
        public long LogId { get; set; }
        public string EventKey { get; set; } = string.Empty;
        public long? TemplateId { get; set; }
        public int? TemplateVersion { get; set; }
        public string SubjectSent { get; set; } = string.Empty;
        public string BodySent { get; set; } = string.Empty;
        public string ToRecipients { get; set; } = string.Empty;
        public string CcRecipients { get; set; } = string.Empty;
        public string BccRecipients { get; set; } = string.Empty;
        public string AttachmentMetadata { get; set; } = string.Empty;
        public DateTime AttemptedOnUtc { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SmtpResponse { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public string CallingComponent { get; set; } = string.Empty;
        public string InitiatedBy { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
        public DateTime? SentToSmtpOnUtc { get; set; }
        public string FailureDetails { get; set; } = string.Empty;
        }

    public class EmailManagementDashboard
        {
        public int EventCount { get; set; }
        public int EnabledEventCount { get; set; }
        public int TemplateCount { get; set; }
        public int FailedAttemptCount { get; set; }
        public List<EmailManagementLog> RecentLogs { get; set; } = new List<EmailManagementLog>();
        }

    public class EmailManagementSendResult
        {
        public bool IsSuccess { get; set; }
        public string Status { get; set; } = EmailManagementStatuses.SendFailed;
        public string Message { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public long? LogId { get; set; }
        public List<string> InvalidRecipients { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        }
    }
