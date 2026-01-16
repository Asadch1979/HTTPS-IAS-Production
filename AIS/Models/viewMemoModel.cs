using System;

using AIS.Validation;
namespace AIS.Models
{
    public class viewMemoModel
    {
        public int COM_ID { get; set; }
        [PlainText]
        public string NEW_PARA_ID { get; set; }
        [PlainText]
        public string OLD_PARA_ID { get; set; }
        [PlainText]
        public string AUDIT_PERIOD { get; set; }
        [PlainText]
        public string PARA_NO { get; set; }
        [PlainText]
        public string ANNEX { get; set; }
        [PlainText]
        public string ANNEX_ID { get; set; }
        [RichTextSanitize]
        public string OBS_GIST { get; set; }
        [PlainText]
        public string OBS_RISK { get; set; }
        [PlainText]
        public string OBS_RISK_ID { get; set; }
        [RichTextSanitize]
        public string PARA_TEXT { get; set; }
        [PlainText]
        public string AMOUNT_INV { get; set; }
        [PlainText]
        public string NO_INSTANCES { get; set; }
        [PlainText]
        public string INDICATOR { get; set; }
    }
}
