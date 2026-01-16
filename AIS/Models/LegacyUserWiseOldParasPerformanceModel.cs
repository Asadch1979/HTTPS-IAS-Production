using AIS.Validation;
namespace AIS.Models
    {
    public class LegacyUserWiseOldParasPerformanceModel
        {
        [PlainText]
        public string AUDIT_ZONEID { get; set; }
        [PlainText]
        public string ZONENAME { get; set; }
        [PlainText]
        public string DATE { get; set; }
        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string EMP_NAME { get; set; }
        [PlainText]
        public string PARA_ENTERED { get; set; }
        }
    }
