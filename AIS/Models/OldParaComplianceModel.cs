using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class OldParaComplianceModel
        {
        public int ParaRef { get; set; }
        public DateTime? ComplianceDate { get; set; }
        [PlainText]
        public string AuditeeCompliance { get; set; }
        [PlainText]
        public string AuditorRemarks { get; set; }
        [PlainText]
        public string CnIRecommendation { get; set; }

        }
    }
