using AIS.Validation;
namespace AIS.Models
    {
    public class FADGetReportEntititiesModel
        {
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        }
    }
