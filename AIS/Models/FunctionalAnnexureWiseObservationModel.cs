using AIS.Validation;
namespace AIS.Models
    {
    public class FunctionalAnnexureWiseObservationModel
        {
        public int ID { get; set; }
        [PlainText]
        public string D_ID { get; set; }
        public int COM_ID { get; set; }
        [PlainText]
        public string NAME { get; set; }
        [PlainText]
        public string P_NAME { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_CATEGORY { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string PARA_REF { get; set; }
        [PlainText]
        public string OBS_ID { get; set; }
        [RichTextSanitize]
        public string PARA_GIST { get; set; }
        [PlainText]
        public string P_RISK { get; set; }
        }
    }
