using AIS.Validation;
namespace AIS.Models
    {
    public class AllParaForAnnexureAssignmentModel
        {
        [PlainText]
        public string ENTITY_ID { get; set; }
        [PlainText]
        public string ANNEX_ID { get; set; }
        [PlainText]
        public string ANNEX_CODE { get; set; }
        [PlainText]
        public string ANNEXURE { get; set; }
        [PlainText]
        public string OBS_ID { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        [PlainText]
        public string PARA_CATEGORY { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARAS { get; set; }

        }
    }
