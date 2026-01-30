using System;

namespace AIS.Models
    {
    public class FrptEngagementListRow
        {
        public int EngId { get; set; }
        public string ReportingOffice { get; set; } = "";
        public string EntityName { get; set; } = "";
        public DateTime? AuditStartDate { get; set; }
        public DateTime? AuditEndDate { get; set; }
        public string ReportStatus { get; set; } = "";
        }
    }
