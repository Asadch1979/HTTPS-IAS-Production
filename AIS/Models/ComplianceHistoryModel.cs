using AIS.Validation;
namespace AIS.Models
    {
    public class ComplianceHistoryModel
        {
        public int ID { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        [PlainText]
        public string OBS_ID { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string ATTENDED_ON { get; set; }
        [PlainText]
        public string ATTENDED_BY { get; set; }
        [PlainText]
        public string ROLE_ID { get; set; }
        [PlainText]
        public string STAGE { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string DESIGNATION { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string C_STATUS { get; set; }
        [PlainText]
        public string SEQ { get; set; }
        [PlainText]
        public string COM_SEQ_NO { get; set; }
        [PlainText]
        public string PARA_CATEGORY { get; set; }
        }
    }
