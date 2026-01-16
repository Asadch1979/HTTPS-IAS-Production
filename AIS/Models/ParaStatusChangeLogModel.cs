using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class ParaStatusChangeLogModel
        {
        public int LogId { get; set; }
        public int ComId { get; set; }
        public int OldStatus { get; set; }
        public int NewStatus { get; set; }
        [PlainText]
        public string MakerRemarks { get; set; }
        [PlainText]
        public string AuthorizerRemarks { get; set; }
        public int ChangedBy { get; set; }
        public DateTime ChangedOn { get; set; }
        public int? AuthorizedBy { get; set; }
        public DateTime? AuthorizedOn { get; set; }
        [PlainText]
        public string ActionStatus { get; set; }

        // From compliance table
        [PlainText]
        public string AuditYear { get; set; }
        [PlainText]
        public string ParaNo { get; set; }
        [PlainText]
        public string Annexure { get; set; }
        [PlainText]
        public string Title { get; set; }
        [PlainText]
        public string Risk { get; set; }

        // For cursor message output
        [PlainText]
        public string ResultMsg { get; set; }
        }
    }
