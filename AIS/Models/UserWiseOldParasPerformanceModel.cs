using AIS.Validation;
namespace AIS.Models
    {
    public class UserWiseOldParasPerformanceModel
        {
        [PlainText]
        public string AUDIT_ZONEID { get; set; }
        [PlainText]
        public string ZONENAME { get; set; }
        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string PARA_ENTERED { get; set; }
        }
    }
