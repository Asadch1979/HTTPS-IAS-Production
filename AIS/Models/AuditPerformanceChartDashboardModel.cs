using AIS.Validation;
namespace AIS.Models
    {
    public class AuditPerformanceChartDashboardModel
        {
        [PlainText]
        public string DEPARTMENT { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string NO_OF_ENTITIES { get; set; }
        [PlainText]
        public string TOTAL_ENTITIES { get; set; }
        [PlainText]
        public string PERCENTAGE { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        }
    }
