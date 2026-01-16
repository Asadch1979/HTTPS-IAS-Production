using AIS.Validation;
namespace AIS.Models
    {
    public class ObservationSummaryModel
        {

        public int? ENG_ID { get; set; }
        [PlainText]
        public string PPNO { get; set; }
        [PlainText]
        public string E_NAME { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string TEAM { get; set; }
        [PlainText]
        public string CREATED { get; set; }
        [PlainText]
        public string SUBMIT_TO_AUDITEE { get; set; }
        [PlainText]
        public string RESPONDED_BY_AUDITEE { get; set; }
        [PlainText]
        public string DROP_RESOLVED_BY_TEAM_HEAD { get; set; }
        [PlainText]
        public string ADDED_TO_DRAFT { get; set; }
        [PlainText]
        public string ADDED_TO_FINAL { get; set; }
        [PlainText]
        public string SETTELED { get; set; }
        [PlainText]
        public string TOTAL { get; set; }

        }
    }
