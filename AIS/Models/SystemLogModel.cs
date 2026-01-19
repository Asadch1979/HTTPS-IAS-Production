using AIS.Validation;
using System;

namespace AIS.Models
    {
    public class SystemLogModel
        {
        public int LOGID { get; set; }
        [PlainText]
        public string LOGLEVEL { get; set; }
        public DateTime LOGTIME { get; set; }
        [PlainText]
        public string MODULE { get; set; }
        [PlainText]
        public string CONTROLLER { get; set; }
        [PlainText]
        public string ACTION { get; set; }
        [PlainText]
        public string MESSAGE { get; set; }
        [PlainText]
        public string TECHDETAILS { get; set; }
        public int? PAGEID { get; set; }
        public int? ENGID { get; set; }
        [PlainText]
        public string USERPPNO { get; set; }
        }
    }
