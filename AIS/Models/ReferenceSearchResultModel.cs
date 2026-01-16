using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class ReferenceSearchResultModel
        {
        public int ID { get; set; }
        [PlainText]
        public string Title { get; set; }
        [RichTextSanitize]
        public string INSTRUCTIONSDETAILS { get; set; }
        public DateTime? InstructionsDate { get; set; }
        [PlainText]
        public string KEYWORDS { get; set; }
        [PlainText]
        public string REFERENCEURL { get; set; }
        [PlainText]
        public string ReferenceType { get; set; }

        }
    }
