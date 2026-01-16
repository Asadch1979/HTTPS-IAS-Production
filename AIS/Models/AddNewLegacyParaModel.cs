using AIS.Validation;
namespace AIS.Models
    {
    public class AddNewLegacyParaModel
        {
        public int ENTITY_TYPE_ID { get; set; }
        public int ENTITY_ID { get; set; }
        [PlainText]
        public string E_CODE { get; set; }
        [PlainText]
        public string E_NAME { get; set; }
        public int NATURE_ID { get; set; }
        [PlainText]
        public string NATURE { get; set; }
        [PlainText]
        public string AUDIT_YEAR { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string NO_OF_INSTANCES { get; set; }
        [RichTextSanitize]
        public string GIST_OF_PARA { get; set; }
        [PlainText]
        public string OLD_GIST_OF_PARA { get; set; }
        [PlainText]
        public string AMOUNT { get; set; }
        [PlainText]
        public string ANNEXURE { get; set; }
        [PlainText]
        public string VOL_I_II { get; set; }
        [PlainText]
        public string PARA_REF { get; set; }


        }
    }

