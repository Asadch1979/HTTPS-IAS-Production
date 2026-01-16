using System.Collections.Generic;

using AIS.Validation;
namespace AIS.Models
    {
    public class AddLegacyParaModel
        {
        public int ID { get; set; }
        [PlainText]
        public string REF_P { get; set; }
        public List<ObservationResponsiblePPNOModel> RESP_PP { get; set; }
        public int PROCESS_ID { get; set; }
        public int SUB_PROCESS_ID { get; set; }
        public int RISK_ID { get; set; }
        public int CHECKLIST_DETAIL_ID { get; set; }
        [RichTextSanitize]
        public string PARA_TEXT { get; set; }




        }
    }
