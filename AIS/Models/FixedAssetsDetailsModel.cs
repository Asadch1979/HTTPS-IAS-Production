using AIS.Validation;
namespace AIS.Models
    {
    public class FixedAssetsDetailsModel
        {
        [PlainText]
        public string FA_ID { get; set; }
        [PlainText]
        public string ASSET_NAME { get; set; }
        [PlainText]
        public string PHYSICAL_EXISTANCE { get; set; }
        [PlainText]
        public string LOCATION_AS_PER_FAR { get; set; }
        [PlainText]
        public string DIFFERENCE { get; set; }
        [PlainText]
        public string REMARKS { get; set; }
        [PlainText]
        public string ENG_ID { get; set; }

        }
    }
