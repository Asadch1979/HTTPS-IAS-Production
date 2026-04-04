using System;
using System.Collections.Generic;

namespace AIS.Models.Notifications
    {
    public class EmailMessageRequest
        {
        public IEnumerable<string> ToRecipients { get; set; } = Array.Empty<string>();
        public IEnumerable<string> CcRecipients { get; set; } = Array.Empty<string>();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; }
        public IEnumerable<NotificationEmailAttachmentData> Attachments { get; set; } = Array.Empty<NotificationEmailAttachmentData>();
        }

    public class EmailSendResult
        {
        public bool IsSuccess { get; set; }
        public List<string> ToRecipients { get; set; } = new List<string>();
        public List<string> CcRecipients { get; set; } = new List<string>();
        public string ErrorMessage { get; set; } = string.Empty;
        }

    public class NotificationEmailAttachmentData
        {
        public string FileName { get; set; } = string.Empty;
        public byte[] ContentBytes { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
        }

    public abstract class NotificationRecipientData
        {
        public int? EngagementId { get; set; }
        public List<string> ToRecipients { get; set; } = new List<string>();
        public List<string> CcRecipients { get; set; } = new List<string>();
        }

    public class ObservationSubmittedNotificationData : NotificationRecipientData
        {
        public int ObservationId { get; set; }
        public string ObservationReference { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string AuditPeriod { get; set; } = string.Empty;
        public string ObservationHeading { get; set; } = string.Empty;
        public string ObservationStatus { get; set; } = string.Empty;
        public string ObservationSummary { get; set; } = string.Empty;
        }

    public class AuditTaskAssignedNotificationData : NotificationRecipientData
        {
        public int? PlanId { get; set; }
        public string EngagementLabel { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string AuditPeriod { get; set; } = string.Empty;
        public string ReportingOffice { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public DateTime? AuditStartDate { get; set; }
        public DateTime? AuditEndDate { get; set; }
        public DateTime? OperationalStartDate { get; set; }
        public DateTime? OperationalEndDate { get; set; }
        public List<string> TeamMembers { get; set; } = new List<string>();
        }

    public class InquiryAssignedNotificationData : NotificationRecipientData
        {
        public int ComplaintId { get; set; }
        public string InquiryReference { get; set; } = string.Empty;
        public string InquiryNature { get; set; } = string.Empty;
        public string AssignedUnitName { get; set; } = string.Empty;
        public string Directions { get; set; } = string.Empty;
        public string AssignedOn { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        }

    public class FinalReportIssuedNotificationData : NotificationRecipientData
        {
        public string EngagementLabel { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string AuditPeriod { get; set; } = string.Empty;
        public string ReportingOffice { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string ReportVersion { get; set; } = string.Empty;
        public DateTime? FinalizedOn { get; set; }
        }
    }
