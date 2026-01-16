using AIS.Validation;
namespace AIS.Models
    {
    public class SubCheckListStatus
        {
        public int S_ID { get; set; }
        public int CD_ID { get; set; }
        public int? T_ID { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string RISK_SEQUENCE { get; set; }
        [PlainText]
        public string RISK_WEIGHTAGE { get; set; }
        [PlainText]
        public string PROCESS { get; set; }
        [PlainText]
        public string COMMENTS { get; set; }
        public int OBS_ID { get; set; }
        [PlainText]
        public string Status { get; set; }
        }
    }
