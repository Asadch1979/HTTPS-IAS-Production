using AIS.Validation;
namespace AIS.Models
    {
    public class AuditParaReconsillation
        {
        [PlainText]
        public string AUDIT_ZONE { get; set; }
        [PlainText]
        public string ENTITY_TYPE_DESC { get; set; }
        [PlainText]
        public string REPORTING_OFFICE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string OPEN_BALANCE { get; set; }
        [PlainText]
        public string ADDED { get; set; }
        [PlainText]
        public string TOTAL { get; set; }
        [PlainText]
        public string SETTLED_LEGACY { get; set; }
        [PlainText]
        public string SETTLED_NEW_PARA { get; set; }
        [PlainText]
        public string UN_SETTLED { get; set; }
        [PlainText]
        public string INDICATOR { get; set; }
        [PlainText]
        public string R1 { get; set; }
        [PlainText]
        public string R2 { get; set; }
        [PlainText]
        public string R3 { get; set; }
        [PlainText]
        public string PERCENTAGE { get; set; }


        }
    }
