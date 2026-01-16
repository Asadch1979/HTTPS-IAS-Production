using AIS.Validation;
namespace AIS.Models
    {
    public class InspectionUnitsModel
        {
        public int I_ID { get; set; }
        [PlainText]
        public string I_CODE { get; set; }
        [PlainText]
        public string UNIT_NAME { get; set; }
        [PlainText]
        public string DISCRIPTION { get; set; }
        [PlainText]
        public string STATUS { get; set; }

        }
    }
