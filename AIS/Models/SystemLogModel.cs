using AIS.Validation;
using System;

namespace AIS.Models
    {
    public class SystemLogModel
        {
        public int LOG_ID { get; set; }
        [PlainText]
        public string LOG_LEVEL { get; set; }
        public DateTime LOG_TIME { get; set; }
        [PlainText]
        public string MODULE { get; set; }
        [PlainText]
        public string CONTROLLER { get; set; }
        [PlainText]
        public string ACTION { get; set; }
        [PlainText]
        public string MESSAGE { get; set; }
        [PlainText]
        public string TECH_DETAILS { get; set; }
        public int? PAGE_ID { get; set; }
        public int? ENG_ID { get; set; }
        [PlainText]
        public string USER_PPNO { get; set; }
        }
    }
