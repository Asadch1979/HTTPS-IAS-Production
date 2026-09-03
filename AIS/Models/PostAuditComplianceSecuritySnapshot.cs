using AIS.Validation;

namespace AIS.Models
    {
    public class PostAuditComplianceSecuritySnapshot
        {
        public int COM_ID { get; set; }
        public int? ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        public int? NEW_PARA_ID { get; set; }
        public int? OLD_PARA_ID { get; set; }
        public int? COM_STAGE { get; set; }
        public int? COM_STATUS { get; set; }
        public int? COM_CYCLE { get; set; }
        public int? NEXT_R_ID { get; set; }
        public int? PER_R_ID { get; set; }
        [PlainText]
        public string IND { get; set; }
        [PlainText]
        public string REC_FROM { get; set; }
        }
    }
