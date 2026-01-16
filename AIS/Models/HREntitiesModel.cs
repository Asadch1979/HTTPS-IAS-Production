using AIS.Validation;
namespace AIS.Models
    {
    public class HREntitiesModel
        {

        [PlainText]
        public string REPORTING_CODE { get; set; }
        [PlainText]
        public string REPORTING_NAME { get; set; }
        [PlainText]
        public string REPORTING_STATUS { get; set; }
        [PlainText]
        public string REPORTING_INDICATOR { get; set; }
        [PlainText]
        public string ENTITY_CODE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ENTITY_STATUS { get; set; }
        [PlainText]
        public string ENTITY_INDICATOR { get; set; }


        }
    }
