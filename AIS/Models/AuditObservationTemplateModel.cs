using AIS.Validation;
namespace AIS.Models
    {
    public class AuditObservationTemplateModel
        {
        public int ACTIVITY_ID { get; set; }
        public int TEMP_ID { get; set; }
        [PlainText]
        public string OBS_TEMPLATE { get; set; }
        }
    }
