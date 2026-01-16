using AIS.Validation;
namespace AIS.Models
    {
    public class AnnexureModel
        {
        public int ID { get; set; }
        [PlainText]
        public string ANNEX { get; set; }
        [PlainText]
        public string CODE { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        [PlainText]
        public string VOL { get; set; }
        [PlainText]
        public string RISK_ID { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string PROCESS_ID { get; set; }
        [PlainText]
        public string FUNCTION_OWNER_ID { get; set; }
        [PlainText]
        public string RISK_MODEL_ID { get; set; }
        [PlainText]
        public string PROCESS { get; set; }
        [PlainText]
        public string FUNCTION_OWNER { get; set; }
        [PlainText]
        public string RISK_MODEL { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        [PlainText]
        public string MAX_NUMBER { get; set; }
        [PlainText]
        public string WEIGHTAGE { get; set; }
        [PlainText]
        public string GRAVITY { get; set; }

        [PlainText]
        public string FUNCTION_ID_1 { get; set; }
        [PlainText]
        public string FUNCTION_1 { get; set; }
        [PlainText]
        public string FUNCTION_ID_2 { get; set; }
        [PlainText]
        public string FUNCTION_2 { get; set; }


        }
    }
