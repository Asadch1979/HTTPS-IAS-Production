using AIS.Validation;
namespace AIS.Models
    {
    public class AuditeeAddressModel
        {
        public int ENG_ID { get; set; }
        public int CODE { get; set; }
        [PlainText]
        public string P_NAME { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string ADDRESS { get; set; }
        [PlainText]
        public string DATE_OF_OPENING { get; set; }
        [PlainText]
        public string LICENSE { get; set; }
        [PlainText]
        public string AUDIT_STARTDATE { get; set; }
        [PlainText]
        public string AUDIT_ENDDATE { get; set; }
        [PlainText]
        public string OPERATION_STARTDATE { get; set; }
        [PlainText]
        public string OPERATION_ENDDATE { get; set; }


        [PlainText]
        public string HIGH { get; set; }
        [PlainText]
        public string MEDIUM { get; set; }
        [PlainText]
        public string LOW { get; set; }


        [PlainText]
        public string SETTLED_HIGH { get; set; }
        [PlainText]
        public string SETTLED_MEDIUM { get; set; }
        [PlainText]
        public string SETTLED_LOW { get; set; }


        [PlainText]
        public string OPEN_HIGH { get; set; }
        [PlainText]
        public string OPEN_MEDIUM { get; set; }
        [PlainText]
        public string OPEN_LOW { get; set; }


        }
    }
