namespace AIS.Models
    {
    public class ManualMasterItemModel
        {
        public long ManualId { get; set; }
        public string ManualName { get; set; }
        public string VolumeName { get; set; }
        public string DisplayLabel { get; set; }
        }

    public class ManualSectionItemModel
        {
        public string SectionName { get; set; }
        public string SectionText { get; set; }
        }

    public class ManualChapterItemModel
        {
        public string ChapterNo { get; set; }
        }

    public class ManualIndexItemModel
        {
        public long IndexId { get; set; }
        public long RefId { get; set; }
        public string ReferenceSourceType { get; set; }
        public long? SourcePkId { get; set; }
        public long? ManualId { get; set; }
        public string SectionText { get; set; }
        public string ChapterNo { get; set; }
        public string SubSectionNo { get; set; }
        public string Heading { get; set; }
        public string DisplayText { get; set; }
        }
    }
