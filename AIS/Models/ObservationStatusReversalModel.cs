using AIS.Validation;
namespace AIS.Models
    {
    public class ObservationStatusReversalModel
        {
        public int STATUS_ID { get; set; }
        [PlainText]
        public string STATUS_NAME { get; set; }

        }
    }
