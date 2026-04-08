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
        public string Risk { get; set; }
        public string Issuancedate { get; set; }
        public int High { get; set; }   
        public int Medium { get; set; } 
        public int Low { get; set; }
        public DateTime? JOINING_DATE { get; set; }
        public DateTime? COMPLETION_DATE { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
