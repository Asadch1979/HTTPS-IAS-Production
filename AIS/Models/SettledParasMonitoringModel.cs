using AIS.Validation;
namespace AIS.Models
    {
    public class SettledParasMonitoringModel
        {
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string SETTLED_BY { get; set; }
        [PlainText]
        public string SETTLED_ON { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string PARA_CATEGORY { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        
        public string AU_OBS_ID { get; set; }
        public string COM_ID { get; set; }
        [PlainText]
        public string COMPLIANCE_CYCLE { get; set; }
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string AUDITED_BY { get; set; }




        }
    }
