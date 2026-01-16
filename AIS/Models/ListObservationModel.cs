using System.Collections.Generic;

using AIS.Validation;
namespace AIS.Models
    {
    public class ListObservationModel
        {
        [PlainText]
        public string ID { get; set; }
        [RichTextSanitize]
        public string MEMO { get; set; }
        [PlainText]
        public string HEADING { get; set; }
        public int? RISK { get; set; }
        [PlainText]
        public string ANNEXURE_ID { get; set; }
        [RichTextSanitize]
        public string OBSERVATION_TEXT { get; set; }
        public int? DAYS { get; set; }
        [PlainText]
        public string ATTACHMENTS { get; set; }
        [PlainText]
        public string LOANCASE { get; set; }
        [PlainText]
        public string AMOUNT_INVOLVED { get; set; }
        [PlainText]
        public string NO_OF_INSTANCES { get; set; }
        public List<ObservationResponsiblePPNOModel> RESPONSIBLE_PPNO { get; set; }
        }
    }
