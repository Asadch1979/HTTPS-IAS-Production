using System.ComponentModel.DataAnnotations.Schema;

using AIS.Validation;
namespace AIS.Models
    {
    public class RiskProcessTransactions
        {
        public int ID { get; set; }
        public int V_ID { get; set; }
        public int PD_ID { get; set; }
        public int DIV_ID { get; set; }
        [PlainText]
        public string DIV_NAME { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        [PlainText]
        public string CONTROL_OWNER { get; set; }
        [PlainText]
        public string ACTION { get; set; }
        public int RISK_WEIGHTAGE { get; set; }
        [PlainText]
        public string RISK { get; set; }
        public int RISK_MAX_NUMBER { get; set; }
        [NotMapped]
        [PlainText]
        public string SUB_PROCESS_NAME { get; set; }
        [NotMapped]
        [PlainText]
        public string PROCESS_NAME { get; set; }
        [NotMapped]
        [PlainText]
        public string PROCESS_STATUS { get; set; }
        [NotMapped]
        [PlainText]
        public string PROCESS_COMMENTS { get; set; }
        [NotMapped]
        [PlainText]
        public string VIOLATION_NAME { get; set; }

        }
    }
