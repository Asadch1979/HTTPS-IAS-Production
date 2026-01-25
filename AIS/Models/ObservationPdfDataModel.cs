using System;
using System.Collections.Generic;

namespace AIS.Models
    {
    public class ObservationPdfDataModel
        {
        public string EntityName { get; set; }
        public string AuditPeriod { get; set; }
        public string MemoNumber { get; set; }
        public DateTime? MemoDate { get; set; }
        public string Annexure { get; set; }
        public string Title { get; set; }
        public string Risk { get; set; }
        public string ParaText { get; set; }
        public string TeamLead { get; set; }
        public List<ObservationPdfResponsibilityModel> Responsibilities { get; set; } = new List<ObservationPdfResponsibilityModel>();
        }

    public class ObservationPdfResponsibilityModel
        {
        public string PpNo { get; set; }
        public string EmployeeName { get; set; }
        public string LoanCase { get; set; }
        public string LcAmount { get; set; }
        public string AccountNumber { get; set; }
        public string AcAmount { get; set; }
        }
    }
