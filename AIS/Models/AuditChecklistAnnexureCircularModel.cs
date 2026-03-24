using System;

using AIS.Validation;
namespace AIS.Models
    {
    public class AuditChecklistAnnexureCircularModel
        {
        /// <summary>
        /// Identifier of <c>TBL_PARA_REFERENCE_LINKS</c> row when the circular
        /// is associated with a para. Null for unlinked references.
        /// </summary>
        public int? LinkId { get; set; }
        public int ID { get; set; }
        public int DivisionEntId { get; set; }
        public int ReferenceTypeId { get; set; }
        [PlainText]
        public string ReferenceType { get; set; }
        [PlainText]
        public string InstructionsDetails { get; set; }
        [PlainText]
        public string Keywords { get; set; }
        [PlainText]
        public string RedirectedPage { get; set; }
        [PlainText]
        public string Division { get; set; }
        [PlainText]
        public string InstructionsTitle { get; set; }
        public DateTime? InstructionsDate { get; set; }
        public int? ManualId { get; set; }
        public int? ManualIndexId { get; set; }
        [PlainText]
        public string SectionName { get; set; }
        [PlainText]
        public string ChapterNo { get; set; }
        [PlainText]
        public string SubSectionNo { get; set; }
        [PlainText]
        public string Heading { get; set; }
        [PlainText]
        public string DocType { get; set; }
        }

    }
