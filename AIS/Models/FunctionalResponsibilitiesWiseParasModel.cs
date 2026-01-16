using AIS.Validation;
namespace AIS.Models
    {
    public class FunctionalResponsibilitiesWiseParasModel
        {

        [PlainText]
        public string PARA_REF { get; set; }
        [PlainText]
        public string OBS_ID { get; set; }
        [PlainText]
        public string PARA_CATEGORY { get; set; }
        [PlainText]
        public string REP_OFFICE { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string ANNEXURE { get; set; }
        [PlainText]
        public string CHECK_LIST { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [RichTextSanitize]
        public string GIST { get; set; }




        }
    }
