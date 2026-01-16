using System.Collections.Generic;

using AIS.Validation;
namespace AIS.Models
    {
    public class ObservationTextModel
        {
        public int OBS_ID { get; set; }
        public int ENG_ID { get; set; }
        [PlainText]
        public string CP_ID { get; set; }
        [PlainText]
        public string CP { get; set; }
        [PlainText]
        public string PSN { get; set; }
        [PlainText]
        public string PSN_ID { get; set; }
        [PlainText]
        public string CD_ID { get; set; }
        [PlainText]
        public string CD { get; set; }
        [PlainText]
        public string INSTANCES { get; set; }
        [PlainText]
        public string AMOUNT { get; set; }
        [RichTextSanitize]
        public string TEXT { get; set; }
        [PlainText]
        public string TITLE { get; set; }
        [PlainText]
        public string RISK_ID { get; set; }
        [PlainText]
        public string RISK { get; set; }
        [PlainText]
        public string IND { get; set; }
        [RichTextSanitize]
        public string OBS_REPLY { get; set; }
        public List<ObservationResponsiblePPNOModel> RESPONSIBLE_PPs { get; set; }

        }
    }
