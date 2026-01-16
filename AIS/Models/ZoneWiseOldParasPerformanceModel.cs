using AIS.Validation;
namespace AIS.Models
    {
    public class ZoneWiseOldParasPerformanceModel
        {
        [PlainText]
        public string ZONEID { get; set; }
        [PlainText]
        public string ZONENAME { get; set; }
        [PlainText]
        public string PARA_ENTERED { get; set; }
        [PlainText]
        public string PARA_PENDING { get; set; }
        [PlainText]
        public string PARA_TOTAL { get; set; }
        }
    }
