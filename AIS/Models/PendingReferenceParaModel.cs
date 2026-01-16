using AIS.Validation;
namespace AIS.Models
    {
    public class PendingReferenceParaModel
        {
        public int ComId { get; set; }
        [PlainText]
        public string AuditPeriod { get; set; }
        [PlainText]
        public string ParaNo { get; set; }
        [PlainText]
        public string GistOfParas { get; set; }
        }
    }
