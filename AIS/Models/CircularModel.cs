using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class CircularModel
        {
        public int CircularId { get; set; }
        [PlainText]
        public string DisplayText { get; set; }
        [PlainText]
        public string Keywords { get; set; }
        [PlainText]
        public string RedirectedPage { get; set; }
        [PlainText]
        public string Division { get; set; }
        [PlainText]
        public string ReferenceNo { get; set; }
        public DateTime? IssueDate { get; set; }
        [PlainText]
        public string DocType { get; set; }
        public int EntId { get; set; }
        }

    }
