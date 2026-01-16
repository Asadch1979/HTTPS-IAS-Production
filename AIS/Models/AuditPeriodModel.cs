using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class AuditPeriodModel
        {
        public int AUDITPERIODID { get; set; }
        [PlainText]
        public string DESCRIPTION { get; set; }
        public DateTime START_DATE { get; set; }
        public DateTime END_DATE { get; set; }
        public int STATUS_ID { get; set; }
        [PlainText]
        public string STATUS { get; set; }
        [PlainText]
        public string REMARKS_OUT { get; set; }
        [PlainText]
        public string IS_SUCCESS { get; set; }

        }
    }
