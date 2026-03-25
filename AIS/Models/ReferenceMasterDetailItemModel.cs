using System;

using AIS.Validation;

namespace AIS.Models
    {
    public class ReferenceMasterDetailItemModel
        {
        public long RefId { get; set; }
        [PlainText]
        public string ReferenceSourceType { get; set; }
        public long? SourcePkId { get; set; }
        public long? ManualId { get; set; }
        [PlainText]
        public string ReferenceType { get; set; }
        [PlainText]
        public string Division { get; set; }
        public DateTime? InstructionDate { get; set; }
        [PlainText]
        public string SectionText { get; set; }
        [PlainText]
        public string ChapterNo { get; set; }
        [PlainText]
        public string SubSectionNo { get; set; }
        [PlainText]
        public string TitleOrHeading { get; set; }
        [PlainText]
        public string DisplayText { get; set; }
        [PlainText]
        public string ManualName { get; set; }
        }
    }
