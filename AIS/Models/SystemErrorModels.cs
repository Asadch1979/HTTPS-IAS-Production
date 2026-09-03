using System;
using System.Collections.Generic;

namespace AIS.Models
    {
    public class SystemErrorContext
        {
        public string Module { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ApiPath { get; set; } = string.Empty;
        public string StoredProcedure { get; set; } = string.Empty;
        public string Ppno { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public int? PageId { get; set; }
        public int? EngagementId { get; set; }
        public int? ParaId { get; set; }
        public int? ComId { get; set; }
        public string TraceId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
        public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
        }

    public class SystemErrorRecord
        {
        public long ErrorId { get; set; }
        public string ErrorReference { get; set; } = string.Empty;
        public string Fingerprint { get; set; } = string.Empty;
        public bool IsFirstOccurrence { get; set; }
        public DateTime FirstOccurrenceUtc { get; set; }
        public DateTime LastOccurrenceUtc { get; set; }
        public int OccurrenceCount { get; set; }
        public bool EmailAlreadySent { get; set; }
        }

    public class SystemErrorNotificationRecipients
        {
        public List<string> ToRecipients { get; set; } = new List<string>();
        public List<string> CcRecipients { get; set; } = new List<string>();
        }
    }
