using System;

using AIS.Validation;
namespace AIS.Models.SM
    {
    public class ExceptionMonitorEntityModel
        {
        public int EngId { get; set; }
        public int EntId { get; set; }
        [PlainText]
        public string EntName { get; set; }
        public int TotalEng { get; set; }
        public int EngWithExc { get; set; }
        [PlainText]
        public string LastRunDateDisp { get; set; }
        }

    public class ExceptionMonitorDetailModel
        {
        public int EngId { get; set; }
        public int PNo { get; set; }
        public int EntId { get; set; }
        [PlainText]
        public string EntName { get; set; }
        public int ErId { get; set; }
        [PlainText]
        public string ReportTitle { get; set; }
        public int ExcCount { get; set; }
        [PlainText]
        public string LastExceptionDateDisp { get; set; }
        [PlainText]
        public string LastRunDateDisp { get; set; }
        [PlainText]
        public string LastStatus { get; set; }
        [PlainText]
        public string LastErrorMessage { get; set; }
        }

    public class ExceptionMonitorModel
        {
        public int ErId { get; set; }
        public int EngId { get; set; }
        public int EntId { get; set; }
        [PlainText]
        public string ExecutionDates { get; set; }
        [PlainText]
        public string ReportingPeriod { get; set; }
        [PlainText]
        public string ReportTitle { get; set; }
        [PlainText]
        public string ReportType { get; set; }
        public int ExceptionCount { get; set; }
        }
    }
