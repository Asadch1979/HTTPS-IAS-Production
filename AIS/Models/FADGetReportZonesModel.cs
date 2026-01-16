using AIS.Validation;
namespace AIS.Models
    {
    public class FADGetReportZonesModel
        {
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        }
    }
