using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class JoiningCompletionReportModel
        {
        [PlainText]
        public string AUDIT_BY { get; set; }
        [PlainText]
        public string AUDITEE_NAME { get; set; }
        [PlainText]
        public string TEAM_NAME { get; set; }
        public int PPNO { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string TEAM_LEAD { get; set; }
        public DateTime? JOINING_DATE { get; set; }
        public DateTime? COMPLETION_DATE { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
