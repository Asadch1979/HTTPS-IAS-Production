using AIS.Validation;
namespace AIS.Models
    {
    public class ChecklistDetailComparisonModel
        {


        public int ID { get; set; }
        [PlainText]
        public string PROCESS { get; set; }
        [PlainText]
        public string PROCESS_ID { get; set; }
        [PlainText]
        public string SUB_PROCESS { get; set; }
        [PlainText]
        public string PROCESS_DETAIL { get; set; }

        [PlainText]
        public string VIOLATION { get; set; }
        [PlainText]
        public string FUNCTIONAL_RESP { get; set; }
        [PlainText]
        public string ROLE_RESP { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string ANNEXURE { get; set; }

        [PlainText]
        public string NEW_SUB_PROCESS { get; set; }
        [PlainText]
        public string NEW_PROCESS_DETAIL { get; set; }

        [PlainText]
        public string NEW_VIOLATION { get; set; }
        [PlainText]
        public string NEW_FUNCTIONAL_RESP { get; set; }
        [PlainText]
        public string NEW_ROLE_RESP { get; set; }
        [PlainText]
        public string NEW_RISK { get; set; }

        [PlainText]
        public string NEW_ANNEXURE { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        [PlainText]
        public string N_D_ID { get; set; }
        [PlainText]
        public string N_S_ID { get; set; }
        [PlainText]
        public string N_V_ID { get; set; }
        [PlainText]
        public string N_RISK_ID { get; set; }
        [PlainText]
        public string N_ROLE_RESP_ID { get; set; }
        [PlainText]
        public string N_OWNER_ID { get; set; }
        [PlainText]
        public string N_ANNEX_ID { get; set; }

        }
    }
