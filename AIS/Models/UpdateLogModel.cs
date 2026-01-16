using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class UpdateLogModel
        {
        public DateTime Date { get; set; }
        [PlainText]
        public string User { get; set; }
        [PlainText]
        public string Field { get; set; }
        [PlainText]
        public string OldValue { get; set; }
        [PlainText]
        public string NewValue { get; set; }
        [PlainText]
        public string ActionType { get; set; }
        }
    }
