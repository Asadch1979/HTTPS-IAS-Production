using AIS.Validation;
namespace AIS.Models
    {
    public class PendingParaModel
        {
        public int ParaId { get; set; }
        [PlainText]
        public string AuditYear { get; set; }
        [PlainText]
        public string ParaNo { get; set; }
        [PlainText]
        public string Gist { get; set; }
        [PlainText]
        public string Risk { get; set; }
        }
    }
