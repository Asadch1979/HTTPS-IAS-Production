using AIS.Validation;
namespace AIS.Models
    {
    public class FADAuditEmpModel
        {
        public int ID { get; set; }
        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string RANK { get; set; }
        [PlainText]
        public string DESIGNATION { get; set; }
        [PlainText]
        public string PLACEMENT { get; set; }
        [PlainText]
        public string QUALIFICATION { get; set; }
        [PlainText]
        public string SPECIALIZATION { get; set; }
        [PlainText]
        public string CERTIFICATION { get; set; }
        [PlainText]
        public string TOTAL_EXPERIENCE { get; set; }
        [PlainText]
        public string AUDIT_EXPERIENCE { get; set; }
        }
    }
