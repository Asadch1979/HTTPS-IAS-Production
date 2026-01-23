using System;
using System.Collections.Generic;

namespace AIS.Models
    {
    public class ObservationPdfDataModel
        {
        public string MemoNumber { get; set; }
        public DateTime? MemoDate { get; set; }
        public string Annexure { get; set; }
        public string Title { get; set; }
        public string Risk { get; set; }
        public string ParaText { get; set; }
        public List<ObservationPdfResponsibilityModel> Responsibilities { get; set; } = new List<ObservationPdfResponsibilityModel>();
        }

    public class ObservationPdfResponsibilityModel
        {
        public string PpNo { get; set; }
        public string LoanCase { get; set; }
        public string LcAmount { get; set; }
        }
    }
