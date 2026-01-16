using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class CAUOMAssignmentPDPModel
        {
        public int ID { get; set; }
        [PlainText]
        public string OM_NO { get; set; }
        public int PARA_ID { get; set; }
        public DateTime DAC_DATES { get; set; }
        [PlainText]
        public string REPORT_FREQUENCY { get; set; }
        [PlainText]
        public string CONTENTS_OF_OM { get; set; }
        public int STATUS { get; set; }
        [PlainText]
        public string STATUS_DES { get; set; }
        [PlainText]
        public string KEY { get; set; }
        }
    }
