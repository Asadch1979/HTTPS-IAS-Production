using AIS.Validation;
namespace AIS.Models
    {
    public class ManageObservationModel
        {

        [PlainText]
        public string STATUS_ID { get; set; }
        [PlainText]
        public string STATUS_NAME { get; set; }
        [PlainText]
        public string IS_ACTIVE { get; set; }
        [PlainText]
        public string CODE { get; set; }
        [PlainText]
        public string SATISFIED { get; set; }

        }
    }
