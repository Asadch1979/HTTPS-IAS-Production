namespace AIS.Models
    {
    public class UpdateObservationStatusRequest
        {
        public int OBS_ID { get; set; }
        public int NEW_STATUS_ID { get; set; }
        public string DRAFT_PARA_NO { get; set; }
        public int? FinalParaNumber { get; set; }
        public int RISK_ID { get; set; }
        public string AUDITOR_COMMENT { get; set; }
        }
    }
