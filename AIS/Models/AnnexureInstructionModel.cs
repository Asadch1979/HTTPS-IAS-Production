using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class AnnexureInstructionModel
        {
        [PlainText]
        public string InstructionsTitle { get; set; }
        [PlainText]
        public string annexureId { get; set; }
        [PlainText]
        public string referenceTypeId { get; set; }
        [PlainText]
        public string referenceType { get; set; }
        public DateTime? InstructionsDate { get; set; }
        [PlainText]
        public string InstructionsDetails { get; set; }
        public int AnnexureRefId { get; set; }
        [PlainText]
        public string IND { get; set; }
        [PlainText]
        public string Status { get; set; }
        }
    }
