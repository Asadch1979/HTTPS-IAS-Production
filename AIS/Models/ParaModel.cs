using AIS.Validation;
namespace AIS.Models
    {
    public class ParaModel
        {
        public int ComId { get; set; }
        [PlainText]
        public string AuditYear { get; set; }
        [PlainText]
        public string ParaNo { get; set; }
        [PlainText]
        public string Annexure { get; set; }
        [PlainText]
        public string Title { get; set; }
        [PlainText]
        public string Risk { get; set; }
        public int Status { get; set; }
        }
    }
