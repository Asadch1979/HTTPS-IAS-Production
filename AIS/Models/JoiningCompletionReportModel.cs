using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class JoiningCompletionReportModel
        {
        [PlainText]
        public string AUDIT_BY { get; set; }
        [PlainText]
        public string Reporting { get; set; }
        [PlainText]
        public string CODE { get; set; }
        [PlainText]
        public string AUDITEE_NAME { get; set; }
        [PlainText]
        public string Risk { get; set; }
        public string Issuancedate { get; set; }
        public DateTime? START_DATE { get; set; }
        public DateTime? END_DATE { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
