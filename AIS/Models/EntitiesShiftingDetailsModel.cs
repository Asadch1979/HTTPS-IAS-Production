using AIS.Validation;
namespace AIS.Models
    {
    public class EntitiesShiftingDetailsModel
        {

        public int ENTITY_ID { get; set; }
        public int ENTITY_CODE { get; set; }
        public int TYPE_ID { get; set; }
        [PlainText]
        public string AUDIT_TYPE { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string E_SIZE { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string ENG_ID { get; set; }
        [PlainText]
        public string START_DATE { get; set; }
        [PlainText]
        public string END_DATE { get; set; }
        [PlainText]
        public string TOTAL_PARA { get; set; }
        [PlainText]
        public string LEGACY_PARA { get; set; }
        [PlainText]
        public string LEGACY_OPEN { get; set; }
        [PlainText]
        public string LEGACY_CLOSE { get; set; }
        [PlainText]
        public string AIS_PARA { get; set; }
        [PlainText]
        public string AIS_OPEN { get; set; }
        [PlainText]
        public string AIS_CLOSE { get; set; }
        [PlainText]
        public string COMP_SUB { get; set; }


        }
    }
