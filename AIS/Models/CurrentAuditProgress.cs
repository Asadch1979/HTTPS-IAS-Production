using AIS.Validation;
namespace AIS.Models
    {
    public class CurrentAuditProgress
        {
        [PlainText]
        public string CODE { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string AREA { get; set; }
        public int OBS_COUNT { get; set; }
        }
    }
