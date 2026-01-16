using AIS.Validation;
namespace AIS.Models
    {
    public class FadOldParaReportModel
        {

        public int PERIOD { get; set; }
        [PlainText]
        public string ENTITY_NAME { get; set; }

        [PlainText]
        public string PROCESS { get; set; }
        [PlainText]
        public string SUB_PROCESS { get; set; }

        [PlainText]
        public string VIOLATION { get; set; }
        [RichTextSanitize]
        public string OBS_TEXT { get; set; }
        [PlainText]
        public string OBS_RISK { get; set; }
        [PlainText]
        public string OBS_STATUS { get; set; }


        }
    }
