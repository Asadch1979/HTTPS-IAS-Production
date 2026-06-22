using AIS.Validation;

namespace AIS.Models
    {
    public class HeadObservationRiskSummaryModel
        {
        public int DepartmentId { get; set; }

        [PlainText]
        public string DepartmentName { get; set; }

        public int TotalObservations { get; set; }
        public int HighRisk { get; set; }
        public int MediumRisk { get; set; }
        public int LowRisk { get; set; }
        public int UnratedRisk { get; set; }

        [PlainText]
        public string RiskStatus { get; set; }
        }
    }
