using AIS.Validation;
namespace AIS.Models
    {
    public class AuditChecklistDetailsModel
        {
        public int ID { get; set; }
        public int S_ID { get; set; }
        public int P_ID { get; set; }
        [PlainText]
        public string S_NAME { get; set; }
        public int V_ID { get; set; }
        [PlainText]
        public string V_NAME { get; set; }
        [PlainText]
        public string COMMENTS { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        public int RISK_ID { get; set; }
        public int ANNEX_ID { get; set; }
        [PlainText]
        public string RISK { get; set; }
        public int ROLE_RESP_ID { get; set; }
        [PlainText]
        public string ROLE_RESP { get; set; }
        public int PROCESS_OWNER_ID { get; set; }
        [PlainText]
        public string PROCESS_OWNER { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string OB_STATUS { get; set; }
        }
    }
