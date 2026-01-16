using AIS.Validation;
namespace AIS.Models
    {
    public class ObservationNumbersModel
        {
        public int OBS_ID { get; set; }
        [PlainText]
        public string MEMO_NUMBER { get; set; }
        [PlainText]
        public string DRAFT_PARA_NUMBER { get; set; }
        [PlainText]
        public string FINAL_PARA_NUMBER { get; set; }

        }
    }
