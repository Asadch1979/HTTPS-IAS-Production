using AIS.Validation;
namespace AIS.Models
    {
    public class ObservationRevisedModel
        {
        public int OBS_ID { get; set; }
        [PlainText]
        public string IND { get; set; }
        [PlainText]
        public string E_NAME { get; set; }
        public int? MEMO { get; set; }
        public int? DRAFT_PARA { get; set; }
        public int? FINAL_PARA { get; set; }
        [PlainText]
        public string TITLE { get; set; }
        [PlainText]
        public string T_IND { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        public int? STATUS_ID { get; set; }


        }
    }
